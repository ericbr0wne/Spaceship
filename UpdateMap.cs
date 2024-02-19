using System.Net;
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
    
    public string GetMap(int gameId, string playerName, HttpListenerRequest req, HttpListenerResponse res)
    {
        string[,] map = new string[3, 3];
        var playerPositionsCommand = _db.CreateCommand("SELECT user_name, position_id FROM user_x_position WHERE game_id = @gameId");
        playerPositionsCommand.Parameters.AddWithValue("gameId", gameId);
        var reader = playerPositionsCommand.ExecuteReader();

       // Dictionary<string, int> playerPositions = new Dictionary<string, int>();
        while (reader.Read())
        {
            int positionId = reader.GetInt32(1);

            int row = (positionId - 1) / 3;
            int col = (positionId - 1) % 3;

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
        mapString.AppendLine();

        bool placedA = false;
        bool placedB = false;
        bool placedC = false;
        bool placedCorner = false;
        bool placed1 = false;
        bool placed2 = false;
        bool placed3 = false;

        for (int i = 0; i < 4; i++)
        {
            for (int j = 0; j < 4; j++)
            {
                string cell;

                if (!placedA)
                {
                    cell = "A";
                    placedA = true;
                }
                else if (!placedB)
                {
                    cell = "B";
                    placedB = true;
                }
                else if (!placedC)
                {
                    cell = "C";
                    placedC = true;
                }
                else
                {
                    cell = map[i, j] ?? "O";
                    if (cell.Length > 6)
                    {
                        cell = cell.Substring(0, 6);
                    }
                }

                mapString.Append(cell.PadRight(3)).Append(" ");
            }
            mapString.AppendLine();
            mapString.AppendLine();

        }
        return mapString.ToString();
    }
}