using System;
using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;
using Microsoft.Xna.Framework.Input;

namespace SecondRealipony
{
    /// <summary>
    /// This is the main type for your game
    /// </summary>
    public class SRController : Microsoft.Xna.Framework.Game
    {
        GraphicsDeviceManager graphics;

        bool VideoMode = false;
        double VideoFrameRate = 60D; //60000D/1001D;
        string VideoPath = @"D:\temp";
        string AudacityProjectFile = "Final Mix.aup";

        int FrameNumber = 0;
        int FrameOffset = 0;
        int SegmentNumber = 0;
        SRSegment[] segments;
        SRSegment currentSegment { get { return SegmentNumber < segments.Length ? segments[SegmentNumber] : null; } }
        TimeSpan SegmentStartTime;
        SRRenderer renderer;

        public SRController()
        {
            graphics = new GraphicsDeviceManager(this);
            graphics.IsFullScreen = false;
            if (graphics.IsFullScreen)
            {
                graphics.PreferredBackBufferWidth = 1600;
                graphics.PreferredBackBufferHeight = 1200;
            }
            else if (VideoMode)
            {
                graphics.PreferredBackBufferWidth = 1280;
                graphics.PreferredBackBufferHeight = 720;
            }
            else
            {
                graphics.PreferredBackBufferWidth = 1280;
                graphics.PreferredBackBufferHeight = 720;
            }
            graphics.PreferMultiSampling = true;
            graphics.PreferredDepthStencilFormat = DepthFormat.Depth24Stencil8;
            graphics.ApplyChanges();
            Content.RootDirectory = "Content";
            IsFixedTimeStep = !VideoMode;
            renderer = new SRRenderer(GraphicsDevice.PresentationParameters.BackBufferWidth, GraphicsDevice.PresentationParameters.BackBufferHeight, VideoPath, VideoFrameRate);
        }

        /// <summary>
        /// Allows the game to perform any initialization it needs to before starting to run.
        /// This is where it can query for any required services and load any non-graphic
        /// related content.  Calling base.Initialize will enumerate through any components
        /// and initialize them as well.
        /// </summary>
        protected override void Initialize()
        {
            base.Initialize();
        }

        /// <summary>
        /// LoadContent will be called once per game and is the place to load
        /// all of your content.
        /// </summary>
        protected override void LoadContent()
        {
            segments = new SRSegment[] {
                new Applejack(this),
                /*new Intro(this),
                new Title(this),
                new Twilight(this),
                new Rarity(this),
                new Vinyl(this),
                new GetDown(this),
                new Rainbow(this),
                new EndFirstHalf(this),
                new Fluttershy(this),
                new Applejack(this),
                new Cmc(this),
                new Cube(this),
                new Pinkie(this),
                new Derpy(this),
                new Waves(this),
                new EndSecondHalf(this),
                new WorldStart(this),
                new World(this),
                new Thanks(this),
                new Credits(this),
                new End(this)*/
            };

            if (VideoMode)
                renderer.WriteMusicTimes(segments, AudacityProjectFile);
        }

        /// <summary>
        /// UnloadContent will be called once per game and is the place to unload
        /// all content.
        /// </summary>
        protected override void UnloadContent()
        {
        }

        /// <summary>
        /// Allows the game to run logic such as updating the world,
        /// checking for collisions, gathering input, and playing audio.
        /// </summary>
        /// <param name="gameTime">Provides a snapshot of timing values.</param>
        protected override void Update(GameTime gameTime)
        {
            // Allows the game to exit
            if (GamePad.GetState(PlayerIndex.One).Buttons.Back == ButtonState.Pressed)
                this.Exit();

            base.Update(gameTime);
        }

        /// <summary>
        /// This is called when the game should draw itself.
        /// </summary>
        /// <param name="gameTime">Provides a snapshot of timing values.</param>
        protected override void Draw(GameTime gameTime)
        {
            TimeSpan span = (VideoMode ? GetTimespan(FrameNumber, VideoFrameRate) : gameTime.TotalGameTime) - SegmentStartTime;

            //System.Console.WriteLine("Draw called: " + gameTime.TotalGameTime.ToString() + " " + span.ToString());

            //Rules for timing: Each segment reports that it has completed on the first frame that is past its stated end time.
            //If the segment reported IsComplete, move on to the next one immediately (not next frame, now)
            if (currentSegment.IsComplete(span))
            {
                ResetViewport();
                SegmentNumber++;
                if (currentSegment == null)
                {
                    this.Exit();
                    return;
                }
                SegmentStartTime = VideoMode ? GetTimespan(FrameNumber, VideoFrameRate) : gameTime.TotalGameTime;
                span = TimeSpan.Zero;
            }

            GraphicsDevice.Clear(Microsoft.Xna.Framework.Color.Black);
            currentSegment.Draw(span);

            if (VideoMode)
            {
                renderer.RenderScreenshot(GraphicsDevice, FrameNumber + FrameOffset);
            }

            base.Draw(gameTime);
            FrameNumber++;
        }

        public static TimeSpan GetTimespan(int FrameNumber, double VideoFrameRate)
        {
            //FromTicks is the only way to get a TimeSpan with fractional milliseconds.  FromMilliseconds truncates the input to an integral number of msec
            return TimeSpan.FromTicks((long)(FrameNumber / VideoFrameRate * 10000000D));
        }

        protected void ResetViewport()
        {
            var viewport = GraphicsDevice.Viewport;
            viewport.X = 0;
            viewport.Y = 0;
            viewport.Width = GraphicsDevice.PresentationParameters.BackBufferWidth;
            viewport.Height = GraphicsDevice.PresentationParameters.BackBufferHeight;
            GraphicsDevice.Viewport = viewport;
        }
    }
}
