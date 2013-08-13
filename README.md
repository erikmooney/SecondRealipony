INTRODUCTION
============

Second Realipony is a remake of the legendary classic PC animation demo Second Reality by Future Crew. Ponified, with characters and themes from My Little Pony: Friendship is Magic.

It is written in C# using the Microsoft XNA game development engine.

The code may be reused and redistributed as per the included MIT License.  The artwork assets are NOT included in this license and may NOT be reused or redistributed without permission from the original artists.  See my page at http://www.dos486.com/secondrealipony for a list of the artwork and artists.

MP3 renderings of the original music from Second Reality are included here.  The source and assets of Second Reality were published under Unlicense at https://github.com/mtuomi/SecondReality so it appears the music is free to use.  Contact Mika Tuomi (Trug) or others in Future Crew for any questions regarding music usage or reproduction.


COMPILED RELEASE
================

See http://www.dos486.com/secondrealipony for a compiled executable version.  For issues and comments, email me or raise on Github.


COMMAND LINE SUPPORT
====================

Second Realipony accepts a string on the command line to specify a list of particular scenes.  This can be used for testing or troubleshooting or just to watch a favorite scene.  Each scene is represented by a single lowercase letter:

a - Introduction
b - Title screen
c - Twilight Sparkle and the bouncing bookahedron
d - Rarity and the gems dot tunnel
e - Vinyl Scratch and the interference moire lines
f - "Get Down"
g - Rainbow Dash and the rainbow techno light show
h - Shining Armor picture
i - Fluttershy and the text scroller
j - Applejack and the apple lens and rotozoomer
k - Cutie Mark Crusaders and the plasma
l - Cutie marks animated on the cube
m - Pinkie Pie with party cannon and the vector bobs
n - Derpy Hooves and the reflective raytracing
o - Wonderbolts and the waves
p - Princess Celestia bouncing picture
q - Beginning of city world scene (fade to white)
r - City world virtual reality scene (prerendered video)
s - Special Thanks picture
t - Credits
u - End scroller

Thus, running SecondRealipony.exe dggr would run only the scenes with Twilight, Rainbow Dash (twice), and the city world.



DEVELOPMENT REQUIREMENTS
========================

Microsoft Visual Studio 2010.  I used the free C# Express edition for development.  Professional and higher versions should also work.

Microsoft XNA Game Studio 4.0.

1 GB memory available.  The program loads and precalculates lots of textures and geometry, which take up a lot of space.


CODE LAYOUT
===========

The starting point for the code is class SRController, which implements an XNA Game object.  The LoadContent method here describes the order of the segments to be shown in the demonstration.  

SRSegment is an abstract base class from which all the segments derive.  This class contains a basic framework for timing and other operations common to all segments like loading and playing the music, and defining the beat tempo and ending time for the segment.  This class also contains lots of other utility functions such as screen fading and some common geometry operations.

Each segment of the demo resides in its own C# class that derives from SRSegment.  Each of these segments is self-contained with all its own animation and display logic.

World.cs is a special case.  It just plays a prerendered video.  In Second Realipony, this scene was rendered from Source Filmmaker instead and later stitched into the final video.


VIDEO RENDERING
===============

The program can output its graphics to a sequence of PNG files rather than displaying on screen.  To do this, flip the boolean VideoMode to true in SRController, and set the constants VideoFrameRate and VideoPath.  The logic for screenshot rendering is contained in SRRenderer.  Once you have this stream of PNGs, an external tool such as FFmpeg can be used to encode them into a video file.
