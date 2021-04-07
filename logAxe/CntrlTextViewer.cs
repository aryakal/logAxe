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
using System.Diagnostics;
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

      Task _backgroundWork;
      CancellationTokenSource _cancelToken;
      BlockingCollection<Work> _queueWork = new BlockingCollection<Work>();
      UserConfig _userConfig;
      DrawSurface _newCanvas = new DrawSurface();
      LogFrame _view;
      TableSkeleton _table;
      TermFilter CurrentFilter = new TermFilter();
      MessageExchangeHelper _msgHelper;
      MouseState _mouse = new MouseState();
      long _totalDraws = 0;
      private frmLineData _lineInfoDlg = new frmLineData();
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

            DoWork_RequestDraw("init");
            SetForeColor();
            UpdateTotal(ViewCommon.Engine.GetStartInfo());
            AddMenuItem();
         }
         else
         {
            _table = new TableSkeleton(masterPanel.Location, masterPanel.Size, null);
         }

         _lineInfoDlg.MoveLine += MoveLine;

      }

      public void SetMasterView()
      {
         if (ViewCommon.Engine != null)
         {
            _queueWork.Add(new Work(Work.WType.SetNewView, ViewCommon.Engine.GetMasterFrame()));
         }
      }

      private void MoveLine(int delta)
      {
         if (_table.MoveByLineDelta(delta))
         {
            DoWork_RequestDraw("Ctrl_MouseUp");
         }
      }

      protected override void OnHandleDestroyed(EventArgs e)
      {
         _msgHelper?.Unregister();
      }

      void RegisterControl(Control ctrl)
      {
         ctrl.Resize += Ctrl_Resize;
         ctrl.MouseUp += Ctrl_MouseUp;
         ctrl.MouseDown += Ctrl_MouseDown;
         ctrl.MouseDoubleClick += Ctrl_MouseDoubleClick;
         ctrl.MouseMove += Ctrl_MouseMove;
         ctrl.MouseWheel += Ctrl_MouseWheel;
      }

      private NamedLogger _logger;

      #region Mouse_cntrl
      bool _cancelNextMouseUp = false;
      private void Ctrl_MouseWheel(object sender, MouseEventArgs e)
      {
         int numberOfTextLinesToMove = (e.Delta * -1) * SystemInformation.MouseWheelScrollLines / 120;
         _table.MoveLine(numberOfTextLinesToMove);
         DoWork_RequestDraw("Ctrl_MouseUp");
      }
      private void Ctrl_MouseMove(object sender, MouseEventArgs e)
      {
         if (_table.ScrollMove)
         {
            ScrollMove(e.X, e.Y);
            DoWork_RequestDraw("Ctrl_MouseMove");
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
                  DoWork_RequestDraw("Ctrl_MouseUp");
               }

            }
         }

      }
      private void ScrollMove(int x, int y)
      {
         _table.ChangeScrollMove(y);
         DoWork_RequestDraw("ScrollMove");
      }
      private void Ctrl_Resize(object sender, EventArgs e)
      {
         DoWork_RequestDraw("Ctrl_Resize");
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
         btnFilter.Text = "Show Filter";
         pnlFilterSearch.Visible = !pnlFilterSearch.Visible;
         btnFilter.Text = pnlFilterSearch.Visible ? "Hide Filter" : "Show Filter";
         if (pnlFilterSearch.Visible)
         {
            txtInclude.Focus();
         }
         else
         {
            masterPanel.Focus();
         }
      }
      private void masterPanel_Paint(object sender, PaintEventArgs e)
      {
         DoWork_RequestDraw("masterPanel_Paint");
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
      private void exportFileToolStripMenuItem_Click(object sender, EventArgs e)
      {
         var selectedFileIndex = _table.GetFileIndexForSeletedLines();
         var fld = new frmFileManager();
         fld.InitialSelectedFileNo = selectedFileIndex;
         fld.ShowDialog();
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

            lblHiddenTotalTxt.Font = new Font(
                      lblHiddenTotalTxt.Font.FontFamily,
                      lblHiddenTotalTxt.Font.Size,
                      lblHiddenTotal.Text != "" ? System.Drawing.FontStyle.Regular : System.Drawing.FontStyle.Strikeout,
                      System.Drawing.GraphicsUnit.Point,
                      ((byte)(0)));

         }));
         DoWork_RequestDraw("SetView");
      }
      private string GetMaxLength(int value)
      {
         if (value > 10000000) return "999999999";
         else if (value > 1000000) return "99999999";
         else if (value > 100000) return "9999999";
         else if (value > 10000) return "999999";
         else if (value > 1000) return "99999";
         else if (value > 100) return "9999";
         else if (value > 10) return "999";
         else return "99";
      }
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
      private void masterPanel_PreviewKeyDown(object sender, PreviewKeyDownEventArgs e)
      {

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
               DoWork_RequestDraw($"ProcessCmdKey, {keyData}");
               break;
            case Keys.End:
               _table.MovetoPage(_table.TotalPages);
               DoWork_RequestDraw($"ProcessCmdKey, {keyData}");
               break;
            case Keys.F6:
               _table["ProcId"].Visible = !_table["ProcId"].Visible;
               _table.Dirty = true;
               DoWork_RequestDraw($"ProcessCmdKey, {keyData}");
               break;
            case Keys.F7:
               _table["ThId"].Visible = !_table["ThId"].Visible;
               _table.Dirty = true;
               DoWork_RequestDraw($"ProcessCmdKey, {keyData}");
               break;
            case Keys.F8:
               _table["Category"].Visible = !_table["Category"].Visible;
               _table.Dirty = true;
               DoWork_RequestDraw($"ProcessCmdKey, {keyData}");
               break;
            case Keys.F9:
               var val = !_table["ProcId"].Visible;
               _table["ProcId"].Visible = val;
               _table["ThId"].Visible = val;
               _table["Category"].Visible = val;
               _table.Dirty = true;
               DoWork_RequestDraw($"ProcessCmdKey, {keyData}");
               break;
            case Keys.F10:
               _table["TimeStamp"].Visible = !_table["TimeStamp"].Visible;               
               DoWork_RequestDraw($"ProcessCmdKey, {keyData}");
               break;
            case Keys.PageDown:
               _table.GotoNextPage();
               DoWork_RequestDraw($"ProcessCmdKey, {keyData}");
               break;
            case Keys.PageUp:
               _table.GotoPreviouPage();
               DoWork_RequestDraw($"ProcessCmdKey, {keyData}");
               break;
            case Keys.Up:
               _table.MoveLine(-1);
               DoWork_RequestDraw($"ProcessCmdKey, {keyData}");
               break;
            case Keys.Down:
               _table.MoveLine(1);
               DoWork_RequestDraw($"ProcessCmdKey, {keyData}");
               break;

            case (Keys.Control | Keys.C):
               CopyLinesToClipBoard();
               break;
            default:
               return base.ProcessCmdKey(ref msg, keyData);
         }
         return true;
      }
      private void SetInfoDialogData()
      {
         var (lineNo, totalLines, lineInfo) = _table.GetCurrentSelectedLineInfo();
         _lineInfoDlg.SetCurrentLine(lineNo, totalLines, lineInfo);
      }

      private void CopyLinesToClipBoard()
      {
         var lst = _table.GetAllSelectedLogLines();
         var str = new StringBuilder();
         foreach (var line in lst)
         {
            str.Append($"{line.LogType.ToString().Substring(0, 1)}, {line.TimeStamp.ToString("yy-MM-dd HH:mm:ss.fff")}, {line.Msg.Replace("\n", "")}{Environment.NewLine}");
         }
         Clipboard.SetText(str.ToString());
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
         var fld = new frmFileManager();
         fld.InitialSelectedFileNo = new int[] { 1 };
         fld.ShowDialog();
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
         ShowFilterView(CurrentFilter);
      }
      private void lblHiddenTotal_Click(object sender, EventArgs e)
      {
         ShowFilterView(new TermFilter());
      }

      private void SetLogTypeFilter(LogType logType)
      {
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
         ShowFilterView(CurrentFilter);
      }

      private void ShowFilterView(TermFilter filter)
      {
         Task.Run(() =>
         {
            var statTime = DateTime.Now;

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
            }
         });

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
                  var stp = new Stopwatch(); stp.Start();
                  DoWork_Draw((string)work.RealWork);
                  stp.Stop();
                  Debug.WriteLine($" {stp.Elapsed}");
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
                  break;

               case LogAxeMessageEnum.FileParseEnd:
                  SetView(ViewCommon.Engine.GetMasterFrame());
                  break;

               case LogAxeMessageEnum.NewUserConfiguration:
                  _userConfig = ViewCommon.UserConfig;
                  masterPanel.Invoke(new Action(() =>
                  {
                     SetForeColor();
                  }));
                  _table.Dirty = true;
                  DoWork_RequestDraw($"LogAxeMessageEnum.NewUserConfiguration");
                  break;
               case LogAxeMessageEnum.BroadCastGlobalLine:
                  {
                     // We got the global line now lets search in the local view.
                     // Either we will have an empty view (because this view does not have any line
                     if (!_view.IsEmpty)
                     {
                        var lineNo = _view.GetGlobalLine(((CurrentGlobalLine)message).GlobalLine);
                        _table.SetGlobalLine(lineNo);
                        DoWork_RequestDraw($"MessageType.BroadCastGlobalLine");
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
                        DoWork_RequestDraw($"LogAxeMessageEnum.AddLineToNotepadEvent");
                     }

                  }
                  break;

               case LogAxeMessageEnum.NewMainFrmAddRemoved:
               case LogAxeMessageEnum.AwakeAllWindows:
                  OnNewNotepadChange?.Invoke(message);
                  break;
            }
         }
         catch (Exception ex)
         {
            _logger.LogError(ex.ToString());
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
         var stringSize = _newCanvas.gc.MeasureString(DateTime.Now.ToString(_userConfig.Column1TimeStampFormat), _userConfig.TableHeaderFont);

         //_table.Resize(
         //    _newCanvas.bmp.Size,                 
         //    _userConfig.FontHeightPad,                 
         //    _userConfig.ShowTableHeader,
         //    _userConfig.TableHeaderFont, 
         //    _userConfig.Column1TimeStampFormat,
         //    _userConfig.PadBetweenCol);
         _table.Resize(_newCanvas.bmp.Size);
      }
      private void DoWork_RequestDraw(string msg)
      {
         _queueWork.Add(new Work(Work.WType.Draw, msg));
         Interlocked.Increment(ref _totalDraws);
      }
      private void DoWork_RequestDrawWithFlag(string msg)
      {
         _table.Dirty = true;
         DoWork_RequestDraw(msg);
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

            //_newCanvas.gc.Clear(_userConfig.BackgroundColor);
            //_newCanvas.gc.FillRectangle(new SolidBrush(_userConfig.TableBackgroundColor),
            //    _table.Location.X,
            //    _table.Location.Y,
            //    _table.Size.Width,
            //    _table.Size.Height);

            _newCanvas.gc.Clear(_userConfig.BackgroundColor);
            //using (TextureBrush brush = new TextureBrush(Properties.Resources.background, System.Drawing.Drawing2D.WrapMode.Tile))
            //{
            //   _newCanvas.gc.FillRectangle(brush, 0, 0, _newCanvas.bmp.Width, _newCanvas.bmp.Height);
            //}

            //Draw table


            if (_userConfig.ShowTableHeader)
            {
               var rect = new RectangleF(
                   _table.Location.X,
                   _table.Location.Y,
                   _table.Size.Width - (_table.ScrollBarOffsetFromRight + 30),
                   _table.RowsHeight);
               ////Utils.LogDebug($"width = {_table.Size.Width - 10}");

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

                              _newCanvas.gc.DrawString(

                                  _userConfig.DebugUI ?
                                  $"{rowData.GlobalLine}| {rowData.FileNumber}| {rowData.LineNumber}| {rowData.Msg.TrimStart()}" :
                                  $"{rowData.Msg.TrimStart()}",

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
            _logger.LogError(ex1.ToString());
            Thread.Sleep(200);
            DoWork_RequestDraw($"Invalid operation");
         }
         catch (Exception ex)
         {
            Thread.Sleep(200);
            _logger.LogError(ex.ToString());
            DoWork_RequestDraw($"Exception.");
         }
      }

      private void filterByThreadId_Click(object sender, EventArgs e)
      {
         ShowFilterView(CurrentFilter);
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
         ShowFilterView(CurrentFilter);
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

         ShowFilterView(CurrentFilter);
      }
      private void handle_enter_key(object sender, KeyPressEventArgs e)
      {
         if (e.KeyChar == (char)Keys.Return)
         {
            btnFllter_Click(null, null);
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
         ViewCommon.Engine.Clear();
      }
      private void lblClearFilter_Click(object sender, EventArgs e)
      {
         //_queueWork.Add(new Work(Work.WType.SetNewView, ViewCommon.Engine.GetMasterFrame()));
         ShowFilterView(new TermFilter());
      }

      
   }
}
