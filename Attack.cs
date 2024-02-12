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

            var attackId = _db.CreateCommand($"SELECT id FROM position WHERE vertical = '$1' AND horizontal = $2;"); //KOLLA POSITION ID
            attackId.Parameters.AddWithValue(posLetter);
            attackId.Parameters.AddWithValue(posNumber);
            object? attack = attackId.ExecuteScalar();
            int AttackPosition = int.Parse(attack.ToString()); //HUR BLIR DENNA INTE NULL?!?!?!?!??!?!

            Console.WriteLine($"{attacker} is attacking on square: {AttackPosition} in x table");
                var defenderPositionCommand = _db.CreateCommand($"SELECT mapid FROM users_x_position where userid = $1;");
                defenderPositionCommand.Parameters.AddWithValue(defenderId);
                int defenderPosition = Convert.ToInt32(defenderPositionCommand.ExecuteScalar());
                defenderPositionCommand.ExecuteNonQuery();
                if (defenderPosition == AttackPosition)
                {
                    var hitRemoveHpCommand = _db.CreateCommand($"UPDATE users SET hp = hp - 1 WHERE id = $1;");
                    hitRemoveHpCommand.Parameters.AddWithValue(defenderId);
                    hitRemoveHpCommand.ExecuteNonQuery();

                    var newDefenderHp = _db.CreateCommand($"SELECT hp FROM users WHERE id = $1;");
                    int defenderHp = Convert.ToInt32(newDefenderHp.ExecuteScalar());
                    newDefenderHp.Parameters.AddWithValue(defenderId);

                    newDefenderHp.ExecuteNonQuery();
                    if (defenderHp == 0)
                    {
                        Console.WriteLine("KABOOOOM! You destroyed the enemy");
                    }
                    else
                    {
                        Console.WriteLine("You hit the enemy and damaged his ship with 1 dmg.");
                    }
                }
                else if (defenderPosition != AttackPosition)
                {

                    Console.WriteLine($"You missed the target!");

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

