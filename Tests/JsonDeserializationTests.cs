using Newtonsoft.Json;
using Sokolovsky;

namespace Tests;

public class JsonDeserializationTests {
    [SetUp]
    public void Setup() {
        _fileName = @"d:\user\Documents\Sokolovsky\IncomingMessage.json";
    }

    private string _fileName;
    private string json;
    
    [Test]
    public void Test1() {
        IncomingMessage x;
        using (StreamReader r = new StreamReader(_fileName)) {
            json = r.ReadToEnd();
            x = JsonConvert.DeserializeObject<IncomingMessage>(json);
        }
        
        Assert.Pass();
    }
}