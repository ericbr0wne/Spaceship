using System.Net;
using System.Text;

namespace Spaceship;

public class Router
{

    public void Navigation(HttpListenerContext context)
    {
        HttpListenerRequest request = context.Request;
        HttpListenerResponse response = context.Response;

        switch (request.HttpMethod, request.Url?.AbsolutePath)
        {
            case ("GET", "/"):
                RootGet(response);
                break;
            case ("POST", "/"):
                // first check then post what u wanted
                RootPost(request, response);
                break;
            default:
                NotFound(response);
                break;
        }
    }


    public void RootGet(HttpListenerResponse response)
    {
        string message = "Skriv encode här";
        byte[] buffer = Encoding.UTF8.GetBytes(message);
        response.ContentType = "text/plain";
        response.StatusCode = (int)HttpStatusCode.OK;

        response.OutputStream.Write(buffer, 0, buffer.Length);
        response.OutputStream.Close();
    }

    public void RootPost(HttpListenerRequest req, HttpListenerResponse res)
    {
        StreamReader reader = new(req.InputStream, req.ContentEncoding);
        string body = reader.ReadToEnd();

        Console.WriteLine($"Created the following in db: {body}");

        res.StatusCode = (int)HttpStatusCode.Created;
        res.Close();
    }

    public void NotFound(HttpListenerResponse res)
    {
        res.StatusCode = (int)HttpStatusCode.NotFound;
        res.Close();
    }

   
}
