using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading;
using Microsoft.Xna.Framework.Graphics;
using Microsoft.Xna.Framework;

namespace SecondRealipony
{
    class Applejack : SRSegment
    {
        BasicEffect basicEffect;
        VertexPositionNormalTexture[] AppleVertices;
        VertexPositionColor[] ColorVertices;
        int[] AppleVertexIndices;
        Vector3[] VertexPositions;
        const float LENSSCALE = 0.45F;

        Texture2D applejack;
        Texture2D blacktexture;

        //Timing: Radial wipes from beat -1 to 2
        //Nothing for 14 beats
        //Lens appears at 14, bounces at 16 then a few more times, falls off bottom at 34
        //"I am not an atomic playboy" 32-36
        //Smash at 36 and start rotating
        //Bottom out around 86, zoom past zero at 94
        //Fade to white and end at 100

        public override float Anacrusis { get { return 2; } }
        public override float EndBeat { get { return 100; } }
        public override string MusicName { get { return "applejack.wav"; } }

        public Applejack(Game game)
            : base(game)
        {
            applejack = game.Content.Load<Texture2D>("applejack.png");
            basicEffect = new BasicEffect(device);
            blacktexture = Get1x1Texture(Color.Black);

            //Do the precalculating asynchronously
            StartupThread = new Thread(new ThreadStart(CreateGeometry));
            StartupThread.Priority = ThreadPriority.Lowest;
            StartupThread.Start();
        }

        private void CreateGeometry()
        {
            //Outline of the left hemisphere of the apple
            var ApplePath = new Vector2[] {
                new Vector2(-0.020000F, 0.700000F),     //top of stem
                new Vector2(-0.010000F, 0.440213F),
                new Vector2(-0.058064F, 0.458075F),
                new Vector2(-0.089738F, 0.47272F),
                new Vector2(-0.124715F, 0.485741F),
                new Vector2(-0.158809F, 0.494292F),
                new Vector2(-0.190711F, 0.498586F),
                new Vector2(-0.22123F, 0.5F),
                new Vector2(-0.247882F, 0.5F),
                new Vector2(-0.277519F, 0.497122F),
                new Vector2(-0.310265F, 0.490708F),
                new Vector2(-0.344814F, 0.481744F),
                new Vector2(-0.376999F, 0.469076F),
                new Vector2(-0.407488F, 0.45381F),
                new Vector2(-0.434271F, 0.437314F),
                new Vector2(-0.459377F, 0.417243F),
                new Vector2(-0.484007F, 0.395206F),
                new Vector2(-0.507048F, 0.369914F),
                new Vector2(-0.526035F, 0.344555F),
                new Vector2(-0.54089F, 0.319227F),
                new Vector2(-0.555707F, 0.290045F),
                new Vector2(-0.569115F, 0.254852F),
                new Vector2(-0.577314F, 0.228198F),
                new Vector2(-0.584468F, 0.19906F),
                new Vector2(-0.588578F, 0.16896F),
                new Vector2(-0.591284F, 0.133185F),
                new Vector2(-0.591284F, 0.098829F),
                new Vector2(-0.589493F, 0.061006F),
                new Vector2(-0.586381F, 0.024448F),
                new Vector2(-0.579149F, -0.016041F),
                new Vector2(-0.568172F, -0.056079F),
                new Vector2(-0.555456F, -0.094218F),
                new Vector2(-0.540547F, -0.132857F),
                new Vector2(-0.521512F, -0.176759F),
                new Vector2(-0.500255F, -0.217859F),
                new Vector2(-0.478427F, -0.254953F),
                new Vector2(-0.45186F, -0.293177F),
                new Vector2(-0.428561F, -0.322585F),
                new Vector2(-0.402263F, -0.353761F),
                new Vector2(-0.373769F, -0.383047F),
                new Vector2(-0.348312F, -0.406423F),
                new Vector2(-0.323264F, -0.42672F),
                new Vector2(-0.285627F, -0.453059F),
                new Vector2(-0.254352F, -0.471896F),
                new Vector2(-0.225644F, -0.486786F),
                new Vector2(-0.193114F, -0.496683F),
                new Vector2(-0.15846F, -0.5F),
                new Vector2(-0.123685F, -0.5F),
                new Vector2(-0.091406F, -0.495908F),
                new Vector2(-0.059537F, -0.486206F),
                new Vector2(-0.029113F, -0.471417F),
                new Vector2(0F, -0.45465F),
            };
            const int LONGITUDES = 41;

            //Take N copies of that to rotate around for the entire half-apple
            VertexPositions = (from i in Enumerable.Range(0, LONGITUDES)
                               from v in ApplePath.Select(v => new Vector3(v, 0))
                               select GetLongitudeTransform(v, i / (float)(LONGITUDES - 1))
                              ).ToArray();

            //For each vertex except the last, create the two triangles between it and the next vertex and this pair of vertices on the next longitude
            var n = ApplePath.Length;
            int[][] TriangleIndices = Enumerable.Range(0, n - 1)
                .SelectMany(i => new int[][] {
                    new int[] { i, i + 1, i + n },
                    new int[] { i + 1, i + n + 1, i + n }
                }).ToArray();
         
            //Explode that to the number of strips (longitudes - 1)
            TriangleIndices = (from l in Enumerable.Range(0, LONGITUDES - 1)
                               from indices in TriangleIndices
                               select indices.Select(i => i + n * l).ToArray())
                              .ToArray();

            //Make a flat list of triangle indices from our jagged array
            AppleVertexIndices = TriangleIndices.SelectMany(i => i).ToArray();

            //Project vertex positions to include texture coordinates
            AppleVertices = CalculateVertexPositionTextures(VertexPositions, TriangleIndices);

            //Set up vertices for the second pass - redraw the polyhedron with VertexPositionColors to produce blue glass
            ColorVertices = AppleVertices.Select(vpt =>
                new VertexPositionColor(
                    vpt.Position,
                    Color.Blue
                    )).ToArray();
        }

