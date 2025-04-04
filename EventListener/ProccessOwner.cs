using System;
using System.Runtime.InteropServices;
using System.Text;

class ProcessOwner
{
    // Импорт необходимых WinAPI функций
    [DllImport("advapi32.dll", SetLastError = true)]
    static extern bool OpenProcessToken(IntPtr ProcessHandle, uint DesiredAccess, out IntPtr TokenHandle);

    [DllImport("advapi32.dll", SetLastError = true)]
    static extern bool GetTokenInformation(IntPtr TokenHandle, uint TokenInformationClass, 
                                        IntPtr TokenInformation, uint TokenInformationLength, 
                                        out uint ReturnLength);

    [DllImport("advapi32.dll", CharSet = CharSet.Auto, SetLastError = true)]
    static extern bool LookupAccountSid(string lpSystemName, IntPtr sid,
                                      StringBuilder lpName, ref uint cchName,
                                      StringBuilder referencedDomainName, ref uint cchReferencedDomainName,
                                      out uint peUse);

    [DllImport("kernel32.dll", SetLastError = true)]
    static extern IntPtr OpenProcess(uint dwDesiredAccess, bool bInheritHandle, int dwProcessId);

    [DllImport("kernel32.dll", SetLastError = true)]
    static extern bool CloseHandle(IntPtr hObject);

    // Константы
    const uint TOKEN_QUERY = 0x0008;
    const int TokenUser = 1;
    const uint PROCESS_QUERY_INFORMATION = 0x0400;

    public static string GetProcessOwner(int processId) {
        if(processId == -1)
            return "Kernel";
        IntPtr processHandle = OpenProcess(PROCESS_QUERY_INFORMATION, false, processId);
        if (processHandle == IntPtr.Zero)
            return $"N/A (Error: {Marshal.GetLastWin32Error()})";

        try {
            if (!OpenProcessToken(processHandle, TOKEN_QUERY, out IntPtr tokenHandle))
                return $"N/A (Error: {Marshal.GetLastWin32Error()})";

            try {
                GetTokenInformation(tokenHandle, TokenUser, IntPtr.Zero, 0, out uint tokenInfoLength);
                if (Marshal.GetLastWin32Error() != 122) // ERROR_INSUFFICIENT_BUFFER
                    return $"N/A (Error: {Marshal.GetLastWin32Error()})";

                IntPtr tokenInfo = Marshal.AllocHGlobal((int)tokenInfoLength);
                try {
                    if (!GetTokenInformation(tokenHandle, TokenUser, tokenInfo, tokenInfoLength, out _))
                        return $"N/A (Error: {Marshal.GetLastWin32Error()})";
                    IntPtr sid = Marshal.ReadIntPtr(tokenInfo);
                    return GetAccountNameFromSid(sid);
                } finally {
                    Marshal.FreeHGlobal(tokenInfo);
                }
            } finally {
                CloseHandle(tokenHandle);
            }
        } finally {
            CloseHandle(processHandle);
        }
    }

    private static string GetAccountNameFromSid(IntPtr sid) {
        StringBuilder name = new StringBuilder(256);
        StringBuilder domain = new StringBuilder(256);
        uint nameLength = (uint)name.Capacity;
        uint domainLength = (uint)domain.Capacity;
        uint sidType;

        if (!LookupAccountSid(null, sid, name, ref nameLength, domain, ref domainLength, out sidType))
            return $"SYSTEM";
        return $"{domain}\\{name}";
    }
}