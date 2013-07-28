using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using Microsoft.Xna.Framework.Graphics;
using Microsoft.Xna.Framework;

namespace SecondRealipony
{
    class Cube : SRSegment
    {
        BasicEffect basicEffect;

        //Cube is defined by only one set of vertices.  Transform this for each face with WorldMatrix at draw time.
        VertexPositionNormalTexture[] faceVertices;
        VertexBuffer faceVertexBuffer;
        Matrix[] faceWorldMatrices;

        Texture2D[] cubeTextures;
        Texture2D spriteSheet;

        public override float BeatLength { get { return 60F / 130; } }
        public override float EndBeat { get { return 64; } }
        public override string MusicName { get { return "cube.wav"; } }

        public Cube(Game game) : base(game)
        {
            basicEffect = new BasicEffect(device);
            
            CreateGeometry();
            spriteSheet = game.Content.Load<Texture2D>("cutie.png");
        }

        private void CreateGeometry()
        {
            faceVertices = new VertexPositionNormalTexture[] {
                new VertexPositionNormalTexture(new Vector3(-0.5F,  0.5F, 0), Vector3.UnitZ, new Vector2(0, 0)),
                new VertexPositionNormalTexture(new Vector3(-0.5F, -0.5F, 0), Vector3.UnitZ, new Vector2(0, 1)),
                new VertexPositionNormalTexture(new Vector3( 0.5F,  0.5F, 0), Vector3.UnitZ, new Vector2(1, 0)),
                new VertexPositionNormalTexture(new Vector3( 0.5F,  0.5F, 0), Vector3.UnitZ, new Vector2(1, 0)),
                new VertexPositionNormalTexture(new Vector3(-0.5F, -0.5F, 0), Vector3.UnitZ, new Vector2(0, 1)),
                new VertexPositionNormalTexture(new Vector3( 0.5F, -0.5F, 0), Vector3.UnitZ, new Vector2(1, 1)),
            };

            faceVertexBuffer = new VertexBuffer(device, typeof(VertexPositionNormalTexture), faceVertices.Length, BufferUsage.WriteOnly);
            faceVertexBuffer.SetData(faceVertices);

            cubeTextures = new Texture2D[] {
                Get1x1Texture(new Color(212, 164, 232)),
                Get1x1Texture(new Color(250, 186, 97)),
                Get1x1Texture(new Color(157, 217, 248)),
                Get1x1Texture(new Color(240, 242, 243)),
                Get1x1Texture(new Color(253, 246, 175)),
                Get1x1Texture(new Color(249, 184, 210))
            };

            //Pony types opposite - Twilight/bottom AJ/back RD/right :: Rarity/top Fluttershy/left Pinkie/front (upside down)
            faceWorldMatrices = new Matrix[] {
                Matrix.CreateRotationX(MathHelper.PiOver2) * Matrix.CreateTranslation(new Vector3(0, -0.5F, 0)),
                Matrix.CreateRotationY(MathHelper.Pi) * Matrix.CreateTranslation(new Vector3(0, 0, -0.5F)),
                Matrix.CreateRotationY(MathHelper.PiOver2) * Matrix.CreateTranslation(new Vector3(0.5F, 0, 0)),
                Matrix.CreateRotationX(-MathHelper.PiOver2) * Matrix.CreateTranslation(new Vector3(0, 0.5F, 0)),
                Matrix.CreateRotationY(-MathHelper.PiOver2) * Matrix.CreateTranslation(new Vector3(-0.5F, 0, 0)),
                Matrix.CreateRotationZ(MathHelper.Pi) * Matrix.CreateTranslation(new Vector3(0, 0, 0.5F)),
            };
        }

        private void SetBasicEffect()
        {
            device.BlendState = BlendState.AlphaBlend;
            device.DepthStencilState = DepthStencilState.None;
            device.SamplerStates[0] = SamplerState.LinearClamp;

            SetCullMode(CullMode.CullClockwiseFace);

            var cameraX = 0;
            var cameraY = MathHelper.SmoothStep(5, 0, Beat / 4F);
            var cameraZ = MathHelper.SmoothStep(5, 2.5F, (Beat - 12F) / 12F);
            if (Beat > 56)
                cameraZ = MathHelper.SmoothStep(2.5F, 4, (Beat - 56) / 8F);

            Vector3 CameraPos = new Vector3(cameraX, cameraY, cameraZ);
            Vector3 CameraTarget = new Vector3(cameraX, cameraY, 0);

            basicEffect.View = Matrix.CreateLookAt(CameraPos, CameraTarget, Vector3.Up);
            basicEffect.Projection = Matrix.CreatePerspective(AspectRatio * 0.5F, 0.5F, 1, 10);

            basicEffect.LightingEnabled = true;
            basicEffect.AmbientLightColor = Vector3.One * 0.85F;
            basicEffect.DirectionalLight0.Enabled = true;
            basicEffect.DirectionalLight0.SpecularColor = Vector3.Zero;
            basicEffect.DirectionalLight0.DiffuseColor = Vector3.One * 0.15F;
            basicEffect.DirectionalLight0.Direction = Vector3.Forward;
        }


