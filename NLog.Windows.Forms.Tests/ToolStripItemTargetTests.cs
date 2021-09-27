using NLog.Config;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Forms;
using Xunit;

namespace NLog.Windows.Forms.Tests
{
    public class ToolStripItemTargetTests
    {
        private Logger logger = LogManager.GetLogger("NLog.UnitTests.Targets.ToolStripItemTargetTests");

        [Fact]
        public void SimpleToolStripItemTargetTest()
        {
            try
            {
                Form testForm = new Form();
                testForm.Name = "Form1";


                ToolStrip testMenuStrip = new ToolStrip();
                testMenuStrip.Name = "ToolStrip1";
                testForm.Controls.Add(testMenuStrip);

                ToolStripMenuItem testItem = new ToolStripMenuItem();
                testItem.Name = "Item1";
                testMenuStrip.Items.Add(testItem);

                ToolStripItemTarget target = new ToolStripItemTarget()
                {
                    ItemName = "Item1",
                    FormName = "Form1",
                    ToolStripName = "ToolStrip1",
                    Layout = "${level} ${logger} ${message}"
                };
                testForm.Show();

                Application.DoEvents();

                SimpleConfigurator.ConfigureForTargetLogging(target, LogLevel.Trace);

                logger.Fatal("Test");
                Assert.Equal("FATAL NLog.UnitTests.Targets.ToolStripItemTargetTests Test", testItem.Text);
                logger.Error("Foo");
                Assert.Equal("ERROR NLog.UnitTests.Targets.ToolStripItemTargetTests Foo", testItem.Text);
                logger.Warn("Bar");
                Assert.Equal("WARN NLog.UnitTests.Targets.ToolStripItemTargetTests Bar", testItem.Text);
                logger.Info("Test");
                Assert.Equal("INFO NLog.UnitTests.Targets.ToolStripItemTargetTests Test", testItem.Text);
                logger.Debug("Foo");
                Assert.Equal("DEBUG NLog.UnitTests.Targets.ToolStripItemTargetTests Foo", testItem.Text);
                logger.Trace("Bar");
                Assert.Equal("Trace NLog.UnitTests.Targets.ToolStripItemTargetTests Bar", testItem.Text);
            }
            finally
            {
                LogManager.Configuration = null;
            }
        }
    }
}
