using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Linq;
using System.Security.Permissions;
using System.Windows;
using System.Windows.Documents;
using System.Windows.Input;

namespace KeyPPP
{
    public class Config : ConfigBase
    {
        private static Config instance;
        public static Config Instance => instance;

        private Config()
        {
            this.SimpleConfig = new SimpleConfig();
            this.SequenceConfig = new SequenceConfig();
            this.KeyMapConfig = new KeyMapConfig();
        }

        static Config()
        {
            instance = new Config();
            instance.Load();
        }

        public SimpleConfig SimpleConfig { get; set; }
        public SequenceConfig SequenceConfig { get; set; }
        public KeyMapConfig KeyMapConfig { get; set; }
    }

    public abstract class ConfigBase : NPCBase
    {
        public virtual void Save()
        {
        }

        public virtual void Load()
        {
        }
    }

    public class SimpleConfig : ConfigBase
    {
        public SimpleConfig()
        {
            this.Key = new KeyWithDuration();
            this.ShortKey = new Key();
        }

        private KeyWithDuration _key;
        private int _duration;
        private Key _shortKey;

        public KeyWithDuration Key
        {
            get => _key;
            set
            {
                _key = value;
                OnPropertyChanged();
            }
        }

        public Key ShortKey
        {
            get => _shortKey;
            set
            {
                _shortKey = value;
                OnPropertyChanged();
            }
        }
    }

    public class SequenceConfig : ConfigBase
    {
        public SequenceConfig()
        {
            this.Keys = new ObservableCollection<KeyWithDuration>();
            this.ShortKey = new Key();
        }

        private ObservableCollection<KeyWithDuration> _keys;
        private Key _shortKey;

        public ObservableCollection<KeyWithDuration> Keys
        {
            get => _keys;
            set
            {
                _keys = value;
                OnPropertyChanged();
            }
        }

        public Key ShortKey
        {
            get => _shortKey;
            set
            {
                _shortKey = value;
                OnPropertyChanged();
            }
        }
    }

    public class KeyMapConfig : ConfigBase
    {
        public KeyMapConfig()
        {
            this.KeyMaps = new ObservableCollection<KeyMap>();
        }

        private ObservableCollection<KeyMap> _keyMaps;

        public ObservableCollection<KeyMap> KeyMaps
        {
            get => _keyMaps;
            set
            {
                _keyMaps = value;
                OnPropertyChanged();
            }
        }
    }

    public class Key : NPCBase
    {
        private System.Windows.Input.Key _kKey;
        private ModifierKeys _modifierKeys;

        public System.Windows.Input.Key KKey
        {
            get => _kKey;
            set
            {
                _kKey = value;
                OnPropertyChanged();
                OnPropertyChanged(nameof(Text));
                OnPropertyChanged(nameof(ScanCode));
            }
        }

        public ModifierKeys ModifierKeys
        {
            get => _modifierKeys;
            set
            {
                _modifierKeys = value;
                OnPropertyChanged();
                OnPropertyChanged(nameof(Text));
            }
        }


        public ushort ScanCode => KeyHelper.VKey2Code(this._kKey);

        public string Text => GetKeyString();

        protected bool hasControl()
        {
            return (this.ModifierKeys & ModifierKeys.Control) != 0;
        }

        protected bool hasAlt()
        {
            return (this.ModifierKeys & ModifierKeys.Alt) != 0;
        }

        protected bool hasShift()
        {
            return (this.ModifierKeys & ModifierKeys.Shift) != 0;
        }

        protected bool hasWin()
        {
            return (this.ModifierKeys & ModifierKeys.Windows) != 0;
        }

        protected int ModifierCount()
        {
            int n = 0;
            if (hasControl())
            {
                n++;
            }

            if (hasAlt())
            {
                n++;
            }

            if (hasShift())
            {
                n++;
            }

            if (hasWin())
            {
                n++;
            }

            return n;
        }

        public string GetKeyString()
        {
            string s = "";
            if (this.KKey != System.Windows.Input.Key.LeftCtrl && this.KKey != System.Windows.Input.Key.RightCtrl &&
                (this.ModifierKeys & ModifierKeys.Control) != 0)
            {
                s += "Ctrl+";
            }

            if (this.KKey != System.Windows.Input.Key.LeftAlt && this.KKey != System.Windows.Input.Key.RightAlt &&
                (this.ModifierKeys & ModifierKeys.Alt) != 0)
            {
                s += "Alt+";
            }

            if (this.KKey != System.Windows.Input.Key.LeftShift && this.KKey != System.Windows.Input.Key.RightShift &&
                (this.ModifierKeys & ModifierKeys.Shift) != 0)
            {
                s += "Shift+";
            }

            if (this.KKey != System.Windows.Input.Key.LWin && this.KKey != System.Windows.Input.Key.RWin &&
                (this.ModifierKeys & ModifierKeys.Windows) != 0)
            {
                s += "Win+";
            }

            return s + KKey;
        }

