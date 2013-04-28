using System;
using System.ComponentModel;
using System.Drawing;
using System.Runtime.InteropServices;
using System.Windows.Forms;

namespace HookEx 
{
    public partial class ScreenKeyboard : FormBase 
    {
        private HookEx.UserActivityHook hook;

        public ScreenKeyboard()
        {
            InitializeComponent();
            base.TopMost = true;
            base.StartPosition = FormStartPosition.CenterScreen;

            HandleClickEvents(this.Controls);

            this.hook = new HookEx.UserActivityHook(true, true);
            this.hook.MouseActivity += HookOnMouseActivity;
        }

        protected override void Dispose(bool disposing)
        {
            if (disposing)
            {
                this.hook.Stop();
                this.hook = null;

                if (components != null)
                {
                    components.Dispose();
                }
            }
            base.Dispose(disposing);
        }

        private void HookOnMouseActivity(object sener, HookEx.MouseExEventArgs e)
        {
            Point location = e.Location;

            if (e.Button == MouseButtons.Left)
            {
                Rectangle captionRect = new Rectangle(this.Location, new Size(this.Width, SystemInformation.CaptionHeight));
                if (captionRect.Contains(location))
                {
                    NativeMethods.SetWindowLong(this.Handle, KeyboardConstaint.GWL_EXSTYLE,
                        (int)NativeMethods.GetWindowLong(this.Handle, KeyboardConstaint.GWL_EXSTYLE) & (~KeyboardConstaint.WS_DISABLED));
                    NativeMethods.SendMessage(this.Handle, KeyboardConstaint.WM_SETFOCUS, IntPtr.Zero, IntPtr.Zero);
                }
                else
                {
                    NativeMethods.SetWindowLong(this.Handle, KeyboardConstaint.GWL_EXSTYLE,
                        (int)NativeMethods.GetWindowLong(this.Handle, KeyboardConstaint.GWL_EXSTYLE) | KeyboardConstaint.WS_DISABLED);
                }
            }
        }

        private void HandleClickEvents(Control.ControlCollection controls)
        {
            foreach (Control ctrl in controls)
            {
                KeyboardButton button = ctrl as KeyboardButton;
                if (button != null)
                {
                    button.Click += ButtonOnClick;
                    continue;
                }

                TableLayoutPanel panel = ctrl as TableLayoutPanel;
                if (panel != null)
                {
                    HandleClickEvents(panel.Controls);
                    continue;
                }

                TabControl tab = ctrl as TabControl;
                if (tab != null)
                {
                    HandleClickEvents(tab.Controls);
                    continue;
                }

                TabPage tabpage = ctrl as TabPage;
                if (tabpage != null)
                {
                    HandleClickEvents(tabpage.Controls);
                    continue;
                }
            }
        }

        private void MenuItemOnClick(object sender, EventArgs e) 
        {
            if (object.ReferenceEquals(sender, this.menuItemExit)) {
                this.Close();
            } else if (object.ReferenceEquals(sender, this.menuItemAbout)) {
                AboutBox aboutBox = new AboutBox();
                aboutBox.ShowDialog(this);
            }
        }

        private void ButtonOnClick(object sender, EventArgs e) 
        {
            KeyboardButton btnKey = sender as KeyboardButton;
            if (btnKey == null)
            {
                return;
            }

            int len = btnKey.Text.Length;
            for (int i = 0; i < len; i++)
            {
                SendKey(btnKey.Text[i], false);
                SendKey(btnKey.Text[i], true);
            }
        }

        private void SendKey(char chr, bool key_up) 
        {
            Input[] input = new Input[1];
            input[0].type = INPUT.KEYBOARD;
            input[0].ki.wVk = KeyboardConstaint.VK_NONE;
            input[0].ki.wScan = (short) chr;
            input[0].ki.time = NativeMethods.GetTickCount();

            input[0].ki.dwFlags = KeyboardConstaint.KEYEVENTF_UNICODE;
            if (key_up)
            {
                input[0].ki.dwFlags += KeyboardConstaint.KEYEVENTF_KEYUP;
            }

            uint res = NativeMethods.SendInput((uint)input.Length, input, Marshal.SizeOf(input[0]));
            if (res < input.Length) 
            {
                throw new Win32Exception(Marshal.GetLastWin32Error());
            }
        }
    }
}
