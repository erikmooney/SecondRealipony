using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using Microsoft.Xna.Framework.Graphics;
using Microsoft.Xna.Framework;

namespace SecondRealipony
{
    class Derpy : SRSegment
    {
        public override float EndBeat { get { return 64; } }
        public override string MusicName { get { return "derpy.wav"; } }

        BasicEffect basicEffect;

        Texture2D transmission;
        Texture2D derpy;
        Texture2D prerender;

        const int LATITUDES = 361;
        const int LONGITUDES = 361;

        Plane TransmissionPlane;
        VertexPositionTexture[] transmissionVertices;
        Matrix transmissionWorldMatrix;

        BoundingSphere[] Spheres;

        VertexPositionTexture[][] sphereVertices;
        VertexBuffer[] sphereVertexBuffers;
        int[] sphereIndices;
        IndexBuffer sphereIndexBuffer;

        class RaySphereCollision
        {
            public int sphereIndex;
            public float distance;
            public Vector3 position;
        }

        class SphereTextureLookup
        {
            public int index;
            public Vector2 baseTextureCoords;
        }
        SphereTextureLookup[][] SphereTextureLookups;

        Vector3 CameraTarget = new Vector3(0, 0, 0);
        Vector3 CameraPos = new Vector3(0, -0.8F, 0.8F);
        Vector3 pointLightSource = new Vector3(0.2F, 0.1F, 0.8F);

        public Derpy(Game game)
            : base(game)
        {
            transmission = game.Content.Load<Texture2D>("transmission.png");
            derpy = game.Content.Load<Texture2D>("derpy.png");

            basicEffect = new BasicEffect(device);
            CreateGeometry();
            CreatePrerender();
        }

        private void CreatePrerender()
        {
            var colors = new Color[device.Viewport.Width * device.Viewport.Height];

            SetBasicEffect();   //Need the view and projection matrices

            for (int v = 0; v < device.Viewport.Height; v++)
                for (int u = 0; u < device.Viewport.Width; u++)
                {
                    Vector3 nearPoint = new Vector3(u, v, 0);
                    Vector3 farPoint = new Vector3(u, v, 1);

                    Vector3 worldPointA = device.Viewport.Unproject(nearPoint, basicEffect.Projection, basicEffect.View, Matrix.Identity);
                    Vector3 worldPointB = device.Viewport.Unproject(farPoint, basicEffect.Projection, basicEffect.View, Matrix.Identity);

                    var direction = Vector3.Normalize(worldPointB - worldPointA);
                    var firstRay = new Ray(worldPointA, direction);
                    Color color = TraceRay(firstRay, -1);
                    colors[v * device.Viewport.Width + u] = color;
                }

            prerender = new Texture2D(device, device.Viewport.Width, device.Viewport.Height);
            prerender.SetData<Color>(colors);
        }

        private Color TraceRay(Ray ray, int sourceSphere)
        {
            //constant
            float reflectivity = 0.75F;

            //Look for sphere collision
            RaySphereCollision collision = FindSphereCollision(ray, sourceSphere);
            if (collision != null)
            {
                var normal = GetSphereNormal(collision.position, collision.sphereIndex);
                var reflected = Vector3.Reflect(ray.Direction, normal);
                var reflectedRay = new Ray(collision.position, reflected);

                Color sphereColor = GetSphereColor(reflectedRay, collision.sphereIndex);
                Color reflectionColor = TraceRay(reflectedRay, collision.sphereIndex) * reflectivity;
                
                return AddColors(sphereColor, reflectionColor);
            }

            //No sphere collision, look for floor collision
            var plane = new Plane(Vector4.UnitZ);
            float? intersect = ray.Intersects(plane);
            if (intersect != null)
            {
                Vector3 planeintercept = ray.Position + ray.Direction * (float)intersect;
                return GetBackdropColor(planeintercept.X, planeintercept.Y);
            }

            //No collisions anywhere
            return Color.Black;
        }


