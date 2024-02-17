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
        // curl -s -d "PLAYERNAME" -X POST http://localhost:3000/newplayer

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

}