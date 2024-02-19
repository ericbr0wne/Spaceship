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

    public string GetMap(int gameId, string playerName)
    {
        string[,] map = new string[4, 4];
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
            row++;
            col++;

            if (map[row, col] == null)
            {
                map[row, col] = "X";
            }
        }

        var mapString = new StringBuilder();
        mapString.AppendLine();

        string cell;

        for (int i = 0; i < 4; i++)
        {
            for (int j = 0; j < 4; j++)
            {

                if (i==0 && j == 0)
                {
                    cell = "     ";
                }

                else if (i == 0 && j == 1)
                {
                    cell = "1";
                }
                else if (i == 0 && j == 2)
                {
                    cell = "2";
                }
                else if (i == 0 && j == 3)
                {
                    cell = "3";
                }
                else if (i == 1 && j == 0)
                {
                    cell = "   A ";
                }

                else if (i == 2 && j == 0)
                {
                    cell = "   B ";
                }
                else if (i == 3 && j == 0)
                {
                    cell = "   C ";
                }
                else
                {
                    cell = map[i, j] ?? "O";
                }
                


                mapString.Append(cell.PadRight(3)).Append(" ");
            }
            mapString.AppendLine();
            mapString.AppendLine();

        }
        return mapString.ToString();
    }
}