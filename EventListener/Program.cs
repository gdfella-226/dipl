using System;
using Microsoft.Diagnostics.Tracing;
using Microsoft.Diagnostics.Tracing.Session;
using Microsoft.Diagnostics.Tracing.Parsers;
using Microsoft.Diagnostics.Tracing.Parsers.Kernel;

class Program
{
    static void Main()
    {
        try
        {
            using (var session = new TraceEventSession("MySession", "C:\\Users\\Danil\\Documents\\ETW\\output.etl"))
            {
                session.EnableProvider(
                    new Guid("{22FB2CD6-0E7B-422B-A0C7-2FAD1FD0E716}"),
                    TraceEventLevel.Informational,
                    (ulong)KernelTraceEventParser.Keywords.Process);

                session.EnableProvider(
                    new Guid("{7DD42A49-5329-4832-8DFD-43D979153A88}"), 
                    TraceEventLevel.Informational,                      
                    (ulong)KernelTraceEventParser.Keywords.NetworkTCPIP);

                Console.WriteLine("Сбор данных... Нажмите Enter для остановки.");
                Console.ReadLine();
            }

            using (var source = new ETWTraceEventSource("output.etl"))
            {
                var kernelParser = new KernelTraceEventParser(source);
                kernelParser.ProcessStart += data => Console.WriteLine($"Process started: {data.ProcessName}");
                kernelParser.ProcessStop += data => Console.WriteLine($"Process stopped: {data.ProcessName}");
                kernelParser.TcpIpRecv += data => Console.WriteLine($"Received TCP/IP-packet: {data.PayloadString(0)}");;
                kernelParser.TcpIpSend += data => Console.WriteLine($"Get TCP/IP-packet: {data.PayloadString(0)}");;

                source.Process();
            }
        }
        catch (Exception ex)
        {
            Console.WriteLine($"Error: {ex.Message}");
        }
    }

}