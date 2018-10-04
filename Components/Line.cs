using System;
using Microsoft.Xna.Framework;

namespace Magicians
{
	class Line
    {
        public Point Start;
        public Point End;
        public Line(Point s, Point e)
        {
            Start = s;
            End = e;
        }
        public int Length()
        {
            return (int)Math.Sqrt(Math.Pow((End.Y - Start.Y), 2) + Math.Pow((End.X - Start.X), 2));
        }
        public bool CompareAgainst(Line s)
        {
            if (Start.X == s.Start.X && Start.Y == s.Start.Y)
                if (End.Y == s.End.Y && End.X == s.End.X)
                    return true;
            return false;
        }
    }
}
