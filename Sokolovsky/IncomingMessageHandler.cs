using System.Data.SqlClient;
using System.Text;
using System.Xml.Serialization;
using Newtonsoft.Json;

namespace Sokolovsky;

public class IncomingMessageHandler {
    private string _fileName;
    private SqlConnection _connection;
    private const string ArchiveFolderName = "ArchiveMessages";

    public IncomingMessageHandler(string fileName, SqlConnection connection) {
        _fileName = fileName;
        _connection = connection;
    }

    public async Task<bool> Processing(FileUtilizationType fileUtilType = FileUtilizationType.Moving) {
        string json, xml;
        var status = 1;
        //2. Чтение json'a
        using (var r = new StreamReader(_fileName)) { json = await r.ReadToEndAsync(); }
            
        //3. Запись json'a в БД со статусом 1 в случае успеха, либо 3 - неуспеха
        if (string.IsNullOrEmpty(json)) return false;
        try {
            var insertCommand = new SqlCommand();
            insertCommand.CommandText = $"insert into invoices(date_time,invoice_json,status) values ({DateTime.Now},{json},{status})";
            insertCommand.Connection = _connection;
            insertCommand.ExecuteNonQuery();
        }
        catch (Exception e) {
            return false;
        }

        status = 2;
        
        //4. Получение IncomingMessage
        var message = JsonConvert.DeserializeObject<IncomingMessage>(json);
            
        if (message != null) {
            try {
                //5. Преобразование в xml
                await using var stringWriter = new StringWriter();
                var serializer = new XmlSerializer(message.GetType());
                serializer.Serialize(stringWriter, message);
                xml = stringWriter.ToString();
            
                //6. Отправка xml на внешний REST API
                HttpContent content = new StringContent(xml, Encoding.UTF8, @"text\xml");
                using var response = await new HttpClient().PostAsync("https://somesite/api/v1/invoice", content);
                var responseText = await response.Content.ReadAsStringAsync();
            }
            catch (Exception e) {
                status = 3;
            }
        }
        
        //7. Обновление статуса отправки в БД
        var updateCommand = new SqlCommand();
        updateCommand.CommandText = $"update invoices set status={status} where id=(select max(id) from invoices)";
        updateCommand.Connection = _connection;
        updateCommand.ExecuteNonQuery();


        //8. Удаление/перемещение файлов в архивную папку
        switch (fileUtilType) {
            case FileUtilizationType.Removal:
                File.Delete(_fileName);
                break;
            case FileUtilizationType.Moving:
                var dir = Directory.GetCurrentDirectory();
                File.Move(Path.Combine(dir, _fileName), 
                    Path.Combine(dir, ArchiveFolderName, _fileName));
                break;
        }

        return true;
    }
}

public enum FileUtilizationType {
    Removal,
    Moving
}