using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using Microsoft.Xna.Framework.Graphics;
using Microsoft.Xna.Framework;

namespace SecondRealipony
{
    class Rarity : SRSegment
    {
        Texture2D rarity;
        Texture2D rarityOuter;
        const int RINGSPERBEAT = 12;
        const int DEPTHPERRING = 2;
        const int GEMSPERRING = 16;
        const float BEATSOFVISIBILITY = 2.5F;
        const float STARTBEAT = -0.5F;
        const float LASTBEAT = 30F;
        BasicEffect basicEffect;
        GemModel[] gemModels;
        Ring[] rings;

        public override float Anacrusis { get { return 2; } }
        public override float EndBeat { get { return 40; } }
        public override string MusicName { get { return "rarity.wav"; } }

        public Rarity(Game game)
            : base(game)
        {
            rarity = game.Content.Load<Texture2D>("Rarity Glow.png");
            rarityOuter = game.Content.Load<Texture2D>("Rarity Glow Outer.png");

            basicEffect = new BasicEffect(device);
            gemModels = new GemModel[] {
                CreateBasicGemModel(),
                CreateRedGemModel(),
                CreateGreenGemModel(),
                CreateBlueGemModel()
            };

            rings = CreateRings();
            SetRingCenters();
        }

        public GemModel CreateBasicGemModel()
        {
            //Basic Gem
            var gemCorners = new Vector3[] {
                new Vector3(0, 1.0F, 0),           //Top
                new Vector3(-0.20F, 0, 0.20F),  //Left-Front
                new Vector3(0.20F, 0, 0.20F),   //Right-Front
                new Vector3(0.20F, 0, -0.20F),  //Right-Back
                new Vector3(-0.20F, 0, -0.20F), //Left-Back
                new Vector3(0, -1.0F, 0)           //Bottom
            };

            var faceCorners = new int[][] {
                new int[] { 0, 1, 2 },
                new int[] { 0, 2, 3 },
                new int[] { 0, 3, 4 },
                new int[] { 0, 4, 1 },
                new int[] { 5, 2, 1 },
                new int[] { 5, 3, 2 },
                new int[] { 5, 4, 3 },
                new int[] { 5, 1, 4 }
            };

            var gemVerticesOuterList = new List<VertexPositionNormalTexture>();
            var gemVerticesInnerList = new List<VertexPositionNormalTexture>();

            foreach (int[] corners in faceCorners)
            {
                gemVerticesOuterList.AddRange(MakeTriangle(gemCorners, corners, new float[] { 0.0F, 0.0F, 0.0F }));
                gemVerticesInnerList.AddRange(MakeTriangle(gemCorners, corners, new float[] { 0.03F, 0.03F, 0.03F }, 0.02F));
            }

            return new GemModel(device, gemVerticesOuterList, gemVerticesInnerList, Color.White);
        }

