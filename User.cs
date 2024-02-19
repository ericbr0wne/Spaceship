using Npgsql;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Net;
using System.Text;
using System.Threading.Tasks;
using BCrypt.Net;

namespace Spaceship;

public class User
{
    private NpgsqlDataSource _db;

    public User(NpgsqlDataSource db)
    {
        _db = db;
    }

    PasswordHasher _PasswordHasher = new PasswordHasher();

    public void NewPlayer(HttpListenerRequest req, HttpListenerResponse res)
    {
        // curl -s -d "name=PLAYERNAME&password=PSW123" -X POST http://localhost:3000/newplayer

        StreamReader reader = new(req.InputStream, req.ContentEncoding);
        string postBody = reader.ReadToEnd().ToLower();

        string[] bodyParts = postBody.Split("&");
        string playerName = string.Empty;
        string password = string.Empty;

        foreach (var part in bodyParts)
        {
            string[] userParts = part.Split("=");
            string column = userParts[0];
            string value = userParts[1];

            if (column == "name")
            {
                playerName = value;
            }
            else if (column == "password")
            {
                password = value;
            }
        }

        var nameCheck = _db.CreateCommand("Select id From users WHERE name = ($1)");
        nameCheck.Parameters.AddWithValue(playerName);
        int playerId = Convert.ToInt32(nameCheck.ExecuteScalar());

        if (playerId > 0)
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
            string hashedPassword = _PasswordHasher.HashPassword(password);

            var cmdInsertPlayer = _db.CreateCommand("insert into users (name, password_hash) values ($1, $2)");

            cmdInsertPlayer.Parameters.AddWithValue(playerName);
            cmdInsertPlayer.Parameters.AddWithValue(hashedPassword);
            cmdInsertPlayer.ExecuteNonQuery();

            string message = $"Created the following in db: Player name: {playerName} & Hashed password: {hashedPassword}";

            res.ContentType = "text/plain";
            byte[] buffer = Encoding.UTF8.GetBytes(message);
            res.OutputStream.Write(buffer, 0, buffer.Length);
            res.OutputStream.Close();
            res.StatusCode = (int)HttpStatusCode.Created;
        }
        res.Close();
    }
}

