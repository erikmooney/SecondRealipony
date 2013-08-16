using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using Microsoft.Xna.Framework.Graphics;
using Microsoft.Xna.Framework;

namespace SecondRealipony
{
    class Twilight : SRSegment
    {
        public override float EndBeat { get { return 78; } }
        public override string MusicName { get { return "twilight.wav"; } }
        public override double MusicCue { get { return 59.6333333; } }

        BasicEffect basicEffect;
        AlphaTestEffect alphaTestEffect;        //need this because the scrolls have small transparent notches
        Texture2D chessboard;
        Texture2D books;
        Texture2D twilight;
        Texture2D spike;
        Texture2D scroll;

        VertexBuffer boardVertexBuffer;
        VertexPositionNormalTexture[] oneBookVertices;
        VertexPositionNormalTexture[] bookVertices;
        VertexBuffer bookVertexBuffer;
        VertexBuffer scrollVertexBuffer;

        class BookDef
        {
            public Matrix matrix;
            public int model;
        };
        BookDef[] BookDefs;

        //Radius of bookahedron
        const float RADIUS = 0.5F;
        const float BOOKSCALE = RADIUS * 5;

        public Twilight(Game game)
            : base(game)
        {
            basicEffect = new BasicEffect(device);
            alphaTestEffect = new AlphaTestEffect(device);
            chessboard = game.Content.Load<Texture2D>("chessboard.png");
            books = game.Content.Load<Texture2D>("books.png");
            twilight = game.Content.Load<Texture2D>("twilight.png");
            spike = game.Content.Load<Texture2D>("spike.png");
            scroll = game.Content.Load<Texture2D>("scroll.png");
            CreateGeometry();
        }

        protected void CreateGeometry()
        {
            CreateBoard();
            CreateBookModels();
            CreateBookDefs();
            CreateScrollModel();
        }

        private void CreateBoard()
        {
            var BoardVertices = new VertexPositionTexture[] {
                new VertexPositionTexture(new Vector3(-AspectRatio, 0, -2.0F), new Vector2(0, 0)),
                new VertexPositionTexture(new Vector3(-AspectRatio, 0, 2.0F), new Vector2(0, 1)),
                new VertexPositionTexture(new Vector3(AspectRatio, 0, -2.0F), new Vector2(1, 0)),
                new VertexPositionTexture(new Vector3(AspectRatio, 0, 2.0F), new Vector2(1, 1))
            };

            boardVertexBuffer = new VertexBuffer(device, typeof(VertexPositionTexture), BoardVertices.Length, BufferUsage.WriteOnly);
            boardVertexBuffer.SetData(BoardVertices);
        }

