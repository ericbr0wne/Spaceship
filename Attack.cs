using Npgsql;
using System.ComponentModel.Design;
using System.Net;
using System.Text;
namespace Spaceship;

public class Attack(NpgsqlDataSource db)
{
    private readonly NpgsqlDataSource _db = db;
    private readonly UpdateMap _updateMap = new(db);

    public void AttackPlayer(HttpListenerRequest req, HttpListenerResponse res)
    {
        res.ContentType = "text/plain";
        StreamReader reader = new(req.InputStream, req.ContentEncoding);
        string postBody = reader.ReadToEnd().ToLower();
        string[] split = postBody.Split(",");
        int gameId = int.Parse(split[0]);
        string attacker = split[1];
        var posLetter = split[2];
        int posNumber = int.Parse(split[3]);
        string defender = split[4];

        if (attacker.ToString() != defender.ToString())
        {
            if (split.Length == 5)
            {
                var attackerCommand = _db.CreateCommand($"SELECT hp FROM user_hitpoints WHERE user_name = @Attacker AND game_id = @game_id;");
                attackerCommand.Parameters.AddWithValue("@Attacker", attacker);
                attackerCommand.Parameters.AddWithValue("@game_id", gameId);
                int attackerHp = Convert.ToInt32(attackerCommand.ExecuteScalar());

                if (attackerHp > 0)
                {
                    var findAttacker = _db.CreateCommand("SELECT id FROM users WHERE name = @Attacker");
                    findAttacker.Parameters.AddWithValue("@Attacker", attacker);
                    int atk = Convert.ToInt32(findAttacker.ExecuteScalar());

                    var findDefender = _db.CreateCommand($"SELECT id FROM users WHERE name = @defender;");
                    findDefender.Parameters.AddWithValue("@defender", defender);
                    int def = Convert.ToInt32(findDefender.ExecuteScalar());

                    if (def > 0 && atk > 0)
                    {
                        var defenderPositionCommand = _db.CreateCommand($"SELECT position_id FROM user_x_position WHERE game_id = @game_id AND user_name = @defender;");
                        defenderPositionCommand.Parameters.AddWithValue("@game_id", gameId);
                        defenderPositionCommand.Parameters.AddWithValue("@defender", defender);
                        object? defenceObject = defenderPositionCommand.ExecuteScalar();
                        if (defenceObject != null && int.TryParse(defenceObject.ToString(), out int defencePosition))
                        {

                            var attackId = _db.CreateCommand($"SELECT id FROM position WHERE vertical = '{posLetter}' AND horizontal = {posNumber};");
                            object? attack = attackId.ExecuteScalar();

                            if (attack != null && int.TryParse(attack.ToString(), out int AttackPosition))
                            {
                                var insertAttackedPositionCommand = _db.CreateCommand("INSERT INTO attacked_positions (game_id, user_name, position_id) VALUES ($1, $2, $3);");
                                insertAttackedPositionCommand.Parameters.AddWithValue(gameId);
                                insertAttackedPositionCommand.Parameters.AddWithValue(attacker);
                                insertAttackedPositionCommand.Parameters.AddWithValue(AttackPosition);
                                insertAttackedPositionCommand.ExecuteNonQuery();

                                string map = _updateMap.GetMap(gameId, attacker);
                                byte[] buffer2 = Encoding.UTF8.GetBytes(map);
                                res.OutputStream.Write(buffer2, 0, buffer2.Length);

                                if (defencePosition == AttackPosition)
                                {
                                    var newDefenderHp = _db.CreateCommand($"SELECT hp FROM user_hitpoints WHERE user_name = @defender AND game_id = @gameId;");
                                    newDefenderHp.Parameters.AddWithValue("@defender", defender);
                                    newDefenderHp.Parameters.AddWithValue("@gameId", gameId);
                                    object? defenderHpObject = newDefenderHp.ExecuteScalar();

                                    if (defenderHpObject != null && int.TryParse(defenderHpObject.ToString(), out int defenderHp))
                                    {
                                        if (defenderHp == 1)
                                        {
                                            var hitRemoveHpCommand = _db.CreateCommand($"UPDATE user_hitpoints SET hp = hp - 1 WHERE user_name = @defender AND game_id = @gameId;");
                                            hitRemoveHpCommand.Parameters.AddWithValue("@defender", defender);
                                            hitRemoveHpCommand.Parameters.AddWithValue("@gameId", gameId);
                                            
                                            hitRemoveHpCommand.ExecuteNonQuery();

                                            string message = $"\nKABOOOOM! You destroyed {defender}! You won the game!\n";
                                            byte[] buffer = Encoding.UTF8.GetBytes(message);
                                            res.OutputStream.Write(buffer, 0, buffer.Length);
                                            res.OutputStream.Close();
                                            res.StatusCode = (int)HttpStatusCode.Created;
                                            UpdateWins(attacker);

                                        }
                                        else
                                        {
                                            string message = $"\nGame ended. {attacker} won the game!\n";
                                            byte[] buffer = Encoding.UTF8.GetBytes(message);
                                            res.OutputStream.Write(buffer, 0, buffer.Length);
                                            res.OutputStream.Close();
                                            res.StatusCode = (int)HttpStatusCode.Created;
                                            res.Close();
                                        }
                                    }
                                    else
                                    {
                                        string message = "Could not parse defender HP.";
                                        byte[] buffer = Encoding.UTF8.GetBytes(message);
                                        res.OutputStream.Write(buffer, 0, buffer.Length);
                                        res.OutputStream.Close();
                                        res.StatusCode = (int)HttpStatusCode.Created;
                                    }
                                }
                                else
                                {
                                    string message = "You missed the target!";
                                    byte[] buffer = Encoding.UTF8.GetBytes(message);
                                    res.OutputStream.Write(buffer, 0, buffer.Length);
                                    res.OutputStream.Close();
                                    res.StatusCode = (int)HttpStatusCode.Created;
                                }
                            }
                            else
                            {
                                string message = "Please choose a valid position to attack";
                                byte[] buffer = Encoding.UTF8.GetBytes(message);
                                res.OutputStream.Write(buffer, 0, buffer.Length);
                                res.OutputStream.Close();
                                res.StatusCode = (int)HttpStatusCode.NotAcceptable;
                            }
                        }
                        else
                        {
                            string message = "Can not parse defenceposition";
                            byte[] buffer = Encoding.UTF8.GetBytes(message);
                            res.OutputStream.Write(buffer, 0, buffer.Length);
                            res.OutputStream.Close();
                            res.StatusCode = (int)HttpStatusCode.NotAcceptable;
                        }
                    }
                    else
                    {
                        string input = "Attacker or defender does not exist.";
                        byte[] inputbuffer = Encoding.UTF8.GetBytes(input);
                        res.OutputStream.Write(inputbuffer, 0, inputbuffer.Length);
                        res.OutputStream.Close();
                        res.StatusCode = (int)HttpStatusCode.InternalServerError;
                    }
                }
                else
                {
                    string message = "You have been destroyed! Game over.";
                    byte[] buffer = Encoding.UTF8.GetBytes(message);
                    res.OutputStream.Write(buffer, 0, buffer.Length);
                    res.OutputStream.Close();
                    res.StatusCode = (int)HttpStatusCode.Created;
                }
            }
            else
            {
                string message = "Expected format: game_id,attacker,A,2,defender";
                byte[] buffer = Encoding.UTF8.GetBytes(message);
                res.OutputStream.Write(buffer, 0, buffer.Length);
                res.StatusCode = (int)HttpStatusCode.InternalServerError;
                res.OutputStream.Close();
            }

        }
        else
        {
            string input = "Expected format: game_id,attacker,A,2,defender";
            byte[] inputbuffer = Encoding.UTF8.GetBytes(input);
            res.OutputStream.Write(inputbuffer, 0, inputbuffer.Length);
            res.OutputStream.Close();
            res.StatusCode = (int)HttpStatusCode.InternalServerError;
        }
        res.Close();
    }

    public void UpdateWins(string playerName)
    {
        using var cmd = _db.CreateCommand("UPDATE users SET wins = wins + 1 WHERE name = @playerName");
        cmd.Parameters.AddWithValue("playerName", playerName);
        cmd.ExecuteNonQuery();
    }

}

