using Npgsql;
using Spaceship;
using System.Net;
using System.Reflection.Metadata;
using System.Text;


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
/*
void HandleRequest(IAsyncResult result)
{
    HttpListener listener = (HttpListener)result.AsyncState; // Cast the AsyncState back to HttpListener
    if (listener != null && listener.IsListening)
    {
        try
        {
            HttpListenerContext context = listener.EndGetContext(result);
            Router(context);
        }
        catch (Exception ex)
        {
            Console.WriteLine("Error handling request: " + ex.Message);
        }
        finally
        {
            if (listener.IsListening)
            {
                listener.BeginGetContext(new AsyncCallback(HandleRequest), listener); // Continue listening for next request
                Console.WriteLine("Server listening on port: " + port);
            }
        }
    }
}
*/

int gameId;
int userId;
int positionId;

void Router(HttpListenerContext context)
{
    User user = new(db);
    Attack attack = new(db);
    HttpListenerRequest request = context.Request;
    HttpListenerResponse response = context.Response;
    Console.WriteLine($"{request.HttpMethod} request received");
    switch (request.HttpMethod, request.Url?.AbsolutePath) // == endpoint
    {
        case ("GET", "/get/users"):
            RootGet(response);
            break;
        case ("POST", "/post/user"):
            RootPost(request, response);
            break;
        case ("POST", "/post/position"):
            user.PositionPost(request, response);
            break;
        case ("POST", "/post/gameplayers"):
            PostGamePlayers(request, response);
            break;
        case ("POST", "post/attack"):
            attack.Check(request, response);
            break;

        default:
            NotFound(response);
            break;
    }
}


void RootGet(HttpListenerResponse response)
{
    // curl -X GET http://localhost:3000/get/users
    string message = "";
    const string getUsers = "select * from users;";
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
    string name = split[1]; 
    if (split[0] == "user")
    {
        cmd.Parameters.AddWithValue(postBody);
    }
    cmd.ExecuteNonQuery();
    Console.WriteLine($"Created the following in db: {postBody}");
    res.StatusCode = (int)HttpStatusCode.Created;
    res.Close();
}



