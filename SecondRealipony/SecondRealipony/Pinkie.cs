using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using Microsoft.Xna.Framework.Graphics;
using Microsoft.Xna.Framework;

namespace SecondRealipony
{
    class Pinkie : SRSegment
    {
        public override float EndBeat { get { return 80; } }
        public override string MusicName { get { return "pinkie.wav"; } }
        public override double MusicCue { get { return 316.333333; } }

        AlphaTestEffect alphaTestEffect;
        BasicEffect basicEffect;
        Texture2D pinkie;
        Texture2D party;
        VertexBuffer floorVertexBuffer;
        VertexBuffer partyVertexBuffer;
        VertexBuffer shadowVertexBuffer;
        int[] bobModelTypes;
        Matrix[] bobRotations;
        const int MAXBOBS = 400;
        const int MODELTYPES = 6;

        public Pinkie(Game game)
            : base(game)
        {
            alphaTestEffect = new AlphaTestEffect(device);
            basicEffect = new BasicEffect(device);
            pinkie = game.Content.Load<Texture2D>("pinkie.png");
            party = game.Content.Load<Texture2D>("party.png");
            CreateGeometry();
        }

        protected void CreateGeometry()
        {
            //remember: in 3d space, Y is positive upwards
            //in texture space, Y is positive downwards

            var floorColor = new Color(64, 64, 64, 255);
            var FloorVertices = new VertexPositionColor[] {
                new VertexPositionColor(new Vector3(-AspectRatio * 10, 0, -20), floorColor),
                new VertexPositionColor(new Vector3(-AspectRatio * 10, 0, 2), floorColor),
                new VertexPositionColor(new Vector3(AspectRatio * 10, 0, -20), floorColor),
                new VertexPositionColor(new Vector3(AspectRatio * 10, 0, 2), floorColor)
            };

            floorVertexBuffer = new VertexBuffer(device, typeof(VertexPositionColor), FloorVertices.Length, BufferUsage.WriteOnly);
            floorVertexBuffer.SetData(FloorVertices);

            var partyVertexQuad = CreateQuad(1, 1, Vector2.Zero, new Vector2(0.1F, 1));

            //Create N copies of the vertices, with different texture coordinates
            var partyVertices = (from i in Enumerable.Range(0, 10)
                                 from v in partyVertexQuad
                                 select new VertexPositionTexture(v.Position, v.TextureCoordinate + new Vector2(0.1F * i, 0))
                                ).ToArray();

            partyVertexBuffer = new VertexBuffer(device, typeof(VertexPositionTexture), partyVertices.Length, BufferUsage.WriteOnly);
            partyVertexBuffer.SetData(partyVertices);

            var shadowVertices = new VertexPositionColor[] {
                new VertexPositionColor(new Vector3(-0.5F, 0, -0.5F), new Color(0, 0, 0, 64)),
                new VertexPositionColor(new Vector3(-0.5F, 0, 0.5F), new Color(0, 0, 0, 64)),
                new VertexPositionColor(new Vector3(0.5F, 0, -0.5F), new Color(0, 0, 0, 64)),
                new VertexPositionColor(new Vector3(0.5F, 0, 0.5F), new Color(0, 0, 0, 64))
            };

            shadowVertexBuffer = new VertexBuffer(device, typeof(VertexPositionColor), shadowVertices.Length, BufferUsage.WriteOnly);
            shadowVertexBuffer.SetData(shadowVertices);

            CalculateBobData();
        }

        private void CalculateBobData()
        {
            bobModelTypes = new int[MAXBOBS];
            bobRotations = new Matrix[MAXBOBS];

            var random = new Random(0);
            for (int i = 0; i < MAXBOBS; i++)
            {
                bobModelTypes[i] = random.Next(MODELTYPES);
                bobRotations[i] = Matrix.CreateRotationZ((float)random.NextDouble() - 0.5F);
            }
        }

        private void SetFog(IEffectFog effect)
        {
            effect.FogEnabled = true;
            effect.FogColor = Vector3.Zero;
            effect.FogStart = 5;
            effect.FogEnd = 8;
        }

