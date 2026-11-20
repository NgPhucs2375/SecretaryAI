using System.Net.Http.Headers;
using System.Text;
using System.Text.Json;

var configPath = Path.Combine("..","..","src","Qeq.Api","appsettings.Development.json");
using var config = JsonDocument.Parse(File.ReadAllText(configPath));
var apiKey = config.RootElement.GetProperty("DeepSeek").GetProperty("ApiKey").GetString()!;

Console.WriteLine($"Length of DeepSeek API Key: {apiKey.Length}");


var tools = new[]
{
    new
    {
        type = "function",
        function = new
        {
            name = "add_transaction",
            description = "Ghhi 1 khhoan chi tieu cua nguoi dung vao he thong",
            parameters = new
            {
                type = "object",
                properties = new
                {
                    amount = new{type = "number",description = "So tien VND"},
                    category = new{type = "string", @enum = new[]
                    {
                        "food",
                        "transportation",
                        "education", 
                        "housing", 
                        "entertainment", 
                        "health", 
                        "other"
                    }
                },
                note = new{type = "string",description = "Ghi chu ngan"},
                occurredAt = new{type = "string", description = "THhoi diem ci tieu, ISO 8601"}
                
                },
                required = new[]
                {
                    "amount",
                    "category",
                    "occurredAt"
                }
            }
        }
    }
};

using var http = new HttpClient();
http.DefaultRequestHeaders.Authorization = new AuthenticationHeaderValue("Bearer",apiKey);

foreach (var prompt in new[]
{
    "trưa nay ăn phở 45k",
    "mua xăng 50k",
    "ghi giúp tôi ý tưởng làm app đọc sách"
})
{
    var body = new
    {
        model = "deepseek-chat",
        messages = new object[]
        {
            new { role = "system", content = $"Bạn là thư ký cá nhân. Khi người dùng kể một khoản chi tiêu, hãy gọi tool add_transaction để ghi lại ngay. Thiếu thời điểm thì dùng thời điểm hiện tại, định dạng ISO 8601. Chỉ hỏi lại khi thiếu số tiền hoặc không rõ nhóm chi. Bây giờ là {DateTimeOffset.Now:yyyy-MM-ddTHH:mm:sszzz}. Chỉ chọn category đúng một trong các giá trị enum đã cho." },            
            new {role = "user",content = prompt}
        },
        tools,
        tool_chhoice = "auto"
    };

    var json = JsonSerializer.Serialize(body);
    var content = new StringContent(json, Encoding.UTF8, "application/json");
    var response = await http.PostAsync("https://api.deepseek.com/v1/chat/completions",content);
    var responseText = await response.Content.ReadAsByteArrayAsync();

    Console.WriteLine($"=={prompt}[{(int)response.StatusCode}]===");
    Console.WriteLine(JsonSerializer.Serialize(JsonDocument.Parse(responseText),
        new JsonSerializerOptions{WriteIndented = true}));
}