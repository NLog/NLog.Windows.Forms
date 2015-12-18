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

using System;
using System.Drawing;
using System.Runtime.InteropServices;
using System.Windows.Forms;

namespace NLog.Windows.Forms
{
    /// <summary>
    /// Form helper methods.
    /// </summary>
    internal class FormHelper
    {
        /// <summary>
        /// Creates RichTextBox and docks in parentForm.
        /// </summary>
        /// <param name="name">Name of RichTextBox.</param>
        /// <param name="parentForm">Form to dock RichTextBox.</param>
        /// <returns>Created RichTextBox.</returns>
        [System.Diagnostics.CodeAnalysis.SuppressMessage("Microsoft.Reliability", "CA2000:Dispose objects before losing scope", Justification = "Objects are disposed elsewhere")]
        internal static RichTextBox CreateRichTextBox(string name, Form parentForm)
        {
            var rtb = new RichTextBox
            {
                Dock = DockStyle.Fill,
                Location = new Point(0, 0),
                Name = name,
                Size = new Size(parentForm.Width, parentForm.Height)
            };
            parentForm.Controls.Add(rtb);
            return rtb;
        }

        /// <summary>
        /// Finds control embedded on searchControl.
        /// </summary>
        /// <param name="name">Name of the control.</param>
        /// <param name="searchControl">Control in which we're searching for control.</param>
        /// <returns>A value of null if no control has been found.</returns>
        internal static Control FindControl(string name, Control searchControl)
        {
            if (searchControl.Name == name)
            {
                return searchControl;
            }

            foreach (Control childControl in searchControl.Controls)
            {
                Control foundControl = FindControl(name, childControl);
                if (foundControl != null)
                {
                    return foundControl;
                }
            }

            return null;
        }

        /// <summary>
        /// Finds control of specified type embended on searchControl.
        /// </summary>
        /// <typeparam name="TControl">The type of the control.</typeparam>
        /// <param name="name">Name of the control.</param>
        /// <param name="searchControl">Control in which we're searching for control.</param>
        /// <returns>
        /// A value of null if no control has been found.
        /// </returns>
        internal static TControl FindControl<TControl>(string name, Control searchControl)
            where TControl : Control
        {
            if (searchControl.Name == name)
            {
                TControl foundControl = searchControl as TControl;
                if (foundControl != null)
                {
                    return foundControl;
                }
            }

            foreach (Control childControl in searchControl.Controls)
            {
                TControl foundControl = FindControl<TControl>(name, childControl);

                if (foundControl != null)
                {
                    return foundControl;
                }
            }

            return null;
        }

        /// <summary>
        /// Creates a form.
        /// </summary>
        /// <param name="name">Name of form.</param>
        /// <param name="width">Width of form.</param>
        /// <param name="height">Height of form.</param>
        /// <param name="show">Auto show form.</param>
        /// <param name="showMinimized">If set to <c>true</c> the form will be minimized.</param>
        /// <param name="toolWindow">If set to <c>true</c> the form will be created as tool window.</param>
        /// <returns>Created form.</returns>
        [System.Diagnostics.CodeAnalysis.SuppressMessage("Microsoft.Globalization", "CA1303:Do not pass literals as localized parameters", MessageId = "System.Windows.Forms.Control.set_Text(System.String)", Justification = "Does not need to be localized.")]
        [System.Diagnostics.CodeAnalysis.SuppressMessage("Microsoft.Reliability", "CA2000:Dispose objects before losing scope", Justification = "Objects are disposed elsewhere")]
        [System.Diagnostics.CodeAnalysis.SuppressMessage("Microsoft.Naming", "CA2204:Literals should be spelled correctly", Justification = "Using property names in message.")]
        internal static Form CreateForm(string name, int width, int height, bool show, bool showMinimized, bool toolWindow)
        {
            var f = new Form
            {
                Name = name,
                Text = "NLog",
                Icon = GetNLogIcon()
            };

#if !Smartphone
            if (toolWindow)
            {
                f.FormBorderStyle = FormBorderStyle.SizableToolWindow;
            }
#endif
            if (width > 0)
            {
                f.Width = width;
            }

            if (height > 0)
            {
                f.Height = height;
            }

            if (show)
            {
                if (showMinimized)
                {
                    f.WindowState = FormWindowState.Minimized;
                    f.Show();
                }
                else
                {
                    f.Show();
                }
            }

            return f;
        }

        private static Icon GetNLogIcon()
        {
            using (var stream = typeof(FormHelper).Assembly.GetManifestResourceStream("NLog.Windows.Forms.Resources.NLog.ico"))
            {
                return new Icon(stream);
            }
        }

#region Link support
        /// <summary>
        /// Replaces currently selected text in the RTB control with a link
        /// </summary>
        /// <param name="textBox">target control</param>
        /// <param name="text">visible text of the new link</param>
        /// <param name="hyperlink">value of the new link</param>
        internal static void ChangeSelectionToLink(RichTextBox textBox, string text, string hyperlink)
        {
            int selectionStart = textBox.SelectionStart;
            textBox.SelectedRtf = @"{\rtf1\ansi " + text + @"\v #" + hyperlink + @"\v0}";
            //textBox.Select(position, text.Length + hyperlink.Length + 1);
            textBox.Select(selectionStart, text.Length + hyperlink.Length + 1);
            SetSelectionStyle(textBox, CFM_LINK, CFE_LINK);
            //textBox.Select(position + text.Length + hyperlink.Length + 1, 0);
        }

        private static void SetSelectionStyle(RichTextBox textBox, UInt32 mask, UInt32 effect)
        {
            CHARFORMAT2_STRUCT cf = new CHARFORMAT2_STRUCT();
            cf.cbSize = (UInt32)Marshal.SizeOf(cf);
            cf.dwMask = mask;
            cf.dwEffects = effect;

            IntPtr wpar = new IntPtr(SCF_SELECTION);
            IntPtr lpar = Marshal.AllocCoTaskMem(Marshal.SizeOf(cf));
            Marshal.StructureToPtr(cf, lpar, false);

            IntPtr res = SendMessage(textBox.Handle, EM_SETCHARFORMAT, wpar, lpar);

            Marshal.FreeCoTaskMem(lpar);
        }

        [StructLayout(LayoutKind.Sequential)]
        private struct CHARFORMAT2_STRUCT
        {
            public UInt32 cbSize;
            public UInt32 dwMask;
            public UInt32 dwEffects;
            public Int32 yHeight;
            public Int32 yOffset;
            public Int32 crTextColor;
            public byte bCharSet;
            public byte bPitchAndFamily;
            [MarshalAs(UnmanagedType.ByValArray, SizeConst = 32)]
            public char[] szFaceName;
            public UInt16 wWeight;
            public UInt16 sSpacing;
            public int crBackColor; // Color.ToArgb() -> int
            public int lcid;
            public int dwReserved;
            public Int16 sStyle;
            public Int16 wKerning;
            public byte bUnderlineType;
            public byte bAnimation;
            public byte bRevAuthor;
            public byte bReserved1;
        }

        private const int WM_USER = 0x0400;
        private const int EM_SETCHARFORMAT = WM_USER + 68;
        private const int SCF_SELECTION = 0x0001;
        private const UInt32 CFE_LINK = 0x0020;
        private const UInt32 CFM_LINK = 0x00000020;

        [DllImport("user32.dll", CharSet = CharSet.Auto)]
        private static extern IntPtr SendMessage(IntPtr hWnd, int msg, IntPtr wParam, IntPtr lParam);
#endregion
    }
}