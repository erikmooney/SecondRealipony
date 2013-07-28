using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using Microsoft.Xna.Framework.Graphics;
using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Audio;

namespace SecondRealipony
{
    abstract class SRSegment
    {
        protected GraphicsDevice device;
        protected SoundEffect music;
        protected SoundEffectInstance musicInstance;
        protected bool musicStarted = false;

        public float Beat { get; protected set; }

        //Most segments are 130bpm, so specify this here unless overridden
        public virtual float BeatLength { get { return 60F / 130F; } }

        //Some segments start a bit before zero beat, fading themselves in or some kind of preliminary effect
        public virtual float Anacrusis { get { return 0; } }

        //Each segment defines when it ends, must implement this
        public abstract float EndBeat { get; }

        //Each segment must specify its music name
        public abstract string MusicName { get; }

        //Delay music (only used by title screen)
        public virtual float MusicDelay { get { return 0; } }

        //Workhorse method into each segment class
        protected abstract void DrawSegment();

        public SRSegment(Game game)
        {
            device = game.GraphicsDevice;
            music = game.Content.Load<SoundEffect>("music/" + MusicName);
            musicInstance = music.CreateInstance();
        }

        public bool IsComplete(TimeSpan span)
        {
            return span.TotalSeconds > (EndBeat + Anacrusis) * BeatLength;
        }
        
        public void Draw(TimeSpan span)
        {
            Beat = (float)span.TotalSeconds / BeatLength - Anacrusis;

            if (!musicStarted && Beat + Anacrusis >= MusicDelay)
            {
                musicInstance.Play();
                musicStarted = true;
            }

            DrawSegment();
        }

        //Calculated properties from device
        public float ScreenHypotenuse
        {
            get
            {
                return (float)Math.Sqrt(Math.Pow(device.Viewport.Width, 2) + Math.Pow(device.Viewport.Height, 2));
            }
        }

        public Vector2 ScreenCenter
        {
            get
            {
                return new Vector2(device.Viewport.Width / 2, device.Viewport.Height / 2);
            }
        }

        public Rectangle FullScreen
        {
            get
            {
                return new Rectangle(0, 0, device.PresentationParameters.BackBufferWidth, device.PresentationParameters.BackBufferHeight);
            }
        }

        public float AspectRatio
        {
            get
            {
                return (float)device.Viewport.Width / (float)device.Viewport.Height;
            }
        }

        protected void SetCullMode(CullMode mode)
        {
            RasterizerState rasterizerState1 = new RasterizerState();
            rasterizerState1.CullMode = mode;
            device.RasterizerState = rasterizerState1;
        }

        //Fade entire screen by specified percentage (100% is full black)
        protected void FadeScreen(float blackPercent)
        {
            FadeOrSmash(blackPercent, BlendStateFadeBlack);
        }

        //Smash entire screen by specified percentage (100% is full white)
        protected void SmashScreen(float whitePercent)
        {
            FadeOrSmash(whitePercent, BlendStateSmashWhite);
        }

        protected void FadeOrSmash(float percent, BlendState blendState)
        {
            var texture = Get1x1Texture(new Color(percent, percent, percent, 1.0F)); //alpha is unused
        
            var batch = new SpriteBatch(device);
            batch.Begin(SpriteSortMode.Immediate, blendState);
            batch.Draw(texture, FullScreen, Color.White);
            batch.End();        
        }

        //Parameterized way to fade or smash by beat markers
        //isIn means start with white or black and fade towards the picture
        //!isIn means start with the picture and fade towards white or black
        protected void FadeScreen(float startBeat, float endBeat, float beat, bool isBlack, bool isIn)
        {
            if (beat < startBeat || beat > endBeat)
                return;

            float percent = (beat - startBeat) / (endBeat - startBeat);
            if (isIn)
                percent = 1.0F - percent;

            if (isBlack)
                FadeScreen(percent);
            else
                SmashScreen(percent);
        }
        
