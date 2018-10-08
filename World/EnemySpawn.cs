using System;
using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;

namespace Magicians
{
	class EnemySpawn
	{
		public Rectangle SpawnBounds;
		public string BattleGroupName;
		public string SpawnFlag;
		public string KillFlag;
		public int Chance;
		public bool SpawnOnPoint;
		public EnemySpawn(Rectangle rec, string name, string spawn, string kill, int chance)
		{
			SpawnBounds = rec;
			BattleGroupName = name;
			SpawnFlag = spawn;
			KillFlag = kill;
			Chance = chance;
		}
	}
}
