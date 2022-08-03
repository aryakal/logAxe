class ViewManager {
    constructor(ctrlLst, channel) {
        this._channel = channel
        this._ctrlLst = ctrlLst;
        this.set_config_on_ui(this._channel.appConfig);

        document.getElementById("lblErrorCount").addEventListener('click', function(e) {
            this.set_filter_log_type(e, 0)
        }
        .bind(this), false);
        document.getElementById("lblInfoCount").addEventListener('click', function(e) {
            this.set_filter_log_type(e, 1)
        }
        .bind(this), false);
        document.getElementById("lblTraceCount").addEventListener('click', function(e) {
            this.set_filter_log_type(e, 2)
        }
        .bind(this), false);
        document.getElementById("lblWarningCount").addEventListener('click', function(e) {
            this.set_filter_log_type(e, 3)
        }
        .bind(this), false);
        document.getElementById("lblTotalCount").addEventListener('click', function(e) {
            this.set_filter_log_type(e, 4)
        }
        .bind(this), false);
        document.getElementById("lblTotalCountHidden").addEventListener('click', function(e) {
            this.set_filter_log_type(e, 5)
        }
        .bind(this), false);

        //Filter ui

        document.getElementById('btnShowFilter').addEventListener('click', this.set_filterUI_toggle.bind(this), false);
        this.filterUI = $("#filterUI");
        this.filterUI.hide();

        

        //this.UserMsgUI = $("#userDialog");
        //this.UserMsgUI.hide();
        //this.UserMsgUI = new LogAxeDialogBox("userDialog")

        document.getElementById("btnFilter").addEventListener('click', this.set_filter_msg.bind(this), false);
        document.getElementById("btnFilterClear").addEventListener('click', this.set_filter_msg_clear.bind(this, 5), false);
        document.getElementById('txtMsgI').addEventListener('keyup', this.set_filter_on_input_event.bind(this), false);
        document.getElementById('txtMsgE').addEventListener('keyup', this.set_filter_on_input_event.bind(this), false);

        //New ui
        document.getElementById('btnNewView').addEventListener('click', this.set_newView_toggle.bind(this), false);
        document.getElementById('btnClear').addEventListener('click', this.set_clear_files.bind(this), false);

        document.getElementById('btnCancelClearFiles').addEventListener('click', this.set_clear_files_cancel.bind(this), false);
        document.getElementById('btnClearFiles').addEventListener('click', this.set_clear_files_ok.bind(this), false);

        this.feUI = $("#feDialog");
        this.feUI.hide();
        //this.set_keep_alive();

        this._channel.OnNewViewInfo = this.set_view_info.bind(this);
        this._channel.OnConnectionStatusEvent = this.on_ws_connection_status_change.bind(this);
        this._ctrlLst.OnPageValueChange = this.OnPageValueChange.bind(this);

    }

    OnPageValueChange() {

        $('#lblPageNo').val(this._channel.appInfo.currentPage);
        $('#lblTotalPagesNo').html(this._channel.appInfo.totalPages);

    }

    on_ws_connection_status_change(status, msg) {
        //console.log(status, msg);
        if ("open" == status) {
            this.get_view_info();
        }
    }

    set_filterUI_toggle() {
        //this.filterUI.toggle(500);
        this.filterUI.slideToggle(100);
    }

    set_newView_toggle() {
        this._channel.cmd_open_newView();
        //TODO this._channel.open_new_ui ?
    }

    get_view_info() {
        this._channel.cmd_get_info();
    }

    set_view_info(viewInfo) {
        this.set_log_info_on_sidebar();
        this._ctrlLst.set_prg_pageNumber();
        this._ctrlLst.ws_send_data();
    }

    helper_value_string(eleId) {
        //console.log(eleId, document.getElementById(eleId));
        var val = document.getElementById(eleId).value;
        if (val == "") {
            return []
        }
        return val.split(";")
    }

    set_filter_on_input_event(e) {
        if (e.key === "Enter") {
            this.set_filter_msg();
        }
    }

    set_filter_msg_clear() {
        var filter = this._channel.info.Filter;
        filter.MsgInclude = [];
        filter.MsgExclude = [];
        this.get_filter();
    }

    set_filter_msg() {
        var filter = this._channel.info.Filter;
        filter.MsgInclude = this.helper_value_string("txtMsgI");
        filter.MsgExclude = this.helper_value_string("txtMsgE");
        this.get_filter();
    }

    set_filter_log_type(event, logType) {
        if (logType <= 3) {
            //console.log(event.ctrlKey, logType);
            if (event.ctrlKey) {
                this._channel.info.Filter.FilterTraces[logType] = !this._channel.info.Filter.FilterTraces[logType];
            } else {
                for (let index = 0; index < 4; index++) {
                    if (index == logType) {
                        this._channel.info.Filter.FilterTraces[logType] = true;
                        continue;
                    }
                    this._channel.info.Filter.FilterTraces[index] = false;
                }
            }
            if (this.helper_is_all_log_type_null()) {
                this.helper_reset_log_type();
            }

        } else {
            this.helper_reset_log_type();
        }
        //console.log(this._channel.info.Filter.FilterTraces)
        this.get_filter();
    }

    helper_reset_log_type() {
        for (var ndx = 0; ndx < 4; ndx++) {
            this._channel.info.Filter.FilterTraces[ndx] = true;
        }
    }

    helper_is_all_log_type_null() {
        for (let index = 0; index < 4; index++) {
            if (this._channel.info.Filter.FilterTraces[index])
                return false;
        }
        return true;
    }

    get_filter() {
        this._channel.cmd_set_filter();
    }

    set_log_info_on_sidebar() {
        var viewInfo = this._channel.info;
        $('#lblErrorCount').html(viewInfo.Error);
        $('#lblInfoCount').html(viewInfo.Info);
        $('#lblWarningCount').html(viewInfo.Warning);
        $('#lblTraceCount').html(viewInfo.Trace);

        $('#lblTotalCount').html(viewInfo.TotalLogLines);
        $('#lblTotalCountHidden').html(viewInfo.SystemTotalLogLine);
        if (viewInfo.SystemTotalLogLine == viewInfo.TotalLogLines) {
            $('#lblTotalCountHidden').html("&#160");
        }
    }

    set_clear_files_cancel(){
        $("#userDialogClearFiles").hide(500);
        $("#userDialog").hide();
    }

    set_clear_files_ok(){
        this._channel.cmd_clear_files();
        this.set_clear_files_cancel();
    }
    
    set_clear_files() {
        console.log(this._channel);
        if (this._channel.info.SystemTotalLogLine != 0) {
            $("#userDialog").show();
            $("#userDialogClearFiles").show(500);            
         }
    }

    set_config_on_ui(cfg) {

        //console.log(cfg);
        //  sidebarUI.css({
        //      'background-color': cfg.color.sideBar,
        //      'font': 'normal 10px \"Segoe UI\"'
        //  });
        $('#lblErrorCount').css({
            'color': cfg.color.error
        });

        $('#lblInfoCount').css({
            'color': cfg.color.info
        });

        $('#lblTraceCount').css({
            'color': cfg.color.trace
        });

        $('#lblWarningCount').css({
            'color': cfg.color.warning
        });

    }

}