void PostGamePlayers(HttpListenerRequest req, HttpListenerResponse res)
{
    // curl -d "user1_id=123&user2_id=456" -X POST http://localhost:3000/post/gameplayers

    StreamReader reader = new(req.InputStream, req.ContentEncoding);
    var cmd = db.CreateCommand("INSERT INTO game (user1_id, user2_id) VALUES (@user1_id, @user2_id)");
    string postBody = reader.ReadToEnd();
    Console.WriteLine(postBody);
    string[] split = postBody.Split("&");
    string[] user1 = split[0].Split("=");
    string[] user2 = split[1].Split("=");
    if (user1[0] == "user1_id" && user2[0] == "user2_id")
    {
        cmd.Parameters.AddWithValue("@user1_id", int.Parse(user1[1]));
        cmd.Parameters.AddWithValue("@user2_id", int.Parse(user2[1]));
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
/*

void RootPost(HttpListenerRequest req, HttpListenerResponse res)
{
    // curl -d "user=eric&password=your_password" -X POST http://localhost:3000/post/user
    StreamReader reader = new(req.InputStream, req.ContentEncoding);
    var cmd = db.CreateCommand("INSERT INTO users (name, password_hash) VALUES (@name, @password) RETURNING id");

    string postBody = reader.ReadToEnd();
    Console.WriteLine(postBody);
    string[] split = postBody.Split("&");
    string[] user = split[0].Split("=");
    string[] password = split[1].Split("=");

    if (user[0] == "user" && password[0] == "password")
    {
        // Hash the password
        string hashedPassword = HashPassword(password[1]);

        cmd.Parameters.AddWithValue("@name", user[1]);
        cmd.Parameters.AddWithValue("@password", hashedPassword);
    }

    int userId = (int)cmd.ExecuteScalar();
    Console.WriteLine($"Created user with ID: {userId}");
    res.StatusCode = (int)HttpStatusCode.Created;
    res.Close();
}

string HashPassword(string password)
{
    using (SHA256 sha256Hash = SHA256.Create())
    {
        // ComputeHash - returns byte array
        byte[] bytes = sha256Hash.ComputeHash(Encoding.UTF8.GetBytes(password));

        // Convert byte array to a string
        StringBuilder builder = new StringBuilder();
        for (int i = 0; i < bytes.Length; i++)
        {
            builder.Append(bytes[i].ToString("x2"));
        }
        return builder.ToString();
    }
}

void PostGamePlayers(HttpListenerRequest req, HttpListenerResponse res)
{
    // curl -d "user1_id=123&user2_id=456" -X POST http://localhost:3000/post/gameplayers

    StreamReader reader = new(req.InputStream, req.ContentEncoding);
    var cmd = db.CreateCommand("INSERT INTO game (user1_id, user2_id) VALUES (@user1_id, @user2_id) RETURNING id");
    string postBody = reader.ReadToEnd();
    Console.WriteLine(postBody);
    string[] split = postBody.Split("&");
    string[] user1 = split[0].Split("=");
    string[] user2 = split[1].Split("=");
    if (user1[0] == "user1_id" && user2[0] == "user2_id")
    {
        cmd.Parameters.AddWithValue("@user1_id", int.Parse(user1[1]));
        cmd.Parameters.AddWithValue("@user2_id", int.Parse(user2[1]));
    }
    int gameId = (int)cmd.ExecuteScalar(); // Get the ID of the newly inserted game
    Console.WriteLine($"Created the following in db: {postBody}");

    // Trigger the posting of positions for the players in the game
    //PositionPost(req, res, int.Parse(user1[1]), gameId);
    //PositionPost(req, res, int.Parse(user2[1]), gameId);

    res.StatusCode = (int)HttpStatusCode.Created;
    res.Close();
}

void PositionPost(HttpListenerRequest req, HttpListenerResponse res)
{
    //curl -d "C,7,Benny" -X POST http://localhost:3000/post/position
    StreamReader reader = new(req.InputStream, req.ContentEncoding);
    string postBody = reader.ReadToEnd();

    string[] split = postBody.Split(",");
    var posLetter = split[0];
    var posNumber = split[1];
    var posName = split[2];


    var getUserIdCommand = db.CreateCommand($"SELECT id FROM users WHERE name = '{posName}';");
    int userId2 = Convert.ToInt32(getUserIdCommand.ExecuteScalar());
    Console.WriteLine($"User ID = {userId}");

    var getMapIdCommand = db.CreateCommand($"SELECT id FROM position WHERE vertical = '{posLetter}' AND horizontal = {posNumber};");
    object? mapIdObject = getMapIdCommand.ExecuteScalar();

    if (mapIdObject != null && int.TryParse(mapIdObject.ToString(), out int mapId))
    {
        Console.WriteLine($"Map ID = {mapId}");

        var checkIfUserExist = db.CreateCommand($"SELECT userid FROM users_x_position;");
        int ifUserExist = Convert.ToInt32(checkIfUserExist.ExecuteScalar());
        if (ifUserExist == userId)
        {
            Console.WriteLine("Sorry user already have a position");
        }
        else if (ifUserExist != mapId)
        {

            var cmd = db.CreateCommand("INSERT INTO users_x_position (userid, mapid) VALUES (@userId, @mapId)");
            cmd.Parameters.AddWithValue("@userId", userId);
            cmd.Parameters.AddWithValue("@mapId", mapId);
            Console.WriteLine($"Position set!");

            cmd.ExecuteNonQuery();
        }
    }
    else
    {
        Console.WriteLine("Failed to retrieve or parse Map ID.");
    }
    res.StatusCode = (int)HttpStatusCode.Created;
    res.Close();
}
*/