        //remember: in 3d space, Y is positive upwards
        //in texture space, Y is positive downwards
        private void CreateBookModels()
        {
            //texture is 1000 x 1000, texture space correlates exactly to model space, 0.001 unit is one texel
            oneBookVertices = new VertexPositionNormalTexture[][] {
                //Spine
                CreateNormalTextureQuad(new Vector3(-0.050F, 0.080F,-0.020F), new Vector3( 0.000F,-0.160F, 0.000F), new Vector3( 0.000F, 0.000F, 0.040F), new Vector2(0.000F, 0.000F), new Vector2(0.040F, 0.160F)),
                //Cover
                CreateNormalTextureQuad(new Vector3(-0.050F, 0.080F, 0.020F), new Vector3( 0.000F,-0.160F, 0.000F), new Vector3( 0.100F, 0.000F, 0.000F), new Vector2(0.040F, 0.000F), new Vector2(0.100F, 0.160F)),
                //Cover outer edge
                CreateNormalTextureQuad(new Vector3( 0.050F, 0.080F, 0.020F), new Vector3( 0.000F,-0.160F, 0.000F), new Vector3( 0.000F, 0.000F,-0.010F), new Vector2(0.140F, 0.000F), new Vector2(0.010F, 0.160F)),
                //Inside cover
                CreateNormalTextureQuad(new Vector3( 0.050F, 0.080F, 0.010F), new Vector3( 0.000F,-0.160F, 0.000F), new Vector3(-0.090F, 0.000F, 0.000F), new Vector2(0.150F, 0.000F), new Vector2(0.090F, 0.160F)),
                //Inside spine
                CreateNormalTextureQuad(new Vector3(-0.040F, 0.080F, 0.010F), new Vector3( 0.000F,-0.160F, 0.000F), new Vector3( 0.000F, 0.000F,-0.020F), new Vector2(0.240F, 0.000F), new Vector2(0.020F, 0.160F)),
                //Inside back cover
                CreateNormalTextureQuad(new Vector3(-0.040F, 0.080F,-0.010F), new Vector3( 0.000F,-0.160F, 0.000F), new Vector3( 0.090F, 0.000F, 0.000F), new Vector2(0.260F, 0.000F), new Vector2(0.090F, 0.160F)),
                //Back cover outer edge
                CreateNormalTextureQuad(new Vector3( 0.050F, 0.080F,-0.010F), new Vector3( 0.000F,-0.160F, 0.000F), new Vector3( 0.000F, 0.000F,-0.010F), new Vector2(0.350F, 0.000F), new Vector2(0.010F, 0.160F)),
                //Back cover
                CreateNormalTextureQuad(new Vector3( 0.050F, 0.080F,-0.020F), new Vector3( 0.000F,-0.160F, 0.000F), new Vector3(-0.100F, 0.000F, 0.000F), new Vector2(0.360F, 0.000F), new Vector2(0.100F, 0.160F)),

                //Top of book - back cover
                CreateNormalTextureQuad(new Vector3(-0.050F, 0.080F,-0.020F), new Vector3( 0.000F, 0.000F, 0.010F), new Vector3( 0.100F, 0.000F, 0.000F), new Vector2(0.460F, 0.000F), new Vector2(0.100F, 0.010F)),
                //Top of book - spine
                CreateNormalTextureQuad(new Vector3(-0.050F, 0.080F,-0.010F), new Vector3( 0.000F, 0.000F, 0.020F), new Vector3( 0.010F, 0.000F, 0.000F), new Vector2(0.460F, 0.010F), new Vector2(0.010F, 0.020F)),
                //Top of book - front cover
                CreateNormalTextureQuad(new Vector3(-0.050F, 0.080F, 0.010F), new Vector3( 0.000F, 0.000F, 0.010F), new Vector3( 0.100F, 0.000F, 0.000F), new Vector2(0.460F, 0.030F), new Vector2(0.100F, 0.010F)),

                //Bottom of book - front cover
                CreateNormalTextureQuad(new Vector3(-0.050F,-0.080F, 0.020F), new Vector3( 0.000F, 0.000F,-0.010F), new Vector3( 0.100F, 0.000F, 0.000F), new Vector2(0.460F, 0.000F), new Vector2(0.100F, 0.010F)),
                //Bottom of book - spine
                CreateNormalTextureQuad(new Vector3(-0.050F,-0.080F, 0.010F), new Vector3( 0.000F, 0.000F,-0.020F), new Vector3( 0.010F, 0.000F, 0.000F), new Vector2(0.460F, 0.010F), new Vector2(0.010F, 0.020F)),
                //Bottom of book - back cover
                CreateNormalTextureQuad(new Vector3(-0.050F,-0.080F,-0.010F), new Vector3( 0.000F, 0.000F,-0.010F), new Vector3( 0.100F, 0.000F, 0.000F), new Vector2(0.460F, 0.030F), new Vector2(0.100F, 0.010F)),

                //Top of pages
                CreateNormalTextureQuad(new Vector3(-0.040F, 0.075F,-0.010F), new Vector3( 0.000F, 0.000F, 0.020F), new Vector3( 0.085F, 0.000F, 0.000F), new Vector2(0.680F, 0.000F), new Vector2(0.085F, 0.020F)),
                //Outer edge of pages
                CreateNormalTextureQuad(new Vector3( 0.045F, 0.075F,-0.010F), new Vector3( 0.000F, 0.000F, 0.020F), new Vector3( 0.000F,-0.150F, 0.000F), new Vector2(0.765F, 0.000F), new Vector2(0.150F, 0.020F)),
                //Bottom of pages
                CreateNormalTextureQuad(new Vector3( 0.045F,-0.075F,-0.010F), new Vector3( 0.000F, 0.000F, 0.020F), new Vector3(-0.085F, 0.000F, 0.000F), new Vector2(0.915F, 0.000F), new Vector2(0.085F, 0.020F)),


            }.SelectMany(v => v).ToArray();



            //Copy for 4 types of books, adding 0.200 to Y texture coordinate for each
            bookVertices = (from i in Enumerable.Range(0, 4)
                            from v in oneBookVertices
                            select new VertexPositionNormalTexture(v.Position, v.Normal, v.TextureCoordinate + new Vector2(0, 0.200F * i))
                           ).ToArray();

            bookVertexBuffer = new VertexBuffer(device, typeof(VertexPositionNormalTexture), bookVertices.Length, BufferUsage.WriteOnly);
            bookVertexBuffer.SetData(bookVertices);
        }

