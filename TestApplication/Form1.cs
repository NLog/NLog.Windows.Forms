﻿using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Data;
using System.Drawing;
using System.Linq;
using System.Text;
using System.Threading;
using System.Threading.Tasks;
using System.Windows.Forms;
using NLog;
using NLog.Windows.Forms;
using System.Globalization;

namespace TestApplication
{
    public partial class Form1 : Form
    {
        public Form1()
        {
            InitializeComponent();

            NLog.Logger Logger = NLog.LogManager.GetCurrentClassLogger();
            Logger.Info("Init");
            
            RichTextBoxTarget.ReInitializeAllTextboxes(this);

            var thread = new Thread(() =>
            {
                Random rnd = new Random();
                for (int i = 0; i < 10; i++)
                {
                    LogEventInfo theEvent = new LogEventInfo(LogLevel.Debug, "", i + ": a line with some length\n a new line");
                    if (rnd.NextDouble() > 0.1)
                    {
                        theEvent.Properties["ShowLink"] = "link via property";
                    }
                    if (rnd.NextDouble() > 0.5)
                    {
                        theEvent.Properties["ShowLink2"] = "Another link";
                    }
                    Logger.Log(theEvent);
                    Thread.Sleep(200);
                }
                Logger.Info("Done");
            });
            Logger.Info("start thread");
            thread.Start();

        }

        private void Form1_Load(object sender, EventArgs e)
        {
            RichTextBoxTarget.GetTargetByControl(richTextBox1).LinkClicked += Form1_LinkClicked;
            RichTextBoxTarget.GetTargetByControl(richTextBox2).LinkClicked += Form1_LinkClicked;
        }

        private void Form1_LinkClicked(RichTextBoxTarget sender, string linkText, LogEventInfo logEvent)
        {
            //COM HRESULT E_FAIL happens when not used BeginInvoke and links are clicked while spinning
            BeginInvokeLambda(this,
                () => { MessageBox.Show("Clicked link '" + linkText + "' for event\n" + logEvent, sender.Name); }
            );
        }

        private static IAsyncResult BeginInvokeLambda(Control control, Action action)
        {
            if (!control.IsDisposed)
            {
                return control.BeginInvoke(action, null);
            }
            return null;
        }
    }
}
