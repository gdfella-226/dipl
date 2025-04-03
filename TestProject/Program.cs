string registryPath = @"SOFTWARE\Microsoft\Windows\CurrentVersion\WINEVT\Publishers";
                using (RegistryKey key = Registry.LocalMachine.OpenSubKey(registryPath)) {
                    if (key != null)
                        foreach (string providerGuid in key.GetSubKeyNames())
                            try {
                                session.EnableProvider(providerGuid, TraceEventLevel.Informational);
                                Console.WriteLine($"Включен провайдер: {providerGuid}");
                            } catch (Exception ex) {
                                Console.WriteLine($"Ошибка при включении провайдера {providerGuid}: {ex.Message}");
                            }
                }
