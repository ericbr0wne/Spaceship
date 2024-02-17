using System.Net;
using System.Text;

namespace Spaceship;

public class Router
{

    public void NotFound(HttpListenerResponse res)
    {
        string message = $" 404 - Path not found.";
        res.ContentType = "text/plain";
        byte[] buffer = Encoding.UTF8.GetBytes(message);
        res.OutputStream.Write(buffer, 0, buffer.Length);
        res.OutputStream.Close();
        res.StatusCode = (int)HttpStatusCode.NotFound;
        res.Close();
    }

}