        protected void CreateGeometry()
        {
            CreateTransmissionVertices();
            CreateTransmissionWorldMatrix();
            CreateTransmissionPlane();
            CreateSpheres();
            CreateSphereVertices();
            CreateSphereIndices();
            CreateSphereTextureLookups();
        }

        private void CreateTransmissionVertices()
        {
            //The quad never moves.  It stays still like a billboard and we move the texture coordinates later.
            transmissionVertices = CreateQuad(10, 1);
        }


        private void CreateTransmissionWorldMatrix()
        {
            Matrix matrix =
                Matrix.CreateTranslation(-5, 0, 0)                      //Move left so origin is center of the right side
                * Matrix.CreateScale(0.2F)                              //Scale
                * Matrix.CreateRotationX(MathHelper.ToRadians(10))
                * Matrix.CreateRotationY(MathHelper.ToRadians(70))
                * Matrix.CreateRotationX(MathHelper.ToRadians(35));

            transmissionWorldMatrix = matrix;
        }

        private Vector3 GetBackdropNormal(float r, float theta)
        {
            //Function along x-axis is (r, 0, 0.75F * (float)Math.Sin((float)r * MathHelper.TwoPi * 2))
            var derivative = new Vector3(1, 0, 1.0F * (float)Math.Cos((float)r * MathHelper.TwoPi * 2));

            //Normalize it
            var normalized = Vector3.Normalize(derivative);

            //Normal along x-axis is derivative rotated -90° around Y-axis
            var normal = Vector3.Transform(normalized, Matrix.CreateRotationY(-MathHelper.PiOver2));

            //Normal everywhere on sheet is that rotated around Z-axis
            return Vector3.Transform(normal, Matrix.CreateRotationZ(-theta));
        }

        private Color GetBackdropColor(float x, float y)
        {
            float r = new Vector2(x, y).Length();
            float theta = (float)Math.Atan2(y, x);

            //Ambient
            var ambient = new Color(0, 0, 20);

            //Attenuate with distance from the center
            var attenuation = MathHelper.SmoothStep(1, 0, r - 0.5F);

            //No lighting if shadowed by any sphere
            var position = new Vector3(x, y, 0);
            Ray ray = new Ray(position, Vector3.Normalize(pointLightSource - position));
            if (Spheres.Any(s => ray.Intersects(s) != null))
                return ambient * attenuation;

            var normal = GetBackdropNormal(r, theta);
            var cameraVector = Vector3.Normalize(position - CameraPos);
            var reflected = Vector3.Reflect(cameraVector, normal);
            var reflectedRay = new Ray(position, reflected);
            var lighting = GetLighting(reflectedRay, normal);

            return AddColors(lighting, ambient) * attenuation;
        }

        private Color GetSphereColor(Ray reflectionFromCamera, int sphereIndex)
        {
            var normal = GetSphereNormal(reflectionFromCamera.Position, sphereIndex);
            return GetLighting(reflectionFromCamera, normal);
        }

        private Color GetLighting(Ray reflectionFromCamera, Vector3 normal)
        {
            var ray = new Ray(reflectionFromCamera.Position, normal);
            var diffuse = GetDiffuseLighting(ray, pointLightSource);
            var specular = GetSpecularLighting(reflectionFromCamera, pointLightSource);
            return AddColors(diffuse, specular);
        }


        private Vector3 GetSphereNormal(Vector3 position, int sphereIndex)
        {
            return Vector3.Normalize(position - Spheres[sphereIndex].Center);
        }
        
        private void CreateSphereIndices()
        {
            CreateIndices(LONGITUDES, LATITUDES, out sphereIndices, out sphereIndexBuffer);
        }