        protected override void DrawSegment()
        {
            SetBasicEffect();
            basicEffect.TextureEnabled = true;
            basicEffect.VertexColorEnabled = false;

            foreach (EffectPass pass in basicEffect.CurrentTechnique.Passes)
            {
                DrawFaceBackgrounds(pass);
                DrawCutieMarks(pass);
            }

            FadeScreen(60, 64, Beat, true, false);
        }

        private void DrawFaceBackgrounds(EffectPass pass)
        {
            device.SetVertexBuffer(faceVertexBuffer);
            for (int i = 0; i < cubeTextures.Length; i++)
            {
                basicEffect.Texture = cubeTextures[i];
                basicEffect.World = faceWorldMatrices[i] * GetCubeWorldMatrix(Beat);
                pass.Apply();

                device.DrawPrimitives(PrimitiveType.TriangleList, 0, faceVertexBuffer.VertexCount - 2);
            }
        }

        private Matrix GetCubeWorldMatrix(float beat)
        {
            return Matrix.CreateFromYawPitchRoll(beat * 0.15F, beat * 0.28F, beat * 0.33F);
        }

        private void DrawCutieMarks(EffectPass pass)
        {
            basicEffect.Texture = spriteSheet;

            //Start animation at beat 10 ramping up to full speed six beats later
            var skewBeat = Beat < 10 ? 0 : GetSkewBeat(Beat - 10, 6);

            for (int i = 0; i < cubeTextures.Length; i++)
            {
                basicEffect.World = faceWorldMatrices[i] * GetCubeWorldMatrix(Beat);
                pass.Apply();

                //List of VertexPositionNormalTextures to be drawn as a triangle list
                var vertices = GetPonyVertices(i, skewBeat);

                device.DrawUserPrimitives<VertexPositionNormalTexture>(PrimitiveType.TriangleList, vertices.ToArray(), 0, vertices.Count / 3);
            }
        }

        private List<VertexPositionNormalTexture> GetPonyVertices(int i, float beat)
        {
            var methods = new Func<float, List<VertexPositionNormalTexture>>[] {
                GetTwilightVertices,
                GetApplejackVertices,
                GetRainbowDashVertices,
                GetRarityVertices,
                GetFluttershyVertices,
                GetPinkiePieVertices
            };

            return methods[i](beat);
        }

        //Get a frame from the sprite sheet as vertices to draw one cube face
        //This could be coded as a degenerate case of GetBoxVertices
        private List<VertexPositionNormalTexture> GetFrameVertices(int row, int frame)
        {
            var result = new List<VertexPositionNormalTexture>();

            result.AddRange(faceVertices.Select(
                vpnt => new VertexPositionNormalTexture(
                    vpnt.Position,
                    Vector3.UnitZ,
                    new Vector2((vpnt.TextureCoordinate.X + frame) * 0.1F, (vpnt.TextureCoordinate.Y + row) / 6F)
                    )));

            return result;
        }