        //Create a blend state for smashing to white
        //This blend state causes the source (texture) color to specify how much of the _remaining_ range to smash towards white.
        //Example: Suppose the existing destination (framebuffer) color is 0.4 and source is 0.75.  We want to end up with 0.85.  as 0.4 + (1 - 0.4) * 0.75
        protected static BlendState BlendStateSmashWhite
        {
            get
            {
                var blendState = new BlendState();
                blendState.ColorBlendFunction = BlendFunction.Add;
                blendState.ColorDestinationBlend = Blend.One;
                blendState.ColorSourceBlend = Blend.InverseDestinationColor;
                return blendState;
            }
        }

        //Create a blend state for fading to black
        //This blend state causes the source (texture) color to specify how much of the destination (framebuffer) color to fade towards black.
        protected static BlendState BlendStateFadeBlack
        {
            get
            {
                var blendState = new BlendState();
                blendState.ColorBlendFunction = BlendFunction.Add;
                blendState.ColorDestinationBlend = Blend.InverseSourceColor;
                blendState.ColorSourceBlend = Blend.Zero;
                return blendState;
            }
        }

        protected void SetViewport(float yOffset)
        {
            var height = device.PresentationParameters.BackBufferHeight;
            var viewport = device.Viewport;
            viewport.Y = (int)(yOffset * height);
            viewport.Height = height - viewport.Y;
            device.Viewport = viewport;
        }

        //Take a dictionary of inflection points mapping a key to a value.  Most often Beat is the key.
        //Take a key and figure out which pair of dictionary entries it is between, and SmoothStep it between the values.
        protected static float SmoothDictionaryLookup(SortedDictionary<float, float> phases, float key)
        {
            var previousPoint = phases.LastOrDefault(kvp => kvp.Key <= key);
            var nextPoint = phases.FirstOrDefault(kvp => kvp.Key > key);

            var duration = nextPoint.Key - previousPoint.Key;
            var percentage = (key - previousPoint.Key) / duration;

            return MathHelper.SmoothStep(previousPoint.Value, nextPoint.Value, percentage);
        }

        //Skew time, so that some effects can ramp up from 0 with an acceleration
        //Skew time accelerates to full speed after X beats, after which it is always X/2 beats behind
        protected static float GetSkewBeat(float beat, float threshold)
        {
            float skewBeat = beat < threshold ? beat * beat / (threshold * 2) : beat - (threshold / 2);
            return skewBeat;
        }

        protected static float GetTriangleWave(float x, float amplitude, float period, float phase = 0.25F)
        {
            return (float)Math.Abs(2 * (x / period + phase - Math.Floor(x / period + phase + 0.5))) * amplitude * 2 - amplitude;
        }

        //Get the normal of a triangle
        protected static Vector3 GetNormal(Vector3 v0, Vector3 v1, Vector3 v2)
        {
            Vector3 normal = Vector3.Normalize(Vector3.Cross(v1 - v0, v2 - v0));
            return normal;
        }

        //Create and SetData a 1x1 texture with a single pixel of a specified color
        protected Texture2D Get1x1Texture(Color color)
        {
            var result = new Texture2D(device, 1, 1);
            result.SetData(new Color[] { color });
            return result;
        }

        public struct ColorHSL
        {
            public float H, S, L, A;
            public ColorHSL(float h, float s, float l, float a)
            {
                H = h;
                S = s;
                L = l;
                A = a;
            }
        }
        
