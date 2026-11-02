using System;
using System.Runtime.InteropServices;

namespace CSharpAgent
{
    internal class AdvApi
    {
        [UnmanagedFunctionPointer(CallingConvention.StdCall, CharSet = CharSet.Unicode)]
        public delegate int RegGetValueDelegate(IntPtr hkey, string lpSubKey, string lpValue, uint dwFlags, out uint pdwType, byte[] pvData, ref uint pcbData);

        public RegGetValueDelegate RegGetValueW; 
        public IntPtr ModuleHandle;

        private static AdvApi _cachedInstance;

        private AdvApi(IntPtr moduleHandle, RegGetValueDelegate regGetValue){
            ModuleHandle = moduleHandle;
            RegGetValueW = regGetValue;
        }

        public static bool TryCreateAdvApi(out AdvApi instance)
        {
            if (_cachedInstance != null)
            {
                instance = _cachedInstance;
                return true;
            }

            IntPtr moduleHandle = PebWalker.ResolveModuleFromPeb("advapi32.dll");

            if (moduleHandle == IntPtr.Zero)
            {
                Kernel32 kernel32 = null;
                if (Kernel32.TryCreateKernel32(out kernel32) && kernel32 != null)
                {
                    moduleHandle = kernel32.LoadLibraryW("advapi32.dll");
                }
            }

            if (moduleHandle == IntPtr.Zero)
            {
                instance = null;
                return false;
            }

            IntPtr ptrRegGetValue = PebWalker.ResolveExportByHash(moduleHandle, PebWalker.HashString("RegGetValueW"));
            if (ptrRegGetValue == IntPtr.Zero)
            {
                instance = null;
                return false;
            }

            RegGetValueDelegate regGetValue = (RegGetValueDelegate)Marshal.GetDelegateForFunctionPointer(ptrRegGetValue, typeof(RegGetValueDelegate));

            _cachedInstance = new AdvApi(moduleHandle, regGetValue);
            instance = _cachedInstance;
            return true;
        }
    }
}