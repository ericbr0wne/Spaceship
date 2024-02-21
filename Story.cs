using System.Net;
using System.Text;

namespace Spaceship;

public class Story
{
    [System.Diagnostics.CodeAnalysis.SuppressMessage("Interoperability", "CA1416:Validate platform compatibility", Justification = "<Pending>")]
    public void Intro(HttpListenerResponse res)
    {
        res.ContentType = "text/plain";
        var responseStream = res.OutputStream;
        var writer = new StreamWriter(responseStream);

        try
        {
            // Header med ANSI escape-sekvenser för att göra texten fetstilt och grön
            var header = "\n\x1b[1;32m        In a galaxy far far away....\n\x1b[0m";
            writer.WriteLine(header);

            // Färgkod för rymden (blå)
            writer.Write("\x1b[34m");
            var upperSpace = @"
                .     .
                 .       *
            '                  *
               *       .               *       *
                        *      .    
                    *            *      *       .
                   *
             *           .             *        *";
            writer.WriteLine(upperSpace);

            var beforeLeftWing = @"
                         .           ";
            writer.WriteLine(beforeLeftWing);

            var afterLeftWing = @"
                                              *       .";
            writer.WriteLine(afterLeftWing);
            
            writer.Write("\x1b[33m");
            // ASCII-konst för rymdskeppet
            var ship = @"
                          \ 
                        --=>[]==--
                          /            ";
            writer.WriteLine(ship);

            writer.Write("\x1b[34m");
            var beforeRightWing = @"
                 *      *     ";
            writer.WriteLine(beforeRightWing);

            var afterRightWing = @"
                                    .            *";
            writer.WriteLine(afterRightWing);

            var lowerSpace = @"
            *       @        .                   *      
                               .                      *
                            .      *            *        .     
                                   .         *
                                       *
                        *       .        *         *
                              .      .
                               *            @             ."; 
            writer.WriteLine(lowerSpace);
            
            
            // Återställ standardfärgen
            writer.Write("\x1b[0m");

            // Övrig text (grön)
            var introMessage = @"

        In a war between humanity and other species you find yourself in control of a spaceship
        in the middle of the Centurio 5.B galaxy, you ended up here after you warped to get away from a battlefield where
        you and your allies were ambushed by the enemy forces.

        You scan the area with your radar but it seems that you are alone.
        You decide to leave the scanner on and then go to check the damage done to your ship.
        While checking the values you see that you are very damaged and can't take much more damage.
        You would probably need to get back to a base to repair, the question is do you have enough fuel?
            
        While being deep in your thoughts all of a sudden the alarm starts beeping, something is picked up on the radar.
        You rush to the radar and see one red dot pretty far away and gather the information that it is one of the
        alien enemy ships. 

        With the knowledge of being in a bad shape and low on fuel and armed with lasermissiles.
        You see no other way out than giving it your all to destroy the enemy ship in hopes of salvaging their fuel before
        more enemies arrive so you can warp to the milky way."
                ;

            // Skriv ut resten av texten (grön)
            writer.Write("\x1b[32m");
            writer.WriteLine(introMessage);
        }
        finally
        {
            // Stäng skrivaren och strömmen
            writer.Flush();
            writer.Close();
            responseStream.Close();
        }

        res.StatusCode = (int)HttpStatusCode.Created;
        
        // Define note frequencies in Hz
        int f4 = 349;
        int gS4 = 415;
        int f5 = 698;
        int c5 = 523;
        int a4 = 440;
        int e5 = 659;
        
        // Define note durations in milliseconds
        int quarter = 325;
        int half = 600;
        
        // Play the melody
        Console.Beep(a4, half);
        Console.Beep(a4, half);
        Console.Beep(a4, half);
        Console.Beep(f4, quarter);
        Console.Beep(c5, quarter);
        Console.Beep(a4, half);
        Console.Beep(f4, quarter);
        Console.Beep(c5, quarter);
        Console.Beep(a4, half);
        
        Thread.Sleep(half);
        
        Console.Beep(e5, half);
        Console.Beep(e5, half);
        Console.Beep(e5, half);
        Console.Beep(f5, quarter);
        Console.Beep(c5, quarter);
        Console.Beep(gS4, half);
        Console.Beep(f4, quarter);
        Console.Beep(c5, quarter);
        Console.Beep(a4, half);
        
        Thread.Sleep(quarter);
    }

    public void Mission(HttpListenerResponse res)
    {
        // Delar upp meddelandet i två delar
        string boldMessage = "        Welcome back commander!";
        string regularMessage =
            @"
        Your mission today is to destroy the enemy ship and salvage what parts and fuel you can
        to be able use it and get back to safety.";

        // Förbereder texten för utskrift
        res.ContentType = "text/plain";
        var responseStream = res.OutputStream;
        var writer = new StreamWriter(responseStream);

        try
        {
            // ANSI escape-sekvens för fetstil och mörkblå text
            writer.Write("\n\x1b[1;34m");
            writer.WriteLine(boldMessage);

            // ANSI escape-sekvens för guldig text
            writer.Write("\x1b[33m");
            writer.WriteLine(regularMessage);
        }
        finally
        {
            // Stäng skrivaren och strömmen
            writer.Flush();
            writer.Close();
            responseStream.Close();
        }

        // Ange HTTP-statuskoden
        res.StatusCode = (int)HttpStatusCode.Created;
    }
}