        private void SetCamera(IEffectMatrices effect)
        {
            effect.View = Matrix.CreateLookAt(new Vector3(0, 1, 6), new Vector3(0, 1, 0), Vector3.Up);
            effect.Projection = Matrix.CreatePerspective(AspectRatio, 1, 2, 20);
        }

        private void SetBasicEffect()
        {
            SetFog(basicEffect);
            SetCamera(basicEffect);
        }
        
        private void SetAlphaTestEffect()
        {
            device.BlendState = BlendState.AlphaBlend;
            device.SamplerStates[0] = SamplerState.LinearClamp;

            SetCullMode(CullMode.CullClockwiseFace);
            alphaTestEffect.VertexColorEnabled = false;
            alphaTestEffect.AlphaFunction = CompareFunction.Greater;
            alphaTestEffect.ReferenceAlpha = 128;
            SetFog(alphaTestEffect);
            SetCamera(alphaTestEffect);
        }

        protected override void DrawSegment()
        {
            //Beat = Beat + 36;

            SetBasicEffect();
            SetAlphaTestEffect();
            DrawFloor();
            DrawShadows();
            DrawBobs();
            DrawPinkie();

            if (Beat < 1)
                FadeScreen(1);
            FadeScreen(1, 3, Beat, true, true);
            FadeScreen(76F, 78.5F, Beat, false, false);
            if (Beat >= 78.5F)
                SmashScreen(1);
            FadeScreen(78.5F, 80F, Beat, true, false);
        }

        protected void DrawFloor()
        {
            foreach (EffectPass pass in basicEffect.CurrentTechnique.Passes)
            {
                basicEffect.World = Matrix.Identity;
                basicEffect.TextureEnabled = false;
                basicEffect.VertexColorEnabled = true;
                pass.Apply();

                device.SetVertexBuffer(floorVertexBuffer);
                device.DrawPrimitives(PrimitiveType.TriangleStrip, 0, floorVertexBuffer.VertexCount / 3 + 1);
            }
        }

        protected void DrawPinkie()
        {
            //Pinkie slides in by keeping screen origin fixed (lower left)
            //and moving texture origin, from lower right (width, height) to lower left (0, height)
            var slidePercent = MathHelper.Clamp(Beat - 4, 0, 1);
            var textureOrigin = new Vector2(MathHelper.Lerp(pinkie.Width, 0, slidePercent), pinkie.Height);

            //Draw version 6, single scale
            var batch = new SpriteBatch(device);
            batch.Begin(SpriteSortMode.Immediate, BlendState.AlphaBlend, SamplerState.LinearClamp, DepthStencilState.None, null);
            batch.Draw(pinkie,
                new Vector2(0, device.Viewport.Height),
                null,
                Color.White,
                0,
                textureOrigin,
                (float)device.Viewport.Height / pinkie.Height * 2F / 3F,        //Pinkie is 2/3 screen height
                SpriteEffects.None,
                0);
            batch.End();
        }

        protected void DrawShadows()
        {
            foreach (EffectPass pass in basicEffect.CurrentTechnique.Passes)
            {
                device.DepthStencilState = DepthStencilState.None;
                
                for (int i = 0; i < MAXBOBS; i++)
                {
                    var bobCoordinates = CylinderToVector(GetCylindricalCoordinates(i, Beat));

                    basicEffect.World = Matrix.CreateScale(0.06F) * Matrix.CreateTranslation(bobCoordinates.X, 0F, bobCoordinates.Z);
                    basicEffect.VertexColorEnabled = true;
                    basicEffect.TextureEnabled = false;
                    pass.Apply();

                    device.SetVertexBuffer(shadowVertexBuffer);
                    device.DrawPrimitives(PrimitiveType.TriangleStrip, 0, shadowVertexBuffer.VertexCount - 2);
                }
            }
        }

