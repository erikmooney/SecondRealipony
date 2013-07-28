using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using Microsoft.Xna.Framework.Graphics;
using Microsoft.Xna.Framework;

namespace SecondRealipony
{
    class WorldStart : SRSegment
    {
        public override float EndBeat { get { return 8; } }
        public override string MusicName { get { return "WorldStart.wav"; } }
        public override float BeatLength { get { return 60F / 125F; } }

        Texture2D picture;

        public WorldStart(Game game)
            : base(game)
        {
            picture = game.Content.Load<Texture2D>("endsecondhalf.png");
        }

        protected override void DrawSegment()
        {
            DrawPicture();
            FadeScreen(0, 2, Beat, false, false);
            if (Beat >= 2)
                SmashScreen(1);
            DrawSidebars();
        }

        private void DrawPicture()
        {
            var batch = new SpriteBatch(device);
            batch.Begin(SpriteSortMode.Immediate, BlendState.Opaque, SamplerState.LinearClamp, DepthStencilState.None, null);

            //Draw version 6, single scale
            batch.Draw(
                picture,
                ScreenCenter,
                null,
                Color.White,
                0,
                picture.Center(),
                (float)device.Viewport.Height / picture.Height,
                SpriteEffects.None,
                0);
            batch.End();
        }

        private void DrawSidebars()
        {
            var opacity = MathHelper.Clamp((6 - Beat) / 2, 0, 1);

            Texture2D black = Get1x1Texture(new Color(0, 0, 0, opacity));
            var sidebarWidth = (int) (device.Viewport.Width - picture.Width * ((float)device.Viewport.Height / picture.Height)) / 2;

            var batch = new SpriteBatch(device);
            batch.Begin(SpriteSortMode.Immediate, BlendState.AlphaBlend, SamplerState.PointClamp, DepthStencilState.None, null);
            batch.Draw(black, new Rectangle(0, 0, sidebarWidth, device.Viewport.Height), Color.White);
            batch.Draw(black, new Rectangle(device.Viewport.Width - sidebarWidth, 0, sidebarWidth, device.Viewport.Height), Color.White);
            batch.End();
        }
    }
}
