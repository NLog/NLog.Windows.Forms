using NLog.Config;

namespace NLog
{
    /// <summary>
    /// Extension methods to setup LogFactory options
    /// </summary>
    public static class SetupBuilderExtensions
    {
        /// <summary>
        /// Register the NLog.Windows.Forms extensions before loading NLog config
        /// </summary>
        public static ISetupBuilder RegisterWindowsForms(this ISetupBuilder setupBuilder)
        {
            setupBuilder.SetupExtensions(e => e.RegisterWindowsForms());
            return setupBuilder;
        }
    }
}
