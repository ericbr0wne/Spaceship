using Npgsql;
using Spaceship;
using System;
using System.ComponentModel.DataAnnotations.Schema;
using System.Net;
using System.Text;


//database goes here 
string dbUri = "Host=localhost;Port=5455;Username=postgres;Password=postgres;Database=spaceship";
await using var db = NpgsqlDataSource.Create(dbUri);


//cancel server
bool listen = true;
Console.CancelKeyPress += delegate (object? sender, ConsoleCancelEventArgs e)
{
    Console.WriteLine("Server gracefully shutdown..");
    e.Cancel = true;
    listen = false;
};


//create listener
int port = 3000;
HttpListener listener = new();
listener.Prefixes.Add($"http://localhost:{port}/"); // <host> kan t.ex. vara 127.0.0.1, 0.0.0.0, ...

//start listener
try
{
    listener.Start();
    listener.BeginGetContext(new AsyncCallback(HandleRequest), listener);  //Här körs resten av kod i Async /wrapper 
    Console.WriteLine("Server listening on port: " + port);
    while (listen) { };

}
//stop listener
finally
{
    listener.Stop();
}


//take request and goes to Router
void HandleRequest(IAsyncResult result)
{
    if (result.AsyncState is HttpListener listener)
    {
        HttpListenerContext context = listener.EndGetContext(result);
        Router(context);
        HttpListenerRequest request = context.Request;

        HttpListenerResponse response = context.Response;

        Console.WriteLine($"{request.HttpMethod} request received");
        listener.BeginGetContext(new AsyncCallback(HandleRequest), listener);

    }
}


void Router(HttpListenerContext context)
{
    // string message = ""; //fill with responde messages to Client

    HttpListenerRequest request = context.Request;
    HttpListenerResponse response = context.Response;
    switch (request.HttpMethod, request.Url?.AbsolutePath) // == endpoint
    {
        case ("GET", "/"):
            RootGet(response);
            break;
        case ("POST", "/post/user"):
            // first check then post what u wanted
            RootPost(request, response);
            break;
        default:
            NotFound(response);
            break;
    }
    // Send the response back to the client
    //byte[] buffer = Encoding.UTF8.GetBytes(message);

    //response.OutputStream.Write(buffer, 0, buffer.Length);
    //response.OutputStream.Close();
}

void RootGet(HttpListenerResponse response)
{
    string message = ""; //fill with responde messages to Client
    //string responseString = "";
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
    //await Console.Out.WriteLineAsync(responseString);
    byte[] buffer = Encoding.UTF8.GetBytes(message);
    response.OutputStream.Write(buffer, 0, buffer.Length);
    response.OutputStream.Close();
}

void RootPost(HttpListenerRequest req, HttpListenerResponse res)
{
    StreamReader reader = new(req.InputStream, req.ContentEncoding);
    var cmd = db.CreateCommand("insert into users (name) values ($1) RETURNING id");
    //curl -d "user=eric" -X POST http://localhost:3000/post/user
    string postBody = reader.ReadToEnd();
    Console.WriteLine(postBody);

    string[] split = postBody.Split("=");

    string column = split[1];

    if (split[0] == "name")
    {
        cmd.Parameters.AddWithValue(column);
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