        // Define 7 spheres.  Then translate and rotate the whole set into world space
        private void CreateSpheres()
        {
            var sphereDefs = Enumerable.Range(0, 7).Select(i => CreateSphere(i)).ToArray();

            var positionMatrix = Matrix.CreateRotationZ(MathHelper.ToRadians(-65))
                               * Matrix.CreateTranslation(new Vector3(+0.05F, 0.4F, 0F));

            Spheres = sphereDefs.Select(s =>
                new BoundingSphere(Vector3.Transform(s.Center, positionMatrix), s.Radius)).ToArray();
        }

        // Define 7 spheres positioned and sized like this, spaced by equilateral triangles
        //  O o o
        // o o O o
        // ^origin
        private BoundingSphere CreateSphere(int i)
        {
            float x = i * 0.18F;
            float y = (i % 2 == 0) ? 0 : 0.18F * (float)Math.Sqrt(3);
            float z = (i % 3 == 1) ? 0.18F : 0.15F;
            float radius = (i % 3 == 1) ? 0.18F : 0.12F;
            return new BoundingSphere(new Vector3(x, y, z), radius);
        }


        private void CreateSphereVertices()
        {
            sphereVertices = new VertexPositionTexture[Spheres.Length][];
            sphereVertexBuffers = new VertexBuffer[Spheres.Length];

            for (int k = 0; k < Spheres.Length; k++)
            {
                CreateOneSphereVertices(k);
            }
        }

        private void CreateOneSphereVertices(int k)
        {
            sphereVertices[k] = new VertexPositionTexture[LATITUDES * LONGITUDES];

            //loop with spherical coordinates
            for (int i = 0; i < LATITUDES; i++)
            {
                float elevation = ((float)i / (LATITUDES - 1) - 0.5F) * MathHelper.Pi;

                for (int j = 0; j < LONGITUDES; j++)
                {
                    float azimuth = (float)j / (LONGITUDES - 1) * MathHelper.TwoPi;

                    //convert spherical coordinates to xyz.  the meridian is on the positive x axis and positive azimuth is clockwise around y
                    Vector3 vertex = Spheres[k].Radius * new Vector3(
                            (float)(Math.Cos(azimuth) * Math.Cos(elevation)),
                            (float)(Math.Sin(elevation)),
                            (float)(Math.Sin(azimuth) * Math.Cos(elevation)));

                    Vector2 textureCoords = CalculateTextureByReflecting(vertex, k);

                    sphereVertices[k][i * LONGITUDES + j] = new VertexPositionTexture(vertex, textureCoords);
                }
            }
            sphereVertexBuffers[k] = new VertexBuffer(device, typeof(VertexPositionTexture), sphereVertices[k].Length, BufferUsage.None);
            sphereVertexBuffers[k].SetData<VertexPositionTexture>(sphereVertices[k]);
        }

        //Take a ray.  Reflect until it no longer hits either sphere.  If sourceSphere is specified, ignore collisions with that sphere.
        private Ray CalculateSphereReflections(Ray ray, int sourceSphere)
        {
            RaySphereCollision result = FindSphereCollision(ray, sourceSphere);

            if (result == null)
                return ray;
            else
            {
                Vector3 sphereIntersectNormal = Vector3.Normalize(result.position - Spheres[result.sphereIndex].Center);
                Vector3 reflected = Vector3.Reflect(ray.Direction, sphereIntersectNormal);
                Ray newRay = new Ray(result.position, reflected);
                return CalculateSphereReflections(newRay, result.sphereIndex);
            }
        }

        private RaySphereCollision FindSphereCollision(Ray ray, int sourceSphere)
        {
            //Find nearest sphere collision
            int closestSphere = -1;
            float closestDistance = float.MaxValue;
            for (int i = 0; i < Spheres.Length; i++)
            {
                float? distance = i == sourceSphere ? null : ray.Intersects(Spheres[i]);
                if (distance != null && (float)distance < closestDistance)
                {
                    closestSphere = i;
                    closestDistance = (float)distance;
                }
            }

            if (closestSphere == -1)
                return null;
            else
                return new RaySphereCollision { sphereIndex = closestSphere, distance = closestDistance, position = ray.Position + ray.Direction * closestDistance };
        }

