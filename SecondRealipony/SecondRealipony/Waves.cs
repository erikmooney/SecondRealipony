using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using Microsoft.Xna.Framework.Graphics;
using Microsoft.Xna.Framework;

namespace SecondRealipony
{
    class Waves : SRSegment
    {
        public override float EndBeat { get { return 62; } }
        public override string MusicName { get { return "waves.wav"; } }

        BasicEffect basicEffect;

        const int WIDTH = 240;
        const int DEPTH = 135;
        const float WONDERBOLTSCALE = 10;

        VertexPositionColor[] waveVertices;
        VertexBuffer waveVertexBuffer;
        int[] waveIndices;
        IndexBuffer waveIndexBuffer;

        Texture2D wbTextures;
        VertexPositionTexture[] wbVertices;
        VertexBuffer wbVertexBuffer;

        WonderboltFlyby[] wbFlybys;

        public Waves(Game game)
            : base(game)
        {
            wbTextures = game.Content.Load<Texture2D>("wonderbolts.png");

            basicEffect = new BasicEffect(device);
            CreateGeometry();
        }

        protected void CreateGeometry()
        {
            InitializeWaveVertices();
            CreateWaveIndices();
            CreateWbVertices();
            CreateWbFlybys();
        }

        private void InitializeWaveVertices()
        {
            waveVertices = (from z in Enumerable.Range(0, DEPTH)
                           from x in Enumerable.Range(0, WIDTH)
                           select new VertexPositionColor(new Vector3(x, 0, z - DEPTH), Color.Blue)
                           ).ToArray();

            waveVertexBuffer = new VertexBuffer(device, typeof(VertexPositionColor), WIDTH * DEPTH, BufferUsage.None);
        }

        private void CreateWaveIndices()
        {
            CreateIndices(WIDTH, DEPTH, out waveIndices, out waveIndexBuffer);
        }

        private void CreateWbVertices()
        {
            var oneQuad = CreateQuad((float)wbTextures.Width / (wbTextures.Height / 5F), 1);

            //Project that to 5 copies with appropriate texture coordinates
            wbVertices = (from i in Enumerable.Range(0, 5)
                         from v in oneQuad
                         select new VertexPositionTexture(v.Position, new Vector2(v.TextureCoordinate.X, (v.TextureCoordinate.Y + i) * 0.2F))
                         ).ToArray();

            wbVertexBuffer = new VertexBuffer(device, typeof(VertexPositionTexture), wbVertices.Length, BufferUsage.None);
            wbVertexBuffer.SetData<VertexPositionTexture>(wbVertices);
        }

        private void CreateWbFlybys()
        {
            //Flybys:
            //1. Solo
            //2. Two with deeper one faster
            //3. Three in a line right to left slightly staggered
            //4. Three opposing perfectly synced
            //5. Five in full formation

            wbFlybys = new WonderboltFlyby[] {
                new WonderboltFlyby(WIDTH * 0.5F, WIDTH * 0.12F, DEPTH * 0.70F, 14, 0),

                new WonderboltFlyby(WIDTH * 0.5F, WIDTH * 0.20F, DEPTH * 0.50F, 25, 2),
                new WonderboltFlyby(WIDTH * 0.5F, WIDTH * 0.12F, DEPTH * 0.60F, 24, 1),

                new WonderboltFlyby(WIDTH * 0.5F, WIDTH * -0.12F, DEPTH * 0.60F, 33.5F, 0),
                new WonderboltFlyby(WIDTH * 0.5F, WIDTH * -0.12F, DEPTH * 0.65F, 34, 1),
                new WonderboltFlyby(WIDTH * 0.5F, WIDTH * -0.12F, DEPTH * 0.70F, 34.5F, 4),

                new WonderboltFlyby(WIDTH * 0.5F, WIDTH * -0.12F, DEPTH * 0.65F, 44F, 0),
                new WonderboltFlyby(WIDTH * 0.5F, WIDTH * 0.12F, DEPTH * 0.70F, 44F, 2),
                new WonderboltFlyby(WIDTH * 0.5F, WIDTH * -0.12F, DEPTH * 0.75F, 44F, 1),

                new WonderboltFlyby(WIDTH * 0.5F, WIDTH * 0.16F, DEPTH * 0.55F, 53F, 3),
                new WonderboltFlyby(WIDTH * 0.5F, WIDTH * 0.16F, DEPTH * 0.60F, 52.5F, 1),
                new WonderboltFlyby(WIDTH * 0.5F, WIDTH * 0.16F, DEPTH * 0.65F, 52F, 0),
                new WonderboltFlyby(WIDTH * 0.5F, WIDTH * 0.16F, DEPTH * 0.70F, 52.5F, 2),
                new WonderboltFlyby(WIDTH * 0.5F, WIDTH * 0.16F, DEPTH * 0.75F, 53F, 4),
            };
        }

