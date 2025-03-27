# Dipl

## EventAnalyser
Программный модуль для воспроизведения событий провайдера **Microsoft-Windows-DotNETRuntime**

#### Порядок запуска:

1. Установить [.NET Framework](https://dotnet.microsoft.com/en-us/download/dotnet-framework) версии не ниже 5.0
2. Открыть файл решения ```./EventsAnalyser/EventsAnalyser.sln``` в среде разработки, поддерживающей .NET (MS Visual Studio или VS Code)
3. Запустить проект

#### Результат работы:

В случае успешного выполнения программы, в системе произойдут события, предоставляемые провайдером Microsoft-Windows-DotNETRuntime. Отследить события можно любым соответствующим инструментом, дополнительная информация выводится в консоли.

При некорректном завершении, сообщения об ошибках будут отражены в консоли.

## EventListener
Программный модуль для получения событий от всех доступных провайдеров в системе

#### Порядок запуска:

1. Установить [.NET Framework](https://dotnet.microsoft.com/en-us/download/dotnet-framework) версии не ниже 5.0
2. Открыть файл решения ```./EventListener/EventListener.sln``` в среде разработки, поддерживающей .NET (MS Visual Studio или VS Code)
3. Запустить проект

#### Результат работы:

При запуске программы генерируется список доступных провайдеров, создается сессия, к которой подключаются эти провайдеры и начинается запись событий. После завершения записи, формируется выходной файл ```output.etl```

С помощью утилиты **tracerpt** этот файл преобразуется в формат csv для дальнейшей обработки:

```powershell
tracerpt C:\Path\to\output\file.etl -o C:\Path\to\new\file.csv -of CSV
```

При некорректном завершении, сообщения об ошибках будут отражены в консоли.