        //Transform a single applepath vertex around the apple globe, by doing a longitudal revolution of specified percentage (100% = half apple)
        private Vector3 GetLongitudeTransform(Vector3 v, float percent)
        {
            var result = Vector3.Transform(v,
                Matrix.CreateRotationY(percent * MathHelper.Pi)         //Revolve around Y axis
                * Matrix.CreateScale(1 + percent * 0.10F)               //Lopsided apple, right half is bigger
                * Matrix.CreateScale(1, 1, 0.9F)                        //Apple lens is slightly flattened not truly spheroid
                );

            //fix stem
            if (result.Y > 0.7)
                result.Y = 0.7F;

            //problem: the dimple can be obscured by vertices near the meridian.
            //solution: compress Y near the meridian
            //xFromMeridian ranges from about -0.5 to 0.5

            var xFromMeridian = Math.Abs(result.X);
            var yFromEquator = Math.Abs(result.Y);

            var compression = 0.90F + xFromMeridian * 0.2F;
            
            //TODO not sure if I want this
            //result.Y *= compression;

            return result;
        }

        //Project vertex positions to include texture coordinates.  This creates them relative to the center of the texture at (0.5, 0.5).
        //At runtime each frame, a new set is created by translating these texture coordinates by the coordinates of the lens center.
        //The correct algorithm would be to treat each triangle as forming a prism along with the rear of the lens at Z=0.  But that (correctly) gives different texture coordinates for each triangle that a vertex belongs to, creating a faceted look.  I want smooth.
        //This algorithm is fake, instead giving each vertex just a single texture coordinate, so it looks like a smooth curved lens rather than a discrete polygonal prism.
        //1. For each vertex, find all triangles it belongs to
        //2. Get the normals of these triangles
        //3. Sum those normals to get their average
        //4. Get midpoint between that average and up (Z) - simulating a prism with some refractive index (is it 1.5, 2.0? not sure)
        //5. Project a ray from the vertex opposite to that midpoint, and find its intercept on the plane z=0
        private VertexPositionNormalTexture[] CalculateVertexPositionTextures(Vector3[] vertexPositions, int[][] TriangleIndices)
        {
            var results = new List<VertexPositionNormalTexture>();

            for (int i = 0; i < vertexPositions.Length; i++)
            {
                //Get relevant triangles
                var ContainingTriangles = TriangleIndices.Where(tri => tri.Contains(i));

                //Get their normals
                var Normals = ContainingTriangles.Select(tri => GetNormal(vertexPositions[tri[0]], vertexPositions[tri[1]], vertexPositions[tri[2]]));
                //Patch any degenerate normals
                Normals = Normals.Select(v => float.IsNaN(v.X) ? Vector3.UnitZ : v).ToArray();

                //Sum them
                var NormalSum = Normals.Aggregate((v0, v1) => v0 + v1);

                //Normalize the sum and add UnitZ to get the midpoint between that and up
                var Midpoint = Vector3.Normalize(NormalSum) + Vector3.UnitZ;

                //Create a ray projecting opposite that midpoint
                Ray ray = new Ray(vertexPositions[i], -Midpoint);

                //Find where that ray intercepts the plane at z=0
                Plane plane = new Plane(Vector3.UnitZ, 0);
                float? distance = ray.Intersects(plane);
                Vector3 intercept = distance != null
                    ? ray.Position + ray.Direction * (float)distance                //z should always be 0
                    : new Vector3(vertexPositions[i].X, vertexPositions[i].Y, 0);   //if there was no intercept, just take the XY coordinates of the vertex

                //Transform the intercept in 3d space to texture space: flatten to vector2, scale X by AspectRatio, invert Y, scale by LENSSCALE, and add (0.5, 0.5) to center it within the 2d texture
                Vector2 textureCoordinate = new Vector2(intercept.X / AspectRatio, -intercept.Y) * LENSSCALE + Vector2.One * 0.5F;

                //Create the final VertexPositionTexture
                results.Add(new VertexPositionNormalTexture(vertexPositions[i], NormalSum, textureCoordinate));
            }

            return results.ToArray();
        }


