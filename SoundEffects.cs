using System;
using System.Media;

namespace Spaceship; 

public class SoundEffects : IDisposable
{
    private SoundPlayer lazer;
    private SoundPlayer kaboom;

    public SoundEffects()
    {
        // Initialize SoundPlayer objects with file paths
        lazer = new SoundPlayer("../../../lazer.wav"); // Adjust the file path as necessary
        kaboom = new SoundPlayer("../../../kaboom.wav"); // Adjust the file path as necessary
    }
    //C:\Users\Eric Browne\source\repos\Spaceship\SoundEffects.cs
    public SoundPlayer PlayLazer()
    {
        // Play lazer sound
        lazer.Load();
        lazer.Play();

        return lazer;
    }

    public SoundPlayer PlayKaboom()
    {
        // Play kaboom sound
        kaboom.Load();
        kaboom.Play();

        return kaboom;
    }

    public void Dispose()
    {
        // Dispose of SoundPlayer objects
        lazer.Dispose();
        kaboom.Dispose();
    }
}
