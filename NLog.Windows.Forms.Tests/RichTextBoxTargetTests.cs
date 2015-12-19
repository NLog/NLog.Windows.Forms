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

using System.Drawing;
using System.IO;
using System.Text;
using System.Windows.Forms;
using NLog.Config;
using NLog.Targets;
using Xunit;
using System.Text.RegularExpressions;
using System;
using System.Threading.Tasks;
using System.Threading;

namespace NLog.Windows.Forms.Tests
{
    public class RichTextBoxTargetTests
    {
        private Logger logger = LogManager.GetLogger("NLog.UnitTests.Targets.RichTextBoxTargetTests");

        [Fact]
        public void SimpleRichTextBoxTargetTest()
        {
            RichTextBoxTarget target = new RichTextBoxTarget()
            {
                ControlName = "Control1",
                UseDefaultRowColoringRules = true,
                Layout = "${level} ${logger} ${message}",
                ToolWindow = false,
                Width = 300,
                Height = 200,
            };

            SimpleConfigurator.ConfigureForTargetLogging(target, LogLevel.Trace);
            logger.Fatal("Test");
            logger.Error("Foo");
            logger.Warn("Bar");
            logger.Info("Test");
            logger.Debug("Foo");
            logger.Trace("Bar");

            Application.DoEvents();

            var form = target.TargetForm;

            Assert.True(target.CreatedForm);
            Assert.True(form.Name.StartsWith("NLog"));
            Assert.Equal(FormWindowState.Normal, form.WindowState);
            Assert.Equal("NLog", form.Text);
            Assert.Equal(300, form.Width);
            Assert.Equal(200, form.Height);

            string rtfText = ExtractRtf(target.TargetRichTextBox);

            Assert.True(target.CreatedForm);

            var result = rtfText;
            Assert.True(result.Contains(@"{\colortbl ;\red255\green255\blue255;\red255\green0\blue0;\red255\green165\blue0;\red0\green0\blue0;\red128\green128\blue128;\red169\green169\blue169;}"));
            Assert.True(result.Contains(@"\viewkind4\uc1\pard\cf1\highlight2\b\f0\fs17 Fatal NLog.UnitTests.Targets.RichTextBoxTargetTests Test\par"));
            Assert.True(result.Contains(@"\cf2\highlight1\i Error NLog.UnitTests.Targets.RichTextBoxTargetTests Foo\par"));
            Assert.True(result.Contains(@"\cf3\ul\b0\i0 Warn NLog.UnitTests.Targets.RichTextBoxTargetTests Bar\par"));
            Assert.True(result.Contains(@"\cf4\ulnone Info NLog.UnitTests.Targets.RichTextBoxTargetTests Test\par"));
            Assert.True(result.Contains(@"\cf5 Debug NLog.UnitTests.Targets.RichTextBoxTargetTests Foo\par"));
            Assert.True(result.Contains(@"\cf6\i Trace NLog.UnitTests.Targets.RichTextBoxTargetTests Bar\par"));
            Assert.True(result.Contains(@"\cf0\highlight0\i0\f1\par"));
            Assert.True(result.Contains(@"}"));

            LogManager.Configuration = null;
            Assert.Null(target.TargetForm);
            Application.DoEvents();
            Assert.True(form.IsDisposed);
        }

