﻿//=====================================================================================================================
// Source : https://github.com/aryakal/
//--------------------------------------------------------------------------------------------------------------------
// Subsystem (for future me)
// MasterThread   | DoWork      | Everyting starts here. This long running task is executing every command.
// Draw           | DoWork_Draw | All drawing function starts here.

// TODO : 
// On click of the line no need to fetch data from the server.
// filters
// Files
// how memory of the files etc.
//=====================================================================================================================

using System;
using System.Drawing;
using System.Windows.Forms;
using System.Threading;
using System.Threading.Tasks;
using System.Collections.Concurrent;
using logAxeCommon;
//using logAxeEngine.Common;
//using logAxeEngine.Interfaces;
//using logAxeEngine.EventMessages;
using System.Text;
using libALogger;
using libACommunication;


namespace logAxe
{
   public partial class CntrlTextViewer : UserControl
   { 
      internal class MouseState
      {
         public bool IsMouseDown { get; set; }
         public int X { get; set; }
         public int Y { get; set; }
      }
      public string UniqueId { get; }

      private ConfigUI _userConfig;

      private bool _drawRequired = false;
      private TableSkeleton _table;
      private DrawSurface _newCanvas = new DrawSurface();

      
      //private frmFileManager _fld;
      private frmLineData _lineInfoDlg = new frmLineData();
      private CustomMoveCntrl _moveFilterBox;

      private TermFilter CurrentFilter = new TermFilter();

      private MouseState _mouse = new MouseState();            
      private ILibALogger _logger;
      
      private bool _isNotepad = false;
      public string NotepadName { get; set; }
      public bool IsViewNotepad
      {
         get
         {
            return _isNotepad;
         }
         set
         {
            _isNotepad = value;
            if (_isNotepad)
            {
               pnlFuture.Enabled = false;
               TogglePanel();
            }

         }
      }      
      public string _filterMessage = "";

      public Action<CntrlTextViewerMsg> OnNewNotepadChange { get; set; }
      public bool IsMainWindow { get; set; }
      public string FilterMessage
      {
         get
         {
            return _filterMessage;
         }
         private set
         {
            _filterMessage = value;
            OnNewNotepadChange?.Invoke(CntrlTextViewerMsg.SetTitle);
         }
      }

      private SemaphoreSlim _lockDraw = new SemaphoreSlim(1, 1);
      private System.Windows.Forms.Timer _timer;
      private System.Windows.Forms.Timer _timerBackgroundDraw;
      private System.Windows.Forms.Timer _timerUpdateInfo;

      public CntrlTextViewer()
      {
         UniqueId = $"View-{DateTime.Now:HHmmssfff}";
         _logger = new NamedLogger("v"+UniqueId.Substring(10,3));
         InitializeComponent();

         _table = new TableSkeleton(masterPanel.Location, masterPanel.Size);
         _table.AddColumn(new TableColumn("LineNo") {Visible = false });
         _table.AddColumn(new TableColumn("ProcId") { Width = 20, Visible = false });
         _table.AddColumn(new TableColumn("ThId") { Width = 20, Visible = false });
         _table.AddColumn(new TableColumn("Category") { Width = 100, Visible = false });
         _table.AddColumn(new TableColumn("TimeStamp"));
         _table.AddColumn(new TableColumn("ImageNo") { Width = 20, DisplayName = "" });
         _table.AddColumn(new TableColumn("Message"));
         _moveFilterBox = new CustomMoveCntrl(lblFiterBoxTxt, pnlFilterSearch);
         _lineInfoDlg.MoveLine += MoveLine;

         if (ViewCommon.Channel != null)
         {
            _userConfig = ViewCommon.ConfigOfSystem;
            //ViewCommon.Channel.RegisterClient(UniqueId, DoWork);
            new HelperAttachFileDrop(this);
            new HelperAttachFileDrop(masterPanel);
            RegisterControl(masterPanel);
            SetForeColor();
            AddMenuItem();
            _newCanvas.SetSize(masterPanel.Size);
            _table.Resize(_newCanvas.bmp.Size, ViewCommon.ConfigOfSystem);
            _table.Dirty = false;

            _timerBackgroundDraw = new System.Windows.Forms.Timer();
            _timerBackgroundDraw.Interval = 50;
            _timerBackgroundDraw.Tick += (sender, args) => DoWork_Draw3(); 
            _timerBackgroundDraw.Start();

            //ViewCommon.GetCurrentTheme(viewName: UniqueId);
            //ViewCommon.GetFilterList(viewName: UniqueId);

            
         }
      }

      public void Register(bool isMainView)
      {
         IsMainWindow = isMainView;
         ViewCommon.Channel.RegisterClient(UniqueId, DoWork, !IsViewNotepad);
         ViewCommon.GetCurrentTheme(viewName: UniqueId);
         ViewCommon.GetFilterList(viewName: UniqueId);
      }
      public void UnRegister()
      {  
         ViewCommon.Channel.UnRegisterClient(UniqueId);
      }

      public void EnableFileAppInfo()
      {
         lblFileSize.Invoke(new Action(() =>
         {
            lblFileSize.Visible = true;
            lblAppMemSize.Visible = true;
            label17.Visible = true;
            label14.Visible = true;
         }));

         ViewCommon.GetFileAppMemInfo(UniqueId);
         _timerUpdateInfo = new System.Windows.Forms.Timer();
         _timerUpdateInfo.Interval = 5000;
         _timerUpdateInfo.Tick += (sender, args) => ViewCommon.GetFileAppMemInfo(UniqueId);
         _timerUpdateInfo.Start();
      }