        protected static Microsoft.Xna.Framework.Color HSLtoRGB(ColorHSL hsl)
        {
            if (hsl.S == 0)
                return new Microsoft.Xna.Framework.Color(hsl.L, hsl.L, hsl.L, hsl.A);

            float temp1;
            float temp2;
            float Rtemp3, Gtemp3, Btemp3;
            float H;

            if (hsl.L < 0.5f)
                temp2 = hsl.L * (1.0f + hsl.S);
            else
                temp2 = hsl.L + hsl.S - hsl.L * hsl.S;

            temp1 = 2.0f * hsl.L - temp2;

            H = hsl.H / 360;

            Rtemp3 = H + 1.0f / 3.0f;
            Gtemp3 = H;
            Btemp3 = H - 1.0f / 3.0f;

            if (Rtemp3 < 0)
                Rtemp3 += 1.0f;
            if (Rtemp3 > 1)
                Rtemp3 -= 1.0f;
            if (Gtemp3 < 0)
                Gtemp3 += 1.0f;
            if (Gtemp3 > 1)
                Gtemp3 -= 1.0f;
            if (Btemp3 < 0)
                Btemp3 += 1.0f;
            if (Btemp3 > 1)
                Btemp3 -= 1.0f;

            float R, G, B;

            if ((6.0f * Rtemp3) < 1)
                R = temp1 + (temp2 - temp1) * 6.0f * Rtemp3;
            else if (2.0f * Rtemp3 < 1)
                R = temp2;
            else if (3.0f * Rtemp3 < 2)
                R = temp1 + (temp2 - temp1) * ((2.0f / 3.0f) - Rtemp3) * 6.0f;
            else
                R = temp1;

            if ((6.0f * Gtemp3) < 1)
                G = temp1 + (temp2 - temp1) * 6.0f * Gtemp3;
            else if (2.0f * Gtemp3 < 1)
                G = temp2;
            else if (3.0f * Gtemp3 < 2)
                G = temp1 + (temp2 - temp1) * ((2.0f / 3.0f) - Gtemp3) * 6.0f;
            else
                G = temp1;

            if ((6.0f * Btemp3) < 1)
                B = temp1 + (temp2 - temp1) * 6.0f * Btemp3;
            else if (2.0f * Btemp3 < 1)
                B = temp2;
            else if (3.0f * Btemp3 < 2)
                B = temp1 + (temp2 - temp1) * ((2.0f / 3.0f) - Btemp3) * 6.0f;
            else
                B = temp1;

            return new Microsoft.Xna.Framework.Color(R, G, B, hsl.A);
        }

        //Calculate the coordinate of an object that is bouncing off the floor.
        //Apply the equation of motion, with an initial position, velocity, acceleration
        //If the coordinate comes out below a specified floor, the object has bounced.  Reset position, velocity, acceleration to the instant of the bounce, and try again.
        //I could only figure out how to do this by iterating through each bounce, but at least we don't have to actually iterate through increments of time.
        //Signed with positive upwards, so accel should be negative for gravity
        public static float GetBouncedCoordinate(float time, float init, float vel, float accel, float elasticity, float floor, int maxBounces)
        {
            int attempts = 0;
            do
            {
                //Equation of motion
                float coord = init + vel * time + 0.5F * accel * time * time;
                if (coord > floor || attempts == maxBounces)
                    return coord;

                //Object is below the floor, so it bounced in the past.  Find the time instant of the bounce.  Assume the larger root of the quadratic.
                float bounceInstant = SolveQuadratic(0.5F * accel, vel, init - floor, true);

                //Reset position and velocity to the instant of the bounce, reset time so that the bounce instant is 0, and try again
                init = floor;
                vel = (vel + bounceInstant * accel) * -elasticity;
                time = time - bounceInstant;
            }
            while (++attempts < 10);        //maximum of 10 bounces then assume it settles on the floor
            return floor;
        }

        public static float SolveQuadratic(float a, float b, float c, bool upperRoot = true)
        {
            float discriminant = b * b - 4 * a * c;

            if (discriminant < 0)
                return float.NaN;

            float root1 = (float)(-b + Math.Sqrt(discriminant)) / (2 * a);
            float root2 = (float)(-b - Math.Sqrt(discriminant)) / (2 * a);

            return (upperRoot == root1 >= root2) ? root1 : root2;
        }

