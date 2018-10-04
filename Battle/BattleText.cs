using System;
using Microsoft.Xna.Framework;

namespace Magicians
{
	//damage numbers etc
    class BattleText
    {
        public Vector2 Position;
        public int r, g, b;
        public string s;
        public Vector2 Speed; //the speed the text moves at
        float interval = 0.01f;
        float timer;
        public BattleText(Vector2 pos, int r, int g, int b, string s, Vector2 sp)
        {
            Position = pos;
            this.r = r;
            this.g = g;
            this.b = b;
            this.s = s;
            Speed = sp;
        }
        public void Update(GameTime gameTime)
        {
            timer += (float)gameTime.ElapsedGameTime.TotalSeconds;
            if (timer > interval)
            {
                timer = 0;
                Position.X = Position.X + Speed.X;
                Position.Y = Position.Y + Speed.Y;
            }
        }
    }
}
