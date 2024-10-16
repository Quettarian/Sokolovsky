using System.Data.SqlClient;
using Serilog;
using Sokolovsky;

var folderPath = Directory.GetCurrentDirectory();

Log.Logger = new LoggerConfiguration().MinimumLevel.Information().WriteTo
    .File(Path.Combine(folderPath, "Logs", DateTime.Now.ToString("yyyyMMdd"), ".txt"),
        outputTemplate: "{Timestamp: yyyy-MM-dd HH:mm:ss.fff} [{Level:u3}]  {Message: lj} {NewLine}{Exception}",
        rollingInterval: RollingInterval.Day, 
        rollOnFileSizeLimit: true )
    .CreateLogger();
Log.Information("Запуск приложения");

while (true) {
    //1. Сканирование папки, получение списка  НОВЫХ файлов json (return List<string fileNames>)
    var fileNames = Directory.GetFiles(folderPath)
        .Select(Path.GetFileName)
        .Where(x => Path.GetExtension(x)!.Equals("json"))
        .ToList();
    
    if (fileNames.Count > 0) {
        Log.Information($"Обнаружено {fileNames.Count} новых сообщений");
        await using SqlConnection connection = new SqlConnection(AppConfiguration.Config.ConnectionString);
        await connection.OpenAsync();

        var c = 0;
        fileNames.ForEach(fileName => {
            if (Task.Run(() => new IncomingMessageHandler(fileName, connection).Processing()).Result)
                c++;
        });

        connection.Close();
        if (c == fileNames.Count)
            Log.Information("Сообщения успешно обработаны");
        else
            Log.Information($"{c} сообщений из {fileNames.Count} успешно обработаны");
    }
    
    
    Thread.Sleep(AppConfiguration.Config.MonitoringFrequency * 1000);
}

// 1. Не стал усложнять большим количеством одно-двух строчных классов и методов, т.к. для MVP мне показалось этого достаточно
// 2. Логирование можно было бы реализовать подробнее, но не уверен, что требуется
// 3. Для записи и обновления данных в БД решил не использовать ORM системыы, а только пару запросов
// 4. Не стал параллелить весь цикл, хотя, по идее, при большом количестве входных сообщений это первое,
//   что нужно сделать, но тут ещё есть над чем подумать
