using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using Microsoft.Xna.Framework.Graphics;
using Microsoft.Xna.Framework;

namespace SecondRealipony
{
    class Rainbow : SRSegment
    {
        Texture2D dash;
        Texture2D RainbowTextureFlat;
        Texture2D RainbowBandTexture;
        Phase[] Phases;
        BlendState RainbowBlendState;

        public override float EndBeat { get { return 62; } }
        public override string MusicName { get { return "rainbow.wav"; } }

        public Rainbow(Game game)
            : base(game)
        {
            dash = game.Content.Load<Texture2D>("dash.png");

            var RainbowTexels = new Vector3[] {
                new Vector3(1.0F, 0.25F, 0.25F),
                new Vector3(1.0F, 0.5F, 0.25F),
                new Vector3(0.96F, 0.96F, 0),
                new Vector3(0.25F, 1.0F, 0.5F),
                new Vector3(0, 0.375F, 1.0F),
                new Vector3(0.375F, 0, 0.625F)
            };

            RainbowTextureFlat = new Texture2D(device, 1, RainbowTexels.Length);
            RainbowTextureFlat.SetData(RainbowTexels.Select(t => new Color(new Vector4(t, 1.0F))).ToArray());

            //Rainbow bands are made up of the original texels, at half intensity
            var RainbowBandTexels = RainbowTexels.Select(t => new Vector4(t * 0.5F, 0F)).ToArray();

            //Add gap equal to the rainbow width
            RainbowBandTexels = RainbowBandTexels.Concat(Enumerable.Repeat(Vector4.Zero, RainbowBandTexels.Length)).ToArray();

            //Multiply it to 11 copies
            RainbowBandTexels = Enumerable.Repeat(RainbowBandTexels, 11).SelectMany(c => c).ToArray();

            //Remove the last gap
            RainbowBandTexels = RainbowBandTexels.Take(RainbowBandTexels.Length - RainbowTexels.Length).ToArray();

            RainbowBandTexture = new Texture2D(device, 1, RainbowBandTexels.Length);
            RainbowBandTexture.SetData(RainbowBandTexels.Select(t => new Color(t)).ToArray());

            Phases = new Phase[] {
                //new Phase(0, 2, DoPhase0),
                new Phase(0, 2, DoPhase1),
                new Phase(2, 17, DoPhase2),
                new Phase(15, 17, DoPhase3),
                new Phase(17, 40, DoPhase4),
                new Phase(38, 40, DoPhase1),
                new Phase(40, 62, DoPhase5),
                new Phase(60, 62, DoPhase6),
            };

            RainbowBlendState = GetRainbowBlendState();
        }

        private static BlendState GetRainbowBlendState()
        {
            //Custom blend state.
            //Consider the "distance" between the existing destination color and white.  meaning (1 - dest color)
            //The source color defines how much of that distance to "go".
            //Example, if dest is 0.2 and source is 0.75, final color is 0.2 + (0.75 * (1 - 0.2)) = 0.8
            var blendState = new BlendState();
            blendState.ColorBlendFunction = BlendFunction.Add;
            blendState.ColorDestinationBlend = Blend.One;
            blendState.ColorSourceBlend = Blend.InverseDestinationColor;
            blendState.AlphaDestinationBlend = Blend.One;           //ALPHA IN THE FRAME BUFFER MUST REMAIN 1.0.  AT 0, IT WILL NOT SHOW UP ON SCREENSHOTS!
            return blendState;
        }

        //Calculated properties from device
        public float RainbowScale
        {
            get
            {
                return device.Viewport.Height / 36;
            }
        }

        public float DashScale
        {
            get
            {
                return (float)device.Viewport.Width / dash.Width * 0.4F;
            }
        }

        protected override void DrawSegment()
        {
            //Handle phases
            foreach (Phase phase in Phases)
            {
                if (Beat >= phase.StartBeat && Beat < phase.EndBeat)
                {
                    phase.PhaseFunction(Beat - phase.StartBeat);
                }
            }
        }