      public readonly string FilterBoxLbl = "Filter log lines ( + Include - Exclude)";
      private RectangleF GetRectF(float x, float y, float size)
      {
         return new RectangleF(x - size, y - size, size * 2, size * 2);
      }
      protected override bool ProcessCmdKey(ref System.Windows.Forms.Message msg, Keys keyData)
      {
         switch (keyData)
         {
            case (Keys.Control | Keys.F):
            case Keys.F3:
               togge_filter_screen();
               break;
            case Keys.NumLock:
               if (!_isNotepad)
                  TogglePanel();
               break;
            case Keys.Escape:
               //if (!_isNotepad)
               //   _msgHelper.PostMessage(new LogAxeGenericMessage() { MessageType = LogAxeMessageEnum.AwakeAllWindows });
               break;
         }

         if (!masterPanel.Focused)
         {
            return base.ProcessCmdKey(ref msg, keyData);
         }

         switch (keyData)
         {
            case Keys.Home:
               _table.MovetoPage(0);
               QueueDrawRequest($"ProcessCmdKey, {keyData}");
               break;
            case Keys.End:
               _table.MovetoPage(_table.TotalPages);
               QueueDrawRequest($"ProcessCmdKey, {keyData}");
               break;
            case Keys.F6:
               _table["ProcId"].Visible = !_table["ProcId"].Visible;
               _table.Dirty = true;
               QueueDrawRequest($"ProcessCmdKey, {keyData}");
               break;
            case Keys.F7:
               _table["ThId"].Visible = !_table["ThId"].Visible;
               _table.Dirty = true;
               QueueDrawRequest($"ProcessCmdKey, {keyData}");
               break;
            case Keys.F8:
               _table["Category"].Visible = !_table["Category"].Visible;
               _table.Dirty = true;
               QueueDrawRequest($"ProcessCmdKey, {keyData}");
               break;
            case Keys.F9:
               var val = !_table["ProcId"].Visible;
               _table["ProcId"].Visible = val;
               _table["ThId"].Visible = val;
               _table["Category"].Visible = val;
               _table.Dirty = true;
               QueueDrawRequest($"ProcessCmdKey, {keyData}");
               break;
            case Keys.F10:
               _table["TimeStamp"].Visible = !_table["TimeStamp"].Visible;
               _table.Dirty = true;
               QueueDrawRequest($"ProcessCmdKey, {keyData}");
               break;
            case Keys.F11:
               _table["LineNo"].Visible = !_table["LineNo"].Visible;
               _table.Dirty = true;
               QueueDrawRequest($"ProcessCmdKey, {keyData}");
               break;
            case Keys.PageDown:
               _table.GotoNextPage();
               QueueDrawRequest($"ProcessCmdKey, {keyData}");
               break;
            case Keys.PageUp:
               _table.GotoPreviouPage();
               QueueDrawRequest($"ProcessCmdKey, {keyData}");
               break;
            case Keys.Up:
               _table.MoveLine(-1);
               QueueDrawRequest($"ProcessCmdKey, {keyData}");
               break;
            case Keys.Down:
               _table.MoveLine(1);
               QueueDrawRequest($"ProcessCmdKey, {keyData}");
               break;

            case (Keys.Control | Keys.C):
               CopyLinesToClipBoard();
               break;
            default:
               return base.ProcessCmdKey(ref msg, keyData);
         }
         return true;
      }
      protected override void OnHandleDestroyed(EventArgs e)
      {
         _moveFilterBox.Release();
         //_msgHelper?.Unregister();
      }
      private void MoveLine(int delta)
      {
         _ = _table.MoveByLineDelta(delta);
         QueueDrawRequest("move line");
      }
      private void RegisterControl(Control ctrl)
      {
         ctrl.Resize += Ctrl_Resize;
         ctrl.MouseUp += Ctrl_MouseUp;
         ctrl.MouseDown += Ctrl_MouseDown;
         ctrl.MouseDoubleClick += Ctrl_MouseDoubleClick;
         ctrl.MouseMove += Ctrl_MouseMove;
         ctrl.MouseWheel += Ctrl_MouseWheel;
      }

      #region Mouse_cntrl
      bool _cancelNextMouseUp = false;
      private void Ctrl_MouseWheel(object sender, MouseEventArgs e)
      {
         int numberOfTextLinesToMove = (e.Delta * -1) * SystemInformation.MouseWheelScrollLines / 120;
         _table.MoveLine(numberOfTextLinesToMove);
         QueueDrawRequest("Ctrl_MouseWheel");
      }
      private void Ctrl_MouseMove(object sender, MouseEventArgs e)
      {
         if (_table.ScrollMove)
         {
            ScrollMove(e.X, e.Y);
            QueueDrawRequest("Ctrl_MouseMove");
         }
      }
      private void Ctrl_MouseDoubleClick(object sender, MouseEventArgs e)
      {
         SetInfoDialogData();
         _lineInfoDlg.ShowDialog();
         _cancelNextMouseUp = true;
      }
      private void Ctrl_MouseDown(object sender, MouseEventArgs e)
      {
         masterPanel.Focus();
         if (MouseButtons.Left == e.Button)
         {
            if (_table.ScrollBarArea.Contains(e.X, e.Y))
            {
               _table.ScrollMove = true;
               ScrollMove(e.X, e.Y);
            }
            else
            {
               //todo:  which line :?
            }
         }

      }
      private void Ctrl_MouseUp(object sender, MouseEventArgs e)
      {
         //code to fix the issue of auto select after a mouse double click.
         if (_cancelNextMouseUp)
         {
            _cancelNextMouseUp = false;
            return;
         }

         if (e.Button == MouseButtons.Left)
         {
            if (_table.ScrollMove)
            {
               _table.ScrollMove = false;
            }
            else
            {
               var (draw, logLine) = _table.SetCurrentSelection(e.Y, Control.ModifierKeys);
               if (draw)
               {
                  QueueDrawRequest("Ctrl_MouseUp");
               }

               if (logLine != null)
               {
                  ViewCommon.MsgBroadcastGlobalLine(UniqueId, logLine.GlobalLine);
               }

            }
         }

      }
      private void ScrollMove(int x, int y)
      {
         _table.ChangeScrollMove(y);
         QueueDrawRequest("ScrollMove");
      }
      private void masterPanel_Paint(object sender, PaintEventArgs e)
      {
         //QueueDrawRequest("masterPanel_Paint");
         if (_timer != null && _timer.Enabled)
         {
            return;
         }
         DoWork_Draw2("masterPanel_Paint");
      }

      private void Ctrl_Resize(object sender, EventArgs e)
      {
         _logger?.Debug("Resize start");
         if (null == _timer)
         {
            _timer = new System.Windows.Forms.Timer();
            _timer.Interval = 200;
            _timer.Tick += _timer_Tick;
         }
         _timer.Start();
         //QueueDrawRequest("Ctrl_Resize", resetSize: true);
      }

      private void _timer_Tick(object sender, EventArgs e)
      {
         _logger?.Debug("Resize complete");
         _timer.Stop();
         QueueDrawRequest("Ctrl_Resize");
      }

      void CalculateChange(int x, int y, bool isMouseDown)
      {
         if (_mouse.IsMouseDown)
         {
         }
      }
      #endregion

      #region cntrl events
      private void CntrlTextViewer_KeyUp(object sender, KeyEventArgs e)
      {

      }
      private void btnFilter_Click(object sender, EventArgs e)
      {
         togge_filter_screen();
      }
      private void togge_filter_screen()
      {
         //btnFilter.Text = "Show Filter";
         pnlFilterSearch.Visible = !pnlFilterSearch.Visible;
         btnFilter.Text = pnlFilterSearch.Visible ? "Hide Filter" : "Show Filter";
         if (pnlFilterSearch.Visible)
         {
            txtInclude.Focus();
            pnlFilterTag.Visible = _showPnlFilterTag;  
         }
         else
         {
            masterPanel.Focus();
            pnlFilterTag.Visible = false;
         }
      }

      bool _showPnlFilterTag = false;
      private void btnShowSavedFilters_Click(object sender, EventArgs e)
      {
         pnlFilterTag.Location = new Point(pnlFilterSearch.Location.X, pnlFilterSearch.Location.Y + pnlFilterSearch.Size.Height + 2);
         _showPnlFilterTag = !_showPnlFilterTag;
         pnlFilterTag.Visible = _showPnlFilterTag;
      }
      
      #endregion

