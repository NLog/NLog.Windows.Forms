using System.Drawing;
using System.Text.RegularExpressions;
using NLog.Config;
using NLog.Layouts;

namespace NLog.Windows.Forms
{
    /// <summary>
    /// Highlighting rule for Win32 colorful console.
    /// 
    /// </summary>
    [NLogConfigurationItem]
    public class RichTextBoxWordColoringRule
    {
        /// <summary>
        /// Gets or sets the regular expression to be matched. You must specify either <c>text</c> or <c>regex</c>.
        /// 
        /// </summary>
        /// <docgen category="Rule Matching Options" order="10"/>
        public Layout Regex { get; set; }

        /// <summary>
        /// Gets or sets the text to be matched. You must specify either <c>text</c> or <c>regex</c>.
        /// 
        /// </summary>
        /// <docgen category="Rule Matching Options" order="10"/>
        public Layout Text { get; set; }

        /// <summary>
        /// Gets or sets a value indicating whether to match whole words only.
        /// 
        /// </summary>
        /// <docgen category="Rule Matching Options" order="10"/>
        public Layout<bool> WholeWords { get; set; }

        /// <summary>
        /// Gets or sets a value indicating whether to ignore case when comparing texts.
        /// 
        /// </summary>
        /// <docgen category="Rule Matching Options" order="10"/>
        public Layout<bool> IgnoreCase { get; set; }

        /// <summary>
        /// Gets or sets the font style of matched text.
        ///             Possible values are the same as in <c>FontStyle</c> enum in <c>System.Drawing</c>.
        /// 
        /// </summary>
        /// <docgen category="Formatting Options" order="10"/>
        public FontStyle Style { get; set; }

        /// <summary>
        /// Gets the compiled regular expression that matches either Text or Regex property.
        /// </summary>
        internal Regex ResolveRegEx(string pattern, string text, bool wholeWords, bool ignoreCase)
        {
            if (string.IsNullOrEmpty(pattern) && text != null)
            {
                pattern = System.Text.RegularExpressions.Regex.Escape(text);
                if (wholeWords)
                    pattern = "\b" + pattern + "\b";
            }

            RegexOptions options = RegexOptions.None;
            if (ignoreCase)
                options |= RegexOptions.IgnoreCase;

            return new Regex(pattern, options);   // RegEx-Cache
        }

        /// <summary>
        /// Gets or sets the font color.
        ///             Names are identical with KnownColor enum extended with Empty value which means that font color won't be changed.
        /// 
        /// </summary>
        /// <docgen category="Formatting Options" order="10"/>
        public Layout FontColor { get; set; }

        /// <summary>
        /// Gets or sets the background color.
        ///             Names are identical with KnownColor enum extended with Empty value which means that background color won't be changed.
        /// 
        /// </summary>
        /// <docgen category="Formatting Options" order="10"/>
        public Layout BackgroundColor { get; set; }

        /// <summary>
        /// Initializes a new instance of the <see cref="T:NLog.Targets.RichTextBoxWordColoringRule"/> class.
        /// 
        /// </summary>
        public RichTextBoxWordColoringRule()
        {
            this.FontColor = "Empty";
            this.BackgroundColor = "Empty";
        }

        /// <summary>
        /// Initializes a new instance of the <see cref="T:NLog.Targets.RichTextBoxWordColoringRule"/> class.
        /// 
        /// </summary>
        /// <param name="text">The text to be matched..</param><param name="fontColor">Color of the text.</param><param name="backgroundColor">Color of the background.</param>
        public RichTextBoxWordColoringRule(string text, string fontColor, string backgroundColor)
        {
            this.Text = text;
            this.FontColor = Layout.FromString(fontColor);
            this.BackgroundColor = Layout.FromString(backgroundColor);
        }

        /// <summary>
        /// Initializes a new instance of the <see cref="T:NLog.Targets.RichTextBoxWordColoringRule"/> class.
        /// 
        /// </summary>
        /// <param name="text">The text to be matched..</param><param name="textColor">Color of the text.</param><param name="backgroundColor">Color of the background.</param><param name="fontStyle">The font style.</param>
        public RichTextBoxWordColoringRule(string text, string textColor, string backgroundColor, FontStyle fontStyle)
        {
            this.Text = text;
            this.FontColor = Layout.FromString(textColor);
            this.BackgroundColor = Layout.FromString(backgroundColor);
            this.Style = fontStyle;
        }
    }
}
