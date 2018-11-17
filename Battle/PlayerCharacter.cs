using System;
using System.Collections.Generic;
using System.Xml;
using System.Xml.Linq;
using System.Linq;
using Microsoft.Xna.Framework.Content;
using Microsoft.Xna.Framework.Graphics;

namespace Magicians
{
	class PlayerCharacter
	{
		public int ID;
		public string Name;
		public string GraphicsFolderName;

		public Gender Gender;

		public Texture2D uiPortrait; //used in inventory/shops

		public int Level;
		public int[] StatGain = new int[3];
		public uint TotalExp;
		public int Exp; //Exp require for the next level, not total Exp
		public int NextLevelExp;
		public SortedList<int, int> ExpRequirements = new SortedList<int, int>();

		public BattleStats BattleStats;

		public List<Item> Inventory = new List<Item>();
		public List<Spell> Spells = new List<Spell>();
		public List<int> LearnableSpellLevels = new List<int>();
		public List<Spell> LearnableSpells = new List<Spell>();

		public EquippableItem[] Equips = new EquippableItem[4]; //refers to index of Inventory

		public Arcana Arcana;

		public void GetLevelUps(Game game)
		{
			var args = System.IO.File.ReadAllText(game.Content.RootDirectory + game.PathSeperator + "Data" + game.PathSeperator + "levelups.txt").Split(';');
			for (int i = 0; i < args.Length - 1; i++)
			{
				if (args[i].StartsWith("\r\n"))
				{
					args[i] = args[i].Remove(0, 2);
				}//remove any escape sequences
				args[i] = args[i].TrimEnd(';');
				ExpRequirements.Add(i, int.Parse(args[i]));
			}
			NextLevelExp = ExpRequirements[Level - 1];
		}
		public bool LevelUp(Game game)
		{
			var val = false;
			if (Level < 40)
			{
				Level++;
				Exp -= NextLevelExp;
				BattleStats.MaxHP += StatGain[0] * 4;
				BattleStats.MaxSP += StatGain[1] * 4;
				BattleStats.HP += StatGain[0] * 4;
				BattleStats.SP += StatGain[1] * 4;
				BattleStats.baseAttributes[Magicians.Attributes.Strength] += StatGain[0];
				BattleStats.baseAttributes[Magicians.Attributes.Magic] += StatGain[1];
				BattleStats.baseAttributes[Magicians.Attributes.Dexterity] += StatGain[2];
				var randomStatBoost = game.randomNumber.Next(1, 3);
				BattleStats.baseAttributes[(Attributes)randomStatBoost] += 1;
				if (randomStatBoost == 0)
				{
					BattleStats.MaxHP += 4;
					BattleStats.HP += 4;
				}
				if (randomStatBoost == 1)
				{
					BattleStats.MaxSP += 4;
					BattleStats.SP += 4;
				}
				if (Level < 40)
					NextLevelExp = ExpRequirements[this.Level - 1];
				else
					NextLevelExp = -1;
				BattleStats.RecalculateStats();
				if (LearnableSpellLevels != null)
				{
					while (LearnableSpellLevels.Contains(Level))
					{
						if (!Spells.Contains(LearnableSpells[0]))
						{
							Spells.Add(LearnableSpells[0]);
							LearnableSpells.RemoveAt(0);
							val = true;
						}
						LearnableSpellLevels.RemoveAt(0);
					}
				}
				if (Exp > NextLevelExp)
				{
					LevelUp(game);
				}
			}
			return val;
		}
		//creating characters from an XML element
		public PlayerCharacter(Game game, XElement element)
		{
			ID = int.Parse(element.Attribute("id").Value);
			Name = element.Attribute("name").Value;
			GraphicsFolderName = element.Attribute("spriteDir").Value;
			Level = int.Parse(element.Attribute("level").Value);
			Exp = int.Parse(element.Attribute("exp").Value);
			NextLevelExp = int.Parse(element.Attribute("nextExp").Value);
			TotalExp = uint.Parse(element.Attribute("totalExp").Value);
			switch (element.Attribute("gender").Value)
			{
				case "Male": Gender = Gender.Male; break;
				case "Female": Gender = Gender.Female; break;
			}
			switch (element.Attribute("arcana").Value)
			{
				case "Light": Arcana = Arcana.Light; break;
				case "Shadow": Arcana = Arcana.Shadow; break;
				case "Fire": Arcana = Arcana.Fire; break;
				case "Water": Arcana = Arcana.Water; break;
				case "Earth": Arcana = Arcana.Earth; break;
				case "Wind": Arcana = Arcana.Wind; break;
			}
			Inventory.Capacity = 16;
			Spells.Capacity = 8;
			BattleStats = new BattleStats(null);
			BattleStats.MaxHP = int.Parse(element.Attribute("maxHP").Value);
			BattleStats.MaxSP = int.Parse(element.Attribute("maxSP").Value);
			BattleStats.HP = int.Parse(element.Attribute("hp").Value);
			BattleStats.SP = int.Parse(element.Attribute("sp").Value);
			BattleStats.baseArmour = int.Parse(element.Attribute("armour").Value);
			var statEls = element.Element("Stats").Descendants("Stat").ToArray<XElement>();
			BattleStats.baseAttributes.Add(Magicians.Attributes.Strength, int.Parse(statEls[0].Value));
			BattleStats.baseAttributes.Add(Magicians.Attributes.Magic, int.Parse(statEls[1].Value));
			BattleStats.baseAttributes.Add(Magicians.Attributes.Dexterity, int.Parse(statEls[2].Value));
			foreach (KeyValuePair<Attributes, int> kvp in BattleStats.baseAttributes)
			{
				BattleStats.Attributes.Add(kvp.Key, kvp.Value);
			}
			var statGns = element.Element("Gains").Descendants("Gain").ToArray<XElement>();
			var gns = new List<int>();
			gns.Add(int.Parse(statGns[0].Value));
			gns.Add(int.Parse(statGns[1].Value));
			gns.Add(int.Parse(statGns[2].Value));
			StatGain[0] = gns[0];
			StatGain[1] = gns[1];
			StatGain[2] = gns[2];
			BattleStats.baseResistances.Add(DamageTypes.Physical, 0);
			BattleStats.baseResistances.Add(DamageTypes.Fire, 0);
			BattleStats.baseResistances.Add(DamageTypes.Cold, 0);
			BattleStats.baseResistances.Add(DamageTypes.Electricity, 0);
			BattleStats.baseResistances.Add(DamageTypes.Poison, 0);
			BattleStats.baseResistances.Add(DamageTypes.Raw, 0);
			BattleStats.baseResistances.Add(DamageTypes.Light, 100);
			foreach (KeyValuePair<DamageTypes, int> kvp in BattleStats.baseResistances)
			{
				BattleStats.Resistances.Add(kvp.Key, kvp.Value);
			}
			var items = element.Element("Items").Descendants("Item").ToList();
			for (int i = 0; i < items.Count; i++)
			{
				Inventory.Add(game.Items.Find(p => p.InternalName == items[i].Value));
			}
			var spells = element.Element("Spells").Descendants("Spell").ToList();
			for (int i = 0; i < spells.Count; i++)
			{
				Spells.Add(game.Spells.Find(p => p.InternalName == spells[i].Value));
			}
			Equips = new EquippableItem[4];
			var equipEls = element.Element("Equips").Descendants("Equip").ToArray();
			for (int i = 0; i < equipEls.Length; i++)
			{
				for (int e = 0; e < Inventory.Count; e++)
				{
					if (Inventory[e].InternalName == equipEls[i].Value)
					{
						EquipItem(e);
					}
				}
			}
			var learnSpells = element.Element("LearnSpells").Descendants("LevelSpell").ToArray();
			for (int i = 0; i < learnSpells.Length; i++)
			{
				var args = learnSpells[i].Value.Split('|');
				LearnableSpellLevels.Add(int.Parse(args[0]));
				LearnableSpells.Add(game.Spells.Find(p => p.InternalName == args[1]));
			}


		}
		public PlayerCharacter(Game game, CharacterSave charsave)
		{
			ID = charsave.ID;
			BattleStats = new BattleStats(null);
			Name = charsave.Name;
			BattleStats.HP = charsave.HpSp[0];
			BattleStats.MaxHP = charsave.HpSp[1];
			BattleStats.SP = charsave.HpSp[2];
			BattleStats.MaxSP = charsave.HpSp[3];
			BattleStats.baseAttributes.Add(Magicians.Attributes.Strength, charsave.Stats[0]);
			BattleStats.baseAttributes.Add(Magicians.Attributes.Magic, charsave.Stats[1]);
			BattleStats.baseAttributes.Add(Magicians.Attributes.Dexterity, charsave.Stats[2]);
			StatGain = charsave.StatGain.ToArray();
			Inventory.Capacity = 16;
			Spells.Capacity = 8;
			foreach (KeyValuePair<Attributes, int> kvp in BattleStats.baseAttributes)
			{
				BattleStats.Attributes.Add(kvp.Key, kvp.Value);
			}
			BattleStats.baseResistances.Add(DamageTypes.Physical, 0);
			BattleStats.baseResistances.Add(DamageTypes.Fire, 0);
			BattleStats.baseResistances.Add(DamageTypes.Cold, 0);
			BattleStats.baseResistances.Add(DamageTypes.Electricity, 0);
			BattleStats.baseResistances.Add(DamageTypes.Poison, 0);
			BattleStats.baseResistances.Add(DamageTypes.Raw, 0);
			BattleStats.baseResistances.Add(DamageTypes.Light, 100);

			foreach (KeyValuePair<DamageTypes, int> kvp in BattleStats.baseResistances)
			{
				BattleStats.Resistances.Add(kvp.Key, kvp.Value);
			}
			this.Arcana = charsave.Arcana;
			for (int i = 0; i < charsave.Inventory.Count; i++)
			{
				Inventory.Add(game.Items.Find(p => p.InternalName == charsave.Inventory[i]));
			}
			for (int i = 0; i < charsave.Equips.Length; i++)
			{
				for (int e = 0; e < Inventory.Count; e++)
				{
					if (Inventory[e].InternalName == charsave.Equips[i])
					{
						EquipItem(e);
					}
				}
			}
			for (int i = 0; i < charsave.Spells.Count; i++)
			{
				for (int x = 0; x < game.Spells.Count; x++)
				{
					if (game.Spells[x].InternalName == charsave.Spells[i])
					{
						LearnSpell(game.Spells[x]);
						break;
					}
				}
			}

			Level = charsave.Level;
			Exp = charsave.Exp;
			TotalExp = charsave.TotalExp;
			NextLevelExp = charsave.NextLevelExp;
			BattleStats.RecalculateStats();
			GraphicsFolderName = charsave.SpriteFolder;
			Gender = charsave.Gender;
			if (Spells.Count > 8)
			{
				Spells.RemoveRange(8, Spells.Count - 8);
			}
			Spells.Capacity = 8;
			GetLevelUps(game);

			LearnableSpellLevels = charsave.LearnLevels;
			if (charsave.LearnSpells != null)
			{
				foreach (string s in charsave.LearnSpells)
				{
					var spell = game.Spells.Find(spl => spl.InternalName == s);
					if (spell != null)
					{
						LearnableSpells.Add(spell);
					}
				}
			}
		}
		public PlayerCharacter(Game game, int id, string n, int[] hpsp, int[] s, int[] g, string gra, Gender gen, Arcana a)
		{
			Name = n;
			ID = id;
			StatGain = g;
			Inventory.Capacity = 16;
			Spells.Capacity = 8;
			//stats
			BattleStats = new BattleStats(null);
			BattleStats.baseAttributes.Add(Magicians.Attributes.Strength, s[0]);
			BattleStats.baseAttributes.Add(Magicians.Attributes.Magic, s[1]);
			BattleStats.baseAttributes.Add(Magicians.Attributes.Dexterity, s[2]);


			BattleStats.MaxHP = hpsp[0];
			BattleStats.HP = BattleStats.MaxHP;
			BattleStats.MaxSP = hpsp[1];
			BattleStats.SP = BattleStats.MaxSP;
			this.Arcana = a;

			foreach (KeyValuePair<Attributes, int> kvp in BattleStats.baseAttributes)
			{
				BattleStats.Attributes.Add(kvp.Key, kvp.Value);
			}
			BattleStats.baseResistances.Add(DamageTypes.Physical, 0);
			BattleStats.baseResistances.Add(DamageTypes.Fire, 0);
			BattleStats.baseResistances.Add(DamageTypes.Cold, 0);
			BattleStats.baseResistances.Add(DamageTypes.Electricity, 0);
			BattleStats.baseResistances.Add(DamageTypes.Poison, 0);
			BattleStats.baseResistances.Add(DamageTypes.Raw, 0);
			BattleStats.baseResistances.Add(DamageTypes.Light, 100);

			foreach (KeyValuePair<DamageTypes, int> kvp in BattleStats.baseResistances)
			{
				BattleStats.Resistances.Add(kvp.Key, kvp.Value);
			}

			for (int i = 0; i < Equips.Length; i++)
			{
				Equips[i] = null;
			}
			GraphicsFolderName = gra;
			Gender = gen;
			Level = 1;
			GetLevelUps(game);
		}


