using Microsoft.Win32;
using System;

class Program
{
    static void Main()
    {
        string registryPath = @"SOFTWARE\Microsoft\Windows\CurrentVersion\WINEVT\Publishers";

        using (RegistryKey key = Registry.LocalMachine.OpenSubKey(registryPath))
        {
            if (key != null)
            {
                foreach (string providerGuid in key.GetSubKeyNames())
                {
                    Console.WriteLine(providerGuid);
                }
            }
            else
            {
                Console.WriteLine("Не удалось открыть реестр. Запусти от админа!");
            }
        }
    }
}