        public GemModel CreateRedGemModel()
        {
            var gemCorners = new Vector3[] {
                //Front face, 6 corners, counterclockwise from top
                new Vector3(0, 0.5F, 0.2F),
                new Vector3(-0.2F, 0.3F, 0.2F),
                new Vector3(-0.2F, -0.3F, 0.2F),
                new Vector3(0, -0.5F, 0.2F),
                new Vector3(0.2F, -0.3F, 0.2F),
                new Vector3(0.2F, 0.3F, 0.2F),

                //Outer vertices, 6 corners, counterclockwise from top
                new Vector3(0F, 1.0F, 0),
                new Vector3(-0.5F, 0.5F, 0),
                new Vector3(-0.5F, -0.5F, 0),
                new Vector3(0F, -1.0F, 0),
                new Vector3(0.5F, -0.5F, 0),
                new Vector3(0.5F, 0.5F, 0)
            };
            //Copy first 6 vertices to last 6, with inverted Z
            gemCorners = gemCorners.Concat(gemCorners.Take(6).Select(v => new Vector3(v.X, v.Y, -v.Z))).ToArray();

            var faceCorners = new int[][] {
                //Front face
                new int[] { 0, 1, 2 },
                new int[] { 2, 3, 4 },
                new int[] { 4, 5, 0 },
                new int[] { 0, 2, 4 },

                //6 faces, counterclockwise from top, 2 triangles each
                new int[] { 0, 6, 7 },
                new int[] { 7, 1, 0 },
                new int[] { 1, 7, 8 },
                new int[] { 8, 2, 1 },
                new int[] { 2, 8, 9 },
                new int[] { 9, 3, 2 },
                new int[] { 3, 9, 10 },
                new int[] { 10, 4, 3 },
                new int[] { 4, 10, 11 },
                new int[] { 11, 5, 4 },
                new int[] { 5, 11, 6 },
                new int[] { 6, 0, 5 },

                //6 faces, clockwise from top, 2 triangles each
                new int[] { 12, 7, 6 },
                new int[] { 12, 13, 7 },
                new int[] { 13, 8, 7 },
                new int[] { 13, 14, 8 },
                new int[] { 14, 9, 8 },
                new int[] { 14, 15, 9 },
                new int[] { 15, 10, 9 },
                new int[] { 15, 16, 10 },
                new int[] { 16, 11, 10 },
                new int[] { 16, 17, 11 },
                new int[] { 17, 6, 11 },
                new int[] { 17, 12, 6 },

                //Back face
                new int[] { 12, 14, 13 },
                new int[] { 16, 15, 14 },
                new int[] { 12, 17, 16 },
                new int[] { 12, 16, 14 },
            
            };

            var gemVerticesOuterList = new List<VertexPositionNormalTexture>();
            var gemVerticesInnerList = new List<VertexPositionNormalTexture>();

            foreach (int[] corners in faceCorners)
            {
                float edge0 = RedGemInsetEdge(corners[0], corners[1]);
                float edge1 = RedGemInsetEdge(corners[1], corners[2]);
                float edge2 = RedGemInsetEdge(corners[2], corners[0]);

                gemVerticesOuterList.AddRange(MakeTriangle(gemCorners, corners, new float[] { 0.0F, 0.0F, 0.0F }));
                gemVerticesInnerList.AddRange(MakeTriangle(gemCorners, corners, new float[] { edge0, edge1, edge2 }, 0.02F));
            }

            return new GemModel(device, gemVerticesOuterList, gemVerticesInnerList, Color.Red);
        }


        public float RedGemInsetEdge(int i0, int i1)
        {
            //Inset for a thick edge if:
            //   Vertices differ by exactly 6
            //   OR Vertices are in same group of 6 and adjacent or differ by 5

            if (Math.Abs(i0 - i1) == 6 || (i0 / 6 == i1 / 6 && (Math.Abs(i0 - i1) == 1 || Math.Abs(i0 - i1) == 5)))
                return 0.03F;
            else
                return 0F;
        }

        public GemModel CreateGreenGemModel()
        {
            var gemCorners = new Vector3[] {
                //Top back vertex
                new Vector3(0, 0.8F, -0.2F),
                //Middle ring two back vertices
                new Vector3(0.12F, 0.4F, -0.30F),
                new Vector3(-0.12F, 0.4F, -0.30F),
                //Bottom ring two back vertices
                new Vector3(0.08F, -0.8F, -0.20F),
                new Vector3(-0.08F, -0.8F, -0.20F),
            };
            //Now create more by rotating these 120° and 240° around the Y axis
            gemCorners = gemCorners
                .Concat(
                    gemCorners.Select(v => Vector3.Transform(v, Matrix.CreateRotationY(MathHelper.Pi * 2 / 3))).ToArray()
                ).Concat(
                    gemCorners.Select(v => Vector3.Transform(v, Matrix.CreateRotationY(MathHelper.Pi * 4 / 3))).ToArray()
                ).ToArray();

            var faceCorners = new int[][] {
                //Top face
                new int[] { 0, 5, 10 },
                //Upper band
                new int[] { 0, 1, 2 },
                new int[] { 0, 2, 6 },
                new int[] { 0, 6, 5 },
                new int[] { 5, 6, 7 },
                new int[] { 5, 7, 11 },
                new int[] { 5, 11, 10 },
                new int[] { 10, 11, 12 },
                new int[] { 10, 12, 1 },
                new int[] { 10, 1, 0 },
                //Lower band
                new int[] { 1, 3, 4 },
                new int[] { 1, 4, 2 },
                new int[] { 2, 4, 8 },
                new int[] { 2, 8, 6 },
                new int[] { 6, 8, 9 },
                new int[] { 6, 9, 7 },
                new int[] { 7, 9, 13 },
                new int[] { 7, 13, 11 },
                new int[] { 11, 13, 14 },
                new int[] { 11, 14, 12 },
                new int[] { 12, 14, 3 },
                new int[] { 12, 3, 1 },
                //Bottom face
                new int[] { 8, 4, 3 },
                new int[] { 13, 9, 8 },
                new int[] { 3, 14, 13 },
                new int[] { 3, 13, 8 }
            };

            //Just hardcode the list of edges to inset
            var insetEdges = new int[][] {
                new int[] {0, 1},
                new int[] {1, 2},
                new int[] {2, 0},
                new int[] {1, 3},
                new int[] {3, 4},
                new int[] {4, 2},
                new int[] {0, 5},
                new int[] {2, 6},
                new int[] {4, 8}
            };
            //Copy it 3 times
            var insetEdges1 = insetEdges.Select(ii => ii.Select(i => (i + 5) % 15).ToArray()).ToArray();
            var insetEdges2 = insetEdges.Select(ii => ii.Select(i => (i + 10) % 15).ToArray()).ToArray();
            insetEdges = insetEdges.Concat(insetEdges1).Concat(insetEdges2).ToArray();

            var gemVerticesOuterList = new List<VertexPositionNormalTexture>();
            var gemVerticesInnerList = new List<VertexPositionNormalTexture>();

            foreach (int[] corners in faceCorners)
            {
                float edge0 = GreenGemInsetEdge(corners[0], corners[1], insetEdges);
                float edge1 = GreenGemInsetEdge(corners[1], corners[2], insetEdges);
                float edge2 = GreenGemInsetEdge(corners[2], corners[0], insetEdges);

                gemVerticesOuterList.AddRange(MakeTriangle(gemCorners, corners, new float[] { 0.0F, 0.0F, 0.0F }));
                gemVerticesInnerList.AddRange(MakeTriangle(gemCorners, corners, new float[] { edge0, edge1, edge2 }, 0.02F));
            }

            return new GemModel(device, gemVerticesOuterList, gemVerticesInnerList, Color.Green );
        }

