using System;
using System.IO;
using System.Linq;
using System.Net;
using System.Diagnostics;
using System.Threading.Tasks;
using System.Collections.Generic;
using System.Runtime.InteropServices;
using Microsoft.Diagnostics.Tracing;
using Microsoft.Diagnostics.Tracing.Parsers;
using Microsoft.Diagnostics.Tracing.Session;
using Microsoft.Diagnostics.Tracing.StackSources;



class Program
{
    // WinAPI импорт для получения информации о пользователе процесса
    [DllImport("advapi32.dll", SetLastError = true)]
    static extern bool OpenProcessToken(IntPtr ProcessHandle, uint DesiredAccess, out IntPtr TokenHandle);

    [DllImport("advapi32.dll", SetLastError = true)]
    static extern bool GetTokenInformation(
        IntPtr TokenHandle,
        uint TokenInformationClass,
        IntPtr TokenInformation,
        uint TokenInformationLength,
        out uint ReturnLength);

    [DllImport("kernel32.dll", SetLastError = true)]
    static extern IntPtr OpenProcess(uint dwDesiredAccess, bool bInheritHandle, int dwProcessId);

    [DllImport("kernel32.dll", SetLastError = true)]
    static extern bool CloseHandle(IntPtr hObject);
    private static readonly object _lock = new object();