        [Fact]
        public void NoColoringTest()
        {
            try
            {
                RichTextBoxTarget target = new RichTextBoxTarget()
                {
                    ControlName = "Control1",
                    Layout = "${level} ${logger} ${message}",
                    ShowMinimized = true,
                    ToolWindow = false,
                };

                SimpleConfigurator.ConfigureForTargetLogging(target, LogLevel.Trace);
                logger.Fatal("Test");
                logger.Error("Foo");
                logger.Warn("Bar");
                logger.Info("Test");
                logger.Debug("Foo");
                logger.Trace("Bar");

                Application.DoEvents();

                var form = target.TargetForm;

                string rtfText = ExtractRtf(target.TargetRichTextBox);

                Assert.True(target.CreatedForm);

                var result = rtfText;
                Assert.True(result.Contains(@"{\colortbl ;\red0\green0\blue0;\red255\green255\blue255;}"));
                Assert.True(result.Contains(@"\viewkind4\uc1\pard\cf1\highlight2\f0\fs17 Fatal NLog.UnitTests.Targets.RichTextBoxTargetTests Test\par"));
                Assert.True(result.Contains(@"Error NLog.UnitTests.Targets.RichTextBoxTargetTests Foo\par"));
                Assert.True(result.Contains(@"Warn NLog.UnitTests.Targets.RichTextBoxTargetTests Bar\par"));
                Assert.True(result.Contains(@"Info NLog.UnitTests.Targets.RichTextBoxTargetTests Test\par"));
                Assert.True(result.Contains(@"Debug NLog.UnitTests.Targets.RichTextBoxTargetTests Foo\par"));
                Assert.True(result.Contains(@"Trace NLog.UnitTests.Targets.RichTextBoxTargetTests Bar\par"));
                Assert.True(result.Contains(@"\cf0\highlight0\f1\par"));
                Assert.True(result.Contains(@"}"));
            }
            finally
            {
                LogManager.Configuration = null;
            }
        }

        [Fact]
        public void CustomRowColoringTest()
        {
            try
            {
                RichTextBoxTarget target = new RichTextBoxTarget()
                {
                    ControlName = "Control1",
                    Layout = "${level} ${logger} ${message}",
                    ShowMinimized = true,
                    ToolWindow = false,
                    RowColoringRules =
                    {
                        new RichTextBoxRowColoringRule("starts-with(message, 'B')", "Maroon", "Empty"),
                    }
                };

                SimpleConfigurator.ConfigureForTargetLogging(target, LogLevel.Trace);
                logger.Fatal("Test");
                logger.Error("Foo");
                logger.Warn("Bar");
                logger.Info("Test");
                logger.Debug("Foo");
                logger.Trace("Bar");

                Application.DoEvents();

                var form = target.TargetForm;

                string rtfText = ExtractRtf(target.TargetRichTextBox);

                Assert.True(target.CreatedForm);

                var result = rtfText;
                Assert.True(result.Contains(@"{\colortbl ;\red0\green0\blue0;\red255\green255\blue255;\red128\green0\blue0;}"));
                Assert.True(result.Contains(@"\viewkind4\uc1\pard\cf1\highlight2\f0\fs17 Fatal NLog.UnitTests.Targets.RichTextBoxTargetTests Test\par"));
                Assert.True(result.Contains(@"Error NLog.UnitTests.Targets.RichTextBoxTargetTests Foo\par"));
                Assert.True(result.Contains(@"\cf3 Warn NLog.UnitTests.Targets.RichTextBoxTargetTests Bar\par"));
                Assert.True(result.Contains(@"\cf1 Info NLog.UnitTests.Targets.RichTextBoxTargetTests Test\par"));
                Assert.True(result.Contains(@"Debug NLog.UnitTests.Targets.RichTextBoxTargetTests Foo\par"));
                Assert.True(result.Contains(@"\cf3 Trace NLog.UnitTests.Targets.RichTextBoxTargetTests Bar\par"));
                Assert.True(result.Contains(@"\cf0\highlight0\f1\par"));
                Assert.True(result.Contains(@"}"));
            }
            finally
            {
                LogManager.Configuration = null;
            }
        }