        private void SetBasicEffect()
        {
            device.DepthStencilState = DepthStencilState.Default;

            SetCullMode(CullMode.CullClockwiseFace);

            Vector3 CameraTarget = Vector3.Zero;
            Vector3 CameraPos = Vector3.UnitZ * 5;
            basicEffect.View = Matrix.CreateLookAt(CameraPos, CameraTarget, Vector3.Up);

            //Projection matrix is set up as follows.
            //The plane with Applejack is at Z=0.  On this plane, Y ranges from -0.5 at the top of the screen to +0.5 at the bottom.
            //Then X ranges from -0.5 * AspectRatio to 0.5 * AspectRatio.
            basicEffect.Projection = Matrix.CreatePerspective(AspectRatio / 2, 0.5F, 2.5F, 5.0F);
        }
        
        protected override void DrawSegment()
        {
            DrawApplejack();
            DrawRadialWipes();

            FadeScreen(36, 37, Beat, false, true);
            FadeScreen(EndBeat - 3, EndBeat, Beat, false, false);

            SetBasicEffect();
            if (Beat >= 13 && Beat < 36)
                DrawLens();
        }

        private void DrawApplejack()
        {
            //Draw version 6, single scale, including source rectangle
            //Screen origin is (0, height/2)
            //Texture origin is also generally (0, height/2), but during zoom in, drifts to top-center of the next copy to the left (-X / 2, Y / 4), then returns
            //(Second Reality cheats a LOT on positioning this!)

            device.SamplerStates[0] = SamplerState.LinearWrap;

            var batch = new SpriteBatch(device);
            var ScreenOrigin = new Vector2(0, ScreenCenter.Y);

            var SourceRectangle = new Rectangle(0, 0, applejack.Width * (Beat < 36 ? 1 : 32), applejack.Height * (Beat < 36 ? 1 : 32));
            var TextureOrigin = new Vector2(Beat < 36 ? 0 : applejack.Width * 16, Beat < 36 ? applejack.Height * 0.5F : applejack.Height * 16.5F);

            SortedDictionary<float, float> phases = new SortedDictionary<float, float>();
            phases.Add(-Anacrusis, 0);
            phases.Add(36, 0);
            phases.Add(46, -0.5F);
            phases.Add(60, 0);
            phases.Add(EndBeat, 0);

            var TextureOriginOffset = SmoothDictionaryLookup(phases, Beat);
            TextureOrigin += new Vector2(TextureOriginOffset * applejack.Width, TextureOriginOffset * applejack.Height / 2);

            batch.Begin(SpriteSortMode.Immediate, BlendState.Opaque, SamplerState.LinearWrap, DepthStencilState.None, RasterizerState.CullNone);
            batch.Draw(applejack, ScreenOrigin, SourceRectangle, Color.White, GetRotation(Beat), TextureOrigin, GetScale(Beat), SpriteEffects.None, 0);
            batch.End();
        }

