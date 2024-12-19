using System;
using System.Collections.Generic;
using System.Diagnostics.Tracing;
using System.Linq;

namespace EventGen
{
    class Program
    {
        static List<EventSource> getProviders(){
            var eventSources = EventSource.GetSources().ToList();
            Console.WriteLine("Доступные провайдеры событий:");
            foreach (var eventSource in eventSources)
                Console.WriteLine($"- {eventSource.Name}");
            Console.WriteLine();
            return eventSources;
        }


        static void Main(string[] args) {
            var eventSources = getProviders();
            foreach (var eventSource in eventSources) {
                if (eventSource.Name == "Microsoft-Windows-DotNETRuntime")
                    Console.WriteLine($"Воспроизведение событий для провайдера: {eventSource.Name}");
                    try {
                        DotNetRuntime_EG.Produce();
                    } catch (Exception ex) {
                        Console.WriteLine($"Ошибка при обработке событий для {eventSource.Name}: {ex.Message}");
                    }
                }
            Console.Write("Any key to finish: ");
            Console.ReadLine();
        }
    }
}