        public InterceptionHelper.Stroke Stroke()
        {
            var stroke = new InterceptionHelper.Stroke();
            stroke.key.code = this.ScanCode;
            return stroke;
        }

        public bool Hit(List<InterceptionHelper.KeyStroke> keyStrokes)
        {
            keyStrokes = keyStrokes.GetRange(keyStrokes.Count - this.ModifierCount() - 1, this.ModifierCount() + 1);
            bool isHit = true;
            Application.Current.Dispatcher.Invoke(() =>
            {
                isHit &= !hasControl() || (Keyboard.IsKeyDown(System.Windows.Input.Key.LeftCtrl) || Keyboard.IsKeyDown(System.Windows.Input.Key.RightCtrl));
                isHit &= !hasAlt() || (Keyboard.IsKeyDown(System.Windows.Input.Key.LeftAlt) || Keyboard.IsKeyDown(System.Windows.Input.Key.RightAlt));
                isHit &= !hasShift() || (Keyboard.IsKeyDown(System.Windows.Input.Key.LeftShift) || Keyboard.IsKeyDown(System.Windows.Input.Key.RightShift));
                isHit &= !hasWin() || (Keyboard.IsKeyDown(System.Windows.Input.Key.LWin) || Keyboard.IsKeyDown(System.Windows.Input.Key.RWin));
                isHit &= Keyboard.IsKeyDown(this.KKey);
                
            });
            return isHit;
        }

        public override int GetHashCode()
        {
            return Text.GetHashCode();
        }
    }

    public class KeyWithDuration : Key
    {
        private int _duration;

        public int Duration
        {
            get => _duration;
            set
            {
                _duration = value;
                OnPropertyChanged();
            }
        }
    }

    public class KeyWithSend : Key
    {
        public void Send(IntPtr context, int device)
        {
            var stroke = new InterceptionHelper.Stroke();
            if (this.hasControl())
            {
                stroke.key = Interception.CtrlDown;
                InterceptionHelper.Send(context, device, ref stroke, 1);
            }
            if (this.hasAlt())
            {
                stroke.key = Interception.AltDown;
                InterceptionHelper.Send(context, device, ref stroke, 1);
            }
            if (this.hasShift())
            {
                stroke.key = Interception.ShiftDown;
                InterceptionHelper.Send(context, device, ref stroke, 1);
            }
            if (this.hasWin())
            {
                stroke.key = Interception.WinDown;
                InterceptionHelper.Send(context, device, ref stroke, 1);
            }
            stroke.key = new InterceptionHelper.KeyStroke(){code = this.ScanCode,state = (ushort)InterceptionHelper.KeyState.Down};
            InterceptionHelper.Send(context, device, ref stroke, 1);
            stroke.key = new InterceptionHelper.KeyStroke(){code = this.ScanCode,state = (ushort)InterceptionHelper.KeyState.Up};
            InterceptionHelper.Send(context, device, ref stroke, 1);
            if (this.hasControl())
            {
                stroke.key = Interception.CtrlDown;
                stroke.key.state = (ushort)InterceptionHelper.KeyState.Up;
                InterceptionHelper.Send(context, device, ref stroke, 1);
            }
            if (this.hasAlt())
            {
                stroke.key = Interception.AltDown;
                stroke.key.state = (ushort)InterceptionHelper.KeyState.Up;
                InterceptionHelper.Send(context, device, ref stroke, 1);
            }
            if (this.hasShift())
            {
                stroke.key = Interception.ShiftDown;
                stroke.key.state = (ushort)InterceptionHelper.KeyState.Up;
                InterceptionHelper.Send(context, device, ref stroke, 1);
            }
            if (this.hasWin())
            {
                stroke.key = Interception.WinDown;
                stroke.key.state = (ushort)InterceptionHelper.KeyState.Up;
                InterceptionHelper.Send(context, device, ref stroke, 1);
            }
        }
    }
    public class KeyMap : NPCBase
    {
        private Key _from;
        private KeyWithSend _to;

        public Key From
        {
            get => _from;
            set
            {
                _from = value;
                OnPropertyChanged();
            }
        }

        public KeyWithSend To
        {
            get => _to;
            set
            {
                _to = value;
                OnPropertyChanged();
            }
        }
    }
}