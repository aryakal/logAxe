//=====================================================================================================================
// Source : https://github.com/aryakal/
//--------------------------------------------------------------------------------------------------------------------
// Subsystem (for future me)
// MasterThread   | DoWork      | Everyting starts here. This long running task is executing every command.
// Draw           | DoWork_Draw | All drawing function starts here.
// 
//=====================================================================================================================

using System;
using System.Drawing;
using System.Windows.Forms;
using System.Threading;
using System.Threading.Tasks;
using System.Collections.Concurrent;
using logAxeCommon;
using logAxeEngine.Common;
using logAxeEngine.Interfaces;
using logAxeEngine.EventMessages;
using System.Text;

namespace logAxe
{
   public partial class CntrlTextViewer : UserControl, IMessageReceiver
   {
      /// <summary>
      /// Internal class where the work is defined.
      /// </summary>
      internal class Work
      {
         public Work(WType type, object work)
         {
            Type = type;
            RealWork = work;
         }

         public enum WType
         {
            Message,
            Draw,
            GetNewView,
            SetNewView,
         }

         public WType Type { get; }
         public object RealWork { get; set; }
      }
      internal class MouseState
      {
         public bool IsMouseDown { get; set; }
         public int X { get; set; }
         public int Y { get; set; }
      }
      private Task _backgroundWork;
      private CancellationTokenSource _cancelToken;
      private BlockingCollection<Work> _queueWork = new BlockingCollection<Work>();
      private UserConfig _userConfig;
      private DrawSurface _newCanvas = new DrawSurface();
      private LogFrame _view;
      private TableSkeleton _table;
      private TermFilter CurrentFilter = new TermFilter();
      private MessageExchangeHelper _msgHelper;
      private MouseState _mouse = new MouseState();
      private long _totalDraws = 0;
      private frmLineData _lineInfoDlg = new frmLineData();
      private NamedLogger _logger;
      private frmFileManager _fld;
      public string UniqueId { get; }
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
      public Action<ILogAxeMessage> OnNewNotepadChange { get; set; }
      public string _filterMessage = "";
      public string FilterMessage
      {
         get
         {
            return _filterMessage;
         }
         private set
         {
            _filterMessage = value;
            OnNewNotepadChange?.Invoke(new LogAxeGenericMessage() { MessageType = LogAxeMessageEnum.NewMainFrmAddRemoved });
         }
      }
      public CntrlTextViewer()
      {
         UniqueId = $"View-{DateTime.Now:HHmmssfff}";
         _logger = new NamedLogger(UniqueId);
         InitializeComponent();

         // prevent issue using development.
         if (ViewCommon.Engine != null)
         {
            new HelperAttachFileDrop(this);
            new HelperAttachFileDrop(masterPanel);

            _userConfig = ViewCommon.UserConfig;

            RegisterControl(masterPanel);

            _msgHelper = new MessageExchangeHelper(ViewCommon.MessageBroker, this);
            _table = new TableSkeleton(masterPanel.Location, masterPanel.Size, _msgHelper);

            _queueWork.Add(new Work(Work.WType.SetNewView, LogFrame.GetEmptyView()));

            _cancelToken = new CancellationTokenSource();
            _backgroundWork = Task.Factory.StartNew(() =>
            {
               DoWork();
            }, TaskCreationOptions.LongRunning);

            QueueDrawRequest("init");
            SetForeColor();
            UpdateTotal(ViewCommon.Engine.GetStartInfo());
            AddMenuItem();
         }
         else
         {
            _table = new TableSkeleton(masterPanel.Location, masterPanel.Size, null);
         }

         _lineInfoDlg.MoveLine += MoveLine;

         LoadFilter();

      }
      public void SetMasterView()
      {
         if (ViewCommon.Engine != null)
         {
            _queueWork.Add(new Work(Work.WType.SetNewView, ViewCommon.Engine.GetMasterFrame()));
         }
      }

      public readonly string FilterBoxLbl = "Filter log lines ( + Include - Exclude)";

