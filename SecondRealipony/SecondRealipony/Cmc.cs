using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using Microsoft.Xna.Framework.Graphics;
using Microsoft.Xna.Framework;

namespace SecondRealipony
{
    class Cmc : SRSegment
    {
        public override float EndBeat { get { return 80; } }
        public override string MusicName { get { return "cmc.wav"; } }

        Texture2D texture;
        Color[] textureColors;
        Texture2D[] cmcTextures;
        Color[][] cmcTexels;
        Color[][] cmcChannels;
        PrecalculatedPoint[] channelPoints;
        PrecalculatedPoint[] cmcPoints;

        //The CMC textures MUST be this size
        const int WIDTH = 240;
        const int HEIGHT = 135;

        public class PrecalculatedPoint
        {
            public float r;
            public float theta;
            public float rLog;
            public float dampening;

            public PrecalculatedPoint(int x, int y, Vector2 origin)
            {
                var point = new Vector2(x, y);
                var relative = point - origin;
                r = relative.Length();
                theta = (float)Math.Atan2(relative.Y, relative.X);
                rLog = (float)Math.Log(r, 10);
                dampening = 1 - r / (new Vector2(WIDTH, 0) - origin).Length();
            }
        }

        public Cmc(Game game)
            : base(game)
        {
            cmcTextures = new Texture2D[3];
            cmcTextures[0] = game.Content.Load<Texture2D>("cmc1.png");
            cmcTextures[1] = game.Content.Load<Texture2D>("cmc2.png");
            cmcTextures[2] = game.Content.Load<Texture2D>("cmc3.png");

            cmcTexels = new Color[3][];
            for (int i = 0; i < 3; i++)
            {
                cmcTexels[i] = new Color[WIDTH * HEIGHT];
                cmcTextures[i].GetData<Color>(cmcTexels[i]);
            }

            cmcChannels = new Color[][] {
                new Color[] { new Color(210, 20, 255), new Color(220, 220, 220) },       //Purple and gray for Sweetie Belle
                new Color[] { new Color(255, 150, 40), new Color(255, 64, 192) },       //Orange and maroon for Scootaloo
                new Color[] { new Color(255, 255, 0), new Color(255, 0, 64) }         //Yellow and red for Apple Bloom
            };

            texture = new Texture2D(device, WIDTH, HEIGHT);
            textureColors = new Color[WIDTH * HEIGHT];

            PrecalculatePoints();
        }

        protected void PrecalculatePoints()
        {
            channelPoints = GetPrecalculatedPoints(new Vector2(0, HEIGHT * 1.2F)).ToArray();
            cmcPoints = GetPrecalculatedPoints(new Vector2(WIDTH / 2, HEIGHT * 1.2F)).ToArray();
        }

        private IEnumerable<PrecalculatedPoint> GetPrecalculatedPoints(Vector2 origin)
        {
            return from y in Enumerable.Range(0, HEIGHT)
                   from x in Enumerable.Range(0, WIDTH)
                   select new PrecalculatedPoint(x, y, origin);
        }

        protected override void DrawSegment()
        {
            //Unset texture so we can load data into it again.  Not entirely convinced this is kosher, device only has one texture at a time?
            device.Textures[0] = null;
            CalculateColors();
            texture.SetData(textureColors);

            SetViewport(0);
            DropViewport(30, 32, Beat);
            DropViewport(62, 64, Beat);
            DropViewport(78, 80, Beat);

            var batch = new SpriteBatch(device);
            batch.Begin(SpriteSortMode.Immediate, BlendState.Opaque, SamplerState.LinearClamp, DepthStencilState.None, RasterizerState.CullNone);
            batch.Draw(texture, FullScreen, Color.White);
            batch.End();

            FadeScreen(0, 3, Beat, false, true);
            FadeScreen(32, 34, Beat, true, true);
            FadeScreen(64, 66, Beat, true, true);
        }

        int phase = 0;
        private void CalculateColors()
        {
            phase++;

            //experimented with this:
            //Calculate every other Y coord and every other X coord every frame (so it takes 4 frames to fully update)
            //Both for speed and because Second Reality looks to do something similar, creating a dithering effect that this approximates
            //for (int y = (phase & 1); y < HEIGHT; y += 2)
            //for (int x = (phase & 2) >> 1; x < WIDTH; x += 2)

            int cmc = (int)Beat / 32;
            float faceBeat = Beat % 32;

            for (int y = 0; y < HEIGHT; y++)
            {
                for (int x = 0; x < WIDTH; x++)
                {
                    int texelOffset = y * WIDTH + x;
                    var color0 = cmcTexels[cmc][texelOffset];
                    var channel0 = GetChannel0(x, y, faceBeat, cmc == 2);
                    var channel0factor = channel0 * color0.A / 255F;    //Strength of lerp factor towards the CMC texel - it's 0 if the CMC texel is transparent

                    var color1 = cmcChannels[cmc][0];
                    var channel1 = GetChannel(x, y, faceBeat);
                    var channel1Color = Color.Multiply(color1, channel1);

                    var color2 = cmcChannels[cmc][1];
                    var channel2 = GetChannel((WIDTH - x - 1), y, (faceBeat + 32) * 0.9F);
                    var channel2Color = Color.Multiply(color2, channel2);

                    //This is overdrivey, we could be summing the CMC pixel twice, but realistically that doesn't seem to happen
                    var finalColor = Color.Lerp(AddColors(channel1Color, channel2Color), color0, channel0factor);

                    textureColors[texelOffset] = finalColor;
                }
            }
        }

        private float GetChannel0(int x, int y, float beat, bool doubleSpeed)
        {
            int offset = y * WIDTH + x;

            float wave1 = (float)Math.Sin((cmcPoints[offset].rLog * 30F + Math.Sin((cmcPoints[offset].theta * 5) * 1.5F)) / 1F - beat * 2.00F);

            if (doubleSpeed)
                beat *= 2;
            
            float materializing = MathHelper.Clamp((beat - 8) / 24F, 0, 1);
            float solidifying = MathHelper.Clamp((beat - 20) / 10F, 0, 1);
            
            return wave1 * materializing * materializing * (1 - solidifying) + solidifying;
        }

        //Basic structure of these waves:
        //((Primary axis + sin(something to make the wavefront nonlinear) * weight of nonlinearity) / spatial width - beat * speed)
        private float GetChannel(int x, int y, float beat)
        {
            int offset = y * WIDTH + x;

            float wave1 = (float)Math.Sin((channelPoints[offset].rLog * 30F + Math.Sin((channelPoints[offset].theta * 3 - beat * 0.2F) * 2.2F)) / 1F - beat * 0.86F);
            float wave2 = (float)Math.Cos((x / 1.2F + y + Math.Sin((y / 2.8F - x) / 5F) * 5F) / ((x + 200) * 0.06F) - beat * 0.52F);

            return (0.5F + Math.Sign(wave1) * wave1 * wave1 * wave2 * 0.6F) * channelPoints[offset].dampening;
        }

        protected void DropViewport(float startBeat, float endBeat, float beat)
        {
            if (beat < startBeat || beat > endBeat)
                return;

            float localBeat = beat - startBeat;
            float duration = endBeat - startBeat;
            float dropPercent = localBeat * localBeat / (duration * duration);
            SetViewport(dropPercent);
        }
    }
}
