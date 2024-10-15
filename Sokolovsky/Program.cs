using System.Data.SqlClient;
using Serilog;
using Sokolovsky;

const string folderPath = @"d:\Sokolovsky\";
const string connectionString = "User ID=root;Password=myPassword;Host=localhost;Port=5432;Database=myDataBase;Pooling=true;Min Pool Size=0;Max Pool Size=100;Connection Lifetime=0;";
const int frequency = 10000;

Log.Logger = new LoggerConfiguration().MinimumLevel.Information().WriteTo
    .File(Path.Combine(Directory.GetCurrentDirectory(), "Logs", DateTime.Now.ToString("yyyyMMdd"), ".txt"),
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
        await using SqlConnection connection = new SqlConnection(connectionString);
        await connection.OpenAsync();
        
        fileNames.ForEach(fileName => new IncomingMessageHandler(fileName, connection).Processing());

        connection.Close();
    }
    
    
    Thread.Sleep(frequency);
}

