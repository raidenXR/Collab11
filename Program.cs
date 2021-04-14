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

        SpriteBatch batch;
        Particles explosion;
        MouseState prevMouse;

        public Program()
        {
            new GraphicsDeviceManager(this);
            IsMouseVisible = true;
        }

        protected override void LoadContent()
        {
            explosion = new Particles(new System.Numerics.Vector2(45, 85));
            explosion.Effect = new BasicEffect(GraphicsDevice);

            base.LoadContent();
        }

        protected override void Update(GameTime gameTime)
        {
            MouseState mouse = Mouse.GetState();
            if(mouse.LeftButton == ButtonState.Released 
                && prevMouse.LeftButton == ButtonState.Pressed)
            {
                explosion.Center = new System.Numerics.Vector2(mouse.X, mouse.Y);
                explosion.ResetColor();
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
