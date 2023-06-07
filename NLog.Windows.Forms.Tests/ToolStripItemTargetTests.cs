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
    [Collection("NLog.Windows.Forms Tests")]
    public class ToolStripItemTargetTests
    {
        private readonly Logger logger = LogManager.GetLogger("NLog.UnitTests.Targets.ToolStripItemTargetTests");

        public ToolStripItemTargetTests()
        {
            LogManager.ThrowExceptions = true;
            LogManager.Setup().SetupExtensions(ext => ext.RegisterWindowsForms());
        }

        [Fact]
        [LogManagerReset]
        public void SimpleToolStripItemTargetTest()
        {
            Form testForm = null;
            try
            {
                testForm = new Form();
                testForm.Name = "Form1";

                ToolStrip testMenuStrip = new ToolStrip();
                testMenuStrip.Name = "ToolStrip1";
                testForm.Controls.Add(testMenuStrip);

                ToolStripMenuItem testItem = new ToolStripMenuItem();
                testItem.Name = "Item1";
                testMenuStrip.Items.Add(testItem);

                testForm.Show();

                Application.DoEvents();

                ToolStripItemTarget target = new ToolStripItemTarget()
                {
                    ItemName = "Item1",
                    FormName = "Form1",
                    ToolStripName = "ToolStrip1",
                    Layout = "${level} ${logger} ${message}"
                };
                NLog.LogManager.Setup().LoadConfiguration(cfg => cfg.ForLogger().WriteTo(target));

                logger.Fatal("Test");   // Send log
                Application.DoEvents(); // Do events to allow the invoked method is completed.
                Assert.Equal("Fatal NLog.UnitTests.Targets.ToolStripItemTargetTests Test", testItem.Text); // Test if method worked.
                
                logger.Error("Foo");
                Application.DoEvents();
                Assert.Equal("Error NLog.UnitTests.Targets.ToolStripItemTargetTests Foo", testItem.Text);
                
                logger.Warn("Bar");
                Application.DoEvents();
                Assert.Equal("Warn NLog.UnitTests.Targets.ToolStripItemTargetTests Bar", testItem.Text);
                
                logger.Info("Test");
                Application.DoEvents();
                Assert.Equal("Info NLog.UnitTests.Targets.ToolStripItemTargetTests Test", testItem.Text);
                
                logger.Debug("Foo");
                Application.DoEvents();
                Assert.Equal("Debug NLog.UnitTests.Targets.ToolStripItemTargetTests Foo", testItem.Text);
                
                logger.Trace("Bar");
                Application.DoEvents();
                Assert.Equal("Trace NLog.UnitTests.Targets.ToolStripItemTargetTests Bar", testItem.Text);

            }
            finally
            {
                if (testForm != null)
                {
                    testForm.Dispose();
                }
                LogManager.Configuration = null;
            }
        }
    }
}
