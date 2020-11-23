using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Diagnostics;
using System.Runtime.CompilerServices;
using System.Threading;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Input;
using KeyPPP.Annotations;

namespace KeyPPP
{
    /// <summary>
    /// Interaction logic for MainWindow.xaml
    /// </summary>
    public partial class MainWindow:INotifyPropertyChanged
    {
        private Config _config = Config.Instance;

        public Config Config
        {
            get => _config;
            set
            {
                _config = value;
                OnPropertyChanged();
            }
        }

        private string _simpleHeader = "单键连击";

        public string SimpleHeader
        {
            get => _simpleHeader;
            set
            {
                _simpleHeader = value;
                OnPropertyChanged();
            }
        }
        private Interception _interception;
        public MainWindow()
        {
            InitializeComponent();
            this.Loaded += OnLoaded;
            this.Closed+= OnClosed;
            this.DataContext = this;
        }

        private void OnClosed(object sender, EventArgs e)
        {
            _interception?.Dispose();
        }

        private void OnLoaded(object sender, RoutedEventArgs e)
        {
            _interception = new Interception(InterceptionHelper.Filter.KeyDown);
            _interception.BeginMonitor();
        }

        public event PropertyChangedEventHandler PropertyChanged;

        [NotifyPropertyChangedInvocator]
        protected virtual void OnPropertyChanged([CallerMemberName] string propertyName = null)
        {
            PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(propertyName));
        }

        private void Key_OnIsKeyboardFocusedChanged(object sender, DependencyPropertyChangedEventArgs e)
        {
            if ((bool) e.NewValue)
            {
                _interception.OnKeyDown += InterceptionOnOnKeyDown;
            }
            else
            {
                _interception.OnKeyDown -= InterceptionOnOnKeyDown;
            }
        }

        private void InterceptionOnOnKeyDown(InterceptionHelper.Stroke stroke)
        {
            // 设置快捷键
            
        }
        private CancellationTokenSource simpleCts = new CancellationTokenSource();
        private bool _simpleStopped = true;

        private void Key_OnKeyDown(object sender, KeyEventArgs e)
        {
        //    var tb = sender as TextBox;
        //    var ctx = tb.DataContext as SimpleConfig;
        //    ctx.Key.VKey = e.Key;
        //    ctx.Key.ModifierKeys = e.KeyboardDevice.Modifiers;
        //    e.Handled = true;
        }

        public bool SimpleStopped
        {
            get => _simpleStopped;
            set
            {
                _simpleStopped = value;
                OnPropertyChanged();
            }
        }

        private void SwitchSimple()
        {
            if (!_simpleStopped)
            {
                simpleCts.Cancel();
                SimpleHeader = "单键连击";
                _simpleStopped = true;
                Application.Current.Dispatcher.Invoke(() =>
                {
                    tiSimple.IsEnabled = true;
                });
            }
            else
            {
                Application.Current.Dispatcher.Invoke(() =>
                {
                    tiSimple.IsEnabled = false;
                });
                simpleCts = new CancellationTokenSource();
                _interception.BeginNewSequence(new List<KeyWithDuration>(){Config.SimpleConfig.Key}, false, simpleCts);
                SimpleHeader = "单键连击(运行中...)";
                _simpleStopped = false;
               
            }
            
        }

        private void SimpleShortcut_OnKeyDown(object sender, KeyEventArgs e)
        {
            var list = new List<System.Windows.Input.Key>
            {
                System.Windows.Input.Key.LeftCtrl,
                System.Windows.Input.Key.RightCtrl,
                System.Windows.Input.Key.LeftShift,
                System.Windows.Input.Key.RightShift,
                System.Windows.Input.Key.LeftAlt,
                System.Windows.Input.Key.RightAlt,
                System.Windows.Input.Key.LWin,
                System.Windows.Input.Key.RWin,
            };
            if (list.Contains(e.Key))
            {
                return;
            }
            var tb = sender as TextBox;
            var ctx = tb.DataContext as SimpleConfig;
            _interception.UnRegisterShortcut(ctx.ShortKey);
            ctx.ShortKey.KKey = e.Key;
            ctx.ShortKey.ModifierKeys = e.KeyboardDevice.Modifiers;
            _interception.RegisterShortcut(ctx.ShortKey, SwitchSimple);
            e.Handled = true;
        }

        private void Key_PreviewKeyDown(object sender, KeyEventArgs e)
        {
            var tb = sender as TextBox;
            var ctx = tb.DataContext as SimpleConfig;
            ctx.Key.KKey = e.Key;
            ctx.Key.ModifierKeys = e.KeyboardDevice.Modifiers;
            e.Handled = true;
        }
    }
}