 class ViewManager {
     constructor(ctrlLst, channel) {
         this._channel = channel
         this._ctrlLst = ctrlLst;
         this.set_config_on_ui(this._channel.appConfig);

         document.getElementById("lblErrorCount").addEventListener('click', this.set_filter_log_type.bind(this, 0), false);
         document.getElementById("lblInfoCount").addEventListener('click', this.set_filter_log_type.bind(this, 1), false);
         document.getElementById("lblTraceCount").addEventListener('click', this.set_filter_log_type.bind(this, 2), false);
         document.getElementById("lblWarningCount").addEventListener('click', this.set_filter_log_type.bind(this, 3), false);

         document.getElementById("lblTotalCount").addEventListener('click', this.set_filter_log_type.bind(this, 4), false);
         document.getElementById("lblTotalCountHidden").addEventListener('click', this.set_filter_log_type.bind(this, 5), false);

         //Filter ui

         document.getElementById('btnShowFilter').addEventListener('click', this.set_filterUI_toggle.bind(this), false);
         this.filterUI = $("#filterUI");
         this.filterUI.hide();

         this.UserMsgUI = $("#userDialog");
         this.UserMsgUI.hide();

         document.getElementById("btnFilter").addEventListener('click', this.set_filter_msg.bind(this), false);
         document.getElementById("btnFilterClear").addEventListener('click', this.set_filter_msg_clear.bind(this, 5), false);
         document.getElementById('txtMsgI').addEventListener('keyup', this.set_filter_on_input_event.bind(this), false);
         document.getElementById('txtMsgE').addEventListener('keyup', this.set_filter_on_input_event.bind(this), false);

         //New ui
         document.getElementById('btnNewView').addEventListener('click', this.set_newView_toggle.bind(this), false);
         document.getElementById('btnClear').addEventListener('click', this.set_clear_files.bind(this), false);

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
         console.log(status, msg);
         if ("open" == status) {
             this.get_view_info();
         }
     }

     set_filterUI_toggle() {
         this.filterUI.toggle();
     }

     set_newView_toggle() {
         var url = this._viewInfo.appRoute + "openView";
         //TODO this._channel.open_new_ui ?
     }

     get_view_info() {
         this._channel.cmd_get_info();
     }

     set_view_info(viewInfo) {
         console.log(viewInfo)
         this.set_log_info_on_sidebar();
         this._ctrlLst.set_prg_pageNumber();
         this._ctrlLst.json_get_data();
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
         var filter = this._channel.info.frmfilter;
         filter.MsgInclude = this.helper_value_string("txtMsgI");
         filter.MsgExclude = this.helper_value_string("txtMsgE");
         this.get_filter();
     }

     set_filter_log_type(logType) {
         if (logType <= 3) {
             this._channel.info.Filter.FilterTraces[logType] = !this._channel.info.Filter.FilterTraces[logType];
         } else {
             for (var ndx = 0; ndx < 4; ndx++) {
                 this._channel.info.Filter.FilterTraces[ndx] = true;
             }
         }
         //console.log(this._viewInfo.info.Filter.FilterTraces)
         this.get_filter();
     }

     get_filter() {
         console.log(this._channel.info)
         this._channel.cmd_get_filter();
         //  var obj = this;
         //  var url = this._viewInfo.appRoute + "vw?op=filter" + "&n=" + this._viewInfo.viewName;
         //  return $.post(url, JSON.stringify(this._viewInfo.info.Filter), function(data, textStatus) {
         //      obj.get_view_info();
         //  }, "json");
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

     };

     //  set_keep_alive() {
     //      //This is to make sure the view is alive on the server.
     //      //if all view are done and some how the server is still runing then we hava problem.
     //      var obj = this;
     //      var url = this._viewInfo.appRoute + "vw?op=ping" + "&n=" + this._viewInfo.viewName;
     //      if (this._viewInfo.viewName != "") {
     //          $.getJSON(url, function(result) {
     //                  //console.log(url);
     //                  if (result.v != obj._viewInfo.info.v) {
     //                      console.log(obj._viewInfo.info.v, result.v);
     //                      obj.get_view_info();
     //                  }
     //                  obj.UserMsgUI.hide();
     //              })
     //              .catch(function(jqXHR, textStatus, errorThrown) {
     //                  console.log(jqXHR.status)
     //                  if (410 == jqXHR.status) {
     //                      obj._viewInfo.reset();
     //                      obj.get_view_info();
     //                  } else {}
     //              });
     //      }
     //      setTimeout($.proxy(this.set_keep_alive, this), 1000 * 2);
     //  }

     set_clear_files() {
         //  var obj = this;
         //  if (confirm('Are you sure you want to clear all files ?')) {
         //      $.getJSON("/logAxe/files?op=clear", function(result) {
         //          obj.get_view_info();
         //      });
         //  }
         this._channel.cmd_clear_files();
     }

 }

 //This will contain the 
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

         this._ws = new HttpLib();
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
         if ("getInfo" == message.op) {
             if ("all" == message.name) {
                 message.name = this.name;
             }
             this.name = message.name;
             this.info = message.value;
             if (null != this.OnNewViewInfo) this.OnNewViewInfo(message);
         } else if ("lines" == message.op) {
             if (null != this.OnNewLinesInfo) this.OnNewLinesInfo(message);
         } else if ("refreshView" == message.op) {
             this.cmd_get_info();
         }

     }

     on_connection_change(status, msg) {
         //  if ("open" == status) {
         //      this.cmd_get_info();
         //  }

         if (null != this.OnConnectionStatusEvent) this.OnConnectionStatusEvent(status, msg);
     }

     cmd_clear_files() {
         if ("" == this.name) return;
         this._ws.SendJson({
             op: "clearFiles",
             name: this.name,
             value: {}
         });
     }

     cmd_get_lines(startLineNo, totalLines) {
         if ("" == this.name) return;
         this._ws.SendJson({
             op: "lines",
             name: this.name,
             value: { "s": startLineNo, "l": totalLines }
         });
     }

     cmd_get_info() {
         this._ws.SendJson({
             op: "getInfo",
             name: this.name,
             value: null
         });
     }

     cmd_get_filter() {
         this._ws.SendJson({
             op: "filter",
             name: this.name,
             value: this.info.Filter
         });
     }
 }