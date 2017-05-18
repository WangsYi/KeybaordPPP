using System;
using System.Collections.Generic;
using System.Linq;
using System.Runtime.InteropServices;
using System.Text;
using System.Threading.Tasks;

namespace EmuKeyBoard
{
   public class User32Invoke
   {
       [DllImport("user32.dll")]
       public static extern UInt32 RegisterHotKey(IntPtr hWnd, UInt32 id, UInt32 fsModifiers, UInt32 vk);

       [DllImport("user32.dll")]
       public static extern UInt32 UnregisterHotKey(IntPtr hWnd, UInt32 id);

       [DllImport("kernel32.dll")]
       public static extern UInt32 GlobalAddAtom(string lpString);

       [DllImport("kernel32.dll")]
       public static extern UInt32 GlobalDeleteAtom(UInt32 nAtom);
   }
}
