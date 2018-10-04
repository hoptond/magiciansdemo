using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;
using Microsoft.Xna.Framework.Input;

namespace Magicians
{
    class Camera2D
    {
		readonly Game game;
        protected float Zoom;
        public Matrix Transform;
        public Point Position;
        protected float Rotation;
        protected Viewport Viewport;
        public ScreenShake Shake;
        public bool clampViewport = true; //if true, the camera will clamp to the edges of the map to avoid displaying black space
 
        public Camera2D(Game game)
        {
            this.game = game;
            Zoom = 1f;
            Rotation = MathHelper.ToDegrees(0);
            Position = Point.Zero;
            Viewport = new Viewport(0, 0, game.GetScreenWidth(), game.GetScreenHeight());
            Shake = new ScreenShake();
        }
        public void Update(GameTime gameTime, Point pos, int x, int y)
        {
            var map = (Map)game.Scene;
            Position = pos;
            Viewport.Width = x;
            Viewport.Height = y;
            Shake.Update(gameTime);
            MoveCamera(pos, map.Width * map.TileWidth, map.Height * map.TileHeight);
            Transform = Matrix.CreateTranslation(new Vector3(-Position.X, -Position.Y, 0)) * Matrix.CreateRotationZ(Rotation) * Matrix.CreateScale(new Vector3(Zoom, Zoom, 1)) * Matrix.CreateTranslation(new Vector3(Viewport.Width * 0.5f, Viewport.Height * 0.5f, 0));
        }
        public void MoveCamera(Point pos ,int mapWidth, int mapHeight)
        {
            Position.X = pos.X;
            Position.Y = pos.Y;
            if (clampViewport)
            {
                Position.X = MathHelper.Clamp(Position.X, 0 + (game.GetScreenWidth() / 2), mapWidth - (game.GetScreenWidth() / 2));
                Position.Y = MathHelper.Clamp(Position.Y, 0 + (game.GetScreenHeight() / 2), mapHeight - (game.GetScreenHeight() / 2));
            }
            Position.X += Shake.ScreenOffset.X;
            Position.Y += Shake.ScreenOffset.Y;
        }
        public class ScreenShake
        {
            public Point ScreenOffset = new Point();
            float Timer;
            int Strength;
            float Interval = 0.05f;
            byte UpDown; // 0 is up, 1 is down
            public void Update(GameTime gameTime)
            {
                if (Strength > 0)
                {
                    Timer += (float)gameTime.ElapsedGameTime.TotalSeconds;
                    if (Timer > Interval)
                    {
                        switch (UpDown)
                        {
                            case 0: ScreenOffset = new Point(0, -(Strength / 10)); UpDown = 1; break;
                            case 1: ScreenOffset = new Point(0, Strength / 10); UpDown = 0; break;
                        }
                        Strength -= 5;
                        Timer = 0;
                    }
                }
            }
            public ScreenShake()
            {
                ScreenOffset = new Point(0);
                Strength = 0;
                UpDown = 1;
                Timer = 0;
            }
            public void StartShake(int i)
            {
                Strength = i;
            }
        }
    }
}