        //PHASE 0: Just draw Vinyl
        /*
        private void DoPhase0(float beat)
        {
            var PhasePercent = beat / 2F;

            //Fade Vinyl
            var batch = new SpriteBatch(device);
            batch.Begin();
            float luminosity = (float)Math.Max(0, 1 - PhasePercent * 1.25);
            batch.Draw(vinyl, new Rectangle(0, 0, device.Viewport.Width, device.Viewport.Height), new Color(new Vector3(luminosity)));
            batch.End();
        }
         */
        
        
        //Phase 1: Dash flies in drawing rainbows, while fading out whatever's there already.
        //This phase is used twice, to crossfade from phase 0 to 1 and from phase 3 to 4
        private void DoPhase1(float beat)
        {
            var PhasePercent = beat / 2F;
            FadeScreen(PhasePercent);

            int RainbowThickness = (int)(RainbowTextureFlat.Height * RainbowScale);
            var DashOrigin = dash.Center();
            var DashOriginScaled = DashOrigin * DashScale;

            const int STRIPES = 3;  //not figuring out how to calculate this on the fly
            var DashCenters = new Vector2[STRIPES];
            var RainbowCenters = new Vector2[STRIPES];

            for (int i = 0; i < STRIPES; i++)
            {
                var Xleft = -DashOriginScaled.X;
                var Xright = device.Viewport.Width + DashOriginScaled.X;
                var Xdistance = (Xright - Xleft) * PhasePercent;

                var DashCenterX = (i % 2 == 0) ? Xright - Xdistance : Xleft + Xdistance;
                var DashCenterY = ScreenCenter.Y + (i - (STRIPES - 1) / 2F) * RainbowThickness * 2;

                DashCenters[i] = new Vector2(DashCenterX, DashCenterY);
                RainbowCenters[i] = new Vector2(
                    MathHelper.Lerp(DashCenterX, (i % 2 == 0) ? device.Viewport.Width : 0, 0.5F),
                    DashCenterY
                    );
            }

            //Draw rainbows to each Dash's center, before drawing Dashes.  Draw version 7 for separate scaling
            var RainbowScaleVector = new Vector2(DashCenters[1].X, RainbowScale);
            var batch = new SpriteBatch(device);
            batch.Begin(SpriteSortMode.Immediate, BlendState.AlphaBlend, SamplerState.PointClamp, DepthStencilState.None, RasterizerState.CullCounterClockwise);
            for (int i = 0; i < STRIPES; i++)
            {
                batch.Draw(RainbowTextureFlat, RainbowCenters[i], null, Color.White, 0, RainbowTextureFlat.Center(), RainbowScaleVector, SpriteEffects.None, 0);
            }
            batch.End();

            //Draw each Dash.  Draw version 6
            //Blend state AlphaBlend which is premultiplied.  Alpha on Dash texture is either 0.0 for transparency or 1.0 to block all previous light
            batch.Begin(SpriteSortMode.Immediate, BlendState.AlphaBlend);
            for (int i = 0; i < DashCenters.Length; i++)
            {
                batch.Draw(dash, DashCenters[i], null, Color.White, 0, DashOrigin, DashScale, (i % 2 == 0 ? SpriteEffects.None : SpriteEffects.FlipHorizontally), 0);
            }
            batch.End();
        }

        //PHASE 2: Spinning throbbing rainbows
        private void DoPhase2(float beat)
        {
            float skewBeat = GetSkewBeat(beat, 2);
            float rotationAngle = -skewBeat;
            float rotationOffset = (float)Math.Min(skewBeat, 1) * 0.20F;
            float extraIntensity = ExtraIntensity(beat);

            var batch = new SpriteBatch(device);
            batch.Begin(SpriteSortMode.Immediate, RainbowBlendState, SamplerState.PointClamp, DepthStencilState.None, RasterizerState.CullCounterClockwise);

            DrawRainbowBand(batch, ScreenCenter, rotationAngle - rotationOffset, 1.0F - 0.25F * TriangleWave(skewBeat * 8F), extraIntensity);
            DrawRainbowBand(batch, ScreenCenter, rotationAngle, 1.0F, extraIntensity);
            DrawRainbowBand(batch, ScreenCenter, rotationAngle, 1.0F, extraIntensity);
            DrawRainbowBand(batch, ScreenCenter, rotationAngle + rotationOffset, 1.0F + 0.25F * TriangleWave(skewBeat * 8F), extraIntensity);

            batch.End();
        }
        
        private float ExtraIntensity(float beat)
        {
            //Add some extra intensity synched to the beat, for a small fraction of a beat thereafter
            float beatOffset = beat - (float)Math.Floor(beat);
            return MathHelper.Clamp(0.8F - beatOffset * 2, 0, 1);
        }

        //turns X into a triangle wave.  (0,0) (1,1) (2,0) (3,-1) (4,0)
        private float TriangleWave(float x)
        {
            return GetTriangleWave(x, 1, 4);
        }

