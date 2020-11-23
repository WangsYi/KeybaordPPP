using System;
using System.Runtime.InteropServices;
using System.Windows.Input;

namespace KeyPPP
{
    public static class KeyHelper
    {
        public static ushort VKey2Code(System.Windows.Input.Key key)
        {
            var vk = KeyInterop.VirtualKeyFromKey(key);
            return (ushort)Win32.MapVirtualKey((uint) vk, Win32.MAPVK_VK_TO_VSC);
        }
    }
    public static class Win32
    {
        public const uint MAPVK_VK_TO_VSC = 0x00;
        public const uint MAPVK_VSC_TO_VK = 0x01;
        public const uint MAPVK_VK_TO_CHAR = 0x02;
        public const uint MAPVK_VSC_TO_VK_EX = 0x03;
        public const uint MAPVK_VK_TO_VSC_EX = 0x04;

        [DllImport("user32.dll")]
        public static extern uint MapVirtualKey(uint uCode, uint uMapType);

        public enum VKKey
        {
            // mouse movements  
            move = 0x0001,
            leftdown = 0x0002,
            leftup = 0x0004,
            rightdown = 0x0008,
            rightup = 0x0010,
            middledown = 0x0020,

            //keyboard stuff  
            LBUTTON = 1,
            RBUTTON = 2,
            CANCEL = 3,
            MBUTTON = 4,
            BACK = 8,
            TAB = 9,
            CLEAR = 12,
            RETURN = 13,
            SHIFT = 16,
            CONTROL = 17,
            MENU = 18,
            PAUSE = 19,
            CAPITAL = 20,
            ESCAPE = 27,
            SPACE = 32,
            PRIOR = 33,
            NEXT = 34,
            END = 35,
            HOME = 36,
            LEFT = 37,
            UP = 38,
            RIGHT = 39,
            DOWN = 40,
            SELECT = 41,
            PRINT = 42,
            EXECUTE = 43,
            SNAPSHOT = 44,
            INSERT = 45,
            DELETE = 46,
            HELP = 47,
            NUM0 = 48, //0  
            NUM1 = 49, //1  
            NUM2 = 50, //2  
            NUM3 = 51, //3  
            NUM4 = 52, //4  
            NUM5 = 53, //5  
            NUM6 = 54, //6  
            NUM7 = 55, //7  
            NUM8 = 56, //8  
            NUM9 = 57, //9  
            A = 65, //A  
            B = 66, //B  
            C = 67, //C  
            D = 68, //D  
            E = 69, //E  
            F = 70, //F  
            G = 71, //G  
            H = 72, //H  
            I = 73, //I  
            J = 74, //J  
            K = 75, //K  
            L = 76, //L  
            M = 77, //M  
            N = 78, //N  
            O = 79, //O  
            P = 80, //P  
            Q = 81, //Q  
            R = 82, //R  
            S = 83, //S  
            T = 84, //T  
            U = 85, //U  
            V = 86, //V  
            W = 87, //W  
            X = 88, //X  
            Y = 89, //Y  
            Z = 90, //Z  
            NUMPAD0 = 96, //0  
            NUMPAD1 = 97, //1  
            NUMPAD2 = 98, //2  
            NUMPAD3 = 99, //3  
            NUMPAD4 = 100, //4  
            NUMPAD5 = 101, //5  
            NUMPAD6 = 102, //6  
            NUMPAD7 = 103, //7  
            NUMPAD8 = 104, //8  
            NUMPAD9 = 105, //9  
            NULTIPLY = 106,
            ADD = 107,
            SEPARATOR = 108,
            SUBTRACT = 109,
            DECIMAL = 110,
            DIVIDE = 111,
            F1 = 112,
            F2 = 113,
            F3 = 114,
            F4 = 115,
            F5 = 116,
            F6 = 117,
            F7 = 118,
            F8 = 119,
            F9 = 120,
            F10 = 121,
            F11 = 122,
            F12 = 123,
            NUMLOCK = 144,
            SCROLL = 145,
            middleup = 0x0040,
            xdown = 0x0080,
            xup = 0x0100,
            wheel = 0x0800,
            virtualdesk = 0x4000,
            absolute = 0x8000
        }
    }
}