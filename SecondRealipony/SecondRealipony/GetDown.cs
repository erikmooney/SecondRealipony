using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using Microsoft.Xna.Framework.Graphics;
using Microsoft.Xna.Framework;

namespace SecondRealipony
{
    class GetDown : SRSegment
    {
        const int PANELS = 4;
        int[] panelY = new int[PANELS];
        //Texture2D vinyl;
        Texture2D paneltexture;

        public override float EndBeat { get { return 4; } }
        public override string MusicName { get { return "getdown.wav"; } }

        public GetDown(Game game)
            : base(game)
        {
            //vinyl = game.Content.Load<Texture2D>("vinyl.png");

            paneltexture = Get1x1Texture(new Color(0.6F, 0.6F, 0.7F));
        }

        protected override void DrawSegment()
        {

            foreach (int i in Enumerable.Range(0, PANELS))
            {
                if (i <= Beat)
                    panelY[i] = (int)(Math.Pow((Beat - i), 2) * device.Viewport.Height);
            }

            float beatOffset = Beat - (float) Math.Floor(Beat);

            var batch = new SpriteBatch(device);
            batch.Begin();
            foreach (int i in Enumerable.Range(0, PANELS))
            {
                batch.Draw(paneltexture, new Rectangle(i * device.Viewport.Width / PANELS, panelY[i], device.Viewport.Width / PANELS, device.Viewport.Height), Color.White);
            }
            batch.End();

            if (beatOffset < 0.5F)
                SmashScreen((0.5F - beatOffset) * 2);

        }
    }
}
