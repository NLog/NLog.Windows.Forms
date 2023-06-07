using System;
using System.Reflection;
using Xunit.Sdk;

namespace NLog.Windows.Forms.Tests
{
    [AttributeUsage(AttributeTargets.Class | AttributeTargets.Method, AllowMultiple = false, Inherited = true)]
    public sealed class LogManagerResetAttribute : BeforeAfterTestAttribute
    {
        public override void Before(MethodInfo methodUnderTest)
        {
            LogManager.Shutdown();
            LogManager.ThrowExceptions = true;
        }

        public override void After(MethodInfo methodUnderTest)
        {
            LogManager.Shutdown();
        }
    }
}
