using System;
using System.IO;

using System.Linq;
using System.Net;
using Microsoft.Win32;
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
    [DllImport("advapi32.dll", SetLastError = true)]
    static extern bool OpenProcessToken(IntPtr ProcessHandle, uint DesiredAccess, out IntPtr TokenHandle);
    [DllImport("advapi32.dll", SetLastError = true)]
    static extern bool GetTokenInformation(IntPtr TokenHandle, uint TokenInformationClass, IntPtr TokenInformation, uint TokenInformationLength, out uint ReturnLength);
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
                    KernelTraceEventParser.Keywords.Registry |
                    KernelTraceEventParser.Keywords.Process |
                    KernelTraceEventParser.Keywords.DiskFileIO);
                /*session.EnableProvider(
                    new Guid("54849625-5478-4994-A5BA-3E3B0328C30D"),
                    TraceEventLevel.Informational,
                    0x80000000000 | // Audit Success
                    0x90000000000
                );
                session.EnableProvider(
                    new Guid("DBEEF1C5-1A1A-4772-A2E9-3F2B7B3D22D9"),
                    TraceEventLevel.Verbose,
                    0x1 
                );*/
                /*string registryPath = @"SOFTWARE\Microsoft\Windows\CurrentVersion\WINEVT\Publishers";
                using (RegistryKey key = Registry.LocalMachine.OpenSubKey(registryPath)) {
                    if (key != null)
                        foreach (string providerGuid in key.GetSubKeyNames())
                            try {
                                session.EnableProvider(providerGuid, TraceEventLevel.Informational);
                                if (providerGuid == "{DBEEF1C5-1A1A-4772-A2E9-3F2B7B3D22D9}")
                                    Console.WriteLine($"Включен провайдер: {providerGuid}");
                            } catch (Exception ex) {
                                Console.WriteLine($"Ошибка при включении провайдера {providerGuid}: {ex.Message}");
                            }
                }*/

                /*session.Source.Dynamic.All += data =>
                {
                    
                    if (data.ProviderName == "Microsoft-Windows-Security-Auditing")
                        Console.WriteLine(data.Dump());

                    switch (data.EventName)
                    {
                        case "4624": // Вход
                        case "4634": // Выход
                        case "4663": // Доступ к файлу
                            break;
                    }
                };

                session.Source.Dynamic.AddCallbackForProviderEvent(
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

                session.Source.Kernel.All += data =>
                {
                    string payload = "N/A";
                    string opcodeName = data.OpcodeName ?? "N/A";

                    if (opcodeName.StartsWith("File"))
                        payload = data.PayloadByName("FileName")?.ToString() 
                            ?? data.PayloadString(0) 
                            ?? "N/A";
                    else if (opcodeName.StartsWith("Reg"))
                        payload = $"{data.PayloadByName("KeyName")}={data.PayloadByName("ValueName")}";
                    else if (opcodeName is "Connect" or "Accept")
                        payload = $"{data.PayloadByName("daddr")}:{data.PayloadByName("dport")}";
                    else if (opcodeName.Contains("Process"))
                        payload = data.PayloadByName("CmdLine").ToString();

                    if (data.ProcessName != "EventListener" && payload != "N/A"){
                            Console.WriteLine($"{data.OpcodeName} ({data.Opcode})");
                            var opcode = data.Opcode.ToString() != "" ? data.Opcode.ToString() : "0" ;
                            var opcName = data.OpcodeName.ToString() != "" ? data.OpcodeName.ToString() : "N/A" ;
                            var time = data.TimeStamp.ToString() != "" ? data.TimeStamp.ToString() : "N/A" ;
                            var pid = data.ProcessID.ToString() != "" ? data.ProcessID.ToString() : "0" ;
                            var name = data.ProcessName.ToString() != "" ? data.ProcessName.ToString() : "N/A" ;
                            if (pid == "-1")
                                name = "System";
                            var username = ProcessOwner.GetProcessOwner(data.ProcessID);
                            var line = $"{opcode}, {opcName}, {time}, {pid}, {name}, {username}, {payload}";
                            lock (_lock) {
                                writer.WriteLine(line);
                                writer.Flush();
                            }
                        }
                    
                };
                Console.WriteLine("Monitoring... Press Enter to stop");
                var processingTask = Task.Run(() => session.Source.Process());
                Console.ReadLine();
                session.Stop();
            }
        }
    }
}

