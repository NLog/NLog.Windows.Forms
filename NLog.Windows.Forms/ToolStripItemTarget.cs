using System;
using System.Windows.Forms;
using NLog.Common;
using NLog.Layouts;
using NLog.Targets;

namespace NLog.Windows.Forms
{
    /// <summary>
    /// Logs text to Windows.Forms.ToolStripItem.Text property control of specified Name.
    /// </summary>
    /// <remarks>
    /// <a href="https://github.com/NLog/NLog.Windows.Forms/wiki/ToolStripItemTarget">See NLog Wiki</a>
    /// </remarks>
    /// <seealso href="https://github.com/NLog/NLog.Windows.Forms/wiki/ToolStripItemTarget">Documentation on NLog Wiki</seealso>
    /// <example>
    /// <p>
    /// To set up the target in the <a href="config.html">configuration file</a>, 
    /// use the following syntax:
    /// </p>
    /// <code lang="XML" source="examples/targets/Configuration File/ToolStripItem/NLog.config" />
    /// <p>
    /// The result is:
    /// </p>
    /// <img src="examples/targets/Screenshots/ToolStripItem/ToolStripItem.gif" />
    /// <p>
    /// To set up the log target programmatically similar to above use code like this:
    /// </p>
    /// <code lang="C#" source="examples/targets/Configuration API/ToolStripItem/Form1.cs" />,
    /// </example>
    [Target("ToolStripItem")]
    public sealed class ToolStripItemTarget : TargetWithLayout
    {
        /// <summary>
        /// Initializes a new instance of the <see cref="ToolStripItemTarget" /> class.
        /// </summary>
        /// <remarks>
        /// The default value of the layout is: <code>${longdate}|${level:uppercase=true}|${logger}|${message:withException=true}</code>
        /// </remarks>
        public ToolStripItemTarget()
        {
        }

        private delegate void DelSendTheMessageToFormControl(ToolStripItem control, string logMessage);

        /// <summary>
        /// Gets or sets the name of the ToolStripItem to which NLog will log write log text.
        /// </summary>
        /// <docgen category='Form Options' order='10' />
        public Layout ItemName { get; set; } = Layout.Empty;

        /// <summary>
        /// Gets or sets the name of ToolStrip that contains the ToolStripItem to which NLog will log write log text.
        /// </summary>
        /// <docgen category='Form Options' order='10' />
        public Layout ToolStripName { get; set; } = Layout.Empty;

        /// <summary>
        /// Gets or sets the name of the Form on which the ToolStrip is located.
        /// </summary>
        /// <docgen category='Form Options' order='10' />
        public Layout? FormName { get; set; }

        /// <inheritdoc />
        protected override void InitializeTarget()
        {
            if (ItemName is null || ReferenceEquals(ItemName, Layout.Empty))
                throw new NLogConfigurationException("ToolStripItemTarget ItemName-property must be assigned.");
            if (ToolStripName is null || ReferenceEquals(ToolStripName, Layout.Empty))
                throw new NLogConfigurationException("ToolStripItemTarget ToolStripName-property must be assigned.");

            base.InitializeTarget();
        }

        /// <summary>
        /// Log message to item.
        /// </summary>
        /// <param name="logEvent">
        /// The logging event.
        /// </param>
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

            var toolStripName = RenderLogEvent(ToolStripName, logEvent);
            var toolStrip = FormHelper.FindControl(toolStripName, form) as ToolStrip;
            if (toolStrip is null)
            {
                if (string.IsNullOrEmpty(toolStripName))
                    toolStripName = ToolStripName?.ToString() ?? string.Empty;
                if (string.IsNullOrEmpty(formName))
                    formName = form.Name;
                InternalLogger.Info("ToolStrip {0} on Form {1} not found", toolStripName, formName);
                return;
            }

            var itemName = RenderLogEvent(ItemName, logEvent);
            var item = FormHelper.FindToolStripItem(itemName, toolStrip.Items);
            if (item == null)
            {
                InternalLogger.Info("ToolStripItem {0} on ToolStrip {1} not found", ItemName, ToolStripName);
                return;
            }

            try
            {
                toolStrip.BeginInvoke(new DelSendTheMessageToFormControl(SendTheMessageToFormControl), item, logMessage);
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

        private static void SendTheMessageToFormControl(ToolStripItem item, string logMessage)
        {
            item.Text = logMessage;
        }
    }
}
