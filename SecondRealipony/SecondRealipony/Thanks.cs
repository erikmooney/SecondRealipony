using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using Microsoft.Xna.Framework.Graphics;
using Microsoft.Xna.Framework;

namespace SecondRealipony
{
    class Thanks : SRSegment
    {
        public override float EndBeat { get { return 16; } }
        public override string MusicName { get { return "Thanks.wav"; } }
        public override float BeatLength { get { return 60F / 125F; } }
        public override double MusicCue { get { return 475.4; } }

        Texture2D thanks;

        public Thanks(Game game)
            : base(game)
        {
            thanks = game.Content.Load<Texture2D>("thanks.png");
        }

        protected override void DrawSegment()
        {
            var batch = new SpriteBatch(device);
            batch.Begin();
            batch.Draw(thanks, FullScreen, Color.White);
            batch.End();

            FadeScreen(0, 1, Beat, false, true);
            FadeScreen(15, 16, Beat, true, false);
        }
    }
}
