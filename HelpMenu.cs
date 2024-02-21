using System.Net;
using System.Text;
namespace Spaceship;

public class HelpMenu
{
   public void Commands(HttpListenerResponse res)
   {
      // Header med ANSI escape-sekvenser för att göra texten neon grön
      string header = "\n\x1b[38;5;82mWELCOME to this help menu.\n\x1b[0m";
      res.OutputStream.Write(Encoding.UTF8.GetBytes(header), 0, header.Length);

      string helpMenu = @"
Use the commands below to navigate in the game.

 - START INTRO -
    To display the game intro:
    curl -s http://localhost:3000/start

 - MISSION -
    Display your mission:
    curl -s http://localhost:3000/mission

 - CREATE CHARACTER - 
    What name do you want for your new character.
    curl -s -d ""PLAYERNAME"" http://localhost:3000/createplayer

 - NEW GAME - 
    Add your character to a map position to start a new game.
    Available positions are: A - C and 1 - 3.
    curl -s -d ""new,PLAYERNAME,A,1"" http://localhost:3000/newgame

 - JOIN GAME - 
    Use the game-id you got from a friend to join a game.
    Add your character to a map position: Available positions are: A - C and 1 - 3.
    curl -s -d ""GAMEID,PLAYERNAME,C,2"" http://localhost:3000/joingame

 - ATTACK - 
    To attack another player, enter the game-id, your character name,
    the position you would like to attack (A-C,1-3) and the enemy name.                          
    curl -s -d ""GAMEID,YOURNAME,E,5,ENEMYNAME"" http://localhost:3000/attack

 - GLOBAL CHAT -
    To use global chat:
    curl -s -d ""PLAYERNAME,message"" http://localhost:3000/chat

 - HIGHSCORE -
    To view the leaderboard:
    curl -s http://localhost:3000/highscore

 - USERS -
    To view players:
    curl -s http://localhost:3000/users

   - HP -
   To see players HP:
   curl -s localhost:3000/hp

";
      string neonBlue = "\x1b[38;5;75m";
   // Konvertera texten till byte-array och skriv till utmatningsströmmen
      byte[] buffer = Encoding.UTF8.GetBytes(neonBlue + helpMenu + "\x1b[0m");
      res.OutputStream.Write(buffer, 0, buffer.Length);
      res.StatusCode = (int)HttpStatusCode.Created;
      res.OutputStream.Close();
   }
}
