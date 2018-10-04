using System;
using System.Collections.Generic;
using System.Linq;
using Microsoft.Xna.Framework;

namespace Magicians
{
    class Bounds
    {
        Entity Entity;
        public Point Position { get; private set; }
        public Point offset { get; private set; }
        public Rectangle Box { get; private set; }
		int width;
        int height;
        public bool CanPassThrough { get; private set; }
        public bool CanFlyOver { get; private set; }
        public void Update()
        {
            Position = new Point(Entity.Position.X + offset.X, Entity.Position.Y + offset.Y);
            Box = new Rectangle(Position.X, Position.Y, width, height);
        }
        public Bounds(Entity ent, Point pos, int w, int h, bool b, Point offs)
        {
            CanFlyOver = false;
            Entity = ent;
            width = w;
            height = h;
            CanPassThrough = b;
            offset = offs;
            Position = new Point(pos.X + offset.X,pos.Y + offset.Y);
            Box = new Rectangle(Position.X, Position.Y, width, height);
        }
        public void ChangeCanPassThrough(bool newState)
        {
            CanPassThrough = newState;
        }
        public void ChangeCanFlyOver(bool newState)
        {
            CanFlyOver = newState;
        }
        public static bool IntersectsLine(Rectangle Box, Point s, Point e)
        {
            for (int i = 0; i < 4; i++)
            {
                int a = 0, b = 0;
                int x = 0, y = 0;
                switch (i)
                {
                    case (0): a = Box.Left + 1; x = Box.Left + 1; b = Box.Top + 1; y = Box.Bottom - 1; break; //left side
                    case (1): a = Box.Right - 1; x = Box.Right - 1; b = Box.Top + 1; y = Box.Bottom - 1; break; //right side
                    case (2): a = Box.Left + 1; x = Box.Right - 1; b = Box.Top + 1; y = Box.Top + 1; break; //top side
                    case (3): a = Box.Left + 1; x = Box.Right - 1; b = Box.Bottom - 1; y = Box.Bottom - 1; break; //bottom side

                }
                if (Intersects(s.ToVector2(), e.ToVector2(), new Vector2(a, b), new Vector2(x, y)))
                    return true;
            }
                return false;
        }
        public static bool Intersects(Vector2 a1, Vector2 a2, Vector2 b1, Vector2 b2)
        {
            Vector2 b = a2 - a1;
            Vector2 d = b2 - b1;
            float bDotDPerp = b.X * d.Y - b.Y * d.X;
            if (bDotDPerp == 0)
                return false;
            Vector2 c = b1 - a1;
            float t = (c.X * d.Y - c.Y * d.X) / bDotDPerp;
            if (t < 0 || t > 1)
                return false;
            float u = (c.X * b.Y - c.Y * b.X) / bDotDPerp;
            if (u < 0 || u > 1)
                return false;
            return true;
        }

    }
}
