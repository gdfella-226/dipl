using Microsoft.Diagnostics.Tracing;
using Microsoft.Diagnostics.Tracing.Session;


class Program
{
    static void Main()
    {
        using (var session = new TraceEventSession("AuthMonitor"))
        {
            // Включить провайдер с Audit Success/Failure
            session.EnableProvider(
                new Guid("54849625-5478-4994-A5BA-3E3B0328C30D"),
                TraceEventLevel.Informational,
                0x80000000000 | 0x90000000000
            );

            // Обработчик для событий входа
            session.Source.Dynamic.All += data => 
            {
                if (data.ProviderName == "Microsoft-Windows-Security-Auditing")
                {
                    switch ((int)data.ID)
                    {
                        case 4624: // Успешный вход
                            Console.WriteLine($"Logon: {data.PayloadByName("TargetUserName")}");
                            break;
                        case 4634: // Выход
                            Console.WriteLine($"Logoff: {data.PayloadByName("TargetUserName")}");
                            break;
                    }
                }
            };

            Task.Run(() => session.Source.Process());
            Console.ReadLine();
        }
    }
}
