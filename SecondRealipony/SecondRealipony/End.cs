using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using Microsoft.Xna.Framework.Graphics;
using Microsoft.Xna.Framework;

namespace SecondRealipony
{
    class End : SRSegment
    {
        public override float EndBeat { get { return 32; } }
        public override string MusicName { get { return "End.wav"; } }
        public override float BeatLength { get { return 60F / 125F; } }
        public override double MusicCue { get { return 592.533333; } }

        SpriteFont bookman;
        string finalscroller;

        public End(Game game)
            : base(game)
        {
            bookman = game.Content.Load<SpriteFont>("bookmanoldstyle");

            finalscroller =
@"Second Realipony
by Erik Mooney (vikingerik)

Published 30 July 2013 for the 20th
anniversary of Assembly '93 and Second Reality

http://www.dos486.com/secondrealipony

Thanks for watching!
Share this link everywhere!";
        }

        protected override void DrawSegment()
        {
            DrawScroller(Beat);
            FadeScreen(EndBeat / 2, EndBeat, Beat, true, false);
        }

        private void DrawScroller(float localBeat)
        {
            float progress = MathHelper.Clamp(localBeat / (EndBeat / 2), 0, 1);
            var lines = finalscroller.Split('\n');

            var batch = new SpriteBatch(device);
            batch.Begin(SpriteSortMode.Immediate, BlendState.Opaque);

            float fontScale = ((float)device.Viewport.Width / 1280F) * 0.75F;

            for (int i = 0; i < lines.Length; i++)
            {
                Vector2 FontSize = bookman.MeasureString(lines[i]);
                Vector2 FontOrigin = new Vector2(FontSize.X / 2, 0);
                float textY = MathHelper.Lerp(device.Viewport.Height, device.Viewport.Height / 2 - FontSize.Y * fontScale * lines.Length * 0.5F, progress);
                Vector2 ScreenOrigin = new Vector2(
                    ScreenCenter.X,
                    textY + FontSize.Y * fontScale * i
                    );

                batch.DrawString(
                    bookman,
                    lines[i],
                    ScreenOrigin,
                    Color.White,
                    0,
                    FontOrigin,
                    fontScale,
                    SpriteEffects.None,
                    0);
            }
            batch.End();
        }


    }
}
