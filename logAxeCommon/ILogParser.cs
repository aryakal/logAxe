//=====================================================================================================================
// Source : https://github.com/aryakal/
//--------------------------------------------------------------------------------------------------------------------
//=====================================================================================================================

namespace logAxeCommon
{
   /// <summary>
   /// ILogParser should be used by plugin developer.
   /// Only one instance will be used.
   /// </summary>
   public interface ILogParser
   {
      /// <summary>
      /// Name of the parser, needs to be unique name.
      /// </summary>
      string ParserName { get; }

      /// <summary>
      /// The log engine will only provide the filename not file path.
      /// </summary>
      /// <param name="fileName">name of the file to process. This needs to say if the file can be parsed by this parser.</param>
      /// <returns>True : if the file can be parsed by this parser.</returns>
      bool CanParseLogFile(string fileName);

      /// <summary>
      /// The main parsing by plugin. Now it is expected the pluging to parse the lines from the
      /// file provided and then attached the LogLine to the logfile. Rest will be taken care by the log engine.
      /// </summary>
      /// <param name="logFile"></param>
      void ParseFile(IParsedLogStore logFile);

      /// <summary>
      /// The features supported by plugin.
      /// </summary>
      /// <returns></returns>
      PluginFeatureSupport GetSupportedFeatures();
   }
}