    static void Main()
    {
        if (!(TraceEventSession.IsElevated() ?? false)) {
            Console.WriteLine("Admin privelegies are required!");
            return;
        }
        
        string filePath = @"C:\Users\Danil\Documents\ETW\custom-output.csv";
        List<string> csv_data = new List<string>();
        TraceEventSession.GetActiveSession("Dipl")?.Stop();
        if(File.Exists(filePath))
            File.Delete(filePath);

        using (var writer = new StreamWriter(filePath, append: true)) {
            writer.WriteLine("OperationCode, OperationName, Time, PID, ProcessName, User, Payload");
            using (var session = new TraceEventSession("SystemMonitorSession")) {
                session.EnableKernelProvider(
                    KernelTraceEventParser.Keywords.FileIO |
                    KernelTraceEventParser.Keywords.NetworkTCPIP |
                    KernelTraceEventParser.Keywords.FileIOInit |
                    KernelTraceEventParser.Keywords.ImageLoad |
                    KernelTraceEventParser.Keywords.Registry);
                session.EnableProvider(
                    new Guid("54849625-5478-4994-A5BA-3E3B0328C30D"), // Microsoft-Windows-Authentication
                    TraceEventLevel.Informational, 0xffffffffffffffff);

                /*session.Source.Kernel.ProcessStart += data =>
                {
                    string username = ProcessOwner.GetProcessOwner(data.ProcessID);
                    Console.WriteLine($"[PROCESS START {data.ID}] " +
                        $"\n\tTime: {DateTime.Now:HH:mm:ss.fff}" +
                        $"\n\tPID: {data.ProcessID}" +
                        $"\n\tUser: {username}" +
                        $"\n\tName: {data.ProcessName}" +
                        $"\n\tCmdLine: {data.CommandLine}\n");
                };*/

                /*session.Source.Kernel.ProcessStop += data =>
                {
                    string username = ProcessOwner.GetProcessOwner(data.ProcessID);
                    Console.WriteLine($"[PROCESS STOP {data.ID}] " +
                        $"\n\tTime: {DateTime.Now:HH:mm:ss.fff}" +
                        $"\n\tPID: {data.ProcessID}" +
                        $"\n\tUser: {username}" +
                        $"\n\tName: {data.ProcessName}" +
                        $"\n\tCmdLine: {data.CommandLine}\n");
                };*/

                /*session.Source.Dynamic.AddCallbackForProviderEvent(
                    "Microsoft-Windows-Security-Auditing",
                    "4624",
                    data =>
                    {
                        Console.WriteLine($"[LOGON EVENT] " +
                            $"\n\tTime: {DateTime.Now:HH:mm:ss.fff}" +
                            $"\n\tUser: {data.PayloadByName("TargetUserName")}" +
                            $"\n\tDomain: {data.PayloadByName("TargetDomainName")}" +
                            $"\n\tSessionID: {data.PayloadByName("SessionId")}" +
                            $"\n\tLogonType: {data.PayloadByName("LogonType")}\n");
                    });

                session.Source.Dynamic.AddCallbackForProviderEvent(
                    "Microsoft-Windows-Security-Auditing",
                    "4634",
                    data =>
                    {
                        Console.WriteLine($"[LOGOFF EVENT] " +
                            $"\n\tTime: {DateTime.Now:HH:mm:ss.fff}" +
                            $"\n\tUser: {data.PayloadByName("TargetUserName")}" +
                            $"\n\tSessionID: {data.PayloadByName("SessionId")}\n");
                    });*/

                session.Source.Kernel.TcpIpConnect += data =>
                {
                    

                    string username = ProcessOwner.GetProcessOwner(data.ProcessID);
                    Console.WriteLine($"[{data.OpcodeName} ({data.Opcode})] " +
                        $"\n\tTime: {data.TimeStamp}" +
                        $"\n\tPID: {data.ProcessID}" +
                        $"\n\tUser: {username}" +
                        $"\n\tName: {data.ProcessName}" +
                        $"\n\tAddress: {data.daddr}:{data.dport}");
                    var line = $"{data.Opcode}, {data.OpcodeName}, {data.TimeStamp}, {data.ProcessID}, {data.ProcessName}, {username}, {data.daddr}:{data.dport}";
                    lock (_lock) {
                        writer.WriteLine(line);
                        writer.Flush();
                    }
                };

                session.Source.Kernel.FileIOFileCreate += data =>
                {
                    string username = ProcessOwner.GetProcessOwner(data.ProcessID);
                    Console.WriteLine($"[{data.OpcodeName} ({data.Opcode})] " +
                        $"\n\tTime: {DateTime.Now:HH:mm:ss.fff}" +
                        $"\n\tPID: {data.ProcessID}" +
                        $"\n\tUser: {username}" +
                        $"\n\tName: {data.ProcessName}" +
                        $"\n\tFile: {data.FileName}");
                    var line = $"{data.Opcode}, {data.OpcodeName}, {data.TimeStamp}, {data.ProcessID}, {data.ProcessName}, {username}, {data.FileName}";
                    lock (_lock) {
                        writer.WriteLine(line);
                        writer.Flush();
                    }
                };

                session.Source.Kernel.FileIOFileDelete += data =>
                {
                    string username = ProcessOwner.GetProcessOwner(data.ProcessID);
                    Console.WriteLine($"[{data.OpcodeName} ({data.Opcode})] " +
                        $"\n\tTime: {DateTime.Now:HH:mm:ss.fff}" +
                        $"\n\tPID: {data.ProcessID}" +
                        $"\n\tUser: {username}" +
                        $"\n\tName: {data.ProcessName}" +
                        $"\n\tFile: {data.FileName}");
                    var line = $"{data.Opcode}, {data.OpcodeName}, {data.TimeStamp}, {data.ProcessID}, {data.ProcessName}, {username}, {data.FileName}";
                    lock (_lock) {
                        writer.WriteLine(line);
                        writer.Flush();
                    }
                };

                session.Source.Kernel.FileIOWrite += data =>
                {
                    string username = ProcessOwner.GetProcessOwner(data.ProcessID);
                    Console.WriteLine($"[{data.OpcodeName} ({data.Opcode})] " +
                        $"\n\tTime: {DateTime.Now:HH:mm:ss.fff}" +
                        $"\n\tPID: {data.ProcessID}" +
                        $"\n\tUser: {username}" +
                        $"\n\tName: {data.ProcessName}" +
                        $"\n\tFile: {data.FileName}");
                    var line = $"{data.Opcode}, {data.OpcodeName}, {data.TimeStamp}, {data.ProcessID}, {data.ProcessName}, {username}, {data.FileName}";
                    lock (_lock) {
                        writer.WriteLine(line);
                        writer.Flush();
                    }
                };

                Console.WriteLine("Системный мониторинг запущен. Нажмите Enter для остановки...");
                var processingTask = Task.Run(() => session.Source.Process());
                Console.ReadLine();
                session.Stop();
            }
        }
    }
}

