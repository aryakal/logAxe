class HttpLib {

    //-------------------------------------------------------------
    // HttpLib is a wrapper around ws (webservice) and json 
    // interface to provide uniform communication.
    // We can use a channel(s) on top of this library to put the 
    // actual communication protocol.
    //-------------------------------------------------------------

    constructor(baseUrl, tryReconnect) {
        this.wsUri = "ws://" + baseUrl + "/register/";
        this.websocket = null;
        this.connected = false;
        this.outbound = new Queue();
        this.onNewMessage = null;
        this.onConnectionChange = null;
        this.tryReconnect = tryReconnect;

        this.enableLog = true;
        if (this.enableLog)
            this.debug = console.log.bind(window.console)
        else
            this.debug = function() {}
    }

    Connect() {

        try {
            console.log("Connecting");
            console.log(this.wsUri);
            this.websocket = new WebSocket(this.wsUri);
        } catch (error) {
            setTimeout(function() {
                this.Connect();
            }, 10000);
        }

        var obj = this;
        this.websocket.onopen = function() {
            obj.connected = true;
            obj.onConnectionChange("open", "");
        }
        ;

        this.websocket.onclose = function() {
            obj.connected = false;
            obj.onConnectionChange("close", "");
            if (obj.tryReconnect) {
                obj.Connect();
            }
        }
        ;
        this.websocket.onerror = function(e) {
            obj.onConnectionChange("error", e);
        }
        ;

        this.websocket.onmessage = function(message) {
            if (null != obj.onNewMessage) {
                var j = JSON.parse(message.data);
                obj.debug("recv | OpCode: " + j.OpCode + " | " + j.Status);
                obj.onNewMessage(j);
            }

        }
        ;
    }

    SendJson(dictMessage) {
        this.debug("send | OpCode: " + dictMessage.OpCode + " | ");
        this.Send(JSON.stringify(dictMessage));
    }

    Send(message) {
        this.outbound.enqueue(message);
        if (this.connected) {
            while (this.outbound.length() != 0) {
                var msg = this.outbound.dequeue()
                if (msg != "") {
                    this.websocket.send(msg);
                    //this.debug("send | " + msg);
                }
            }
            return true;
        }
        return false;
    }
}

function Queue() {
    var data = [];

    this.isEmpty = function() {
        return (data.length == 0);
    }
    ;

    this.enqueue = function(obj) {
        data.push(obj);
    }
    ;

    this.dequeue = function() {
        return data.shift();
    }
    ;

    this.peek = function() {
        return data[0];
    }
    ;

    this.clear = function() {
        data = [];
    }
    ;

    this.length = function() {
        return data.length;
    }
}
