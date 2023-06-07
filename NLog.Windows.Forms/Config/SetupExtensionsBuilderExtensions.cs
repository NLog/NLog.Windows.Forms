using NLog.Config;
using NLog.Windows.Forms;

namespace NLog
{
    /// <summary>
    /// Extension methods to setup NLog extensions, so they are known when loading NLog LoggingConfiguration
    /// </summary>
    public static class SetupExtensionsBuilderExtensions
    {
        /// <summary>
        /// Register the NLog.Windows.Forms extensions before loading NLog config
        /// </summary>
        public static ISetupExtensionsBuilder RegisterWindowsForms(this ISetupExtensionsBuilder setupBuilder)
        {
            return setupBuilder.RegisterTarget<FormControlTarget>("FormControl").
                RegisterTarget<MessageBoxTarget>("MessageBox").
                RegisterTarget<RichTextBoxTarget>("RichTextBox").
                RegisterTarget<ToolStripItemTarget>("ToolStripItem").
                RegisterLayoutRenderer<RichTextBoxLinkLayoutRenderer>("rtb-link").
                RegisterType<RichTextBoxRowColoringRule>().
                RegisterType<RichTextBoxWordColoringRule>();
        }
    }
}
