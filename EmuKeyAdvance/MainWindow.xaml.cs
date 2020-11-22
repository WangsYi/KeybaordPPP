using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Runtime.Serialization.Formatters.Binary;
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
            Interception.DestroyContext(context);
        }

        private void MainWindow_SwitchState()
        {

            Running = !Running;
            ButtonStart.Content = Running ? "停止" : "开始";
            Task.Factory.StartNew(() =>
            {
                while (Running)
                {

                    Thread.Sleep(IntervalTime / 2);
                    var s = KeyToClick;
                    s.key.state = 0;
                    Interception.Send(context, tdevice, ref s, 1);
                    s.key.state = 1;
                    Interception.Send(context, tdevice, ref s, 1);
                    
                    Thread.Sleep(IntervalTime / 2);

                }
            });

            //System.Windows.MessageBox.Show(Running.ToString());

        }

        private void MainWindow_Loaded(object sender, RoutedEventArgs e)
        {

            Init();
            hWnd = new WindowInteropHelper(this).Handle;
            var hwndSource = HwndSource.FromHwnd(hWnd);
            hwndSource?.AddHook(new HwndSourceHook(WndProc));
            RegHotkey();
        }

        private IntPtr context;
        private int tdevice;
        Interception.Stroke stroke = new Interception.Stroke();
        private void Init()
        {
            int device;
            context = Interception.CreateContext();
            
            Console.WriteLine($"context:{context}");
            Interception.SetFilter(context, Interception.IsKeyboard, Interception.Filter.KeyDown);
            Task.Factory.StartNew(() =>
            {
                while (Interception.Receive(context, device = Interception.Wait(context), ref stroke, 1) > 0)
                {
                    Console.WriteLine("SCAN CODE: {0}/{1}", stroke.key.code, stroke.key.state);
                    if (getFocus)
                    {
                        tdevice = device;
                        KeyToClick = stroke;
                    }

                  


                    Interception.Send(context, device, ref stroke, 1);

                }
            });

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
            hotKeyRes = hotkey.RegisterHotkey(Keys.W, Hotkey.KeyFlags.MOD_ALT);
        }

        #endregion

        private void UIElement_OnKeyDown(object sender, System.Windows.Input.KeyEventArgs e)
        {
            ((System.Windows.Controls.TextBox)sender).Text = e.Key.ToString();
        }

        public Interception.Stroke KeyToClick { get; set; }

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

        private bool getFocus = false;
        private void UIElement_OnGotKeyboardFocus(object sender, KeyboardFocusChangedEventArgs e)
        {
            var ele = sender as UIElement;
            getFocus = true;
            int device;
          

        }

        private void UIElement_OnLostKeyboardFocus(object sender, KeyboardFocusChangedEventArgs e)
        {
            getFocus = false;
        }
    }
    public static class Tools
    {
        public static T Clone<T>(T obj)
        {
            MemoryStream ms = new MemoryStream();
            BinaryFormatter formatter = new BinaryFormatter();
            formatter.Serialize(ms, obj);
            return (T)formatter.Deserialize(ms);
        }
    }
}
