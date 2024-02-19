using System.Net;
using System.Text;
namespace Spaceship;

public class HelpMenu
{
    public void Commands(HttpListenerResponse res)
    {
        string helpMenu = @"

    Welcome to this help menu.
    Use the commands below to navigate in the game.


 - CREATE CHARACTER - 
    What name do you want for your new character.
    curl -s -d ""name=PLAYERNAME&password=PSW123"" -X POST http://localhost:3000/newplayer

 - NEW GAME - 
    Add your character to a map position to start a new game.
    Avaiable positions is: A - C and 1 - 3.
    curl -s -d ""new,PLAYERNAME,A,1"" -X POST http://localhost:3000/newgame


 - JOIN GAME - 
    Use the game-id you got from a friend to join a game.
    Add your character to a map position: Avaiable positions is: A - C and 1 - 3.
    curl -s -d ""GAMEID,PLAYERNAME,C,2"" -X POST http://localhost:3000/joingame


 - ATTACK - 
    To attack another player, enter the game-id, your character name,
    the position you would like to attack (A-C,1-3) and the enemy name.                          
    curl -s -d ""GAMEID,YOURNAME,E,5,ENEMYNAME"" -X POST http://localhost:3000/attack";
 
        byte[] buffer = Encoding.UTF8.GetBytes(helpMenu);
        res.OutputStream.Write(buffer, 0, buffer.Length);
        res.StatusCode = (int)HttpStatusCode.Created;
        res.OutputStream.Close();
    }
}
