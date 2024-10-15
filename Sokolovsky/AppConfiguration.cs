using Newtonsoft.Json;
using Serilog;

namespace Sokolovsky;

public class AppConfiguration {
    private static Config _config;
    private string _connectionString;
    private int _monitoringFrequency;
    private const string _fileName = "SokolovskyConfig.json";
    
    private AppConfiguration(){}
    
    public static Config Config {
        get {
            if (_config != null) return _config;
            try {
                _config = JsonConvert.DeserializeObject<Config>(ReadFromFile());
            }
            catch (Exception e) {
                Log.Warning("Ошибка чтения файла конфигурации, инициализация конфигурации по-умолчанию");
                _config = Create();
                try {
                    using var sw = new StreamWriter(_fileName);
                    sw.Write(JsonConvert.SerializeObject(_config));
                }
                catch (Exception exception) {
                    Log.Warning("Ошибка сохранения файла конфигурации");
                }
            }

            return _config;
        }
    }

    private static string ReadFromFile() {
        var configFullName = Path.Combine(Directory.GetCurrentDirectory(), _fileName);
        using var r = new StreamReader(configFullName);
        return r.ReadToEnd();
    }

    private static Config Create() {
        const string connectionString =
            "User ID=root;Password=myPassword;Host=localhost;Port=5432;Database=myDataBase;Pooling=true;Min Pool Size=0;Max Pool Size=100;Connection Lifetime=0;";
        const int frequency = 60;
        return new Config(connectionString, frequency);
    }
}

public class Config(string connectionString, int monitoringFrequency) {
    public string ConnectionString => connectionString;
    
    /// <summary> Частота просмотра папки сообщений в секундах </summary>
    public int MonitoringFrequency => monitoringFrequency;
}