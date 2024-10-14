using System.Data.SqlClient;
using System.Text;
using System.Xml.Serialization;
using Newtonsoft.Json;
using Sokolovsky;

const string folderPath = @"d:\Sokolovsky\";
const string connectionString = "User ID=root;Password=myPassword;Host=localhost;Port=5432;Database=myDataBase;Pooling=true;Min Pool Size=0;Max Pool Size=100;Connection Lifetime=0;";
const int frequency = 1000;

while (true) {
    //1. Сканирование папки, получение списка  НОВЫХ файлов json (return List<string fileNames>)
    var fileNames = Directory.GetFiles(folderPath).Select(x => Path.GetFileName(x)).ToList();
    var status = 1;
    foreach (var fileName in fileNames) {
        string json, xml;
        //2. Чтение json'a
        using (var r = new StreamReader(fileName)) { json = r.ReadToEnd(); }
            
        //3. Запись json'a в БД со статусом 1 в случае успеха, либо 3 - неуспеха
        await using SqlConnection connection = new SqlConnection(connectionString);
        await connection.OpenAsync();
        if (!string.IsNullOrEmpty(json)) {
            SqlCommand insertCommand = new SqlCommand();
            insertCommand.CommandText = $"insert into invoices(date_time,invoice_json,status) values ({DateTime.Now},{json},{status})";
            insertCommand.Connection = connection;
        }

        //4. Получение IncomingMessage
        var message = JsonConvert.DeserializeObject<IncomingMessage>(json);
            
        //5. Преобразование в xml
        await using(var stringWriter = new StringWriter())
        { 
            var serializer = new XmlSerializer(message.GetType());
            serializer.Serialize(stringWriter, message);
            xml = stringWriter.ToString();
        }

        //6. Отправка xml на внешний REST API
        HttpContent content = new StringContent(xml, Encoding.UTF8, @"text\xml");
        using var response = await new HttpClient().PostAsync("https://somesite/api/v1/invoice", content);
        var responseText = await response.Content.ReadAsStringAsync();

        //7. Обновление статуса отправки в БД
        SqlCommand updateCommand = new SqlCommand();
        updateCommand.CommandText = $"update invoices set status=2 where id=(select max(id) from invoices)";
        updateCommand.Connection = connection;

        //8. Удаление/перемещение файлов в архивную папку
        File.Delete(fileName);

        connection.Close();
    }
    
    Thread.Sleep(frequency);
}

