using Npgsql;
using System;
using System.Collections.Generic;
using System.IO;
using System.Net;
using System.Text;

namespace Spaceship
{
    public class Globalchat
    {
        private readonly NpgsqlDataSource _db;
        private readonly Dictionary<string, string> _playerColors;

        public Globalchat(NpgsqlDataSource db)
        {
            _db = db;
            _playerColors = new Dictionary<string, string>();
        }

        public void Chat(HttpListenerRequest req, HttpListenerResponse res)
        {
            string message;
            using (var reader = new StreamReader(req.InputStream, req.ContentEncoding))
            {
                message = reader.ReadToEnd().ToLower();
            }

            string[] requestData = message.Split(',');
            if (requestData.Length != 2)
            {
                res.StatusCode = (int)HttpStatusCode.BadRequest;
                res.Close();
                return;
            }

            string playerName = requestData[0];
            string chatMessage = requestData[1];


            var cmd = _db.CreateCommand("SELECT name FROM users WHERE name = @playerName");
            cmd.Parameters.AddWithValue("@playerName", playerName);
            object? playerExists = cmd.ExecuteScalar();

            if (playerExists != null)
            {
                cmd =_db.CreateCommand("SELECT player_name FROM player_colors  WHERE player_name = @playerName");
                cmd.Parameters.AddWithValue("@playerName", playerName);
                object? playerNameObjects= cmd.ExecuteScalar();

                cmd = _db.CreateCommand("INSERT INTO game_chat (player_name, message) VALUES (@playerName, @message)");
                cmd.Parameters.AddWithValue("playerName", playerName);
                cmd.Parameters.AddWithValue("message", chatMessage);
                cmd.ExecuteNonQuery();

                cmd = _db.CreateCommand("SELECT player_name, message FROM game_chat ORDER BY id DESC LIMIT 10");
                using (var reader = cmd.ExecuteReader())
                {
                    var responseStream = res.OutputStream;
                    var writer = new StreamWriter(responseStream);

                    while (reader.Read())
                    {
                        string messagePlayerName = reader.GetString(0);
                        string messageChat = reader.GetString(1);
                        string colorCode = GetPlayerColor(messagePlayerName, playerExists.ToString());
                        var line = $"\u001b[{colorCode}m{messagePlayerName}: {messageChat}\u001b[0m"; // ANSI escape codes for colors
                        writer.WriteLine(line);
                    }
                    reader.Close();
                    writer.Close();
                }
            }
            else
            {
                res.ContentType = "text/plain";
                res.StatusCode = (int)HttpStatusCode.BadRequest;
                byte[] errorMessage = Encoding.UTF8.GetBytes("\nPlayer name does not exist!\n");
                res.ContentLength64 = errorMessage.Length;
                res.OutputStream.Write(errorMessage, 0, errorMessage.Length);
                res.Close();
                return;
            }

        }

        private string GetPlayerColor(string playerName, string playerExists)
        {
            if (!_playerColors.ContainsKey(playerName))
            {
                string colorCode = GetColorFromDatabase(playerName);

                if (colorCode == null)
                {
                    colorCode = GenerateRandomColor();
                    StoreColorInDatabase(playerName, colorCode, playerExists);
                }

                _playerColors[playerName] = colorCode;
            }

            return _playerColors[playerName];
        }

        private string GetColorFromDatabase(string playerName)
        {
            using (var cmd = _db.CreateCommand())
            {
                cmd.CommandText = "SELECT color_code FROM player_colors WHERE player_name = @playerName";
                cmd.Parameters.AddWithValue("playerName", playerName);
                object? result = cmd.ExecuteScalar();
                return result != null ? result.ToString() : null;
            }
        }

        private void StoreColorInDatabase(string playerName, string colorCode, string playerExists)
        {
            var cmd = _db.CreateCommand();
            {
                cmd = _db.CreateCommand("SELECT player_name FROM player_colors WHERE player_name = @playerName");
                cmd.Parameters.AddWithValue("@playerName", playerName);
                object? playerNameObject = cmd.ExecuteScalar();

                if (playerNameObject != playerExists)
                {
                    cmd.CommandText = "INSERT INTO player_colors (player_name, color_code) VALUES (@playerName, @colorCode)";
                    cmd.Parameters.AddWithValue("playerName", playerName);
                    cmd.Parameters.AddWithValue("colorCode", colorCode);
                    cmd.ExecuteNonQuery();
                }
            }
        }

        private string GenerateRandomColor()
        {
            Random rand = new Random();
            int color = rand.Next(31, 38);
            return color.ToString();
        }
    }
}
