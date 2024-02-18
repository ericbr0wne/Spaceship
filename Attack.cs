using Npgsql;
using System.Net;
using System.Text;
namespace Spaceship;

public class Attack
{
    private NpgsqlDataSource _db;
    bool gameover = false;

    public Attack(NpgsqlDataSource db)
    {
        _db = db;
    }

    public void AttackPlayer(HttpListenerRequest req, HttpListenerResponse res)
    {
        //curl -s -d "game_id,attacker,E,5,attacked" -X POST http://localhost:3000/attack

        StreamReader reader = new(req.InputStream, req.ContentEncoding);
        string postBody = reader.ReadToEnd().ToLower();
        try
        {
            string[] split = postBody.Split(",");
            if (split.Length > 5)
            {
                throw new ArgumentException("Wrong amount of arguments in request. Expected format: game_id,attacker,E,5,attacked");
            }
            int gameId = int.Parse(split[0]);       //gameid for the attack method
            string attacker = split[1];     //attacker - player who is attacking  1
            var posLetter = split[2];         //letter position 
            int posNumber = int.Parse(split[3]);    //nr position
            string defender = split[4];     //attacked - player who is getting attacked 2
            
            var gameEndedCommand = _db.CreateCommand("SELECT game_ended FROM game WHERE id = @gameId");
            gameEndedCommand.Parameters.AddWithValue("gameId", gameId);
            bool gameEnded = Convert.ToBoolean(gameEndedCommand.ExecuteScalar());
            
            if (gameEnded)
            {
                string message = "This game has ended. No further actions are allowed.";
                res.ContentType = "text/plain";
                byte[] buffer = Encoding.UTF8.GetBytes(message);
                res.OutputStream.Write(buffer, 0, buffer.Length);
                res.OutputStream.Close();
                res.StatusCode = (int)HttpStatusCode.BadRequest;
                return;
            }
            
            var attackerCommand = _db.CreateCommand($"SELECT hp FROM user_hitpoints WHERE user_name = $1;"); //KOLLA HP PÅ DEFENDER I USERS_HITPOINTS
            attackerCommand.Parameters.AddWithValue(attacker);

            int attackerHp = Convert.ToInt32(attackerCommand.ExecuteScalar());
            attackerCommand.ExecuteNonQuery();

            if (attackerHp > 0)
            {
                var attackId = _db.CreateCommand($"SELECT id FROM position WHERE vertical = '{posLetter}' AND horizontal = {posNumber};"); //KOLLA POSITION ID
                object? attack = attackId.ExecuteScalar();
                if (attack != null && int.TryParse(attack.ToString(), out int AttackPosition))
                {
                    Console.WriteLine($"{attacker} is attacking on square: {posLetter} {posNumber} !");
                    var defenderPositionCommand = _db.CreateCommand($"SELECT position_id FROM users_x_position WHERE game_id = {gameId} AND user_name = '{defender}';");
                    int defencePosition = Convert.ToInt32(defenderPositionCommand.ExecuteScalar());

                    if (defencePosition == AttackPosition)
                    {
                        var hitRemoveHpCommand = _db.CreateCommand($"UPDATE user_hitpoints SET hp = hp - 1 WHERE user_name = '{defender}' AND game_id = {gameId};");
                        hitRemoveHpCommand.ExecuteNonQuery();
                        var newDefenderHp = _db.CreateCommand($"SELECT hp FROM user_hitpoints WHERE user_name = '{defender}' AND game_id = {gameId};");
                        object? defenderHpObject = newDefenderHp.ExecuteScalar();
                        if (defenderHpObject != null && int.TryParse(defenderHpObject.ToString(), out int defenderHp))
                        {
                            if (defenderHp == 0)
                            {
                                string message = $"KABOOOOM! You destroyed the enemy \nPlayer {attacker} got one win";
                                res.ContentType = "text/plain";
                                byte[] buffer = Encoding.UTF8.GetBytes(message);
                                res.OutputStream.Write(buffer, 0, buffer.Length);
                                res.OutputStream.Close();
                                res.StatusCode = (int)HttpStatusCode.Created;
                                
                                UpdateWins(attacker);
                                EndGame(gameId);
                            }
                            else
                            {
                                string message = "You hit the enemy and damaged the spaceship with 1 dmg.";
                                res.ContentType = "text/plain";
                                byte[] buffer = Encoding.UTF8.GetBytes(message);
                                res.OutputStream.Write(buffer, 0, buffer.Length);
                                res.OutputStream.Close();
                                res.StatusCode = (int)HttpStatusCode.Created;
                            }
                        }
                        else
                        {
                            string message = "Could not parse defender HP.";
                            res.ContentType = "text/plain";
                            byte[] buffer = Encoding.UTF8.GetBytes(message);
                            res.OutputStream.Write(buffer, 0, buffer.Length);
                            res.OutputStream.Close();
                            res.StatusCode = (int)HttpStatusCode.Created;
                        }
                    }
                    else
                    {
                        string message = "You missed the target!";
                        res.ContentType = "text/plain";
                        byte[] buffer = Encoding.UTF8.GetBytes(message);
                        res.OutputStream.Write(buffer, 0, buffer.Length);
                        res.OutputStream.Close();
                        res.StatusCode = (int)HttpStatusCode.Created;
                    }
                }
                else
                {
                    string message = "Please choose a valid position to attack";
                    res.ContentType = "text/plain";
                    byte[] buffer = Encoding.UTF8.GetBytes(message);
                    res.OutputStream.Write(buffer, 0, buffer.Length);
                    res.OutputStream.Close();
                    res.StatusCode = (int)HttpStatusCode.Created;
                }
            }
            else
            {
                string message = "Game over! You got destroyed!";
                res.ContentType = "text/plain";
                byte[] buffer = Encoding.UTF8.GetBytes(message);
                res.OutputStream.Write(buffer, 0, buffer.Length);
                res.OutputStream.Close();
                res.StatusCode = (int)HttpStatusCode.Created;

                gameover = true;
            }
        }
        catch (Exception)
        {
            string message = "Error: Expected format: game_id,attacker,E,5,attacked";
            byte[] buffer = Encoding.UTF8.GetBytes(message);
            res.OutputStream.Write(buffer, 0, buffer.Length);
            res.StatusCode = (int)HttpStatusCode.Created;
            res.OutputStream.Close();
        }

        res.Close();
    }

    public void UpdateWins(string playerName)
    {
        using (var cmd = _db.CreateCommand("UPDATE users SET wins = wins + 1 WHERE name = @playerName"))
        {
            cmd.Parameters.AddWithValue("playerName", playerName);
            cmd.ExecuteNonQuery();
        }
    }
    
    public void EndGame(int gameId)
    {
        using (var cmd = _db.CreateCommand("UPDATE game SET game_ended = true WHERE id = @gameId"))
        {
            cmd.Parameters.AddWithValue("gameId", gameId);
            cmd.ExecuteNonQuery();
        }
    }
}