      #region context menu
      private void contextMenu_Opening(object sender, System.ComponentModel.CancelEventArgs e)
      {
         //Check if any line is selected in the view.
         var (isLineSlected, logLine) = _table.GetSelectedLine();

         //enable disable menu button based on line selected and if it is a notepad.
         copyToNotepadToolStripMenuItem.Enabled = isLineSlected;
         exportFileToolStripMenuItem.Enabled = isLineSlected;
         filterByToolStripMenuItem.Enabled = false;

         if ((isLineSlected && !_isNotepad) && null!=logLine)
         {
            filterByToolStripMenuItem.Enabled = true;
            filterByThreadId.Text = $"ThreadId- {logLine.ThreadNo}";
            filterByCategoryId.Text = $"Category- {logLine.Category}";
            filterByProcId.Text = $"ProcessId- {logLine.ProcessId}";
         }

         //which time format is currently selected.
         showDefaultTimeToolStripMenuItem.Checked = _table.ShowTimeSelected == TableSkeleton.ShowTime.Default;
         setStartTimeToolStripMenuItem.Checked = _table.ShowTimeSelected == TableSkeleton.ShowTime.StartTime;
         setStartTimeToolStripMenuItem.Enabled = isLineSlected;
         copyLinesToolStripMenuItem.Enabled = isLineSlected;

         //All column and line show.
         showTableHeaderToolStripMenuItem.Checked = _table.ShowTableHeader;
         showLineNoToolStripMenuItem.Checked = _table["LineNo"].Visible;
         showProcIdToolStripMenuItem.Checked = _table["ProcId"].Visible;
         showThreadIdToolStripMenuItem.Checked = _table["ThId"].Visible;
         showCategoryToolStripMenuItem.Checked = _table["Category"].Visible;

      }
      private void copyLinesToolStripMenuItem_Click(object sender, EventArgs e)
      {
         CopyLinesToClipBoard();
      }
      private void showTableHeaderToolStripMenuItem_Click(object sender, EventArgs e)
      {
         _table.ShowTableHeader = !showTableHeaderToolStripMenuItem.Checked;
         QueueDrawRequest("showLineNoToolStripMenuItem");
      }
      private void showLineNoToolStripMenuItem_Click(object sender, EventArgs e)
      {
         _table["LineNo"].Visible = !showLineNoToolStripMenuItem.Checked;
         QueueDrawRequest("showLineNoToolStripMenuItem");
      }
      private void showProcIdToolStripMenuItem_Click(object sender, EventArgs e)
      {
         _table["ProcId"].Visible = !showProcIdToolStripMenuItem.Checked;
         QueueDrawRequest("showLineNoToolStripMenuItem");
      }
      private void showThreadIdToolStripMenuItem_Click(object sender, EventArgs e)
      {
         _table["ThId"].Visible = !showThreadIdToolStripMenuItem.Checked;
         QueueDrawRequest("showLineNoToolStripMenuItem");
      }
      private void showCategoryToolStripMenuItem_Click(object sender, EventArgs e)
      {
         _table["Category"].Visible = !showCategoryToolStripMenuItem.Checked;
         QueueDrawRequest("showLineNoToolStripMenuItem");
      }

      private void exportFileToolStripMenuItem_Click(object sender, EventArgs e)
      {
         //TODO : export file for the selected lines.
         //ShowFileManager(_table.GetFileIndexForSeletedLines());
      }
      private void newNotepadToolStripMenuItem_Click(object sender, EventArgs e)
      {
         var notepadName = ((ToolStripMenuItem)sender).Text;
         if (notepadName == "New Notepad Window")
         {
            notepadName = ViewCommon.OpenNewNotepad();
         }

         //_msgHelper.PostMessage(
         //   new AddLineToNotepadEvent()
         //   {
         //      NotebookName = notepadName,
         //      GlobalLine = _table.GetGlobalLineIndexForSeletedLines()
         //   });

         AddMenuItem();
      }
      private void showDefaultTimeToolStripMenuItem_Click(object sender, EventArgs e)
      {
         _table.ShowTimeSelected = TableSkeleton.ShowTime.Default;
         QueueDrawRequest("showDefaultTimeToolStripMenuItem");
      }
      private void setStartTimeToolStripMenuItem_Click(object sender, EventArgs e)
      {
         var (_, logLine) = _table.GetSelectedLine();
         _table.ShowTimeSelected = TableSkeleton.ShowTime.StartTime;
         _table.ShowTime_Default_StartTime = logLine.TimeStamp;
         QueueDrawRequest("setStartTimeToolStripMenuItem");
      }
      #endregion

      private void SetForeColor()
      {
         lblTrace.ForeColor = _userConfig.MsgTraceFontColor;
         lblError.ForeColor = _userConfig.MsgErrorFontColor;
         lblWarning.ForeColor = _userConfig.MsgWarningFontColor;
         lblInfo.ForeColor = _userConfig.MsgInfoFontColor;
      }
      //public void GetMessage(logAxeEngine.Interfaces.ILogAxeMessage message)
      //{
      //   _queueWork.Add(new Work(Work.WType.Message, message)); ;
      //}
      //private void SetView(LogFrame frame)
      //{
      //   _table.SetViewFrame(frame);
      //   _view = frame;
      //   _table.SetTotalLines(_view.TotalLogLines);
      //   _table.CurrentDataLine = 0;
      //   _table.Dirty = true;
      //   masterPanel.Invoke(new Action(() =>
      //   {
      //      lblTrace.Text = _view.LogTypeLength(LogType.Trace).ToString();
      //      lblError.Text = _view.LogTypeLength(LogType.Error).ToString();
      //      lblWarning.Text = _view.LogTypeLength(LogType.Warning).ToString();
      //      lblInfo.Text = _view.LogTypeLength(LogType.Info).ToString();
      //      lblTotal.Text = _table.TotalDataLines.ToString();
      //      lblHiddenTotal.Text = _view.TotalLogLines != _view.SystemTotalLogLine ?
      //         (_view.SystemTotalLogLine - _table.TotalDataLines).ToString() :
      //         "";

      //      lblPage.Text = _table.TotalPages.ToString();
      //      HelperSetLevelFontStrikeOut(lblHiddenTotalTxt, lblHiddenTotal.Text != "");

      //   }));
      //   QueueDrawRequest("SetView");
      //}
      //private string GetMaxLength(int value)
      //{
      //   if (value > 10000000) return "999999999";
      //   else if (value > 1000000) return "99999999";
      //   else if (value > 100000) return "9999999";
      //   else if (value > 10000) return "999999";
      //   else if (value > 1000) return "99999";
      //   else if (value > 100) return "9999";
      //   else if (value > 10) return "999";
      //   else return "99";
      //}      
      private void masterPanel_PreviewKeyDown(object sender, PreviewKeyDownEventArgs e)
      {

      }
      private void SetInfoDialogData()
      {
         var (lineNo, totalLines, lineInfo) = _table.GetCurrentSelectedLineInfo();
         _lineInfoDlg.SetCurrentLine(lineNo, totalLines, lineInfo);
      }
      
