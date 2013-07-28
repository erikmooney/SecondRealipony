using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using Microsoft.Xna.Framework.Graphics;
using Microsoft.Xna.Framework;

namespace SecondRealipony
{
    class Credits : SRSegment
    {
        public override float EndBeat { get { return texts.Length * BEATS;  } }
        public override string MusicName { get { return "Credits.wav"; } }
        public override float BeatLength { get { return 60F / 125F; } }

        Texture2D credits;
        SpriteFont calibri;
        string[] texts;

        const float BEATS = 12;
        const float SCROLLERLENGTH = 32;

        public Credits(Game game)
            : base(game)
        {
            credits = game.Content.Load<Texture2D>("credits.png");
            calibri = game.Content.Load<SpriteFont>("calibri");
            texts = new string[] {
@"Ponies and landscape - vikingerik
Music - Skaven
Code - vikingerik
(All code by vikingerik)",
@"Objects - vikingerik
Music - Skaven",
@"Princess Luna - Sulyo
Text - vikingerik",
@"Twilight Sparkle - Sulyo
Spike - Demigod-Spike
Objects - vikingerik
Music - Purple Motion",
@"Rarity - vikingerik
Music - Purple Motion
Objects - vikingerik",
@"Vinyl Scratch - Ambassad0r
Music - Purple Motion",
@"Rainbow Dash - tamalesyatole
Music - Purple Motion",
@"Shining Armor - RegolithX
Background - vikingerik
Music - Purple Motion",
@"Fluttershy - Sulyo
Tree - vikingerik
Music - Purple Motion",
@"Applejack - KyssS90
Apple lens - vikingerik
Music - Purple Motion",
@"Cutie Mark Crusaders - NerdiRockstar
Music - Purple Motion",
@"Cutie marks - ParclyTaxel
Animation - vikingerik
Music - Purple Motion",
@"Pinkie Pie and cannon - MyLittlePinkieDash
Music - Purple Motion",
@"Derpy Hooves - TheJourneysEnd
Mail - vikingerik
Muffin - maxmontezuma
Music - Purple Motion",
@"Wonderbolts - D4SVader
Music - Purple Motion",
@"Princess Celestia - Fehlung
Music - Purple Motion",
@"Source Filmmaker - Valve
Original world - Trug
Map recreation - vikingerik
Music - Skaven",
@"Pony models -
Poninnahka and team
Chiramii-chan    Yukitoshii
gonzalolog     KP-ShadowSquirrel",
@"Code - vikingerik
Music - Skaven"
            };
        }

        protected override void DrawSegment()
        {
            DrawPicture();
            DrawText();
        }

        protected void DrawPicture()
        {
            int iCreditNum = GetCreditNum();
            int row = iCreditNum / 4;
            int col = iCreditNum % 4;

            var sourceRect = new Rectangle(col * credits.Width / 4, row * credits.Height / 5, credits.Width / 4, credits.Height / 5);
            var textureScale = (float)device.Viewport.Width / (credits.Width / 4) * 0.5F;
            
            //must cast this to int, or else for the pixel straddling the edge of the picture, spritebatch helpfully interpolates a value from the next image over on the sprite sheet
            var textureCenterX = (int)MathHelper.Lerp(device.Viewport.Width, -credits.Width * textureScale / 4, GetProgress(Beat));
            var position = new Vector2(textureCenterX, 0);

            var batch = new SpriteBatch(device);
            batch.Begin(SpriteSortMode.Immediate, BlendState.Opaque, SamplerState.PointClamp, DepthStencilState.None, null);
            batch.Draw(
                credits,
                position,
                sourceRect,
                Color.White,
                0,
                Vector2.Zero,
                textureScale,
                SpriteEffects.None,
                0);
            batch.End();
        }

        private int GetCreditNum()
        {
            return (int)(Beat / BEATS);
        }

        //Each credits element slides in for 20% of the time, stays in place for 60% of the time, leaves for 20% of the time
        //Calculate the progress between 0 and 1, suitable for lerping/smoothstepping
        protected float GetProgress(float beat)
        {
            beat = beat % BEATS;
            float offset = beat / BEATS;

            if (offset <= 0.2F)
                return 0.5F + DoubleSquareHalve((offset - 0.2F) * 2.5F);

            if (offset >= 0.8F)
                return 0.5F + DoubleSquareHalve((offset - 0.8F) * 2.5F);

            return 0.5F;
        }

        protected float DoubleSquareHalve(float input)
        {
            return Math.Sign(input) * input * input * 2;
        }

        protected float GetTextYProgress(float beat)
        {
            float p = GetProgress(beat);
            if (p <= 0.5F)
                return p * 2;
            else
                return (1 - p) * 2;
        }

        protected void DrawText()
        {
            DrawCredits();
        }

        private void DrawCredits()
        {
            int creditNum = GetCreditNum();
            var lines = texts[creditNum].Split('\n');

            var batch = new SpriteBatch(device);
            batch.Begin(SpriteSortMode.Immediate, BlendState.Opaque);

            for (int i = 0; i < lines.Length; i++)
            {
                Vector2 FontSize = calibri.MeasureString(lines[i]);
                Vector2 FontOrigin = new Vector2(FontSize.X / 2, 0);
                float textY = MathHelper.Lerp(device.Viewport.Height, device.Viewport.Height / 2, GetTextYProgress(Beat));
                Vector2 ScreenOrigin = new Vector2(
                    ScreenCenter.X,
                    textY + FontSize.Y * (i + 0.5F) * (device.Viewport.Width / 1280F)
                    );

                batch.DrawString(
                    calibri,
                    lines[i],
                    ScreenOrigin,
                    Color.White,
                    0,
                    FontOrigin,
                    ((float)device.Viewport.Width / 1280F),
                    SpriteEffects.None,
                    0);
            }
            batch.End();
        }

    }
}