        private void SetBasicEffect()
        {
            //spriteBatch changes the DepthStencilState to None and that's why 3D objects don't draw correctly. Other properties get changed too. Check these out:
            device.BlendState = BlendState.AlphaBlend;
            device.DepthStencilState = DepthStencilState.Default;

            SetCullMode(CullMode.CullClockwiseFace);

            Vector3 CameraPos = new Vector3(WIDTH * 0.5F, 10, DEPTH * 0.10F);
            Vector3 CameraTarget = new Vector3(WIDTH * 0.5F, -5, -DEPTH);

            var view = Matrix.CreateLookAt(CameraPos, CameraTarget, Vector3.Up);
            view *= Matrix.CreateRotationY((float)Math.Sin(Beat / 16F * MathHelper.TwoPi - MathHelper.PiOver2) * 0.2F);

            if (Beat < 4)
                view *= Matrix.CreateRotationX(MathHelper.SmoothStep(-0.4F, 0, Beat / 4));
            if (Beat > EndBeat - 4)
                view *= Matrix.CreateRotationX(MathHelper.SmoothStep(0, -0.4F, (Beat - EndBeat + 4) / 4));

            basicEffect.View = view;
            basicEffect.Projection = Matrix.CreatePerspectiveFieldOfView(MathHelper.ToRadians(30), AspectRatio, 0.001F, DEPTH * 1.5F);
            basicEffect.TextureEnabled = false;
            basicEffect.LightingEnabled = false;
            basicEffect.VertexColorEnabled = true;
        }

        protected override void DrawSegment()
        {
            device.SetVertexBuffer(null);
            CalculateWaveHeights();
            ApplyWonderboltWaves();
            CalculateWaveColors();
            SetBasicEffect();
            DrawWaves();
            DrawPonies();
        }

        private void CalculateWaveHeights()
        {
            for (int z = 0; z < DEPTH; z++)
                for (int x = 0; x < WIDTH; x++)
                {
                    var y = (float)Math.Sin(x / MathHelper.TwoPi + Beat) * 0.75F;
                    y += (float)Math.Sin((x + z) / MathHelper.TwoPi + Beat * 1.5F);
                    y += (float)Math.Sin((-z) / MathHelper.Pi * 0.82F + Beat * 0.82F) * 0.75F;
                    y += (float)Math.Cos((x * (x + Math.Sin(x)) / WIDTH) / MathHelper.Pi + Beat);

                    waveVertices[z * WIDTH + x].Position.Y = y;
                }
        }

        private void ApplyWonderboltWaves()
        {
            foreach (var wb in wbFlybys)
            {
                if (Math.Abs(Beat - wb.KeyBeat) < 10)
                    ApplyWonderboltWave(wb);
            }
        }

        //Takes Wonderbolt position and applies its shockwave
        private void ApplyWonderboltWave(WonderboltFlyby wb)
        {
            const int SIDEWIDTH = 20;

            int minZ = Math.Max(0, (int)wb.Z - SIDEWIDTH);
            int maxZ = Math.Min(DEPTH, (int)wb.Z + SIDEWIDTH);

            for (int x = 0; x < WIDTH; x++)
            {
                var timeOffset = wb.GetTimeOffsetLeadingEdge(x, Beat);
                float y = 0;

                //Factor for smoothstepping between original wave and shockwave.  0 means original, 1 means shockwave
                float stepFactor = 0;

                //Begin bow shock in front of WB
                if (timeOffset >= -0.25F && timeOffset < 0)
                {
                    float percent = timeOffset * 4 + 1;         //percent from 0 at front of shockwave to 1 at leading edge
                    y = -5 * (float)Math.Sqrt(percent * percent);
                    stepFactor = percent;
                }
                //Flat shockwave trough
                else if (timeOffset >= 0 && timeOffset < 0.5F)
                {
                    y = -5;
                    stepFactor = 1;
                }
                //Ramp from shockwave trough to crest
                else if (timeOffset >= 0.5F && timeOffset < 3F)
                {
                    float percent = (timeOffset - 0.5F) / 2.5F;
                    y = MathHelper.SmoothStep(-5, 5, percent);
                    stepFactor = MathHelper.SmoothStep(1, 0, percent / 2F);
                }
                //Ramp from crest to long tail eventually flat
                else if (timeOffset >= 3F && timeOffset < 9)
                {
                    float percent = (timeOffset - 3F) / 6F;
                    y = MathHelper.SmoothStep(5, 0, percent);
                    stepFactor = MathHelper.SmoothStep(1, 0, percent / 2F + 0.5F);
                }

                //Add sine wave for a wake
                if (timeOffset >= 1F && timeOffset < 9F)
                    y += (float)Math.Sin(timeOffset * 2 * MathHelper.TwoPi) * 1.5F;

                if (stepFactor > 0)
                {
                    for (int z = minZ; z < maxZ; z++)
                    {
                        //Everything is damped by distance from travel axis
                        float adjustedStepFactor = stepFactor * (1 - wb.GetSideOffset(z) / SIDEWIDTH);

                        //Smooth step from original wave y to shockwave y
                        float finalY = MathHelper.SmoothStep(waveVertices[z * WIDTH + x].Position.Y, y, adjustedStepFactor);

                        waveVertices[z * WIDTH + x].Position.Y = finalY;
                    }
                }
            }
        }

