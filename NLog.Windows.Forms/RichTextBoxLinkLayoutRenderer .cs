using NLog.Config;
using NLog.LayoutRenderers;
using NLog.Layouts;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading;

namespace NLog.Windows.Forms
{
    /// <summary>
    /// Strings rendered with this rendrer would convert to links in the control. <see cref="RichTextBoxTarget.SupportLinks"/>
    /// </summary>
    [LayoutRenderer("rtb-link")]
    public sealed class RichTextBoxLinkLayoutRenderer : LayoutRenderer
    {
        private static int s_id = 0;

        /// <summary>
        /// Inner layout that actually provides text
        /// </summary>
        [DefaultParameter]
        public Layout Inner { get; set; }

        /// <summary>
        /// Implementation of a <see cref="LayoutRenderer.Append"/>
        /// </summary>
        /// <param name="builder"></param>
        /// <param name="logEvent"></param>
        protected override void Append(StringBuilder builder, LogEventInfo logEvent)
        {
            string msg = this.Inner.Render(logEvent);

            //store new linkInfo to be retreived by RichTextBox
            int id = Interlocked.Increment(ref s_id);
            LinkInfo linkInfo = new LinkInfo() {
                id = id,
                offset = builder.Length,
                length = msg.Length
            };
            logEvent.Properties[LinkInfo.PropertyName] = linkInfo;

            builder.Append(msg);
        }

        /// <summary>
        /// Inernal class storing the captured link info, used by <see cref="RichTextBoxTarget.SendTheMessageToRichTextBox"/> to convert the text to link and then identify the logEvent
        /// </summary>
        internal sealed class LinkInfo
        {
            internal const string PropertyName = "NLog.Windows.Forms.RichTextBoxLinkLayoutRenderer.LinkInfo";

            internal int id { get; set; }
            internal int offset { get; set; }
            internal int length { get; set; }
        }
    }
}
