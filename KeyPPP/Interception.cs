using System;
using System.Collections.Concurrent;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Diagnostics;
using System.IO;
using System.Linq;
using System.Runtime.InteropServices;
using System.Threading;
using System.Threading.Tasks;
using System.Windows.Documents;
using System.Windows.Media.Imaging;

namespace KeyPPP
{
    public static class InterceptionHelper
    {
        #region install/uninstall

        public static bool CheckInstall()
        {
            var context = CreateContext();
            bool installed = context != IntPtr.Zero;
            DestroyContext(context);
            return installed;
        }

        public static void Install(Action callback)
        {
            var path = Path.Combine(Environment.CurrentDirectory, "install.exe");
            var p = System.Diagnostics.Process.Start(
                path, "/install");
            if (p != null)
                p.Exited += (sender, args) => { callback?.Invoke(); };
        }

        public static void Uninstall(Action callback)
        {
            var path = Path.Combine(Environment.CurrentDirectory, "install.exe");
            var p = System.Diagnostics.Process.Start(
                path, "/uninstall");
            if (p != null)
                p.Exited += (sender, args) => { callback?.Invoke(); };
        }

        #endregion

        #region external

        [UnmanagedFunctionPointer(CallingConvention.Cdecl)]
        public delegate int Predicate(int device);

        [Flags]
        public enum KeyState
        {
            Down = 0x00,
            Up = 0x01,
            E0 = 0x02,
            E1 = 0x04,
            TermsrvSetLED = 0x08,
            TermsrvShadow = 0x10,
            TermsrvVKPacket = 0x20
        }

        [Flags]
        public enum Filter : ushort
        {
            None = 0x0000,
            All = 0xFFFF,
            KeyDown = KeyState.Up,
            KeyUp = KeyState.Up << 1,
            KeyE0 = KeyState.E0 << 1,
            KeyE1 = KeyState.E1 << 1,
            KeyTermsrvSetLED = KeyState.TermsrvSetLED << 1,
            KeyTermsrvShadow = KeyState.TermsrvShadow << 1,
            KeyTermsrvVKPacket = KeyState.TermsrvVKPacket << 1
        }

        [StructLayout(LayoutKind.Sequential)]
        public struct MouseStroke
        {
            public ushort state;
            public ushort flags;
            public short rolling;
            public int x;
            public int y;
            public uint information;
        }

        [StructLayout(LayoutKind.Sequential)]
        public struct KeyStroke
        {
            public ushort code;
            public ushort state;
            public uint information;
        }

        [StructLayout(LayoutKind.Explicit)]
        public struct Stroke
        {
            [FieldOffset(0)] public MouseStroke mouse;

            [FieldOffset(0)] public KeyStroke key;
        }

        [DllImport("interception.dll", EntryPoint = "interception_create_context",
            CallingConvention = CallingConvention.Cdecl)]
        public static extern IntPtr CreateContext();

        [DllImport("interception.dll", EntryPoint = "interception_destroy_context",
            CallingConvention = CallingConvention.Cdecl)]
        public static extern void DestroyContext(IntPtr context);

        [DllImport("interception.dll", EntryPoint = "interception_set_filter",
            CallingConvention = CallingConvention.Cdecl)]
        public static extern void SetFilter(IntPtr context, Predicate predicate, Filter filter);

        [DllImport("interception.dll", EntryPoint = "interception_receive",
            CallingConvention = CallingConvention.Cdecl)]
        public static extern int Receive(IntPtr context, int device, ref Stroke stroke, uint nstroke);

        [DllImport("interception.dll", EntryPoint = "interception_send", CallingConvention = CallingConvention.Cdecl)]
        public static extern int Send(IntPtr context, int device, ref Stroke stroke, uint nstroke);

        [DllImport("interception.dll", EntryPoint = "interception_is_keyboard",
            CallingConvention = CallingConvention.Cdecl)]
        public static extern int IsKeyboard(int device);

        [DllImport("interception.dll", EntryPoint = "interception_wait", CallingConvention = CallingConvention.Cdecl)]
        public static extern int Wait(IntPtr context);

        #endregion
    }

    public class Interception : IDisposable
    {
        public static readonly InterceptionHelper.KeyStroke CtrlDown;
        public static readonly InterceptionHelper.KeyStroke AltDown;
        public static readonly InterceptionHelper.KeyStroke ShiftDown;
        public static readonly InterceptionHelper.KeyStroke WinDown;

