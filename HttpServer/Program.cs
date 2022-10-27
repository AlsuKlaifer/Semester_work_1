using Server;

bool appIsRunning = true;

var settings = new ServerSettings();
settings.Serialize();
var settingsDes = ServerSettings.Deserialize();

var server = new HttpServer();

using (server)
{
    server.Start();

    while (appIsRunning)
    {
        Handler(Console.ReadLine()?.ToLower(), server);
    }
}

static void Handler(string command, HttpServer server)
{
    switch (command)
    {
        case "start":
            server.Start();
            break;

        case "restart":
            server.Stop();
            server.Start();
            break;

        case "stop":
            server.Stop();
            break;

        case "status":
            Console.WriteLine(server.Status);
            break;
    }
}

Console.Read();