      private void CopyLinesToClipBoard()
      {
         try
         {
            var lst = _table.GetAllSelectedLogLines();

            if (lst.Length == 0)
               return;
            ViewCommon.ExportLinesToClipBoard(UniqueId, lst);
            
         }
         catch (Exception ex)
         {
            MessageBox.Show(ex.ToString(), "Error in copy to clipboard", MessageBoxButtons.OK);
         }
      }
      public void ExportToClipboard(string selectedHLines, string selectedPLines)
      {
         var clipText = new DataObject();
         clipText.SetData(DataFormats.Html, GetMSFormattedHtml(selectedHLines.ToString()));
         clipText.SetData(DataFormats.Text, selectedPLines.ToString());
         clipText.SetData(DataFormats.UnicodeText, selectedPLines.ToString());
         Clipboard.SetDataObject(clipText, true);
      }
      private string GetMSFormattedHtml(string html)
      {
         //https://docs.microsoft.com/en-us/windows/win32/dataxchg/html-clipboard-format

         var headerSb = new StringBuilder();

         headerSb.Append("Version:0.9" + Environment.NewLine);
         headerSb.Append("StartHTML:000000001" + Environment.NewLine);
         headerSb.Append("EndHTML:000000002" + Environment.NewLine);
         headerSb.Append("StartFragment:000000003" + Environment.NewLine);
         headerSb.Append("EndFragment:000000004" + Environment.NewLine);
         headerSb.Append("StartSelection:000000003" + Environment.NewLine);
         headerSb.Append("EndSelection:000000004" + Environment.NewLine);

         var indexOfStartHtml = Encoding.ASCII.GetByteCount(headerSb.ToString());

         headerSb.AppendLine(@"<!DOCTYPE HTML PUBLIC ""-//W3C//DTD HTML 4.0 Transitional//EN"">");
         headerSb.Append("<html><body>");

         headerSb.Append("<!--StartFragment-->");
         var indexOfStartOfFragment = Encoding.UTF8.GetByteCount(headerSb.ToString());


         headerSb.Append(html);

         headerSb.Append("<!--EndFragment-->");
         var indexOfEndOfFragment = Encoding.UTF8.GetByteCount(headerSb.ToString());

         headerSb.Append("</body></html>");
         var indexOfEndHtml = Encoding.UTF8.GetByteCount(headerSb.ToString());


         headerSb.Replace("000000001", indexOfStartHtml.ToString("D9"), 0, indexOfStartHtml);
         headerSb.Replace("000000002", indexOfEndHtml.ToString("D9"), 0, indexOfStartHtml);
         headerSb.Replace("000000003", indexOfStartOfFragment.ToString("D9"), 0, indexOfStartHtml);
         headerSb.Replace("000000004", indexOfEndOfFragment.ToString("D9"), 0, indexOfStartHtml);

         return headerSb.ToString();
      }
      private void TogglePanel()
      {
         pnlFuture.Visible = !pnlFuture.Visible;
         if (pnlFuture.Visible)
         {
            masterPanel.Width -= pnlFuture.Width;
         }
         else
         {
            masterPanel.Width += pnlFuture.Width;
         }
      }
      private void btnManageFile_Click(object sender, EventArgs e)
      {
         ViewCommon.ShowFileManager();
         //ShowFileManager(new int[] { 1 });
      }
      private void lblTrace_Click(object sender, EventArgs e)
      {
         SetLogTypeFilter(LogType.Trace);
      }
      private void lblError_Click(object sender, EventArgs e)
      {
         SetLogTypeFilter(LogType.Error);
      }
      private void lblWarning_Click(object sender, EventArgs e)
      {
         SetLogTypeFilter(LogType.Warning);
      }
      private void lblInfo_Click(object sender, EventArgs e)
      {
         SetLogTypeFilter(LogType.Info);
      }
      private void lblTotal_Click(object sender, EventArgs e)
      {
         CurrentFilter.FilterTraces = new bool[] { true, true, true, true };
         HelperSetFilter(CurrentFilter);
      }
      private void lblHiddenTotal_Click(object sender, EventArgs e)
      {
         ResetFilter();
      }
      private void SetLogTypeFilter(LogType logType)
      {
         
         if (_table.ViewFrame.SystemTotalLogLine == 0)
            return;

         if (Control.ModifierKeys == Keys.Control)
         {
            CurrentFilter.FilterTraces[(int)logType] = !CurrentFilter.FilterTraces[(int)logType];
         }
         else
         {
            //todo: change universal constant.
            CurrentFilter.FilterTraces = new bool[4];
            CurrentFilter.FilterTraces[(int)logType] = true;
         }
         HelperSetFilter(CurrentFilter);
      }
      private void HelperSetLevelFontStrikeOut(Label lbl, bool strike)
      {
         lbl.Font = new Font(
                      lblTrace.Font.FontFamily,
                      lblTrace.Font.Size,
                      strike ? System.Drawing.FontStyle.Regular : System.Drawing.FontStyle.Strikeout,
                      System.Drawing.GraphicsUnit.Point,
                      ((byte)(0)));
      }
      private void ResetFilter()
      {
         HelperSetFilter(new TermFilter());
      }
      private void HelperSetFilter(TermFilter filter, bool assignToText = false)
      {
         if (CurrentFilter.GetHashCode() != filter.GetHashCode())
         {
            CurrentFilter = filter;
         }
         txtExclude.Text = string.Join(";", filter.MsgExclude);
         txtInclude.Text = string.Join(";", filter.MsgInclude);
         txtTagsInclude.Text = string.Join(";", filter.TagsInclude);
         txtTagsExclude.Text = string.Join(";", filter.TagsExclude);
         HelperSetLevelFontStrikeOut(lblError, filter.FilterTraces[(int)LogType.Error]);
         HelperSetLevelFontStrikeOut(lblTrace, filter.FilterTraces[(int)LogType.Trace]);
         HelperSetLevelFontStrikeOut(lblInfo, filter.FilterTraces[(int)LogType.Info]);
         HelperSetLevelFontStrikeOut(lblWarning, filter.FilterTraces[(int)LogType.Warning]);

         HelperSetLevelFontStrikeOut(lblErrorText, filter.FilterTraces[(int)LogType.Error]);
         HelperSetLevelFontStrikeOut(lblTraceText, filter.FilterTraces[(int)LogType.Trace]);
         HelperSetLevelFontStrikeOut(lblInfoText, filter.FilterTraces[(int)LogType.Info]);
         HelperSetLevelFontStrikeOut(lblWarningText, filter.FilterTraces[(int)LogType.Warning]);

         lblTrace.Font = new Font(
                      lblTrace.Font.FontFamily,
                      lblTrace.Font.Size,
                      filter.FilterTraces[0] ? System.Drawing.FontStyle.Regular : System.Drawing.FontStyle.Strikeout,
                      System.Drawing.GraphicsUnit.Point,
                      ((byte)(0)));
         ViewCommon.Channel.SendMsg(new UnitMsg(
            opCode: WebFrameWork.CMD_SET_FILTER, 
            name: UniqueId, 
            value: filter)
            );
         
         //Task.Run(() =>
         //{
         //   var statTime = DateTime.Now;
         //   lblFiterBoxTxt.Invoke(new Action(() => { lblFiterBoxTxt.Text = $"{FilterBoxLbl}"; }));

         //   if (filter.IsMasterFilter)
         //   {
         //      //TODo : What view.
         //      //_queueWork.Add(new Work(Work.WType.SetNewView, ViewCommon.Engine.GetMasterFrame()));
         //      FilterMessage = "";
         //   }
         //   else if (filter.IsValidFilter)
         //   {
         //      //TOD : What view
         //      //var view = ViewCommon.Engine.Filter(filter);
         //      //_queueWork.Add(new Work(Work.WType.SetNewView, view));
         //      FilterMessage = "Filter applied";
         //      var totalTime = DateTime.Now - statTime;

         //      lblFiterBoxTxt.Invoke(new Action(() => { lblFiterBoxTxt.Text = $"{FilterBoxLbl} [ {GetSeconds(totalTime)} ]"; }));


         //   }
         //});
      }
      //private string GetSeconds(TimeSpan span)
      //{
      //   return (span.TotalSeconds < 0) ? $"{span.TotalSeconds:#.#}s" : $"{span.TotalSeconds:.###}s";
      //}
      private void btnCloneView_Click(object sender, EventArgs e)
      {
         ViewCommon.StartMain();
      }
      private void btnConfigure_Click(object sender, EventArgs e)
      {
         ViewCommon.ShowPropertyScreen();
      }
      private void DoWork(UnitMsg message)
      {

         //f (command.OpCode != WebFrameWork.CMD_GET_LINES)
         _logger?.Debug($"processing, {message.OpCode}");
            

         switch (message.OpCode)
         {
            case WebFrameWork.CMD_PUT_INFO:               
               var frame = message.GetData<WebFrame>();
               _table.SetViewFrame(frame);
               if (_table.ViewFrame.TotalLogLines > 0)
               {
                  _table.CurrentDataLine = 0;
                  _table.WebLogLinesData = null;
                  ViewCommon.Channel.SendMsg(new UnitMsg(opCode: WebFrameWork.CMD_GET_LINES, name: UniqueId, value: new UnitCmdGetLines() { StartLine = _table.CurrentDataLine, Length = _table.RowsPerPage }));                  
               }
               DoWork_UpdateStatInfo();
               break;
            case WebFrameWork.CMD_PUT_LINES:               
               var val = message.GetData<WebLogLines>();
               //Update the log info viewer if that data was not in the view earlier.
               if (val.LogLines.Count <= _table.RowsPerPage) { 
                  DoWork_Draw2(WebFrameWork.CMD_GET_LINES, val);
               }
               if (_lineInfoDlg.Visible) {
                  _lineInfoDlg.Invoke(new Action(() =>
                  {
                     SetInfoDialogData();
                  }));                
               }
               break;
            case WebFrameWork.CMD_PUT_NEW_VIEW:               
               //Now we need to get the view from the server.
               ViewCommon.Channel.SendMsg(new UnitMsg(opCode: WebFrameWork.CMD_PUT_NEW_VIEW, name: UniqueId));
               ViewCommon.Channel.SendMsg(new UnitMsg(opCode: WebFrameWork.CMD_GET_LINES, name: UniqueId, value: new UnitCmdGetLines() { StartLine = _table.CurrentDataLine, Length = _table.RowsPerPage }));
               break;
            case WebFrameWork.CMD_PUT_ALL_FILTER_UPDATE:               
               OnNewNotepadChange?.Invoke(CntrlTextViewerMsg.SetTitle);
               QueueDrawRequest("on new theme");                           
               break;
            case WebFrameWork.MSG_BST_PROGRESS:
               {
                  var fileAppMemInfo = message.GetData<FileParseProgressEvent>();
                  lblFileSize.Invoke(new Action(() =>
                  {                     
                     if (IsMainWindow)
                     {
                        if(fileAppMemInfo.ParseInProgress && _timerUpdateInfo.Interval != 1000)
                           _timerUpdateInfo.Interval = 1000;

                        if (!fileAppMemInfo.ParseInProgress && _timerUpdateInfo.Interval != 5000)
                           _timerUpdateInfo.Interval = 5000;
                     }

                     if (fileAppMemInfo.ParseInProgress != pnlShowProgress.Visible)
                     {
                        if (!pnlShowProgress.Visible)
                        {
                           prgFileIndexed.Value = 0;
                           prgFileIndexed.Maximum = fileAppMemInfo.TotalFile;
                           prgFileParsed.Value = 0;
                           prgFileParsed.Maximum = fileAppMemInfo.TotalFile;
                           prgOptimizing.Value = 0;
                        }
                        pnlShowProgress.Visible = fileAppMemInfo.ParseInProgress;
                     }

                     if(fileAppMemInfo.ParseInProgress)
                     {
                        prgFileParsed.Value = fileAppMemInfo.FilesParsed;
                        prgFileIndexed.Value = fileAppMemInfo.FilesIndexed;
                        lblParsingUpdate.Text = Utils.Percentage(fileAppMemInfo.FilesParsed, fileAppMemInfo.TotalFile);
                        lblIndexingUpdate.Text = Utils.Percentage(fileAppMemInfo.FilesIndexed, fileAppMemInfo.TotalFile);
                     }


                     lblFileSize.Text = Utils.GetHumanSize(fileAppMemInfo.FilesLoadedSize, showInt:false);
                     lblAppMemSize.Text = Utils.GetHumanSize(fileAppMemInfo.AppSize);
                     lblTotalFiles.Text = $"{fileAppMemInfo.FilesParsed} of {fileAppMemInfo.TotalFile}";
                  }));
                }
               break;
            case WebFrameWork.CMD_PUT_FILTER_DETAILS:
               {
                  CurrentFilter = message.GetData<TermFilter>();
                  btnSavedFilterApply.Invoke(new Action(() => { 
                     HelperSetFilter(CurrentFilter, true);
                  }));
                  break;
               }
            case WebFrameWork.CMD_GET_CONFIG_CURRENT:
               {
                  _userConfig = message.GetData<ConfigUI>();
                  if (IsMainWindow)
                  {
                     ViewCommon.ConfigOfSystem = _userConfig;                     
                  }
                  _drawRequired = true;
                  _table.Dirty = true;
                  QueueDrawRequest("on new theme");
                  break;
               }
            case WebFrameWork.CMD_GET_FILTER_LIST:
               {
                  var filters = message.GetData<string[]>();
                  lstSavedFilter.Invoke(new Action(() => {
                     LoadFilter(filters);
                  }));
                  break;
               }
            /*
             * Donot like the implemenation but here it goes. Once user chooses the lines it is giving to the server as it has reference to all the lines.
             * There it iterates and exportes these lines back to the client. Now client can copy the lines to clipboard why because client is runnign in
             * STA threading (windows form thing !). As of now we cannot put this functionality to a Util classs and export from the server/engine itself.
             * MSG_NAVIGATE_TO_VIEW_LINE sends lines to the server
             * MSG_COPY_TO_CLIPBOARD_UNTIL_FIXED_HTML sends the two HTML and plain text as string array.
             */
            case WebFrameWork.MSG_NAVIGATE_TO_VIEW_LINE:
               {
                  if (message.UniqueId == UniqueId)
                  {
                     var navigateToLine = message.GetData<int>();
                     _table.SetGlobalLine(navigateToLine);
                     QueueDrawRequest("move to global line");
                  }
                  break;
               }
            case WebFrameWork.MSG_COPY_TO_CLIPBOARD_UNTIL_FIXED_HTML:
               {
                  var clipboardStings = message.GetData<string[]>();
                  masterPanel.Invoke(new Action(() => {
                     ExportToClipboard(clipboardStings[0], clipboardStings[1]);
                  }));
                  break;
               }
            default:
               _logger?.Error($"No handling for {message.OpCode}");
               break;
         }
      }
      private void DoWork_UpdateStatInfo()
      {
         _table.SetTotalLines(_table.ViewFrame.TotalLogLines);
         masterPanel.Invoke(new Action(() =>
         {
            lblTrace.Text = _table.ViewFrame.Trace.ToString();
            lblError.Text = _table.ViewFrame.Error.ToString();
            lblWarning.Text = _table.ViewFrame.Warning.ToString();
            lblInfo.Text = _table.ViewFrame.Info.ToString();
            lblTotal.Text = _table.ViewFrame.TotalLogLines.ToString();
            lblHiddenTotal.Text = _table.ViewFrame.TotalLogLines != _table.ViewFrame.SystemTotalLogLine ?
               (_table.ViewFrame.SystemTotalLogLine - _table.TotalDataLines).ToString() :
               "";
            lblTotalFiles.Text = _table.ViewFrame.TotalLogFiles.ToString();
            HelperSetLevelFontStrikeOut(lblHiddenTotalTxt, lblHiddenTotal.Text != "");
         }));

      }
      private void DoWork_Draw2(string msg, WebLogLines data = null)
      {
         try
         {
            _lockDraw.Wait();
            if (data != null)
            {
               _table.WebLogLinesData = data;
               //_logger?.Info($"Draw {msg}, adding new data, {data.StartLogLine} - {data.LogLines.Count + data.StartLogLine}, {data.LogLines.Count}");
            }
            _drawRequired = true;
         }
         finally {
            _lockDraw.Release();
         }
      }      
      //TODO : rename to draw.
      private void DoWork_Draw3()
      {
         
         try
         {
            //var startTime = DateTime.Now;
            _lockDraw.Wait();
            _drawRequired = _drawRequired || _table.Dirty;
            if (
               !IsHandleCreated||
               !_drawRequired ||
               _table.WebLogLinesData == null ||
               !(_table.CurrentDataLine == _table.WebLogLinesData.StartLogLine))
               return;
            _table.Dirty = false;
            _newCanvas.gc.Clear(_userConfig.BackgroundColor);


            Draw_Step1_TableHeader();
            Draw_Step2_DataLines();

            if (_table.TotalDataLines != 0 && _table.TotalDataLines >= _table.RowsPerPage)
            {
               Draw_Step3_Scrollbar(_table);
            }


            if (!masterPanel.InvokeRequired)
            {
               //masterPanel.BackgroundImage = _newCanvas.bmp;
               lblPage.Text = _table.TotalPages == 0 ? "" : $"{_table.CurrentPage + 1} of {_table.TotalPages}";
               if (lblPage.Text.Length > 9)
               {
                  lblPage.Text = $"{_table.CurrentPage + 1} of {_table.TotalPages}";
               }
               masterPanel.CreateGraphics().DrawImage(_newCanvas.bmp, 0, 0);
            }

            //_logger?.Debug($"Draw took {(DateTime.Now - startTime).TotalMilliseconds}");


         }
         catch (Exception ex)
         {
            _logger?.Error("Error in draw, " + ex.ToString());
         }
         finally
         {
            _drawRequired = false;
            //_logger?.Info($"Draw {msg}, complete");
            _lockDraw.Release();
         }
         
      }
      private void Draw_Step1_TableHeader()
      {
         if (!_userConfig.ShowTableHeader)
         {
            return;
         }
         string addTextToMsg = "";
         for (int ndx = 0; ndx < _table.TotalColumns; ndx++)
         {
            if (string.IsNullOrEmpty(_table[ndx].DisplayName) || !_table[ndx].Visible)
               continue;
            switch (_table[ndx].DisplayName)
            {
               default:
                  _newCanvas.gc.DrawString(
                      _table[ndx].DisplayName,
                      _userConfig.TableHeaderFont,
                      new SolidBrush(_userConfig.TableHeaderForeGroundColor),
                      new PointF(
                          _table.Location.X + _table[ndx].StartLoc,
                          _table.Location.Y + _table.OffsetStringData)
                      );
                  break;

               case "Message":
                  _newCanvas.gc.DrawString(
                      $"{_table[ndx].DisplayName}{addTextToMsg}",
                      _userConfig.TableHeaderFont,
                      new SolidBrush(_userConfig.TableHeaderForeGroundColor),
                      new PointF(
                          _table.Location.X + _table[ndx].StartLoc,
                          _table.Location.Y + _table.OffsetStringData)
                      );
                  break;
            }
         }

      }
      private void Draw_Step2_DataLines()
      {
         if (_table.WebLogLinesData == null || _table.WebLogLinesData.LogLines.Count == 0)
         {
            return;
         }
         //_logger?.Info(
         //   $"Draw data {_table.CurrentDataLine == _table.WebLogLinesData.StartLogLine}[{_table.CurrentDataLine}, {_table.WebLogLinesData.StartLogLine}]"+
         //   $"{_table.WebLogLinesData.LogLines.Count == _table.RowsPerPage}[{_table.WebLogLinesData.LogLines.Count}, {_table.RowsPerPage}]");
         var brushes = new SolidBrush[] {
                        new SolidBrush(_userConfig.MsgErrorFontColor),
                        new SolidBrush(_userConfig.MsgInfoFontColor),
                        new SolidBrush(_userConfig.MsgTraceFontColor),
                        new SolidBrush(_userConfig.MsgWarningFontColor)
                    };
         var brushes_blur = new SolidBrush[] {
                        new SolidBrush(_userConfig.MsgErrorFontColor),
                        new SolidBrush(_userConfig.MsgInfoFontColor),
                        new SolidBrush(_userConfig.MsgTraceFontColor),
                        new SolidBrush(_userConfig.MsgWarningFontColor)
                    };
         for (int ndx = 0; ndx < brushes.Length; ndx++)
         {
            var color = brushes_blur[ndx].Color;
            brushes_blur[ndx].Color = Color.FromArgb(color.A / 2, color.R, color.G, color.B);
         }

         var currentLine = _table.CurrentDataLine;
         string prevTime = "";

         //Draw the gobal line indicator first.
         if (_table.ShowGlobalLine)
         {
            var line = Math.Abs(_table.CurrentGlobalLine);
            if (line >= _table.CurrentDataLine && line <= (_table.CurrentDataLine + _table.RowsPerPage))
            {
               if (_table.CurrentGlobalLine < 0)
               {
                  float rowYPos = (_table.RowsHeight * (((line - 1) - (_table.CurrentDataLine)) + 1)) + (_table.Location.Y + _table.OffsetStringData);
                  _newCanvas.gc.DrawLine(
                      new Pen(_userConfig.GlobalLineSelected, _userConfig.GlobalLineSelectedWidth),
                      0,
                      rowYPos,
                      _table.Size.Width,
                      rowYPos);
               }
               else
               {
                  float rowYPos = (_table.RowsHeight * ((line - _table.CurrentDataLine) + 1)) + (_table.Location.Y + _table.OffsetStringData);
                  _newCanvas.gc.FillRectangle(
                      new SolidBrush(_userConfig.GlobalLineSelected),
                      0,
                      rowYPos,
                      _table.Size.Width,
                      _table.RowsHeight);
               }
            }
         }

         var brushLineBkg = new SolidBrush(_userConfig.TableLineSelectedBkg);
         var brushMsgBkg = new SolidBrush(_userConfig.TableSearchLinesBkg);
         for (int ndx = 0; ndx < _table.RowsPerPage; ndx++)
         {
            if (currentLine < _table.TotalDataLines)
            {
               var (isRowSelected, rowData) = _table.GetLine(currentLine);
               currentLine++;

               //float rowYPos = (_table.RowsHeight * (ndx + 1)) + (_table.Location.Y + _table.OffsetStringData);
               float rowYPos = _table.GetYPos(ndx);

               //TODO : future
               //if (currentLine > 10 && currentLine < 20)
               //{
               //   DrawMsgBackground(rowYPos, brushMsgBkg);
               //}

               if (isRowSelected && !_table.ShowGlobalLine)
               {
                  DrawLineBackground(rowYPos, brushLineBkg);
               }

               foreach (var col in _table.Columns)
               {
                  if (!col.Visible) continue;
                  switch (col.Name)
                  {
                     case "LineNo":
                        _newCanvas.gc.DrawString(
                            $"{currentLine}",
                            _userConfig.TableBodyFont,
                            new SolidBrush(_userConfig.MsgTraceFontColor),
                            _table.Location.X + col.StartLoc,
                            rowYPos);
                        break;

                     case "TimeStamp":

                        string newTime = _table.ShowTimeSelected == TableSkeleton.ShowTime.Default ?
                           $"{rowData.TimeStamp.ToString(_userConfig.Column1TimeStampFormat)}" :
                           $"{(rowData.TimeStamp - _table.ShowTime_Default_StartTime)}"
                           ;
                        if (prevTime != newTime)
                        {
                           prevTime = newTime;
                           _newCanvas.gc.DrawString(
                               newTime,
                               _userConfig.TableBodyFont,
                              new SolidBrush(_userConfig.MsgTraceFontColor),
                               _table.Location.X + col.StartLoc,
                               rowYPos
                               );
                        }
                        break;
                     case "ThId":
                        _newCanvas.gc.DrawString(
                            $"{rowData.ThreadNo}",
                            _userConfig.TableBodyFont,
                            new SolidBrush(_userConfig.MsgTraceFontColor),
                            _table.Location.X + col.StartLoc,
                            rowYPos);
                        break;
                     case "Category":
                        _newCanvas.gc.DrawString(
                             $"{(rowData.Category.Length <= 10 ? rowData.Category : rowData.Category.Substring(0, 10) + "~")}",
                            _userConfig.TableBodyFont,
                            new SolidBrush(_userConfig.MsgTraceFontColor),
                            _table.Location.X + col.StartLoc,
                            rowYPos);
                        break;
                     case "ProcId":
                        _newCanvas.gc.DrawString(
                            $"{rowData.ProcessId}",
                            _userConfig.TableBodyFont,
                            new SolidBrush(_userConfig.MsgTraceFontColor),
                            _table.Location.X + col.StartLoc,
                            rowYPos);
                        break;

                     case "ImageNo":
                        if (rowData.StackTraceId != LogLine.INVALID)
                        {
                           _newCanvas.gc.DrawImage(Properties.Resources.stack,
                              _table.Location.X + col.StartLoc,
                              rowYPos + 2);
                        }
                        break;

                     case "Message":
                        var lineData = rowData.Msg.Replace("\n", "").Replace("\r", "").TrimStart();
                        if (_userConfig.FeatureShowCategoryWithMsg)
                        {
                           lineData = $"[{rowData.Category}], {lineData}";
                        }
                        _newCanvas.gc.DrawString(

                            _userConfig.DebugUI ?
                            $"{rowData.GlobalLine}| {rowData.FileNumber}| {rowData.LineNumber}| {lineData}" :
                            $"{lineData}",

                            _userConfig.TableBodyFont,
                            brushes[(int)rowData.LogType],
                            _table.Location.X + col.StartLoc,
                            rowYPos);
                        break;
                  }
               }
            }
            else
            {
               break;
            }
         }

      }
     