        static Interception()
        {
            var ctrlCode = KeyHelper.VKey2Code(System.Windows.Input.Key.LeftCtrl);
            var altCode = KeyHelper.VKey2Code(System.Windows.Input.Key.LeftAlt);
            var shiftCode = KeyHelper.VKey2Code(System.Windows.Input.Key.LeftShift);
            var winCode = KeyHelper.VKey2Code(System.Windows.Input.Key.LWin);
            CtrlDown = new InterceptionHelper.KeyStroke()
            {
                code = ctrlCode, state = (ushort) InterceptionHelper.KeyState.Down
            };
            AltDown = new InterceptionHelper.KeyStroke()
            {
                code = altCode, state = (ushort) InterceptionHelper.KeyState.Down
            };
            ShiftDown = new InterceptionHelper.KeyStroke()
            {
                code = shiftCode, state = (ushort) InterceptionHelper.KeyState.Down
            };
            WinDown = new InterceptionHelper.KeyStroke()
            {
                code = winCode, state = (ushort) InterceptionHelper.KeyState.Down
            };
        }

        public Interception(InterceptionHelper.Filter filter)
        {
            Context = InterceptionHelper.CreateContext();
            if (Context == IntPtr.Zero)
            {
                throw new Exception("初始化失败，请检查驱动是否安装（安装后需重启）");
            }

            InterceptionHelper.SetFilter(Context, InterceptionHelper.IsKeyboard, filter); //仅模拟键盘
        }

        private List<InterceptionHelper.KeyStroke> cache = new List<InterceptionHelper.KeyStroke>();

        public void BeginMonitor()
        {
            Task.Factory.StartNew(() =>
            {
                InterceptionHelper.Stroke stroke = new InterceptionHelper.Stroke();
                while (InterceptionHelper.Receive(Context, device = InterceptionHelper.Wait(Context), ref stroke, 1) >
                       0)
                {
                    Debug.WriteLine("SCAN CODE: {0}/{1}", stroke.key.code, stroke.key.state);
                    OnKeyDown?.Invoke(stroke);
                    cache.Add(stroke.key);
                    if (cache.Count > 4)
                    {
                        cache.RemoveAt(0);
                    }

                    // 按键映射
                    var km = KeyMaps?.FirstOrDefault(o => o.From.Hit(cache));
                    if (km != null)
                    {
                        km.To.Send(Context, device);
                    }

                    // 快捷键
                    var action = shortcuts.FirstOrDefault(o =>
                        (km == null && o.Key.Hit(cache)) || (km != null && km.To.GetHashCode() == o.Key.GetHashCode()));
                    if (!default(KeyValuePair<Key, Action>).Equals(action))
                    {
                        cache.Clear();
                        action.Value.Invoke();
                        continue;
                    }

                    if (km == null)
                    {
                        InterceptionHelper.Send(Context, device, ref stroke, 1);
                    }
                   
                }
            });
        }

        public void BeginNewSequence(List<KeyWithDuration> keys, bool once = true, CancellationTokenSource cts = null)
        {
            var action = new Action(() =>
            {
                do
                {
                    cts?.Token.ThrowIfCancellationRequested();
                    foreach (var key in keys)
                    {
                        var stroke = key.Stroke();
                        stroke.key.state = (ushort) InterceptionHelper.KeyState.Down;
                        InterceptionHelper.Send(Context, device, ref stroke, 1);
                        stroke.key.state = (ushort) InterceptionHelper.KeyState.Up;
                        Thread.Sleep(System.TimeSpan.FromMilliseconds(Math.Max(10, key.Duration)));
                    }
                } while (!once);
            });
            Task.Factory.StartNew(action);
        }

        public void RegisterShortcut(Key key, Action action)
        {
            shortcuts[key] = action;
        }

        public void UnRegisterShortcut(Key key)
        {
            shortcuts.TryRemove(key, out _);
        }

        private int device;
        public IntPtr Context { get; set; }
        public event Action<InterceptionHelper.Stroke> OnKeyDown;
        public ObservableCollection<KeyMap> KeyMaps;
        private readonly ConcurrentDictionary<Key, Action> shortcuts = new ConcurrentDictionary<Key, Action>();

        public void Dispose()
        {
            InterceptionHelper.DestroyContext(Context);
        }
    }
}