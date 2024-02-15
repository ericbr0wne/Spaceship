using System.Net;
using System.Text;

namespace Spaceship;

public class Router
{

    public void NotFound(HttpListenerResponse res)
    {
        res.StatusCode = (int)HttpStatusCode.NotFound;
        res.Close();
    }

}
