using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Data;
using System.Windows.Documents;
using System.Windows.Forms;
using System.Windows.Input;
using System.Windows.Interop;
using System.Windows.Media;
using System.Windows.Media.Imaging;
using System.Windows.Navigation;
using System.Windows.Shapes;

namespace EmuKeyAdvance
{
    /// <summary>
    /// MainWindow.xaml 的交互逻辑
    /// </summary>
    public partial class MainWindow : Window
    {
        private const string dllPath = @".\dd71800x64.64.dll";
        private CDD dd;
        private int hotKeyRes;
        private IntPtr hWnd;
        private bool Running;
        private event HotKeyPressedHandler SwitchState;
        private delegate void HotKeyPressedHandler();
        
        public MainWindow()
        {
            InitializeComponent();
            this.SwitchState += MainWindow_SwitchState;
            this.Loaded += MainWindow_Loaded;
            this.Closed += MainWindow_Closed;
        }

        private void MainWindow_Closed(object sender, EventArgs e)
        {
          
        }

        private void MainWindow_SwitchState()
        {
            
            Running = !Running;
            ButtonStart.Content = Running ? "停止" : "开始";
            Task.Factory.StartNew(() =>
            {
                while (Running)
                {
                    dd.key(306, 1);
                    Thread.Sleep(IntervalTime / 2);
                    dd.key(306, 2);
                    Thread.Sleep(IntervalTime / 2);

                }
            });

            //System.Windows.MessageBox.Show(Running.ToString());

        }

        private void MainWindow_Loaded(object sender, RoutedEventArgs e)
        {

            dd = new CDD();
            LoadDllFile(dllPath);
            hWnd = new WindowInteropHelper(this).Handle;
            var hwndSource = HwndSource.FromHwnd(hWnd);
            hwndSource?.AddHook(new HwndSourceHook(WndProc));
            RegHotkey();
        }
        private void LoadDllFile(string dllfile)
        {
         

            System.IO.FileInfo fi = new System.IO.FileInfo(dllfile);
            if (!fi.Exists)
            {
                System.Windows.MessageBox.Show("文件不存在");
                return;
            }

            int ret = dd.Load(dllfile);
            if (ret == -2) { System.Windows.MessageBox.Show("装载库时发生错误"); return; }
            if (ret == -1) { System.Windows.MessageBox.Show("取函数地址时发生错误"); return; }
            if (ret == 0) { System.Windows.MessageBox.Show("非增强模块"); }

            return;
        }
        #region 快捷键相关
        private IntPtr WndProc(IntPtr hwnd, int msg, IntPtr wparam, IntPtr lparam, ref bool handled)
        {
            if (msg == 0x312)
            {
                if ((int)wparam == hotKeyRes)
                {
                    SwitchState?.Invoke();
                }
            }
            //throw new NotImplementedException();
            return IntPtr.Zero;
        }

        void RegHotkey()
        {
            Hotkey hotkey = new Hotkey(hWnd);
            hotKeyRes = hotkey.RegisterHotkey(Keys.F10, null);
        }

        #endregion

        private void UIElement_OnKeyDown(object sender, System.Windows.Input.KeyEventArgs e)
        {
            ((System.Windows.Controls.TextBox)sender).Text = e.Key.ToString();
            this.KeyToClick = e.Key;
        }

        public Key KeyToClick { get; set; }

        private void IntervalText_OnTextChanged(object sender, TextChangedEventArgs e)
        {
            System.Windows.Controls.TextBox t = sender as System.Windows.Controls.TextBox;
            if (t != null && !string.IsNullOrWhiteSpace(t.Text))
            {
                foreach (var c in t.Text)
                {
                    if (!Char.IsDigit(c))
                    {
                        System.Windows.MessageBox.Show("输数字!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!");
                        t.Text = "";
                        break;
                    }
                }
                this.IntervalTime = int.Parse(t.Text);
            }
        }

        public int IntervalTime { get; set; } = 200;

        private void ButtonBase_OnClick(object sender, RoutedEventArgs e)
        {
            SwitchState?.Invoke();
            System.Windows.Controls.Button b = (System.Windows.Controls.Button)sender;
            b.Content = Running ? "停止" : "开始";
        }
    }
}