        [Fact]
        public void CustomWordRowColoringTest()
        {
            try
            {
                RichTextBoxTarget target = new RichTextBoxTarget()
                {
                    ControlName = "Control1",
                    Layout = "${level} ${logger} ${message}",
                    ShowMinimized = true,
                    ToolWindow = false,
                    WordColoringRules =
                    {
                        new RichTextBoxWordColoringRule("zzz", "Red", "Empty"),
                        new RichTextBoxWordColoringRule("aaa", "Green", "Empty"),
                    }
                };

                SimpleConfigurator.ConfigureForTargetLogging(target, LogLevel.Trace);
                logger.Fatal("Test zzz");
                logger.Error("Foo xxx");
                logger.Warn("Bar yyy");
                logger.Info("Test aaa");
                logger.Debug("Foo zzz");
                logger.Trace("Bar ccc");

                Application.DoEvents();

                var form = target.TargetForm;

                string rtfText = ExtractRtf(target.TargetRichTextBox);

                Assert.True(target.CreatedForm);

                // "zzz" string will be highlighted

                var result = rtfText;
                Assert.True(result.Contains(@"{\colortbl ;\red0\green0\blue0;\red255\green255\blue255;\red255\green0\blue0;\red0\green128\blue0;}"));
                Assert.True(result.Contains(@"\viewkind4\uc1\pard\cf1\highlight2\f0\fs17 Fatal NLog.UnitTests.Targets.RichTextBoxTargetTests Test \cf3\f1 zzz\cf1\f0\par"));
                Assert.True(result.Contains(@"Error NLog.UnitTests.Targets.RichTextBoxTargetTests Foo xxx\par"));
                Assert.True(result.Contains(@"Warn NLog.UnitTests.Targets.RichTextBoxTargetTests Bar yyy\par"));
                Assert.True(result.Contains(@"Info NLog.UnitTests.Targets.RichTextBoxTargetTests Test \cf4\f1 aaa\cf1\f0\par"));
                Assert.True(result.Contains(@"Debug NLog.UnitTests.Targets.RichTextBoxTargetTests Foo \cf3\f1 zzz\cf1\f0\par"));
                Assert.True(result.Contains(@"Trace NLog.UnitTests.Targets.RichTextBoxTargetTests Bar ccc\par"));
                Assert.True(result.Contains(@"\cf0\highlight0\f1\par"));
                Assert.True(result.Contains(@"}"));
            }
            finally
            {
                LogManager.Configuration = null;
            }
        }

        [Fact]
        public void RichTextBoxTargetDefaultsTest()
        {
            var target = new RichTextBoxTarget();
            Assert.False(target.UseDefaultRowColoringRules);
            Assert.Equal(0, target.WordColoringRules.Count);
            Assert.Equal(0, target.RowColoringRules.Count);
            Assert.Null(target.FormName);
            Assert.Null(target.ControlName);
        }

        [Fact]
        public void AutoScrollTest()
        {
            try
            {
                RichTextBoxTarget target = new RichTextBoxTarget()
                {
                    ControlName = "Control1",
                    Layout = "${level} ${logger} ${message}",
                    ShowMinimized = true,
                    ToolWindow = false,
                    AutoScroll = true,
                };

                var form = target.TargetForm;
                SimpleConfigurator.ConfigureForTargetLogging(target, LogLevel.Trace);
                for (int i = 0; i < 100; ++i)
                {
                    logger.Info("Test");
                    Application.DoEvents();
                    Assert.Equal(target.TargetRichTextBox.SelectionStart, target.TargetRichTextBox.TextLength);
                    Assert.Equal(target.TargetRichTextBox.SelectionLength, 0);
                }
            }
            finally
            {
                LogManager.Configuration = null;
            }
        }

        [Fact]
        public void MaxLinesTest()
        {
            try
            {
                RichTextBoxTarget target = new RichTextBoxTarget()
                {
                    ControlName = "Control1",
                    Layout = "${message}",
                    ShowMinimized = true,
                    ToolWindow = false,
                    AutoScroll = true,
                };

                Assert.Equal(0, target.MaxLines);
                target.MaxLines = 7;

                var form = target.TargetForm;
                SimpleConfigurator.ConfigureForTargetLogging(target, LogLevel.Trace);
                for (int i = 0; i < 100; ++i)
                {
                    logger.Info("Test {0}", i);
                }

                Application.DoEvents();
                string expectedText = "Test 93\nTest 94\nTest 95\nTest 96\nTest 97\nTest 98\nTest 99\n";

                Assert.Equal(expectedText, target.TargetRichTextBox.Text);
            }
            finally
            {
                LogManager.Configuration = null;
            }
        }

