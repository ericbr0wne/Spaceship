using Npgsql;
using Spaceship;
using System;
using System.ComponentModel.DataAnnotations.Schema;
using System.Data;
using System.Net;
using System.Net.WebSockets;
using System.Text;
using static Npgsql.Replication.PgOutput.Messages.RelationMessage;
using static System.Runtime.InteropServices.JavaScript.JSType;

string dbUri = "Host=localhost;Port=5455;Username=postgres;Password=postgres;Database=spaceship";
await using var db = NpgsqlDataSource.Create(dbUri);
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
    listener.BeginGetContext(new AsyncCallback(HandleRequest), listener);  //Här körs resten av kod i Async /wrapper 
    Console.WriteLine("Server listening on port: " + port);
    while (listen) { };
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
    User user = new(db);
    HttpListenerRequest request = context.Request;
    HttpListenerResponse response = context.Response;
    Console.WriteLine($"{request.HttpMethod} request received");
    switch (request.HttpMethod, request.Url?.AbsolutePath) // == endpoint
    {
        case ("GET", "/users"):
            RootGet(response);
            break;
        case ("POST", "/post/user"):
            RootPost(request, response);
            break;
        case ("POST", "/post/position"):
            user.PositionPost(request, response);
            break;
        default:
            NotFound(response);
            break;
    }
}

void RootGet(HttpListenerResponse response)
{

    const string getUsers = "select * from users";
    var cmd = db.CreateCommand(getUsers);
    var reader = cmd.ExecuteReader();
    response.ContentType = "text/plain";
    response.StatusCode = (int)HttpStatusCode.OK;
    while (reader.Read())
    {
        message += reader.GetInt32(0) + ", "; // user id
        message += reader.GetString(1) + ", "; // name
        message += reader.GetInt32(2) + ", "; // hp
    }
    byte[] buffer = Encoding.UTF8.GetBytes(message);
    response.OutputStream.Write(buffer, 0, buffer.Length);
    response.OutputStream.Close();
}

void RootPost(HttpListenerRequest req, HttpListenerResponse res)
{
    // curl -d "user=eric" -X POST http://localhost:3000/post/user
    StreamReader reader = new(req.InputStream, req.ContentEncoding);
    var cmd = db.CreateCommand("insert into users (name) values ($1) RETURNING id");

    string postBody = reader.ReadToEnd();
    Console.WriteLine(postBody);
    string[] split = postBody.Split("=");
    string column = split[1];
    if (split[0] == "name")
    {
        cmd.Parameters.AddWithValue(postBody);
    }
    cmd.ExecuteNonQuery();
    Console.WriteLine($"Created the following in db: {postBody}");
    res.StatusCode = (int)HttpStatusCode.Created;
    res.Close();
}



void NotFound(HttpListenerResponse res)
{
    res.StatusCode = (int)HttpStatusCode.NotFound;
    res.Close();
}
