using System;
using Microsoft.Xna.Framework;

namespace Magicians
{
	class Node
    {
        public Point RealLocation { get; private set; } //the location of the node in the gameworld
        public Point ArrayLocation { get; private set; } //the location of the node in the array
        public NodeType Type { get; set; }
        public Data SearchData;
        public class Data
        {
            public Data(Node parentNode, Node thisNode, Node endNode)
            {
                ParentNode = parentNode;
                G = ParentNode.SearchData.G + Heuristic(thisNode.ArrayLocation, ParentNode.ArrayLocation);
                H = Heuristic(thisNode.ArrayLocation, endNode.ArrayLocation) * 1.001f;

            }
            public Data(Node thisNode, Node endNode)
            {
                G = 0;
                H = Heuristic(thisNode.ArrayLocation, endNode.ArrayLocation);
            }
            public Node ParentNode;
            public float F { get { return G + H; } }
            public float G;
            public float H;
            public bool Used { get; set; }
            public bool Closed { get; set; }
            public bool Open { get; set; }
        }
        public Node(Point rl, Point al, NodeType nt)
        {
            RealLocation = rl;
            ArrayLocation = al;
            Type = nt;
        }
        internal static float Heuristic(Point location, Point otherLocation)
        {
            float x = Math.Abs(location.X - otherLocation.X);
            float y = Math.Abs(location.Y - otherLocation.Y);
            return (x + y);
        }
    }
    public enum NodeType
    {
        Clear = 1,
        BlocksAll = 2,
        BlocksNonFlying = 3,
        OutOfBounds = 4
    }
}