        [Fact]
        public void ColoringRuleDefaults()
        {
            var expectedRules = new[]
            {
                new RichTextBoxRowColoringRule("level == LogLevel.Fatal", "White", "Red", FontStyle.Bold),
                new RichTextBoxRowColoringRule("level == LogLevel.Error", "Red", "Empty", FontStyle.Bold | FontStyle.Italic),
                new RichTextBoxRowColoringRule("level == LogLevel.Warn", "Orange", "Empty", FontStyle.Underline),
                new RichTextBoxRowColoringRule("level == LogLevel.Info", "Black", "Empty"),
                new RichTextBoxRowColoringRule("level == LogLevel.Debug", "Gray", "Empty"),
                new RichTextBoxRowColoringRule("level == LogLevel.Trace", "DarkGray", "Empty", FontStyle.Italic),
            };

            var actualRules = RichTextBoxTarget.DefaultRowColoringRules;
            Assert.Equal(expectedRules.Length, actualRules.Count);
            for (int i = 0; i < expectedRules.Length; ++i)
            {
                Assert.Equal(expectedRules[i].BackgroundColor, actualRules[i].BackgroundColor);
                Assert.Equal(expectedRules[i].FontColor, actualRules[i].FontColor);
                Assert.Equal(expectedRules[i].Condition.ToString(), actualRules[i].Condition.ToString());
                Assert.Equal(expectedRules[i].Style, actualRules[i].Style);
            }
        }

        [Fact]
        public void ActiveFormTest()
        {
            var config = new LoggingConfiguration();
            var target = new RichTextBoxTarget()
            {
                FormName = "MyForm1",
                ControlName = "Control1",
                UseDefaultRowColoringRules = true,
                Layout = "${level} ${logger} ${message}",
                ToolWindow = false,
                Width = 300,
                Height = 200,
            };
            config.AddTarget("target", target);
            config.LoggingRules.Add(new LoggingRule("*", target));
            LogManager.ThrowExceptions = true;

            using (Form form = new Form())
            {
                form.Name = "MyForm1";
                form.WindowState = FormWindowState.Minimized;
                RichTextBox rtb = new RichTextBox();
                rtb.Dock = DockStyle.Fill;
                rtb.Name = "Control1";
                form.Controls.Add(rtb);
                form.Show();
                form.Activate();

                new LogFactory(config);
                Assert.Same(form, target.TargetForm);
                Assert.Same(rtb, target.TargetRichTextBox);
            }
        }

        [Fact]
        public void ActiveFormTest2()
        {
            var config = new LoggingConfiguration();
            RichTextBoxTarget target = new RichTextBoxTarget()
            {
                FormName = "MyForm2",
                ControlName = "Control1",
                UseDefaultRowColoringRules = true,
                Layout = "${level} ${logger} ${message}",
                ToolWindow = false,
                Width = 300,
                Height = 200,
            };
            config.AddTarget("target", target);
            config.LoggingRules.Add(new LoggingRule("*", target));
            LogManager.ThrowExceptions = true;

            using (Form form = new Form())
            {
                form.Name = "MyForm1";
                form.WindowState = FormWindowState.Minimized;

                RichTextBox rtb = new RichTextBox();
                rtb.Dock = DockStyle.Fill;
                rtb.Name = "Control1";
                form.Controls.Add(rtb);
                form.Show();
                using (Form form1 = new Form())
                {
                    form1.Name = "MyForm2";
                    RichTextBox rtb2 = new RichTextBox();
                    rtb2.Dock = DockStyle.Fill;
                    rtb2.Name = "Control1";
                    form1.Controls.Add(rtb2);
                    form1.Show();
                    form1.Activate();

                    new LogFactory(config);
                    Assert.Same(form1, target.TargetForm);
                    Assert.Same(rtb2, target.TargetRichTextBox);
                }
            }
        }

        [Fact]
        public void ActiveFormNegativeTest1()
        {
            var config = new LoggingConfiguration();
            var target = new RichTextBoxTarget()
            {
                FormName = "MyForm1",
                ControlName = "Control1",
                UseDefaultRowColoringRules = true,
                Layout = "${level} ${logger} ${message}",
                ToolWindow = false,
                Width = 300,
                Height = 200,
            };
            config.AddTarget("target", target);
            config.LoggingRules.Add(new LoggingRule("*", target));
            LogManager.ThrowExceptions = true;

            using (var form = new Form())
            {
                form.Name = "MyForm1";
                form.WindowState = FormWindowState.Minimized;

                form.Show();
                try
                {
                    new LogFactory(config);
                    Assert.True(false, "Expected exception.");
                }
                catch (NLogConfigurationException ex)
                {
                    Assert.Equal("Rich text box control 'Control1' cannot be found on form 'MyForm1'.", ex.Message);
                }
            }
        }