        private Color GetSpecularLighting(Ray ray, Vector3 pointLightSource)
        {
            //constants
            Color specularColor = Color.White;
            float specularPower = 50;

            return GetLightingElement(ray, pointLightSource, specularColor, specularPower);
        }

        private Color GetDiffuseLighting(Ray ray, Vector3 pointLightSource)
        {
            //constants
            Color diffuseColor = new Color(10, 40, 128);
            float power = 1;

            return GetLightingElement(ray, pointLightSource, diffuseColor, power);
        }

        private Color GetLightingElement(Ray ray, Vector3 pointLightSource, Color color, float power)
        {
            Vector3 lightingDirection = Vector3.Normalize(pointLightSource - ray.Position);
            float dot = Math.Max(0, Vector3.Dot(ray.Direction, lightingDirection));
            return color * (float)Math.Pow(dot, power);
        }

        //calc this once instead of for every sphere vertex
        private void CreateTransmissionPlane()
        {
            var points = transmissionVertices.Take(3).Select(v => Vector3.Transform(v.Position, transmissionWorldMatrix)).ToArray();
            TransmissionPlane = new Plane(points[0], points[1], points[2]);
        }

        private Vector2 CalculateTextureByReflecting(Vector3 vertex, int sourceSphere)
        {
            var noResult = new Vector2(float.NaN, float.NaN);

            var direction = Vector3.Normalize(Spheres[sourceSphere].Center + vertex - CameraPos);
            Ray firstRay = new Ray(CameraPos, direction);
            Ray ray = CalculateSphereReflections(firstRay, -1);

            //Ray has been reflected until it hit no more spheres.
            //See if it hits the transmission.
            float? transmissionIntersectDistance = ray.Intersects(TransmissionPlane);
            if (transmissionIntersectDistance == null)
                return noResult;

            //Figure out where on the plane we hit and untransform to texture coordinates!
            //Z must be positive, otherwise we hit the floor first before the transmission.
            var intersectPoint = ray.Position + ray.Direction * (float)transmissionIntersectDistance;
            if (intersectPoint.Z < 0)
                return noResult;

            var untransform = Vector3.Transform(intersectPoint, Matrix.Invert(transmissionWorldMatrix));
            var textureCoords = new Vector2(untransform.X / 10F, 1 - untransform.Y - 0.5F);

            //If Y is out of range, return NANs, so the frame-by-frame processing doesn't have to update them
            if (textureCoords.Y < -0.1F || textureCoords.Y > 1.1F)
                return noResult;

            return textureCoords;
        }

        //Create a list of sphere vertices - ONLY those with valid texture coordinates
        private void CreateSphereTextureLookups()
        {
            SphereTextureLookups = new SphereTextureLookup[Spheres.Length][];

            for (int i = 0; i < SphereTextureLookups.Length; i++)
            {
                SphereTextureLookups[i] = sphereVertices[i]
                    .Select((v, index) => new SphereTextureLookup { index = index, baseTextureCoords = v.TextureCoordinate })
                    .Where(v => !float.IsNaN(v.baseTextureCoords.X))
                    .ToArray();
            }
        }

        private void SetBasicEffect()
        {
            //spriteBatch changes the DepthStencilState to None and that's why 3D objects don't draw correctly. Other properties get changed too. Check these out:
            device.BlendState = BlendState.AlphaBlend;
            device.DepthStencilState = DepthStencilState.Default;
            device.SamplerStates[0] = SamplerState.PointClamp;

            SetCullMode(CullMode.CullClockwiseFace);
            
            basicEffect.View = Matrix.CreateLookAt(CameraPos, CameraTarget, Vector3.Up);
            basicEffect.Projection = CreatePerspectiveAtDepth(1.8F, (CameraTarget - CameraPos).Length(), 0.01F, 5F);

            basicEffect.LightingEnabled = false;
            basicEffect.TextureEnabled = true;
        }