		public void LearnSpell(Spell spell)
		{
			if (!Spells.Contains(spell))
			{
				Spells.Add(spell);
			}
		}

		public void EquipItem(int i) //also unEquips items and stuff yeman
		{
			if (this.Inventory.Count == 0 || i == -1)
			{
				return;
			}
			if ((this.Inventory[i] is EquippableItem) == false)
			{
				Console.WriteLine("PANIC: ATTEMPTED TO EQUIP NON EQUIPABLE ITEM");
				return;
			}
			var Equipslot = (int)((EquippableItem)Inventory[i]).EquipSlot;
			if (Equips[Equipslot] == Inventory[i])
			{
				Equips[Equipslot] = null;
				for (int x = 0; x < BattleStats.Modifiers.Count; x++)
				{
					if (BattleStats.Modifiers[x] is EquipmentModifier)
					{
						BattleStats.Modifiers.RemoveAt(x);
						break;
					}
				}
				BattleStats.Modifiers.Add(new EquipmentModifier(this));
				BattleStats.RecalculateStats();
				return;
			}
			if (Equips[Equipslot] != Inventory[i])
			{
				Equips[Equipslot] = (EquippableItem)Inventory[i];
				for (int x = 0; x < BattleStats.Modifiers.Count; x++)
				{
					if (BattleStats.Modifiers[x] is EquipmentModifier)
					{
						BattleStats.Modifiers.RemoveAt(x);
						break;
					}
				}
				BattleStats.Modifiers.Add(new EquipmentModifier(this));
				BattleStats.RecalculateStats();
				return;
			}
		}
		public void RemoveItemFromInventory(int itemno)
		{
			Item item = Inventory[itemno];
			if (item is EquippableItem)
			{
				bool hasSpareItem = false;
				int spareItemNo = -1;
				for (int i = 0; i < Inventory.Count; i++)
				{
					if (i != itemno && item == Inventory[i])
					{
						hasSpareItem = true;
						spareItemNo = i; break;
					}
				}
				for (int i = 0; i < Equips.Length; i++)
				{
					if (Equips[i] == Inventory[itemno])
					{
						EquipItem(itemno);
						break;
					}
				}
				if (hasSpareItem)
					EquipItem(itemno);
			}
			Inventory.RemoveAt(itemno);
		}
	}

}