        public float GreenGemInsetEdge(int i0, int i1, int[][] insetEdges)
        {
            //Inset for a thick edge if the vertex pair is in insetEdges
            if (insetEdges.Any(i => i.Contains(i0) && i.Contains(i1)))
                return 0.03F;
            else
                return 0F;
        }

        public GemModel CreateBlueGemModel()
        {
            var gemCorners = new Vector3[] {
                new Vector3(0F, 0.7F, 0.5F),
                new Vector3(0.35F, 0.4F, 0.35F * (float) Math.Sqrt(3))
            };

            //Create 6 rotations of those two vertices
            gemCorners = (from i in Enumerable.Range(0, 6)
                          from v in gemCorners
                         select Vector3.Transform(v, Matrix.CreateRotationY(MathHelper.Pi * i / 3F))).ToArray();

            //Append top-center and bottom-center vertices
            gemCorners = gemCorners.Concat(new Vector3[] {
                new Vector3(0, 0.7F, 0),
                new Vector3(0, -0.7F, 0)
            }).ToArray();

            //Tilt entire gem slightly towards the viewer, so we can see the top face
            gemCorners = gemCorners.Select(v => Vector3.Transform(v, Matrix.CreateRotationX(0.3F))).ToArray();
                
            //Define 4 triangles, then create 6 rotations of them too
            var faceCorners = new int[][] {
                new int[] {12, 0, 2},
                new int[] {0, 1, 2},
                new int[] {1, 3, 2},
                new int[] {1, 13, 3}
            };
            Func<int, int, int> nextStep = (i, n) => (i >= 12 ? i : (i + n * 2) % 12);
            faceCorners = (from n in Enumerable.Range(0, 6)
                          from ints in faceCorners
                          select ints.Select(i => nextStep(i, n)).ToArray()).ToArray();

            var gemVerticesOuterList = new List<VertexPositionNormalTexture>();
            var gemVerticesInnerList = new List<VertexPositionNormalTexture>();

            foreach (int[] corners in faceCorners)
            {
                //Inset all edges, except edges 0 and 2 when vertex0 is 12 (top center)
                float edge0 = corners[0] != 12 ? 0.03F : 0;
                float edge1 = 0.03F;
                float edge2 = corners[0] != 12 ? 0.03F : 0;

                gemVerticesOuterList.AddRange(MakeTriangle(gemCorners, corners, new float[] { 0.0F, 0.0F, 0.0F }));
                gemVerticesInnerList.AddRange(MakeTriangle(gemCorners, corners, new float[] { edge0, edge1, edge2 }, 0.02F));
            }

            return new GemModel(device, gemVerticesOuterList, gemVerticesInnerList, new Color(48, 48, 255));
        }


