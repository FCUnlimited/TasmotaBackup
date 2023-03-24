#load "tasmota.csx"

#nullable enable


using System.Net.Http;
using System.Net;
using System.Net.NetworkInformation;
using System.Text.Json.Nodes;

static readonly HttpClient client = new HttpClient();

public async Task<List<Tasmota>> getTasmotas(string start = "192.168.178.1", uint end = 255)
{
    var ips = await scanIPs(IPAddress.Parse(start), end);
    var tasmos = await checkIsTasmota(ips);
    await GetTasmotaStatus(tasmos);
    return tasmos;
}

public async Task downloadConfig(string dir, Tasmota tasmo)
{
    using var s = await client.GetStreamAsync($"http://{tasmo.Address}/dl");
    var filename = $"{tasmo.GetFilePrefix()}_{DateTime.Now.ToString("""yyMMdd""")}.bin";
    var path = Path.Combine(dir, filename);
    using var fs = new FileStream(path, FileMode.OpenOrCreate);
    await s.CopyToAsync(fs);
}

private async Task GetTasmotaStatus(List<Tasmota> tasmos)
{
    var replyTasks = tasmos.Select(tas =>
    {

        var responseTask = client.GetAsync($"http://{tas.Address}/cm?cmnd=Status0");
        return new { responseTask, tasmota = tas };
    }).ToList();

    try
    {
        await Task.WhenAll(replyTasks.Select(x => x.responseTask));
    }
    catch (System.Exception e)
    {
        Console.WriteLine(e.Message);
    }

    foreach (var rt in replyTasks)
    {
        try
        {
            var response = rt.responseTask.Result;
            if (response.IsSuccessStatusCode)
            {
                var s = await response.Content.ReadAsStringAsync();
                var jobject = JsonNode.Parse(s)!.AsObject();
                if (jobject != null)
                {
                    rt.tasmota.Version = (string?)jobject["StatusFWR"]?["Version"] ?? string.Empty;
                    rt.tasmota.Hardware = (string?)jobject["StatusFWR"]?["Hardware"] ?? string.Empty;
                    rt.tasmota.Hostname = (string?)jobject["StatusNET"]?["Hostname"] ?? string.Empty;
                    rt.tasmota.MacAddress = (string?)jobject["StatusNET"]?["Mac"] ?? string.Empty;
                    rt.tasmota.Rssi = (int?)jobject["StatusSTS"]?["Wifi"]?["RSSI"] ?? 999;
                }
            }
        }
        catch (System.Exception e)
        {
            Console.WriteLine(e.Message);
        }
    }
}

private async Task<List<Tasmota>> checkIsTasmota(List<IPAddress> ips)
{
    // static readonly HttpClient client = new HttpClient();
    client.Timeout = TimeSpan.FromSeconds(5);
    var replyTasks = ips.Select(ip =>
    {

        var responseTask = client.GetAsync($"http://{ip}/cm?cmnd=DeviceName");
        return new { responseTask, addr = ip };
    }).ToList();

    try
    {
        await Task.WhenAll(replyTasks.Select(x => x.responseTask));
    }
    catch (System.Exception e)
    {
        Console.WriteLine(e.Message);
    }

    var tasmos = new List<Tasmota>();
    foreach (var rt in replyTasks)
    {
        try
        {
            var response = rt.responseTask.Result;
            if (response.IsSuccessStatusCode)
            {
                var s = await response.Content.ReadAsStringAsync();
                tasmos.Add(new Tasmota(rt.addr, s));
                Console.WriteLine(rt.addr + ": " + s);
            }
        }
        catch (System.Exception)
        {

        }
    }

    return tasmos;
}

private async Task<List<IPAddress>> scanIPs(IPAddress start, uint end = 255)
{
    var bytes = start.GetAddressBytes();
    var leastSigByte = start.GetAddressBytes().Last();
    var range = (int)end - leastSigByte;

    var pingReplyTasks = Enumerable.Range(leastSigByte, range)
        .Select(x =>
        {
            using var p = new Ping();

            var bb = start.GetAddressBytes();
            bb[3] = (byte)x;
            var destIp = new IPAddress(bb);
            var pingResultTask = p.SendPingAsync(destIp);
            return new { pingResultTask, addr = destIp };

        })
        .ToList();

    await Task.WhenAll(pingReplyTasks.Select(x => x.pingResultTask));
    var pingable = new List<IPAddress>();
    foreach (var pr in pingReplyTasks)
    {
        var tsk = pr.pingResultTask;
        var pingResult = tsk.Result; //we know these are completed tasks
        var ip = pr.addr;


        if (pingResult.Status == IPStatus.TimedOut)
        {
            using var p = new Ping();
            pingResult = await p.SendPingAsync(ip, 2000);

        }
        Console.WriteLine("{0} : {1}", ip, pingResult.Status);
        if (pingResult.Status == IPStatus.Success)
        {
            pingable.Add(ip);
        }
    }

    return pingable;
}

private static bool PingHost(string nameOrAddress)
{
    bool pingable = false;
    Ping? pinger = null;

    try
    {
        pinger = new Ping();
        PingReply reply = pinger.Send(nameOrAddress);
        pingable = reply.Status == IPStatus.Success;
    }
    catch (PingException)
    {
        // Discard PingExceptions and return false;
    }
    finally
    {
        if (pinger != null)
        {
            pinger.Dispose();
        }
    }

    return pingable;
}

