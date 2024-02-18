using Npgsql;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Net;
using System.Text;
using System.Threading.Tasks;

namespace Spaceship;

public class GamePlay
{
    private readonly NpgsqlDataSource _db;
    private UpdateMap _updateMap;
    public GamePlay(NpgsqlDataSource db)
    {
        _db = db;
        _updateMap = new UpdateMap(db);
    }


    public void NewGame(HttpListenerRequest req, HttpListenerResponse res)
    {

        // curl -s -d "new,PLAYERNAME,A,1" -X POST http://localhost:3000/newgame

        res.ContentType = "text/plain";
        StreamReader reader = new(req.InputStream, req.ContentEncoding);
        string postBody = reader.ReadToEnd().ToLower();
        try
        {

            string[] split = postBody.Split(",");
            string gameid = split[0];
            string playerName = split[1];
            string posLr = split[2];
            string posNr = split[3];


            var getplayerNameCommand = _db.CreateCommand($"SELECT name FROM users WHERE name = '{playerName}';");
            object? p1 = getplayerNameCommand.ExecuteScalar();

            if (p1 != null)
            {
                var getPositionCommand = _db.CreateCommand($"SELECT id FROM position WHERE vertical = '{posLr}' AND horizontal = {posNr};");
                object? posIdObject = getPositionCommand.ExecuteScalar();

                if (posIdObject != null && int.TryParse(posIdObject.ToString(), out int positionId))
                {
                    if (gameid == "new")
                    {
                        var gameInsertCommand = _db.CreateCommand("INSERT INTO game (p1_name) VALUES ($1) RETURNING id;");
                        gameInsertCommand.Parameters.AddWithValue(p1);

                        int newGameId = Convert.ToInt32(gameInsertCommand.ExecuteScalar());

                        var posInsertCommand = _db.CreateCommand("INSERT INTO users_x_position (game_id, user_name, position_id) VALUES ($1, $2, $3);");
                        posInsertCommand.Parameters.AddWithValue(newGameId);
                        posInsertCommand.Parameters.AddWithValue(p1);
                        posInsertCommand.Parameters.AddWithValue(positionId);
                        posInsertCommand.ExecuteNonQuery();

                        var insertHitpointsCmd = _db.CreateCommand("INSERT INTO user_hitpoints (game_id, user_name) VALUES ($1, $2);");
                        insertHitpointsCmd.Parameters.AddWithValue(newGameId);
                        insertHitpointsCmd.Parameters.AddWithValue(p1);
                        insertHitpointsCmd.ExecuteNonQuery();
                        
                        string map = _updateMap.GetMap(newGameId, playerName);
                        
                        string message = $"{p1} created a new game. REMEMBER THIS GAME-ID: {newGameId}\nHere is the current map: \n{map} ";
                        byte[] buffer = Encoding.UTF8.GetBytes(message);
                        res.OutputStream.Write(buffer, 0, buffer.Length);
                        res.StatusCode = (int)HttpStatusCode.Created;
                        res.OutputStream.Close();
                    }
                    else
                    {
                        string message = "Type new as gameId";
                        byte[] buffer = Encoding.UTF8.GetBytes(message);
                        res.OutputStream.Write(buffer, 0, buffer.Length);
                        res.OutputStream.Close();
                        res.StatusCode = (int)HttpStatusCode.Created;
                    }
                }
                else
                {
                    string message = "This map location does not exist. Try A-C and 1-3!";
                    byte[] buffer = Encoding.UTF8.GetBytes(message);
                    res.OutputStream.Write(buffer, 0, buffer.Length);
                    res.OutputStream.Close();
                    res.StatusCode = (int)HttpStatusCode.Created;
                }

            }
            else
            {
                string message = "A player with this name does not exist!";
                byte[] buffer = Encoding.UTF8.GetBytes(message);
                res.OutputStream.Write(buffer, 0, buffer.Length);
                res.OutputStream.Close();
                res.StatusCode = (int)HttpStatusCode.Created;
            }

        }
        catch (Exception)
        {
            string message = @"wrong game input! Use this format: ""new,PLAYERNAME,A,1""";
            byte[] buffer = Encoding.UTF8.GetBytes(message);
            res.OutputStream.Write(buffer, 0, buffer.Length);
            res.StatusCode = (int)HttpStatusCode.Created;
            res.OutputStream.Close();
        }
        res.Close();

    }

