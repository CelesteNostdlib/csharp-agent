using System;
using System.Runtime.InteropServices;

namespace CSharpAgent
{
    public class User32
    {
        [UnmanagedFunctionPointer(CallingConvention.StdCall, CharSet = CharSet.Unicode)]
        public delegate int MessageBoxDelegate(IntPtr hWnd, string lpText, string lpCaption, uint uType);
        private delegate IntPtr LoadLibraryWDelegate(string lpLibFileName);

        public IntPtr ModuleHandle;
        public MessageBoxDelegate MessageBox;

        private User32(IntPtr moduleHandle, MessageBoxDelegate messageBox)
        {
            ModuleHandle = moduleHandle;
            MessageBox = messageBox;
        }

        public static bool TryCreateUser32(out User32 instance)
        {
            instance = null;
            IntPtr moduleHandle = PebWalker.ResolveModuleFromPeb("user32.dll");

            if (moduleHandle == IntPtr.Zero)
            {
                Kernel32 kernel32;
                if (!Kernel32.TryCreateKernel32(out kernel32))
                    return false;

                moduleHandle = kernel32.LoadLibraryW("user32.dll");
                if (moduleHandle == IntPtr.Zero)
                    return false;
            }

            ulong messageBoxHash = PebWalker.HashString("MessageBoxW");
            IntPtr ptrMessageBox = PebWalker.ResolveExportByHash(moduleHandle, messageBoxHash);
            if (ptrMessageBox == IntPtr.Zero)
                return false;

            MessageBoxDelegate messageBox = (MessageBoxDelegate)Marshal.GetDelegateForFunctionPointer(ptrMessageBox, typeof(MessageBoxDelegate));

            instance = new User32(moduleHandle, messageBox);
            return true;
        }
    }
}