        //Define a normaled textured quad, as a triangle list.  Defined by the following parameters:
        //Upper-left corner
        //Offset of bottom-left corner
        //Offset of top-right corner (We add both offsets to get the fourth vertex)
        //Texture coordinates of upper-left corner
        //Texture offsets to lower-right corner
        private VertexPositionNormalTexture[] CreateNormalTextureQuad(Vector3 upperLeft, Vector3 offsetDown, Vector3 offsetRight, Vector2 textureStart, Vector2 textureOffsets)
        {
            var results = new VertexPositionNormalTexture[6];

            results[0].Position = upperLeft;
            results[0].TextureCoordinate = textureStart;

            results[1].Position = results[4].Position = upperLeft + offsetDown;
            results[1].TextureCoordinate = results[4].TextureCoordinate = textureStart + textureOffsets * Vector2.UnitY;

            results[2].Position = results[3].Position = upperLeft + offsetRight;
            results[2].TextureCoordinate = results[3].TextureCoordinate = textureStart + textureOffsets * Vector2.UnitX;

            results[5].Position = upperLeft + offsetDown + offsetRight;
            results[5].TextureCoordinate = textureStart + textureOffsets;

            var normal = GetNormal(results[0].Position, results[1].Position, results[2].Position);
            for (int i = 0; i < results.Length; i++)
                results[i].Normal = normal;

            return results;
        }

        private void CreateBookDefs()
        {
            //brown, green, blue, red
            int[] bookModels = new int[]
            {
                0, 1, 2, 3,
                3, 0, 2, 1,
                1, 0, 3, 2,
                1, 2, 0, 3,
                2, 0, 1, 3,
                0, 2, 3, 1
            };

            List<Matrix> matrices = new List<Matrix>();

            Matrix[] poleMatrices = new Matrix[] {
                //Front pole
                Matrix.Identity,
                //Right pole
                Matrix.CreateRotationZ(MathHelper.PiOver2) * Matrix.CreateRotationY(MathHelper.PiOver2),
                //Back pole
                Matrix.CreateRotationZ(MathHelper.Pi) * Matrix.CreateRotationY(MathHelper.Pi),
                //Left pole
                Matrix.CreateRotationZ(-MathHelper.PiOver2) * Matrix.CreateRotationY(-MathHelper.PiOver2),
                //Top pole
                Matrix.CreateRotationZ(MathHelper.PiOver2) * Matrix.CreateRotationX(-MathHelper.PiOver2),
                //Bottom pole
                Matrix.CreateRotationZ(-MathHelper.PiOver2) * Matrix.CreateRotationX(MathHelper.PiOver2),
            };

            for (int i = 0; i < bookModels.Length; i++)
            {
                var matrix = Matrix.CreateScale(BOOKSCALE);

                //Rotate books into four classes of 90° increments
                matrix *= Matrix.CreateRotationZ((i % 4) * MathHelper.PiOver2);

                //Translate it towards viewer
                matrix *= Matrix.CreateTranslation(0, 0, RADIUS);

                //Put first 4 books arranged around front pole
                Vector3 axisangle = Vector3.Transform(new Vector3(-1, 1, 0), Matrix.CreateRotationZ((i % 4) * MathHelper.PiOver2));
                matrix *= Matrix.CreateFromAxisAngle(axisangle, MathHelper.TwoPi / 16F);

                //Move to one of the poles
                matrix *= poleMatrices[i / 4];

                matrices.Add(matrix);
            }

            BookDefs = Enumerable.Zip(matrices, bookModels, (m, i) => new BookDef() { matrix = m, model = i }).ToArray();

        }

