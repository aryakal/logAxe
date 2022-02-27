//=====================================================================================================================
// Source : https://github.com/aryakal/
//--------------------------------------------------------------------------------------------------------------------
//=====================================================================================================================

using System;
using System.Collections.Generic;
using System.Linq;
using System.Drawing;
using logAxeCommon;
using System.Windows.Forms;
using logAxeEngine.Common;

namespace logAxe
{
   class SkeletonControlLocation
   {
      public PointF Location
      {
         get
         {
            return Dimensions.Location;
         }
      }
      public SizeF Size
      {
         get
         {
            return Dimensions.Size;
         }
      }
      public RectangleF Dimensions
      {
         get;
         set;
      } = Rectangle.Empty;

      public SkeletonControlLocation(RectangleF dimension)
      {
         Dimensions = dimension;
      }
      public SkeletonControlLocation(PointF location, SizeF size)
      {
         Dimensions = new RectangleF(location, size);
      }
   }

   class TableColumn
   {
      public string Name { get; set; }
      public string DisplayName { get; set; }
      public float StartLoc { get; set; }
      public bool Visible { get; set; } = true;
      public float Width { get; set; }

      public TableColumn(string name)
      {
         Name = name;
         DisplayName = name;
      }
   }

   public class SelectedItems
   {
      public bool[] SelectedLines { get; set; } = new bool[0];
      public int TotalSelected { get; set; } = 0;
      public int LastSelected { get; private set; } = LogLine.INVALID;

      public void Toggle(int itemNo)
      {
         SelectedLines[itemNo] = !SelectedLines[itemNo];
         TotalSelected = SelectedLines[itemNo] ? ++TotalSelected : --TotalSelected;
      }
      public void SelectItem(int itemNo)
      {
         SelectedLines[itemNo] = true;
         TotalSelected++;
         LastSelected = itemNo;
      }
      public void RemoveSelect(int itemNo)
      {
         SelectedLines[itemNo] = false;
         TotalSelected--;
      }
      public void SelectOnly(int itemNo)
      {
         if (TotalSelected > 1)
         {
            ClearAll();
         }

         if (TotalSelected == 1)
         {
            RemoveSelect(LastSelected);
         }

         SelectItem(itemNo);

      }
      public void SelectUpto(int itemNo)
      {
         if (LastSelected == LogLine.INVALID)
         {
            SelectItem(itemNo);
         }
         else
         {
            var min = Math.Min(LastSelected, itemNo);
            var max = Math.Max(LastSelected, itemNo);
            for (int ndx = min; ndx <= max; ndx++)
            {
               SelectedLines[ndx] = true;
               TotalSelected++;
            }
         }
      }
      public void ClearAll()
      {
         //Resize(SelectedLines.Length);
         Array.Clear(SelectedLines, 0, SelectedLines.Length);
         TotalSelected = 0;
         LastSelected = LogLine.INVALID;
      }
      public void Resize(int length)
      {
         SelectedLines = new bool[length];
         TotalSelected = 0;
         LastSelected = LogLine.INVALID;
      }
      public int[] GetSelectedLines()
      {
         var selectedLines = new List<int>();
         for (var ndx = 0; ndx < SelectedLines.Length; ndx++)
         {
            if (SelectedLines[ndx])
               selectedLines.Add(ndx);
         }
         return selectedLines.ToArray();
      }
   }

   class TableSkeleton : SkeletonControlLocation
   {
      #region Columns

      private readonly Dictionary<string, int> _columnNames = new Dictionary<string, int>();
      private List<TableColumn> _columns = new List<TableColumn>(10);
      public TableColumn this[int index]
      {
         get { return _columns[index]; }
         set { _columns[index] = (TableColumn)value; }
      }
      public TableColumn this[string columnName]
      {
         get { return _columns[_columnNames[columnName]]; }
      }

      public void AddColumn(TableColumn tableColumn)
      {
         _columnNames.Add(tableColumn.Name, _columns.Count);
         _columns.Add(tableColumn);
      }
      public TableColumn[] Columns
      {
         get
         {
            return _columns.ToArray();
         }
      }
      public int TotalColumns
      {
         get
         {
            return _columns.Count;
         }
      }

      #endregion

