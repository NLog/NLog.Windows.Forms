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

using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.ComponentModel;
using System.Drawing;
using System.Linq;
using System.Text.RegularExpressions;
using System.Windows.Forms;
using NLog.Config;
using NLog.Targets;

namespace NLog.Windows.Forms
{
    /// <summary>
    /// Log text a Rich Text Box control in an existing or new form.
    /// </summary>
    /// <seealso href="https://github.com/nlog/nlog/wiki/RichTextBox-target">Documentation on NLog Wiki</seealso>
    /// <example>
    /// <p>
    /// To set up the target in the <a href="config.html">configuration file</a>, 
    /// use the following syntax:
    /// </p><code lang="XML" source="examples/targets/Configuration File/RichTextBox/Simple/NLog.config">
    /// </code>
    /// <p>
    /// The result is:
    /// </p><img src="examples/targets/Screenshots/RichTextBox/Simple.gif"/><p>
    /// To set up the target with coloring rules in the <a href="config.html">configuration file</a>, 
    /// use the following syntax:
    /// </p><code lang="XML" source="examples/targets/Configuration File/RichTextBox/RowColoring/NLog.config">
    /// </code>
    /// <code lang="XML" source="examples/targets/Configuration File/RichTextBox/WordColoring/NLog.config">
    /// </code>
    /// <p>
    /// The result is:
    /// </p><img src="examples/targets/Screenshots/RichTextBox/RowColoring.gif"/><img src="examples/targets/Screenshots/RichTextBox/WordColoring.gif"/><p>
    /// To set up the log target programmatically similar to above use code like this:
    /// </p><code lang="C#" source="examples/targets/Configuration API/RichTextBox/Simple/Form1.cs">
    /// </code>
    /// ,
    /// <code lang="C#" source="examples/targets/Configuration API/RichTextBox/RowColoring/Form1.cs">
    /// </code>
    /// for RowColoring,
    /// <code lang="C#" source="examples/targets/Configuration API/RichTextBox/WordColoring/Form1.cs">
    /// </code>
    /// for WordColoring
    /// </example>
    [Target("RichTextBox")]
    public sealed class RichTextBoxTarget : TargetWithLayout
    {
        /// <summary>
        /// Initializes static members of the RichTextBoxTarget class.
        /// </summary>
        /// <remarks>
        /// The default value of the layout is: <code>${longdate}|${level:uppercase=true}|${logger}|${message}</code>
        /// </remarks>
        static RichTextBoxTarget()
        {
            var rules = new List<RichTextBoxRowColoringRule>()
            {
                new RichTextBoxRowColoringRule("level == LogLevel.Fatal", "White", "Red", FontStyle.Bold),
                new RichTextBoxRowColoringRule("level == LogLevel.Error", "Red", "Empty", FontStyle.Bold | FontStyle.Italic),
                new RichTextBoxRowColoringRule("level == LogLevel.Warn", "Orange", "Empty", FontStyle.Underline),
                new RichTextBoxRowColoringRule("level == LogLevel.Info", "Black", "Empty"),
                new RichTextBoxRowColoringRule("level == LogLevel.Debug", "Gray", "Empty"),
                new RichTextBoxRowColoringRule("level == LogLevel.Trace", "DarkGray", "Empty", FontStyle.Italic),
            };

            DefaultRowColoringRules = rules.AsReadOnly();
        }

        /// <summary>
        /// Initializes a new instance of the <see cref="RichTextBoxTarget" /> class.
        /// </summary>
        /// <remarks>
        /// The default value of the layout is: <code>${longdate}|${level:uppercase=true}|${logger}|${message}</code>
        /// </remarks>
        public RichTextBoxTarget()
        {
            WordColoringRules = new List<RichTextBoxWordColoringRule>();
            RowColoringRules = new List<RichTextBoxRowColoringRule>();
            ToolWindow = true;
        }

        private delegate void DelSendTheMessageToRichTextBox(string logMessage, RichTextBoxRowColoringRule rule);

        private delegate void FormCloseDelegate();

        /// <summary>
        /// Gets the default set of row coloring rules which applies when <see cref="UseDefaultRowColoringRules"/> is set to true.
        /// </summary>
        public static ReadOnlyCollection<RichTextBoxRowColoringRule> DefaultRowColoringRules { get; private set; }

