using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using Microsoft.Xna.Framework.Graphics;
using Microsoft.Xna.Framework;

namespace SecondRealipony
{
    class Fluttershy : SRSegment
    {
        BasicEffect basicEffect;
        VertexPositionColor[] TerrainVertices;
        VertexBuffer terrainVertexBuffer;
        VertexPositionTexture[] OverlayVertices;
        VertexBuffer overlayVertexBuffer;
        int[] indices;
        IndexBuffer indexBuffer;
        const int WIDTH = 160;
        const int DEPTH = 120;
        Texture2D animals;
        Texture2D leaves;

        public override float EndBeat { get { return 62; } }
        public override string MusicName { get { return "fluttershy.wav"; } }
        public override double MusicCue { get { return 174.15; } }

        public Fluttershy(Game game)
            : base(game)
        {
            animals = game.Content.Load<Texture2D>("beatree.png");
            leaves = game.Content.Load<Texture2D>("fluttershy.png");
            basicEffect = new BasicEffect(device);

            CreateGeometry();
        }

        private void CreateGeometry()
        {
            CreateTerrainVertices();
            CreateSimpleBumps();
            AddPerlinNoise();
            CalculateVertexColors();
            CreateIndices(WIDTH, DEPTH, out indices, out indexBuffer);
            CreateOverlayVertices();
            CreateVertexBuffers();
        }

        private void CreateTerrainVertices()
        {
            List<VertexPositionColor> results = new List<VertexPositionColor>();

            for (int y = 0; y < DEPTH; y++)
            {
                for (int x = 0; x < WIDTH; x++)
                {
                    var vertex = new VertexPositionColor(new Vector3(x, y, 0), Color.Blue);
                    results.Add(vertex);
                }
            }

            TerrainVertices = results.ToArray();
        }

        private void CreateSimpleBumps()
        {
            Vector2 center = new Vector2(WIDTH, DEPTH) / 2;

            //Fill most of the surface to start with a plateau in the middle dropping off towards the edges
            CreateBump(
                center,
                p => MathHelper.Clamp(p * 1.5F, 0, 1) * 2.0F,
                center
            );

            //Fill in bottom left
            CreateBump(
                new Vector2(WIDTH * 0.16F, DEPTH * 0.52F),
                p => (float)Math.Sin(MathHelper.PiOver2 * p) * 2.0F,
                new Vector2(WIDTH / 6, DEPTH / 4)
            );
        }

        //Applies an elliptical bump (or trough) to the drawing sheet
        //DistanceFunc should map the range 0..1 to a result (can be negative)
        //Center is floats and doesn't need to be exactly on a vertex
        private void CreateBump(Vector2 center, Func<float, float> DistanceFunc, Vector2 scale)
        {
            int minY = Math.Max(0, (int)(center.Y - scale.Y));
            int maxY = Math.Min(DEPTH, (int)(center.Y + scale.Y));
            int minX = Math.Max(0, (int)(center.X - scale.X));
            int maxX = Math.Min(WIDTH, (int)(center.X + scale.X));

            for (int y = minY; y < maxY; y++)
                for (int x = minX; x < maxX; x++)
                {
                    var VertexVector = new Vector2(x - center.X, y - center.Y);
                    var angle = (float)Math.Atan2(VertexVector.Y, VertexVector.X);
                    var radius = RadiusOfEllipse(angle, scale);
                    var percentage = 1 - (VertexVector.Length() / radius);
                    percentage = MathHelper.Clamp(percentage, 0, 1);
                    var finalValue = DistanceFunc(percentage);

                    TerrainVertices[GetVertexIndex(x, y)].Position.Z += finalValue;
                }
        }

        private void AddPerlinNoise()
        {
            PerlinOctave[] octaves;

            octaves = new PerlinOctave[] {
                new PerlinOctave(13, 13, 2.0F),
                new PerlinOctave(29, 29, 1.0F),
                new PerlinOctave(42, 42, 0.75F),
                new PerlinOctave(80, 80, 0.15F)
            };

            foreach (var octave in octaves)
                octave.Seed();

            for (int y = 0; y < DEPTH; y++)
            {
                for (int x = 0; x < WIDTH; x++)
                {
                    foreach (var octave in octaves)
                    {
                        TerrainVertices[y * WIDTH + x].Position.Z += octave.GetValue((float)x / WIDTH, (float)y / DEPTH);
                    }
                }
            }
        }

        //Create overlay vertices, with dummy texture coordinates (that gets filled in later every frame)
        private void CreateOverlayVertices()
        {
            OverlayVertices = TerrainVertices.Select(v => new VertexPositionTexture(v.Position, Vector2.Zero)).ToArray();
        }

        private int GetVertexIndex(int x, int y)
        {
            return y * WIDTH + x;
        }

        private float RadiusOfEllipse(float angle, Vector2 scale)
        {
            return scale.X * scale.Y / (float) (Math.Sqrt(Math.Pow(scale.Y * Math.Cos(angle), 2) + Math.Pow(scale.X * Math.Sin(angle), 2)));
        }

