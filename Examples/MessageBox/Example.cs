using System;

using NLog;
using NLog.Windows.Forms;

class Example
{
    static void Main(string[] args)
    {
        // Programmatic configuration that is equivalent to
        // the "msgbox" target configuration in NLog.config
        MessageBoxTarget target = new MessageBoxTarget();
        target.Layout = "${longdate}: ${message}";
        target.Caption = "${level} message";

        NLog.Config.SimpleConfigurator.ConfigureForTargetLogging(target, LogLevel.Debug);

        Logger logger = LogManager.GetLogger("Example");
        logger.Debug("log message");
    }
}
