namespace Bellight.MongoDb;

public class MongoDbSettings
{
    public string? ConnectionString { get; set; }
    public string UseSsl { get; set; } = "false";

    public string? DatabaseName { get; set; }
    public string? LogQuery { get; set; }
    public string? DirectConnection { get; set; } = "false";
}