using System.Text;
using Newtonsoft.Json.Linq;
using System.Net.Http.Headers;

public class Program
{
    static async Task Main()
    {
        // 1. Tìm đường dẫn file Calculator.cs
        string? calculatorPath = FindUpwardFile(AppContext.BaseDirectory, "Calculator.cs");
        if (calculatorPath == null) { Console.WriteLine("Calculator.cs not found"); return; }

        // 2. Đọc nội dung file Calculator.cs
        string methodCode = await File.ReadAllTextAsync(calculatorPath, Encoding.UTF8);

        // 3. Tạo câu lệnh (Prompt) yêu cầu AI viết Test
        var prompt = $"""
        Write a real xUnit test for the following C# method.
        Do not use Moq. Just call the method and assert the result.
        Code:
        {methodCode}
        """;

        // 4. Kết nối tới LM Studio (đang chạy ở localhost:1234)
        var client = new HttpClient { Timeout = TimeSpan.FromMinutes(6) };
        client.DefaultRequestHeaders.Authorization = new AuthenticationHeaderValue("Bearer", "lm-studio");

        var body = new
        {
            model = "gemma-4-queen-31b-it-i1", // Đã cập nhật đúng model của bạn
            messages = new[] { new { role = "user", content = prompt } },
            max_tokens = 400,
            stream = false,
            temperature = 0.2
        };

        var json = System.Text.Json.JsonSerializer.Serialize(body);
        var resp = await client.PostAsync("http://localhost:1234/v1/chat/completions",
                                          new StringContent(json, Encoding.UTF8, "application/json"));
        resp.EnsureSuccessStatusCode();

        // 5. Đọc kết quả AI trả về và trích xuất code
        var text = await resp.Content.ReadAsStringAsync();
        var raw = JObject.Parse(text)["choices"]![0]!["message"]!["content"]!.ToString();
        string unitTestCode = StripCodeFence(raw);

        // 6. Tự động lưu đoạn code Test vào project UnitTest
        var unitTestDir = Path.GetFullPath(Path.Combine(Path.GetDirectoryName(calculatorPath)!, "..", "UnitTest"));
        Directory.CreateDirectory(unitTestDir);
        string outFile = Path.Combine(unitTestDir, "UnitTest_Generated.cs");

        await File.WriteAllTextAsync(outFile, unitTestCode, Encoding.UTF8);
        Console.WriteLine($"Saved: {outFile}");
    }

    // Hàm phụ trợ: Đi ngược lên các thư mục cha để tìm file Calculator.cs
    static string? FindUpwardFile(string start, string name, int max = 8)
    {
        var d = new DirectoryInfo(start);
        for (int i = 0; i < max && d != null; i++, d = d.Parent)
        {
            string c = Path.Combine(d.FullName, name);
            if (File.Exists(c)) return c;
        }
        return null;
    }

    // Hàm phụ trợ: Xóa các dấu ```csharp thừa mà AI hay sinh ra ở đầu/cuối code
    static string StripCodeFence(string s)
    {
        if (string.IsNullOrWhiteSpace(s)) return s;
        int a = s.IndexOf("```");
        if (a >= 0)
        {
            int b = s.IndexOf("```", a + 3);
            if (b > a) s = s.Substring(a + 3, b - a - 3);
            s = s.Replace("csharp", "").Replace("cs", "");
        }
        return s.Trim();
    }
}