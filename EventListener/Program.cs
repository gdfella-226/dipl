using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using Microsoft.Diagnostics.Tracing;
using Microsoft.Diagnostics.Tracing.Session;
using Microsoft.Diagnostics.Tracing.Parsers;
using System.Diagnostics.Eventing.Reader;
using Microsoft.Win32;

class Program
{
    static void Main()
    {
        try {
            using (var session = new TraceEventSession("DiplETWSession")) {     //, "C:\\Users\\Danil\\Documents\\ETW\\output.etl"
                /*string registryPath = @"SOFTWARE\Microsoft\Windows\CurrentVersion\WINEVT\Publishers";
                using (RegistryKey key = Registry.LocalMachine.OpenSubKey(registryPath)) {
                    if (key != null)
                        foreach (string providerGuid in key.GetSubKeyNames())
                            try {
                                session.EnableProvider(providerGuid, TraceEventLevel.Informational);
                                Console.WriteLine($"Включен провайдер: {providerGuid}");
                            } catch (Exception ex) {
                                Console.WriteLine($"Ошибка при включении провайдера {providerGuid}: {ex.Message}");
                            }
                }*/

                var kernelProviderGuid = new Guid("9E814AAD-3204-11D2-9A82-006008A86939");
                session.EnableKernelProvider(
                    KernelTraceEventParser.Keywords.Process | 
                    KernelTraceEventParser.Keywords.Thread);

                // 2. Microsoft-Windows-Authentication for logon events
                // {DBEEF1C5-1A1A-4772-A2E9-3F2B7B3D22D9}
                var authProviderGuid = new Guid("DBEEF1C5-1A1A-4772-A2E9-3F2B7B3D22D9");
                session.EnableProvider(authProviderGuid);

                // 3. Microsoft-Windows-TCPIP for network events
                // {2F07E2EE-15DB-40F1-90EF-9D7BA282188A}
                var tcpipProviderGuid = new Guid("2F07E2EE-15DB-40F1-90EF-9D7BA282188A");
                session.EnableProvider(tcpipProviderGuid);

                // 4. Microsoft-Windows-HttpService for HTTP events
                // {DD5EF90A-6398-47A4-AD34-4DCECDEF795F}
                var httpProviderGuid = new Guid("DD5EF90A-6398-47A4-AD34-4DCECDEF795F");
                session.EnableProvider(httpProviderGuid);


                Console.WriteLine("Сбор данных... Нажмите Enter для остановки.");
                var task = System.Threading.Tasks.Task.Run(() =>
                {
                    session.Source.Process();
                });

                // Установка обработчиков для конкретных событий
                /*session.Source.Kernel.ProcessStart += data =>
                {
                    Console.WriteLine($"Процесс создан: PID={data.ProcessID} Имя={data.ProcessName} " +
                                      $"Командная строка={data.CommandLine}");
                };

                session.Source.Kernel.ProcessStop += data =>
                {
                    Console.WriteLine($"Процесс завершен: PID={data.ProcessID} Имя={data.ProcessName}");
                };*/

                // Для событий входа/выхода нужно использовать соответствующие провайдеры
                // Это пример, конкретные события могут отличаться
                session.Source.Dynamic.AddCallbackForProviderEvent(
                    "Microsoft-Windows-Authentication",
                    "Logon", 
                    data =>
                    {
                        Console.WriteLine($"Вход в систему: Пользователь={data.PayloadByName("TargetUserName")} " +
                                          $"Домен={data.PayloadByName("TargetDomainName")}");
                    });

                session.Source.Dynamic.AddCallbackForProviderEvent(
                    "Microsoft-Windows-Authentication",
                    "Logoff", 
                    data =>
                    {
                        Console.WriteLine($"Выход из системы: Пользователь={data.PayloadByName("TargetUserName")}");
                    });

                // Для HTTP трафика можно использовать Microsoft-Windows-HttpService
                session.Source.Dynamic.AddCallbackForProviderEvent(
                    "Microsoft-Windows-HttpService",
                    "SendRequest", 
                    data =>
                    {
                        Console.WriteLine($"HTTP запрос: URL={data.PayloadByName("Url")}");
                    });

                Console.ReadLine();
                session.Stop();


            }
        }
        catch (Exception ex) {
            Console.WriteLine($"Ошибка: {ex.Message}");
        }
    }
}

