using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using Microsoft.Xna.Framework.Graphics;
using Microsoft.Xna.Framework;

namespace SecondRealipony
{
    class EndSecondHalf : SRSegment
    {
        public override float EndBeat { get { return 20; } }
        public override string MusicName { get { return "EndSecondHalf.wav"; } }

        Texture2D picture;
        BasicEffect basicEffect;
        VertexPositionTexture[] picVertices;
        VertexBuffer picVertexBuffer;
        int[] picIndices;
        IndexBuffer picIndexBuffer;

        const int WIDTH = 51;
        const int HEIGHT = 51;

        public EndSecondHalf(Game game)
            : base(game)
        {
            basicEffect = new BasicEffect(device);
            picture = game.Content.Load<Texture2D>("endsecondhalf.png");
            CreateGeometry();
        }

        protected void CreateGeometry()
        {
            //Create mesh for the picture.  Centered at (0, 0).  In XY plane with Z = 0.  Top row comes first.
            //Position is unused here, it is populated later by CalculateBulge
            picVertices = (from y in Enumerable.Range(0, HEIGHT)
                           from x in Enumerable.Range(0, WIDTH)
                           select new VertexPositionTexture(
                               new Vector3(0, 0, 0),
                               new Vector2((float)x / (WIDTH - 1), (float)y / (HEIGHT - 1))
                               )).ToArray();

            picVertexBuffer = new VertexBuffer(device, typeof(VertexPositionTexture), picVertices.Length, BufferUsage.WriteOnly);
            picVertexBuffer.SetData(picVertices);

            CreateIndices(WIDTH, HEIGHT, out picIndices, out picIndexBuffer);
        }

        protected void SetBasicEffect()
        {
            device.BlendState = BlendState.Opaque;
            device.DepthStencilState = DepthStencilState.None;
            device.SamplerStates[0] = SamplerState.LinearClamp;

            SetCullMode(CullMode.CullClockwiseFace);

            //No view or projection matrices - we are rendering triangles directly in viewport space (after world matrix translation)
            //Well, almost no projection matrix - it just corrects for aspect ratio (pic is square, screen isn't)
            basicEffect.View = Matrix.Identity;
            basicEffect.Projection = Matrix.CreateScale(1F / AspectRatio, 1, 1);

            basicEffect.LightingEnabled = false;
            basicEffect.VertexColorEnabled = false;
            basicEffect.TextureEnabled = true;
            basicEffect.Texture = picture;
        }

        protected override void DrawSegment()
        {
            SetBasicEffect();
            CalculateBulge();
            DrawPicture();
        }

        private void CalculateBulge()
        {
            float bulgeFactor = GetBulgeFactor(Beat);

            for (int y = 0; y < HEIGHT; y++)
                for (int x = 0; x < WIDTH; x++)
                {
                    float relX = (float)x / (WIDTH - 1) * 2 - 1;
                    float relY = 1 - (float)y / (HEIGHT - 1) * 2;

                    relX *= 1 + bulgeFactor * (float)Math.Cos(relY * MathHelper.PiOver2);
                    relY *= 1 - bulgeFactor;

                    picVertices[y * WIDTH + x].Position.X = relX;
                    picVertices[y * WIDTH + x].Position.Y = relY;
                }

        }

        private void DrawPicture()
        {
            float centerY = GetCenterY(Beat);

            device.SetVertexBuffer(null);
            picVertexBuffer.SetData(picVertices);
            device.SetVertexBuffer(picVertexBuffer);
            device.Indices = picIndexBuffer;

            foreach (EffectPass pass in basicEffect.CurrentTechnique.Passes)
            {
                basicEffect.World = Matrix.CreateTranslation(0, centerY, 0);
                pass.Apply();

                device.DrawIndexedPrimitives(
                    PrimitiveType.TriangleStrip,
                    0,
                    0,
                    picIndexBuffer.IndexCount,
                    0,
                    picIndexBuffer.IndexCount - 2
                    );
            }
        }

        //TIMELINE:
        //Beat 0..4: Fall
        //Beat 4..5: Bounce
        //Beat 5+: Run free on GetBouncedCoordinate
        private float GetCenterY(float beat)
        {
            float bounceBeat = 4;
            float accel = -4F / (bounceBeat * bounceBeat);
            float elasticity = 0.60F;

            if (beat < 4)
                return GetBouncedCoordinate(beat, 2, 0, accel, elasticity, 0, -1);
            else if (beat >= 4 && beat < 5)
                return -GetBulgeFactor(beat);
            else
                return GetBouncedCoordinate(beat - 5, 0, -accel * bounceBeat * elasticity, accel, elasticity, 0, -1);
        }

        private float GetBulgeFactor(float beat)
        {
            if (beat < 4)
                return 0;

            //Damp from 0.3 to 0 gradually, over a period of 11 beats starting at beat 5
            float percent = (beat - 5) / 11F;
            float dampFactor = MathHelper.Clamp(MathHelper.Lerp(0.3F, 0, percent), 0, 1);
            return dampFactor * (float)Math.Sin(beat * MathHelper.Pi);
        }
    }
}
