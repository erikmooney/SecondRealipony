using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading;
using Microsoft.Xna.Framework.Graphics;
using Microsoft.Xna.Framework;

namespace SecondRealipony
{
    class Vinyl : SRSegment
    {
        public override float EndBeat { get { return 32; } }
        public override string MusicName { get { return "vinyl.wav"; } }

        Texture2D[] Circles;
        Texture2D contours;
        Texture2D vinyl;
        const int STEPS = 8;

        public Vinyl(Game game)
            : base(game)
        {
            //Create the textures asynchronously!  Because this takes a while
            StartupThread = new Thread(new ThreadStart(CreateTextures));
            StartupThread.Priority = ThreadPriority.Lowest;
            StartupThread.Start();

            contours = game.Content.Load<Texture2D>("vinyl contours.png");
            vinyl = game.Content.Load<Texture2D>("vinyl head.png");
        }

        private void CreateTextures()
        {
            var texelints = CalculateTexels(device.Viewport.Width * 2, device.Viewport.Height * 2, STEPS, 80 / STEPS);
            Circles = new Texture2D[STEPS];

            for (int i = 0; i < Circles.Length; i++)
            {
                var texels = texelints.Select(c =>
                    new Color(
                        (c + i) % STEPS / (float)(STEPS - 1),
                        (c + i) % STEPS / (float)(STEPS - 1) + 0.05F,    //slight green shade to the circles, will make the inversion slightly purple
                        (c + i) % STEPS / (float)(STEPS - 1),
                        1
                    )
                ).ToArray();
                Circles[i] = new Texture2D(device, device.Viewport.Width * 2, device.Viewport.Height * 2);
                Circles[i].SetData(texels);
            }
        }

        private int[] CalculateTexels(int width, int height, int steps, float thickness)
        {
            var texels = new int[width * height];
            Vector2 center = new Vector2(width / 2, height / 2);
            for (int i = 0; i < height; i++)
                for (int j = 0; j < width; j++)
                {
                    Vector2 point = new Vector2(j, i);
                    float distance = Vector2.Distance(center, point);
                    distance = Math.Max(0, distance - thickness * 4);    //make center circle big
                    int result = (int) (distance % (steps * thickness) / thickness);
                    texels[i * width + j] = steps - result - 1;
                }

            return texels;
        }

        protected override void DrawSegment()
        {
            DrawCircles();
            DrawVinyl();
            DrawContours();
        }

        private void DrawCircles()
        {
            int circleNum = (int)((Beat * 15) % STEPS);

            var CircleCenter = ScreenCenter + GetCircleCenter(Beat); 

            var batch = new SpriteBatch(device);
            batch.Begin();
            batch.Draw(Circles[circleNum], CircleCenter, null, Color.White, 0, Circles[circleNum].Center(), 1, SpriteEffects.None, 0);
            batch.End();
        }

        private void DrawContours()
        {
            //Custom blend state.  Any pixel defined by these contours will invert the destination color that's already in the frame buffer.
            var InvertBlendState = new BlendState();
            InvertBlendState.ColorDestinationBlend = Blend.InverseSourceAlpha;
            InvertBlendState.ColorSourceBlend = Blend.InverseDestinationColor;
            InvertBlendState.AlphaDestinationBlend = Blend.One;                 //ALPHA IN THE FRAME BUFFER MUST REMAIN 1.0.  AT 0, IT WILL NOT SHOW UP ON SCREENSHOTS!

            var ContourCenter = ScreenCenter + GetNoteCenter(Beat);

            var skewBeat = (Beat < 16) ? 0 : GetSkewBeat((Beat - 16), 2);
            var rotation = 0.5F * (float)-Math.Sin(skewBeat * MathHelper.PiOver2);
            var scale = 1.4F - 0.4F * (float)Math.Cos(skewBeat * Math.PI);
            scale = scale * device.Viewport.Height / 720;

            var batch = new SpriteBatch(device);
            batch.Begin(SpriteSortMode.Immediate, InvertBlendState);
            batch.Draw(contours, ContourCenter, null, Color.White, rotation, contours.Center(), scale, SpriteEffects.None, 0);
            batch.End();
        }

        private Vector2 GetCircleCenter(float beat)
        {
            float centerX = (float)Math.Cos((4 - Beat) * MathHelper.PiOver4) * device.Viewport.Width / 3;
            float centerY = (float)Math.Sin((4 - Beat) * MathHelper.PiOver4) * device.Viewport.Height / 3;
            return new Vector2(centerX, centerY);
        }

        private Vector2 GetNoteCenter(float beat)
        {
            var centerX = (float)Math.Cos(-Beat * MathHelper.PiOver4 * 5F / 4F) * device.Viewport.Width / 3;
            var centerY = (float)Math.Sin(-Beat * MathHelper.PiOver4 * 5F / 4F) * device.Viewport.Height / 3;
            return new Vector2(centerX, centerY);
        }

        private void DrawVinyl()
        {
            var VinylCenter = ScreenCenter + GetCircleCenter(Beat);

            var batch = new SpriteBatch(device);
            batch.Begin(SpriteSortMode.Immediate, BlendState.AlphaBlend);
            batch.Draw(vinyl, VinylCenter, null, Color.White, 0, vinyl.Center(), 1.0F, SpriteEffects.None, 0);
            batch.End();
        }
    }
}