      public bool ShowGlobalLine { get; set; }
      public int CurrentGlobalLine { get; set; }
      public int TotalPages { get; set; }
      public int CurrentPage { get; set; }
      public int RowsPerPage { get; set; }
      public int RowsHeight { get; set; }
      public int OffsetStringData { get; set; }
      public string[,] Data { get; set; }
      public int TotalDataLines { get; private set; }
      public int CurrentDataLine
      {
         get
         {           
            return _currentSkeletonLine;
         }
         set
         {
            if (value < 0)
            {
               _currentSkeletonLine = 0;
            }
            else
               _currentSkeletonLine = value;
            SetPageStats();

         }
      }
      public int CurrentMouseLine { get; set; }
      public PointF ScrollBarStartLoc { get; set; }
      public PointF ScrollBarStopLoc { get; set; }
      public float ScrollY { get; set; } = 0;
      public bool ScrollMove { get; set; } = false;
      public RectangleF ScrollBarArea { get; set; }
      public float ScrollBarOffsetFromRight { get; } = 30F;
      public float ScrollBarOffsetFromBottom { get; } = 20F;
      public float ScrollBarOffsetFromTop { get; } = 10F;
      public float ScrollBarLineWidth { get; } = 3;
      public bool ScrollBarIsCircle { get; } = true;
      public float ScrollBarLocation { get; } = 0;
      public float ScrollBarCirclRadius { get; set; } = 7F;
      public float ScrollHeight { get; } = 0;
      public double ScrollPagesPerPixel { get; set; }
      public RectangleF CirclePoint { get; set; } = new RectangleF(0, 0, 0, 0);
      public bool Dirty { get; set; }
      public LogFrame ViewFrame { get; set; }
      public bool ShowTableHeader { get; set; }
      public SelectedItems SelectedLine { get; set; } = new SelectedItems();
      public enum ShowTime { 
         Default,
         StartTime
      }

      public ShowTime ShowTimeSelected { get; set; } = ShowTime.Default;
      public DateTime ShowTime_Default_StartTime { get; set; } = DateTime.Now;

      private MessageExchangeHelper _msgHelper;
      private int _currentSkeletonLine = 0;

