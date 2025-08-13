using System;
using System.Diagnostics;
using System.Net.Http;
using System.Text;
using System.Text.Json;
using System.Threading.Tasks;
using System.Linq;
using System.Threading;

class Program
{
    static readonly string API_URL = "https://controll.akaikumogo.uz/add-log";
    static readonly string DEVICE_NAME = Environment.MachineName;

    static async Task Main()
    {
        var oldProcs = Process.GetProcesses().Select(p => p.ProcessName.ToLower()).ToHashSet();

        while (true)
        {
            Thread.Sleep(3000); // 3 soniya intervallar bilan tekshiradi

            var newProcs = Process.GetProcesses().Select(p => p.ProcessName.ToLower()).ToHashSet();

            // Yangi ochilganlar
            foreach (var p in newProcs.Except(oldProcs))
            {
                await SendLog(p, "open");
            }

            // Yopilganlar
            foreach (var p in oldProcs.Except(newProcs))
            {
                await SendLog(p, "close");
            }

            oldProcs = newProcs;
        }
    }

    static async Task SendLog(string app, string action)
    {
        using var client = new HttpClient();
        var data = new { device = DEVICE_NAME, application = app, action = action, time = DateTime.UtcNow.ToString("o") };
        var json = new StringContent(JsonSerializer.Serialize(data), Encoding.UTF8, "application/json");

        try
        {
            await client.PostAsync(API_URL, json);
        }
        catch
        {
            // Exceptionni e'tiborsiz qoldiramiz
        }
    }
}
