using System;
using System.Windows.Forms;
using logAxeEngine.Interfaces;
using logAxeEngine.Common;
using System.Text;

namespace logAxe
{
    public partial class frmConfigAbout : Form
    {
        public frmConfigAbout()
        {
            InitializeComponent();
            ViewCommon.LoadConfiguration();
            ShowLicense();
            this.Icon = Properties.Resources.axe_icon_svg_128;
        }

        private void frmConfiguration_Load(object sender, EventArgs e)
        {
            propertyGrid1.SelectedObject = ViewCommon.UserConfig;
        }

        private void propertyGrid1_PropertyValueChanged(object s, PropertyValueChangedEventArgs e)
        {
            ViewCommon.UserConfig.TableHeaderFont = ViewCommon.UserConfig.TableBodyFont;
            ViewCommon.SaveConfiguration();
            ViewCommon.BroadCastMessage(new LogAxeGenericMessage() { MessageType = LogAxeMessageEnum.NewUserConfiguration });
        }

        private void ShowLicense()
        {
            var text = new StringBuilder();

            text.Append($"logAxe{Environment.NewLine}");
            text.Append($"        Version:  {ViewCommon.VersionNo}{Environment.NewLine}");
            text.Append($"         Source:  https://github.com/aryakal/logAxe {Environment.NewLine}");
            text.Append($"          Issue:  https://github.com/aryakal/logAxe/issues {Environment.NewLine}");
            text.Append($"Feature Request:  https://github.com/aryakal/logAxe/issues {Environment.NewLine}");
            //text.Append($"           Used:              Svg |  MS-PL | https://github.com/vvvv/SVG {Environment.NewLine}");
            //text.Append($"           Used:          Fizzler | Custom | https://github.com/atifaziz/Fizzler {Environment.NewLine}");
            text.Append($"           Used:  Newtonsoft.Json |    MIT | https://www.newtonsoft.com/json {Environment.NewLine}");


            if (ViewCommon.Engine != null)
            {
                text.Append($"{ViewCommon.Engine.GetLicenseInfo()}");
            }

            text.Append($"---- {Environment.NewLine}");

            text.Append("Copyright 2020 aryakal" + Environment.NewLine);
            text.Append("Permission is hereby granted, free of charge, to any person obtaining a copy of this software and associated documentation files(the \"Software\"), to deal in the Software without restriction, including without limitation the rights to use, copy, modify, merge, publish, distribute, sublicense, and / or sell copies of the Software, and to permit persons to whom the Software is furnished to do so, subject to the following conditions:" + Environment.NewLine);
            text.Append("The above copyright notice and this permission notice shall be included in all copies or substantial portions of the Software." + Environment.NewLine);
            text.Append("THE SOFTWARE IS PROVIDED \"AS IS\", WITHOUT WARRANTY OF ANY KIND, EXPRESS OR IMPLIED, INCLUDING BUT NOT LIMITED TO THE WARRANTIES OF MERCHANTABILITY, FITNESS FOR A PARTICULAR PURPOSE AND NONINFRINGEMENT.IN NO EVENT SHALL THE AUTHORS OR COPYRIGHT HOLDERS BE LIABLE FOR ANY CLAIM, DAMAGES OR OTHER LIABILITY, WHETHER IN AN ACTION OF CONTRACT, TORT OR OTHERWISE, ARISING FROM, OUT OF OR IN CONNECTION WITH THE SOFTWARE OR THE USE OR OTHER DEALINGS IN THE SOFTWARE." + Environment.NewLine);

            txtAbout.Text = text.ToString();
        }
    }
}
