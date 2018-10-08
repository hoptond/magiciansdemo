using System;
using System.Collections.Generic;
using System.Linq;
using Microsoft.Xna.Framework;

namespace Magicians
{
	class PathFinder
	{
		int width;
		int height;
		public Node[,] nodes;
		Node startNode;
		Node endNode;
		List<Node> openNodes = new List<Node>();
		List<Node> closedNodes = new List<Node>();
		SearchType searchType;

		public PathFinder(Node[,] nodes, Node start, Node End, SearchType st)
		{
			this.nodes = nodes;
			startNode = start;
			endNode = End;
			width = nodes.GetLength(0);
			height = nodes.GetLength(1);
			searchType = st;
		}
		public List<Node> FindPath()
		{
			var path = new List<Node>();
			openNodes.Add(startNode);
			startNode.SearchData = new Node.Data(startNode, endNode);
			while (openNodes.Count > 0)
			{
				Node currentNode = null;
				currentNode = openNodes[0];
				openNodes.Sort((node1, node2) => node1.SearchData.F.CompareTo(node2.SearchData.F));
				if (currentNode.ArrayLocation == endNode.ArrayLocation)
				{
					Node node = endNode;
					while (node.SearchData.ParentNode != null)
					{
						node.SearchData.Used = true;
						path.Add(node.SearchData.ParentNode);
						node = node.SearchData.ParentNode;
					}
					path.Reverse();
					return path;
				}
				openNodes.Remove(currentNode);
				currentNode.SearchData.Open = false;
				currentNode.SearchData.Closed = true;
				closedNodes.Add(currentNode);
				//TODO: add diag nodes when we make our sort method not horrifically slow
				var adjacentNodes = GetAdjacentWalkableNodes(nodes, currentNode, searchType, false);
				foreach (Node neighbour in adjacentNodes)
				{
					if (closedNodes.Contains(neighbour))
						continue;
					if (openNodes.Contains(neighbour))
					{
						float gScore = Node.Heuristic(currentNode.ArrayLocation, neighbour.ArrayLocation) + currentNode.SearchData.G;
						if (gScore < neighbour.SearchData.G)
							neighbour.SearchData = new Node.Data(currentNode, neighbour, endNode);
					}
					else
					{
						neighbour.SearchData = new Node.Data(currentNode, neighbour, endNode);
						openNodes.Add(neighbour);
						neighbour.SearchData.Open = true;
					}
				}
			}
			return new List<Node>();
		}
		internal static List<Node> GetAdjacentWalkableNodes(Node[,] nodes, Node fromNode, SearchType st, bool diagonals)
		{
			var nextLocations = new List<Node>();
			for (int d = 1; d <= 8; d += 1)
			{
				var p = Mover.ReturnDirectionPoint((Directions)d) + fromNode.ArrayLocation;
				if (p.X < 1 || p.X > nodes.GetLength(0) - 1 || p.Y < 1 || p.Y > nodes.GetLength(1) - 1)
					continue;
				Node n = nodes[p.X, p.Y];
				if (IsWalkable(st, n))
					nextLocations.Add(n);
				if (!diagonals)
					d += 1;
			}
			return nextLocations;
		}
		bool isDiagonal(Node fromNode, Node nextNode)
		{
			if (fromNode.ArrayLocation.X != nextNode.ArrayLocation.X && fromNode.ArrayLocation.Y != nextNode.ArrayLocation.Y)
				return true;
			return false;
		}
		internal static bool IsWalkable(SearchType st, Node node)
		{
			if (st == SearchType.Flying)
			{
				if (node.Type == NodeType.BlocksNonFlying || node.Type == NodeType.Clear)
					return true;
			}
			else
			{
				if (node.Type == NodeType.Clear)
					return true;
			}
			return false;
		}
		List<Node> getDiagonalAdjacentLocations(Node fromNode)
		{
			var nds = new List<Node>();
			nds.Add(nodes[fromNode.ArrayLocation.X - 1, fromNode.ArrayLocation.Y]);
			nds.Add(nodes[fromNode.ArrayLocation.X - 1, fromNode.ArrayLocation.Y]);
			nds.Add(nodes[fromNode.ArrayLocation.X - 1, fromNode.ArrayLocation.Y]);
			nds.Add(nodes[fromNode.ArrayLocation.X - 1, fromNode.ArrayLocation.Y]);
			return nds;
		}
	}
	public enum SearchType
	{
		Normal,
		Flying
	}
}