        //Project a bounding box into the sprite sheet
        //Takes parameters in VERTEX space (range -0.5 to +0.5 on each axis, and Y increases upwards)
        //Returns vertices in vertex space, with texture coordinates in texture space (range 0 to 0.1 within this box, and Y increases downwards)
        //The texture coordinates can be transformed by an input matrix.
        //We invert that matrix to be more intuitive.  Example: Scale 2 would double the coords of the bounding box, shrinking the contents.  Inversion makes that the other way around, scale 2 doubles the size of the contents
        private List<VertexPositionNormalTexture> GetBoxVertices(int row, int frame, Vector2 center, float width, float height, Matrix transform)
        {
            //in VERTEX space, Y increases upwards
            //6 vertices, middle two repeated: NW SW NE NE SW SE
            var vertices = new Vector2[] {
                new Vector2(center.X - width / 2, center.Y + height / 2),
                new Vector2(center.X - width / 2, center.Y - height / 2),
                new Vector2(center.X + width / 2, center.Y + height / 2),
                new Vector2(center.X + width / 2, center.Y + height / 2),
                new Vector2(center.X - width / 2, center.Y - height / 2),
                new Vector2(center.X + width / 2, center.Y - height / 2),
            };

            var sheetOffset = new Vector2(frame * 0.1F, row / 6F);
            var centerInTextureSpace = new Vector3(0.5F + center.X, 1 - (0.5F + center.Y), 0);
            var scaleVector = new Vector2(0.1F, 1 / 6F);

            //Create VertexPositionNormalTexture by projecting those vertices into texture space (Y increases downwards)
            var result = vertices.Select(v =>
                new VertexPositionNormalTexture(
                    new Vector3(v, 0),
                    Vector3.UnitZ,
                    Vector2.Transform(
                        new Vector2(0.5F + v.X, 1 - (0.5F + v.Y)),          //Project to texture space
                        Matrix.CreateTranslation(-centerInTextureSpace)
                        * Matrix.Invert(transform)                          //Transform by inverted matrix
                        * Matrix.CreateTranslation(centerInTextureSpace)
                        )
                        * scaleVector                                       //Scale into box on sprite sheet
                        + sheetOffset                                       //Locate correct box on sprite sheet
                    ));

            return result.ToList();
        }

        Random random = new Random(0);
        int currentPulse = -1;
        int previousPulse = -1;
        int previousBeat = 0;
        private List<VertexPositionNormalTexture> GetTwilightVertices(float beat)
        {
            var result = new List<VertexPositionNormalTexture>();

            //The back center star, rotate it
            result.AddRange(GetBoxVertices(0, 1, new Vector2(0.0225F, -0.015F), 0.57F, 0.57F, Matrix.CreateRotationZ(beat * MathHelper.Pi / 3)));

            //The main star
            result.AddRange(GetFrameVertices(0, 0));

            var areaDefs = new AreaDef[] {
                new AreaDef(new Vector2(0.195F, 0.3075F), 0.25F, 0.355F, 0),
                new AreaDef(new Vector2(0.2425F, -0.225F), 0.2625F, 0.51F, 1),
                new AreaDef(new Vector2(-0.11F, -0.3175F), 0.2F, 0.365F, 2),
                new AreaDef(new Vector2(-0.2925F, -0.06F), 0.255F, 0.405F, 3),
                new AreaDef(new Vector2(-0.185F, 0.3525F), 0.185F, 0.295F, 4),
            };

            AdvancePulse(beat);

            result.AddRange(areaDefs.SelectMany(a =>
                GetBoxVertices(0, a.Index + 2, a.Center, a.Width, a.Height, a.Index == currentPulse ? Matrix.CreateScale(2 - (beat - previousBeat)) : Matrix.Identity)));
            
            return result;
        }

        private void AdvancePulse(float beat)
        {
            if (beat >= previousBeat + 1)
            {
                //Find the next outer star to pulse, but don't repeat either of the previous two
                int newPulse;
                do
                {
                    newPulse = random.Next(5);
                }
                while (newPulse == currentPulse || newPulse == previousPulse);

                previousPulse = currentPulse;
                currentPulse = newPulse;
                previousBeat = (int)beat;
            }
        }

        private List<VertexPositionNormalTexture> GetApplejackVertices(float beat)
        {
            var result = new List<VertexPositionNormalTexture>();

            var areaDefs = new AreaDef[] {
                new AreaDef(new Vector2(-0.245F, -0.1775F), 0.5F, 0.5F, 0),
                new AreaDef(new Vector2(0.06F, 0.1725F), 0.5F, 0.5F, 1),
                new AreaDef(new Vector2(0.23F, -0.2725F), 0.5F, 0.63F, 2),
            };

            result.AddRange(areaDefs.SelectMany(a =>
                GetBoxVertices(1, a.Index, a.Center, a.Width, a.Height, Matrix.CreateRotationZ((float)Math.Sin((beat - a.Index / 3F) * MathHelper.PiOver2) * 0.75F))));

            return result;
        }

        private List<VertexPositionNormalTexture> GetRainbowDashVertices(float beat)
        {
            var result = new List<VertexPositionNormalTexture>();

            //Not doing a matrix transform on this one, just change the X texture coordinates of the bottom vertices (where vertex Y is negative)
            var offset = new Vector2(GetTriangleWave(beat, 0.02F, 2, 0F), 0);
            var boltVertices = GetBoxVertices(2, 1, new Vector2(0F, -1F / 12F), 1, 5F / 6F, Matrix.Identity)
                .Select(vpnt =>
                new VertexPositionNormalTexture(vpnt.Position, vpnt.Normal, vpnt.Position.Y < 0 ? vpnt.TextureCoordinate + offset : vpnt.TextureCoordinate)
                ).ToList();

            result.AddRange(boltVertices);
            result.AddRange(GetFrameVertices(2, 0));
            return result;
        }