        //If you have three vertices, V1, V2 and V3, ordered in counterclockwise order, you can obtain the direction of the normal by computing
        //(V2 - V1) x (V3 - V1), where x is the cross product of the two vectors.
        public VertexPositionNormalTexture[] MakeTriangle(Vector3[] v, int[] i, float[] insetFactor, float shiftTowardsNormal = 0.0F)
        {
            Vector3[] newTriangle = InsetTriangle(new Vector3[] { v[i[0]], v[i[1]], v[i[2]] }, insetFactor);
            Vector3 normal = GetNormal(newTriangle[0], newTriangle[1], newTriangle[2]);

            return newTriangle.Select(vec => new VertexPositionNormalTexture(vec + normal * shiftTowardsNormal, normal, Vector2.Zero)).ToArray();
        }

        //Take a triangle whose points are defined by three vectors.
        //Take three inset values, which will apply to sides v0v1, v1v2, and v2v0.
        //Return a new triangle with the three calculated inset vertices.
        public Vector3[] InsetTriangle(Vector3[] v, float[] insets)
        {
            var newv0 = InsetVertex(v[2], v[0], v[1], insets[2], insets[0]);
            var newv1 = InsetVertex(v[0], v[1], v[2], insets[0], insets[1]);
            var newv2 = InsetVertex(v[1], v[2], v[0], insets[1], insets[2]);

            return new Vector3[] { newv0, newv1, newv2 };
        }

        //Take a triangle whose points are defined by three vectors.  Call v0-v1 the trailing side and v2-v1 the leading side.
        //This function calculates a new v1 by insetting the trailing and leading sides by specified amounts.
        public Vector3 InsetVertex(Vector3 v0, Vector3 v1, Vector3 v2, float trailingInset, float leadingInset)
        {
            var TrailingSide = v0 - v1;
            var LeadingSide = v2 - v1;
            var angle = Math.Acos(Vector3.Dot(Vector3.Normalize(TrailingSide), Vector3.Normalize(LeadingSide)));

            var offset1 = Vector3.Normalize(LeadingSide) * (float) (trailingInset / Math.Sin(angle));
            var offset2 = Vector3.Normalize(TrailingSide) * (float) (leadingInset / Math.Sin(angle));

            return v1 + offset1 + offset2;
        }

        public Ring[] CreateRings()
        {
            var rings = new List<Ring>();
            int ringCount = (int)((LASTBEAT - STARTBEAT) * RINGSPERBEAT);

            for (int i = 0; i < ringCount; i++)
            {
                var ring = new Ring();
                ring.gems = CreateGems(i);
                rings.Add(ring);
            }
            return rings.ToArray();  
        }

        public Gem[] CreateGems(int ringNum)
        {
            var gems = new List<Gem>();
            var beat = (float)ringNum / RINGSPERBEAT + STARTBEAT;        //Gems start 0.5 beats before the reference downbeat

            for (int i = 0; i < GEMSPERRING; i++)
            {
                Gem gem = new Gem();
                gem.Model = gemModels[ChooseGemModel(beat, i, GEMSPERRING)];
                gem.Scale = Matrix.CreateScale(1F);
                gem.Rotation = Matrix.CreateRotationY((float) Math.Sin(Math.PI * -beat / 2F) / 2);
                gem.Rotation *= Matrix.CreateRotationZ((float) Math.Sin(Math.PI * beat / 2F));
                var angle = MathHelper.ToRadians(360F / GEMSPERRING * (i + (float) ringNum % 2 / 2));
                gem.Position = new Vector3((float)Math.Cos(angle) * 9, (float)Math.Sin(angle) * 6, GetFarRingDepth(beat));
                gems.Add(gem);
            }
            return gems.ToArray();
        }

        private int ChooseGemModel(float beat, int i, int GEMSPERRING)
        {
            //First plain gem
            if (beat < 8)
                return 0;

            //Cycle through each type for 2 beats each
            if (beat < 16)
                return (((int)beat / 2) + 1) % 4;

            //Stripes
            if (beat < 20)
                return (i / (GEMSPERRING / 8) + 2) % 4;

            //Alternate stripes
            if (beat < 24)
                return (i / (GEMSPERRING / 8)) % 4;

            //Spiral
            return 3 - (int)((beat * 4F - i) / (GEMSPERRING / 8)) % 4;
        }

