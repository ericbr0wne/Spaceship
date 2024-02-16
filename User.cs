using Npgsql;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Net;
using System.Text;
using System.Threading.Tasks;

namespace Spaceship;

public class User
{
    private NpgsqlDataSource _db;

    public User(NpgsqlDataSource db)
    {
        _db = db;
    }


    public void CreatePlayer(HttpListenerRequest req, HttpListenerResponse res)
    {
        // curl -s -d "eric" -X POST http://localhost:3000/newplayer

        StreamReader reader = new(req.InputStream, req.ContentEncoding);
        string playerName = reader.ReadToEnd().ToLower();

        var nameCheck = _db.CreateCommand("Select id From users WHERE name = ($1)");
        nameCheck.Parameters.AddWithValue(playerName);
        int playerId = Convert.ToInt32(nameCheck.ExecuteScalar());
        if (playerId > 0 )
        {
            string message = $"Player {playerName} alredy exists";
            res.StatusCode = (int)HttpStatusCode.Conflict;
            
            res.ContentType = "text/plain";
            byte[] buffer = Encoding.UTF8.GetBytes(message);
            res.OutputStream.Write(buffer, 0, buffer.Length);
            res.OutputStream.Close();
        }
        else
        {
            var cmd = _db.CreateCommand("insert into users (name) values ($1)");

            cmd.Parameters.AddWithValue(playerName);
            cmd.ExecuteNonQuery();

            string message = $"Created the following in db: {playerName}";
            
            
            res.ContentType = "text/plain";
            byte[] buffer = Encoding.UTF8.GetBytes(message);
            res.OutputStream.Write(buffer, 0, buffer.Length);
            res.OutputStream.Close();
            res.StatusCode = (int)HttpStatusCode.Created;
        }

        res.Close();
    }

    public void Position(HttpListenerRequest req, HttpListenerResponse res)

    {
        //curl -s -d "C,7,Benny" -X POST http://localhost:3000/position
        StreamReader reader = new(req.InputStream, req.ContentEncoding);
        string postBody = reader.ReadToEnd();

        string[] split = postBody.Split(",");
        var posLetter = split[0];
        var posNumber = split[1]; 
        var posName = split[2];


        var getUserIdCommand = _db.CreateCommand($"SELECT id FROM users WHERE name = '{posName}';");
        int userId = Convert.ToInt32(getUserIdCommand.ExecuteScalar());
        Console.WriteLine($"User ID = {userId}");

        var getMapIdCommand = _db.CreateCommand($"SELECT id FROM position WHERE vertical = '{posLetter}' AND horizontal = {posNumber};");
        object? mapIdObject = getMapIdCommand.ExecuteScalar();

        if (mapIdObject != null && int.TryParse(mapIdObject.ToString(), out int mapId))
        {
            Console.WriteLine($"Map ID = {mapId}");

            var checkIfUserExist = _db.CreateCommand($"SELECT userid FROM users_x_position;");
            int ifUserExist = Convert.ToInt32(checkIfUserExist.ExecuteScalar());
            if (ifUserExist == userId)
            {
                Console.WriteLine("Sorry user already have a position");
            }
            else if (ifUserExist != mapId)
            {
                var cmd = _db.CreateCommand("INSERT INTO users_x_position (userid, mapid) VALUES ($1, $2)");
                cmd.Parameters.AddWithValue(userId);
                cmd.Parameters.AddWithValue(mapId);
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

     //Denna behöver fixas eller slängas
    public void Get(HttpListenerResponse response)
    {

        // curl -X GET http://localhost:3000/get/users
        string message = "";
        const string getUsers = "select * from users;";
        var cmd = _db.CreateCommand(getUsers);
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
    
}