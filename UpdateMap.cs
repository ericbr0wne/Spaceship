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
    
    public string GetMap(int gameId, string playerName)
    {
        string[,] map = new string[3, 3];
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

            if (userName == playerName)
            {
                map[row, col] = userName;
            }
        }

        var attackedPositionsCommand = _db.CreateCommand("SELECT position_id FROM attacked_positions WHERE game_id = @gameId AND user_name = @userName");
        attackedPositionsCommand.Parameters.AddWithValue("gameId", gameId);
        attackedPositionsCommand.Parameters.AddWithValue("userName", playerName);
        var attackedReader = attackedPositionsCommand.ExecuteReader();

        while (attackedReader.Read())
        {
            int attackedPositionId = attackedReader.GetInt32(0);

            int row = (attackedPositionId - 1) / 3;
            int col = (attackedPositionId - 1) % 3;

            if (map[row, col] == null)
            {
                map[row, col] = "X";
            }
        }
        
        var mapString = new StringBuilder();
        for (int i = 0; i < 3; i++)
        {
            for (int j = 0; j < 3; j++)
            {
                string cell = map[i, j] ?? "O";
                if (cell.Length > 6)
                {
                    cell = cell.Substring(0, 6);
                }
                mapString.Append(cell.PadRight(6)).Append(" ");
            }
            mapString.AppendLine();
        }
        return mapString.ToString();
    }
}