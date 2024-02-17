﻿using Npgsql;
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

        string[] split = postBody.Split(",");
        int gameId = int.Parse(split[0]);               //gameid for the attack method
        int attacker = int.Parse(split[1]);             //attacker - player who is attacking  1
        var posLetter = split[2];                       //letter position 
        int posNumber = int.Parse(split[3]);            //nr position
        int defender = int.Parse(split[4]);             //attacked - player who is getting attacked 2

        
        var attackerCommand = _db.CreateCommand($"SELECT hp FROM user_hitpoints WHERE user_id = $1;"); //KOLLA HP PÅ DEFENDER I USERS_HITPOINTS
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
                var defenderPositionCommand = _db.CreateCommand($"SELECT position_id FROM users_x_position WHERE game_id = {gameId} AND user_id = {defender};");
                int defencePosition = Convert.ToInt32(defenderPositionCommand.ExecuteScalar());

                if (defencePosition == AttackPosition)
                {
                    var hitRemoveHpCommand = _db.CreateCommand($"UPDATE user_hitpoints SET hp = hp - 1 WHERE user_id = {defender} AND game_id = {gameId};");
                    hitRemoveHpCommand.ExecuteNonQuery();

                    var newDefenderHp = _db.CreateCommand($"SELECT hp FROM user_hitpoints WHERE user_id = {defender} AND game_id = {gameId};");
                    object? defenderHpObject = newDefenderHp.ExecuteScalar();
                    if (defenderHpObject != null && int.TryParse(defenderHpObject.ToString(), out int defenderHp))
                    {
                        if (defenderHp == 0)
                        {
                            string message ="KABOOOOM! You destroyed the enemy";
                            res.ContentType = "text/plain";
                            byte[] buffer = Encoding.UTF8.GetBytes(message);
                            res.OutputStream.Write(buffer, 0, buffer.Length);
                            res.OutputStream.Close();
                            res.StatusCode = (int)HttpStatusCode.Created;
                            //win_id
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
                        Console.WriteLine("Could not parse defender HP.");
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
        }
        else
        {
            //Denna behöver fixas 
            string message = "Game over! You got destroyed!";
            res.ContentType = "text/plain";
            byte[] buffer = Encoding.UTF8.GetBytes(message);
            res.OutputStream.Write(buffer, 0, buffer.Length);
            res.OutputStream.Close();
            res.StatusCode = (int)HttpStatusCode.Created;
            
            gameover = true;
            //gametable ends. You lost the game!
        }
        res.StatusCode = (int)HttpStatusCode.Created;
        res.Close();
    }

}