      private void Draw_Step3_Scrollbar(TableSkeleton table)
      {
         var color = _userConfig.ScrollBarColor;//TODO, color of the scroll bar should be configurable.

         _newCanvas.gc.DrawLine(new Pen(color, 1),
             table.ScrollBarStartLoc,
             table.ScrollBarStopLoc);

         table.CirclePoint = GetRectF(
             table.ScrollBarStartLoc.X,
             table.ScrollBarStartLoc.Y + (float)(table.CurrentPage / table.ScrollPagesPerPixel),
             table.ScrollBarCircleRadius);

         _newCanvas.gc.FillEllipse(
             new SolidBrush(color),
             table.CirclePoint);
      }

      private void AddMenuItem()
      {
         copyToNotepadToolStripMenuItem.DropDownItems.Clear();
         var names = ViewCommon.GetAllNotepadNames();
         names.Add("New Notepad Window");
         foreach (var name in names)
         {
            ToolStripMenuItem item = new ToolStripMenuItem();
            item.Text = name;
            item.Click += new EventHandler(newNotepadToolStripMenuItem_Click);
            copyToNotepadToolStripMenuItem.DropDownItems.Add(item);
         }
      }
      private void QueueDrawRequest(string msg)
      {
         try
         {
            _lockDraw.Wait();
            if (_newCanvas.SetSize(masterPanel.Size))
            {
               _table.Resize(_newCanvas.bmp.Size, ViewCommon.ConfigOfSystem);               
            }
            //_logger?.Debug($"QueueDrawRequest: {msg}");
            ViewCommon.Channel.SendMsg(new UnitMsg(opCode: WebFrameWork.CMD_GET_LINES, name: UniqueId, value: new UnitCmdGetLines() { StartLine = _table.CurrentDataLine, Length = _table.RowsPerPage }));
         }
         finally {
            _lockDraw.Release();
         }
      }     
      private void DrawLineBackground(float yPos, SolidBrush brush)
      {
         _newCanvas.gc.FillRectangle(brush, 2, yPos, _table.Size.Width, _table.RowsHeight);
      }
      private void filterByThreadId_Click(object sender, EventArgs e)
      {
         HelperSetFilter(CurrentFilter);
         txtTagsInclude.Text = $"T:{filterByThreadId.Text.Replace("ThreadId- ", "")};";
         btnFllter_Click(null, null);
      }
      private void filterByCategoryId_Click(object sender, EventArgs e)
      {
         txtTagsInclude.Text = $"C:{filterByCategoryId.Text.Replace("Category- ", "")}";
         btnFllter_Click(null, null);
      }
      private void filterByProcId_Click(object sender, EventArgs e)
      {
         CurrentFilter.TagsInclude = new string[] { $"P:{filterByProcId.Text.Replace("ProcessId- ", "")};" };
         HelperSetFilter(CurrentFilter);
      }
      private void btnFllter_Click(object sender, EventArgs e)
      {
         CurrentFilter.Enabled = true;
         CurrentFilter.MatchCase = false;
         CurrentFilter.MatchExact = false;
         CurrentFilter.MsgInclude = txtInclude.Text.Split(new string[] { ";" }, StringSplitOptions.RemoveEmptyEntries);
         CurrentFilter.MsgExclude = txtExclude.Text.Split(new string[] { ";" }, StringSplitOptions.RemoveEmptyEntries);
         CurrentFilter.TagsInclude = txtTagsInclude.Text.Split(new string[] { ";" }, StringSplitOptions.RemoveEmptyEntries);
         CurrentFilter.TagsExclude = txtTagsExclude.Text.Split(new string[] { ";" }, StringSplitOptions.RemoveEmptyEntries);

         HelperSetFilter(CurrentFilter);
      }
      private void handle_enter_key(object sender, KeyPressEventArgs e)
      {
         if (e.KeyChar == (char)Keys.Return)
         {
            btnFllter_Click(null, null);
            //Suppresses the ding sound when we press enter.
            e.Handled = true;
         }
      }
      private void lblCloseFilterBox_Click(object sender, EventArgs e)
      {
         togge_filter_screen();
      }
      private void lblTotalFiles_Click(object sender, EventArgs e)
      {
         var fld = new frmFileManager();
         fld.InitialSelectedFileNo = new int[] { 1 };
         fld.ShowDialog();
      }
      private void btnClear_Click(object sender, EventArgs e)
      {
         //TODO: simple reset all filters.
         if (
            _table.ViewFrame== null ||
            _table.ViewFrame.TotalLogFiles == 0 
            )
         {
            return;
         }

         if (MessageBox.Show(this, "There are logs in viewer. Please confirm to clear ?", "Alert", MessageBoxButtons.OKCancel) == DialogResult.OK)
         { 
            ViewCommon.Channel.SendMsg(new UnitMsg(opCode: WebFrameWork.CMD_PUT_CLEAR, name: UniqueId, value: null));
         }

      }
      private void lblClearFilter_Click(object sender, EventArgs e)
      {
         //_queueWork.Add(new Work(Work.WType.SetNewView, ViewCommon.Engine.GetMasterFrame()));
         ResetFilter();
      }
      private void btnSavedFilterAdd_Click(object sender, EventArgs e)
      {         
         ViewCommon.AddFilter(UniqueId, txtSavedFilter.Text, CurrentFilter);
      }
      private void btnSavedFilterApply_Click(object sender, EventArgs e)
      {
         ViewCommon.Channel.SendMsg(new UnitMsg(opCode: WebFrameWork.CMD_GET_FILTER_DETAILS, name: UniqueId, value: new TermFilter() { Name= txtSavedFilter.Text }));
         //TODO : fix this.
         //CurrentFilter = ViewCommon.GetFilter(txtSavedFilter.Text);
         //HelperSetFilter(CurrentFilter, true);
      }
      private void btnSavedFilterDelete_Click(object sender, EventArgs e)
      {
         ViewCommon.Channel.SendMsg(new UnitMsg(opCode: WebFrameWork.CMD_DEL_FILTER_DETAIL, name: UniqueId, value: new TermFilter() { Name = txtSavedFilter.Text }));
      }
      private void lstSavedFilter_SelectedIndexChanged(object sender, EventArgs e)
      {

         btnSavedFilterDelete.Enabled = false;
         btnSavedFilterApply.Enabled = false;

         if (lstSavedFilter.SelectedIndex != -1)
         {
            txtSavedFilter.Text = lstSavedFilter.Items[lstSavedFilter.SelectedIndex].ToString();
            btnSavedFilterDelete.Enabled = true;
            btnSavedFilterApply.Enabled = true;
         }
      }