        private void CalculateWaveColors()
        {
            for (int z = 0; z < DEPTH; z++)
                for (int x = 0; x < WIDTH; x++)
                {
                    var y = waveVertices[z * WIDTH + x].Position.Y;

                    //Range of the wave function is -5 to +5

                    var color =
                        y > -2F ?
                        Color.Lerp(Color.Blue, new Color(182, 240, 255), (y + 2F) / 7F) :
                        Color.Lerp(new Color(64, 0, 32), Color.Blue, (y + 5) / 3F);

                    waveVertices[z * WIDTH + x].Color = color;
                }
        }

        private void DrawWaves()
        {
            waveVertexBuffer.SetData(waveVertices);
            device.SetVertexBuffer(waveVertexBuffer);
            device.Indices = waveIndexBuffer;

            foreach (EffectPass pass in basicEffect.CurrentTechnique.Passes)
            {
                basicEffect.World = Matrix.Identity;
                pass.Apply();

                device.DrawIndexedPrimitives(
                    PrimitiveType.TriangleStrip,
                    0,
                    0,
                    waveIndexBuffer.IndexCount,
                    0,
                    waveIndexBuffer.IndexCount - 2
                    );
            }
        }

        private void DrawPonies()
        {
            basicEffect.VertexColorEnabled = false;
            basicEffect.TextureEnabled = true;
            SetCullMode(CullMode.None);                     //no cull for Wonderbolts because we flip the model to fly the other direction
            device.SetVertexBuffer(wbVertexBuffer);

            foreach (EffectPass pass in basicEffect.CurrentTechnique.Passes)
            {
                foreach (var wb in wbFlybys)
                {
                    basicEffect.Texture = wbTextures;
                    basicEffect.World =
                        Matrix.CreateScale(WONDERBOLTSCALE)
                        * Matrix.CreateScale(Math.Sign(wb.Velocity), 1, 1)
                        * Matrix.CreateTranslation(wb.GetX(Beat), 2F, wb.Z - DEPTH);
                    pass.Apply();

                    device.DrawPrimitives(PrimitiveType.TriangleStrip, wb.Model * 4, 2);
                }
            }
        }

        //Defines a horizontal Wonderbolt flyby
        //Wonderbolt position is defined as positive X and Z (positive away from camera), at a certain point in time (keyBeat)
        //Velocity is in vertices/beat (same as worldspace/beat)
        //Model is an int, specifies which Wonderbolt model (set of vertices with corresponding texture coordinates)
        public class WonderboltFlyby
        {
            public float KeyPosition;
            public float Velocity;
            public float Z;
            public float KeyBeat;
            public int Model;

            public WonderboltFlyby(float keyPosition, float velocity, float z, float keyBeat, int model)
            {
                KeyPosition = keyPosition;
                Velocity = velocity;
                Z = z;
                KeyBeat = keyBeat;
                Model = model;
            }

            //Calculate current X position of Wonderbolt
            public float GetX(float beat)
            {
                return KeyPosition + (beat - KeyBeat) * Velocity;
            }

            //Calculate current X position of Wonderbolt's leading edge
            public float GetLeadingX(float beat)
            {
                return GetX(beat) + GetLeadingEdgeOffset();
            }

            //Calculate at what time the Wonderbolt passes the specified X coordinate
            public float GetTime(float x)
            {
                return (x - KeyPosition) / Velocity + KeyBeat;
            }

            //Calculate at what time the Wonderbolt's leading edge passes the specified X coordinate
            public float GetTimeLeadingEdge(float x)
            {
                return (x - GetLeadingEdgeOffset() - KeyPosition) / Velocity + KeyBeat;
            }

            //Calculate how long it's been since the Wonderbolt passed the specified X coordinate.  If it hasn't passed yet, result will be negative
            public float GetTimeOffset(float x, float beat)
            {
                return beat - GetTime(x);
            }

            //Calculate how long it's been since the Wonderbolt's leading edge passed the specified X coordinate
            public float GetTimeOffsetLeadingEdge(float x, float beat)
            {
                return beat - GetTimeLeadingEdge(x);
            }

            //Calculate the X offset for the Wonderbolt's leading edge
            public float GetLeadingEdgeOffset()
            {
                return WONDERBOLTSCALE * Math.Sign(Velocity);
            }

            //Calculate the sideways distance between the Wonderbolt's depth and a specified Z coordinate
            public float GetSideOffset(float z)
            {
                return Math.Abs(z - Z);
            }
        }
    }
}