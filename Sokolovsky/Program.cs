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
        
        fileNames.ForEach(fileName => new IncomingMessageHandler(fileName, connection).Processing());

        connection.Close();
        Log.Information("Сообщения успешно обработаны");
    }
    
    
    Thread.Sleep(AppConfiguration.Config.MonitoringFrequency * 1000);
}