        [Fact]
        public void ActiveFormNegativeTest2()
        {
            var config = new LoggingConfiguration();
            RichTextBoxTarget target = new RichTextBoxTarget()
            {
                FormName = "MyForm1",
                UseDefaultRowColoringRules = true,
                Layout = "${level} ${logger} ${message}",
            };
            config.AddTarget("target", target);
            config.LoggingRules.Add(new LoggingRule("*", target));
            LogManager.ThrowExceptions = true;

            using (Form form = new Form())
            {
                form.Name = "MyForm1";
                form.WindowState = FormWindowState.Minimized;
                form.Show();

                try
                {
                    new LogFactory(config);
                    Assert.True(false, "Expected exception.");
                }
                catch (NLogConfigurationException ex)
                {
                    Assert.Equal("Rich text box control name must be specified for RichTextBoxTarget.", ex.Message);
                }
            }
        }

        [Fact]
        public void ManualRegisterTest()
        {
            var config = new LoggingConfiguration();
            var target = new RichTextBoxTarget()
            {
                FormName = "MyForm1",
                ControlName = "Control1",
                UseDefaultRowColoringRules = true,
                Layout = "${level} ${logger} ${message}",
                ToolWindow = false,
                Width = 300,
                Height = 200,
                AllowAccessoryFormCreation = false
            };
            LogManager.ThrowExceptions = true;
            SimpleConfigurator.ConfigureForTargetLogging(target, LogLevel.Trace);

            logger.Trace("No Control");
            Application.DoEvents();

            Assert.False(target.CreatedForm);
            Assert.Null(target.TargetForm);
            Assert.Null(target.TargetRichTextBox);

            using (Form form = new Form())
            {
                form.Name = "MyForm1";
                form.WindowState = FormWindowState.Minimized;
                RichTextBox rtb = new RichTextBox();
                rtb.Dock = DockStyle.Fill;
                rtb.Name = "Control1";
                form.Controls.Add(rtb);
                form.Show();
                form.Activate();

                RichTextBoxTarget.ReInitializeAllTextboxes(form);

                logger.Trace("Has Control");

                Application.DoEvents();

                Assert.False(target.CreatedForm);
                Assert.Same(form, target.TargetForm);
                Assert.Same(rtb, target.TargetRichTextBox);

                string result = ExtractRtf(target.TargetRichTextBox);

                Assert.False(result.Contains(@"No Control"));
                Assert.True(result.Contains(@"Has Control"));
            }
        }

        [Fact]
        public void ManualRegisterTestWithRetention()
        {
            var config = new LoggingConfiguration();
            var target = new RichTextBoxTarget()
            {
                FormName = "MyForm1",
                ControlName = "Control1",
                UseDefaultRowColoringRules = true,
                Layout = "${level} ${logger} ${message}",
                ToolWindow = false,
                Width = 300,
                Height = 200,
                AllowAccessoryFormCreation = false,
                MaxLines = 10,
                MessageRetention = RichTextBoxTargetMessageRetentionStrategy.OnlyMissed
            };
            LogManager.ThrowExceptions = true;
            SimpleConfigurator.ConfigureForTargetLogging(target, LogLevel.Trace);

            logger.Trace("No Control");
            Application.DoEvents();

            Assert.False(target.CreatedForm);
            Assert.Null(target.TargetForm);
            Assert.Null(target.TargetRichTextBox);

            using (Form form = new Form())
            {
                form.Name = "MyForm1";
                //with minimized forms textbox is created with width of 0, so texts get trimmed and result.Contains() fails
                //form.WindowState = FormWindowState.Minimized; 
                form.Width = 600;
                RichTextBox rtb = new RichTextBox();
                rtb.Dock = DockStyle.Fill;
                rtb.Name = "Control1";
                form.Controls.Add(rtb);
                form.Show();
                form.Activate();

                RichTextBoxTarget.ReInitializeAllTextboxes(form);

                logger.Trace("Has Control");

                Application.DoEvents();

                Assert.False(target.CreatedForm);
                Assert.Same(form, target.TargetForm);
                Assert.Same(rtb, target.TargetRichTextBox);

                string result = ExtractRtf(target.TargetRichTextBox);

                Assert.True(result.Contains(@"No Control"));
                Assert.True(result.Contains(@"Has Control"));
            }
        }