        private void CreateScrollModel()
        {
            //Contour of bottom half of the scroll
            var contour = new Vector3[] {
                new Vector3(0,  0.000000F,  0.000000F),
                new Vector3(0, -0.015507F,  0.008316F),
                new Vector3(0, -0.029162F,  0.014086F),
                new Vector3(0, -0.041047F,  0.017580F),
                new Vector3(0, -0.051240F,  0.019065F),
                new Vector3(0, -0.059825F,  0.018810F),
                new Vector3(0, -0.066881F,  0.017083F),
                new Vector3(0, -0.072490F,  0.014152F),
                new Vector3(0, -0.076732F,  0.010285F),
                new Vector3(0, -0.079689F,  0.005751F),
                new Vector3(0, -0.081440F,  0.000817F),
                new Vector3(0, -0.082068F, -0.004247F),
                new Vector3(0, -0.081653F, -0.009175F),
                new Vector3(0, -0.080275F, -0.013696F),
                new Vector3(0, -0.078016F, -0.017545F),
                new Vector3(0, -0.074957F, -0.020451F),
                new Vector3(0, -0.071178F, -0.022147F)
            };

            //Create full contour by prepending a reversed list of all but the first vertex, and flip Y and Z signs
            var scrollVertexPositions = contour.Skip(1).Reverse().Select(v => new Vector3(v.X, -v.Y, -v.Z)).Concat(contour).ToArray();
            
            //Reverse list because we need the triangle strip from the bottom up for triangles to be counterclockwise;
            //From each vertex, create two VertexPositionTextures, at each X edge
            var scrollVerticesFront = scrollVertexPositions.Reverse().SelectMany((v, i) =>
                new VertexPositionTexture[] {
                    new VertexPositionTexture(new Vector3(-0.050F, v.Y, v.Z), new Vector2(0, (float) i / (scrollVertexPositions.Length - 1))),
                    new VertexPositionTexture(new Vector3( 0.050F, v.Y, v.Z), new Vector2(0.4975F, (float) i / (scrollVertexPositions.Length - 1)))
                }).ToArray();

            //Project front of scroll to create back of scroll
            //Just flip X and adjust texture coordinate
            var scrollVerticesBack = scrollVerticesFront.Select(v =>
                new VertexPositionTexture(v.Position * new Vector3(-1, 1, 1), v.TextureCoordinate + new Vector2(0.5F, 0))
                ).ToArray();

            //Link them together, duplicating last and first vertices for triangle strip discontinuity
            var scrollVertices =
                scrollVerticesFront
                .Concat(new[] { scrollVerticesFront.Last() })
                .Concat(new[] { scrollVerticesBack.First() })
                .Concat(scrollVerticesBack)
                .ToArray();

            scrollVertexBuffer = new VertexBuffer(device, typeof(VertexPositionTexture), scrollVertices.Length, BufferUsage.WriteOnly);
            scrollVertexBuffer.SetData(scrollVertices);
        }

