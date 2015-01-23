// Copyright 2013 Kim Christensen, et. al.
//  
// Licensed under the Apache License, Version 2.0 (the "License"); you may not use 
// this file except in compliance with the License. You may obtain a copy of the 
// License at 
// 
//     http://www.apache.org/licenses/LICENSE-2.0 
// 
// Unless required by applicable law or agreed to in writing, software distributed 
// under the License is distributed on an "AS IS" BASIS, WITHOUT WARRANTIES OR 
// CONDITIONS OF ANY KIND, either express or implied. See the License for the 
// specific language governing permissions and limitations under the License.

using System.ComponentModel;
using System.Windows.Forms;
using NLog.Config;
using NLog.Targets;

namespace NLog.Windows.Forms
{
    /// <summary>
    /// Logs text to Windows.Forms.Control.Text property control of specified Name.
    /// </summary>
    /// <example>
    /// <p>
    /// To set up the target in the <a href="config.html">configuration file</a>, 
    /// use the following syntax:
    /// </p>
    /// <code lang="XML" source="examples/targets/Configuration File/FormControl/NLog.config" />
    /// <p>
    /// The result is:
    /// </p>
    /// <img src="examples/targets/Screenshots/FormControl/FormControl.gif" />
    /// <p>
    /// To set up the log target programmatically similar to above use code like this:
    /// </p>
    /// <code lang="C#" source="examples/targets/Configuration API/FormControl/Form1.cs" />,
    /// </example>
    [Target("FormControl")]
    public sealed class FormControlTarget : TargetWithLayout
    {
        /// <summary>
        /// Initializes a new instance of the <see cref="FormControlTarget" /> class.
        /// </summary>
        /// <remarks>
        /// The default value of the layout is: <code>${longdate}|${level:uppercase=true}|${logger}|${message}</code>
        /// </remarks>
        public FormControlTarget()
        {
            Append = true;
        }

        private delegate void DelSendTheMessageToFormControl(Control ctrl, string logMessage);

        /// <summary>
        /// Gets or sets the name of control to which NLog will log write log text.
        /// </summary>
        /// <docgen category='Form Options' order='10' />
        [RequiredParameter]
        public string ControlName { get; set; }

        /// <summary>
        /// Gets or sets a value indicating whether log text should be appended to the text of the control instead of overwriting it. </summary>
        /// <docgen category='Form Options' order='10' />
        [DefaultValue(true)]
        public bool Append { get; set; }

        /// <summary>
        /// Gets or sets the name of the Form on which the control is located.
        /// </summary>
        /// <docgen category='Form Options' order='10' />
        public string FormName { get; set; }

        /// <summary>
        /// Gets or sets whether new log entry are added to the start or the end of the control
        /// </summary>
        public bool ReverseOrder { get; set; }

        /// <summary>
        /// Log message to control.
        /// </summary>
        /// <param name="logEvent">
        /// The logging event.
        /// </param>
        protected override void Write(LogEventInfo logEvent)
        {
            string logMessage = Layout.Render(logEvent);

            FindControlAndSendTheMessage(logMessage);
        }

        private void FindControlAndSendTheMessage(string logMessage)
        {
            Form form = null;

            if (Form.ActiveForm != null)
            {
                form = Form.ActiveForm;
            }

            if (Application.OpenForms[FormName] != null)
            {
                form = Application.OpenForms[FormName];
            }

            if (form == null)
            {
                return;
            }

            Control ctrl = FormHelper.FindControl(ControlName, form);

            if (ctrl == null)
            {
                return;
            }

            ctrl.BeginInvoke(new DelSendTheMessageToFormControl(SendTheMessageToFormControl), ctrl, logMessage);
        }

        private void SendTheMessageToFormControl(Control ctrl, string logMessage)
        {
            if (Append)
            {
                if (ReverseOrder)
                    ctrl.Text = logMessage + ctrl.Text;
                else
                    ctrl.Text += logMessage;
            }
            else
            {
                ctrl.Text = logMessage;
            }
        }
    }
}
