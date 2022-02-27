class HttpLib {
    constructor() {
        this.wsUri = "ws://127.0.0.1:8080/";
        this.websocket = null;
        this.connected = false;
        this.outbound = new Queue();
        this.onNewMessage = null;
        this.onConnectionChange = null;
    }

    Connect() {

        this.websocket = new WebSocket(this.wsUri);
        var obj = this;
        this.websocket.onopen = function() {
            obj.connected = true;
            obj.onConnectionChange("open", "");
        };
        this.websocket.onclose = function() {
            console.log("closing");
            obj.connected = false;
            obj.onConnectionChange("close", "");
        };
        this.websocket.onerror = function(e) {
            obj.onConnectionChange("error", e);
        };

        this.websocket.onmessage = function(message) {

            if (null != obj.onNewMessage) {
                var j = JSON.parse(message.data);
                console.log("recv | op: " + j.op + " | ");
                obj.onNewMessage(j);
            }

        };
    }

    SendJson(dictMessage) {
        this.Send(JSON.stringify(dictMessage));
    }

    Send(message) {
        this.outbound.enqueue(message);
        if (this.connected) {
            while (this.outbound.length() != 0) {
                var msg = this.outbound.dequeue()
                if (msg != "") {
                    this.websocket.send(msg);
                    console.log("send | " + msg);
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
    };

    this.enqueue = function(obj) {
        data.push(obj);
    };

    this.dequeue = function() {
        return data.shift();
    };

    this.peek = function() {
        return data[0];
    };

    this.clear = function() {
        data = [];
    };

    this.length = function() {
        return data.length;
    }
}