    public void JoinGame(HttpListenerRequest req, HttpListenerResponse res)
    {
        // curl -s -d "gameid,PLAYERNAME,C,2" -X POST http://localhost:3000/joingame

        res.ContentType = "text/plain";
        StreamReader reader = new(req.InputStream, req.ContentEncoding);
        string postBody = reader.ReadToEnd().ToLower();
        try
        {

            string[] split = postBody.Split(",");
            string inputgameid = split[0];
            string playerName = split[1];
            string posLr = split[2];
            string posNr = split[3];

            var gameOpenCommand = _db.CreateCommand($"SELECT p2_name FROM game WHERE id = '{inputgameid}';");
            object? openslot = gameOpenCommand.ExecuteScalar();
            if (openslot == DBNull.Value)
            {
                var getPlayerNameCommand = _db.CreateCommand($"SELECT name FROM users WHERE name = '{playerName}';");
                object? p2 = getPlayerNameCommand.ExecuteScalar();
                if (p2 != null)
                {
                    var getPositionCommand = _db.CreateCommand($"SELECT id FROM position WHERE vertical = '{posLr}' AND horizontal = {posNr};");
                    object? posIdObject = getPositionCommand.ExecuteScalar();

                    if (posIdObject != null && int.TryParse(posIdObject.ToString(), out int positionId))
                    {
                        if (int.TryParse(inputgameid.ToString(), out int gameid) && gameid > 0)
                        {
                            var gameInsertCommand = _db.CreateCommand("UPDATE game SET p2_name = $1 WHERE id = $2");
                            gameInsertCommand.Parameters.AddWithValue(p2);
                            gameInsertCommand.Parameters.AddWithValue(gameid);
                            gameInsertCommand.ExecuteNonQuery();

                            var posInsertCommand = _db.CreateCommand("INSERT INTO users_x_position (game_id, user_name, position_id) VALUES ($1, $2, $3);");
                            posInsertCommand.Parameters.AddWithValue(gameid);
                            posInsertCommand.Parameters.AddWithValue(p2);
                            posInsertCommand.Parameters.AddWithValue(positionId);
                            posInsertCommand.ExecuteNonQuery();

                            var insertHitpointsCmd = _db.CreateCommand("INSERT INTO user_hitpoints (game_id, user_name) VALUES ($1, $2);");
                            insertHitpointsCmd.Parameters.AddWithValue(gameid);
                            insertHitpointsCmd.Parameters.AddWithValue(p2);
                            insertHitpointsCmd.ExecuteNonQuery();
                            
                            string map = _updateMap.GetMap(gameid,playerName);

                            string message = $"{p2} Joined the game.\nHere is the current map:\n{map}";
                            byte[] buffer = Encoding.UTF8.GetBytes(message);
                            res.OutputStream.Write(buffer, 0, buffer.Length);
                            res.StatusCode = (int)HttpStatusCode.Created;
                            res.OutputStream.Close();
                        }

                        else
                        {
                            string message = $"GameId is does not exist";
                            byte[] buffer = Encoding.UTF8.GetBytes(message);
                            res.OutputStream.Write(buffer, 0, buffer.Length);
                            res.StatusCode = (int)HttpStatusCode.Created;
                            res.OutputStream.Close();
                        }
                    }
                    else
                    {
                        string message = "This map location does not exist. Try A-C and 1-3!";
                        byte[] buffer = Encoding.UTF8.GetBytes(message);
                        res.OutputStream.Write(buffer, 0, buffer.Length);
                        res.OutputStream.Close();
                        res.StatusCode = (int)HttpStatusCode.Created;
                    }
                }
                else
                {
                    string message = "A player with this name does not exist!";
                    byte[] buffer = Encoding.UTF8.GetBytes(message);
                    res.OutputStream.Write(buffer, 0, buffer.Length);
                    res.StatusCode = (int)HttpStatusCode.Created;
                    res.OutputStream.Close();
                }
            }
            else
            {
                string message = "Game full!";
                byte[] buffer = Encoding.UTF8.GetBytes(message);
                res.OutputStream.Write(buffer, 0, buffer.Length);
                res.StatusCode = (int)HttpStatusCode.Created;
                res.OutputStream.Close();
            }
        }
        catch (Exception)
        {
            string message = @"wrong game input! Use this format: ""gameid,PLAYERNAME,C,2""";
            byte[] buffer = Encoding.UTF8.GetBytes(message);
            res.OutputStream.Write(buffer, 0, buffer.Length);
            res.StatusCode = (int)HttpStatusCode.Created;
            res.OutputStream.Close();
        }
        res.Close();
    }
}