      public TableSkeleton(PointF point, SizeF size, MessageExchangeHelper msgHelper) : base(point, size)
      {
         _msgHelper = msgHelper;
      }
      public void Resize(SizeF size)
      {
         var config = ViewCommon.UserConfig;

         ShowTableHeader = config.ShowTableHeader;
         //this["LineNo"].Visible = config.ShowLineNo;

         Dimensions = new RectangleF(Location, size);
         //SelectedLines = new bool[ViewFrame.TotalLogLines];
         using (Bitmap bmp = new Bitmap(500, 500))
         {
            using (var canvas = Graphics.FromImage(bmp))
            {
               var stringSize = canvas.MeasureString(DateTime.Now.ToString(config.Column1TimeStampFormat), config.TableHeaderFont);
               var lineColSize = canvas.MeasureString("3000000", config.TableHeaderFont);
               RowsHeight = (int)(stringSize.Height + config.FontHeightPad);
               OffsetStringData = (int)(config.FontHeightPad / 2);
               RowsPerPage = (int)(Size.Height / RowsHeight);
               RowsPerPage = ShowTableHeader ? RowsPerPage - 1 : RowsPerPage;

               SetPageStats();

               this["LineNo"].Width = lineColSize.Width;
               this["TimeStamp"].Width = stringSize.Width;
               this["ThId"].Width = lineColSize.Width;
               this["ProcId"].Width = lineColSize.Width;

               //Resize all column start stop
               float columnStartLoc = 0;
               for (int colNdx = 0; colNdx < TotalColumns; colNdx++)
               {
                  if (!this[colNdx].Visible) continue;
                  this[colNdx].StartLoc = columnStartLoc;
                  columnStartLoc += this[colNdx].Width + config.PadBetweenCol;
               }
            }


            float x = Dimensions.Location.X + (Size.Width - ScrollBarOffsetFromRight);
            float y = config.ShowTableHeader ? (RowsHeight + ScrollBarOffsetFromTop) : ScrollBarOffsetFromTop;
            float y1 = Size.Height - ScrollBarOffsetFromBottom;
            ScrollBarStartLoc = new PointF(x, y);
            ScrollBarStopLoc = new PointF(x, y1);
            ScrollPagesPerPixel = TotalDataLines == 0 ? 0 : (TotalPages / (y1 - y));
            ScrollBarArea = new RectangleF(
                x - ScrollBarCirclRadius,
                y - ScrollBarCirclRadius,
                Size.Width - (x + ScrollBarCirclRadius),
                (y1 - y) + (ScrollBarCirclRadius * 2));
         }
      }
      public void Resize(SizeF size, int FontHeightPad, bool showTableHeader, Font tableHeaderFont, string timeStampFormat, int PadBetweenCol)
      {
         ShowTableHeader = showTableHeader;
         Dimensions = new RectangleF(Location, size);
         //SelectedLines = new bool[ViewFrame.TotalLogLines];
         using (Bitmap bmp = new Bitmap(500, 500))
         {
            using (var canvas = Graphics.FromImage(bmp))
            {
               var stringSize = canvas.MeasureString(DateTime.Now.ToString(timeStampFormat), tableHeaderFont);
               var lineColSize = canvas.MeasureString("3000000", tableHeaderFont);
               RowsHeight = (int)(stringSize.Height + FontHeightPad);
               OffsetStringData = (int)(FontHeightPad / 2);
               RowsPerPage = (int)(Size.Height / RowsHeight);
               RowsPerPage = ShowTableHeader ? RowsPerPage - 1 : RowsPerPage;

               SetPageStats();

               this["LineNo"].Width = lineColSize.Width;
               this["TimeStamp"].Width = stringSize.Width;
               this["ThId"].Width = lineColSize.Width;
               this["ProcId"].Width = lineColSize.Width;

               //Resize all column start stop
               float columnStartLoc = 0;
               for (int colNdx = 0; colNdx < TotalColumns; colNdx++)
               {
                  if (!this[colNdx].Visible) continue;
                  this[colNdx].StartLoc = columnStartLoc;
                  columnStartLoc += this[colNdx].Width + PadBetweenCol;
               }
            }


            float x = Dimensions.Location.X + (Size.Width - ScrollBarOffsetFromRight);
            float y = showTableHeader ? (RowsHeight + ScrollBarOffsetFromTop) : ScrollBarOffsetFromTop;
            float y1 = Size.Height - ScrollBarOffsetFromBottom;
            ScrollBarStartLoc = new PointF(x, y);
            ScrollBarStopLoc = new PointF(x, y1);
            ScrollPagesPerPixel = TotalDataLines == 0 ? 0 : (TotalPages / (y1 - y));
            ScrollBarArea = new RectangleF(
                x - ScrollBarCirclRadius,
                y - ScrollBarCirclRadius,
                Size.Width - (x + ScrollBarCirclRadius),
                (y1 - y) + (ScrollBarCirclRadius * 2));
         }
      }
      public void ChangeScrollMove(int y)
      {
         if (y < ScrollBarStartLoc.Y)
         {
            y = (int)ScrollBarStartLoc.Y;
         }
         else if (y > ScrollBarStopLoc.Y)
         {
            y = ((int)ScrollBarStopLoc.Y);
         }
         ScrollY = y;
         MovetoPage((int)(ScrollPagesPerPixel * (ScrollY - ScrollBarStartLoc.Y)));
      }
      public bool SetCurrentSelection(int y, Keys keys)
      {
         var cntrlPressed = keys == Keys.Control;
         var shiftPressed = keys == Keys.Shift;

         var selectedLineNo = (int)(y / RowsHeight);
         if (ShowTableHeader)
            selectedLineNo--;

         return ResetCurrentSelectedLine(CurrentDataLine + selectedLineNo, cntrlPressed, shiftPressed);
      }
      public bool MoveByLineDelta(int delta)
      {
         return ResetCurrentSelectedLine(SelectedLine.LastSelected + delta);
      }
      public (int, int, LogLine) GetCurrentSelectedLineInfo()
      {
         if (SelectedLine.LastSelected != LogLine.INVALID)
         {
            var (_, lineInfo) = GetLine(SelectedLine.LastSelected);
            return (SelectedLine.LastSelected, ViewFrame.TotalLogLines, lineInfo);
         }
         else
         {
            return (LogLine.INVALID, LogLine.INVALID, null);
         }

      }
      public void GotoNextPage()
      {
         MovetoPage(++CurrentPage);
      }
      public void GotoPreviouPage()
      {
         MovetoPage(--CurrentPage);
      }
      public void MovetoPage(int pageNo)
      {
         if (pageNo < 0)
         {
            pageNo = 0;
         }
         if (pageNo >= TotalPages)
         {
            pageNo = TotalPages - 1;
         }
         CurrentDataLine = pageNo * RowsPerPage;
      }
      public void MovetoPageWithLine(int lineNo)
      {
         int pageNo = Convert.ToInt32(lineNo / RowsPerPage);
         MovetoPage(pageNo);
      }
      public void MoveLine(int delta)
      {
         SetCurrentLine(CurrentDataLine + delta);
      }
      public void SetCurrentLine(int lineNumber)
      {
         CurrentDataLine = lineNumber;
         if (CurrentDataLine < 0)
         {
            CurrentDataLine = 0;
         }
         else if (CurrentDataLine > ViewFrame.TotalLogLines)
         {
            CurrentDataLine = ViewFrame.TotalLogLines;
         }
      }
      public void SetGlobalLine(int lineNumber)
      {
         MovetoPageWithLine(Math.Abs(lineNumber));
         CurrentGlobalLine = lineNumber;
         ShowGlobalLine = true;
      }
      public void SetTotalLines(int totalLines)
      {
         TotalDataLines = totalLines;
         SetPageStats();

      }
      public void SetViewFrame(LogFrame frame)
      {
         ViewFrame = frame;
         SelectedLine.Resize(ViewFrame.TotalLogLines);
      }
      /// <summary>
      /// Get the actual line data
      /// </summary>
      /// <param name="lineNumber">Line from local index.</param>
      /// <returns>bool: line is selected, LogLine : from global line.</returns>
      public (bool, LogLine) GetLine(int lineNumber)
      {
         return (SelectedLine.SelectedLines.Length == 0 ? false : SelectedLine.SelectedLines[lineNumber],
                 ViewCommon.Engine.GetLogLine(ViewFrame.TranslateLine(lineNumber)));
      }
      public (bool, LogLine) GetSelectedLine()
      {
         if (SelectedLine.TotalSelected == 0 || SelectedLine.LastSelected == LogLine.INVALID)
            return (false, null);

         return (true,
             ViewCommon.Engine.GetLogLine(
                 ViewFrame.TranslateLine(
                     SelectedLine.LastSelected)));
      }
      
