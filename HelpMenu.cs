using System.Net;
using System.Text;
namespace Spaceship;

public class HelpMenu
{
    public void Commands(HttpListenerResponse res)
    {
        string helpMenu = "Welcome to this help menu. \n" +
                          "To start a new game you first need to create a character.\n" +
                          "This is the command for that just switch the name to your liking." +
                          "curl -s -d \"eric\" -X POST http://localhost:3000/createplayer\n\n" +
                          "To start a new game use this command and change player1 to your created player\n" +
                          "and choose a position for your character A - C and then 1 - 3 \n" +
                          "curl -d \"new,player1,C,7\" -X POST http://localhost:3000/newgame\n\n" +
                          "To join an existing game use following command and enter the Id you got \n" +
                          "from the host, enter your character name, a position between A - C and 1 - 3 \n" +
                          "curl -d \"gameid,player2,C,2\" -X POST http://localhost:3000/joingame\n\n" +
                          "And finally to attack use the following command and change the gameid to the id that you \n" +
                          "got from your host, your id, the possition you want to attack and the enemy id.\n" +
                          "curl -s -d \"gameid,attacker,E,5,defender\" -X POST http://localhost:3000/attack";
 
        byte[] buffer = Encoding.UTF8.GetBytes(helpMenu);
        res.OutputStream.Write(buffer, 0, buffer.Length);
        res.StatusCode = (int)HttpStatusCode.Created;
        res.OutputStream.Close();
    }
}