        private void SetRingCenters()
        {
            for (int i = 0; i < rings.Length; i++)
            {
                float beat = ((float)i / RINGSPERBEAT) + STARTBEAT;
                rings[i].center = GetRingCenter(beat);
            }

            var centers = rings.Select(r => r.center).ToArray();
        }

        private float GetFarRingDepth(float beat)
        {
            return -beat * RINGSPERBEAT * DEPTHPERRING;
        }

        private Vector2 GetRingCenter(float beat)
        {
            return new Vector2(
                (float)Math.Sin(Math.Pow(MathHelper.Max(beat, 0F) * Math.PI / 4F, 1.22)) * Math.Max(beat - 4, 0) / 5F,
                (float)Math.Cos((beat - 1) * Math.PI / 4F) * Math.Max(beat - 8, 0) / 3F
            );
            //Multiplication by beat-4 or beat-8 makes for increasing amplitude during the scene, but zero amplitude for first few beats
            //Y is calibrated so beat 30 yields pi/4 for a fast upwards velocity at exit
        }

        protected override void DrawSegment()
        {
            DrawRarity(Beat);
            DrawRipples(Beat);

            SetBasicEffect();

            foreach (EffectPass pass in basicEffect.CurrentTechnique.Passes)
            {
                RenderRings(pass, Beat);
                //RenderTest(pass, beat);
            }
        }

        private void DrawRarity(float beat)
        {
            //Fade Rarity in and out from beat -1 to 3 to 7
            var color = new Color(new Vector3(1 - Math.Abs(3F - beat) / 4F));

            //Fade Rarity back in from beat 28 to 32
            if (beat > 28)
                color = new Color(new Vector3(beat - 28F) / 4);

            //Calculate scale
            var scale = (float)device.Viewport.Width / (float)rarity.Width * 0.8F;

            //Draw version 6, single scale
            var batch = new SpriteBatch(device);
            batch.Begin();
            batch.Draw(rarity,
                ScreenCenter,
                null,
                color,
                0,
                rarity.Center(),
                scale,
                SpriteEffects.None,
                0);
            batch.End();
        }

        private void DrawRipples(float beat)
        {
            if (beat < 32)
                return;

            //2 ripples per beat starting at 32
            float ripples = (beat - 32) * 2;
            float rippleOffset = ripples % 1;

            //Fade in during beats 32-36
            var color = new Color(new Vector3((beat - 32) / 4));

            var baseScale = (float)device.Viewport.Width / (float)rarity.Width * 0.8F;

            var batch = new SpriteBatch(device);
            batch.Begin(SpriteSortMode.Immediate, BlendState.Additive);
            for (int i = 0; i < ripples; i++)
            {
                batch.Draw(
                    rarityOuter,
                    ScreenCenter,
                    null,
                    color,
                    0,
                    rarityOuter.Center(),
                    baseScale * (1 + (i + rippleOffset) * 1.5F),
                    SpriteEffects.None,
                    0);
            }
            batch.End();

            //Fade to white during beats 36-38
            FadeScreen(36, 38, Beat, false, false);
            if (beat >= 38)
                SmashScreen(1.0F);
        }

        private void SetBasicEffect()
        {
            //spriteBatch changes the DepthStencilState to None and that's why 3D objects don't draw correctly. Other properties get changed too. Check these out:
            device.BlendState = BlendState.Opaque;
            device.DepthStencilState = DepthStencilState.Default;
            device.SamplerStates[0] = SamplerState.LinearWrap;

            SetCullMode(CullMode.CullClockwiseFace);

            basicEffect.Projection = Matrix.CreatePerspectiveFieldOfView(
                MathHelper.ToRadians(30),
                AspectRatio,
                0.01f,
                500.0f);

            basicEffect.TextureEnabled = true;
            basicEffect.LightingEnabled = true;
            basicEffect.AmbientLightColor = new Vector3(0.1f);
            basicEffect.DirectionalLight0.Enabled = true;
            basicEffect.DirectionalLight0.DiffuseColor = Vector3.One;
            basicEffect.DirectionalLight0.SpecularColor = Vector3.One;
            basicEffect.DirectionalLight0.Direction = Vector3.Forward;
            /*
            basicEffect.FogColor = Color.Black.ToVector3();
            basicEffect.FogEnabled = true;
            basicEffect.FogStart = 5;
            basicEffect.FogEnd = 100;
             */
        }
        
