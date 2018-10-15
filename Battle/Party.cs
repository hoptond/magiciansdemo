using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using Microsoft.Xna.Framework.Graphics;
using System.Xml;
using System.IO;
using System.Xml.Linq;

namespace Magicians
{
    class Party
    {
	readonly Game game;
	public int Gold;
        public List<Item> TempInventory = new List<Item>(16);
        public QuestStats QuestStats = new QuestStats();
        public List<PlayerCharacter> PlayerCharacters;
        public List<int> ActiveCharacters = new List<int>(); //characters currently in the party
        public List<int> InactiveCharacters = new List<int>(); //characters currently out of the party
        public List<int> LockedCharacters = new List<int>(); //characters that have not joined the party yet

        public int[] lastPartyComp = new int[3]; //refers to the full composition of the last party you had.

        public Party(Game game)
        {
            this.game = game;
            PlayerCharacters = new List<PlayerCharacter>();
        }
        public PlayerCharacter GetPlayerCharacter(int id)
        {
            for(int i = 0; i < PlayerCharacters.Count; i++)
            {
                if(PlayerCharacters[i].ID == id)
                {
                    return PlayerCharacters[i];
                }
            }
            return null;
        }
        //removes the character from the ActiveCharacters list and puts them into the inactive characters list
	public void RemoveCharacterFromParty(int id, bool lockPC) 
        {
            var pc = GetPlayerCharacter(id);
            if (pc != null && ActiveCharacters.Contains(pc.ID))
            {
                var map = (Map)game.Scene;
                map.RemoveEntity("ENT_" + pc.Name.ToUpper());
                ActiveCharacters.Remove(id);
                game.gameFlags["b" + pc.Name + "InParty"] = false;
				if (!lockPC)
					InactiveCharacters.Add(id);
				else
					LockedCharacters.Add(id);
            }
        }
        //if the character is joining the party for the first time, boost levels to match player's
        public void AddCharacterToParty(int id,bool boostLevels, Map map, bool viaEvent)
        {
            var pc = GetPlayerCharacter(id);
            if (pc == null)
                return;
			if (ActiveCharacters.Contains(id))
                return;
            if(ActiveCharacters.Count == 3 && !viaEvent)
            {
                lastPartyComp[0] = ActiveCharacters[1];
                lastPartyComp[1] = ActiveCharacters[2];
                lastPartyComp[2] = pc.ID;
            }
            ActiveCharacters.Add(id);
            LockedCharacters.Remove(id);
            InactiveCharacters.Remove(id);
            game.gameFlags["b" + pc.Name + "InParty"] = true;
            if (boostLevels)
            {
                var Player = GetPlayerCharacter(0);
                while (Player.Level - pc.Level > 3)
                {
                    pc.Exp += pc.ExpRequirements[pc.Level];
                    pc.TotalExp += (uint)pc.ExpRequirements[pc.Level];
                    pc.LevelUp(game);
                }
                pc.NextLevelExp = pc.ExpRequirements[pc.Level - 1];
            }
            map.SpawnPlayerEntity(pc);
        }    
    }
}