class AppInfo {
    constructor() {
        this.totalPages = 0;
        this.currentPage = 0;
    }
}

class LogAxeChannel {
    constructor(appConfig) {
        this.info = null;
        this.appInfo = new AppInfo();
        this.appConfig = appConfig;
        this.name = "";
        this.filter = null;
        // console.log(window.location.href);
        // console.log(window.location.host);
        // console.log(window.location.origin);
        // console.log(window.location.pathname);
        this._ws = new HttpLib(window.location.host, false);
        this._ws.onNewMessage = this.on_new_message.bind(this);
        this._ws.onConnectionChange = this.on_connection_change.bind(this);

        this.OnNewViewInfo = null;
        this.OnNewLinesInfo = null;
        this.OnConnectionStatusEvent = null;
    }

    start() {
        this._ws.Connect();
    }

    on_new_message(message) {
        if ("getInfo" == message.OpCode) {
            if ("all" == message.Name) {
                message.name = this.Name;
            }
            this.name = message.Name;
            this.info = message.Value;

            if (null != this.OnNewViewInfo)
                this.OnNewViewInfo(message);

        } else if ("lines" == message.OpCode) {
            if (null != this.OnNewLinesInfo)
                this.OnNewLinesInfo(message);

        } else if ("refreshView" == message.OpCode) {
            this.cmd_get_info();
        }

    }

    on_connection_change(status, msg) {
        if (null != this.OnConnectionStatusEvent)
            this.OnConnectionStatusEvent(status, msg);
    }

    send_cmd(opCode, name, value) {
        //function to create the command and control structure.
        var cmd = {
            OpCode: opCode,
            name: name,
            Value: value
        };
        this._ws.SendJson(cmd);

    }

    cmd_clear_files() {
        if ("" == this.name)
            return;
        this.send_cmd("clearFiles", this.name, {});
    }

    cmd_get_lines(startLineNo, totalLines) {
        if ("" == this.name)
            return;
        this.send_cmd("lines", this.name, {
            "s": startLineNo,
            "l": totalLines
        });
    }

    cmd_get_info() {
        this.send_cmd("getInfo", this.name, null);
    }

    cmd_set_filter() {
        this.send_cmd("filter", this.name, this.info.Filter);
    }

    cmd_open_newView() {
        this.send_cmd("openView", this.name, null);
    }

    cmd_process_files() {
        this.send_cmd("process", this.name, null);
    }

}

class LogAxeDialogBox{
    constructor(masterDivId) {
        //this._master = document.getElementById(masterDivId);
        this._master = $("#"+masterDivId)
    }

    toggle_modal_dialog(){  
        this._master.toggle();       
    }
    show_modal_dialog(){  
        this._master.show();
    }
    show_modal_dialog(){  
        this._master.hide();
    }
    
}
