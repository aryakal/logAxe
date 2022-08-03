class LogLineViewer {

    constructor(canvasName, channelWs) {
        this._channel = channelWs
        this._canvasName = canvasName;
        this._canvas = document.getElementById(this._canvasName);

        this.OnPageValueChange = null;

        this._ctx = null;
        this.fontSize = 13;
        this.fontFamily = 'Courier';
        this._viewCurrentLineNo = 0;
        this._viewTotalLines = 0;
        this._view_RowHeight = 0;
        this._view_PixelBetweenLines = 1;
        this._mouse_down = false;
        this._mouse_downInProgressBar = false;

        this._prg_right = 120;
        this._prg_bar_height = 13;
        this._prg_bar_position = null;
        this._prg_pages_per_pixel = 0;
        this._prg_bar_enabled = false

        this._current_data = null;

        this._current_selected_line_no = -1;

        window.addEventListener("resize", this.set_canvas_resize.bind(this));
        this._canvas.addEventListener('mousedown', this.set_canvas_mousedown.bind(this), false);
        this._canvas.addEventListener('mousemove', this.set_canvas_mousemove.bind(this), false);
        this._canvas.addEventListener('mouseup', this.set_canvas_mouseup.bind(this), false);
        this._canvas.addEventListener('mouseout', this.set_canvas_mouseout.bind(this), false);
        this._canvas.addEventListener('click', this.set_canvas_click.bind(this), false);

        this._canvas.addEventListener('mousewheel', this.set_canvas_mousewheel.bind(this), false);

        this._channel.OnNewLinesInfo = this.ws_receive_data.bind(this);

        this.set_canvas_resize();

    }

    set_font(fontSize, fontFamily) {
        this.fontSize = fontSize;
        this.fontFamily = fontFamily;
    }

    get_font() {
        return this.fontSize + "px " + this.fontFamily;
    }

    get_pixel_ratio() {
        var ctx = document.getElementById(this._canvasName).getContext("2d")
          , dpr = window.devicePixelRatio || 1
          , bsr = ctx.webkitBackingStorePixelRatio || ctx.mozBackingStorePixelRatio || ctx.msBackingStorePixelRatio || ctx.oBackingStorePixelRatio || ctx.backingStorePixelRatio || 1;
        return dpr / bsr;
    }

    set_canvas_resize() {
        this._canvas = document.getElementById(this._canvasName)
        var ratio = this.get_pixel_ratio()

        this._canvas.width = window.innerWidth * ratio;
        this._canvas.height = window.innerHeight * ratio;
        this._canvas.style.width = window.innerWidth + "px";
        this._canvas.style.height = window.innerHeight + "px";
        this._ctx = this._canvas.getContext("2d");
        this._ctx.setTransform(ratio, 0, 0, ratio, 0, 0);

        // this._prg_bar_position = new StoreRect(new StorePoint(window.innerWidth - this._prg_right, radius),
        //     window.innerWidth - this._prg_right,
        //     window.innerHeight - this._prg_bar_height);
        var radius = Math.floor(this._prg_bar_height / 2);
        this._prg_bar_position = new PrgBar(window.innerWidth - this._prg_right,radius,window.innerHeight - (radius * 2),radius,"red");
        this._view_RowHeight = Math.ceil((this.fontSize + this._view_PixelBetweenLines));
        this._viewTotalLines = Math.floor(window.innerHeight / this._view_RowHeight);
        this._ctx.font = this.get_font();
        this._current_data = null;
        //this.set_prg_pageNumber();
        this.ws_send_data();
    }

    set_prg_pageNumber() {
        if (this._channel.info == null) {
            this._prg_bar_enabled = false;
            return;
        }

        this._channel.appInfo.totalPages = Math.ceil(this._channel.info.TotalLogLines / this._viewTotalLines);
        this._prg_pages_per_pixel = this._channel.appInfo.totalPages / this._prg_bar_position.rect.height;
        this._channel.appInfo.currentPage = 0;
        this._prg_bar_enabled = this._channel.appInfo.totalPages != 0;
        if (null != this.OnPageValueChange)
            this.OnPageValueChange();
    }

    set_mouse_position(mouse_pos) {
        this._prg_bar_position.set_y(mouse_pos);
        this._channel.appInfo.currentPage = Math.ceil(this._prg_bar_position.get_height() * this._prg_pages_per_pixel);
        this._viewCurrentLineNo = this._channel.appInfo.currentPage * this._viewTotalLines;
        this.ws_send_data();
        if (null != this.OnPageValueChange)
            this.OnPageValueChange();
    }

    set_canvas_mousedown(event) {
        this._mouse_down = true;
        this._mouse_downInProgressBar = this._prg_bar_position.get_hit(event.clientX, event.clientY);
    }

    set_canvas_mouseup(event) {
        this._mouse_down = false;
        if (this._mouse_downInProgressBar) {
            this.set_mouse_position(event.y);
            this._mouse_downInProgressBar = false;
        }
    }

    set_canvas_mouseout(event) {
        if (this._mouse_down) {
            if (this._mouse_downInProgressBar) {
                this.set_mouse_position(event.y);
                this._mouse_downInProgressBar = false;
            }
        }
    }

    set_canvas_mousemove(event) {
        if (this._mouse_down) {
            if (this._mouse_downInProgressBar) {
                this.set_mouse_position(event.y);
            }
        }
    }

    set_canvas_click(event) {
        let lineNo = parseInt(event.y / this._view_RowHeight, 10);
        if (this._current_selected_line_no != lineNo) {
            this._current_selected_line_no = lineNo;
            this.draw();
        }
    }

    set_canvas_mousewheel(event) {
        let prev = this._viewCurrentLineNo;
        let lineNo = parseInt(event.deltaY / this._view_RowHeight, 10);
        this._viewCurrentLineNo = this._viewCurrentLineNo + lineNo;
        if (this._viewCurrentLineNo < 0) {
            this._viewCurrentLineNo = 0;
        } else if (this._viewCurrentLineNo > this._channel.info.TotalLogLines) {
            this._viewCurrentLineNo = this._channel.info.TotalLogLines;
        }
        if (prev != this._viewCurrentLineNo)
            this.ws_send_data();
    }

    draw() {
        var ctx = this._ctx;
        var cfg = this._channel.appConfig;
        ctx.clearRect(0, 0, this._canvas.width, this._canvas.height);
        var rowY = 0;
        var previousDate = "";
        if (this._current_selected_line_no != -1) {
            this.set_line_background(this._current_selected_line_no, "#DCD7B6");
        }
        if (this._current_data != null && this._current_data.LogLines.length != 0) {
            for (let ndx = 0; ndx < this._viewTotalLines; ndx++) {
                if (ndx < this._current_data.LogLines.length) {
                    switch (this._current_data.LogLines[ndx].LogType) {
                    case 0:
                        // $('#m' + lineId).css('color', configGlobal.color.error);
                        ctx.fillStyle = cfg.color.error;
                        break;
                    case 1:
                        // $('#m' + lineId).css('color', configGlobal.color.info);
                        ctx.fillStyle = cfg.color.info;
                        break;
                    case 2:
                        // $('#m' + lineId).css('color', configGlobal.color.trace);
                        ctx.fillStyle = cfg.color.trace;
                        break;
                    case 3:
                        // $('#m' + lineId).css('color', configGlobal.color.warning);
                        ctx.fillStyle = cfg.color.warning;
                        break;
                    }
                    // this.set_line_background(0, "#F9F7F7");
                    rowY += this._view_RowHeight;
                    if (previousDate != this._current_data.LogLines[ndx].TimeStamp) {
                        ctx.fillText(this._current_data.LogLines[ndx].TimeStamp, 10, rowY);
                        previousDate = this._current_data.LogLines[ndx].TimeStamp;

                    }

                    ctx.fillText(this._current_data.LogLines[ndx].Msg, 180, rowY);
                }
            }

        }
        this.draw_prg();
    }

    set_line_background(lineNo, color) {
        this._ctx.fillStyle = color;
        this._ctx.fillRect(0, (this._view_RowHeight * lineNo) + 3, window.innerWidth, this._view_RowHeight);
    }

    draw_prg() {

        if (!this._prg_bar_enabled)
            return;

        var ctx = this._ctx;
        ctx.beginPath();
        ctx.strokeStyle = '#A8A8A8';
        ctx.fillStyle = '#D0D0D0';
        ctx.lineWidth = 1;
        var rect = this._prg_bar_position.rect;
        ctx.moveTo(rect.x, rect.y);
        ctx.lineTo(rect.x, rect.y + rect.height);
        ctx.stroke();
        ctx.beginPath();
        ctx.arc(this._prg_bar_position.x, this._prg_bar_position.y, this._prg_bar_position.radius, 0, 2 * Math.PI, false);
        ctx.fill();
        ctx.stroke();
    }

    ws_send_data() {
        this._channel.cmd_get_lines(this._viewCurrentLineNo, this._viewTotalLines);
    }

    ws_receive_data(message) {
        this._current_data = message.Value;
        this.draw();
    }

}
//class ends

class StorePoint {
    constructor(x, y) {
        this.x = x;
        this.y = y;

    }
}

class StoreRect {
    constructor(x, y, width, height) {
        this.x = x;
        this.y = y;
        this.height = height;
        this.width = width;
        this.x2 = this.x + this.width;
        this.y2 = this.y + this.height;
    }
}

class PrgBar {
    constructor(x, y, height, radius, color) {
        //console.log(x, y, height, radius, color)
        this.x = x;
        this.y = y;
        this.rect = new StoreRect(x,y,radius * 2,height);
        this.radius = radius;
        this.color = color;
    }
    get_hit(x, y) {
        return (x >= (this.x - this.radius) && x <= (this.x + this.radius) && y >= (this.y - this.radius) && y <= (this.y + this.radius));

    }
    set_y(y) {
        // if (y < (this.y - this.radius)) {
        //     this.y = this.rect.y;
        // } else if (y > (this.y + this.radius)) {
        //     this.y = y;
        // }
        this.y = y;
    }
    get_height() {
        var ret = this.y - this.radius
        if (ret < 0)
            return 0;
        if (ret > this.rect.height)
            return this.rect.height;
        return ret;

    }
}