        private void DrawRadialWipes()
        {
            float StartBeat = -Anacrusis + 1;
            const float EndBeat = 2;

            if (Beat >= EndBeat)
                return;

            var WipePercent = (Beat - StartBeat) / (EndBeat - StartBeat);

            var startingAngle0 = (float) Math.Atan2(device.Viewport.Width, device.Viewport.Height);
            var currentAngle0 = MathHelper.Lerp(startingAngle0, 0, WipePercent);
            
            var startingAngle1 = (float) Math.Atan2(-device.Viewport.Height, device.Viewport.Width);
            var currentAngle1 = MathHelper.Lerp(startingAngle1, -MathHelper.PiOver2, WipePercent);

            //Draw version 6, single scale
            var batch = new SpriteBatch(device);
            batch.Begin();

            //The square anchored at upper right
            batch.Draw(blacktexture, new Vector2(device.Viewport.Width, 0), null, Color.White, currentAngle0, Vector2.Zero, ScreenHypotenuse, SpriteEffects.None, 0);

            //The square anchored at lower left
            batch.Draw(blacktexture, new Vector2(0, device.Viewport.Height), null, Color.White, currentAngle1, new Vector2(0, 1), ScreenHypotenuse, SpriteEffects.None, 0);
            batch.End();
        }

        private float GetScale(float beat)
        {
            var ScreenScale = Math.Min((float)device.Viewport.Width / applejack.Width, (float)device.Viewport.Height / applejack.Height);

            return ScreenScale / Math.Abs(GetDistance(beat));
        }

        private float GetDistance(float beat)
        {
            SortedDictionary<float, float> phases = new SortedDictionary<float, float>();
            phases.Add(-Anacrusis, 1);
            phases.Add(36, 1);
            phases.Add(46, 0.4F);
            phases.Add(60, 0.75F);
            phases.Add(83.5F, 7);
            phases.Add(EndBeat, -5);

            return SmoothDictionaryLookup(phases, beat);
        }

        private float GetRotation(float beat)
        {
            float rotateBeat = Beat - 36;

            if (rotateBeat < 0)
                return 0;

            //Accelerate from zero for first beats of rotation
            float skewBeat = GetSkewBeat(rotateBeat, 10);

            return -(float) Math.Pow(skewBeat / 6, 1.4);
        }

