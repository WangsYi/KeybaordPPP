using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Forms;

namespace EmuKeyBoard
{
   public class Hotkey
    {
        Hashtable keyIDs=new Hashtable();
        private IntPtr hWnd;
        public enum KeyFlags
        {
            MOD_ALT=0x1,
            MOD_CONTROL=0X2,
            MOD_SHIFT=0X4,
            MOD_WIN=0X8
        }

        public Hotkey(IntPtr hWnd)
        {
            this.hWnd = hWnd;
        }

        public int RegisterHotkey(Keys key, KeyFlags? keyflags)
        {
            UInt32 hotkeyId = User32Invoke.GlobalAddAtom(Guid.NewGuid().ToString());
            User32Invoke.RegisterHotKey((IntPtr) hWnd, hotkeyId, (UInt32) (keyflags ?? 0), (UInt32) key);
            keyIDs.Add(hotkeyId,hotkeyId);
            return (int)hotkeyId;
        }

        public void UnregisterHotkeys()
        {
            foreach (UInt32 keyId in keyIDs.Values)
            {
                User32Invoke.UnregisterHotKey(hWnd, keyId);
                User32Invoke.GlobalDeleteAtom(keyId);
            }
        }
    }

    public delegate void HotkeyEventHandler(int hotKeyId);
}
