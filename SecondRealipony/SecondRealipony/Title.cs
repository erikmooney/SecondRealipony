using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using Microsoft.Xna.Framework.Graphics;
using Microsoft.Xna.Framework;

namespace SecondRealipony
{
    class Title : SRSegment
    {
        Texture2D title;

        public override float Anacrusis { get { return 4; } }
        public override float EndBeat { get { return 16; } }
        public override string MusicName { get { return "title.wav"; } }
        public override float MusicDelay { get { return 4; } }

        public Title(Game game)
            : base(game)
        {
            title = game.Content.Load<Texture2D>("title.png");

        }

        protected override void DrawSegment()
        {
            DrawTitle();
            FadeScreen(-4, 0, Beat, false, true);
            FadeGray();
            DrawBars();
        }

        private void DrawTitle()
        {
            var batch = new SpriteBatch(device);
            batch.Begin();
            batch.Draw(title, FullScreen, Color.White);
            batch.End();
        }

        private void FadeGray()
        {
            var percent = (Beat - EndBeat + 2) / 2F;

            Texture2D graytexture = Get1x1Texture(new Color(0.5F, 0.5F, 0.5F, percent));

            //NonPremultiplied blend forces the resulting color to converge to the texture value as alpha approaches 1
            var batch = new SpriteBatch(device);
            batch.Begin(SpriteSortMode.Immediate, BlendState.NonPremultiplied);
            batch.Draw(graytexture, FullScreen, Color.White);
            batch.End();
        }

        private void DrawBars()
        {
            if (Beat < EndBeat - 2)
                return;

            var percent = (Beat - EndBeat + 2) / 2F;

            Texture2D blacktexture = Get1x1Texture(Color.Black);

            //Wipe in bounding bars
            var topRectangle = new Rectangle(0, 0, device.Viewport.Width, (int)(ScreenCenter.Y * percent));
            var bottomLineY = (int)MathHelper.Lerp(device.Viewport.Height, ScreenCenter.Y, percent);
            var bottomRectangle = new Rectangle(0, bottomLineY, device.Viewport.Width, device.Viewport.Height - bottomLineY);

            var batch = new SpriteBatch(device);
            batch.Begin(SpriteSortMode.Immediate, BlendState.Opaque);
            batch.Draw(blacktexture, topRectangle, Color.White);
            batch.Draw(blacktexture, bottomRectangle, Color.White);
            batch.End();
        }
    }
}