      private void pnlFilterSearch_Move(object sender, EventArgs e)
      {
         pnlFilterTag.Location = new Point(pnlFilterSearch.Location.X, pnlFilterSearch.Location.Y + pnlFilterSearch.Size.Height + 2);
      }

      private void LoadFilter(string [] filters)
      {
         lstSavedFilter.ClearSelected();
         lstSavedFilter.Items.Clear();         
         lstSavedFilter.Items.AddRange(filters);
         txtSavedFilter.Text = "";

         btnSavedFilterAdd.Enabled = true;
         btnSavedFilterDelete.Enabled = false;
         btnSavedFilterApply.Enabled = false;
      }

      #region ShowProgress functionality

      #endregion

   }

   public class CustomMoveCntrl
   {
      private Point lblFilterBoxStart;
      private Control _cntrl1;
      private Control _cntrl2;
      public CustomMoveCntrl(Control cntrl1, Control cntrl2 = null)
      {
         _cntrl1 = cntrl1;
         _cntrl2 = cntrl2 ?? cntrl1;
         _cntrl1.MouseDown += new MouseEventHandler(lblFiterBoxTxt_MouseDown);
      }
      public void Release()
      {
         _cntrl1.MouseDown -= new MouseEventHandler(lblFiterBoxTxt_MouseDown);
      }
      private void lblFiterBoxTxt_MouseDown(object sender, MouseEventArgs e)
      {
         if (e.Button == MouseButtons.Left)
         {
            lblFilterBoxStart = e.Location;
            _cntrl1.MouseUp += new MouseEventHandler(lblFilterBoxtTxt_MouseUp);
            _cntrl1.MouseMove += new MouseEventHandler(lblFilterBoxtTxt_MouseDown);
            Cursor.Current = System.Windows.Forms.Cursors.SizeAll;
         }
      }
      private void lblFilterBoxtTxt_MouseUp(object sender, MouseEventArgs e)
      {
         _cntrl1.MouseMove -= new MouseEventHandler(lblFilterBoxtTxt_MouseDown);
         _cntrl1.MouseUp -= new MouseEventHandler(lblFilterBoxtTxt_MouseUp);
         Cursor.Current = System.Windows.Forms.Cursors.Default;
      }
      private void lblFilterBoxtTxt_MouseDown(object sender, MouseEventArgs e)
      {
         _cntrl2.Location = new Point(_cntrl2.Location.X - (lblFilterBoxStart.X - e.X), _cntrl2.Location.Y - (lblFilterBoxStart.Y - e.Y));
      }
   }
}