        //Create quad in XY plane (Z = 0) as 4 vertices for drawing as TriangleStrip
        //remember: in 3d space, Y is positive upwards
        //in texture space, Y is positive downwards
        public static VertexPositionTexture[] CreateQuad(float width, float height, Vector2 upperLeft, Vector2 lowerRight)
        {
            return new VertexPositionTexture[] {
                new VertexPositionTexture(new Vector3(-width / 2, height / 2, 0), upperLeft),
                new VertexPositionTexture(new Vector3(-width / 2, -height / 2, 0), new Vector2(upperLeft.X, lowerRight.Y)),
                new VertexPositionTexture(new Vector3(width / 2, height / 2, 0), new Vector2(lowerRight.X, upperLeft.Y)),
                new VertexPositionTexture(new Vector3(width / 2, -height / 2, 0), lowerRight),
            };
        }

        public static VertexPositionTexture[] CreateQuad(float width, float height)
        {
            return CreateQuad(width, height, Vector2.Zero, Vector2.One);
        }

        public static VertexPositionTexture[] CreateQuad()
        {
            return CreateQuad(1, 1);
        }

        public static Color AddColors(Color color1, Color color2)
        {
            return new Color(color1.R + color2.R, color1.G + color2.G, color1.B + color2.B, color1.A + color2.A);
        }

        public Matrix CreatePerspectiveAtDepth(float width, float distance, float nearPlane, float farPlane)
        {
            return Matrix.CreatePerspective(width * nearPlane / distance, (width * nearPlane / distance) / AspectRatio, nearPlane, farPlane);
        }

        //Build indices for a triangle strip mesh
        public void CreateIndices(int width, int height, out int[] indices, out IndexBuffer indexBuffer)
        {
            //There are Height - 1 strips
            //For each strip, duplicate the first and last vertices to create triangle strip discontinuity
            //Except first strip does not duplicate first vertex
            indices = new int[(width + 1) * (height - 1) * 2];
            int i = 0;

            for (int stripRow = 0; stripRow < height - 1; stripRow++)
            {
                //Duplicate first vertex, EXCEPT for first strip
                if (i > 0)
                    indices[i++] = stripRow * width;

                for (int stripCol = 0; stripCol < width; stripCol++)
                {
                    indices[i++] = stripRow * width + stripCol;
                    indices[i++] = (stripRow + 1) * width + stripCol;
                }
                indices[i++] = (stripRow + 1) * width + width - 1;
            }

            indexBuffer = new IndexBuffer(device, typeof(int), indices.Length, BufferUsage.None);
            indexBuffer.SetData<int>(indices);
        }
    }

    public static class MyExtensions
    {
        public static Vector2 Center(this Texture2D texture)
        {
            return new Vector2(texture.Width / 2F, texture.Height / 2F);
        }
    }

    public class PerlinOctave
    {
        public int Height;
        public int Width;
        public float[,] Values;
        public float Amplitude;

        public PerlinOctave(int width, int height, float amplitude)
        {
            Height = height;
            Width = width;
            Values = new float[width, height];
            Amplitude = amplitude;
        }

        public void Seed(int seed = 0)
        {
            var random = new Random(seed);
            for (int i = 0; i < Values.GetLength(0); i++)
            {
                for (int j = 0; j < Values.GetLength(1); j++)
                {
                    Values[i, j] = ((float)random.NextDouble() * 2 - 1) * Amplitude;
                }
            }
        }

        //Pass ONLY values in the range [0,1) into here.  (Not 1 exactly.)  No bounds checking or error handling, for speed
        public float GetValue(float x, float y)
        {
            int lowerX = (int)(x * Width);
            int lowerY = (int)(y * Height);
            int upperX = lowerX + (lowerX < Width - 1 ? 1 : 0);
            int upperY = lowerY + (lowerY < Width - 1 ? 1 : 0);

            float xPercent = (x - (lowerX / (float)Width)) * Width;
            float yPercent = (y - (lowerY / (float)Height)) * Height;

            return Amplitude *
                MathHelper.Lerp(
                    MathHelper.Lerp(Values[lowerX, lowerY], Values[upperX, lowerY], xPercent),
                    MathHelper.Lerp(Values[lowerX, upperY], Values[upperX, upperY], xPercent),
                    yPercent);
        }
    }
}
