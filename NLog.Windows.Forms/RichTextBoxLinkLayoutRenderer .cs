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

            //TODO: should we synchronize access to logEvent.Properties??
            LinkInfo linkInfo;
            object linkInfoObj;
            if (logEvent.Properties.TryGetValue(LinkInfo.PropertyName, out linkInfoObj))
            {
                linkInfo = (LinkInfo)linkInfoObj;
            }
            else
            {
                linkInfo = new LinkInfo();
                logEvent.Properties.Add(LinkInfo.PropertyName, linkInfo);
            }

            string guid = Guid.NewGuid().ToString("P"); //(xxxxxxxx-xxxx-xxxx-xxxx-xxxxxxxxxxxx)
            linkInfo.Add(guid, msg);

            builder.Append(guid);
        }

        /// <summary>
        /// Inernal class storing the captured link info, used by <see cref="RichTextBoxTarget.SendTheMessageToRichTextBox"/> to convert the text to link and then identify the logEvent
        /// </summary>
        internal sealed class LinkInfo
        {
            internal const string PropertyName = "NLog.Windows.Forms.RichTextBoxLinkLayoutRenderer.LinkInfo";

            private readonly object m_lock = new object();
            private readonly Dictionary<string, string> m_guidToLinkText = new Dictionary<string, string>();

            internal void Add(string guid, string linkText)
            {
                lock (m_lock)
                {
                    m_guidToLinkText.Add(guid, linkText);
                }
            }

            internal string GetValue(string guid)
            {
                string result = null;
                lock (m_lock)
                {
                    m_guidToLinkText.TryGetValue(guid, out result);
                }
                return result;
            }
        }
    }
}