        protected override void DrawSegment()
        {
            DrawPrerender();
            SetBasicEffect();

            DrawSpheres();
            DrawTransmission();
            DrawDerpy();
            FadeScreen(0, 2, Beat, true, true);
            FadeScreen(62, 64, Beat, true, false);
        }

        private void DrawPrerender()
        {
            var batch = new SpriteBatch(device);
            batch.Begin();
            batch.Draw(prerender, FullScreen, Color.White);
            batch.End();
        }

        private void DrawSpheres()
        {
            foreach (EffectPass pass in basicEffect.CurrentTechnique.Passes)
            {
                for (int i = 0; i < Spheres.Length; i++)
                {
                    basicEffect.World = Matrix.CreateTranslation(Spheres[i].Center);
                    basicEffect.Texture = transmission;

                    //Hate resetting the whole vertex buffer, but there's no way to resend just a few vertices (those that need texture coordinates changed), or to draw user vertices with a preset index buffer
                    device.SetVertexBuffer(null);
                    SetSphereTextureCoordinates(Beat, i);
                    sphereVertexBuffers[i].SetData(sphereVertices[i]);
                    device.SetVertexBuffer(sphereVertexBuffers[i]);
                    device.Indices = sphereIndexBuffer;
                    pass.Apply();

                    device.DrawIndexedPrimitives(PrimitiveType.TriangleStrip, 0, 0, sphereIndexBuffer.IndexCount, 0, sphereIndexBuffer.IndexCount - 2);
                }
            }
        }

        private void DrawTransmission()
        {
            foreach (EffectPass pass in basicEffect.CurrentTechnique.Passes)
            {
                basicEffect.LightingEnabled = false;
                basicEffect.World = transmissionWorldMatrix;
                basicEffect.Texture = transmission;
                pass.Apply();

                SetTransmissionTextureCoordinates(Beat);

                device.DrawUserPrimitives<VertexPositionTexture>(
                    PrimitiveType.TriangleStrip,
                    transmissionVertices,
                    0,
                    transmissionVertices.Length - 2);
            }
        }

        private float GetTextureOffsetAtOrigin(float beat)
        {
            return (beat - 20) / 36F;
        }

        private void SetTransmissionTextureCoordinates(float beat)
        {
            float offset = GetTextureOffsetAtOrigin(beat);

            transmissionVertices[0].TextureCoordinate.X = offset - 0.5F;
            transmissionVertices[1].TextureCoordinate.X = offset - 0.5F;
            transmissionVertices[2].TextureCoordinate.X = offset;
            transmissionVertices[3].TextureCoordinate.X = offset;
        }

        private void SetSphereTextureCoordinates(float beat, int sphereIndex)
        {
            float offset = GetTextureOffsetAtOrigin(beat);

            foreach (var lookup in SphereTextureLookups[sphereIndex])
            {
                sphereVertices[sphereIndex][lookup.index].TextureCoordinate.X = lookup.baseTextureCoords.X + offset - 0.5F;
            }
        }

        private void DrawDerpy()
        {
            if (Beat < 24)
                return;

            //Derpy slides in by keeping screen origin fixed (lower left)
            //and moving texture origin, from lower right (width, height) to lower left (0, height)
            var slidePercent = MathHelper.Clamp((Beat - 24) / 2, 0, 1);
            var textureOrigin = new Vector2(MathHelper.Lerp(derpy.Width, 0, slidePercent), derpy.Height);

            //Draw version 6, single scale
            var batch = new SpriteBatch(device);
            batch.Begin(SpriteSortMode.Immediate, BlendState.AlphaBlend, SamplerState.LinearClamp, DepthStencilState.None, null);
            batch.Draw(derpy,
                new Vector2(0, device.Viewport.Height),
                null,
                Color.White,
                0,
                textureOrigin,
                (float)device.Viewport.Height / derpy.Height * 1F / 3F,        //Derpy is 1/3 screen height
                SpriteEffects.None,
                0);
            batch.End();
        }
    }
}
