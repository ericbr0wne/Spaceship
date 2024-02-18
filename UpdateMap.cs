using System.Text;
using Npgsql;

namespace Spaceship;

public class UpdateMap
{
    private NpgsqlDataSource _db;
    public UpdateMap(NpgsqlDataSource db)
    {
        _db = db;
    }
    
    public string GetMap(int gameId)
    {
        string[,] map = new string[3, 3];
        // Fetch player positions
        var playerPositionsCommand = _db.CreateCommand("SELECT user_name, position_id FROM users_x_position WHERE game_id = @gameId");
        playerPositionsCommand.Parameters.AddWithValue("gameId", gameId);
        var reader = playerPositionsCommand.ExecuteReader();

       // Dictionary<string, int> playerPositions = new Dictionary<string, int>();
        while (reader.Read())
        {
            string userName = reader.GetString(0);
            int positionId = reader.GetInt32(1);

            int row = (positionId - 1) / 3;
            int col = (positionId - 1) % 3;
            
            map[row, col] = userName;
        }

        // var attackPositionsCommand = _db.CreateCommand("SELECT user_name, position_id FROM attack_positions WHERE game_id = @gameId");
        // attackPositionsCommand.Parameters.AddWithValue("gameId", gameId);
        // var attackPositions = attackPositionsCommand.ExecuteReader();

// Fill in player positions and attack positions
// ...

        var mapString = new StringBuilder();
        for (int i = 0; i < 3; i++)
        {
            for (int j = 0; j < 3; j++)
            {
                mapString.Append(map[i, j] ?? "0").Append(" ");
            }
            mapString.AppendLine();
        }
        return mapString.ToString();
    }
}