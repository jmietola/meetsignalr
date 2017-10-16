using System;
using System.Web;
using Microsoft.AspNet.SignalR;
using System.Data.SqlClient;
using System.Configuration;
using System.Device.Location;
using System.Data.SqlTypes;
using System.Threading.Tasks;

namespace SignalRChat
{
    public class ChatHub : Hub
    {
        public string connString = "";

        public ChatHub()
        {
            var conn = ConfigurationManager.ConnectionStrings["AzureConnectionString"];
            connString = conn.ConnectionString;
        }


        public void Send(string name, string message)
        {
            // Call the broadcastMessage method to update clients.
          //  Clients.All.broadcastMessage(name, message);

            int roomID = GetRoomsStatus(Context.ConnectionId);
            SendMessageToRoom(roomID, message);

        }


        public async Task Locations(string name, string latitude, string longitude)
        {
            // Call the broadcastMessage method to update clients.

            using (SqlConnection con = new SqlConnection(connString))
            {

                int userid = 0;
                bool match = false;
                int locationuserid = 0;
                //
                // Open the SqlConnection.
                //
                con.Open();

                //

                string sql = "INSERT INTO users(name,connectionID) VALUES(@param1,@param2)";
                SqlCommand cmd = new SqlCommand(sql, con);
                cmd.Parameters.AddWithValue("@param1", name);
                cmd.Parameters.AddWithValue("@param2", Context.ConnectionId);
                cmd.ExecuteNonQuery();

                using (SqlCommand command = new SqlCommand("SELECT TOP 1 * FROM users ORDER BY userID DESC", con))
                using (SqlDataReader reader = command.ExecuteReader())
                {
                    while (reader.Read())
                    {
                        userid = reader.GetSqlInt32(0).Value;
                    }
                }

                //

                sql = "INSERT INTO rooms(userID, name) VALUES(@param1,@param2)";
                cmd = new SqlCommand(sql, con);
                cmd.Parameters.AddWithValue("@param1", userid);
                cmd.Parameters.AddWithValue("@param2", name + "'s room");
                cmd.ExecuteNonQuery();

                //

                sql = "INSERT INTO locations(userID, latitude,longitude) VALUES(@param1,@param2, @param3)";
                cmd = new SqlCommand(sql, con);
                cmd.Parameters.AddWithValue("@param1", userid);
                cmd.Parameters.AddWithValue("@param2", latitude);
                cmd.Parameters.AddWithValue("@param3", longitude);
                cmd.ExecuteNonQuery();

                var coordinateA = new GeoCoordinate(double.Parse(latitude), double.Parse(longitude));


                string roomqueryresult = $"SELECT * FROM rooms WHERE userID = {userid}";
                var initialroomid = 0;
                using (SqlCommand command = new SqlCommand(roomqueryresult, con))
                using (SqlDataReader reader = command.ExecuteReader())
                {
                    while (reader.Read())
                    {
                        initialroomid = reader.GetSqlInt32(0).Value;
                    }
                }

                using (SqlCommand command = new SqlCommand("SELECT * FROM locations", con))
                using (SqlDataReader reader = command.ExecuteReader())
                {
                    while (reader.Read())
                    {

                        locationuserid = reader.GetSqlInt32(1).Value;
                        double latitude2 = reader.GetSqlDouble(2).Value;
                        double longitude2 = reader.GetSqlDouble(3).Value;

                        var coordinateB = new GeoCoordinate(latitude2, longitude2);

                        var distance = coordinateB.GetDistanceTo(coordinateA);

                        if (distance < 1000000 && distance > 1)
                        {
                            match = true;
                            break;
                        }
                    }

                }

                if (match)
                {
                    // emit client found and add user to room
                    int roomID = 0;

                    string result = $"SELECT * FROM rooms WHERE userID = {locationuserid}";

                    using (SqlCommand command = new SqlCommand(result, con))
                    using (SqlDataReader reader = command.ExecuteReader())
                    {
                        while (reader.Read())
                        {
                            roomID = reader.GetSqlInt32(0).Value;
                        }
                    }

                    // join room and emit message to room
                    await JoinRoom(Convert.ToString(roomID));
                    Clients.Group(Convert.ToString(roomID)).addChatMessage(Context.User.Identity.Name + " joined.");
                    InsertToRoomStatus(roomID, userid);



                }
                else
                {
                    int roomID = 0;
                    string result = $"SELECT * FROM rooms WHERE userID = {locationuserid}";

                    using (SqlCommand command = new SqlCommand(result, con))
                    using (SqlDataReader reader = command.ExecuteReader())
                    {
                        while (reader.Read())
                        {
                            roomID = reader.GetSqlInt32(0).Value;
                        }
                    }

                    // join own room first
                    await JoinRoom(Convert.ToString(initialroomid));
                    InsertToRoomStatus(roomID, userid);
                }


            }
        }

        public Task JoinRoom(string roomName)
        {
            return Groups.Add(Context.ConnectionId, roomName);
        }

        public Task LeaveRoom(string roomName)
        {
            return Groups.Remove(Context.ConnectionId, roomName);
        }

        public Task SendMessageToRoom(int roomID, string message)
        {
            return Clients.Group(Convert.ToString(roomID)).addChatMessage(message);
        }


        public void InsertToRoomStatus(int roomID, int userID) 
        {
            using (SqlConnection con = new SqlConnection(connString))
            {
                //
                // Open the SqlConnection.
                //
                con.Open();

                string sql = "INSERT INTO roomsstatus(roomID, userID) VALUES(@param1,@param2)";
                SqlCommand cmd = new SqlCommand(sql, con);
                cmd.Parameters.AddWithValue("@param1", roomID);
                cmd.Parameters.AddWithValue("@param2", userID);
                cmd.ExecuteNonQuery();
            }
        }

        public int GetRoomsStatus(string connectionID)
        {
            using (SqlConnection con = new SqlConnection(connString))
            {
                //
                // Open the SqlConnection.
                //
                con.Open();

                var userID = 0;
                var roomID = 0;
                string result = $"SELECT * FROM users WHERE connectionID = '{connectionID}'";
                using (SqlCommand command = new SqlCommand(result, con))
                using (SqlDataReader reader = command.ExecuteReader())
                {
                    while (reader.Read())
                    {
                        userID = reader.GetSqlInt32(0).Value;
                    }

                }

                string result2 = $"SELECT * FROM roomsstatus WHERE userID = {userID}";
                using (SqlCommand command = new SqlCommand(result2, con))
                using (SqlDataReader reader = command.ExecuteReader())
                {
                    while (reader.Read())
                    {
                        roomID = reader.GetSqlInt32(1).Value;
                    }

                    return roomID;
                }
            }    
        }

    }

}

