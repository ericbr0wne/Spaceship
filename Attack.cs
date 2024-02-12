using Npgsql;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Net;
using System.Text;
using System.Threading.Tasks;

namespace Spaceship;

public class Attack
{
    private NpgsqlDataSource _db;
    bool gameover = false;
    public Attack(NpgsqlDataSource db)
    {
        _db = db;

    }
    public void Check(HttpListenerRequest req, HttpListenerResponse res)
    {

        //curl -d "player1,C,4,player2" -X POST http://localhost:3000/attack
        StreamReader reader = new(req.InputStream, req.ContentEncoding);
        string postBody = reader.ReadToEnd();

        string[] split = postBody.Split(",");
        var attacker = split[0];                //you
        var posLetter = split[1];               //letter
        int posNumber = int.Parse(split[2]);    //nbr
        var defender = split[3];                //defender

        var attackerCommand = _db.CreateCommand($"SELECT hp FROM users WHERE id = $1;"); //KOLLA HP PÅ DEFENDER
        attackerCommand.Parameters.AddWithValue(attacker);
        int attackerHp = Convert.ToInt32(attackerCommand.ExecuteScalar());
        attackerCommand.ExecuteNonQuery();
        if (attackerHp > 0)
        {
            var attackerId = _db.CreateCommand($"SELECT id FROM users WHERE id = $1;");   //KOLLA VEM SOM ÄR ATTACKER (VIA ID)
            attackerId.Parameters.AddWithValue(attacker);
            attackerId.ExecuteNonQuery();

            var defenderId = _db.CreateCommand($"SELECT id FROM users WHERE id = $1;");   //KOLLA VEM SOM ÄR DEFENDER (VIA ID)
            defenderId.Parameters.AddWithValue(defender);
            defenderId.ExecuteNonQuery();


            //var getMapIdCommand = _db.CreateCommand($"SELECT id FROM position WHERE vertical = '{posLetter}' AND horizontal = {posNumber};");

            var attackId = _db.CreateCommand($"SELECT id FROM position WHERE vertical = '{posLetter}' AND horizontal = {posNumber};"); //KOLLA POSITION ID
            object? attack = attackId.ExecuteScalar();
            if (attack != null && int.TryParse(attack.ToString(), out int AttackPosition))
            {
                Console.WriteLine($"{attacker} is attacking on square: {posLetter} {posNumber} !");

                var defenderPositionCommand = _db.CreateCommand($"SELECT mapID FROM users_x_position WHERE userID = '{defender}';");
                int defencePosition = Convert.ToInt32(defenderPositionCommand.ExecuteScalar());



                if (defencePosition == AttackPosition)
                {
                    var hitRemoveHpCommand = _db.CreateCommand($"UPDATE users SET hp = hp - 1 WHERE id = '{defender}';");
                    hitRemoveHpCommand.ExecuteNonQuery();

                    var newDefenderHp = _db.CreateCommand($"SELECT hp FROM users WHERE id = '{defender}';");
                    object? defenderHpObject = newDefenderHp.ExecuteScalar();
                    if (defenderHpObject != null && int.TryParse(defenderHpObject.ToString(), out int defenderHp))
                    {
                        if (defenderHp == 0)
                        {
                            Console.WriteLine("KABOOOOM! You destroyed the enemy");
                        }
                        else
                        {
                            Console.WriteLine("You hit the enemy and damaged his ship with 1 dmg.");
                        }
                    }
                    else
                    {
                        Console.WriteLine("Could not parse defender HP.");
                    }
                }
                else if (defencePosition != AttackPosition)
                {

                    Console.WriteLine($"You missed the target!");

                }
            }
            else
            {

            }

        }
        else
        {
            Console.WriteLine("Game over! You got destroyed!");
            gameover = true;
            //listen = false and break;
        }
        res.StatusCode = (int)HttpStatusCode.Created;
        res.Close();
    }

}

