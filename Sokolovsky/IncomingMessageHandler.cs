﻿using System.Data.SqlClient;
using System.Text;
using System.Xml.Serialization;
using Newtonsoft.Json;

namespace Sokolovsky;

public class IncomingMessageHandler {
    private string _fileName;
    private SqlConnection _connection;

    public IncomingMessageHandler(string fileName, SqlConnection connection) {
        _fileName = fileName;
        _connection = connection;
    }

    public async void Processing(FileUtilizationType fileUtilType = FileUtilizationType.Moving) {
        string json, xml;
        var status = 3;
        //2. Чтение json'a
        using (var r = new StreamReader(_fileName)) { json = r.ReadToEnd(); }
            
        //3. Запись json'a в БД со статусом 1 в случае успеха, либо 3 - неуспеха
        if (!string.IsNullOrEmpty(json)) status = 1;
        SqlCommand insertCommand = new SqlCommand();
        insertCommand.CommandText = $"insert into invoices(date_time,invoice_json,status) values ({DateTime.Now},{json},{status})";
        insertCommand.Connection = _connection;

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
        updateCommand.CommandText = "update invoices set status=2 where id=(select max(id) from invoices)";
        updateCommand.Connection = _connection;

        //8. Удаление/перемещение файлов в архивную папку
        switch (fileUtilType) {
            case FileUtilizationType.Removal:
                File.Delete(_fileName);
                break;
            case FileUtilizationType.Moving:
                throw new NotImplementedException();
        }
    }
}

public enum FileUtilizationType {
    Removal,
    Moving
}