      public (bool, TimeSpan) GetSelectedLineDiff()
      {
         if (SelectedLine.TotalSelected < 2 || SelectedLine.LastSelected == LogLine.INVALID)
            return (false, new TimeSpan(0));
         var lines = SelectedLine.GetSelectedLines();
         var first = ViewCommon.Engine.GetLogLine(ViewFrame.TranslateLine(lines[0]));
         var last = ViewCommon.Engine.GetLogLine(ViewFrame.TranslateLine(lines[lines.Length-1]));
         
         return (true, last.TimeStamp - first.TimeStamp);

      }

      public List<LogLine> GetAllSelectedLogLines()
      {
         var lst = new List<LogLine>();

         if (SelectedLine.TotalSelected == 0 || SelectedLine.LastSelected == LogLine.INVALID)
            return lst;
         
         foreach (var line in SelectedLine.GetSelectedLines())
         {
            lst.Add(ViewCommon.Engine.GetLogLine(ViewFrame.TranslateLine(line)));
         }

         return lst;

      }
      /// <summary>
      /// Gets the index of the selected files on the view.
      /// </summary>
      /// <returns></returns>
      public int[] GetGlobalLineIndexForSeletedLines()
      {
         var globalLine = new List<int>();
         var selectedLines = SelectedLine.GetSelectedLines();
         foreach (var line in selectedLines)
         {
            globalLine.Add(ViewFrame.TranslateLine(line));
         }
         return globalLine.ToArray();
      }
      /// <summary>
      /// Gets the index of the selected files on the view.
      /// </summary>
      /// <returns></returns>
      public int[] GetFileIndexForSeletedLines()
      {
         var fileList = new List<int>();
         var selectedLines = SelectedLine.GetSelectedLines();
         foreach (var line in selectedLines)
         {
            var (_, logLine) = GetLine(line);
            fileList.Add(logLine.FileNumber);
         }
         return fileList.Distinct().ToArray();
      }

      private bool ResetCurrentSelectedLine(int worldLine, bool cntrlPressed = false, bool shiftPressed = false)
      {
         if (worldLine >= 0 && worldLine < ViewFrame.TotalLogLines)
         {
            ShowGlobalLine = false;
            _msgHelper.PostMessage(new logAxeEngine.EventMessages.CurrentGlobalLine() { GlobalLine = ViewFrame.TranslateLine(worldLine) }); ;

            if (cntrlPressed)
            {
               SelectedLine.Toggle(worldLine);
            }
            else if (shiftPressed)
            {
               SelectedLine.SelectUpto(worldLine);
            }
            else
            {
               SelectedLine.SelectOnly(worldLine);
            }
            return true;
         }
         return false;
      }
      private void SetPageStats()
      {
         if (RowsPerPage != 0)
         {
            var extraPage = TotalDataLines % RowsPerPage;
            TotalPages = (int)(TotalDataLines / RowsPerPage) + (extraPage > 0 ? 1 : 0);
            CurrentPage = (int)(_currentSkeletonLine / RowsPerPage);
            //CurrentPage += 1;
         }

      }

   }
}