        private void RenderRings(EffectPass pass, float beat)
        {
            int farRing = (int)((beat - STARTBEAT) * RINGSPERBEAT);
            if (farRing < 0)
                return;
            if (farRing >= rings.Length)
                farRing = rings.Length - 1;

            var cameraPos = new Vector3(
                GetRingCenter(beat - BEATSOFVISIBILITY * 3 / 4),
                GetFarRingDepth(beat) + BEATSOFVISIBILITY * RINGSPERBEAT * DEPTHPERRING
            );

            basicEffect.View = Matrix.CreateLookAt(cameraPos, Vector3.Add(cameraPos, -Vector3.UnitZ), Vector3.Up);

            var gemTextureOuter = Get1x1Texture(Color.Black);

            for (int i = farRing; i >= 0; i--)
            {
                foreach (Gem gem in rings[i].gems)
                {
                    basicEffect.World = gem.Scale;
                    basicEffect.World *= gem.Rotation;
                    basicEffect.World *= Matrix.CreateTranslation(new Vector3(rings[i].center, 0));
                    basicEffect.World *= Matrix.CreateTranslation(gem.Position);
                    basicEffect.TextureEnabled = true;
                    basicEffect.Texture = gemTextureOuter;
                    pass.Apply();

                    device.SetVertexBuffer(gem.Model.VertexBufferOuter);
                    device.DrawPrimitives(PrimitiveType.TriangleList, 0, gem.Model.VertexBufferOuter.VertexCount / 3);

                    basicEffect.Texture = gem.Model.Texture;
                    pass.Apply();

                    device.SetVertexBuffer(gem.Model.VertexBufferInner);
                    device.DrawPrimitives(PrimitiveType.TriangleList, 0, gem.Model.VertexBufferInner.VertexCount / 3);
                }

                //Stop drawing gems behind camera
                if (rings[i].gems[0].Position.Z > cameraPos.Z)
                    break;
            }
        }

        private void RenderTest(EffectPass pass, float beat)
        {
            var gemTextureOuter = Get1x1Texture(Color.Black);

            basicEffect.View = Matrix.CreateLookAt(new Vector3(0, 0, 5), Vector3.Zero, Vector3.Up);
            var gem = rings[0].gems[0];
            basicEffect.TextureEnabled = true;
            basicEffect.Texture = gemTextureOuter;
            //basicEffect.World = Matrix.CreateRotationY(beat);
            pass.Apply();

            device.SetVertexBuffer(gem.Model.VertexBufferOuter);
            device.DrawPrimitives(PrimitiveType.TriangleList, 0, gem.Model.VertexBufferOuter.VertexCount / 3);

            basicEffect.TextureEnabled = true;
            basicEffect.Texture = gem.Model.Texture;
            //basicEffect.World = Matrix.CreateRotationY(beat);
            pass.Apply();

            device.SetVertexBuffer(gem.Model.VertexBufferInner);
            device.DrawPrimitives(PrimitiveType.TriangleList, 0, gem.Model.VertexBufferInner.VertexCount / 3);
        }
    }

    class GemModel
    {
        public VertexPositionNormalTexture[] VerticesOuter;
        public VertexBuffer VertexBufferOuter;

        public VertexPositionNormalTexture[] VerticesInner;
        public VertexBuffer VertexBufferInner;

        public Texture2D Texture;

        public GemModel(GraphicsDevice device, IEnumerable<VertexPositionNormalTexture> gemVerticesOuterList, IEnumerable<VertexPositionNormalTexture> gemVerticesInnerList, Color color)
        {
            VerticesOuter = gemVerticesOuterList.ToArray();
            VertexBufferOuter = new VertexBuffer(device, typeof(VertexPositionNormalTexture), VerticesOuter.Length, BufferUsage.WriteOnly);
            VertexBufferOuter.SetData<VertexPositionNormalTexture>(VerticesOuter);

            VerticesInner = gemVerticesInnerList.ToArray();
            VertexBufferInner = new VertexBuffer(device, typeof(VertexPositionNormalTexture), VerticesInner.Length, BufferUsage.WriteOnly);
            VertexBufferInner.SetData<VertexPositionNormalTexture>(VerticesInner);

            Texture = new Texture2D(device, 1, 1);
            Texture.SetData<Color>(new Color[] { color });
        }
    }

    class Gem
    {
        public Matrix Scale;
        public Matrix Rotation;
        public Vector3 Position;
        public GemModel Model;
    }

    class Ring
    {
        public Vector2 center = new Vector2(0, 0);
        public Gem[] gems;
    }
}
