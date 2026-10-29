using System;
using System.Runtime.InteropServices;

namespace CSharpAgent
{
    internal class NtdllApi
    {
        [UnmanagedFunctionPointer(CallingConvention.StdCall)]
        public delegate int RtlGetVersionDelegate(ref OSVERSIONINFOW versionInfo);

        [UnmanagedFunctionPointer(CallingConvention.StdCall)]
        public delegate int NtQuerySystemInformationDelegate(int systemInformationClass, IntPtr systemInformation, uint systemInformationLength, out uint returnLength);

        [UnmanagedFunctionPointer(CallingConvention.StdCall)]
        public delegate IntPtr RtlAllocateHeapDelegate(IntPtr heapHandle, uint flags, UIntPtr size);

        [UnmanagedFunctionPointer(CallingConvention.StdCall)]
        public delegate bool RtlFreeHeapDelegate(IntPtr heapHandle, uint flags, IntPtr memoryPointer);

        [UnmanagedFunctionPointer(CallingConvention.StdCall)]
        public delegate int NtAllocateVirtualMemoryDelegate(IntPtr processHandle, ref IntPtr baseAddress, IntPtr zeroBits, ref UIntPtr regionSize, uint allocationType, uint protect);

        public RtlGetVersionDelegate RtlGetVersion;
        public NtQuerySystemInformationDelegate NtQuerySystemInformation;
        public RtlAllocateHeapDelegate RtlAllocateHeap;
        public RtlFreeHeapDelegate RtlFreeHeap;
        public NtAllocateVirtualMemoryDelegate NtAllocateVirtualMemory;

        public IntPtr ModuleHandle;
        private static NtdllApi _cached;

        private NtdllApi(
            IntPtr moduleHandle,
            RtlGetVersionDelegate rtlGetVersion,
            NtQuerySystemInformationDelegate zwQuerySystemInformation,
            RtlAllocateHeapDelegate rtlAllocateHeap,
            RtlFreeHeapDelegate rtlFreeHeap,
            NtAllocateVirtualMemoryDelegate ntAllocateVirtualMemory)
        {
            ModuleHandle = moduleHandle;
            RtlGetVersion = rtlGetVersion;
            NtQuerySystemInformation = zwQuerySystemInformation;
            RtlAllocateHeap = rtlAllocateHeap;
            RtlFreeHeap = rtlFreeHeap;
            NtAllocateVirtualMemory = ntAllocateVirtualMemory;
        }

        public static bool TryCreateNtdll(out NtdllApi instance)
        {
            if (_cached != null)
            {
                instance = _cached;
                return true;
            }
            instance = null;

            IntPtr moduleHandle = PebWalker.ResolveModuleFromPeb("ntdll.dll");
            if (moduleHandle == IntPtr.Zero)
                return false;

            IntPtr ptrRtlGetVersion = PebWalker.ResolveExportByHash(moduleHandle, PebWalker.HashString("RtlGetVersion"));
            if (ptrRtlGetVersion == IntPtr.Zero)
                return false;
            var rtlGetVersion = (RtlGetVersionDelegate)Marshal.GetDelegateForFunctionPointer(ptrRtlGetVersion, typeof(RtlGetVersionDelegate));

            IntPtr ptrNtQuerySystemInformation = PebWalker.ResolveExportByHash(moduleHandle, PebWalker.HashString("NtQuerySystemInformation"));
            if (ptrNtQuerySystemInformation == IntPtr.Zero)
                return false;
            var ntQuerySystemInformation = (NtQuerySystemInformationDelegate)Marshal.GetDelegateForFunctionPointer(ptrNtQuerySystemInformation, typeof(NtQuerySystemInformationDelegate));

            IntPtr ptrRtlAllocateHeap = PebWalker.ResolveExportByHash(moduleHandle, PebWalker.HashString("RtlAllocateHeap"));
            if (ptrRtlAllocateHeap == IntPtr.Zero)
                return false;
            var rtlAllocateHeap = (RtlAllocateHeapDelegate)Marshal.GetDelegateForFunctionPointer(ptrRtlAllocateHeap, typeof(RtlAllocateHeapDelegate));

            IntPtr ptrRtlFreeHeap = PebWalker.ResolveExportByHash(moduleHandle, PebWalker.HashString("RtlFreeHeap"));
            if (ptrRtlFreeHeap == IntPtr.Zero)
                return false;
            var rtlFreeHeap = (RtlFreeHeapDelegate)Marshal.GetDelegateForFunctionPointer(ptrRtlFreeHeap, typeof(RtlFreeHeapDelegate));

            IntPtr ptrNtAllocateVirtualMemory = PebWalker.ResolveExportByHash(moduleHandle, PebWalker.HashString("NtAllocateVirtualMemory"));
            if (ptrNtAllocateVirtualMemory == IntPtr.Zero)
                return false;
            var ntAllocateVirtualMemory = (NtAllocateVirtualMemoryDelegate)Marshal.GetDelegateForFunctionPointer(ptrNtAllocateVirtualMemory, typeof(NtAllocateVirtualMemoryDelegate));

            _cached = new NtdllApi(
                moduleHandle,
                rtlGetVersion,
                ntQuerySystemInformation,
                rtlAllocateHeap,
                rtlFreeHeap,
                ntAllocateVirtualMemory
            );

            instance = _cached;
            return true;
        }
    }
}