        [Fact]
        public void LinkLayoutTestDisabledLinks()
        {
            RichTextBoxTarget target = new RichTextBoxTarget()
            {
                ControlName = "Control1",
                UseDefaultRowColoringRules = true,
                Layout = "${level} ${logger} ${message} ${rtb-link:inner=descr}",
                ToolWindow = false,
                Width = 300,
                Height = 200,
                SupportLinks = false
            };

            SimpleConfigurator.ConfigureForTargetLogging(target, LogLevel.Trace);
            logger.Info("Test");

            Application.DoEvents();

            string result = ExtractRtf(target.TargetRichTextBox);
            Assert.True(Regex.IsMatch(result, @"(\([a-z0-9]{8}-[a-z0-9]{4}-[a-z0-9]{4}-[a-z0-9]{4}-[a-z0-9]{12}\))"));  //the placeholder GUID was not replaced by was not replaced because of SupportLinks set to false
        }

        [Fact]
        public void LinkTest()
        {
            RichTextBoxTarget target = new RichTextBoxTarget()
            {
                ControlName = "Control1",
                UseDefaultRowColoringRules = true,
                Layout = "${level} ${logger} ${message} ${rtb-link:inner=descr}",
                ToolWindow = false,
                Width = 300,
                Height = 200,
                SupportLinks = true
            };

            SimpleConfigurator.ConfigureForTargetLogging(target, LogLevel.Trace);
            logger.Info("Test");

            Application.DoEvents();

            Assert.Same(target, RichTextBoxTarget.GetTargetByControl(target.TargetRichTextBox));

            string resultRtf = ExtractRtf(target.TargetRichTextBox);
            string resultText = target.TargetRichTextBox.Text;
            Assert.False(Regex.IsMatch(resultRtf, @"(\([a-z0-9]{8}-[a-z0-9]{4}-[a-z0-9]{4}-[a-z0-9]{4}-[a-z0-9]{12}\))"));  //the placeholder GUID was replaced
            //cant check for specific ids here because of the possible parallel execution
            Assert.True(resultText.Contains("descr#link"));  //text contains visible and invisible parts
            Assert.True(resultRtf.Contains(@"descr\v #link"));  //RTF contains everything
        }

        [Fact]
        public void LinkTestConditional()
        {
            RichTextBoxTarget target = new RichTextBoxTarget()
            {
                ControlName = "Control1",
                UseDefaultRowColoringRules = true,
                Layout = "${level} ${logger} ${message} ${rtb-link:inner=${event-properties:item=ShowLink}}",
                ToolWindow = false,
                Width = 300,
                Height = 200,
                SupportLinks = true
            };

            SimpleConfigurator.ConfigureForTargetLogging(target, LogLevel.Trace);

            logger.Info("TestNoLink");
            Application.DoEvents();

            Assert.Same(target, RichTextBoxTarget.GetTargetByControl(target.TargetRichTextBox));

            //check first event
            {
                string resultText = target.TargetRichTextBox.Text;
                string resultRtf = ExtractRtf(target.TargetRichTextBox);
                //cant check for specific ids here because of the possible parallel execution
                Assert.True(resultText.Contains("TestNoLink"));
                Assert.False(resultText.Contains("#link"));  //no link for first event
                Assert.False(resultRtf.Contains(@"\v #link"));  //no link for first event
            }


            //log next event
            {
                LogEventInfo info = new LogEventInfo(LogLevel.Info, "", "TestWithLink");
                info.Properties["ShowLink"] = "marker_text";
                logger.Log(info);
            }
            Application.DoEvents();

            //check second event
            {
                string resultText = target.TargetRichTextBox.Text;
                string resultRtf = ExtractRtf(target.TargetRichTextBox);
                //cant check for specific ids here because of the possible parallel execution
                Assert.True(resultText.Contains("TestWithLink marker_text#link"));  //link for a second event
                Assert.True(resultRtf.Contains(@"marker_text\v #link"));  //link for a second event
            }
        }

