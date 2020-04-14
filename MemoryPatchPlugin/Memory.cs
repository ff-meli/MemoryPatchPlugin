using System;
using System.Diagnostics;
using System.Runtime.InteropServices;

// Simple wrapper around Read/WriteProcessMemory
namespace MemoryPatchPlugin
{
    static class Memory
    {
        [DllImport("kernel32.dll", SetLastError = true)]
        private static extern bool ReadProcessMemory(
            IntPtr hProcess,
            IntPtr lpBaseAddress,
            [Out] byte[] lpBuffer,
            int dwSize,
            out IntPtr lpNumberOfBytesRead);

        [DllImport("kernel32.dll", SetLastError = true)]
        private static extern bool WriteProcessMemory(
            IntPtr hProcess,
            IntPtr lpBaseAddress,
            byte[] lpBuffer,
            int nSize,
            out IntPtr lpNumberOfBytesWritten);

        public static byte[] Read(IntPtr address, int numBytesToRead)
        {
            if (numBytesToRead <= 0)
            {
                throw new ArgumentOutOfRangeException();
            }

            var buffer = new byte[numBytesToRead];
            // storing the handle was failing sometimes...??
            if (!ReadProcessMemory(Process.GetCurrentProcess().Handle, address, buffer, numBytesToRead, out _))
            {
                throw new Exception("ReadProcessMemory failed with win32 error " + Marshal.GetLastWin32Error());
            }
            return buffer;
        }

        public static byte[] Read(IntPtr address, byte[] buffer, int numBytesToRead)
        {
            if (numBytesToRead <= 0 || numBytesToRead >= buffer.Length)
            {
                throw new ArgumentOutOfRangeException();
            }

            if (!ReadProcessMemory(Process.GetCurrentProcess().Handle, address, buffer, numBytesToRead, out _))
            {
                throw new Exception("ReadProcessMemory failed with win32 error " + Marshal.GetLastWin32Error());
            }
            return buffer;
        }

        public static void Write(IntPtr address, byte[] bytes)
        {
            if (!WriteProcessMemory(Process.GetCurrentProcess().Handle, address, bytes, bytes.Length, out _))
            {
                throw new Exception("WriteProcessMemory failed with win32 error " + Marshal.GetLastWin32Error());
            }
        }
    }
}
