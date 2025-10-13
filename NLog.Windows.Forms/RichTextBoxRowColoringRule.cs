using System;
using System.Drawing;
using NLog.Conditions;
using NLog.Config;
using NLog.Layouts;

namespace NLog.Windows.Forms
{
    /// <summary>
    /// The row-coloring condition.
    /// </summary>
    [NLogConfigurationItem]
    public class RichTextBoxRowColoringRule
    {
        private static readonly Layout EmptyColor = Layout.FromLiteral("Empty");

        /// <summary>
        /// Gets the default highlighting rule. Doesn't change the color.
        /// </summary>
        /// <docgen category="Rule Matching Options" order="10"/>
        public static RichTextBoxRowColoringRule Default { get; } = new RichTextBoxRowColoringRule();

        /// <summary>
        /// Gets or sets the condition that must be met in order to set the specified font color.
        /// </summary>
        /// <docgen category="Rule Matching Options" order="10"/>
        public ConditionExpression? Condition { get; set; }

        /// <summary>
        /// Gets or sets the font color.
        /// </summary>
        /// <remarks>
        /// Names are identical with KnownColor enum extended with Empty value which means that background color won't be changed.
        /// </remarks>
        /// <docgen category="Formatting Options" order="10"/>
        public Layout FontColor { get; set; } = EmptyColor;

        /// <summary>
        /// Gets or sets the background color.
        /// </summary>
        /// <remarks>
        /// Names are identical with KnownColor enum extended with Empty value which means that background color won't be changed.
        /// </remarks>
        /// <docgen category="Formatting Options" order="10"/>
        public Layout BackgroundColor { get; set; } = EmptyColor;

        /// <summary>
        /// Gets or sets the font style of matched text.
        /// </summary>
        /// <remarks>
        /// Possible values are the same as in <c>FontStyle</c> enum in <c>System.Drawing</c>
        /// </remarks>
        /// <docgen category="Formatting Options" order="10"/>
        public FontStyle Style { get; set; } = FontStyle.Regular;

        /// <summary>
        /// Initializes a new instance of the <see cref="T:NLog.Targets.RichTextBoxRowColoringRule"/> class.
        /// </summary>
        public RichTextBoxRowColoringRule()
        {
        }

        /// <summary>
        /// Initializes a new instance of the <see cref="T:NLog.Targets.RichTextBoxRowColoringRule"/> class.
        /// </summary>
        /// <param name="condition">The condition.</param>
        /// <param name="fontColor">Color of the foreground text.</param>
        /// <param name="backColor">Color of the background text.</param>
        /// <param name="fontStyle">The font style.</param>
        [Obsolete("Instead use default constructor and assign relevant properties. Marked obsolete with NLog v6")]
        public RichTextBoxRowColoringRule(string condition, string fontColor, string backColor, FontStyle fontStyle)
        {
            this.Condition = condition;
            this.FontColor = Layout.FromString(fontColor);
            this.BackgroundColor = Layout.FromString(backColor);
            this.Style = fontStyle;
        }

        /// <summary>
        /// Initializes a new instance of the <see cref="T:NLog.Targets.RichTextBoxRowColoringRule"/> class.
        /// </summary>
        /// <param name="condition">The condition.</param>
        /// <param name="fontColor">Color of the text.</param>
        /// <param name="backColor">Color of the background.</param>
        [Obsolete("Instead use default constructor and assign relevant properties. Marked obsolete with NLog v6")]
        public RichTextBoxRowColoringRule(string condition, string fontColor, string backColor)
        {
            this.Condition = condition;
            this.FontColor = Layout.FromString(fontColor);
            this.BackgroundColor = Layout.FromString(backColor);
            this.Style = FontStyle.Regular;
        }

        /// <summary>
        /// Checks whether the specified log event matches the condition.
        /// </summary>
        /// <param name="logEvent">Log event.</param>
        /// <returns>
        /// A value of <see langword="true"/> if the condition matches, <see langword="false"/> otherwise.
        /// </returns>
        public bool CheckCondition(LogEventInfo logEvent)
        {
            return true.Equals(this.Condition?.Evaluate(logEvent));
        }
    }
}
