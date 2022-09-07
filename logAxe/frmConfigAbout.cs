//=====================================================================================================================
// Source : https://github.com/aryakal/
//--------------------------------------------------------------------------------------------------------------------
//=====================================================================================================================

using System;
using System.Windows.Forms;
//using logAxeEngine.Interfaces;
using System.Text;
using logAxeCommon;
using libACommunication;

namespace logAxe
{
   public partial class frmConfigAbout : Form
   {
      ConfigUI _userConfig;
      public frmConfigAbout()
      {
         InitializeComponent();
         
         ShowLicense();
         Icon = Properties.Resources.axe_icon_svg_128;
      }

      private void frmConfiguration_Load(object sender, EventArgs e)
      {
         _userConfig = ViewCommon.ConfigOfSystem;
         propertyGrid1.SelectedObject = _userConfig;
      }

      private void propertyGrid1_PropertyValueChanged(object s, PropertyValueChangedEventArgs e)
      {
         //TODO review         
         ViewCommon.PostCurrentTheme(_userConfig);
      }

      private void ShowLicense()
      {
         var text = new StringBuilder();

         text.Append($"logAxe{Environment.NewLine}");
         text.Append($"        Version:  {ViewCommon.VersionNo}{Environment.NewLine}");
         text.Append($"         Source:  https://github.com/aryakal/logAxe {Environment.NewLine}");
         text.Append($"          Issue:  https://github.com/aryakal/logAxe/issues {Environment.NewLine}");
         text.Append($"Feature Request:  https://github.com/aryakal/logAxe/issues {Environment.NewLine}");
         text.Append($"           Used:  | MIT | SharpZipLib     | https://github.com/icsharpcode/SharpZipLib {Environment.NewLine}");
         text.Append($"           Used:  | MIT | Newtonsoft.Json | https://www.newtonsoft.com/json {Environment.NewLine}");

         //TODO : Prefect the license info.
         //if (ViewCommon.Engine != null)
         //{
         //   text.Append($"{ViewCommon.Engine.GetLicenseInfo()}");
         //}

         text.Append($"---- {Environment.NewLine}");

         text.Append("Copyright 2022 aryakal" + Environment.NewLine);
         text.Append("Permission is hereby granted, free of charge, to any person obtaining a copy of this software and associated documentation files(the \"Software\"), to deal in the Software without restriction, including without limitation the rights to use, copy, modify, merge, publish, distribute, sublicense, and / or sell copies of the Software, and to permit persons to whom the Software is furnished to do so, subject to the following conditions:" + Environment.NewLine);
         text.Append("The above copyright notice and this permission notice shall be included in all copies or substantial portions of the Software." + Environment.NewLine);
         text.Append("THE SOFTWARE IS PROVIDED \"AS IS\", WITHOUT WARRANTY OF ANY KIND, EXPRESS OR IMPLIED, INCLUDING BUT NOT LIMITED TO THE WARRANTIES OF MERCHANTABILITY, FITNESS FOR A PARTICULAR PURPOSE AND NONINFRINGEMENT.IN NO EVENT SHALL THE AUTHORS OR COPYRIGHT HOLDERS BE LIABLE FOR ANY CLAIM, DAMAGES OR OTHER LIABILITY, WHETHER IN AN ACTION OF CONTRACT, TORT OR OTHERWISE, ARISING FROM, OUT OF OR IN CONNECTION WITH THE SOFTWARE OR THE USE OR OTHER DEALINGS IN THE SOFTWARE." + Environment.NewLine);

         //TODO : fix this.
         //text.Append($"{Environment.NewLine}ConfigPath : {ViewCommon.TransitionToCommonFunctionality.RootAppDataPath} {Environment.NewLine}");

         txtAbout.Text = text.ToString();
      }
   }
}
