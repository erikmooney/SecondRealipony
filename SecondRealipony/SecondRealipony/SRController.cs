using System;
using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;
using Microsoft.Xna.Framework.Input;
using System.Linq;

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
        string VideoPath = @"C:\temp";
        string AudacityProjectFile = "Final Mix.aup";

        int FrameNumber = 0;
        int FrameOffset = 0;
        int SegmentNumber = 0;
        Type[] segmentTypes;
        SRSegment[] segments;
        SRSegment currentSegment { get { return SegmentNumber < segments.Length ? segments[SegmentNumber] : null; } }
        TimeSpan SegmentStartTime;
        SRRenderer renderer;

        public SRController(string[] args)
        {
            var parameters = ParseCommandLine(args);

            graphics = new GraphicsDeviceManager(this);
            graphics.IsFullScreen = false;
            if (VideoMode)
            {
                graphics.PreferredBackBufferWidth = 1280;
                graphics.PreferredBackBufferHeight = 720;
            }
            else
            {
                graphics.PreferredBackBufferWidth = parameters.Item1;
                graphics.PreferredBackBufferHeight = parameters.Item2;
            }
            graphics.PreferMultiSampling = true;
            graphics.PreferredDepthStencilFormat = DepthFormat.Depth24Stencil8;
            graphics.ApplyChanges();
            Content.RootDirectory = "Content";
            IsFixedTimeStep = !VideoMode;
            renderer = new SRRenderer(GraphicsDevice.PresentationParameters.BackBufferWidth, GraphicsDevice.PresentationParameters.BackBufferHeight, VideoPath, VideoFrameRate);

            segmentTypes = parameters.Item3;
        }

        public Tuple<int, int, Type[]> ParseCommandLine(string[] args)
        {
            int width = 0;
            int height = 0;
            Type[] segmentTypes = null;
            char[] validSegments = Enumerable.Range(0, GetStandardSegmentOrder().Length).Select(i => (char)(i + (int)'a')).ToArray();

            bool widthSet = false;
            foreach (string arg in args)
            {
                if (arg.All(c => char.IsDigit(c)))
                {
                    if (!widthSet)
                        width = int.Parse(arg);
                    else
                        height = int.Parse(arg);
                    widthSet = !widthSet;
                }
                else if (arg.All(c => validSegments.Contains(c)))
                    segmentTypes = GetSegmentTypes(arg);
            }

            if (width == 0)
                width = 1280;

            if (height == 0)
                height = width * 9 / 16;

            //If the height exactly matches screen height, subtract a bit to leave room for the title bar so the window doesn't extend offscreen and hurt performance
            if (height == GraphicsAdapter.DefaultAdapter.CurrentDisplayMode.Height)
            {
                width -= 48 * width / height;
                height -= 48;
            }

            if (segmentTypes == null)
                segmentTypes = GetStandardSegmentOrder();

            return new Tuple<int, int, Type[]>(width, height, segmentTypes);
        }

        public Type[] GetStandardSegmentOrder()
        {
            return new Type[] {
                typeof(Intro),
                typeof(Title),
                typeof(Twilight),
                typeof(Rarity),
                typeof(Vinyl),
                typeof(GetDown),
                typeof(Rainbow),
                typeof(EndFirstHalf),
                typeof(Fluttershy),
                typeof(Applejack),
                typeof(Cmc),
                typeof(Cube),
                typeof(Pinkie),
                typeof(Derpy),
                typeof(Waves),
                typeof(EndSecondHalf),
                typeof(WorldStart),
                typeof(World),
                typeof(Thanks),
                typeof(Credits),
                typeof(End)
            };
        }

        public int[] GetSegmentIndices(string input)
        {
            return input.Select(c => (int)c - (int)'a').ToArray();
        }

        public Type[] GetSegmentTypes(string input)
        {
            int[] indices = GetSegmentIndices(input);
            Type[] result = indices.Select(i => GetStandardSegmentOrder()[i]).ToArray();
            return result;
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
            //This instantiates all the segments, and they all load their content in their constructors
            segments = segmentTypes.Select(T => (SRSegment)Activator.CreateInstance(T, this)).ToArray();
            
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
            if (Keyboard.GetState().IsKeyDown(Keys.Escape) || GamePad.GetState(PlayerIndex.One).Buttons.Back == ButtonState.Pressed)
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

            // Ask current segment if it is ready (done with any precalculating).  If not, wait.
            if (!currentSegment.IsReady)
            {
                SegmentStartTime = VideoMode ? GetTimespan(FrameNumber, VideoFrameRate) : gameTime.TotalGameTime;
                return;
            }

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

        protected override void OnExiting(object sender, EventArgs args)
        {
            foreach (var segment in segments)
            {
                segment.AbortThreads();
            }

            base.OnExiting(sender, args);
        }
    }
}
