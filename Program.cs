using System;
using System.Diagnostics;
using System.Net.Http;
using System.Text;
using System.Text.Json;
using System.Threading.Tasks;
using System.Linq;
using System.Threading;
using System.Net;

class Program
{
    static readonly string API_URL = "https://controll.akaikumogo.uz/add-log";
    static readonly string DEVICE_NAME = Environment.MachineName;
    static readonly HttpClient Http = new HttpClient { Timeout = TimeSpan.FromSeconds(10) };

    static async Task Main()
    {
#if NET48
        ServicePointManager.SecurityProtocol |= SecurityProtocolType.Tls12;
#endif
        var oldProcs = Process.GetProcesses().Select(p => p.ProcessName.ToLower()).ToHashSet();

        while (true)
        {
            await Task.Delay(3000);

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
        var data = new { device = DEVICE_NAME, application = app, action = action, time = DateTime.UtcNow.ToString("o") };
        var json = new StringContent(JsonSerializer.Serialize(data), Encoding.UTF8, "application/json");

        try
        {
            using var resp = await Http.PostAsync(API_URL, json);
        }
        catch
        {
            // Exceptionni e'tiborsiz qoldiramiz
        }
    }
}