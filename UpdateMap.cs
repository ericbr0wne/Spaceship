using System.Net;
using System.Text;
using Npgsql;

namespace Spaceship;

public class UpdateMap(NpgsqlDataSource db)
{
    private NpgsqlDataSource _db = db;

    public string GetMap(int gameId, string playerName)
    {
        string[,] map = new string[4, 4];

        var attackedPositionsCommand =
            _db.CreateCommand(
                "SELECT position_id FROM attacked_positions WHERE game_id = @gameId AND user_name = @userName");
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

        mapString.Append(" "); // Start with one spaces to align with the column letters
        for (int j = 1; j <= 3; j++)
        {
            mapString.Append(" " + j + " "); // Reduced padding to 1
        }

        mapString.AppendLine();

        for (int i = 1; i <= 3; i++)
        {
            mapString.Append((char)('A' + i - 1)); // Removed the space before the letter
            for (int j = 1; j <= 3; j++)
            {
                cell = (map[i, j] ?? "O").PadLeft(2); // Reduced padding to 2
                if (map[i, j] == "X")
                {
                    cell = "\x1b[31m" + cell + "\x1b[0m"; // Red
                }
                else
                {
                    cell = "\x1b[32m" + cell + "\x1b[0m"; // Green
                }

                mapString.Append(cell + " ");
            }

            mapString.AppendLine();
        }

        return mapString.ToString();
    }
}