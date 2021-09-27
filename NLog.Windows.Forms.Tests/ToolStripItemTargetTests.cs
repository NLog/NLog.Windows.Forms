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
            Form testForm = null;
            ToolStripMenuItem testItem = null;
            try
            {
                RunForm(out testForm,out testItem);

                Application.DoEvents();

                ToolStripItemTarget target = new ToolStripItemTarget()
                {
                    ItemName = "Item1",
                    FormName = "Form1",
                    ToolStripName = "ToolStrip1",
                    Layout = "${level} ${logger} ${message}"
                };
                SimpleConfigurator.ConfigureForTargetLogging(target, LogLevel.Trace);

                logger.Fatal("Test");   // Send log
                Application.DoEvents(); // Do events so invoked method can run.
                Assert.Equal("Fatal NLog.UnitTests.Targets.ToolStripItemTargetTests Test", getText(testForm,testItem)); // Test if method worked.
                
                logger.Error("Foo");
                Application.DoEvents();
                Assert.Equal("Error NLog.UnitTests.Targets.ToolStripItemTargetTests Foo", getText(testForm, testItem));
                
                logger.Warn("Bar");
                Application.DoEvents();
                Assert.Equal("Warn NLog.UnitTests.Targets.ToolStripItemTargetTests Bar", getText(testForm, testItem));
                
                logger.Info("Test");
                Application.DoEvents();
                Assert.Equal("Info NLog.UnitTests.Targets.ToolStripItemTargetTests Test", getText(testForm, testItem));
                
                logger.Debug("Foo");
                Application.DoEvents();
                Assert.Equal("Debug NLog.UnitTests.Targets.ToolStripItemTargetTests Foo", getText(testForm, testItem));
                
                logger.Trace("Bar");
                Application.DoEvents();
                Assert.Equal("Trace NLog.UnitTests.Targets.ToolStripItemTargetTests Bar", getText(testForm, testItem));

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

        private string getText(Form testForm, ToolStripMenuItem testItem)
        {
            return (string)testForm.Invoke((Func<string>)(()=> testItem.Text));
        }

        [STAThread]
        private void RunForm(out Form testForm,out ToolStripMenuItem testItem)
        {
            testForm = new Form();
            testForm.Name = "Form1";

            ToolStrip testMenuStrip = new ToolStrip();
            testMenuStrip.Name = "ToolStrip1";
            testForm.Controls.Add(testMenuStrip);

            testItem = new ToolStripMenuItem();
            testItem.Name = "Item1";
            testMenuStrip.Items.Add(testItem);

            testForm.Show();
        }
    }
}
