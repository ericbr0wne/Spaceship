using System;
using System.Collections.Generic;
using System.Linq;
using System.Net;
using System.Text;
using System.Threading.Tasks;

namespace Spaceship;

public class Server
{
    bool listen = true;

    public void ConsoleCancel()
    {


        Console.CancelKeyPress += delegate (object? sender, ConsoleCancelEventArgs e)

        {
            Console.WriteLine("Interupting cancel event");
            e.Cancel = true;
            listen = false;
        };


    }

    public void Listener()
    {
        int port = 3000;
        listen = true;
        HttpListener listener = new();
        listener.Prefixes.Add($"http://localhost:{port}/");

        Console.WriteLine("Server is listening");

        try
        {
            listener.Start();
            listener.BeginGetContext(new AsyncCallback(HandleRequest), listener);
            while (listen) { };

        }
        finally
        {
            listener.Stop();
        }

        void HandleRequest(IAsyncResult result)
        {
            if (result.AsyncState is HttpListener listener)
            {
                HttpListenerContext context = listener.EndGetContext(result);

                // metod eller kod här som hanterar request och response från context
                Router router = new();
                router.Navigation(context);


                listener.BeginGetContext(new AsyncCallback(HandleRequest), listener);
            }
        }

    }


}
