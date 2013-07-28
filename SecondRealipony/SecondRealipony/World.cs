using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using Microsoft.Xna.Framework.Graphics;
using Microsoft.Xna.Framework;

namespace SecondRealipony
{
    class World : SRSegment
    {
        public override float EndBeat { get { return 106; } }
        public override string MusicName { get { return "World.wav"; } }
        public override float BeatLength { get { return 60F / 125F; } }

        Texture2D placeholder;

        public World(Game game)
            : base(game)
        {
            placeholder = game.Content.Load<Texture2D>("placeholder.jpg");
        }

        protected override void DrawSegment()
        {
            var batch = new SpriteBatch(device);
            batch.Begin(SpriteSortMode.Immediate, BlendState.NonPremultiplied);
            batch.Draw(placeholder, ScreenCenter, null, Color.White, 0, placeholder.Center(), 0.5F, SpriteEffects.None, 0);
            batch.End();
        }
    }
}