        private void SetBasicEffect()
        {
            device.BlendState = BlendState.AlphaBlend;
            device.DepthStencilState = DepthStencilState.Default;
            device.SamplerStates[0] = SamplerState.LinearClamp;
            SetCullMode(CullMode.CullClockwiseFace);

            basicEffect.TextureEnabled = true;
            SetCamera(basicEffect);
        }

        private void SetAlphaTestEffect()
        {
            alphaTestEffect.AlphaFunction = CompareFunction.Greater;
            SetCamera(alphaTestEffect);
        }

        private void SetCamera(IEffectMatrices effect)
        {
            //from beats 37 to 40, after chessboard fades, camera altitude and target lerp to 0
            var cameraY = MathHelper.SmoothStep(1, 0, (Beat - 37) / 3F);
            effect.View = Matrix.CreateLookAt(new Vector3(0, cameraY, 8F), new Vector3(0, cameraY, 0), Vector3.Up);
            effect.Projection = Matrix.CreatePerspective(AspectRatio, 1, 3F, 100);
        }

        protected override void DrawSegment()
        {
            SetBasicEffect();
            SetAlphaTestEffect();

            if (Beat < 37)
                DrawBoard();

            DrawBooks();
            DrawScrolls();
            DrawTwilight();
            DrawSpike();

            FadeScreen(76, 78F, Beat, true, false);
        }

        protected void DrawBoard()
        {
            basicEffect.World = Matrix.CreateTranslation(new Vector3(0, GetBoardY(Beat), 0));
            basicEffect.Texture = chessboard;
            float fadePercent = MathHelper.Clamp((Beat - 34) / 3, 0, 1);
            basicEffect.Alpha = MathHelper.Lerp(1, 0, fadePercent);
            basicEffect.LightingEnabled = false;

            foreach (EffectPass pass in basicEffect.CurrentTechnique.Passes)
            {
                pass.Apply();

                device.SetVertexBuffer(boardVertexBuffer);
                device.DrawPrimitives(PrimitiveType.TriangleStrip, 0, boardVertexBuffer.VertexCount / 3 + 1);
            }
        }

        protected float GetBoardY(float beat)
        {
            return GetBouncedCoordinate(beat, 1, 0, -0.5F, 0.6F, 0, -1);
        }


        protected void DrawBooks()
        {
            basicEffect.Alpha = 1;
            basicEffect.Texture = books;
            basicEffect.LightingEnabled = true;
            basicEffect.AmbientLightColor = Vector3.One * 0.6F;
            basicEffect.DirectionalLight0.Enabled = true;
            basicEffect.DirectionalLight0.SpecularColor = Vector3.Zero;
            basicEffect.DirectionalLight0.DiffuseColor = Vector3.One * 0.6F;
            basicEffect.DirectionalLight0.Direction = Vector3.Forward;
            

            device.SetVertexBuffer(bookVertexBuffer);

            foreach (EffectPass pass in basicEffect.CurrentTechnique.Passes)
            {
                for (int i = 0; i < BookDefs.Length; i++)
                {
                    basicEffect.World = GetBookMatrix(i, Beat);

                    pass.Apply();
                    device.DrawPrimitives(PrimitiveType.TriangleList, BookDefs[i].model * oneBookVertices.Length, oneBookVertices.Length / 3);
                }
            }
        }

        protected Matrix GetBookMatrix(int i, float beat)
        {
            Matrix matrix = BookDefs[i].matrix;

            //Global rotation
            matrix *= Matrix.CreateFromYawPitchRoll(beat, (float)Math.Sin(beat * 0.5F), 0);

            //Squish factor
            float squishFactor = GetSquishFactor(beat);
            matrix *= Matrix.CreateScale(1 / squishFactor, squishFactor, 1 / squishFactor);

            //While still bouncing, dislodge books upwards at low end (don't sink into the floor)
            if (beat < 36)
            {
                var y = Vector3.Transform(Vector3.Zero, matrix).Y + GetBookahedronCenter(beat).Y;
                if (y < 0.50 * RADIUS)
                    matrix *= Matrix.CreateTranslation(0, 0.50F * RADIUS - y, 0);
            }

            //Move into world space (first bounce along Y axis, then oscillate)
            matrix *= Matrix.CreateTranslation(GetBookahedronCenter(beat));

            return matrix;
        }

