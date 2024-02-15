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
    public GamePlay(NpgsqlDataSource db)
    {
        _db = db;
    }


    public void NewGame(HttpListenerRequest req, HttpListenerResponse res)
    {
        // curl -d "new,player1,C,7" -X POST http://localhost:3000/newgame - Start new game

        res.ContentType = "text/plain";
        StreamReader reader = new(req.InputStream, req.ContentEncoding);
        string postBody = reader.ReadToEnd().ToLower();

        string[] split = postBody.Split(",");
        string gameid = split[0];
        string playerName = split[1];
        string posLr = split[2];
        string posNr = split[3];


        var getUserIdCommand = _db.CreateCommand($"SELECT id FROM users WHERE name = '{playerName}';");
        int userId = Convert.ToInt32(getUserIdCommand.ExecuteScalar()); ;

        var getPositionCommand = _db.CreateCommand($"SELECT id FROM position WHERE vertical = '{posLr}' AND horizontal = {posNr};");
        object? posIdObject = getPositionCommand.ExecuteScalar();

        if (posIdObject != null && int.TryParse(posIdObject.ToString(), out int positionId))
        {
            if (gameid == "new")
            {
                var gameInsertCommand = _db.CreateCommand("INSERT INTO game (user1_id) VALUES ($1) RETURNING id;");
                gameInsertCommand.Parameters.AddWithValue(userId);

                int newGameId = Convert.ToInt32(gameInsertCommand.ExecuteScalar());

                var posInsertCommand = _db.CreateCommand("INSERT INTO users_x_position (game_id, user_id, position_id) VALUES ($1, $2, $3);");
                posInsertCommand.Parameters.AddWithValue(newGameId);
                posInsertCommand.Parameters.AddWithValue(userId);
                posInsertCommand.Parameters.AddWithValue(positionId);
                posInsertCommand.ExecuteNonQuery();

                var insertHitpointsCmd = _db.CreateCommand("INSERT INTO user_hitpoints (game_id, user_id) VALUES ($1, $2);");
                insertHitpointsCmd.Parameters.AddWithValue(newGameId);
                insertHitpointsCmd.Parameters.AddWithValue(userId);
                insertHitpointsCmd.ExecuteNonQuery();

                string message = $"{playerName} created a new game {newGameId} and placed position {positionId}";

                byte[] buffer = Encoding.UTF8.GetBytes(message);
                res.OutputStream.Write(buffer, 0, buffer.Length);
                res.StatusCode = (int)HttpStatusCode.Created;
                res.OutputStream.Close();
            }
            else
            {
                Console.WriteLine("Type new as gameId");
            }
        }
        
    }
    
    public void JoinGame(HttpListenerRequest req, HttpListenerResponse res)
    {
        // curl -d "gameid,player2,C,2" -X POST http://localhost:3000/joingame - Join existing game
        
        
        res.ContentType = "text/plain";
        StreamReader reader = new(req.InputStream, req.ContentEncoding);
        string postBody = reader.ReadToEnd().ToLower();

        string[] split = postBody.Split(",");
        int gameid = int.Parse(split[0]);
        string playerName = split[1];
        string posLr = split[2];
        string posNr = split[3];


        var getUserIdCommand = _db.CreateCommand($"SELECT id FROM users WHERE name = '{playerName}';");
        int userId = Convert.ToInt32(getUserIdCommand.ExecuteScalar());

        var getPositionCommand = _db.CreateCommand($"SELECT id FROM position WHERE vertical = '{posLr}' AND horizontal = {posNr};");
        object? posIdObject = getPositionCommand.ExecuteScalar();

        if (posIdObject != null && int.TryParse(posIdObject.ToString(), out int positionId))
        {
            if (gameid > 0)
            {
                var gameInsertCommand = _db.CreateCommand("INSERT INTO game (user2_id) VALUES ($1) RETURNING id;");
                gameInsertCommand.Parameters.AddWithValue(userId);
                int newGameId = Convert.ToInt32(gameInsertCommand.ExecuteScalar());

                var posInsertCommand = _db.CreateCommand("INSERT INTO users_x_position (game_id, user_id, position_id) VALUES ($1, $2, $3);");
                posInsertCommand.Parameters.AddWithValue(newGameId);
                posInsertCommand.Parameters.AddWithValue(userId);
                posInsertCommand.Parameters.AddWithValue(positionId);
                posInsertCommand.ExecuteNonQuery();

                var insertHitpointsCmd = _db.CreateCommand("INSERT INTO user_hitpoints (game_id, user_id) VALUES ($1, $2);");
                insertHitpointsCmd.Parameters.AddWithValue(newGameId);
                insertHitpointsCmd.Parameters.AddWithValue(userId);
                insertHitpointsCmd.ExecuteNonQuery();

                string message = $"{playerName} joined game {newGameId} and placed position {positionId}";

                byte[] buffer = Encoding.UTF8.GetBytes(message);
                res.OutputStream.Write(buffer, 0, buffer.Length);
                res.StatusCode = (int)HttpStatusCode.Created;
                res.OutputStream.Close();
            }
            else
            {
                string message =$"GameId is does not exist";
                byte[] buffer = Encoding.UTF8.GetBytes(message);
                res.OutputStream.Write(buffer, 0, buffer.Length);
                res.StatusCode = (int)HttpStatusCode.Created;
                res.OutputStream.Close();
            }
        }
        
    }


    /*

    public void CreateGameJoinGameSelectPosition(HttpListenerRequest req, HttpListenerResponse res)
    {
        // curl -d "eric,benny" -X POST http://localhost:3000/joingame
        res.ContentType = "text/plain";
        StreamReader reader = new(req.InputStream, req.ContentEncoding);
        string postBody = reader.ReadToEnd().ToLower();

        string[] split = postBody.Split(",");
        string player1 = split[0];
        string player2 = split[1];


        var getUserIdCommand = _db.CreateCommand($"SELECT id FROM users WHERE name = '{player1}';");
        int userId = Convert.ToInt32(getUserIdCommand.ExecuteScalar());

        var getUserId2Command = _db.CreateCommand($"SELECT id FROM users WHERE name = '{player2}';");
        int userId2 = Convert.ToInt32(getUserId2Command.ExecuteScalar());

        var cmd = _db.CreateCommand("INSERT INTO game (user1_id, user2_id) VALUES ($1, $2) RETURNING id");

        cmd.Parameters.AddWithValue(userId);
        cmd.Parameters.AddWithValue(userId2);
        int gameId = Convert.ToInt32(cmd.ExecuteScalar());

        var insertHitpointsCmd = _db.CreateCommand("INSERT INTO user_hitpoints (game_id, user_id) VALUES (@gameId, @userId), (@gameId, @userId2)");
        insertHitpointsCmd.Parameters.AddWithValue("gameId", gameId);
        insertHitpointsCmd.Parameters.AddWithValue("userId", userId);
        insertHitpointsCmd.Parameters.AddWithValue("userId2", userId2);
        insertHitpointsCmd.ExecuteNonQuery();

        string message = $"{player1} and {player2} has joined the game!";
        byte[] buffer = Encoding.UTF8.GetBytes(message);
        res.OutputStream.Write(buffer, 0, buffer.Length);
        res.StatusCode = (int)HttpStatusCode.Created;
        res.OutputStream.Close();
    }

*/

}
