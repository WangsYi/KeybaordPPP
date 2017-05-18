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
using System.Windows.Input;
using System.Windows.Media;
using System.Windows.Media.Imaging;
using System.Windows.Navigation;
using System.Windows.Shapes;
using System.Windows.Forms;
using System.Windows.Interop;
using Button = System.Windows.Controls.Button;
using KeyEventArgs = System.Windows.Input.KeyEventArgs;
using MessageBox = System.Windows.MessageBox;
using TextBox = System.Windows.Controls.TextBox;
using Timer = System.Timers.Timer;

namespace EmuKeyBoard
{
    /// <summary>
    /// MainWindow.xaml 的交互逻辑
    /// </summary>
    public partial class MainWindow : Window
    {
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
        }

        private void MainWindow_SwitchState()
        {

            Running = !Running;
            Task.Factory.StartNew(() =>
            {
                while (Running)
                {
                    Keyboard.Press(KeyToClick);
                    Thread.Sleep(IntervalTime / 2);
                    Keyboard.Release(KeyToClick);
                    Thread.Sleep(IntervalTime / 2);
                        //Timer timer = new Timer(0.1) {Enabled = true,AutoReset = false};
                        //timer.Elapsed += (s,a) =>
                        //{
                        //    Keyboard.Release(Key.A);
                        //    System.Timers.Timer t = s as Timer;
                        //    t.Enabled = false;
                        //    t.Stop();
                        //    t.Close();
                        //};
                        //timer.Start();

                    }
            });

            //System.Windows.MessageBox.Show(Running.ToString());

        }

        private void MainWindow_Loaded(object sender, RoutedEventArgs e)
        {
            hWnd = new WindowInteropHelper(this).Handle;
            var hwndSource = HwndSource.FromHwnd(hWnd);
            hwndSource?.AddHook(new HwndSourceHook(WndProc));
            RegHotkey();
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

        private void UIElement_OnKeyDown(object sender, KeyEventArgs e)
        {
            ((TextBox)sender).Text = e.Key.ToString();
            this.KeyToClick = e.Key;
        }

        public Key KeyToClick { get; set; }

        private void IntervalText_OnTextChanged(object sender, TextChangedEventArgs e)
        {
            TextBox t = sender as TextBox;
            if (t != null && !string.IsNullOrWhiteSpace(t.Text))
            {
                foreach (var c in t.Text)
                {
                    if (!Char.IsDigit(c))
                    {
                        MessageBox.Show("输数字!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!");
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
            Button b = (Button) sender;
            b.Content = Running ? "停止" : "开始";
        }
    }
}
