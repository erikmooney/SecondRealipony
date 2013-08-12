using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using Microsoft.Xna.Framework.Graphics;
using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Media;

namespace SecondRealipony
{
    class World : SRSegment
    {
        public override float EndBeat { get { return 106; } }
        public override string MusicName { get { return "World.wav"; } }
        public override float BeatLength { get { return 60F / 125F; } }
        VideoPlayer videoPlayer;
        Video video;

        public World(Game game)
            : base(game)
        {
            video = game.Content.Load<Video>("World.wmv");
            videoPlayer = new VideoPlayer();
        }

        protected override void DrawSegment()
        {
            if (videoPlayer.State == MediaState.Stopped)
            {
                videoPlayer.Play(video);
            }

            var batch = new SpriteBatch(device);
            batch.Begin(SpriteSortMode.Immediate, BlendState.NonPremultiplied);
            batch.Draw(videoPlayer.GetTexture(), FullScreen, Color.White);
            batch.End();
        }
    }
}
