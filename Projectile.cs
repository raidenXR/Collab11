using XNA = Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;
using System;
using System.Numerics;
using System.Collections.Generic;

namespace Collab11
{
    public static class Ext
    {
        public static XNA.Color ToColor(this Random rand)
        {
            return new XNA.Color()
            {
                R = (byte)rand.Next(150, 250),
                G = (byte)rand.Next(20, 50),
                B = (byte)35,
                A = 250
            };
        }

        public static Vector2 ToVector2(this Random rand)
        {
            float angle = rand.Next(0, 360) * (float)Math.PI;

            return new Vector2
            {
                X = (float)Math.Cos(angle),
                Y = (float)Math.Sin(angle)
            };
        }

        public static Vector2 ToVector2Clamped(this Random rand, Vector2 Angle)
        {
            float angle = rand.Next((int)Angle.X, (int)Angle.Y + 1) * (float)Math.PI;

            return new Vector2
            {
                X = (float)Math.Cos(angle),
                Y = (float)Math.Sin(angle)
            };
        }
    }

    public readonly struct PositionColor
    {
        public readonly Vector2 Position;
        public readonly XNA.Color Color;

        public PositionColor(Vector2 position, XNA.Color color)
        {
            Position = position;
            Color = color;
        }

        public static readonly VertexDeclaration VertexDeclaration = new VertexDeclaration(
            new VertexElement[]
            {
                new VertexElement(0, VertexElementFormat.Vector2, VertexElementUsage.Position, 0),
                new VertexElement(8, VertexElementFormat.Color, VertexElementUsage.Color, 0)
            });
    }

    public struct Particle
    {
        public Vector2 Position;
        public Vector2 Scale;
        public Vector2 Direction;
        public Vector2 Destination;
        public Vector2 Speed;
        public int Sides;
        public float Rotation;
        public float LifeSpan;
        public XNA.Color Stroke;
        public XNA.Color Fill;
        public bool IsActive;

        private static Vector2[][] _vertices;

        public const int MinSides = 8;
        public const int MaxSides = 16;
        public const int MaxVertices = ((MaxSides - 1) * 3) + 3;


        static Particle()
        {
            _vertices = new Vector2[(MaxSides - MinSides) + 1][];

            for (int i = 0, sides = Particle.MinSides; i < _vertices.GetLength(0); i++, sides++)
            {
                _vertices[i] = new Vector2[sides];
                float angle = (float)Math.PI * 2f / (float)sides;

                SetVertices(_vertices[i], angle, sides);
            }
        }


        #region properties - methods
        public readonly int Vertices => ((Sides - 1) * 3) + 3;

        public readonly Matrix3x2 Transform => Matrix3x2.Identity 
                                               * Matrix3x2.CreateScale(Scale)                                            
                                               * Matrix3x2.CreateRotation(Rotation)
                                               * Matrix3x2.CreateTranslation(Position);

        public readonly void CopyTo(PositionColor[] buffer, ref int stride)
        {
            Vector2[] vertices = _vertices[Sides - MinSides];
            Vector2 vec = Vector2.Zero;
            Matrix3x2 transform = this.Transform;


            for (int i = 0; i < Sides - 1; i++)
            {
                vec = Vector2.Transform(vertices[i], transform);
                buffer[stride + i * 3] = new PositionColor(vec, Stroke);

                vec = Vector2.Transform(vertices[i + 1], transform);
                buffer[stride + i * 3 + 1] = new PositionColor(vec, Stroke);

                buffer[stride + i * 3 + 2] = new PositionColor(Position, Fill);
            }

            vec = Vector2.Transform(vertices[Sides - 1], transform);
            buffer[stride + (Sides - 1) * 3] = new PositionColor(vec, Stroke);

            vec = Vector2.Transform(vertices[0], transform);
            buffer[stride + (Sides - 1) * 3 + 1] = new PositionColor(vec, Stroke);
            buffer[stride + (Sides - 1) * 3 + 2] = new PositionColor(Position, Fill);


            stride += this.Vertices; // ((Sides - 1) * 3) + 3;
        }

        private static void SetVertices(Vector2[] points, float angle, int sides)
        {
            for (int i = 0; i < sides; i++)
            {
                points[i].X = (float)Math.Cos(angle * i);
                points[i].Y = (float)Math.Sin(angle * i);
            }
        }
        #endregion
    }

    public class Particles
    {
        Vector2 position;
        int count;
        private Particle[] particles; 
        private PositionColor[] vertexData;
        
        public BasicEffect Effect { get; set; }

        static Random rand = new Random();
        static Stack<PositionColor> pool = new Stack<PositionColor>();

        const int minTime = 1500;
        const int maxTime = 2500;

        #region fields
        public Vector2 Center = new Vector2(400, 300);
        Vector2 angle;
        Vector2 initialDestination;
        //Vector2 initialSpeed;
        //Vector2 initialScale;
        Vector2 scale;
        Vector2 direction;
        Vector2 speed;
        Vector2 particlesDirection;