        protected void DrawBobs()
        {
            device.DepthStencilState = DepthStencilState.Default;
            foreach (EffectPass pass in alphaTestEffect.CurrentTechnique.Passes)
            {
                for (int i = 0; i < MAXBOBS; i++)
                {
                    alphaTestEffect.World = Matrix.CreateScale(0.25F) * bobRotations[i] * Matrix.CreateTranslation(CylinderToVector(GetCylindricalCoordinates(i, Beat)));
                    alphaTestEffect.Texture = party;
                    pass.Apply();

                    device.SetVertexBuffer(partyVertexBuffer);
                    device.DrawPrimitives(PrimitiveType.TriangleStrip, bobModelTypes[i] * 4, 2);
                }
            }
        }

        protected Vector3 CylinderToVector(VectorCylindrical v)
        {
            return new Vector3(
                v.R * (float)Math.Cos(v.θ) * AspectRatio,
                v.H,
                v.R * -(float)Math.Sin(v.θ)
            );

            //Second Reality cheats on the perspective - it's NOT circular in 3D space.  that's why we multiply X by AspectRatio
        }

        protected VectorCylindrical GetCylindricalCoordinates(int index, float beat)
        {
            //phase 0: launch, beats 0 to 16 (launches at 6 and 14)
            //phase 1: lerp/smoothstep between launch and hypotrochoid, beats 16 to 20
            //phase 2: hypotrochoid, beats 16 to (24..36) depending on bob index
            //phase 3: circle, starts at (24..36), lasts to (40..52)
            //phase 4: spray fountain, starts beat (40..52), lasts to (52..64)
            //phase 5: recycle into another spray, starts (52..64), lasts to (64..76)
            //phase 6: chaos, starts at beat (64..76)

            if (beat < 6)
                return GetPhase0(index, beat);

            if (beat < 20)
                return GetPhase1(index, beat);

            float bobPercent = (float)index / MAXBOBS;

            if (bobPercent > (beat - 24) / 12F)
                return GetPhase2(index, beat);

            if (bobPercent > (beat - 40) / 12F)
                return GetPhase3(index, beat);

            if (bobPercent > (beat - 52) / 12F)
                return GetPhase4(index, beat);

            if (bobPercent > (beat - 64) / 12F)
                return GetPhase5(index, beat);

            return GetPhase6(index, beat);
        }

        protected float GetLaunchBeat(int index)
        {
            return index < MAXBOBS / 2 ? 6F : 14F;
        }

        //Launch position
        protected VectorCylindrical GetPhase0(int index, float beat)
        {
            float localBeat = beat - GetLaunchBeat(index);

            //If it hasn't launched yet, bury it offscreen
            if (localBeat < 0)
                return new VectorCylindrical { R = 20 };

            const float depth = -0.5F;
            var xCenter = -1F + 0.65F * localBeat - 0.1F * localBeat * localBeat;
            var yCenter = 0.20F + 1.1F * localBeat - 0.22F * localBeat * localBeat;

            //Dispersion
            var random = new Random(index);
            float dispersionAngle = (float)(random.NextDouble() * MathHelper.TwoPi);
            float dispersionRadius = (float)(random.NextDouble());

            float dispersionX = (float)(Math.Cos(dispersionAngle) * dispersionRadius);
            float dispersionY = (float)(Math.Sin(dispersionAngle) * dispersionRadius);

            var x = xCenter + dispersionX * 0.15F * localBeat;
            var y = yCenter + dispersionY * 0.15F * localBeat;

            return new VectorCylindrical
            {
                R = (float)Math.Sqrt(x * x + depth * depth),
                θ = (float)Math.Atan2(depth, x),
                H = y
            };
        }

        //Smoothstep between the launch position from phase 0 and position from phase 2
        protected VectorCylindrical GetPhase1(int index, float beat)
        {
            var phase0 = GetPhase0(index, beat);
            var phase2 = GetPhase2(index, beat);
            float percent = MathHelper.Clamp((beat - GetLaunchBeat(index)) / 6F, 0, 1);

            return new VectorCylindrical
            {
                R = MathHelper.SmoothStep(phase0.R, phase2.R, percent),
                θ = MathHelper.SmoothStep(phase0.θ, phase2.θ, percent),
                H = MathHelper.SmoothStep(phase0.H, phase2.H, percent)
            };
        }

