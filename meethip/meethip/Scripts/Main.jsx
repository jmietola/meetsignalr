
/*$(function () {
    // Declare a proxy to reference the hub.
    var chat = $.connection.chatHub;
    // Create a function that the hub can call to broadcast messages.
    chat.client.broadcastMessage = function (name, message) {
        // Html encode display name and message.
        var encodedName = $('<div />').text(name).html();
        var encodedMsg = $('<div />').text(message).html();
        // Add the message to the page.
        $('#discussion').append('<li><strong>' + encodedName
            + '</strong>:&nbsp;&nbsp;' + encodedMsg + '</li>');
    };
    // Get the user name and store it to prepend to messages.
    $('#displayname').val(prompt('Enter your name:', ''));
    // Set initial focus to message input box.
    $('#message').focus();
    // Start the connection.
    $.connection.hub.start().done(function () {
        $('#sendmessage').click(function () {
            // Call the Send method on the hub.
            chat.server.send($('#displayname').val(), $('#message').val());
            // Clear text box and reset focus for next comment.
            $('#message').val('').focus();
        });
    });
});
*/
// Declare a proxy to reference the hub.
var chat = $.connection.chatHub;

var CommentForm = React.createClass({

    componentDidMount: function () {

        // Create a function that the hub can call to broadcast messages.
        chat.client.addChatMessage = function (message) {
            // Html encode display name and message.
            var encodedMsg = $('<div />').text(message).html();
            // Add the message to the page.
            $('#discussion').append('<li>:&nbsp;&nbsp;' + encodedMsg + '</li>');
        };




        // Create a function that the hub can call to broadcast messages.
        chat.client.broadcastMessage = function (name, message) {
            // Html encode display name and message.
            var encodedName = $('<div />').text(name).html();
            var encodedMsg = $('<div />').text(message).html();
            // Add the message to the page.
            $('#discussion').append('<li><strong>' + encodedName
                + '</strong>:&nbsp;&nbsp;' + encodedMsg + '</li>');
        };
        // Get the user name and store it to prepend to messages.
        $('#displayname').val(prompt('Enter your name:', ''));
        // Set initial focus to message input box.
        $('#message').focus();
        // Start the connection.
        $.connection.hub.start().done(function () {
            $('#sendmessage').click(function () {
                // Call the Send method on the hub.
                chat.server.send($('#displayname').val(), $('#message').val());
                // Clear text box and reset focus for next comment.
                $('#message').val('').focus();
            });



        });
    },

    render: function () {
        return (
            <div className="container">
                <input type="text" id="message" />
                <input type="button" id="sendmessage" value="Send" />
                <input type="hidden" id="displayname" />
                <ul id="discussion"></ul>
      </div>
        );
    }
});



var CommentBox = React.createClass({

    getInitialState: function () {
        return {
            latitude: 66,
            longitude: 66,
            hipFound: false,
            error: null };
    },

    componentDidMount: function () {

        navigator.geolocation.getCurrentPosition(
            (position) => {

                $.connection.hub.start().done(function () {
                    // Call the Locations method on the hub.
                    
                    chat.server.locations($('#displayname').val(), position.coords.latitude, position.coords.longitude);
                });

                this.setState({
                    latitude: position.coords.latitude,
                    longitude: position.coords.longitude,
                    error: null,
                });
            },
            (error) => this.setState({ error: error.message }),
            { enableHighAccuracy: true, timeout: 20000, maximumAge: 1000 },
        );


    },


    render: function () {
        return (
        <div className="CommentBox">
        <header className="App-header">
          <h1 className="App-title">Welcome to MeetHip</h1>
        </header>
        <p>Latitude: {this.state.latitude}</p>
        <p>Longitude: {this.state.longitude}</p>
        {this.state.error ? <p>Error: {this.state.error}</p> : null}

          <p>No hips found</p>
          <CommentForm />
      </div>
        );
    }
});

ReactDOM.render(
    <CommentBox />,
    document.getElementById('content')
);