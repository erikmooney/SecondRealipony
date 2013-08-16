using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using Microsoft.Xna.Framework.Graphics;
using Microsoft.Xna.Framework;

namespace SecondRealipony
{
    class Intro : SRSegment
    {
        Texture2D landscape;
        Texture2D dash;
        Texture2D spitfire;
        Texture2D soarin;
        Texture2D mistyfly;

        SpriteFont bookman;
        IntroText[] texts;

        BasicEffect basicEffect;
        VertexPositionTexture[] quadVertices;
        VertexBuffer quadVertexBuffer;
        VertexBuffer rainbowTrailVertexBuffer;
        VertexBuffer[] rainboomVertexBuffers;
        VertexBuffer discVertexBuffer;
        VertexBuffer radialVertexBuffer;

        Matrix tilt;
        Matrix boomCenter;

        public override float BeatLength { get { return 60F / 125F; } }
        public override float EndBeat { get { return 137-32; } }
        public override string MusicName { get { return "intro.wav"; } }
        public override double MusicCue { get { return 0; } }

        public Intro(Game game)
            : base(game)
        {
            basicEffect = new BasicEffect(device);

            landscape = game.Content.Load<Texture2D>("intro.png");
            dash = game.Content.Load<Texture2D>("intro dash.png");
            spitfire = game.Content.Load<Texture2D>("wonderbolt1.png");
            soarin = game.Content.Load<Texture2D>("wonderbolt2.png");
            mistyfly = game.Content.Load<Texture2D>("wonderbolt3.png");

            //Keep two \ns at the end of each string, so that the text will be centered above the screen middle - so lower lines don't overlap the landscape
            bookman = game.Content.Load<SpriteFont>("bookmanoldstyle");
            texts = new IntroText[] {
                new IntroText(1, 15,
@"This is a fan work for
non-commercial purposes
and free distribution.

My Little Pony: Friendship is Magic
and all its characters
were created by Lauren Faust
and are owned and copyrighted by
Hasbro and DHX Media.

"),
                new IntroText(17, 31,
@"Original music from ""Second Reality""
composed and owned by
Purple Motion, Skaven

Original design of ""Second Reality"" by
Psi, Trug, Wildfire, Abyss, Gore, Marvel, Pixel

of Future Crew.

"),
                new IntroText(33, 47,
@"Artwork elements by:

Sulyo|Demigod-Spike
ambassad0r|tamalesyatole
RegolithX|KyssS90
NerdiRockstar|ParclyTaxel
mylittlepinkiedash|TheJourneysEnd
maxmontezuma|D4SVader
fehlung|Poninnahka
vikingerik|
"),
                new IntroText(49, 63, "Created and programmed by vikingerik\n\n")
            };

            CreateGeometry();
        }

        private void CreateGeometry()
        {
            CreatePonyQuad();
            CreateRainbowTrail();
            CreateRaindrops();
            CreateDisc();
            CreateRadial();
            tilt = Matrix.CreateRotationX((float)Math.PI / 12);
            boomCenter = Matrix.CreateTranslation(new Vector3(0, 2, -84));
        }

        private void CreatePonyQuad()
        {
            quadVertices = CreateQuad();

            quadVertexBuffer = new VertexBuffer(device, typeof(VertexPositionTexture), quadVertices.Length, BufferUsage.WriteOnly);
            quadVertexBuffer.SetData(quadVertices);
        }
        
        private void CreateRainbowTrail()
        {
            var alpha = 0.95F;
            var maxZ = 80F;
            var noColor = new Color(0, 0, 0, 0);
            var rainbowTrailVertices = new VertexPositionColor[] {
                new VertexPositionColor(new Vector3(0, 0, 0), new Color(1.0F, 0.25F, 0.25F, alpha)),
                new VertexPositionColor(new Vector3(-2, 0, maxZ), noColor),
                new VertexPositionColor(new Vector3(-3, 0, maxZ), noColor),
                new VertexPositionColor(new Vector3(0, 0, 0), new Color(1.0F, 0.5F, 0.25F, alpha)),
                new VertexPositionColor(new Vector3(-1, 0, maxZ), noColor),
                new VertexPositionColor(new Vector3(-2, 0, maxZ), noColor),
                new VertexPositionColor(new Vector3(0, 0, 0), new Color(0.96F, 0.96F, 0, alpha)),
                new VertexPositionColor(new Vector3(0, 0, maxZ), noColor),
                new VertexPositionColor(new Vector3(-1, 0, maxZ), noColor),
                new VertexPositionColor(new Vector3(0, 0, 0), new Color(0.25F, 1.0F, 0.5F, alpha)),
                new VertexPositionColor(new Vector3(1, 0, maxZ), noColor),
                new VertexPositionColor(new Vector3(0, 0, maxZ), noColor),
                new VertexPositionColor(new Vector3(0, 0, 0), new Color(0, 0.375F, 1.0F, alpha)),
                new VertexPositionColor(new Vector3(2, 0, maxZ), noColor),
                new VertexPositionColor(new Vector3(1, 0, maxZ), noColor),
                new VertexPositionColor(new Vector3(0, 0, 0), new Color(0.375F, 0, 0.625F, alpha)),
                new VertexPositionColor(new Vector3(3, 0, maxZ), noColor),
                new VertexPositionColor(new Vector3(2, 0, maxZ), noColor),
            };

            rainbowTrailVertexBuffer = new VertexBuffer(device, typeof(VertexPositionColor), rainbowTrailVertices.Length, BufferUsage.WriteOnly);
            rainbowTrailVertexBuffer.SetData(rainbowTrailVertices);
        }

        private void CreateRaindrops()
        {
            //Create one outer contour line for the rainboom teardrop model
            const float segments = 8;
            var contour =
                    Enumerable.Range(0, (int)segments + 1).Select(
                    i => new VertexPositionColor(
                        new Vector3(
                            0,
                            (float)Math.Sin(i * MathHelper.PiOver2 / segments),
                            (float)Math.Cos(i * MathHelper.PiOver2 / segments)
                            ),
                        new Color(0, 0, 0, MathHelper.Lerp(0.6F, 0.4F, i / segments)) //color part will be overwritten by conversion from HSL, but alpha is retained
                        )
                    )
                //Backmost vertex
                .Concat(
                    new[] { new VertexPositionColor(new Vector3(0, 0.75F, -10), new Color(0, 0, 0, 0.0F)) }
                )
                .ToArray();

            const int raindrops = 180;
            rainboomVertexBuffers = new VertexBuffer[raindrops];
            for (int i = 0; i < raindrops; i++)
            {
                //Add colors to the contour, defined by HSL.  hueOffset causes something other than pure red to point at the camera
                var hueOffset = 225;
                var contourColored = contour.Select(v =>
                    new VertexPositionColor(v.Position, HSLtoRGB(new ColorHSL(((i * 720F / raindrops) + hueOffset) % 360, 1.0F, v.Color.A / 255F, v.Color.A)))
                    ).ToArray();

                //Duplicate it to a second line, in order to have a triangle strip between the two contours
                const float strips = 20F;
                var contourStrip = (from v in contourColored
                                    from j in Enumerable.Range(0, 2)
                                    select new VertexPositionColor(Vector3.Transform(v.Position, Matrix.CreateRotationZ(j * MathHelper.TwoPi / strips)), v.Color)
                                   ).ToArray();

                //Duplicate first and last vertices to create discontinuity for TriangleStrip purposes
                contourStrip = contourStrip.Take(1).Concat(contourStrip).Concat(contourStrip.Skip(contourStrip.Length - 1).Take(1)).ToArray();

                //Duplicate the strip rotating around Z to form the whole model
                var vertices = (from j in Enumerable.Range(0, (int)strips)
                                from v in contourStrip
                                select new VertexPositionColor(Vector3.Transform(v.Position, Matrix.CreateRotationZ(j * MathHelper.TwoPi / strips)), v.Color)
                               ).ToArray();

                rainboomVertexBuffers[i] = new VertexBuffer(device, typeof(VertexPositionColor), vertices.Length, BufferUsage.WriteOnly);
                rainboomVertexBuffers[i].SetData(vertices);
            }
        }

        protected void CreateDisc()
        {
            var discVertices = Enumerable.Range(0, 181)
                .SelectMany(i => new[] {
                    new VertexPositionColor(new Vector3(0, 0, 0), Color.LightGray),
                    new VertexPositionColor(new Vector3((float)Math.Sin(MathHelper.ToRadians(i * 2)), 0, (float)Math.Cos(MathHelper.ToRadians(i * 2))), new Color(0.0F, 0.0F, 0.0F, 1.0F))
                }).ToArray();

            discVertexBuffer = new VertexBuffer(device, typeof(VertexPositionColor), discVertices.Length, BufferUsage.None);
            discVertexBuffer.SetData(discVertices);
        }

        protected void CreateRadial()
        {
            var radialVertices = new[] {
                new VertexPositionColor(new Vector3(0F, 0F, 0.05F), Color.Gray),
                new VertexPositionColor(new Vector3(-0.005F, 0F, 1F), Color.Gray),
                new VertexPositionColor(new Vector3(0.005F, 0F, 1F), Color.Gray)
            };

            radialVertexBuffer = new VertexBuffer(device, typeof(VertexPositionColor), radialVertices.Length, BufferUsage.WriteOnly);
            radialVertexBuffer.SetData(radialVertices);
        }

        protected override void DrawSegment()
        {
            DrawLandscape();
            DrawTexts();
            SetBasicEffect();
            DrawPonies();
            DrawRainboom();

            //Explosion smash
            FadeScreen(EndBeat - 9, EndBeat - 8, Beat, false, true);
            
            //Fade to white
            FadeScreen(EndBeat - 4, EndBeat - 1, Beat, false, false);
            if (Beat >= EndBeat - 1)
                SmashScreen(1);
        }

        private void DrawLandscape()
        {
            if (Beat < 8)
                return;

            //Scroll from beats 8 to 72
            var scrollPercent = Math.Min((Beat - 8) / 64, 1);
            var sourceRectangle = new Rectangle((int)(landscape.Width / 2 * scrollPercent), 0, landscape.Width / 2, landscape.Height);

            var batch = new SpriteBatch(device);
            batch.Begin();
            batch.Draw(landscape, FullScreen, sourceRectangle, Color.White);
            batch.End();

            FadeScreen(8, 24, Beat, true, true);
        }

        private void DrawTexts()
        {
            var batch = new SpriteBatch(device);
            batch.Begin(SpriteSortMode.Immediate, BlendState.NonPremultiplied);

            foreach (var text in texts)
            {
                if (Beat >= text.StartBeat && Beat <= text.EndBeat)
                {
                    //calculate opacity
                    float fade = Math.Min(1.0F, Beat - text.StartBeat);
                    fade = Math.Min(fade, text.EndBeat - Beat);
                    var color = new Color(1, 1, 1, fade);

                    var lines = text.Text.Split('\n');

                    var scale = device.Viewport.Width / 1920F;

                    for (int i = 0; i < lines.Length; i++)
                    {
                        var segments = lines[i].Split('|');
                        for (int j = 0; j < segments.Length; j++)
                        {
                            Vector2 FontSize = bookman.MeasureString(segments[j]);

                            float centerXPercent = (float) (j + 0.5F) / segments.Length;

                            float columnCenterX = centerXPercent * device.Viewport.Width;
                            float lineCenterY = ScreenCenter.Y - FontSize.Y * ((lines.Length / 2F) - i) * scale;

                            Vector2 ScreenOrigin = new Vector2(columnCenterX, lineCenterY);
                            batch.DrawString(bookman, segments[j], ScreenOrigin, color, 0, new Vector2(FontSize.X / 2, 0), scale, SpriteEffects.None, 0);
                        }
                    }
                }
            }
            batch.End();
        }

        private void SetBasicEffect()
        {
            basicEffect.LightingEnabled = false;
            basicEffect.View = Matrix.CreateLookAt(new Vector3(0, 0, 4), Vector3.Zero, Vector3.Up);

            //Projection matrix is off-center so the vanishing point will be north of screen-center.  So the plane of the rainboom can fill more of the screen.
            basicEffect.Projection = Matrix.CreatePerspectiveOffCenter(
                -AspectRatio * 0.2F,
                AspectRatio * 0.2F,
                -0.25F,
                0.15F,
                0.5F,
                200F);
        }

        //Dash enters from center (4.5 beats before first Wonderbolt)
        //Misty Fly enters from left (first)
        //Spitfire enters from right (second)
        //Soarin enters from center (last)
        private void DrawPonies()
        {
            device.BlendState = BlendState.AlphaBlend;
            device.DepthStencilState = DepthStencilState.Default;
            device.SamplerStates[0] = SamplerState.PointClamp;
            SetCullMode(CullMode.CullClockwiseFace);

            float ponyStartBeat = 80;

            if (Beat < ponyStartBeat || Beat >= ponyStartBeat + 16)
                return;

            //During beats -2 to 0 before explosion, scale each model down to 0 size - makes it disappear into a point (Second Reality fudges this similarly)
            Matrix scaleMatrix = Matrix.CreateScale(MathHelper.Clamp((96 - Beat) / 2, 0, 1));
            
            float localBeat;
            Vector3 center;
            Matrix matrix;

            localBeat = Beat - ponyStartBeat;
            center = new Vector3(0F, 2F, -localBeat * 6);
            matrix = scaleMatrix * Matrix.CreateTranslation(center);
            DrawQuad(dash, quadVertexBuffer, matrix);

            center = new Vector3(0F, 1.9F, -localBeat * 6 - 0.5F);
            matrix = scaleMatrix * Matrix.CreateTranslation(center);
            DrawRainbowTrail(matrix);

            //Wonderbolts start slightly below Dash in the Y coordinate, slowly catch up according to localbeat
            //Wonderbolts go faster than Dash (localBeat * 7 instead of 6) in order to catch up
            localBeat = Beat - (ponyStartBeat + 4.5F);
            center = new Vector3(-1.5F, 0.75F + localBeat / 10, -localBeat * 7);
            matrix = scaleMatrix * Matrix.CreateRotationZ((-localBeat + 4) / 12) * Matrix.CreateTranslation(center);
            DrawQuad(mistyfly, quadVertexBuffer, matrix);

            localBeat = Beat - (ponyStartBeat + 5F);
            center = new Vector3(1F, 0.5F + localBeat / 10, -localBeat * 7);
            matrix = scaleMatrix * Matrix.CreateRotationZ((localBeat - 4) / 8) * Matrix.CreateTranslation(center);
            DrawQuad(spitfire, quadVertexBuffer, matrix);

            localBeat = Beat - (ponyStartBeat + 5.5F);
            center = new Vector3(-0.25F, 1F + localBeat / 10, -localBeat * 7);
            matrix = scaleMatrix * Matrix.CreateRotationZ((-localBeat + 2) / 16) * Matrix.CreateTranslation(center);
            DrawQuad(soarin, quadVertexBuffer, matrix);
        }
        
        private void DrawQuad(Texture2D texture, VertexBuffer vertexBuffer, Matrix worldMatrix)
        {
            basicEffect.TextureEnabled = true;
            basicEffect.VertexColorEnabled = false;

            foreach (EffectPass pass in basicEffect.CurrentTechnique.Passes)
            {
                device.SetVertexBuffer(vertexBuffer);
                basicEffect.Texture = texture;
                basicEffect.World = worldMatrix;
                pass.Apply();

                device.DrawPrimitives(PrimitiveType.TriangleStrip, 0, 2);
            }
        }

        private void DrawRainbowTrail(Matrix worldMatrix)
        {
            basicEffect.TextureEnabled = false;
            basicEffect.VertexColorEnabled = true;
            basicEffect.World = worldMatrix;

            device.SetVertexBuffer(rainbowTrailVertexBuffer);

            foreach (EffectPass pass in basicEffect.CurrentTechnique.Passes)
            {
                pass.Apply();

                device.DrawPrimitives(PrimitiveType.TriangleList, 0, rainbowTrailVertexBuffer.VertexCount / 3);
            }
        }

        private void DrawRainboom()
        {
            if (Beat < 96)
                return;
            var localBeat = Beat - 96;

            DrawDisc(localBeat);
            DrawRadial(localBeat);
            DrawRaindrops(localBeat);
        }

        private void DrawDisc(float localBeat)
        {

            SetCullMode(CullMode.CullClockwiseFace);
            device.DepthStencilState = DepthStencilState.None;
            device.BlendState = BlendState.Additive;
            basicEffect.VertexColorEnabled = true;
            basicEffect.TextureEnabled = false;
            device.SetVertexBuffer(discVertexBuffer);

            foreach (EffectPass pass in basicEffect.CurrentTechnique.Passes)
            {
                basicEffect.World = Matrix.CreateScale(localBeat * 11) * tilt * boomCenter;
                pass.Apply();
                device.DrawPrimitives(PrimitiveType.TriangleStrip, 0, discVertexBuffer.VertexCount - 2);
            }
        }

        private void DrawRadial(float localBeat)
        {
            SetCullMode(CullMode.None);
            device.DepthStencilState = DepthStencilState.None;
            device.BlendState = BlendState.Additive;
            basicEffect.VertexColorEnabled = true;
            basicEffect.TextureEnabled = false;
            device.SetVertexBuffer(radialVertexBuffer);

            foreach (EffectPass pass in basicEffect.CurrentTechnique.Passes)
            {
                for (int i = 0; i < 16; i++)
                {
                    float zscale = 1F;
                    if (i % 4 == 0) zscale = zscale * 0.5F;
                    if (i % 2 == 0) zscale = zscale * 0.5F;

                    basicEffect.World = Matrix.CreateScale(1F, 1F, zscale) *  Matrix.CreateScale(localBeat * 11) * Matrix.CreateRotationY(0.3F + i / 16F * MathHelper.TwoPi) * tilt * boomCenter;
                    pass.Apply();
                    device.DrawPrimitives(PrimitiveType.TriangleStrip, 0, radialVertexBuffer.VertexCount - 2);
                }
            }
        }

        private void DrawRaindrops(float localBeat)
        {
            SetCullMode(CullMode.CullClockwiseFace);
            device.DepthStencilState = DepthStencilState.None;
            device.BlendState = BlendState.Additive;
            basicEffect.VertexColorEnabled = true;
            basicEffect.TextureEnabled = false;

            var number = rainboomVertexBuffers.Length;

            for (int i = 0; i < number; i++)
            {
                device.SetVertexBuffer(rainboomVertexBuffers[i]);

                //This scale ensures that raindrops _just_ touch
                //expansion * 2pi is the circumference.  expansion * 2pi / number is the width available for each drop, and the model is 2 units wide

                Matrix scale = Matrix.CreateScale(localBeat * 12 * MathHelper.TwoPi / number / 2);
                Matrix expansion = Matrix.CreateTranslation(new Vector3(0, 0, localBeat * 12));
                Matrix theta = Matrix.CreateRotationY(MathHelper.ToRadians(i * 2));
                DrawRaindropRing(scale, expansion, theta, tilt, boomCenter);

                scale = Matrix.CreateScale(localBeat * 9 * MathHelper.TwoPi / number / 2);
                expansion = Matrix.CreateTranslation(new Vector3(0, 0, localBeat * 9));
                DrawRaindropRing(scale, expansion, theta, tilt, boomCenter);

                scale = Matrix.CreateScale(localBeat * 6 * MathHelper.TwoPi / number / 2);
                expansion = Matrix.CreateTranslation(new Vector3(0, 0, localBeat * 6));
                DrawRaindropRing(scale, expansion, theta, tilt, boomCenter);
            }
        }

        private void DrawRaindropRing(Matrix scale, Matrix expansion, Matrix theta, Matrix tilt, Matrix center)
        {
            foreach (EffectPass pass in basicEffect.CurrentTechnique.Passes)
            {
                basicEffect.World = scale * expansion * theta * tilt * center;

                pass.Apply();
                device.DrawPrimitives(PrimitiveType.TriangleStrip, 0, rainboomVertexBuffers[0].VertexCount - 2);
            }
        }
    }

    class IntroText
    {
        public float StartBeat;
        public float EndBeat;
        public string Text;

        public IntroText(float startbeat, float endbeat, string text)
        {
            StartBeat = startbeat;
            EndBeat = endbeat;
            Text = text;
        }
    }
}
