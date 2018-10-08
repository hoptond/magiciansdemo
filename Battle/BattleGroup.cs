using System;
using System.Collections.Generic;
using Microsoft.Xna.Framework;


namespace Magicians
{
	class BattleGroup
	{
		public string Name { get; private set; }
		public string[] battlers { get; private set; }
		public EncounterType encounterType;
		public string music;
		public List<Point> Positions;
		public bool CanUseItems;
		public string battleBackground;
		public int MaxPlayers;

		public bool ContinueAfterDefeat;
		public string ContinueTag;

		public BattleGroup(string n, string[] b, List<Point> positions, bool bl, string m, bool useItems, int maxPlayers, string background)
		{
			Name = n;
			battlers = b;
			if (bl == false)
				encounterType = EncounterType.Mook;
			else
				encounterType = EncounterType.Boss;
			music = m;
			Positions = positions;
			CanUseItems = useItems;
			battleBackground = background;
			MaxPlayers = maxPlayers;
		}

		public void SetContinueAfterDefeat(string s)
		{
			ContinueAfterDefeat = true;
			ContinueTag = s;
		}
	}
}