        float dspeed;
        float dscale;
        float rotation = 1f;
        XNA.Color fill, stroke;
        XNA.Color fillVariance, strokeVariance;        
        #endregion

        ~Particles()
        {
            Effect.Dispose();
        }

        public Particles(Vector2 position)
        {
            this.position = position;
            particles = new Particle[100];
            vertexData = new PositionColor[particles.Length * Particle.MaxVertices];

            ResetColor();

#if DEBUG
            Console.WriteLine(vertexData.Length);
            Console.WriteLine();
#endif
        }

        #region methods

        public void ResetColor()
        {
            fill.R = (byte)rand.Next(150, 256);
            fill.G = (byte)rand.Next(1, 150);
            fill.B = (byte)rand.Next(1, 150);
            fill.A = (byte)rand.Next(100, 200);
            fillVariance.R = (byte)rand.Next(1, 51);
            fillVariance.G = (byte)rand.Next(1, 51);
            fillVariance.B = (byte)rand.Next(1, 51);
            particlesDirection = new Vector2(0f, -1f);
            initialDestination = Center + particlesDirection;
        }

        private void EmitParticles()
        {
            float lifeSpan = rand.Next(minTime, maxTime);

            for (count = 0; count < particles.Length; count++)
            {
                particles[count] = ExplosionEmit(Center + new Vector2(rand.Next(-100, 100), rand.Next(100, 100)), lifeSpan);
            }
        }


        public void Update(double dt)
        {
            if(count == 0) EmitParticles();

            for (int i = 0; i < count; i++)
            {
                ref Particle particle = ref particles[i];
                particle.LifeSpan -= (float)dt;
                particle.IsActive = particle.LifeSpan > 0 ? true : false;

                if (!particle.IsActive 
                    || particle.Scale.X <= 0f 
                    || particle.Scale.Y <= 0f)
                {
                    particles[i--] = particles[--count];
                }                
            }

            for (int i = 0; i < count; i++)
            {
                ExplosionBehaviour(ref particles[i], (float)dt);
            }            
        }

        public void Draw(GraphicsDevice graphics)
        {
            if (count > 0)
            {
                graphics.BlendState = BlendState.AlphaBlend;
                Effect.Alpha = 1.0f;
                Effect.VertexColorEnabled = true;
                Effect.Projection = XNA.Matrix.CreateOrthographicOffCenter(0, graphics.Viewport.Width, graphics.Viewport.Height, 0, 0, 1);
                Effect.CurrentTechnique.Passes[0].Apply();

                int verticesCount = 0;
                for (int i = 0; i < count; i++)
                {
                    particles[i].CopyTo(vertexData, ref verticesCount);
                }

                graphics.DrawUserPrimitives<PositionColor>(PrimitiveType.TriangleList, vertexData, 0, verticesCount / 3, PositionColor.VertexDeclaration);               
            }
        }
        #endregion


        #region behaviours
        private void ExplosionBehaviour(ref Particle particle, float dt)
        {
            if (MathF.Abs(particle.Position.Y - particle.Destination.Y) > 2f)
            {
                particle.Destination.Y = particle.Position.Y + particlesDirection.Y * (Center - particle.Position).Length();
            }

            particle.Direction += Vector2.Normalize(particle.Destination - particle.Position) * (1 / particle.Speed.Length());
            
            particle.Direction += direction;
            particle.Direction = Vector2.Normalize(particle.Direction);

            particle.Speed += speed;            
            particle.Rotation += rotation * dt;
            particle.Scale += scale;
            particle.Position += particle.Direction * particle.Speed * dt / 5f;
        }

        public Particle ExplosionEmit(Vector2 destination, float lifeSpan)
        {            
            Vector2 angle = this.angle;
            float scaleVariance = this.dscale;
            float speedVariance = this.dspeed;
            Vector2 direction = angle == Vector2.Zero ? rand.ToVector2() : rand.ToVector2Clamped(angle);
            float rotation = (float)Math.Atan2(direction.Y, direction.X);

            scaleVariance = rand.Next(-(int)(scaleVariance * 100f), (int)((scaleVariance + 1) * 100f)) * 0.01f;
            speedVariance = rand.Next(-(int)(speedVariance * 100f), (int)((speedVariance + 1) * 100f)) * 0.01f;

            return new Particle
            {
                IsActive = true,
                Sides = rand.Next(Particle.MinSides, Particle.MaxSides),
                Rotation = rotation,
                Stroke = stroke,
                Fill = fill,
                Position = Center,
                LifeSpan = lifeSpan,
                Scale = new Vector2(rand.Next(20, 30)) + new Vector2(scaleVariance),
                Direction = direction,
                Speed = (destination - Center) / 100f + new Vector2(speedVariance),
                Destination = destination
            };
        }
        #endregion
    }
}