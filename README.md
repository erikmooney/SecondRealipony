INTRODUCTION
============

Second Realipony is a remake of the legendary classic PC animation demo Second Reality by Future Crew. Ponified, with characters and themes from My Little Pony: Friendship is Magic.

It is written in C# using the Microsoft XNA game development engine.

The code may be reused and redistributed as per the included MIT License.  The artwork assets are NOT included in this license and may NOT be reused or redistributed without permission from the original artists.  See my page at http://www.dos486.com/secondrealipony for a list of the artwork and artists.


NOTICE - EXECUTABLE VERSION COMING
==================================

Thanks for all your support!  By popular demand, I will be working on a proper executable version.  Anyone who wants to help with hardware testing, or knows anything about XNA compiling and packaging, please let me know if you'd like to help.



REQUIREMENTS
============

Microsoft Visual Studio 2010.  I used the free C# Express edition for development.  Professional and higher versions should also work.

Microsoft XNA Game Studio 4.0.

1 GB memory available.  The program loads and precalculates lots of textures and geometry, which take up a lot of space.


MUSIC
=====

I have chosen NOT to include Future Crew's original music, for reasons of copyright and also file size.  Included are just very short placeholder .wav files with the same names, so that the code can run.  For any projects derived from this, you will need to supply your own music.


CODE LAYOUT
===========

The starting point for the code is class SRController, which implements an XNA Game object.  The LoadContent method here describes the order of the segments to be shown in the demonstration.  

SRSegment is an abstract base class from which all the segments derive.  This class contains a basic framework for timing and other operations common to all segments like loading and playing the music, and defining the beat tempo and ending time for the segment.  This class also contains lots of other utility functions such as screen fading and some common geometry operations.

Each segment of the demo resides in its own C# class that derives from SRSegment.  Each of these segments is self-contained with all its own animation and display logic.

World.cs is a special case.  It just displays a placeholder image.  In Second Realipony, this scene was rendered from Source Filmmaker instead and later stitched into the final video.  The C# class for World.cs is just here to preserve the timing and flow with regards to the frame count and music playing.


VIDEO RENDERING
===============

The program can output its graphics to a sequence of PNG files rather than displaying on screen.  To do this, flip the boolean VideoMode to true in SRController, and set the constants VideoFrameRate and VideoPath.  The logic for screenshot rendering is contained in SRRenderer.  Once you have this stream of PNGs, an external tool such as FFmpeg can be used to encode them into a video file.