        //This function is in charge of the timeline for the bookahedron center.
        protected Vector3 GetBookahedronCenter(float beat)
        {
            //before it appears, bury it offscreen beyond the far plane
            if (beat < 12)
                return new Vector3(0, 0, -1000);

            float y = GetBookahedronBounceY(beat);

            //before first bounce at beat 16, add some to altitude, so it falls in faster from offscreen
            if (beat < 16)
                y += (16 - beat) / 3F;

            //after chessboard fades, lerp to 0 (camera also moves now, so that screen center is at y=0)
            if (beat < 40)
                return new Vector3(0, MathHelper.SmoothStep(y, 0F, (beat - 37) / 3F), 0);

            //start oscillating in all three axes
            float skewBeat = GetSkewBeat(beat - 40, 4);
            return new Vector3(
                0.9F * (float)Math.Sin(skewBeat / 10 * MathHelper.TwoPi),
                -0.7F * (float)Math.Sin(skewBeat / 8 * MathHelper.TwoPi),
                0.5F * (float)Math.Sin(skewBeat / 6.5F * MathHelper.TwoPi) * MathHelper.Clamp((beat - 68) * 3, 1, 8)   //suddenly at beat 68 we oscillate more in Z
                );
        }

        protected float GetBookahedronBounceY(float beat)
        {
            //Acceleration is 1/4 of velocity, which makes for 8 beats for a full bounce period (vt + ½at² = 0, equivalently v = -½at)
            float accel = -4F / 16F;
            float vel = 16F / 16F;

            //Calculate from zeroth bounce cycle starting on beat 8, so first real bounce will be beat 16
            float y = GetBouncedCoordinate((beat - 8), 0, vel, accel, 1.00F, 0, 10);

            //Correct for squish zone
            if (y < 0.75F * RADIUS)
            {
                float squishFactor = GetSquishFactor(beat);
                y = 0.75F * RADIUS * squishFactor;
            }

            //fudge upwards just slightly to account for the height of the book model lying horizontally
            y += 0.005F * BOOKSCALE;

            return y;
        }

        //This function calculates squishing the bookahedron
        //Squish factor multiplies the height, so is minimum of 0.5F at the bounce instant
        protected float GetSquishFactor(float beat)
        {
            float squishTime = GetSquishTime(-4 / 16F, 16F / 16F);

            //No squishing before first contact or after fourth contact
            if (beat < 16 - squishTime || beat >= 40 - squishTime)
                return 1;

            float cycleLength = 4 * squishTime;
            float previousContact = (float)Math.Floor((beat + squishTime) / 8) * 8 - squishTime;
            float beatOffset = beat - previousContact;
            float dampFactor = 1 - MathHelper.Clamp(beatOffset / 6F, 0, 1);

            return 1 - 0.5F * (float)Math.Sin(beatOffset / cycleLength * MathHelper.TwoPi) * dampFactor;
        }

        //Calculates the time from minimum Y to exiting the squish zone at y = 0.75 * RADIUS
        //accel should be negative, vel positive
        //This time is 1/4 of a full cycle of shockwaving
        protected float GetSquishTime(float accel, float vel)
        {
            return SolveQuadratic(0.5F * accel, vel, -0.75F * RADIUS, false);
        }

