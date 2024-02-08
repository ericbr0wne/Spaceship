using System;
using System.Collections.Generic;
using System.Linq;
using System.Net;
using System.Text;
using System.Threading.Tasks;

namespace Spaceship;

public class Server
{
    public void ConsoleCancel()
    {

        bool listen = true;

        /// Handle ctrl + c interup event, and gracefully shut down server
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
        bool listen = true;

        HttpListener listener = new();
        listener.Prefixes.Add($"<host>:{port}/"); // <host> kan t.ex. vara 127.0.0.1, 0.0.0.0, ...

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

                listener.BeginGetContext(new AsyncCallback(HandleRequest), listener);
            }
        }

    }

}