        private void DrawRainbowBand(SpriteBatch batch, Vector2 origin, float angle, float scaleFactor, float extraIntensity)
        {
            DrawRainbowBand(batch, origin, angle, scaleFactor, RainbowBandTexture);

            if (extraIntensity > 0)
            {
                //Draw a white texture of the same size and shape on top of the rainbow we just drew

                var RainbowBandWhiteTexels = new Color[RainbowBandTexture.Height];
                RainbowBandTexture.GetData(RainbowBandWhiteTexels);

                RainbowBandWhiteTexels = RainbowBandWhiteTexels.Select(c => new Color(new Vector4(new Vector3(c.ToVector3().Length() > 0 ? extraIntensity : 0), 0))).ToArray();
                var RainbowBandWhiteTexture = new Texture2D(device, 1, RainbowBandWhiteTexels.Length);
                RainbowBandWhiteTexture.SetData(RainbowBandWhiteTexels);

                DrawRainbowBand(batch, origin, angle, scaleFactor, RainbowBandWhiteTexture);
            }
        }

        private void DrawRainbowBand(SpriteBatch batch, Vector2 origin, float angle, float scaleFactor, Texture2D texture)
        {
            //Draw version #5, DestinationRectangle
            //batch.Draw(rainbow, destinationRectangle, null, Color.White, (float)-phaseTime.TotalSeconds, new Vector2(0.5F, 3.0F), SpriteEffects.None, 0);
            //Draw version #6, with float Scale
            //batch.Draw(rainbow, new Vector2(640F, 360F), null, Color.White, (float)-phaseTime.TotalSeconds, new Vector2(rainbow.Width / 2F, rainbow.Height / 2F), 6.0F, SpriteEffects.None, 0);
            //Draw version #7, with Vector2 Scale, allows different scales for width and height.
            //batch.Draw(AllRainbows, new Vector2(640F, 360F), null, Color.White, (float)-phaseTime.TotalSeconds, new Vector2(AllRainbows.Width / 2F, AllRainbows.Height / 2F), new Vector2(200.0F, 5.0F), SpriteEffects.None, 0);
            //We need version 7

            batch.Draw(
                texture,
                origin,
                null,
                Color.White,
                angle,
                new Vector2(texture.Width / 2F, texture.Height / 2F),
                new Vector2(ScreenHypotenuse / scaleFactor * 1.5F, RainbowScale) * scaleFactor,
                SpriteEffects.None,
                0);
        }

        //PHASE 3: SONIC RAINBOOM?  Hah I wish.  Not really.  Fade out phase 2 while Dash draws in a vertical set of rainbows
        private void DoPhase3(float beat)
        {
            var PhasePercent = beat / 2F;
            FadeScreen(PhasePercent);

            int RainbowThickness = (int)(RainbowTextureFlat.Height * RainbowScale);
            var DashOrigin = dash.Center();
            var DashOriginScaled = DashOrigin * DashScale;

            const int STRIPES = 5;  //not figuring out how to calculate this on the fly
            var DashCenters = new Vector2[STRIPES];
            var RainbowCenters = new Vector2[STRIPES];

            for (int i = 0; i < STRIPES; i++)
            {
                var Ytop = -DashOriginScaled.X;
                var Ybottom = device.Viewport.Height + DashOriginScaled.X;
                var Ydistance = (Ybottom - Ytop) * PhasePercent;

                var DashCenterX = ScreenCenter.X + (i - (STRIPES - 1) / 2F) * RainbowThickness * 2;
                var DashCenterY = (i % 2 == 0) ? Ybottom - Ydistance : Ytop + Ydistance;

                DashCenters[i] = new Vector2(DashCenterX, DashCenterY);
                RainbowCenters[i] = new Vector2(
                    DashCenterX,
                    MathHelper.Lerp(DashCenterY, (i % 2 == 0) ? device.Viewport.Height : 0, 0.5F)
                    );
            }

            //Draw rainbows to each Dash's center, before drawing Dashes.  Draw version 7 for separate scaling
            var RainbowScaleVector = new Vector2(DashCenters[1].Y, RainbowScale);
            var batch = new SpriteBatch(device);
            batch.Begin(SpriteSortMode.Immediate, BlendState.AlphaBlend, SamplerState.PointClamp, DepthStencilState.None, RasterizerState.CullCounterClockwise);
            for (int i = 0; i < STRIPES; i++)
            {
                batch.Draw(RainbowTextureFlat, RainbowCenters[i], null, Color.White, -MathHelper.PiOver2, RainbowTextureFlat.Center(), RainbowScaleVector, SpriteEffects.None, 0);
            }
            batch.End();

            //Draw each Dash.  Draw version 6
            batch.Begin();
            for (int i = 0; i < DashCenters.Length; i++)
            {
                batch.Draw(dash, DashCenters[i], null, Color.White, -MathHelper.PiOver2, DashOrigin, DashScale, (i % 2 == 0 ? SpriteEffects.FlipHorizontally : SpriteEffects.None), 0);
            }
            batch.End();
        }