        protected void DrawScrolls()
        {
            alphaTestEffect.Alpha = 1;
            alphaTestEffect.ReferenceAlpha = 128;
            alphaTestEffect.Texture = scroll;
            device.SetVertexBuffer(scrollVertexBuffer);

            foreach (EffectPass pass in alphaTestEffect.CurrentTechnique.Passes)
            {
                for (int i = 0; i < BookDefs.Length; i++)
                {
                    alphaTestEffect.World = BookDefs[i].matrix * GetScrollMatrix(Beat);
                    pass.Apply();
                    device.DrawPrimitives(PrimitiveType.TriangleStrip, 0, scrollVertexBuffer.VertexCount - 2);
                }
            }
        }

        //This function is in charge of the timeline for the scrollahedron center.
        protected Vector3 GetScrollahedronCenter(float beat)
        {
            //before it appears, bury it offscreen beyond the far plane
            if (beat < 32)
                return new Vector3(0, 0, -1000);

            //start oscillating in all three axes
            float skewBeat = GetSkewBeat(beat - 40, 4);
            Vector3 center = new Vector3(
                0.8F * (float)Math.Sin(skewBeat / 16 * MathHelper.TwoPi),
                -0.5F * (float)Math.Sin(skewBeat / 14 * MathHelper.TwoPi),
                0.5F * (float)Math.Sin(skewBeat / 6.5F * MathHelper.TwoPi) * MathHelper.Clamp((beat - 68) * 3, 1, 8)   //suddenly at beat 68 we oscillate more in Z
                );

            //arrives from far distance during beats 32-40
            center.Z += MathHelper.SmoothStep(-100, -3, (beat - 32) / 8F);

            return center;
        }

        protected Matrix GetScrollMatrix(float beat)
        {
            Matrix matrix;

            //scale up from zero size during beats 32-40 to appear from a point
            //goes to double size
            matrix = Matrix.CreateScale(MathHelper.Clamp((beat - 32) / 4F, 0, 2));

            //Global rotation
            matrix *= Matrix.CreateFromYawPitchRoll(-beat * 0.5F, (float)Math.Cos(beat * 0.5F), 0);

            //Move into world space
            matrix *= Matrix.CreateTranslation(GetScrollahedronCenter(beat));

            return matrix;
        }
        
        protected void DrawTwilight()
        {
            //Twilight slides in by keeping screen origin fixed (lower left)
            //and moving texture origin, from lower right (width, height) to lower left (0, height)
            var slidePercent = MathHelper.Clamp(Beat - 8, 0, 1);
            var textureOrigin = new Vector2(MathHelper.Lerp(twilight.Width, 0, slidePercent), twilight.Height);

            //Draw version 6, single scale
            var batch = new SpriteBatch(device);
            batch.Begin(SpriteSortMode.Immediate, BlendState.AlphaBlend, SamplerState.LinearClamp, DepthStencilState.None, null);
            batch.Draw(twilight,
                new Vector2(0, device.Viewport.Height),
                null,
                Color.White,
                0,
                textureOrigin,
                (float)device.Viewport.Height / twilight.Height * 1F / 3F,        //Twilight is 1/3 screen height
                SpriteEffects.None,
                0);
            batch.End();
        }

        protected void DrawSpike()
        {
            //Spike slides in by keeping screen origin fixed (lower right)
            //and moving texture origin, from lower left (0, height) to lower right (width, height)
            var slidePercent = MathHelper.Clamp(Beat - 36, 0, 1);
            var textureOrigin = new Vector2(MathHelper.Lerp(0, spike.Width, slidePercent), spike.Height);

            //Draw version 6, single scale
            var batch = new SpriteBatch(device);
            batch.Begin(SpriteSortMode.Immediate, BlendState.AlphaBlend, SamplerState.LinearClamp, DepthStencilState.None, null);
            batch.Draw(spike,
                new Vector2(device.Viewport.Width, device.Viewport.Height),
                null,
                Color.White,
                0,
                textureOrigin,
                (float)device.Viewport.Height / spike.Height * 1F / 3F,        //Spike is 1/3 screen height
                SpriteEffects.None,
                0);
            batch.End();
        }
    }
}
