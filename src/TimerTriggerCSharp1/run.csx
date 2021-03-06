#r "Newtonsoft.Json"
#r "System.Web"

using System;
using System.Configuration;
using System.Net;
using Newtonsoft.Json;
using System.Web;
using System.IO;

private static string GetScriptPath()
    => Path.Combine(GetEnvironmentVariable("HOME"), @"site\wwwroot");

private static string GetEnvironmentVariable(string name)
    => System.Environment.GetEnvironmentVariable(name, EnvironmentVariableTarget.Process);

private static object _obj = new object(); 

private static string readContentFile()
{

    var staticFilesPath = 
        Path.GetFullPath(Path.Combine(GetScriptPath(), ""));
    var fullPath = Path.GetFullPath(Path.Combine(staticFilesPath, "arquivo.txt"));

    
    if (File.Exists(fullPath))
    {
        string r = "";
        lock(_obj){
            r = File.ReadAllText(fullPath);

            writeFile("");
        }
        return r;        
    }
    return null;
}

private static void writeFile(string text)
{
    var staticFilesPath = 
        Path.GetFullPath(Path.Combine(GetScriptPath(), ""));
    var fullPath = Path.GetFullPath(Path.Combine(staticFilesPath, "arquivo.txt"));
    lock(_obj){
        if (File.Exists(fullPath))
        {
            Console.WriteLine(text);
            TextWriter tw = new StreamWriter(fullPath);
            tw.Write(text);
            tw.Close();
        } else
        {
            File.Create(fullPath).Dispose();
            TextWriter tw = new StreamWriter(fullPath);
            tw.Write(text);
            tw.Close();
        }
    }
}

public static void Run(TimerInfo myTimer, TraceWriter log)
{
    log.Info($"C# Timer trigger function executed at: {DateTime.Now}");
    ObterClimaCuritiba(log).Wait();
}

static async Task ObterClimaCuritiba(TraceWriter log)
{
    var query = "select * from weather.forecast where woeid in (select woeid from geo.places(1) where text = '{0}')";
                    
    var q = string.Format(query, "Curitiba");
    var url = string.Format("q={0}&format=json", HttpUtility.UrlEncode(q));

    log.Info($"Log: {url}");
    YahooResponse yahooResponse = null;
    using (var client = new HttpClient())
    {
        client.BaseAddress = new Uri("https://query.yahooapis.com");

        var request = await client.GetAsync(string.Format("v1/public/yql?{0}", url));
        var response = await request.Content.ReadAsStringAsync();
        yahooResponse = JsonConvert.DeserializeObject<YahooResponse>(response);

    }
        var temp = FtoC(float.Parse(yahooResponse.query.results.channel.item.condition.temp));
        var condicao = CodeBR.GetWeatherBR()[int.Parse(yahooResponse.query.results.channel.item.condition.code)];
        var responseOKa = new Fulfillment();
        var b = new Message[]{
                        new Message {
                            type = 0,
                            speech = $"Temperatura: {temp} graus\r\nClima: {condicao}"
                        }
                    };

        int code = int.Parse(yahooResponse.query.results.channel.item.condition.code);

        switch (code)
        {
        case 0: case 1: case 2: case 3: case 4: case 5:case 6: case 10: case 12: case 13: case 17: case 21: case 22: case 23: 
        case 27: case 29: case 35: case 37: case 45: case 46: case 47:
            log.Info("CLIMA ATUAL: " + condicao);
            writeFile($"true;externo");
            break;
        default:
            log.Info("Está OK");
            break;
        }

        DateTime now = TimeZoneInfo.ConvertTime(DateTime.Now, TimeZoneInfo.FindSystemTimeZoneById("E. South America Standard Time"));
        DateTime sunrise = DateTime.Parse(yahooResponse.query.results.channel.astronomy.sunrise);
        DateTime sunset = DateTime.Parse(yahooResponse.query.results.channel.astronomy.sunset);

        if(now > sunset && now < sunset.AddMinutes(5)){
            log.Info("Está de noite |" + " Hora por do sol: " + sunset + " Hora atual: "+ now);
            writeFile($"true;externo");
        }
        if(now > sunrise && now < sunrise.AddMinutes(5)){
            log.Info("Está amanhecendo |" + " Hora do nascer do sol: " + sunrise + " Hora atual: "+ now);
            writeFile($"false;externo");
        }
        responseOKa.messages = b;
        responseOKa.source = "apiazurefunctionsimionatohome";
        responseOKa.speech = $"Temperatura: {temp} graus\r\nClima: {condicao}";
        responseOKa.displayText = $"Temperatura: {temp} graus\r\nClima: {condicao}";
}


private static int FtoC(float f)
{
    return (int)((f - 32) / 1.8000);
}