        private List<VertexPositionNormalTexture> GetRarityVertices(float beat)
        {
            var result = new List<VertexPositionNormalTexture>();
            result.AddRange(GetFrameVertices(3, 0));

            //Glint is stored as white with alpha of 10%.  Draw it up to 10 times depending on intensity of the glint.
            int phase =  (int)((beat * 4 + 1) % 8);
            for (int i = 0; i < 8; i++)
            {
                int intensity = beat == 0 ? 0 : (int)(((8 - phase + i) % 8) * 3F - 10F);     //No intensity while skewbeat is still 0
                result.AddRange(GetGlint(i, intensity));
            }

            return result;
        }

        private List<VertexPositionNormalTexture> GetGlint(int glint, int intensity)
        {
            var result = new List<VertexPositionNormalTexture>();
            var vertices = GetFrameVertices(3, glint + 1);

            for (int i = 0; i < intensity; i++)
                result.AddRange(vertices);
            
            return result;
        }

        private List<VertexPositionNormalTexture> GetFluttershyVertices(float beat)
        {
            var result = new List<VertexPositionNormalTexture>();

            //3 butterfly wings, then 3 butterfly bodies
            var areaDefs = new AreaDef[] {
                new AreaDef(new Vector2(-0.1975F, -0.18F), 0.6F, 0.5F, 0),
                new AreaDef(new Vector2(0.13F, 0.2F), 0.6F, 0.5F, 1),
                new AreaDef(new Vector2(0.24F, -0.245F), 0.6F, 0.5F, 2),
                new AreaDef(new Vector2(-0.1975F, -0.18F), 0.6F, 0.5F, 3),
                new AreaDef(new Vector2(0.13F, 0.2F), 0.6F, 0.5F, 4),
                new AreaDef(new Vector2(0.24F, -0.245F), 0.6F, 0.5F, 5),
            };

            //butterflies are actually stored on sprite sheet oriented vertically.  rotate them to the proper orientations for the cutie mark _after_ flapping the wings
            var rotations = new Matrix[] {
                Matrix.CreateRotationZ(MathHelper.ToRadians(-33.6F)),
                Matrix.CreateRotationZ(MathHelper.ToRadians(30.1F)),
                Matrix.CreateRotationZ(MathHelper.ToRadians(27.8F))
            };

            result.AddRange(areaDefs.SelectMany(a =>
                GetBoxVertices(4, a.Index, a.Center, a.Width, a.Height,
                    (a.Index <= 2 ? Matrix.CreateScale(1 + (float)Math.Sin(beat * MathHelper.TwoPi) / 4F, 1 + (float)Math.Sin((beat + 0.5F) * MathHelper.TwoPi) / 4F, 1) : Matrix.Identity)
                    * rotations[a.Index % 3])));

            return result;
        }

        private List<VertexPositionNormalTexture> GetPinkiePieVertices(float beat)
        {
            var result = new List<VertexPositionNormalTexture>();

            //actual center Ys are -0.1025, 0.1625, -0.1375.  but we don't care where the center is if just translating

            var areaDefs = new AreaDef[] {
                new AreaDef(new Vector2(-0.125F, 0), 0.5F, 1.0F, 0),
                new AreaDef(new Vector2(-0.0175F, 0), 0.5F, 1.0F, 1),
                new AreaDef(new Vector2(0.195F, 0), 0.5F, 1.0F, 2),
            };

            result.AddRange(areaDefs.SelectMany(a =>
                GetBoxVertices(5, a.Index, a.Center, a.Width, a.Height,
                Matrix.CreateTranslation(
                    (float)Math.Sin((beat + a.Index / 2F) * MathHelper.Pi) / 16F,
                    (float)Math.Cos((beat + a.Index / 2F) * MathHelper.Pi) / 16F,
                    0))));

            return result;
        }
    }

    class AreaDef
    {
        public Vector2 Center;
        public float Width;
        public float Height;
        public int Index;

        public AreaDef(Vector2 center, float width, float height, int index)
        {
            Center = center;
            Width = width;
            Height = height;
            Index = index;
        }
    }
}
