namespace Qeq.Domain.Settings;

public class DeepSeekSettings
{
    public string ApiKey { get; set; } = "";
    public string Endpoint { get; set; } = "https://api.deepseek.com/v1";
    public string Model { get; set; } = "deepseek-chat";
}