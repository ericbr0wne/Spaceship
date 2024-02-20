﻿using Microsoft.VisualBasic;
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
        // curl -s -d "PLAYERNAME" -X POST http://localhost:3000/createplayer

        StreamReader reader = new(req.InputStream, req.ContentEncoding);
        string playerName = reader.ReadToEnd().ToLower();
        try
        {
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
        }
        catch (Exception)
        {
            string message = $"Could not create player! Try again.";
            res.ContentType = "text/plain";
            byte[] buffer = Encoding.UTF8.GetBytes(message);
            res.OutputStream.Write(buffer, 0, buffer.Length);
            res.OutputStream.Close();
            res.StatusCode = (int)HttpStatusCode.Created;
        }

        res.Close();
    }

public void getHp(HttpListenerResponse response)
    {
        response.ContentType= "text/plain";
        var getHp = _db.CreateCommand($"select user_name, hp from user_hitpoints;");

        using (var reader = getHp.ExecuteReader())
        {
            var header = "\x1b[34mUsers hitpoints:\x1b[0m";
             var responsestream = response.OutputStream;
            var writer = new StreamWriter(responsestream);
            writer.WriteLine(header);

            while (reader.Read())
            {
                var name = reader.GetString(0);
                var line = $"name: {name}";
                var hp = reader.GetInt32(1);
                var line2=  $"hp: {hp}";
               
                writer.WriteLine(line + " " + line2);
            }
            reader.Close();
            writer.Close();

        }
            
    }




  
public void GetUsers(HttpListenerResponse res)
    {
        res.ContentType = "text/plain";
        var getUsers = _db.CreateCommand($"SELECT name FROM users; ");

        using (var reader = getUsers.ExecuteReader())
        {
            var header = "\x1b[34mUsers:\x1b[0m";
            var responseStream = res.OutputStream;
            var writer = new StreamWriter(responseStream);
            writer.WriteLine(header);

            while (reader.Read())
            {
                var name = reader.GetString(0);
                var line = $"Name: {name}";

                writer.WriteLine("\x1b[94m" + line + "\x1b[0m");
            }

            reader.Close();
            writer.Close();
        }
    }
}