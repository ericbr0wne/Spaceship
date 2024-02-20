using Npgsql;
using Spaceship;
using System.Net;
using System.Reflection.Metadata;
using System.Text;


string dbUri = "Host=localhost;Port=5455;Username=postgres;Password=postgres;Database=spaceship";
await using var _db = NpgsqlDataSource.Create(dbUri);
bool listen = true;

Console.CancelKeyPress += delegate (object? sender, ConsoleCancelEventArgs e)
{
    Console.WriteLine("Server gracefully shutdown..");
    e.Cancel = true;
    listen = false;
};
int port = 3000;
HttpListener listener = new();
listener.Prefixes.Add($"http://localhost:{port}/"); 

try
{
    listener.Start();
    listener.BeginGetContext(new AsyncCallback(HandleRequest), listener); 
    Console.WriteLine("Server listening on port: " + port);
    while (listen)
    {
    }
}
finally
{
    listener.Stop();
}
void HandleRequest(IAsyncResult result)
{
    if (result.AsyncState is HttpListener listener)
    {
        HttpListenerContext context = listener.EndGetContext(result);
        Router(context);
        listener.BeginGetContext(new AsyncCallback(HandleRequest), listener);
    }
}


void Router(HttpListenerContext context)
{

    User user = new(_db);
    Attack attack = new(_db);
    GamePlay gameplay = new(_db);
    Router router = new();
    Story story = new Story();
    HelpMenu menu = new HelpMenu();

    HttpListenerRequest request = context.Request;
    HttpListenerResponse response = context.Response;
    Console.WriteLine($"{request.HttpMethod} request received");
    switch (request.HttpMethod, request.Url?.AbsolutePath) // == endpoint
    {
        case ("GET", "/users"):
            user.GetUsers(response);
            break;
        case ("GET", "/help"):
            menu.Commands(response);
            break;
        case ("GET", "/start"):
            story.Intro(response);
            break;
        case ("GET", "/mission"):
            story.Mission(response);
            break;
        case ("POST", $"/createplayer"):
            user.CreatePlayer(request, response);
            break;
        case ("POST", "/newgame"):
            gameplay.NewGame(request, response);
            break;
        case ("POST", "/joingame"):
            gameplay.JoinGame(request, response);
            break;
        case ("POST", "/attack"):
            attack.AttackPlayer(request, response);
            break;
        default:
            router.NotFound(response);
            break;
    }
}




