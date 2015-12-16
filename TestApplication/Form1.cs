using System;
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

namespace TestApplication
{
    public partial class Form1 : Form
    {
    

        public Form1()
        {
            InitializeComponent();

            NLog.Logger Logger = NLog.LogManager.GetCurrentClassLogger();
            Logger.Info("Init");
            

            var thread = new Thread(() =>
            {

                for (int i = 0; i < 1000; i++)
                {
                    Logger.Debug("{0}: a line with some length\n a new line", i);
                    Thread.Sleep(200);

                }
                Logger.Info("Done");
            });
            Logger.Info("start thread");
            thread.Start();

        }

        private void Form1_Load(object sender, EventArgs e)
        {
            RichTextBoxTarget.ReInitializeAllTextboxes(this);
            RichTextBoxTarget.GetTargetByControl(richTextBox1).LinkClicked += Form1_LinkClicked;
        }

        void Form1_LinkClicked(string linkText, LogEventInfo logEvent)
        {
            MessageBox.Show("Clicked link '" + linkText + "' for event\n" + logEvent);
        }

        
    }
}