      public void FillCircle(Brush brush, float radius, PointF point)
      {
         _newCanvas.gc.FillEllipse(brush,
             point.X - radius,
             point.Y - radius,
             radius * 2,
             radius * 2);
      }
      public void DrawCircle(Brush brush, float radius, PointF point)
      {
         _newCanvas.gc.FillEllipse(brush,
             point.X - radius,
             point.Y - radius,
             radius * 2,
             radius * 2);
      }
      public RectangleF GetRectF(float x, float y, float size)
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
               if (!_isNotepad)
                  _msgHelper.PostMessage(new LogAxeGenericMessage() { MessageType = LogAxeMessageEnum.AwakeAllWindows });
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
         _msgHelper?.Unregister();
      }

      private void MoveLine(int delta)
      {
         if (_table.MoveByLineDelta(delta))
         {
            QueueDrawRequest("Ctrl_MouseUp");
         }
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
         QueueDrawRequest("Ctrl_MouseUp");
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
               if (_table.SetCurrentSelection(e.Y,
                   Control.ModifierKeys
                   ))
               {
                  QueueDrawRequest("Ctrl_MouseUp");
               }

            }
         }

      }
      private void ScrollMove(int x, int y)
      {
         _table.ChangeScrollMove(y);
         QueueDrawRequest("ScrollMove");
      }
      private void Ctrl_Resize(object sender, EventArgs e)
      {
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
         _showPnlFilterTag = !_showPnlFilterTag;
         pnlFilterTag.Visible = _showPnlFilterTag;
      }
      private void masterPanel_Paint(object sender, PaintEventArgs e)
      {
         QueueDrawRequest("masterPanel_Paint");
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
         filterByToolStripMenuItem.Enabled = (isLineSlected && !_isNotepad);
         
         if (filterByToolStripMenuItem.Enabled)
         {
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
         DoWork_RequestDrawWithFlag("showLineNoToolStripMenuItem");
      }
      private void showLineNoToolStripMenuItem_Click(object sender, EventArgs e)
      {
         _table["LineNo"].Visible = !showLineNoToolStripMenuItem.Checked;
         DoWork_RequestDrawWithFlag("showLineNoToolStripMenuItem");
      }
      private void showProcIdToolStripMenuItem_Click(object sender, EventArgs e)
      {
         _table["ProcId"].Visible = !showProcIdToolStripMenuItem.Checked;
         DoWork_RequestDrawWithFlag("showLineNoToolStripMenuItem");
      }
      private void showThreadIdToolStripMenuItem_Click(object sender, EventArgs e)
      {
         _table["ThId"].Visible = !showThreadIdToolStripMenuItem.Checked;
         DoWork_RequestDrawWithFlag("showLineNoToolStripMenuItem");
      }
      private void showCategoryToolStripMenuItem_Click(object sender, EventArgs e)
      {
         _table["Category"].Visible = !showCategoryToolStripMenuItem.Checked;
         DoWork_RequestDrawWithFlag("showLineNoToolStripMenuItem");
      }
      

      private void ShowFileManager(int [] selectedFileIndex)
      {
         if (null == _fld)
         {
            _fld = new frmFileManager();
            _fld.InitialSelectedFileNo = selectedFileIndex;
         }
         _fld.ShowDialog();
      }
      private void exportFileToolStripMenuItem_Click(object sender, EventArgs e)
      {  
         ShowFileManager(_table.GetFileIndexForSeletedLines());
      }
      private void newNotepadToolStripMenuItem_Click(object sender, EventArgs e)
      {
         var notepadName = ((ToolStripMenuItem)sender).Text;
         if (notepadName == "New Notepad Window")
         {
            notepadName = ViewCommon.OpenNewNotepad();
         }

         _msgHelper.PostMessage(
            new AddLineToNotepadEvent()
            {
               NotebookName = notepadName,
               GlobalLine = _table.GetGlobalLineIndexForSeletedLines()
            });

         AddMenuItem();
      }
      private void showDefaultTimeToolStripMenuItem_Click(object sender, EventArgs e)
      {
         _table.ShowTimeSelected = TableSkeleton.ShowTime.Default;
         DoWork_RequestDrawWithFlag("showDefaultTimeToolStripMenuItem");
      }
      private void setStartTimeToolStripMenuItem_Click(object sender, EventArgs e)
      {
         var (_, logLine) = _table.GetSelectedLine();
         _table.ShowTimeSelected = TableSkeleton.ShowTime.StartTime;
         _table.ShowTime_Default_StartTime = logLine.TimeStamp;
         DoWork_RequestDrawWithFlag("setStartTimeToolStripMenuItem");
      }
      #endregion

      private void SetForeColor()
      {
         lblTrace.ForeColor = _userConfig.MsgTraceFontColor;
         lblError.ForeColor = _userConfig.MsgErrorFontColor;
         lblWarning.ForeColor = _userConfig.MsgWarningFontColor;
         lblInfo.ForeColor = _userConfig.MsgInfoFontColor;
      }
      public void GetMessage(logAxeEngine.Interfaces.ILogAxeMessage message)
      {
         _queueWork.Add(new Work(Work.WType.Message, message)); ;
      }
      private void SetView(LogFrame frame)
      {
         _table.SetViewFrame(frame);
         _view = frame;
         _table.SetTotalLines(_view.TotalLogLines);
         _table.CurrentDataLine = 0;
         _table.Dirty = true;
         masterPanel.Invoke(new Action(() =>
         {
            lblTrace.Text = _view.LogTypeLength(LogType.Trace).ToString();
            lblError.Text = _view.LogTypeLength(LogType.Error).ToString();
            lblWarning.Text = _view.LogTypeLength(LogType.Warning).ToString();
            lblInfo.Text = _view.LogTypeLength(LogType.Info).ToString();
            lblTotal.Text = _table.TotalDataLines.ToString();
            lblHiddenTotal.Text = _view.TotalLogLines != _view.SystemTotalLogLine ?
               (_view.SystemTotalLogLine - _table.TotalDataLines).ToString() :
               "";

            lblPage.Text = _table.TotalPages.ToString();
            HelperSetLevelFontStrikeOut(lblHiddenTotalTxt, lblHiddenTotal.Text != "");

         }));
         QueueDrawRequest("SetView");
      }
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

      private void CopyLinesToClipBoard()
      {
         try
         {
            var lst = _table.GetAllSelectedLogLines();
            
            if (lst.Count == 0)
               return;

            var selectedPLines = new StringBuilder();
            var selectedHLines = new StringBuilder();            

            foreach (var line in lst)
            {
               var logType = line.LogType.ToString().Substring(0, 1);
               var timeStamp = line.TimeStamp.ToString(ViewCommon.DefaultDateTimeFmt);
               var logText = line.Msg.Length > 120 ? line.Msg.Substring(0, 120).Replace("\n", "") + "..." : line.Msg.Replace("\n", "");
               var lineColor = "red";

               switch (line.LogType)
               {
                  case LogType.Info:
                     lineColor = "green";
                     break;
                  case LogType.Trace:
                     lineColor = "black";
                     break;
                  case LogType.Warning:
                     lineColor = "orange";
                     break;

               }
               selectedPLines.Append($"{logType}, {timeStamp}, {logText}{Environment.NewLine}");
               selectedHLines.Append($"{logType}, {timeStamp}, <span style=\"color: {lineColor}\">{logText}</span><br>");


            }
            var clipText = new DataObject();
            clipText.SetData(DataFormats.Html, GetMSFormattedHtml(selectedHLines.ToString()));
            clipText.SetData(DataFormats.Text, selectedPLines.ToString());
            clipText.SetData(DataFormats.UnicodeText, selectedPLines.ToString());
            Clipboard.SetDataObject(clipText, true);            
         }         
         catch (Exception ex)
         {
            MessageBox.Show(ex.ToString(), "Error in copy to clipboard", MessageBoxButtons.OK);
         }
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
         ShowFileManager(new int[] { 1 });
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
         HelperSetFilter(new TermFilter());
      }
      private void SetLogTypeFilter(LogType logType)
      {
         //No filtering required when there is any log file in system.
         if (_view.SystemTotalLogLine == 0)
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
      private void HelperSetFilter(TermFilter filter, bool assignToText=false)
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

         Task.Run(() =>
         {
            var statTime = DateTime.Now;
            lblFiterBoxTxt.Invoke(new Action(() => { lblFiterBoxTxt.Text = $"{FilterBoxLbl}"; }));            

            if (filter.IsMasterFilter)
            {
               _queueWork.Add(new Work(Work.WType.SetNewView, ViewCommon.Engine.GetMasterFrame()));
               FilterMessage = "";
            }
            else if (filter.IsValidFilter)
            {
               var view = ViewCommon.Engine.Filter(filter);
               _queueWork.Add(new Work(Work.WType.SetNewView, view));
               FilterMessage = "Filter applied";
               var totalTime = DateTime.Now - statTime;

               lblFiterBoxTxt.Invoke(new Action(() => { lblFiterBoxTxt.Text = $"{FilterBoxLbl} [ {GetSeconds(totalTime)} ]"; }));


            }
         });
      }

      private string GetSeconds(TimeSpan span)
      {
         return (span.TotalSeconds < 0) ? $"{span.TotalSeconds:#.#}s" : $"{span.TotalSeconds:.###}s";
      }


      private void btnCloneView_Click(object sender, EventArgs e)
      {
         ViewCommon.StartMain();
      }
      private void btnConfigure_Click(object sender, EventArgs e)
      {
         ViewCommon.ShowPropertyScreen();
      }
      private void DoWork()
      {
         _table.AddColumn(new TableColumn("LineNo"));
         _table.AddColumn(new TableColumn("ProcId") { Width = 20, Visible = false });
         _table.AddColumn(new TableColumn("ThId") { Width = 20, Visible = false });
         _table.AddColumn(new TableColumn("Category") { Width = 100, Visible = false });
         _table.AddColumn(new TableColumn("TimeStamp"));
         _table.AddColumn(new TableColumn("ImageNo") { Width = 20, DisplayName = "" });
         _table.AddColumn(new TableColumn("Message"));

         _table["LineNo"].Visible = _userConfig.ShowLineNo;

         while (!_cancelToken.IsCancellationRequested)
         {
            if (!IsHandleCreated)
            {
               Thread.Sleep(100);
               continue;
            }

            var work = _queueWork.Take(_cancelToken.Token);

            switch (work.Type)
            {
               case Work.WType.Message:
                  DoWork_Message((ILogAxeMessage)work.RealWork);
                  break;

               case Work.WType.SetNewView:
                  SetView((LogFrame)work.RealWork);
                  break;

               case Work.WType.Draw:
                  NamedLogger.PublishLogs = true;
                  Interlocked.Decrement(ref _totalDraws);
                  if (Interlocked.Read(ref _totalDraws) > 2)
                  {
                     continue;
                  }
                  //var stp = new Stopwatch(); stp.Start();
                  DoWork_Draw((string)work.RealWork);
                  //stp.Stop();
                  //Debug.WriteLine($" {stp.Elapsed}");
                  break;

            }
         }
      }//Dowork
      private void DoWork_Message(ILogAxeMessage message)
      {
         try
         {
            switch (message.MessageType)
            {
               case LogAxeMessageEnum.NewViewAnnouncement:
                  if (!_isNotepad)
                     SetView(ViewCommon.Engine.GetMasterFrame());
                  break;
               case LogAxeMessageEnum.FileParseProgress:
                  var fpPrg = (FileParseProgressEvent)message;
                  pnlShowProgress.Invoke(new Action(() =>
                      {
                         pnlShowProgress.Visible = !fpPrg.ParseComplete;

                         prgFileParsed.Maximum = (int)fpPrg.TotalFileCount;
                         prgFileParsed.Value = (int)fpPrg.TotalFileParsedCount;

                         prgFileLoad.Maximum = (int)fpPrg.TotalFileCount;
                         prgFileLoad.Value = (int)fpPrg.TotalFileLoadedCount;

                         lblParsingUpdate.Text = Utils.Percentage(fpPrg.TotalFileLoadedCount, fpPrg.TotalFileCount);
                         lblIndexingUpdate.Text = Utils.Percentage(fpPrg.TotalFileParsedCount, fpPrg.TotalFileCount);

                         UpdateTotal(fpPrg);

                      }));
                  if (fpPrg.ParseComplete)
                  {
                     if (null != _fld)
                     {
                        _fld.Invoke(new Action(() =>
                        {
                           _fld.RefreshList();
                        }));
                     }
                  }
                  break;

               case LogAxeMessageEnum.FileParseEnd:
                  SetView(ViewCommon.Engine.GetMasterFrame());
                  if (null != _fld)
                  {
                     _fld.Invoke(new Action(() =>
                     {
                        _fld.RefreshList();
                     }));
                  }
                  break;

               case LogAxeMessageEnum.NewUserConfiguration:
                  _userConfig = ViewCommon.UserConfig;
                  masterPanel.Invoke(new Action(() =>
                  {
                     SetForeColor();
                  }));
                  _table.Dirty = true;
                  QueueDrawRequest($"LogAxeMessageEnum.NewUserConfiguration");
                  break;
               case LogAxeMessageEnum.BroadCastGlobalLine:
                  {
                     // We got the global line now lets search in the local view.
                     // Either we will have an empty view (because this view does not have any line
                     if (!_view.IsEmpty)
                     {
                        var lineNo = _view.GetGlobalLine(((CurrentGlobalLine)message).GlobalLine);
                        _table.SetGlobalLine(lineNo);
                        QueueDrawRequest($"MessageType.BroadCastGlobalLine");
                     }
                  }
                  break;
               case LogAxeMessageEnum.NotepadAddedOrRemoved:
                  masterPanel.Invoke(new Action(() =>
                  {
                     AddMenuItem();
                  }));
                  break;
               case LogAxeMessageEnum.AddLineToNotepadEvent:                  
                  if (_isNotepad)
                  {
                     var notepadEvent = (AddLineToNotepadEvent)message;
                     if (notepadEvent.NotebookName == NotepadName)
                     {  
                        _view.AddGlobalLines(notepadEvent.GlobalLine);
                        SetView(_view);
                        QueueDrawRequest($"LogAxeMessageEnum.AddLineToNotepadEvent");
                     }

                  }
                  break;

               case LogAxeMessageEnum.NewMainFrmAddRemoved:
               case LogAxeMessageEnum.AwakeAllWindows:
                  OnNewNotepadChange?.Invoke(message);
                  break;

               case LogAxeMessageEnum.FilterChanged:
                  lstSavedFilter.Invoke(
                     new Action(() => {
                        LoadFilter();
                     }
                     ));
                  
                  break;
            }
         }
         catch (Exception ex)
         {
            _logger.Error(ex.ToString());
         }
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
      private void UpdateTotal(FileParseProgressEvent fpPrg)
      {
         lblAppMemSize.Text = Utils.GetHumanSize(System.Diagnostics.Process.GetCurrentProcess().PrivateMemorySize64, true);
         lblTotalFiles.Text = $"{fpPrg.TotalFileLoadedCount} of {fpPrg.TotalFileCount}";
         lblFileSize.Text = $"{Utils.GetHumanSize(fpPrg.TotalFileSizeLoaded)}";
      }
      private void DoWork_DrawScrollBar(TableSkeleton table)
      {
         var color = Color.LightGray;

         _newCanvas.gc.DrawLine(new Pen(color, 1),
             table.ScrollBarStartLoc,
             table.ScrollBarStopLoc);

         table.CirclePoint = GetRectF(
             table.ScrollBarStartLoc.X,
             table.ScrollBarStartLoc.Y + (float)(table.CurrentPage / table.ScrollPagesPerPixel),
             table.ScrollBarCirclRadius);

         _newCanvas.gc.FillEllipse(
             new SolidBrush(color),
             table.CirclePoint);
      }
      private void DoWork_ResizeControl()
      {         
         _table.Resize(_newCanvas.bmp.Size);
      }
      private void QueueDrawRequest(string msg)
      {
         _queueWork.Add(new Work(Work.WType.Draw, msg));
         Interlocked.Increment(ref _totalDraws);
      }
      private void DoWork_RequestDrawWithFlag(string msg)
      {
         _table.Dirty = true;
         QueueDrawRequest(msg);
      }
      private void DoWork_Draw(string drawType)
      {
         try
         {
            if (_newCanvas.SetSize(masterPanel.Size) || _table.Dirty)
            {
               DoWork_ResizeControl();
               _table.Dirty = false;
            }

            _newCanvas.gc.Clear(_userConfig.BackgroundColor);

            //Draw table

            if (_userConfig.ShowTableHeader)
            {
               var rect = new RectangleF(
                   _table.Location.X,
                   _table.Location.Y,
                   _table.Size.Width - (_table.ScrollBarOffsetFromRight + 30),
                   _table.RowsHeight);
            
               string addTextToMsg =
                   _userConfig.DebugUI ?
                   $"-{DateTime.Now.ToString(_userConfig.Column1TimeStampFormat)} - {Interlocked.Read(ref _totalDraws)} - {drawType}" :
                   "";

               var (enbled, timeDiff) = _table.GetSelectedLineDiff();
               if (enbled)
               {
                  addTextToMsg += $"- {timeDiff}";
               }

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

            if (_view != null)
            {
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

               for (int ndx = 0; ndx < _table.RowsPerPage; ndx++)
               {
                  if (currentLine < _table.TotalDataLines)
                  {
                     var (isRowSelected, rowData) = _table.GetLine(currentLine);
                     currentLine++;

                     float rowYPos = (_table.RowsHeight * (ndx + 1)) + (_table.Location.Y + _table.OffsetStringData);

                     if (isRowSelected && !_table.ShowGlobalLine)
                     {
                        _newCanvas.gc.FillRectangle(
                            new SolidBrush(Color.FromArgb(30, Color.Blue)),
                            0,
                            rowYPos,
                            _table.Size.Width,
                            _table.RowsHeight);
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
                                 $"{(rowData.TimeStamp -_table.ShowTime_Default_StartTime)}"
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
            if (_table.TotalDataLines != 0)
            {
               DoWork_DrawScrollBar(_table);
            }

            masterPanel.Invoke(new Action(() =>
            {
               lblPage.Text = _table.TotalPages == 0 ? "" : $"{_table.CurrentPage + 1} of {_table.TotalPages}";
               if (lblPage.Text.Length > 9)
               {
                  lblPage.Text = $"{_table.CurrentPage + 1} of {_table.TotalPages}";
               }
               masterPanel.CreateGraphics().DrawImage(_newCanvas.bmp, 0, 0);
               if (_lineInfoDlg.Visible)
               {
                  SetInfoDialogData();
               }

               UpdateTotal(ViewCommon.Engine.GetStartInfo());
            }));

         }
         catch (InvalidOperationException ex1)
         {
            _logger.Error(ex1.ToString());
            Thread.Sleep(200);
            QueueDrawRequest($"Invalid operation");
         }
         catch (Exception ex)
         {
            Thread.Sleep(200);
            _logger.Error(ex.ToString());
            QueueDrawRequest($"Exception.");
         }
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
         if (ViewCommon.Engine.TotalLogLines != 0 && (MessageBox.Show(this, "There are logs in viewer. Please confirm to clear ?", "Alert", MessageBoxButtons.OKCancel) == DialogResult.Cancel))
         {
            return;
         }
         ViewCommon.Engine.Clear();
      }
      private void lblClearFilter_Click(object sender, EventArgs e)
      {
         //_queueWork.Add(new Work(Work.WType.SetNewView, ViewCommon.Engine.GetMasterFrame()));
         HelperSetFilter(new TermFilter());
      }
      private void btnSavedFilterAdd_Click(object sender, EventArgs e)
      {
         ViewCommon.AddFilter(txtSavedFilter.Text, CurrentFilter);
      }
      private void btnSavedFilterApply_Click(object sender, EventArgs e)
      {
         CurrentFilter = ViewCommon.GetFilter(txtSavedFilter.Text);
         HelperSetFilter(CurrentFilter, true);
      }
      private void btnSavedFilterDelete_Click(object sender, EventArgs e)
      {
         ViewCommon.RemoveFilter(txtSavedFilter.Text);
      }
      private void lstSavedFilter_SelectedIndexChanged(object sender, EventArgs e)
      {
         
         btnSavedFilterDelete.Enabled = false;
         btnSavedFilterApply.Enabled = false;

         if (lstSavedFilter.SelectedIndex != -1) {
            txtSavedFilter.Text = lstSavedFilter.Items[lstSavedFilter.SelectedIndex].ToString();            
            btnSavedFilterDelete.Enabled = true;
            btnSavedFilterApply.Enabled = true;
         }
      }
      private void LoadFilter()
      {
         lstSavedFilter.ClearSelected();
         lstSavedFilter.Items.Clear();
         var items = ViewCommon.GetAllNames();
         lstSavedFilter.Items.AddRange(items);
         txtSavedFilter.Text = "";

         btnSavedFilterAdd.Enabled = true;
         btnSavedFilterDelete.Enabled = false;
         btnSavedFilterApply.Enabled = false;
      }
   }
}
