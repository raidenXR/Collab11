using System;
using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;
using Microsoft.Xna.Framework.Input;
using Collab11;

namespace MGclb
{
    class Program : Game
    {
        static void Main(string[] args)
        {
            using(Program game = new Program())
            {
                game.Run();
            }
        }

        private SpriteBatch batch;
        private Particles explosion;
        private MouseState prevMouse;
        private System.Numerics.Vector2 position = new System.Numerics.Vector2(45, 85);
        private Random rand = new Random();
        private BasicEffect effect;

        public Program()
        {
            new GraphicsDeviceManager(this);
            IsMouseVisible = true;
        }

        protected override void LoadContent()
        {
            effect = new BasicEffect(GraphicsDevice);
            explosion = Particles.CreateSource(position, rand.Next(10, 101));
            explosion.Effect = effect;

            base.LoadContent();
        }

        protected override void Update(GameTime gameTime)
        {
            MouseState mouse = Mouse.GetState();
            if (mouse.LeftButton == ButtonState.Released
                && prevMouse.LeftButton == ButtonState.Pressed)
            {
                position = new System.Numerics.Vector2(mouse.X, mouse.Y);
            }

            if (!explosion.IsActive)
            {
                Particles.ReturnSource(ref explosion);
                explosion = Particles.CreateSource(position, rand.Next(10, 101));
                explosion.Effect = effect;
            }

            explosion.Update(gameTime.ElapsedGameTime.TotalMilliseconds);
            prevMouse = mouse;

            base.Update(gameTime);
        }

        protected override void Draw(GameTime gameTime)
        {
            GraphicsDevice.Clear(Color.CornflowerBlue);

            explosion.Draw(GraphicsDevice);

            base.Draw(gameTime);
        }
    }
}
