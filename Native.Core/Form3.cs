using System;
using System.Runtime.InteropServices;
using System.Windows.Forms;

namespace Native.Core
{
    public partial class Form3 : Form
    {
        public Form3(bool g, string code)
        {
            InitializeComponent();
            label2.Text = g ? "群聊" : "私聊";
            textBox1.Text = code.Trim(' ', '\n');
        }

        private void button1_Click(object sender, EventArgs e)
        {
            Clipboard.SetText(textBox1.Text);
            Close();
        }

        private void Form3_Load(object sender, EventArgs e)
        {
            TopMost = true;
            TopLevel = true;
            BringToFront();
            Focus();
        }
        [DllImport("User32.dll", CharSet = CharSet.Unicode, EntryPoint = "FlashWindowEx")]
        private static extern void FlashWindowEx(ref FLASHWINFO pwfi);
        public struct FLASHWINFO
        {
            public UInt32 cbSize;//该结构的字节大小
            public IntPtr hwnd;//要闪烁的窗口的句柄，该窗口可以是打开的或最小化的
            public UInt32 dwFlags;//闪烁的状态
            public UInt32 uCount;//闪烁窗口的次数
            public UInt32 dwTimeout;//窗口闪烁的频度，毫秒为单位；若该值为0，则为默认图标的闪烁频度
        }
        public const UInt32 FLASHW_TRAY = 2;
        public const UInt32 FLASHW_TIMERNOFG = 12;
        private void FlashWin()
        {
            FLASHWINFO fInfo = new FLASHWINFO();
            fInfo.cbSize = Convert.ToUInt32(Marshal.SizeOf(fInfo));
            fInfo.hwnd = this.Handle;
            fInfo.dwFlags = FLASHW_TRAY | FLASHW_TIMERNOFG;
            fInfo.uCount = 3;// UInt32.MaxValue;
            fInfo.dwTimeout = 500;
            FlashWindowEx(ref fInfo);
        }
        private void Form3_Shown(object sender, EventArgs e)
        {
            Form3_Load(null, null);
            FlashWin();
        }
    }
}
