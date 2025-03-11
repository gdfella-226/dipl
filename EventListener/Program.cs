using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using Microsoft.Diagnostics.Tracing;
using Microsoft.Diagnostics.Tracing.Session;

class Program
{
    static void Main()
    {
        try {
            using (var session = new TraceEventSession("DiplETWSession", "C:\\Users\\Danil\\Documents\\ETW\\output.etl")) {
                ProcessStartInfo startInfo = new ProcessStartInfo();
                startInfo.FileName = @"powershell.exe";
                startInfo.Arguments = @"& 'C:\Users\Danil\Documents\ETW\dev\EventListener\GetProviders.ps1'";
                startInfo.RedirectStandardOutput = true;
                startInfo.RedirectStandardError = true;
                startInfo.UseShellExecute = false;
                startInfo.CreateNoWindow = true;
                Process process = new Process();
                process.StartInfo = startInfo;
                process.Start();
                string output = process.StandardOutput.ReadToEnd();
                var providers = output.Split('\n').ToList();

                foreach (var provider in providers) {
                    try {
                        session.EnableProvider(provider, TraceEventLevel.Informational);
                        Console.WriteLine($"Включен провайдер: {provider}");
                    } catch (Exception ex) {
                        Console.WriteLine($"Ошибка при включении провайдера {provider}: {ex.Message}");
                    }
                }

                Console.WriteLine("Сбор данных... Нажмите Enter для остановки.");
                Console.ReadLine();
            }
        }
        catch (Exception ex) {
            Console.WriteLine($"Ошибка: {ex.Message}");
        }
    }
}