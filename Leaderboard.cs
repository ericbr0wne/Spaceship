using System.Net;
using Npgsql;

namespace Spaceship;

public class Leaderboard(NpgsqlDataSource _db)
{
    public void Highscore(HttpListenerResponse res)
    {
        res.ContentType = "text/plain";
        var getHighscore = _db.CreateCommand($"SELECT name, wins FROM users ORDER BY wins DESC; ");
        using var reader = getHighscore.ExecuteReader();
        var header = "\n\x1b[1;33mHIGHSCORE:\n\x1b[0m";
        var responseStream = res.OutputStream;
        var writer = new StreamWriter(responseStream);
        writer.WriteLine(header);

        while (reader.Read())
        {
            var name = reader.GetString(0);
            var wins = reader.GetInt32(1);
            var line = $"Name: {name}, Wins: {wins}";
            writer.WriteLine("\x1b[33m" + line + "\x1b[0m");
        }
        Console.WriteLine("");
        reader.Close();
        writer.Close();
    }
}