        //Calculate colors for all vertices in the sheet.
        //Normalized to the lowest vertex being 0 and the highest 1.0.
        //Mostly blue, but also throw in 0.10 red and 0.20 green for a bit more depth.
        private void CalculateVertexColors()
        {
            var minZ = TerrainVertices.Min(v => v.Position.Z);
            var maxZ = TerrainVertices.Max(v => v.Position.Z);
            var ScaleFactor = maxZ - minZ;

            TerrainVertices = TerrainVertices.Select(v => new VertexPositionColor(v.Position, new Color(new Vector3(0.10F, 0.20F, 1.0F) * (v.Position.Z - minZ) / ScaleFactor))).ToArray();
        }

        private void CreateVertexBuffers()
        {
            terrainVertexBuffer = new VertexBuffer(device, typeof(VertexPositionColor), TerrainVertices.Length, BufferUsage.WriteOnly);
            terrainVertexBuffer.SetData(TerrainVertices);

            overlayVertexBuffer = new VertexBuffer(device, typeof(VertexPositionTexture), TerrainVertices.Length, BufferUsage.WriteOnly);
            //SetData happens later, each frame
        }

        //Set texture coordinates for the overlay vertices
        //ScrollPercent ranges from 0 to 1.  0 has the texture just offscreen to the right beyond the view window, 1 just offscreen to the left
        private void SetTextureCoordinates(float scrollPercent)
        {
            var windowWidth = 0.6F;     //what fraction of the entire texture is displayed at once
            var textureLeftEdge = -windowWidth + (1 + windowWidth) * scrollPercent;
            var textureRightEdge = textureLeftEdge + windowWidth;

            for (int y = 0; y < DEPTH; y++)
                for (int x = 0; x < WIDTH; x++)
                {
                    var textureX = MathHelper.Lerp(textureLeftEdge, textureRightEdge, (float)x / WIDTH);
                    var textureY = (1 - (float)y / DEPTH) * 1.2F - 0.15F;
                    OverlayVertices[y * WIDTH + x].TextureCoordinate.X = textureX;
                    OverlayVertices[y * WIDTH + x].TextureCoordinate.Y = textureY;
                }
        }

        private void SetBasicEffect()
        {
            //spriteBatch changes the DepthStencilState to None and that's why 3D objects don't draw correctly. Other properties get changed too. Check these out:
            device.BlendState = BlendState.Additive;
            device.DepthStencilState = DepthStencilState.Default;
            device.SamplerStates[0] = SamplerState.LinearClamp;

            SetCullMode(CullMode.CullCounterClockwiseFace);

            Vector3 CameraTarget = new Vector3(WIDTH * 0.35F, DEPTH * 0.35F, 0);
            Vector3 CameraPos = new Vector3(WIDTH * 0F, DEPTH * -0.2F, WIDTH * 0.35F);

            basicEffect.View = Matrix.CreateLookAt(CameraPos, CameraTarget, Vector3.UnitZ);

            basicEffect.Projection = Matrix.CreatePerspectiveFieldOfView(
                MathHelper.ToRadians(30),
                AspectRatio,
                0.01f,
                500.0f);

            basicEffect.LightingEnabled = false;
            basicEffect.FogEnabled = true;
            basicEffect.FogStart = DEPTH * 1.2F;
            basicEffect.FogEnd = DEPTH * 1.6F;
        }

        protected override void DrawSegment()
        {
            SetBasicEffect();

            var scrollPercent = (Beat - 4) / 64;
            SetTextureCoordinates(scrollPercent);

            foreach (EffectPass pass in basicEffect.CurrentTechnique.Passes)
            {
                basicEffect.Alpha = MathHelper.Clamp((Beat - 4) / 2, 0, 1);
                DrawTerrain(pass);
                DrawTexture(pass);
                DrawLeaves(MathHelper.Clamp(Beat / 2, 0, 1));

                if (Beat >= 60)
                    FadeScreen((Beat - 60) / 2);
            }
        }

        private void DrawTerrain(EffectPass pass)
        {
            basicEffect.VertexColorEnabled = true;
            basicEffect.TextureEnabled = false;
            pass.Apply();

            device.SetVertexBuffer(terrainVertexBuffer);
            device.Indices = indexBuffer;

            device.DrawIndexedPrimitives(PrimitiveType.TriangleStrip, 0, 0, indexBuffer.IndexCount, 0, indexBuffer.IndexCount - 2);
        }

        private void DrawTexture(EffectPass pass)
        {
            basicEffect.VertexColorEnabled = false;
            basicEffect.TextureEnabled = true;
            basicEffect.Texture = animals;
            pass.Apply();

            overlayVertexBuffer.SetData(OverlayVertices);
            device.SetVertexBuffer(overlayVertexBuffer);

            device.DrawIndexedPrimitives(PrimitiveType.TriangleStrip, 0, 0, indexBuffer.IndexCount, 0, indexBuffer.IndexCount - 2);
        }

        private void DrawLeaves(float fadeInPercent)
        {
            device.BlendState = BlendState.Opaque;
            device.DepthStencilState = DepthStencilState.Default;
            device.SamplerStates[0] = SamplerState.LinearWrap;

            SetCullMode(CullMode.CullClockwiseFace);

            var batch = new SpriteBatch(device);
            batch.Begin(SpriteSortMode.Immediate, BlendState.AlphaBlend);
            batch.Draw(leaves, FullScreen, new Color(new Vector3(fadeInPercent)));
            batch.End();
        }
    }
}