        /// <summary>
        /// Gets or sets the Name of RichTextBox to which Nlog will write.
        /// </summary>
        /// <docgen category='Form Options' order='10' />
        public string ControlName { get; set; }

        /// <summary>
        /// Gets or sets the name of the Form on which the control is located. 
        /// If there is no open form of a specified name than NLog will create a new one.
        /// </summary>
        /// <docgen category='Form Options' order='10' />
        public string FormName { get; set; }

        /// <summary>
        /// Gets or sets a value indicating whether to use default coloring rules.
        /// </summary>
        /// <docgen category='Highlighting Options' order='10' />
        [DefaultValue(false)]
        public bool UseDefaultRowColoringRules { get; set; }

        /// <summary>
        /// Gets the row coloring rules.
        /// </summary>
        /// <docgen category='Highlighting Options' order='10' />
        [ArrayParameter(typeof(RichTextBoxRowColoringRule), "row-coloring")]
        public IList<RichTextBoxRowColoringRule> RowColoringRules { get; private set; }

        /// <summary>
        /// Gets the word highlighting rules.
        /// </summary>
        /// <docgen category='Highlighting Options' order='10' />
        [ArrayParameter(typeof(RichTextBoxWordColoringRule), "word-coloring")]
        public IList<RichTextBoxWordColoringRule> WordColoringRules { get; private set; }

        /// <summary>
        /// Gets or sets a value indicating whether the created window will be a tool window.
        /// </summary>
        /// <remarks>
        /// This parameter is ignored when logging to existing form control.
        /// Tool windows have thin border, and do not show up in the task bar.
        /// </remarks>
        /// <docgen category='Form Options' order='10' />
        [DefaultValue(true)]
        public bool ToolWindow { get; set; }

        /// <summary>
        /// Gets or sets a value indicating whether the created form will be initially minimized.
        /// </summary>
        /// <remarks>
        /// This parameter is ignored when logging to existing form control.
        /// </remarks>
        /// <docgen category='Form Options' order='10' />
        public bool ShowMinimized { get; set; }

        /// <summary>
        /// Gets or sets the initial width of the form with rich text box.
        /// </summary>
        /// <remarks>
        /// This parameter is ignored when logging to existing form control.
        /// </remarks>
        /// <docgen category='Form Options' order='10' />
        public int Width { get; set; }

        /// <summary>
        /// Gets or sets the initial height of the form with rich text box.
        /// </summary>
        /// <remarks>
        /// This parameter is ignored when logging to existing form control.
        /// </remarks>
        /// <docgen category='Form Options' order='10' />
        public int Height { get; set; }

        /// <summary>
        /// Gets or sets a value indicating whether scroll bar will be moved automatically to show most recent log entries.
        /// </summary>
        /// <docgen category='Form Options' order='10' />
        public bool AutoScroll { get; set; }

        /// <summary>
        /// Gets or sets the maximum number of lines the rich text box will store (or 0 to disable this feature).
        /// </summary>
        /// <remarks>
        /// After exceeding the maximum number, first line will be deleted. 
        /// </remarks>
        /// <docgen category='Form Options' order='10' />
        public int MaxLines { get; set; }

        /// <summary>
        /// Gets or sets the form to log to.
        /// </summary>
        public Form TargetForm { get; set; }

        /// <summary>
        /// Gets or sets the rich text box to log to.
        /// </summary>
        public RichTextBox TargetRichTextBox { get; set; }

        public bool CreatedForm { get; set; }

        /// <summary>
        /// Initializes the target. Can be used by inheriting classes
        /// to initialize logging.
        /// </summary>
        protected override void InitializeTarget()
        {
            if (FormName == null)
            {
                FormName = "NLogForm" + Guid.NewGuid().ToString("N");
            }

            var openFormByName = Application.OpenForms[FormName];
            if (openFormByName != null)
            {
                TargetForm = openFormByName;
                if (string.IsNullOrEmpty(ControlName))
                {
                    throw new NLogConfigurationException("Rich text box control name must be specified for " + GetType().Name + ".");
                }

                CreatedForm = false;
                TargetRichTextBox = FormHelper.FindControl<RichTextBox>(ControlName, TargetForm);

                if (TargetRichTextBox == null)
                {
                    throw new NLogConfigurationException("Rich text box control '" + ControlName + "' cannot be found on form '" + FormName + "'.");
                }
            }
            else
            {
                TargetForm = FormHelper.CreateForm(FormName, Width, Height, true, ShowMinimized, ToolWindow);
                TargetRichTextBox = FormHelper.CreateRichTextBox(ControlName, TargetForm);
                CreatedForm = true;
            }
        }

