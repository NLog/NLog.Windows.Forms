﻿using System;
using System.Windows.Forms;
using NLog.Common;
using NLog.Config;
using NLog.Layouts;
using NLog.Targets;

namespace NLog.Windows.Forms.Targets
{
    /// <summary>
    /// Logs text to Windows.Forms.ToolStripItem.Text property control of specified Name.
    /// </summary>
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
        /// The default value of the layout is: <code>${longdate}|${level:uppercase=true}|${logger}|${message}</code>
        /// </remarks>
        public ToolStripItemTarget()
        {
        }

        private delegate void DelSendTheMessageToFormControl(ToolStripItem control, string logMessage);

        /// <summary>
        /// Gets or sets the name of the ToolStripItem to which NLog will log write log text.
        /// </summary>
        /// <docgen category='Form Options' order='10' />
        [RequiredParameter]
        public Layout ItemName { get; set; }

        /// <summary>
        /// Gets or sets the name of ToolStrip that contains the ToolStripItem to which NLog will log write log text.
        /// </summary>
        /// <docgen category='Form Options' order='10' />
        [RequiredParameter]
        public Layout ToolStripName { get; set; }

        /// <summary>
        /// Gets or sets the name of the Form on which the ToolStrip is located.
        /// </summary>
        /// <docgen category='Form Options' order='10' />
        public Layout FormName { get; set; }

        /// <summary>
        /// Log message to item.
        /// </summary>
        /// <param name="logEvent">
        /// The logging event.
        /// </param>
        protected override void Write(LogEventInfo logEvent)
        {
            string logMessage = RenderLogEvent(Layout, logEvent);

            Form form = null;

            if (Form.ActiveForm != null)
            {
                form = Form.ActiveForm;
            }

            string renderedFormName = RenderLogEvent(FormName, logEvent);
            if (Application.OpenForms[renderedFormName] != null)
            {
                form = Application.OpenForms[renderedFormName];
            }

            if (form == null)
            {
                InternalLogger.Info("Form {0} not found", FormName);
                return;
            }

            Control control = FormHelper.FindControl(RenderLogEvent(ToolStripName, logEvent), form);

            if (control == null || !(control is ToolStrip))
            {
                InternalLogger.Info("ToolStrip {0} on Form {1} not found", ToolStripName, FormName);
                return;
            }

            ToolStrip toolStrip = control as ToolStrip;

            ToolStripItem item = FormHelper.FindToolStripItem(RenderLogEvent(ItemName, logEvent), toolStrip.Items);

            if (item == null)
            {
                InternalLogger.Info("ToolStripItem {0} on ToolStrip {1} not found", ItemName, ToolStripName);
                return;
            }

            try
            {
                control.BeginInvoke(new DelSendTheMessageToFormControl(SendTheMessageToFormControl), item, logMessage);
                
            }
            catch (Exception ex)
            {
                InternalLogger.Warn(ex.ToString());

                if (LogManager.ThrowExceptions)
                {
                    throw;
                }
            }
        }
        private void SendTheMessageToFormControl(ToolStripItem item, string logMessage)
        {
            item.Text = logMessage;
        }
    }
}
