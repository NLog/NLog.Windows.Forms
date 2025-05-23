using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Drawing;
using System.Linq;
using System.Text.RegularExpressions;
using System.Windows.Forms;
using NLog.Common;
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
        /// Attempts to attach existing targets that have yet no textboxes to controls that exist on specified form if appropriate
        /// </summary>
        /// <remarks>
        /// Setting <see cref="AllowAccessoryFormCreation"/> to true (default) actually causes target to always have a text box 
        /// (after having <see cref="InitializeTarget"/> called), so such targets are not affected by this method.
        /// </remarks>
        /// <param name="form">a Form to check for RichTextBoxes</param>
        public static void ReInitializeAllTextboxes(Form form)
        {
            InternalLogger.Info("Executing ReInitializeAllTextboxes for Form {0}", form);
            var configuration = LogManager.Configuration;

            if (configuration == null)
            {
                throw new NLogConfigurationException("NLog configuration is empty");
            }

            ReInitializeAllTextboxes(form, configuration);
        }

        /// <summary>
        /// Attempts to attach existing targets that have yet no textboxes to controls that exist on specified form if appropriate
        /// </summary>
        /// <remarks>
        /// Setting <see cref="AllowAccessoryFormCreation"/> to true (default) actually causes target to always have a text box 
        /// (after having <see cref="InitializeTarget"/> called), so such targets are not affected by this method.
        /// </remarks>
        /// <param name="form">a Form to check for RichTextBoxes</param>
        /// /// <param name="configuration">NLog's configuration. <c>null</c> is not allowed</param>
        public static void ReInitializeAllTextboxes(Form form, LoggingConfiguration configuration)
        {
            if (configuration == null)
            {
                throw new ArgumentNullException(nameof(configuration), "NLog configuration is empty");
            }

            foreach (var target in GetRichTextBoxTargets(configuration))
            {
                if (target.FormName == form.Name)
                {
                    //can't use InitializeTarget here as the Application.OpenForms would not work from Form's constructor
                    RichTextBox textBoxControl = FormHelper.FindControl<RichTextBox>(target.ControlName, form);
                    if (textBoxControl != null && !textBoxControl.IsDisposed)
                    {
                        if (target.TargetRichTextBox == null
                            || target.TargetRichTextBox.IsDisposed
                            || target.TargetRichTextBox != textBoxControl
                        )
                        {
                            target.AttachToControl(form, textBoxControl);
                        }
                    }
                }
            }
        }

        /// <summary>
        /// Returns a target attached to a given RichTextBox control
        /// </summary>
        /// <param name="control">a RichTextBox control for which the target is to be returned</param>
        /// <returns>A RichTextBoxTarget attached to a given control or <code>null</code> if no target is attached</returns>
        public static RichTextBoxTarget GetTargetByControl(RichTextBox control)
        {
            var configuration = LogManager.Configuration;

            if (configuration == null)
            {
                throw new NLogConfigurationException("NLog configuration is empty");
            }

            return GetTargetByControl(control, configuration);
        }

        /// <summary>
        /// Returns a target attached to a given RichTextBox control
        /// </summary>
        /// <param name="control">a RichTextBox control for which the target is to be returned</param>
        /// <returns>A RichTextBoxTarget attached to a given control or <code>null</code> if no target is attached</returns>
        /// /// <param name="configuration">NLog's configuration. <c>null</c> is not allowed</param>
        public static RichTextBoxTarget GetTargetByControl(RichTextBox control, LoggingConfiguration configuration)
        {
            if (configuration == null)
            {
                throw new ArgumentNullException(nameof(configuration), "NLog configuration is empty");
            }

            foreach (var target in GetRichTextBoxTargets(configuration))
            {
                if (target != null && target.TargetRichTextBox == control)
                {
                    return target;
                }
            }

            return null;
        }

        private static IEnumerable<RichTextBoxTarget> GetRichTextBoxTargets(LoggingConfiguration loggingConfiguration)
        {
            return loggingConfiguration.AllTargets.OfType<RichTextBoxTarget>();
        }

        private static ReadOnlyCollection<RichTextBoxRowColoringRule> CreateDefaultColoringRules()
        {
            return new List<RichTextBoxRowColoringRule>()
            {
                new RichTextBoxRowColoringRule("level == LogLevel.Fatal", "White", "Red", FontStyle.Bold),
                new RichTextBoxRowColoringRule("level == LogLevel.Error", "Red", "Empty", FontStyle.Bold | FontStyle.Italic),
                new RichTextBoxRowColoringRule("level == LogLevel.Warn", "Orange", "Empty", FontStyle.Underline),
                new RichTextBoxRowColoringRule("level == LogLevel.Info", "Black", "Empty"),
                new RichTextBoxRowColoringRule("level == LogLevel.Debug", "Gray", "Empty"),
                new RichTextBoxRowColoringRule("level == LogLevel.Trace", "DarkGray", "Empty", FontStyle.Italic),
            }.AsReadOnly();
        }

        /// <summary>
        /// Initializes a new instance of the <see cref="RichTextBoxTarget" /> class.
        /// </summary>
        /// <remarks>
        /// The default value of the layout is: <code>${longdate}|${level:uppercase=true}|${logger}|${message:withException=true}</code>
        /// </remarks>
        public RichTextBoxTarget()
        {
            _sendTheMessageToRichTextBox = new DelSendTheMessageToRichTextBox((txtbox, msg, rule, evt) =>
            {
                try
                {
                    SendTheMessageToRichTextBox(txtbox, msg, rule, evt);
                }
                catch (Exception ex)
                {
                    if (NLog.LogManager.ThrowExceptions)
                        throw;

                    InternalLogger.Warn(ex, "{0}: Failed to append RichTextBox", this);
                }
            });
        }


        private delegate void DelSendTheMessageToRichTextBox(RichTextBox textBox, string logMessage, RichTextBoxRowColoringRule rule, LogEventInfo logEvent);

        private readonly DelSendTheMessageToRichTextBox _sendTheMessageToRichTextBox;

        private delegate void FormCloseDelegate();

        /// <summary>
        /// Gets the default set of row coloring rules which applies when <see cref="UseDefaultRowColoringRules"/> is set to true.
        /// </summary>
        public static ReadOnlyCollection<RichTextBoxRowColoringRule> DefaultRowColoringRules { get; } = CreateDefaultColoringRules();

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
        public bool UseDefaultRowColoringRules { get; set; }

        /// <summary>
        /// Gets the row coloring rules.
        /// </summary>
        /// <docgen category='Highlighting Options' order='10' />
        [ArrayParameter(typeof(RichTextBoxRowColoringRule), "row-coloring")]
        public IList<RichTextBoxRowColoringRule> RowColoringRules { get; } = new List<RichTextBoxRowColoringRule>();

        /// <summary>
        /// Gets the word highlighting rules.
        /// </summary>
        /// <docgen category='Highlighting Options' order='10' />
        [ArrayParameter(typeof(RichTextBoxWordColoringRule), "word-coloring")]
        public IList<RichTextBoxWordColoringRule> WordColoringRules { get; } = new List<RichTextBoxWordColoringRule>();

        /// <summary>
        /// Gets or sets a value indicating whether the created window will be a tool window.
        /// </summary>
        /// <remarks>
        /// This parameter is ignored when logging to existing form control.
        /// Tool windows have thin border, and do not show up in the task bar.
        /// </remarks>
        /// <docgen category='Form Options' order='10' />
        public bool ToolWindow { get; set; } = true;

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
        [NLogConfigurationIgnoreProperty]
        public Form TargetForm { get; set; }

        /// <summary>
        /// Gets or sets the rich text box to log to.
        /// </summary>
        [NLogConfigurationIgnoreProperty]
        public RichTextBox TargetRichTextBox { get; set; }

        /// <summary>
        /// Form created (true) or used an existing (false). Set after <see cref="InitializeTarget"/>. Can be true only if <see cref="AllowAccessoryFormCreation"/> is set to true (default).
        /// </summary>
        public bool CreatedForm { get; set; }

        /// <summary>
        /// Gets or sets a value indicating whether to create accessory form if the specified form/control combination was not found during target initialization.
        /// </summary>
        /// <remarks>
        /// If set to false and the control was not found during target initialization, the target would skip events until the control is found during <see cref="ReInitializeAllTextboxes(System.Windows.Forms.Form)"/> call
        /// </remarks>
        /// <docgen category='Form Options' order='10' />
        public bool AllowAccessoryFormCreation { get; set; } = true;

        /// <summary>
        /// gets or sets the message retention strategy which determines how the target handles messages when there's no control attached, or when switching between controls
        /// </summary>
        /// <remarks>
        /// </remarks>
        /// <docgen category='Form Options' order='10' />
        public RichTextBoxTargetMessageRetentionStrategy MessageRetention
        {
            get { return messageRetention; }
            set
            {
                lock (messageQueueLock)
                {
                    messageRetention = value;
                    if (messageRetention == RichTextBoxTargetMessageRetentionStrategy.None)
                    {
                        messageQueue = null;
                    }
                    else
                    {
                        if (MaxLines <= 0)
                        {
                            HandleError("Forbidden usage of RetentionStrategy ({0}) when MaxLines is not set", value);
                        }
                        if (messageQueue == null)
                        {
                            messageQueue = new Queue<MessageInfo>();    //no need to use MaxLine here, it could cause unnecessary memory allocation for huge limits
                        }
                    }
                }
            }
        }

        /// <summary>
        /// Actual value of the <see cref="MessageRetention"/>.
        /// </summary>
        private RichTextBoxTargetMessageRetentionStrategy messageRetention = RichTextBoxTargetMessageRetentionStrategy.None;

        /// <summary>
        /// A textbox to which we have logged last time. Used to prevent duplicating messages in the same textbox in case of config reload and RichTextBoxTargetMessageRetentionStrategy.All
        /// see https://github.com/NLog/NLog.Windows.Forms/pull/22
        /// </summary>
        private RichTextBox lastLoggedTextBoxControl;

        /// <summary>
        /// a lock object used to synchronize access to <see cref="messageQueue"/>
        /// </summary>
        private readonly object messageQueueLock = new object();

        /// <summary>
        /// A queue used to store messages based on <see cref="MessageRetention"/>.
        /// </summary>
        private volatile Queue<MessageInfo> messageQueue;

        /// <summary>
        /// If set to true, using "rtb-link" renderer (<see cref="RichTextBoxLinkLayoutRenderer"/>) would create clickable links in the control.
        /// <seealso cref="LinkClicked"/>
        /// </summary>
        public bool SupportLinks
        {
            get { return supportLinks; }
            set
            {
                if (value)
                {
                    lock (linkRegexLock)
                    {
                        if (linkAddRegex == null)
                        {
                            linkAddRegex = new Regex(@"(\([a-z0-9]{8}-[a-z0-9]{4}-[a-z0-9]{4}-[a-z0-9]{4}-[a-z0-9]{12}\))", RegexOptions.Compiled);
#if NETCOREAPP
                            linkRemoveRtfRegex = new Regex(@"\{\\field\{\\\*\\fldinst\{HYPERLINK ""[^""]*\#" + LinkPrefix + @"(\d+)"" \}\}", RegexOptions.Compiled);
#else
                            linkRemoveRtfRegex = new Regex(@"\\v #" + LinkPrefix + @"(\d+)\\v0", RegexOptions.Compiled);
#endif
                        }
                    }
                    lock (linkedEventsLock)
                    {
                        if (linkedEvents == null)
                        {
                            linkedEvents = new Dictionary<int, LogEventInfo>();
                        }
                    }
                }
                //update the field value after regex initialization to prevent concurrent access to not-initialized fields.
                supportLinks = value;
            }
        }

        /// <summary>
        /// Type of delegate for <see cref="LinkClicked"/> event.
        /// </summary>
        /// <param name="sender">The target that caused the event</param>
        /// <param name="linkText">Visible text of the link being clicked</param>
        /// <param name="logEvent">Original log event that caused a line with the link</param>
        public delegate void DelLinkClicked(RichTextBoxTarget sender, string linkText, LogEventInfo logEvent);

        /// <summary>
        /// Event fired when the user clicks on a link in the control created by the "rtb-link" renderer (<see cref="RichTextBoxLinkLayoutRenderer"/>).
        /// <seealso cref="DelLinkClicked"/>
        /// </summary>
        public event DelLinkClicked LinkClicked;
        
        /// <summary>
        /// Actual value of the <see cref="LinkClicked"/> property
        /// </summary>
        private bool supportLinks;

        /// <summary>
        /// Lock for <see cref="linkedEvents"/> dictionary access
        /// </summary>
        private readonly object linkedEventsLock = new object();

        /// <summary>
        /// A map from link id to a corresponding log event
        /// </summary>
        private Dictionary<int, LogEventInfo> linkedEvents;


        /// <summary>
        /// Returns number of events stored for active links in the control. 
        /// Used only in tests to check that non needed events are removed.
        /// </summary>
        internal int? LinkedEventsCount
        {
            get
            {
                lock (linkedEventsLock)
                {
                    if (linkedEvents == null)
                    {
                        return null;
                    }
                    return linkedEvents.Count;
                }
            }
        }

        /// <summary>
        /// Internal prefix that is added to the link id in RTF
        /// </summary>
        private const string LinkPrefix = "link";

        /// <summary>
        /// Used to synchronize lazy initialization of <see cref="linkAddRegex"/> and <see cref="linkRemoveRtfRegex"/> in <see cref="SupportLinks"/>.set
        /// </summary>
        private static readonly object linkRegexLock = new object();

        /// <summary>
        /// Used to capture link placeholders in <see cref="SendTheMessageToRichTextBox"/>
        /// Lazily initialized in <see cref="SupportLinks"/>.set(true). Assure checking <see cref="SupportLinks"/> before accessing the field 
        /// </summary>
        private static Regex linkAddRegex;

        /// <summary>
        /// Used to parse RTF with links when removing excess lines in <see cref="SendTheMessageToRichTextBox"/>
        /// Lazily initialized in <see cref="SupportLinks"/>.set(true). Assure checking <see cref="SupportLinks"/> before accessing the field
        /// </summary>
        private static Regex linkRemoveRtfRegex;

        /// <summary>
        /// Initializes the target. Can be used by inheriting classes
        /// to initialize logging.
        /// </summary>
        protected override void InitializeTarget()
        {
            base.InitializeTarget();

            if (TargetRichTextBox != null)
            {
                //already initialized by ReInitializeAllTextboxes call
                return;
            }

            CreatedForm = false;
            Form openFormByName;
            RichTextBox targetControl;

            if (AllowAccessoryFormCreation)
            {
                //old behaviour which causes creation of accessory form in case specified control cannot be found on specified form

                if (FormName == null)
                {
                    InternalLogger.Info("{0}: FormName not set, creating acceccory form", this);
                    CreateAccessoryForm();
                    return;
                }

                openFormByName = Application.OpenForms[FormName];
                if (openFormByName == null)
                {
                    InternalLogger.Info("{0}: FormName '{1}' not found, creating accessory form", this, FormName);
                    CreateAccessoryForm();
                    return;
                }

                if (string.IsNullOrEmpty(ControlName))
                {
                    HandleError("Rich text box control name must be specified for {0}.", GetType().Name);
                    CreateAccessoryForm();
                    return;
                }

                targetControl = FormHelper.FindControl<RichTextBox>(ControlName, openFormByName);
                if (targetControl == null)
                {
                    HandleError("Rich text box control '{0}' cannot be found on form '{1}'.", ControlName, FormName);
                    CreateAccessoryForm();
                    return;
                }

                //finally attached to proper control
            }
            else
            {
                //new behaviour which postpones attaching to textbox if it's not yet available at the time,

                if (FormName == null)
                {
                    HandleError("FormName should be specified for {0}.{1}", GetType().Name, this.Name);
                    return;
                }

                if (string.IsNullOrEmpty(ControlName))
                {
                    HandleError("Rich text box control name must be specified for {0}.{1}", GetType().Name, this.Name);
                    return;
                }

                openFormByName = Application.OpenForms[FormName];
                if (openFormByName == null)
                {
                    InternalLogger.Info("{0}: FormName '{1}' not found, waiting for ReInitializeAllTextboxes.", this, FormName);
                    return;
                }

                targetControl = FormHelper.FindControl<RichTextBox>(ControlName, openFormByName);
                if (targetControl == null)
                {
                    InternalLogger.Info("{0}: FormName '{1}' does not contain ControlName '{2}'. Waiting for ReInitializeAllTextboxes.", this, FormName, ControlName);
                    return;
                }

                //actually attached to a target, all ok
            }

            AttachToControl(openFormByName, targetControl);
        }

        /// <summary>
        /// Called from constructor when error is detected. In case LogManager.ThrowExceptions is enabled, throws the exception, otherwise - logs the problem message
        /// </summary>
        /// <param name="message">exception/log message format</param>
        /// <param name="args">message format arguments</param>
        private static void HandleError(string message, params object[] args)
        {
            if (LogManager.ThrowExceptions)
            {
                throw new NLogConfigurationException(String.Format(message, args));
            }
            InternalLogger.Error(message, args);
        }

        /// <summary>
        /// Used to create accessory form with textbox in case specified form or control were not found during InitializeTarget() and AllowAccessoryFormCreation==true
        /// </summary>
        private void CreateAccessoryForm()
        {
            if (FormName == null)
            {
                FormName = "NLogForm" + Guid.NewGuid().ToString("N");
            }
            Form form = FormHelper.CreateForm(FormName, Width, Height, true, ShowMinimized, ToolWindow);
            RichTextBox control = FormHelper.CreateRichTextBox(ControlName, form);
            AttachToControl(form, control);
            CreatedForm = true;
        }

        /// <summary>
        /// Used to (re)initialize target when attaching it to another RTB control
        /// </summary>
        /// <param name="form">form owning textboxControl</param>
        /// <param name="textboxControl">a new control to attach to</param>
        private void AttachToControl(Form form, RichTextBox textboxControl)
        {
            InternalLogger.Info("{0}: Attaching target to textbox {1}.{2}", this, form.Name, textboxControl.Name);
            DetachFromControl();
            this.TargetForm = form;
            this.TargetRichTextBox = textboxControl;

            if (this.SupportLinks)
            {
                this.TargetRichTextBox.DetectUrls = false;
                this.TargetRichTextBox.LinkClicked += TargetRichTextBox_LinkClicked;
            }

            //OnReattach?
            switch (messageRetention)
            {
                case RichTextBoxTargetMessageRetentionStrategy.None:
                    break;
                case RichTextBoxTargetMessageRetentionStrategy.All:
                    if (lastLoggedTextBoxControl != textboxControl) //don't log to same RTB as befre, because it already has everything. Happens in case of config reload, see https://github.com/NLog/NLog.Windows.Forms/pull/22
                    {
                        lock (messageQueueLock)
                        {
                            foreach (MessageInfo messageInfo in messageQueue)
                            {
                                DoSendMessageToTextbox(messageInfo.Message, messageInfo.Rule, messageInfo.LogEvent);
                            }
                        }
                    }
                    break;
                case RichTextBoxTargetMessageRetentionStrategy.OnlyMissed:
                    lock (messageQueueLock)
                    {
                        while (messageQueue.Count > 0)
                        {
                            MessageInfo messageInfo = messageQueue.Dequeue();
                            DoSendMessageToTextbox(messageInfo.Message, messageInfo.Rule, messageInfo.LogEvent);
                        }
                    }
                    break;
                default:
                    HandleError("Unexpected retention strategy {0}", messageRetention);
                    break;
            }
        }

        /// <summary>
        /// Attached to RTB-control's LinkClicked event to convert link text to logEvent
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="e"></param>
        private void TargetRichTextBox_LinkClicked(object sender, LinkClickedEventArgs e)
        {
            Match match = Regex.Match(e.LinkText, "#" + LinkPrefix + @"(\d+)");
            if (!match.Success)
            {
                //could be a link inserted by another RTB control user
                InternalLogger.Warn("{0}: Unexpected link format '{1}', skipping", this, e.LinkText);
                return;
            }

            int id;
            if (!int.TryParse(match.Groups[1].Value, out id))
            {
                //still could be a link inserted by another RTB control user
                InternalLogger.Warn("{0}: Unexpected link format '{1}', skipping", this, e.LinkText);
                return;
            }

            LogEventInfo logEvent;
            lock (linkedEventsLock)
            {
                linkedEvents.TryGetValue(id, out logEvent);
            }
            if (logEvent == null)
            {
                HandleError("Missing link id {0}", id);
                return;
            }

            DelLinkClicked linkClickEvent = LinkClicked;
            if (linkClickEvent != null)
            {
                string linkText = e.LinkText.Substring(0, match.Index);
                linkClickEvent(this, linkText, logEvent);
            }
        }

        /// <summary>
        /// if <see cref="CreatedForm"/> is true, then destroys created form. Resets <see cref="CreatedForm"/>, <see cref="TargetForm"/> and <see cref="TargetRichTextBox"/> to default values
        /// </summary>
        private void DetachFromControl()
        {
            if (CreatedForm)
            {
                try
                {
                    if (!TargetForm.IsDisposed)
                    {
                        if (TargetForm.InvokeRequired)
                        {
                            TargetForm.BeginInvoke((FormCloseDelegate)TargetForm.Close);
                        }
                        else
                        {
                            TargetForm.Close();
                        }
                    }
                }
                catch (Exception ex)
                {
                    InternalLogger.Warn(ex, "{0}: Failed DetachFromControl", this);

                    if (LogManager.ThrowExceptions)
                    {
                        throw;
                    }
                }
                CreatedForm = false;
            }
            TargetForm = null;
            TargetRichTextBox = null;
        }

        /// <summary>
        /// Closes the target and releases any unmanaged resources.
        /// </summary>
        protected override void CloseTarget()
        {
            DetachFromControl();
        }

        /// <summary>
        /// Log message to RichTextBox.
        /// </summary>
        /// <param name="logEvent">The logging event.</param>
        protected override void Write(LogEventInfo logEvent)
        {
            RichTextBox textbox = TargetRichTextBox;
            if (textbox == null || textbox.IsDisposed)
            {
                //no last logged textbox
                lastLoggedTextBoxControl = null;
                if (AllowAccessoryFormCreation)
                {
                    CreateAccessoryForm();
                }
                else if (messageRetention == RichTextBoxTargetMessageRetentionStrategy.None)
                {
                    InternalLogger.Trace("{0}: Attached Textbox is {1}, skipping logging", this, textbox == null ? "null" : "disposed");
                    return;
                }
            }

            string logMessage = RenderLogEvent(Layout, logEvent);
            RichTextBoxRowColoringRule matchingRule = FindMatchingRule(logEvent);

            bool messageSent = DoSendMessageToTextbox(logMessage, matchingRule, logEvent);

            if (messageSent)
            {
                //remember last logged text box
                lastLoggedTextBoxControl = textbox;
            }

            switch (messageRetention)
            {
                case RichTextBoxTargetMessageRetentionStrategy.None:
                    break;
                case RichTextBoxTargetMessageRetentionStrategy.All:
                    StoreMessage(logMessage, matchingRule, logEvent);
                    break;
                case RichTextBoxTargetMessageRetentionStrategy.OnlyMissed:
                    if (!messageSent)
                    {
                        StoreMessage(logMessage, matchingRule, logEvent);
                    }
                    break;
                default:
                    HandleError("Unexpected retention strategy {0}", messageRetention);
                    break;
            }
        }

        /// <summary>
        /// Actually sends log message to <see cref="TargetRichTextBox"/>
        /// </summary>
        /// <param name="logMessage">a message to send</param>
        /// <param name="rule">matching coloring rule</param>
        /// <param name="logEvent">original logEvent</param>
        /// <returns>true if the message was actually sent (i.e. <see cref="TargetRichTextBox"/> is not null and not disposed, and no exception happened during message send)</returns>
        private bool DoSendMessageToTextbox(string logMessage, RichTextBoxRowColoringRule rule, LogEventInfo logEvent)
        {
            RichTextBox textbox = TargetRichTextBox;
            try
            {
                if (textbox != null && !textbox.IsDisposed)
                {
                    if (textbox.InvokeRequired)
                    {
                        textbox.BeginInvoke(_sendTheMessageToRichTextBox, textbox, logMessage, rule, logEvent);
                    }
                    else
                    {
                        SendTheMessageToRichTextBox(textbox, logMessage, rule, logEvent);
                    }
                    return true;
                }
            }
            catch (Exception ex)
            {
                InternalLogger.Warn(ex, "{0}: Failed to append RichTextBox", this);

                if (LogManager.ThrowExceptions)
                {
                    throw;
                }
            }
            return false;
        }

        /// <summary>
        /// Find first matching rule
        /// </summary>
        /// <param name="logEvent"></param>
        /// <returns></returns>
        private RichTextBoxRowColoringRule FindMatchingRule(LogEventInfo logEvent)
        {
            //custom rules first
            if (RowColoringRules.Count > 0)
            {
                foreach (RichTextBoxRowColoringRule coloringRule in RowColoringRules)
                {
                    if (coloringRule.CheckCondition(logEvent))
                    {
                        return coloringRule;
                    }
                }
            }

            if (UseDefaultRowColoringRules && DefaultRowColoringRules != null)
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
            if (string.IsNullOrEmpty(color) || color == "Empty")
            {
                return defaultColor;
            }

            return Color.FromName(color);
        }

        private void SendTheMessageToRichTextBox(RichTextBox textBox, string logMessage, RichTextBoxRowColoringRule rule, LogEventInfo logEvent)
        {
            int startIndex = textBox.TextLength;
            textBox.SelectionStart = startIndex;
            textBox.SelectionBackColor = GetColorFromString(rule.BackgroundColor?.Render(logEvent), textBox.BackColor);
            textBox.SelectionColor = GetColorFromString(rule.FontColor?.Render(logEvent), textBox.ForeColor);
            textBox.SelectionFont = new Font(textBox.SelectionFont, textBox.SelectionFont.Style ^ rule.Style);
            textBox.AppendText(logMessage + "\n");
            textBox.SelectionLength = textBox.TextLength - textBox.SelectionStart;

            if (WordColoringRules.Count > 0)
            {
                // find word to color
                foreach (RichTextBoxWordColoringRule wordRule in WordColoringRules)
                {
                    var wordRulePattern = wordRule.Regex?.Render(logEvent) ?? string.Empty;
                    var wordRuleText = wordRule.Text?.Render(logEvent) ?? string.Empty;
                    var wordRuleWholeWords = wordRule.WholeWords.RenderValue(logEvent);
                    var wordRuleIgnoreCase = wordRule.IgnoreCase.RenderValue(logEvent);

                    MatchCollection matches = wordRule.ResolveRegEx(wordRulePattern, wordRuleText, wordRuleWholeWords, wordRuleIgnoreCase).Matches(textBox.Text, startIndex);
                    foreach (Match match in matches)
                    {
                        textBox.SelectionStart = match.Index;
                        textBox.SelectionLength = match.Length;
                        textBox.SelectionBackColor = GetColorFromString(wordRule.BackgroundColor?.Render(logEvent), textBox.BackColor);
                        textBox.SelectionColor = GetColorFromString(wordRule.FontColor?.Render(logEvent), textBox.ForeColor);
                        textBox.SelectionFont = new Font(textBox.SelectionFont, textBox.SelectionFont.Style ^ wordRule.Style);
                    }
                }
            }

            if (SupportLinks)
            {
                object linkInfoObj = null;
                if (logEvent.HasProperties)
                {
                    logEvent.Properties.TryGetValue(RichTextBoxLinkLayoutRenderer.LinkInfo.PropertyName, out linkInfoObj);
                }

                if (linkInfoObj != null)
                {
                    RichTextBoxLinkLayoutRenderer.LinkInfo linkInfo = (RichTextBoxLinkLayoutRenderer.LinkInfo)linkInfoObj;

                    bool linksAdded = false;

                    textBox.SelectionStart = startIndex;
                    textBox.SelectionLength = textBox.TextLength - textBox.SelectionStart;
                    string addedText = textBox.SelectedText;
                    MatchCollection matches = linkAddRegex.Matches(addedText); //only access regex after checking SupportLinks, as it assures the initialization
                    for (int i = matches.Count - 1; i >= 0; --i)    //backwards order, so the string positions are not affected
                    {
                        Match match = matches[i];
                        string linkText = linkInfo.GetValue(match.Value);
                        if (linkText != null)
                        {
                            textBox.SelectionStart = startIndex + match.Index;
                            textBox.SelectionLength = match.Length;
                            FormHelper.ChangeSelectionToLink(textBox, linkText, LinkPrefix + logEvent.SequenceID);
                            linksAdded = true;
                        }
                    }
                    if (linksAdded)
                    {
                        linkedEvents[logEvent.SequenceID] = logEvent;
                    }
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
                        if (SupportLinks)
                        {
                            string selectedRtf = textBox.SelectedRtf;
                            //only access regex after checking SupportLinks, as it assures the initialization
                            foreach (Match match in linkRemoveRtfRegex.Matches(selectedRtf))
                            {
                                int id;
                                if (int.TryParse(match.Groups[1].Value, out id))
                                {
                                    lock (linkedEventsLock)
                                    {
                                        linkedEvents.Remove(id);
                                    }
                                }
                            }
                        }
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

        /// <summary>
        /// Stores a new message in internal queue, if it exists. Removes overflowing messages.
        /// </summary>
        /// <param name="logMessage">a message to store</param>
        /// <param name="rule">a corresponding coloring rule</param>
        /// <param name="logEvent">original LogEvent</param>
        private void StoreMessage(string logMessage, RichTextBoxRowColoringRule rule, LogEventInfo logEvent)
        {
            lock (messageQueueLock)
            {
                if (messageQueue == null)
                {
                    return;
                }
                if (MaxLines > 0)
                {
                    while (messageQueue.Count >= MaxLines)
                    {
                        messageQueue.Dequeue();
                    }
                }
                messageQueue.Enqueue(new MessageInfo(logMessage, rule, logEvent));
            }
        }

        private readonly struct MessageInfo
        {
            internal string Message { get; }
            internal RichTextBoxRowColoringRule Rule { get; }
            internal LogEventInfo LogEvent { get; }
            internal MessageInfo(string message, RichTextBoxRowColoringRule rule, LogEventInfo logEvent)
            {
                Message = message;
                Rule = rule;
                LogEvent = logEvent;
            }
        }
    }
}