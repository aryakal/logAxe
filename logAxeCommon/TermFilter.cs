//=====================================================================================================================
// Source : https://github.com/aryakal/
//--------------------------------------------------------------------------------------------------------------------
//=====================================================================================================================

using System.Linq;

namespace logAxeCommon
{
   /// <summary>
   /// The seach query to the log engine. This support for nested search etc. but it will all come out in time.
   /// TODO : add more examples.
   /// </summary>
   public class TermFilter
   {
      /// <summary>
      /// Used internally to save the name of the filter.
      /// </summary>
      public string Name { get; set; }
      /// <summary>
      /// The child of this filter where filter is applied on this filter.
      /// </summary>
      public TermFilter[] Child { get; set; } = new TermFilter[0];
      /// <summary>
      /// The sibling of this term are or that means in addiont to this term the 
      /// result of the other term will be used.
      /// </summary>
      public TermFilter[] Sibling { get; set; } = new TermFilter[0];
      /// <summary>
      /// Messages to be included. delimited by ;  or delimiter set.
      /// </summary>
      public string[] MsgExclude { get; set; } = new string[0];
      /// <summary>
      /// Messages to be execluded. delimited by ;  or delimiter set.
      /// </summary>
      public string[] MsgInclude { get; set; } = new string[0];
      /// <summary>
      /// Tags to be included. delimited by ;  or delimiter set.
      /// </summary>
      public string[] TagsInclude { get; set; } = new string[0];
      /// <summary>
      /// Messages to be excluded. delimited by ;  or delimiter set.
      /// </summary>
      public string[] TagsExclude { get; set; } = new string[0];
      /// <summary>
      /// Which of the log messages to be seen..
      /// </summary>
      public bool[] FilterTraces { get; set; } = new bool[] { true, true, true, true };

      public bool MatchExact { get; set; }

      /// <summary>
      /// do we match the cases.
      /// </summary>
      public bool MatchCase { get; set; }
      /// <summary>
      /// ist he term enabled. Used to be in sbiling and the childs.
      /// </summary>
      public bool Enabled { get; set; } = true;
      /// <summary>
      /// Messages retrived from the filters for this term.
      /// </summary>
      public int[] MsgIds { get; set; } = new int[0];
      /// <summary>
      /// Tag ids filters for this term.
      /// </summary>
      public int[] TagIds { get; set; } = new int[0];

      /// <summary>
      /// This will lets us known if we have a valid filter or not.
      /// </summary>
      public bool IsValidFilter => (

          MsgInclude.Length != 0 ||
          MsgExclude.Length != 0 ||
          TagsInclude.Length != 0 ||
          !FilterTraces[(int)LogType.Error] ||
          !FilterTraces[(int)LogType.Info] ||
          !FilterTraces[(int)LogType.Trace] ||
          !FilterTraces[(int)LogType.Warning] ||
          Sibling.Length != 0
          );

      public bool IsMasterFilter => (

          MsgInclude.Length == 0 &&
          MsgExclude.Length == 0 &&
          TagsInclude.Length == 0 &&
          FilterTraces[(int)LogType.Error] &&
          FilterTraces[(int)LogType.Info] &&
          FilterTraces[(int)LogType.Trace] &&
          FilterTraces[(int)LogType.Warning] &&
          Sibling.Length == 0
          );

      public int TotalLogTypeFilters
      {
         get
         {
            return FilterTraces == null ? 0 : FilterTraces.Count(c => c == true);
         }
      }
   }
}
