using System;
using System.Diagnostics;
using System.Runtime.InteropServices;
using System.Windows;
using System.Windows.Interop;

namespace SmartExit
{
    public partial class MainWindow : Window
    {
        public MainWindow()
        {
            InitializeComponent();
        }

        [DllImport("user32.dll")]
        private static extern bool RegisterHotKey(IntPtr hWnd, int id, uint fsModifiers, uint vk);

        [DllImport("user32.dll")]
        private static extern bool UnregisterHotKey(IntPtr hWnd, int id);

        private const int HOTKEY_ID = 9000;
        
        private const uint MOD_CONTROL = 0x0002;
        private const uint VK_TAB = 0x09;

        private IntPtr _windowHandle;
        private HwndSource _source;

        protected override void OnSourceInitialized(EventArgs e)
        {
            base.OnSourceInitialized(e);

            _windowHandle = new WindowInteropHelper(this).Handle;
            _source = HwndSource.FromHwnd(_windowHandle);
            _source.AddHook(HwndHook);

            RegisterHotKey(_windowHandle, HOTKEY_ID, MOD_CONTROL, VK_TAB);
        }

        private IntPtr HwndHook(IntPtr hwnd, int msg, IntPtr wParam, IntPtr lParam, ref bool handled)
        {
            const int WM_HOTKEY = 0x0312;
            if (msg != WM_HOTKEY)
                return IntPtr.Zero;
            if (wParam.ToInt32() != HOTKEY_ID)
                return IntPtr.Zero;
            int vkey = ((int) lParam >> 16) & 0xFFFF;
            if (vkey == VK_TAB)
                SmartExit();
            handled = true;
            return IntPtr.Zero;
        }

        protected override void OnClosed(EventArgs e)
        {
            _source.RemoveHook(HwndHook);
            UnregisterHotKey(_windowHandle, HOTKEY_ID);
            base.OnClosed(e);
        }

        private void SmartExit_OnClick(object sender, RoutedEventArgs e)
        {
            SmartExit();
        }

        private void SmartExit()
        {
            string startupPath = System.IO.Path.GetDirectoryName(System.Reflection.Assembly.GetExecutingAssembly().Location);

            Process cPorts = new Process
            {
                StartInfo =
                {
                    FileName = $@"{startupPath}/cports/cports.exe",
                    Arguments = @"/close * * * * Tibia.exe",
                    UseShellExecute = false,
                    CreateNoWindow = false,
                    RedirectStandardOutput = true,
                    Verb = "runas"
                }
            };
            cPorts.Start();
            cPorts.WaitForExit();
        }
    }
}
