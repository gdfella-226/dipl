using System;
using Microsoft.Diagnostics.Tracing;
using Microsoft.Diagnostics.Tracing.Session;

class Program
{
    static void Main()
    {
        try
        {
            using (var session = new TraceEventSession("MySession"))
            {
                // Получаем список всех зарегистрированных провайдеров
                var providers = TraceEventSession.GetRegisteredProviders();

                // Включаем каждый провайдер
                foreach (var provider in providers)
                {
                    try
                    {
                        session.EnableProvider(provider, TraceEventLevel.Informational);
                        Console.WriteLine($"Включен провайдер: {provider}");
                    }
                    catch (Exception ex)
                    {
                        Console.WriteLine($"Ошибка при включении провайдера {provider}: {ex.Message}");
                    }
                }

                Console.WriteLine("Сбор данных... Нажмите Enter для остановки.");
                Console.ReadLine();
            }
        }
        catch (Exception ex)
        {
            Console.WriteLine($"Ошибка: {ex.Message}");
        }
    }
}