        //PHASE 4: Spinning and bouncing rainbows, while fading out phase 2
        private void DoPhase4(float beat)
        {
            var PhasePercent = beat / 2F;
            FadeScreen(PhasePercent);

            float skewBeat = GetSkewBeat(beat, 2);
            float extraIntensity = ExtraIntensity(beat);

            float BaseSpinSpeed = 2.0F;
            float[] SpinSpeed = new float[] { BaseSpinSpeed, BaseSpinSpeed * 1.1F, BaseSpinSpeed * 1.3F, BaseSpinSpeed * 1.5F };
            float[] SpinPower = new float[] { 1.4F, 1.4F, 1.4F, 1.4F };
            float[] BounceTimeLag = new float[] { 0, 0.05F, 0.10F, 0.20F };

            var batch = new SpriteBatch(device);
            batch.Begin(SpriteSortMode.Immediate, RainbowBlendState, SamplerState.PointClamp, DepthStencilState.None, RasterizerState.CullCounterClockwise);

            for (int i = 0; i <= 3; i++)
            {
                //Rainbows start at -90°
                float rotationAngle = -MathHelper.PiOver2 - SpinSpeed[i] * (float) Math.Pow(skewBeat * BeatLength, SpinPower[i]);
                float scale = BounceScale(beat - BounceTimeLag[i]);
                DrawRainbowBand(batch, ScreenCenter, rotationAngle, scale, extraIntensity);
            }
            batch.End();
        }

        //Calculate the scale for the bouncing rainbows.
        //Scale is maxed at start at beat 0
        //Reach minimum scale at beat period / 2
        //Use real time, not skew time here, because it's already a quadratic start
        private float BounceScale(float beat)
        {
            const float PERIOD = 7;
            float OFFSET = 1;

            //Scale beat to a range -1..1, by doing modulo 2 then -1
            //Also translate the function by adding before the modulo, so at beat 0 the max value occurs
            float DistanceFromZero = (beat / (PERIOD / 2) + OFFSET) % 2 - 1;
            float result = 1 - (float)Math.Pow(DistanceFromZero, 2);

            return result;
        }

        //PHASE 5: Same as phase 4, except the centers of the rainbowbands move around.
        private void DoPhase5(float beat)
        {
            float skewBeat = GetSkewBeat(beat, 2);
            float extraIntensity = ExtraIntensity(beat);

            float BaseSpinSpeed = 3.0F;
            float[] SpinSpeed = new float[] { BaseSpinSpeed, BaseSpinSpeed * 1.1F, BaseSpinSpeed * 1.3F, BaseSpinSpeed * 1.5F };
            float[] SpinPower = new float[] { 1.40F, 1.40F, 1.40F, 1.40F };
            float[] BounceTimeLag = new float[] { 0, 0.05F, 0.10F, 0.20F };

            float scaleCenterOrbit = device.Viewport.Height / 3;
            Vector2[] centers = new Vector2[] {
                new Vector2(ScreenCenter.X + (float)Math.Cos(MathHelper.PiOver2 + skewBeat) * scaleCenterOrbit, ScreenCenter.Y - (float)Math.Sin(MathHelper.PiOver2 + skewBeat) * scaleCenterOrbit),
                new Vector2(ScreenCenter.X + (float)Math.Cos(MathHelper.PiOver2 + skewBeat * 1.2) * scaleCenterOrbit, ScreenCenter.Y - (float)Math.Sin(MathHelper.PiOver2 + skewBeat * 1.2) * scaleCenterOrbit),
            };

            var batch = new SpriteBatch(device);
            batch.Begin(SpriteSortMode.Immediate, RainbowBlendState, SamplerState.PointClamp, DepthStencilState.None, RasterizerState.CullCounterClockwise);

            for (int i = 0; i <= 3; i++)
            {
                float rotationAngle = -SpinSpeed[i] * (float)Math.Pow(skewBeat * BeatLength, SpinPower[i]);
                float scale = BounceScale(beat - BounceTimeLag[i]);
                DrawRainbowBand(batch, centers[i / 2], rotationAngle, scale, extraIntensity);
            }
            batch.End();
        }

        //PHASE 6: slide viewport off to the right (slightly hacky since it won't take effect until next frame, but whatever)
        private void DoPhase6(float beat)
        {
            var width = device.PresentationParameters.BackBufferWidth;
            var viewport = device.Viewport;
            viewport.X = (int)MathHelper.Clamp(beat / 2 * width, 0, width);
            viewport.Width = width - viewport.X;
            device.Viewport = viewport;
        }
    }

    public class Phase
    {
        public float StartBeat;
        public float EndBeat;
        public Action<float> PhaseFunction;

        public Phase(float startBeat, float endBeat, Action<float> phaseFunction)
        {
            StartBeat = startBeat;
            EndBeat = endBeat;
            PhaseFunction = phaseFunction;
        }
    }
}
