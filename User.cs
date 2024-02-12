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
    public void Name()
    {



    }


    
        public void PositionPost(HttpListenerRequest req, HttpListenerResponse res)
        {

            //curl -d "C,7,Benny" -X POST http://localhost:3000/post/position
            StreamReader reader = new(req.InputStream, req.ContentEncoding);
            string postBody = reader.ReadToEnd();

            string[] split = postBody.Split(",");
            var posLetter = split[0];
            var posNumber = split[1];
            var posName = split[2];


            var getUserIdCommand = _db.CreateCommand($"SELECT id FROM users WHERE name = '{posName}';");
            int userId = Convert.ToInt32(getUserIdCommand.ExecuteScalar());
            Console.WriteLine($"User ID = {userId}");

            var getMapIdCommand = _db.CreateCommand($"SELECT id FROM position WHERE vertical = '{posLetter}' AND horizontal = {posNumber};");
            object? mapIdObject = getMapIdCommand.ExecuteScalar();

            if (mapIdObject != null && int.TryParse(mapIdObject.ToString(), out int mapId))
            {
                Console.WriteLine($"Map ID = {mapId}");

                var checkIfUserExist = _db.CreateCommand($"SELECT userid FROM users_x_position;");
                int ifUserExist = Convert.ToInt32(checkIfUserExist.ExecuteScalar());
                if (ifUserExist == userId)
                {
                    Console.WriteLine("Sorry user already have a position");
                }
                else if (ifUserExist != mapId)
                {

                    var cmd = _db.CreateCommand("INSERT INTO users_x_position (userid, mapid) VALUES ($1, $2)");
                    cmd.Parameters.AddWithValue(userId);
                    cmd.Parameters.AddWithValue(mapId);
                    Console.WriteLine($"Position set!");

                    cmd.ExecuteNonQuery();
                }
            }
            else
            {
                Console.WriteLine("Failed to retrieve or parse Map ID.");
            }

            res.StatusCode = (int)HttpStatusCode.Created;
            res.Close();

        }


    



}
