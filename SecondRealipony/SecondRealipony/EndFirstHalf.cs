using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using Microsoft.Xna.Framework.Graphics;
using Microsoft.Xna.Framework;

namespace SecondRealipony
{
    class EndFirstHalf : SRSegment
    {
        Texture2D picture;
        public override float Anacrusis { get { return 2; } }
        public override float EndBeat { get { return 28; } }
        public override string MusicName { get { return "EndFirstHalf.wav"; } }
        public override double MusicCue { get { return 160.3333333; } }

        public EndFirstHalf(Game game) : base(game)
        {
            picture = game.Content.Load<Texture2D>("endfirsthalf.png");
        }

        protected override void DrawSegment()
        {
            //Collapse scale for each axis during picture turn-off
            Vector2 scale = new Vector2((float)device.Viewport.Height / picture.Height);
            const float TURNOFFBEAT = 16;
            if (Beat >= TURNOFFBEAT)
            {
                scale.Y *= MathHelper.Clamp((TURNOFFBEAT + 0.5F - Beat) * 2F, 3.0F / picture.Height, 1.0F);
                scale.X *= MathHelper.Clamp((TURNOFFBEAT + 2.5F - Beat) / 2F, 3.0F / picture.Width, 1.0F);
            }

            //Slide into place
            var x = ScreenCenter.X + (device.Viewport.Width + picture.Width * scale.X - ScreenCenter.X) * (-Beat / 2);
            x = Math.Max(x, ScreenCenter.X);
            //Shimmy into place
            if (Beat > -1 / 6F)
                x -= (float)Math.Cos(Math.PI * Beat * 3) * Math.Max(0, (1.5F - Beat) * 40);

            var position = new Vector2(x, ScreenCenter.Y);

            var batch = new SpriteBatch(device);
            batch.Begin();
            //Draw version 7, Vector2 scale
            batch.Draw(picture, position, null, Color.White, 0, picture.Center(), scale, SpriteEffects.None, 0);
            batch.End();

            //Smash screen to white then fade
            FadeScreen(0, 1, Beat, false, true);

            //Fade screen
            FadeScreen(TURNOFFBEAT + 4, TURNOFFBEAT + 8, Beat, true, false);
            if (Beat >= TURNOFFBEAT + 8)
                FadeScreen(1);
        }
    }
}
