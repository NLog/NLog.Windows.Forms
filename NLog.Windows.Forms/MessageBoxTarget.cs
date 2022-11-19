using System;
using System.Collections.Generic;
using System.Text;
using System.Windows.Forms;
using NLog.Common;
using NLog.Layouts;
using NLog.Targets;

namespace NLog.Windows.Forms
{
    /// <summary>
    /// Pops up log messages as message boxes.
    /// </summary>
    /// <seealso href="https://github.com/nlog/nlog/wiki/MessageBox-target">Documentation on NLog Wiki</seealso>
    /// <example>
    /// <p>
    /// To set up the target in the <a href="config.html">configuration file</a>, 
    /// use the following syntax:
    /// </p>
    /// <code lang="XML" source="examples/targets/Configuration File/MessageBox/NLog.config" />
    /// <p>
    /// This assumes just one target and a single rule. More configuration
    /// options are described <a href="config.html">here</a>.
    /// </p>
    /// <p>
    /// The result is a message box:
    /// </p>
    /// <img src="examples/targets/Screenshots/MessageBox/MessageBoxTarget.gif" />
    /// <p>
    /// To set up the log target programmatically use code like this:
    /// </p>
    /// <code lang="C#" source="examples/targets/Configuration API/MessageBox/Simple/Example.cs" />
    /// </example>
    [Target("MessageBox")]
    public sealed class MessageBoxTarget : TargetWithLayout
    {
        /// <summary>
        /// Initializes a new instance of the <see cref="MessageBoxTarget" /> class.
        /// </summary>
        /// <remarks>
        /// The default value of the layout is: <code>${longdate}|${level:uppercase=true}|${logger}|${message:withException=true}</code>
        /// </remarks>
        public MessageBoxTarget()
        {
            Caption = "NLog";
        }

        /// <summary>
        /// Gets or sets the message box title.
        /// </summary>
        /// <docgen category='UI Options' order='10' />
        public Layout Caption
        {
            get;
            set;
        }

        /// <summary>
        /// Displays the message box with the log message and caption specified in the Caption
        /// parameter.
        /// </summary>
        /// <param name="logEvent">The logging event.</param>
        [System.Diagnostics.CodeAnalysis.SuppressMessage("Microsoft.Globalization", "CA1300:SpecifyMessageBoxOptions",
            Justification = "This is just debugging output.")]
        protected override void Write(LogEventInfo logEvent)
        {
            try
            {
                MessageBox.Show(RenderLogEvent(Layout, logEvent), RenderLogEvent(Caption, logEvent));
            }
            catch (Exception ex)
            {
                InternalLogger.Warn(ex, "Failed MessageBox.Show");

                if (LogManager.ThrowExceptions)
                {
                    throw;
                }
            }
        }

        /// <summary>
        /// Displays the message box with the array of rendered logs messages and caption specified in the Caption
        /// parameter.
        /// </summary>
        /// <param name="logEvents">The array of logging events.</param>
        [System.Diagnostics.CodeAnalysis.SuppressMessage("Microsoft.Globalization", "CA1300:SpecifyMessageBoxOptions",
    Justification = "This is just debugging output.")]
        protected override void Write(IList<AsyncLogEventInfo> logEvents)
        {
            if (logEvents.Count == 0)
            {
                return;
            }

            var sb = new StringBuilder();
            var lastLogEvent = logEvents[logEvents.Count - 1];
            foreach (var ev in logEvents)
            {
                sb.Append(RenderLogEvent(Layout, ev.LogEvent));
                sb.Append("\n");
            }

            MessageBox.Show(sb.ToString(), RenderLogEvent(Caption, lastLogEvent.LogEvent));

            for (int i = 0; i < logEvents.Count; ++i)
            {
                logEvents[i].Continuation(null);
            }
        }
    }
}