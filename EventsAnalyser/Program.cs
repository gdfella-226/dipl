using System;
using System.Diagnostics.Tracing;
using System.Linq;
using System.Reflection;

namespace EventProviderExample
{
    class Program
    {
        static void Main(string[] args)
        {
            var eventSources = EventSource.GetSources().ToList();

            Console.WriteLine("Доступные провайдеры событий:");
            foreach (var eventSource in eventSources)
                Console.WriteLine($"- {eventSource.Name}");
            Console.WriteLine();

            foreach (var eventSource in eventSources) {
                Console.WriteLine($"Воспроизведение событий для провайдера: {eventSource.Name}");
                try {
                    var methods = eventSource.GetType().GetMethods(BindingFlags.Instance | BindingFlags.Public | BindingFlags.NonPublic);
                    var eventMethods = methods.Where(m => m.Name.StartsWith("Write")).ToList(); // Методы, начинающиеся с "Write" - это события

                    foreach (var method in eventMethods)
                        Console.WriteLine($"  Метод события: {method.Name}");

                } catch (Exception ex) {
                    Console.WriteLine($"Ошибка при обработке событий для {eventSource.Name}: {ex.Message}");
                }
                Console.WriteLine();
            }
            Console.Write("Any key to finish: ");
            Console.ReadLine();
        }
    }
}
