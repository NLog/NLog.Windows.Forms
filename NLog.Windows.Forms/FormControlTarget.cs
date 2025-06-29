using System;
using System.Windows.Forms;
using NLog.Common;
using NLog.Layouts;
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
        /// The default value of the layout is: <code>${longdate}|${level:uppercase=true}|${logger}|${message:withException=true}</code>
        /// </remarks>
        public FormControlTarget()
        {
        }

        private delegate void DelSendTheMessageToFormControl(Control control, string logMessage, bool append, bool reverseOrder);

        /// <summary>
        /// Gets or sets the name of control to which NLog will log write log text.
        /// </summary>
        /// <docgen category='Form Options' order='10' />
        public Layout ControlName { get; set; } = Layout.Empty;

        /// <summary>
        /// Gets or sets a value indicating whether log text should be appended to the text of the control instead of overwriting it. </summary>
        /// <docgen category='Form Options' order='10' />
        public Layout<bool> Append { get; set; } = true;

        /// <summary>
        /// Gets or sets the name of the Form on which the control is located.
        /// </summary>
        /// <docgen category='Form Options' order='10' />
        public Layout? FormName { get; set; }

        /// <summary>
        /// Gets or sets whether new log entry are added to the start or the end of the control
        /// </summary>
        public Layout<bool> ReverseOrder { get; set; } = false;

        /// <inheritdoc />
        protected override void InitializeTarget()
        {
            if (ControlName is null || ReferenceEquals(ControlName, Layout.Empty))
                throw new NLogConfigurationException("FormControlTarget ControlName-property must be assigned.");

            base.InitializeTarget();
        }

        /// <summary>
        /// Log message to control.
        /// </summary>
        /// <param name="logEvent">The logging event.</param>
        protected override void Write(LogEventInfo logEvent)
        {
            string logMessage = RenderLogEvent(Layout, logEvent);

            Form? form = null;

            if (Form.ActiveForm != null)
            {
                form = Form.ActiveForm;
            }

            string formName = RenderLogEvent(FormName, logEvent);
            if (Application.OpenForms[formName] != null)
            {
                form = Application.OpenForms[formName];
            }

            if (form is null)
            {
                if (string.IsNullOrEmpty(formName))
                    formName = FormName?.ToString() ?? string.Empty;
                InternalLogger.Info("Form {0} not found", formName);
                return;
            }

            var controlName = RenderLogEvent(ControlName, logEvent);
            var control = FormHelper.FindControl(controlName, form);
            if (control is null)
            {
                if (string.IsNullOrEmpty(controlName))
                    controlName = ControlName?.ToString() ?? string.Empty;
                if (string.IsNullOrEmpty(formName))
                    formName = form.Name;
                InternalLogger.Info("Control {0} on Form {1} not found", controlName, formName);
                return;
            }

            try
            {
                bool append = RenderLogEvent(Append, logEvent);
                bool reverseOrder = RenderLogEvent(ReverseOrder, logEvent);
                control.BeginInvoke(new DelSendTheMessageToFormControl(SendTheMessageToFormControl), control, logMessage, append, reverseOrder);
            }
            catch (Exception ex)
            {
                InternalLogger.Warn(ex, "Failed to assign Control.Text");

                if (LogManager.ThrowExceptions)
                {
                    throw;
                }
            }
        }

        private static void SendTheMessageToFormControl(Control control, string logMessage, bool append, bool reverseOrder)
        {
            //append of replace?
            if (append)
            {
                //beginning or end?
                if (reverseOrder)
                    control.Text = logMessage + control.Text;
                else
                    control.Text += logMessage;
            }
            else
            {
                control.Text = logMessage;
            }
        }
    }
}