        [Fact]
        public void LinkTestExcessLinksRemoved()
        {
            RichTextBoxTarget target = new RichTextBoxTarget()
            {
                ControlName = "Control1",
                UseDefaultRowColoringRules = true,
                Layout = "${level} ${logger} ${message} ${rtb-link:inner=${event-properties:item=LinkIndex}}",
                ToolWindow = false,
                Width = 300,
                Height = 200,
                SupportLinks = true,
                MaxLines = 5
            };

            SimpleConfigurator.ConfigureForTargetLogging(target, LogLevel.Trace);

            Assert.Same(target, RichTextBoxTarget.GetTargetByControl(target.TargetRichTextBox));

            for (int i = 0; i < 100; ++i)
            {
                LogEventInfo info = new LogEventInfo(LogLevel.Info, "", "Test");
                info.Properties["LinkIndex"] = i;
                logger.Log(info);
            }
            Application.DoEvents();

            string resultText = target.TargetRichTextBox.Text;
            string resultRtf = ExtractRtf(target.TargetRichTextBox);
            //cant check for specific ids here because of the possible parallel execution
            Assert.True(resultText.Contains("#link"));  //some links exist
            Assert.True(resultRtf.Contains(@"\v #link"));  //some links exist

            Assert.True(target.LinkedEventsCount == target.MaxLines); //storing 5, not 100 events
        }

        [Fact]
        public void LinkClickTest()
        {
            RichTextBoxTarget target = new RichTextBoxTarget()
            {
                ControlName = "Control1",
                UseDefaultRowColoringRules = true,
                Layout = "${rtb-link:inner=link}",
                ToolWindow = false,
                Width = 300,
                Height = 200,
                SupportLinks = true
            };

            SimpleConfigurator.ConfigureForTargetLogging(target, LogLevel.Trace);
            logger.Info("Test");

            Application.DoEvents();

            Assert.Same(target, RichTextBoxTarget.GetTargetByControl(target.TargetRichTextBox));
            Assert.True(target.TargetRichTextBox.Text.Contains("link"));

            bool linkClicked = false;

            RichTextBoxTarget.DelLinkClicked clickHandler = (RichTextBoxTarget sender, string linkText, LogEventInfo logEvent) => {
                linkClicked = true;
                target.TargetForm.Close();
            };

            RichTextBoxTarget.GetTargetByControl(target.TargetRichTextBox).LinkClicked += clickHandler;

            //simulate clicking on a link
            Task.Run(() => {
                for (int i = 0; i < 3; ++i) //needs a number of clicks. Probably - to make application focused, form focused, and finally link clicked.
                {
                    InvokeLambda(target.TargetRichTextBox, () =>
                    {
                        Point scrPoint = target.TargetRichTextBox.PointToScreen(new Point(5, 5));
                        LeftMouseClick(scrPoint.X, scrPoint.Y);
                    });
                }
            });

            //in case link does not click, this would hang up infinitely;
            Application.Run(target.TargetForm);

            Assert.True(linkClicked); //check that we have actually clicked on a link, not just missed anything
        }


        #region mouse click smulation
        //http://stackoverflow.com/a/8273118/376066

        [System.Runtime.InteropServices.DllImport("user32.dll")]
        private static extern bool SetCursorPos(int x, int y);

        [System.Runtime.InteropServices.DllImport("user32.dll")]
        private static extern void mouse_event(int dwFlags, int dx, int dy, int cButtons, int dwExtraInfo);

        private const int MOUSEEVENTF_ABSOLUTE = 0x8000;
        private const int MOUSEEVENTF_LEFTDOWN = 0x02;
        private const int MOUSEEVENTF_LEFTUP = 0x04;

        //This simulates a left mouse click
        private static void LeftMouseClick(int xpos, int ypos)
        {
            SetCursorPos(xpos, ypos);
            mouse_event(MOUSEEVENTF_ABSOLUTE | MOUSEEVENTF_LEFTDOWN, xpos, ypos, 0, 0);
            mouse_event(MOUSEEVENTF_ABSOLUTE | MOUSEEVENTF_LEFTUP, xpos, ypos, 0, 0);
        }
        #endregion

        private static string ExtractRtf(RichTextBox conrol)
        {
            MemoryStream ms = new MemoryStream();
            conrol.SaveFile(ms, RichTextBoxStreamType.RichText);
            string resultRtf = Encoding.UTF8.GetString(ms.GetBuffer());
            return resultRtf;
        }

        private static void InvokeLambda(Control control, Action action)
        {
            control.Invoke(action);
        }
    }
}
