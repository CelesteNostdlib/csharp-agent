using System;
using System.Runtime.InteropServices;

namespace CSharpAgent
{
    public class AdvApi
    {
        [UnmanagedFunctionPointer(CallingConvention.StdCall, CharSet = CharSet.Unicode)]
        public delegate int RegGetValueDelegate(IntPtr hkey, string lpSubKey, string lpValue, uint dwFlags, out uint pdwType, byte[] pvData, ref uint pcbData);

        public RegGetValueDelegate RegGetValueW; 
        public IntPtr ModuleHandle;

        private AdvApi(IntPtr moduleHandle, RegGetValueDelegate regGetValue){
            ModuleHandle = moduleHandle;
            RegGetValueW = regGetValue;
        }

        public static bool TryCreateAdvApi(out AdvApi instance)
        {
            instance = null;
            IntPtr moduleHandle = PebWalker.ResolveModuleFromPeb("advapi32.dll");
            if (moduleHandle == IntPtr.Zero)
                return false;

            IntPtr ptrRegGetValue = PebWalker.ResolveExportByHash(moduleHandle, PebWalker.HashString("RegGetValueW"));
            if (ptrRegGetValue == IntPtr.Zero)
                return false;

            RegGetValueDelegate regGetValue = (RegGetValueDelegate)Marshal.GetDelegateForFunctionPointer(ptrRegGetValue, typeof(RegGetValueDelegate));

            instance = new AdvApi(moduleHandle, regGetValue);

            return true;
        }
    }
}