public class CodeBR
{
    public static Dictionary<int, string> GetWeatherBR()
    {
        var str = jsonBR();
        var j = JsonConvert.DeserializeObject<Dictionary<int, string>>(str);
        return j;
    }
    private static string jsonBR()
    {
        return "{\r\n\t\"0\": \"tornado\",\r\n\t\"1\": \"tempestade tropical\",\r\n\t\"2\": \"furac\u00E3o\",\r\n\t\"3\": \"tempestade severa\",\r\n\t\"4\": \"trovoadas\",\r\n\t\"5\": \"chuva e neve\",\r\n\t\"6\": \"chuva e granizo fino\",\r\n\t\"7\": \"neve e granizo fino\",\r\n\t\"8\": \"garoa g\u00E9lida\",\r\n\t\"9\": \"garoa\",\r\n\t\"10\": \"chuva g\u00E9lida\",\r\n\t\"11\": \"chuvisco\",\r\n\t\"12\": \"chuva\",\r\n\t\"13\": \"neve em flocos finos\",\r\n\t\"14\": \"leve precipita\u00E7\u00E3o de neve\",\r\n\t\"15\": \"ventos com neve\",\r\n\t\"16\": \"neve\",\r\n\t\"17\": \"chuva de granizo\",\r\n\t\"18\": \"pouco granizo\",\r\n\t\"19\": \"p\u00F3 em suspens\u00E3o\",\r\n\t\"20\": \"neblina\",\r\n\t\"21\": \"n\u00E9voa seca\",\r\n\t\"22\": \"enfuma\u00E7ado\",\r\n\t\"23\": \"vendaval\",\r\n\t\"24\": \"ventando\",\r\n\t\"25\": \"frio\",\r\n\t\"26\": \"nublado\",\r\n\t\"27\": \"muitas nuvens (noite)\",\r\n\t\"28\": \"muitas nuvens (dia)\",\r\n\t\"29\": \"parcialmente nublado (noite)\",\r\n\t\"30\": \"parcialmente nublado (dia)\",\r\n\t\"31\": \"c\u00E9u limpo (noite)\",\r\n\t\"32\": \"ensolarado\",\r\n\t\"33\": \"tempo bom (noite)\",\r\n\t\"34\": \"tempo bom (dia)\",\r\n\t\"35\": \"chuva e granizo\",\r\n\t\"36\": \"quente\",\r\n\t\"37\": \"tempestades isoladas\",\r\n\t\"38\": \"tempestades esparsas\",\r\n\t\"39\": \"tempestades esparsas\",\r\n\t\"40\": \"chuvas esparsas\",\r\n\t\"41\": \"nevasca\",\r\n\t\"42\": \"tempestades de neve esparsas\",\r\n\t\"43\": \"nevasca\",\r\n\t\"44\": \"parcialmente nublado\",\r\n\t\"45\": \"chuva com trovoadas\",\r\n\t\"46\": \"tempestade de neve\",\r\n\t\"47\": \"rel\u00E2mpagos e chuvas isoladas\",\r\n\t\"3200\": \"n\u00E3o dispon\u00EDvel\"\r\n}";
    }
}


public class MyObjct
{
    public string id { get; set; }
    public DateTime timestamp { get; set; }
    public string lang { get; set; }
    public Result result { get; set; }
    public Status status { get; set; }
    public string sessionId { get; set; }
}

public class Result
{
    public string source { get; set; }
    public string resolvedQuery { get; set; }
    public string action { get; set; }
    public bool actionIncomplete { get; set; }
    public Parameters parameters { get; set; }
    public object[] contexts { get; set; }
    public Metadata metadata { get; set; }
    public Fulfillment fulfillment { get; set; }
    public decimal score { get; set; }
}

public class Parameters
{
    public string date { get; set; }
    [JsonProperty(PropertyName = "geo-city")]
    public string geocity { get; set; }
    public string Local { get; set; }
    public string Acao { get; set; }
}

public class Metadata
{
    public string intentId { get; set; }
    public string webhookUsed { get; set; }
    public string webhookForSlotFillingUsed { get; set; }
    public int webhookResponseTime { get; set; }
    public string intentName { get; set; }
}

public class Fulfillment
{
    public string speech { get; set; }
    public string source { get; set; }
    public string displayText { get; set; }
    public Message[] messages { get; set; }
}

public class Message
{
    public int type { get; set; }
    public string speech { get; set; }
}

public class Status
{
    public int code { get; set; }
    public string errorType { get; set; }
}


public class YahooResponse
{
    public Query query { get; set; }
}

public class Query
{
    public int count { get; set; }
    public DateTime created { get; set; }
    public string lang { get; set; }
    public Results results { get; set; }
}

public class Results
{
    public Channel channel { get; set; }
}

public class Channel
{
    public Units units { get; set; }
    public string title { get; set; }
    public string link { get; set; }
    public string description { get; set; }
    public string language { get; set; }
    public string lastBuildDate { get; set; }
    public string ttl { get; set; }
    public Location location { get; set; }
    public Wind wind { get; set; }
    public Atmosphere atmosphere { get; set; }
    public Astronomy astronomy { get; set; }
    public Image image { get; set; }
    public Item item { get; set; }
}

public class Units
{
    public string distance { get; set; }
    public string pressure { get; set; }
    public string speed { get; set; }
    public string temperature { get; set; }
}

public class Location
{
    public string city { get; set; }
    public string country { get; set; }
    public string region { get; set; }
}

public class Wind
{
    public string chill { get; set; }
    public string direction { get; set; }
    public string speed { get; set; }
}

public class Atmosphere
{
    public string humidity { get; set; }
    public string pressure { get; set; }
    public string rising { get; set; }
    public string visibility { get; set; }
}

public class Astronomy
{
    public string sunrise { get; set; }
    public string sunset { get; set; }
}

public class Image
{
    public string title { get; set; }
    public string width { get; set; }
    public string height { get; set; }
    public string link { get; set; }
    public string url { get; set; }
}

public class Item
{
    public string title { get; set; }
    public string lat { get; set; }
    public string _long { get; set; }
    public string link { get; set; }
    public string pubDate { get; set; }
    public Condition condition { get; set; }
    public Forecast[] forecast { get; set; }
    public string description { get; set; }
    public Guid guid { get; set; }
}

public class Condition
{
    public string code { get; set; }
    public string date { get; set; }
    public string temp { get; set; }
    public string text { get; set; }
}

public class Guid
{
    public string isPermaLink { get; set; }
}

public class Forecast
{
    public string code { get; set; }
    public string date { get; set; }
    public string day { get; set; }
    public string high { get; set; }
    public string low { get; set; }
    public string text { get; set; }
}