        private void DrawLens()
        {
            device.SamplerStates[0] = SamplerState.LinearClamp;

            foreach (EffectPass pass in basicEffect.CurrentTechnique.Passes)
            {
                device.BlendState = BlendState.Opaque;
                basicEffect.VertexColorEnabled = false;
                basicEffect.TextureEnabled = true;
                basicEffect.Texture = applejack;
                basicEffect.LightingEnabled = true;
                basicEffect.AmbientLightColor = Vector3.One;
                basicEffect.DirectionalLight0.Enabled = true;
                basicEffect.DirectionalLight0.SpecularColor = new Vector3(0.75F);
                basicEffect.DirectionalLight0.Direction = new Vector3(-1, -1, -0.5F);
                basicEffect.DirectionalLight1.Enabled = true;
                basicEffect.DirectionalLight1.SpecularColor = new Vector3(0.4F);
                basicEffect.DirectionalLight1.Direction = new Vector3(1, 1, -0.5F);

                Vector3 LensCenterIn3DSpace = new Vector3(GetLensCenter(Beat), 0);
                Vector2 LensOffsetInTextureSpace = new Vector2(LensCenterIn3DSpace.X / AspectRatio, -LensCenterIn3DSpace.Y);

                //Translate the texture coordinates to incorporate where the lens center currently is
                VertexPositionNormalTexture[] newVertices = AppleVertices.Select(
                    v => new VertexPositionNormalTexture(v.Position, v.Normal, v.TextureCoordinate + LensOffsetInTextureSpace)
                    ).ToArray();

                basicEffect.World = Matrix.CreateScale(LENSSCALE) * Matrix.CreateTranslation(LensCenterIn3DSpace);

                pass.Apply();
                device.DrawUserIndexedPrimitives<VertexPositionNormalTexture>(
                    PrimitiveType.TriangleList,
                    newVertices,
                    0,
                    newVertices.Length,
                    AppleVertexIndices,
                    0,
                    AppleVertexIndices.Length / 3
                    );

                //Redraw the polyhedron with only vertex colors, to create the blue glass
                basicEffect.LightingEnabled = false;
                basicEffect.TextureEnabled = false;
                basicEffect.VertexColorEnabled = true;
                basicEffect.Alpha = 0.75F;
                device.BlendState = BlendState.Additive;
                pass.Apply();

                device.DrawUserIndexedPrimitives<VertexPositionColor>(
                    PrimitiveType.TriangleList,
                    ColorVertices,
                    0,
                    ColorVertices.Length,
                    AppleVertexIndices,
                    0,
                    AppleVertexIndices.Length / 3
                    );
            }
        }

        //Define and calculate center of the apple lens.  In 3D space.  screen center is (0,0).  corners range from (-0.5*AspectRatio,-0.5) to (0.5*AspectRatio, 0.5)
        private Vector2 GetLensCenter(float beat)
        {
            float bounceBeat = 16F;
            float launchOffset = -3.15F;
            float floor = -0.5F + LENSSCALE / 2;
            float startAltitude = 0.5F - LENSSCALE / 4F;                                    //faked, this should be higher up offscreen, but then it bounces too high
            float accel = (floor - startAltitude) * 2 / (launchOffset * launchOffset);      //calculate acceleration at launch to bounce precisely at bounceBeat

            float x = GetTriangleWave((beat - bounceBeat), 0.5F - LENSSCALE / 2, 13.5F, 0.20F) * AspectRatio;
            float y = GetBouncedCoordinate((beat - (bounceBeat + launchOffset)), startAltitude, 0, accel, 0.87F, floor, 4);

            //before first bounce, double its altitude from floor, to make it fall in faster
            if (beat < 16)
                y += y - floor;

            return new Vector2(x, y);
        }

        //Unused - used this version of the lens for testing before doing the real apple model
        private void AppleLensTestModel()
        {
            var s = (float)Math.Sqrt(2) / 2;

            var VertexPositionsOneQuadrant = new Vector3[] {
                new Vector3(0.5F, 0, 0.75F),
                new Vector3(1.0F, 0, 0),
                new Vector3(s, s, 0)
            };

            VertexPositions = (from quadrant in Enumerable.Range(0, 4)
                               from vertex in VertexPositionsOneQuadrant
                               select Vector3.Transform(vertex, Matrix.CreateRotationZ(quadrant * MathHelper.PiOver2))
                               ).ToArray();

            var TriangleIndices = new int[][] {
                new int[] {0, 3, 6},
                new int[] {0, 6, 9},
                new int[] {0, 1, 2},
                new int[] {0, 2, 3},
                new int[] {3, 2, 4},
                new int[] {3, 4, 5},
                new int[] {3, 5, 6},
                new int[] {6, 5, 7},
                new int[] {6, 7, 8},
                new int[] {6, 8, 9},
                new int[] {9, 8,10},
                new int[] {9,10,11},
                new int[] {9,11, 0},
                new int[] {0,11, 1}
            };
        }
    }
}
