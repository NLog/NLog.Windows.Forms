﻿using System;
using System.Collections.Generic;
using System.Text;
using NLog.Config;
using NLog.LayoutRenderers;
using NLog.Layouts;

namespace NLog.Windows.Forms
{
    /// <summary>
    /// Strings rendered with this rendrer would convert to links in the control. <see cref="RichTextBoxTarget.SupportLinks" />
    /// </summary>
    /// <remarks>
    /// Internally this renderer replaces the rendered text with a GUID and stores the info in <see cref="LogEventInfo.Properties" /> by <see cref="LinkInfo.PropertyName" /> as a key
    /// Actual rendering is done in <see cref="RichTextBoxTarget.SendTheMessageToRichTextBox" />
    /// </remarks>
    [LayoutRenderer("rtb-link")]
    public sealed class RichTextBoxLinkLayoutRenderer : LayoutRenderer
    {
        /// <summary>
        /// Inner layout that actually provides text
        /// </summary>
        [DefaultParameter]
        public Layout? Inner { get; set; }

        /// <summary>
        /// Implementation of a <see cref="LayoutRenderer.Append" />
        /// </summary>
        /// <param name="builder"></param>
        /// <param name="logEvent"></param>
        protected override void Append(StringBuilder builder, LogEventInfo logEvent)
        {
            var msg = Inner?.Render(logEvent);
            if (msg is null || string.IsNullOrEmpty(msg))
            {
                return;
            }

            //store new linkInfo to be retreived by RichTextBox
            LinkInfo? linkInfo;
            object? linkInfoObj;
            lock (logEvent.Properties)
            {
                if (logEvent.Properties.TryGetValue(LinkInfo.PropertyName, out linkInfoObj) && linkInfoObj is LinkInfo)
                {
                    linkInfo = (LinkInfo)linkInfoObj;
                }
                else
                {
                    linkInfo = new LinkInfo();
                    logEvent.Properties.Add(LinkInfo.PropertyName, linkInfo);
                }
            }

            var guid = Guid.NewGuid().ToString("P"); //(xxxxxxxx-xxxx-xxxx-xxxx-xxxxxxxxxxxx)
            linkInfo.Add(guid, msg);

            builder.Append(guid);
        }

        /// <summary>
        /// Inernal class storing the captured link info, used by <see cref="RichTextBoxTarget.SendTheMessageToRichTextBox" /> to convert the text to link and then identify the logEvent
        /// </summary>
        internal sealed class LinkInfo
        {
            /// <summary>
            /// Used as a key in <see cref="LogEventInfo.Properties" />
            /// </summary>
            internal const string PropertyName = "NLog.Windows.Forms.RichTextBoxLinkLayoutRenderer.LinkInfo";

            private readonly Dictionary<string, string> guidToLinkText = new Dictionary<string, string>();

            private readonly object lockObj = new object();

            internal void Add(string guid, string linkText)
            {
                lock (lockObj)
                {
                    guidToLinkText.Add(guid, linkText);
                }
            }

            internal string? GetValue(string guid)
            {
                lock (lockObj)
                {
                    guidToLinkText.TryGetValue(guid, out var result);
                    return result;
                }
            }
        }
    }
}