        /// <summary>
        /// Closes the target and releases any unmanaged resources.
        /// </summary>
        protected override void CloseTarget()
        {
            if (CreatedForm)
            {
                TargetForm.BeginInvoke((FormCloseDelegate)TargetForm.Close);
                TargetForm = null;
            }
        }

        /// <summary>
        /// Log message to RichTextBox.
        /// </summary>
        /// <param name="logEvent">The logging event.</param>
        protected override void Write(LogEventInfo logEvent)
        {
            var matchingRule = FindMatchingRule(logEvent);

            string logMessage = Layout.Render(logEvent);

            TargetRichTextBox.BeginInvoke(new DelSendTheMessageToRichTextBox(SendTheMessageToRichTextBox), logMessage, matchingRule);
        }

        /// <summary>
        /// Find first matching rule
        /// </summary>
        /// <param name="logEvent"></param>
        /// <returns></returns>
        private RichTextBoxRowColoringRule FindMatchingRule(LogEventInfo logEvent)
        {
            //custom rules first
            foreach (RichTextBoxRowColoringRule coloringRule in RowColoringRules)
            {
                if (coloringRule.CheckCondition(logEvent))
                {
                    return coloringRule;
                }
            }
            
            if (UseDefaultRowColoringRules)
            {
                foreach (RichTextBoxRowColoringRule coloringRule in DefaultRowColoringRules)
                {
                    if (coloringRule.CheckCondition(logEvent))
                    {
                        return coloringRule;
                    }
                }
            }

            return RichTextBoxRowColoringRule.Default;
        }

        private static Color GetColorFromString(string color, Color defaultColor)
        {
            if (color == "Empty")
            {
                return defaultColor;
            }

            return Color.FromName(color);
        }

        private void SendTheMessageToRichTextBox(string logMessage, RichTextBoxRowColoringRule rule)
        {
            RichTextBox textBox = TargetRichTextBox;

            int startIndex = textBox.Text.Length;
            textBox.SelectionStart = startIndex;
            textBox.SelectionBackColor = GetColorFromString(rule.BackgroundColor, textBox.BackColor);
            textBox.SelectionColor = GetColorFromString(rule.FontColor, textBox.ForeColor);
            textBox.SelectionFont = new Font(textBox.SelectionFont, textBox.SelectionFont.Style ^ rule.Style);
            textBox.AppendText(logMessage + "\n");
            textBox.SelectionLength = textBox.Text.Length - textBox.SelectionStart;

            // find word to color
            foreach (RichTextBoxWordColoringRule wordRule in WordColoringRules)
            {
                MatchCollection matches = wordRule.CompiledRegex.Matches(textBox.Text, startIndex);
                foreach (Match match in matches)
                {
                    textBox.SelectionStart = match.Index;
                    textBox.SelectionLength = match.Length;
                    textBox.SelectionBackColor = GetColorFromString(wordRule.BackgroundColor, textBox.BackColor);
                    textBox.SelectionColor = GetColorFromString(wordRule.FontColor, textBox.ForeColor);
                    textBox.SelectionFont = new Font(textBox.SelectionFont, textBox.SelectionFont.Style ^ wordRule.Style);
                }
            }

            //remove some lines if there above the max
            if (MaxLines > 0)
            {
                //find the last line by reading the textbox
                var lastLineWithContent = textBox.Lines.LastOrDefault(f => !string.IsNullOrEmpty(f));
                if (lastLineWithContent != null)
                {
                    char lastChar = lastLineWithContent.Last();
                    var visibleLineCount = textBox.GetLineFromCharIndex(textBox.Text.LastIndexOf(lastChar));
                    var tooManyLines = (visibleLineCount - MaxLines) + 1;
                    if (tooManyLines > 0)
                    {
                        textBox.SelectionStart = 0;
                        textBox.SelectionLength = textBox.GetFirstCharIndexFromLine(tooManyLines);
                        textBox.SelectedRtf = "{\\rtf1\\ansi}";
                    }
                }
            }

            if (AutoScroll)
            {
                textBox.Select(textBox.TextLength, 0);
                textBox.ScrollToCaret();
            }
        }
    }
}