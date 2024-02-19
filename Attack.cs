using Npgsql;
using System.Media;
using System.Net;
using System.Text;
namespace Spaceship;

public class Attack
{

    private NpgsqlDataSource _db;
    private SoundPlayer _lazer;
    private SoundPlayer _kaboom;
    public Attack(NpgsqlDataSource db, SoundPlayer lazer, SoundPlayer kaboom)
    {
        _db = db;
        _lazer = lazer;
        _kaboom = kaboom;

    }

    public void AttackPlayer(HttpListenerRequest req, HttpListenerResponse res)
    {
        //curl -s -d "game_id,attacker,A,1,defender" -X POST http://localhost:3000/attack

        StreamReader reader = new(req.InputStream, req.ContentEncoding);
        string postBody = reader.ReadToEnd().ToLower();
        string[] split = postBody.Split(",");
        int gameId = int.Parse(split[0]);
        string attacker = split[1];
        var posLetter = split[2];
        int posNumber = int.Parse(split[3]);
        string defender = split[4];

        if (split.Length == 5 && attacker.ToString() != defender.ToString())
        {
            var findAttacker = _db.CreateCommand("SELECT id FROM users WHERE name = @Attacker");
            findAttacker.Parameters.AddWithValue("@Attacker", attacker);
            int atk = Convert.ToInt32(findAttacker.ExecuteScalar());

            var findDefender = _db.CreateCommand($"SELECT id FROM users WHERE name = @defender;");
            findDefender.Parameters.AddWithValue("@defender", defender);
            int def = Convert.ToInt32(findDefender.ExecuteScalar());

            if (def > 0 && atk > 0)
            {
                try
                {
                    var attackerCommand = _db.CreateCommand($"SELECT hp FROM user_hitpoints WHERE user_name = @Attacker AND game_id = @game_id;");
                    attackerCommand.Parameters.AddWithValue("@Attacker", attacker);
                    attackerCommand.Parameters.AddWithValue("@game_id", gameId);
                    int attackerHp = Convert.ToInt32(attackerCommand.ExecuteScalar());
                    attackerCommand.ExecuteNonQuery();

                    if (attackerHp > 0)
                    {
                        var attackId = _db.CreateCommand($"SELECT id FROM position WHERE vertical = '{posLetter}' AND horizontal = {posNumber};");
                        object? attack = attackId.ExecuteScalar();
                        if (attack != null && int.TryParse(attack.ToString(), out int AttackPosition))
                        {
                            Console.WriteLine($"{attacker} is attacking on square: {posLetter} {posNumber} !");
                            var defenderPositionCommand = _db.CreateCommand($"SELECT position_id FROM users_x_position WHERE game_id = {gameId} AND user_name <> '{attacker}' AND user_name = '{defender}';");
                            int defencePosition = Convert.ToInt32(defenderPositionCommand.ExecuteScalar());

                            if (defencePosition == AttackPosition)
                            {
                                var newDefenderHp = _db.CreateCommand($"SELECT hp FROM user_hitpoints WHERE user_name = '{defender}' AND game_id = {gameId};");
                                object? defenderHpObject = newDefenderHp.ExecuteScalar();
                                if (defenderHpObject != null && int.TryParse(defenderHpObject.ToString(), out int defenderHp))
                                {
                                    if (defenderHp == 1)
                                    {


                                        
                                        var hitRemoveHpCommand = _db.CreateCommand($"UPDATE user_hitpoints SET hp = hp - 1 WHERE user_name = '{defender}' AND game_id = {gameId};");
                                        hitRemoveHpCommand.ExecuteNonQuery();
                                        string message = "\nKABOOOOM! You destroyed the enemy\n";
                                        res.ContentType = "text/plain";
                                        byte[] buffer = Encoding.UTF8.GetBytes(message);
                                        res.OutputStream.Write(buffer, 0, buffer.Length);
                                        res.OutputStream.Close();
                                        res.StatusCode = (int)HttpStatusCode.Created;
                                        UpdateWins(attacker);
                                        EndGame(gameId);
                                        _kaboom.Play();
                                    }
                                    else if (defenderHp <= 0)
                                    {
                                        string message = "\nEnemy already destroyed. You won the game!\n";
                                        res.ContentType = "text/plain";
                                        byte[] buffer = Encoding.UTF8.GetBytes(message);
                                        res.OutputStream.Write(buffer, 0, buffer.Length);
                                        res.OutputStream.Close();
                                        res.StatusCode = (int)HttpStatusCode.Created;
                                        res.Close();
                                    }
                                    else
                                    {
                                      

                                        var hitRemoveHpCommand = _db.CreateCommand($"UPDATE user_hitpoints SET hp = hp - 1 WHERE user_name = '{defender}' AND game_id = {gameId};");
                                        hitRemoveHpCommand.ExecuteNonQuery();
                                        string message = "\nYou hit the enemy and damaged the spaceship with 1 dmg.\n";
                                        res.ContentType = "text/plain";
                                        byte[] buffer = Encoding.UTF8.GetBytes(message);
                                        res.OutputStream.Write(buffer, 0, buffer.Length);
                                        res.OutputStream.Close();
                                        res.StatusCode = (int)HttpStatusCode.Created;
                                        _lazer.Play();
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
                            res.StatusCode = (int)HttpStatusCode.NotAcceptable;
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
                    }
                }
                catch (Exception)
                {
                    string message = "Error: Expected format: game_id,attacker,A,2,defender";
                    byte[] buffer = Encoding.UTF8.GetBytes(message);
                    res.OutputStream.Write(buffer, 0, buffer.Length);
                    res.StatusCode = (int)HttpStatusCode.InternalServerError;
                    res.OutputStream.Close();
                }
            }
            else
            {
                string input = "Attacker or defender does not exist.";
                res.ContentType = "text/plain";
                byte[] inputbuffer = Encoding.UTF8.GetBytes(input);
                res.OutputStream.Write(inputbuffer, 0, inputbuffer.Length);
                res.OutputStream.Close();
                res.StatusCode = (int)HttpStatusCode.InternalServerError;
            }
        }
        else
        {
            string input = "Wrong input or attacking yourself? \nExpected input: game_id,attacker,A,2,defender";
            res.ContentType = "text/plain";
            byte[] inputbuffer = Encoding.UTF8.GetBytes(input);
            res.OutputStream.Write(inputbuffer, 0, inputbuffer.Length);
            res.OutputStream.Close();
            res.StatusCode = (int)HttpStatusCode.InternalServerError;
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
