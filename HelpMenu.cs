namespace Spaceship;

public class HelpMenu
{
    public void Commands()
    {
        string helpMenu = "Welcome to this help menu. \n" +
                          "To start a new game you first need to create a character." +
                          "This is the command for that just switch the name to your liking and remove the '\':" +
                          "curl -s -d \"eric\" -X POST http://localhost:3000/createplayer" +
                          "\n" +
                          "To "
    }
}