        //Bouncing Hypotrochoid
        protected VectorCylindrical GetPhase2(int index, float beat)
        {
            //Age works slightly differently than for the other phases.
            //16 beats worth of age, so that the first launched set has 8 beats worth, then second launched set begins 8 beats later to link up smoothly
            //Also add 8 so we're not dealing with negative ages

            var startbeat = 6F + (index / (MAXBOBS / 16F));
            var age = beat - startbeat + 8;

            //Polar equation of a hypotrochoid:
            // r(θ)² = (R - r)² + 2d(R - r) cos (R/r θ) + d²

            //Each bob: R remains constant with beat (depends only on θ)
            //θ remains constant for use by other coords - THEN add GetGlobalRotate as function of beat
            //H comes from GetBouncedCoordinate with the age factor

            float θ = (float)index / MAXBOBS * MathHelper.TwoPi * 4;

            float R = 0.5F;
            float r = 0.3F;
            float d = 0.5F;

            return new VectorCylindrical
            {
                R = (float)Math.Sqrt((Math.Pow(R - r, 2)) + 2 * d * (R - r) * Math.Cos(R / r * θ) + d * d),
                θ = θ % MathHelper.TwoPi + GetGlobalRotate(beat),
                H = GetBouncedCoordinate(age, 0F, 1.30F, -0.35F, 0.85F, 0, 10)
            };
        }

        //Bouncing circle
        protected VectorCylindrical GetPhase3(int index, float beat)
        {
            var startbeat = 24F + (index / (MAXBOBS / 12F));
            var age = beat - startbeat;

            return new VectorCylindrical
            {
                R = 1.0F,
                θ = (float)index / MAXBOBS * MathHelper.TwoPi * 4.01F + GetGlobalRotate(beat) + MathHelper.PiOver2,  //last addition is to start it in back, less visible transition
                H = GetBouncedCoordinate(age, 0.5F, 1.05F, -0.35F, 0.85F, 0, 10)
            };
        }

        //Spray
        protected VectorCylindrical GetPhase4(int index, float beat)
        {
            var startbeat = 40F + (index / (MAXBOBS / 12F));
            var age = beat - startbeat;

            return new VectorCylindrical
            {
                //each bob never moves in radius.  starts negative for first bobs
                R = (float)Math.Sin((float)index / MAXBOBS * MathHelper.Pi * 0.5F) - 0.2F,
                θ = (float)index / MAXBOBS * MathHelper.TwoPi * 24F + GetGlobalRotate(beat),    //* 24F means the bobs form 24 circular shells
                H = GetBouncedCoordinate(age, 0F, 1.7F, -0.72F, 0.85F, 0, 10)
            };
        }

        //Second round of spray, treat these bobs as indexes 400-800
        protected VectorCylindrical GetPhase5(int index, float beat)
        {
            return GetPhase4(index + MAXBOBS, beat);
        }

        //Chaos
        protected VectorCylindrical GetPhase6(int index, float beat)
        {
            var startbeat = 60F + (index / (MAXBOBS / 12F));
            var age = beat - startbeat;

            var random = new Random(index);

            var R = (float)(random.NextDouble() + 0.3);
            var θ = (float)(random.NextDouble() * MathHelper.TwoPi + GetGlobalRotate(beat));
            var H = GetBouncedCoordinate(age, (float)(random.NextDouble()) * 2.2F, (float)random.NextDouble() - 0.1F, -0.3F, 0.90F, 0, 10);

            return new VectorCylindrical
            {
                R = R,
                θ = θ,
                H = H
            };
        }

        private static float GetGlobalRotate(float beat)
        {
            return (float)(beat < 64 ? MathHelper.PiOver2 * Math.Sin(beat * MathHelper.TwoPi / 16F) : Math.Pow((beat - 64) / MathHelper.PiOver2, 1.30F));
        }
    }

    struct VectorCylindrical
    {
        public float R;
        public float θ;
        public float H;
    }
}
