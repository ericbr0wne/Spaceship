﻿using System.Net;
using System.Text;

namespace Spaceship;

public class Story
{
   public void Intro(HttpListenerResponse res)
    {
        string introMessage =
            "In a war between humanity and other species you find yourself in control of a spaceship \n" +
            "in the middle of the Centurio 5.B galaxy, you ended up here after you warped to get away from a battlefield where \n" +
            "you and your allies were ambushed by the enemy forces.\n" +
            "You scan the area with your radar but it seems that you are alone. \n" +
            "You decide to leave the scanner on and then go to check the damage done to your ship. \n" +
            "While checking the values you see that you only have energy for one lasermissile and that you are very damaged. \n" +
            "You would probably need to get back to a base to repair, the question is do you have enough fuel? \n" +
            "While beeing deep in your thoughts all of a sudden the alarm starts beeping, something is picked up on the radar. \n" +
            "You rush to the radar and see one red dot pretty far away and gather the information that it is one of the \n" +
            "alien enemy ships. With the knowledge of beeing in a bad shape and low on fuel and armed with only one lasermissile. \n" +
            "You see no other way out than giving it your all to destroy the enemy ship in hopes of salvaging their fuel before \n" +
            "more enemies arrive so you can warp to the milky way. ";

        res.ContentType = "text/plain";
        byte[] buffer = Encoding.UTF8.GetBytes(introMessage);
        res.OutputStream.Write(buffer, 0, buffer.Length);
        res.OutputStream.Close();
        res.StatusCode = (int)HttpStatusCode.Created;
    }

   public void Mission(HttpListenerResponse res)
    {
        string missionMessage =
            "Welcome back commander! Your mission today is to destroy the enemy ship and salvage what parts and fuel you can \n" +
            "to be able use it and get back to safety";
        
        res.ContentType = "text/plain";
        byte[] buffer = Encoding.UTF8.GetBytes(missionMessage);
        res.OutputStream.Write(buffer, 0, buffer.Length);
        res.OutputStream.Close();
        res.StatusCode = (int)HttpStatusCode.Created;
    }
}