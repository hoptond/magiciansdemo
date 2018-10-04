using System;
using System.Collections.Generic;
using System.Linq;
using System.Xml;
using System.Xml.Linq;
using System.IO;
using Microsoft.Xna.Framework;


namespace Magicians
{
	class BattleStats //various stuff like health, mana, defense, etc. contained in its own class for ezness reasons
    {
        public int bonusHP;
        public int MaxHP;
        public int HP; //health. durr
        public int bonusSP;
        public int MaxSP;
        public int SP; //spell points. Further durr

        public List<BattlerTags> tags = new List<BattlerTags>();
        public Battler battler;



        public SortedList<Attributes, int> baseAttributes = new SortedList<Attributes, int>();
        public SortedList<Attributes, int> Attributes = new SortedList<Attributes, int>();

        public int baseArmour;
        public int Armour;

        public int Luck; //starts at 0, min is -2, max is +2#
        public int baseLuck;

        //Resistance is calculated in percentage so a fire resistance of 95 blocks 95% of incoming fire damage

        public SortedList<DamageTypes, int> baseResistances = new SortedList<DamageTypes, int>();
        public SortedList<DamageTypes, int> Resistances = new SortedList<DamageTypes, int>();

        public int MissRate;
        public int DodgeChance;
        public int spellDamageBonus;
        public Team team;
        public List<Modifier> Modifiers = new List<Modifier>();
        public bool canAct = true; //if false, the battler cannot do anything during battle
        public bool canTarget = true; //whether this battle can be targetted by a battleAction

        public string[] modInvulnerabilities = new string[1];
        public string[] modVulnerabilities = new string[1];

        public bool isPC;
        public PlayerCharacter pc;

        public void RecalculateStats()
        {
            canAct = true;
            canTarget = true;
            if (battler != null)
            {
                battler.Sprite.alpha = 255;
            }
            MissRate = 0;
            Luck = baseLuck;
            Armour = baseArmour;
            spellDamageBonus = 0;
            HP = HP - bonusHP;
            SP = SP - bonusSP;
            MaxHP -= bonusHP;
            MaxSP -= bonusSP;
            bonusHP = 0;
            bonusSP = 0;
            DodgeChance = 0;
            for (int i = 1; i < ((Magicians.Attributes[])Enum.GetValues(typeof(Magicians.Attributes))).Length + 1; i++)
            {
                Attributes[(Attributes)i] = baseAttributes[(Attributes)i];
            }
            for (int i = 0; i < ((Magicians.DamageTypes[])Enum.GetValues(typeof(Magicians.DamageTypes))).Length; i++)
            {
                Resistances[(Magicians.DamageTypes)i] = baseResistances[(Magicians.DamageTypes)i];
            }
            for (int x = 0; x < Modifiers.Count; x++)
            {
                if (Modifiers[x].effects != null)
                {
                    for (int i = 0; i < Modifiers[x].effects.Count; i++)
                    {
                        Modifiers[x].effects[i].Modify(this);
                    }
                }
            }
            MaxHP += bonusHP;
            MaxSP += bonusSP;
            HP += bonusHP;
            SP += bonusSP;

            DodgeChance += (this.Attributes[Magicians.Attributes.Dexterity] / 100);
            Resistances[DamageTypes.Poison] = MathHelper.Clamp(Attributes[Magicians.Attributes.Strength] / 100, 0, 99);
            for (int i = 0; i < ((Magicians.DamageTypes[])Enum.GetValues(typeof(Magicians.DamageTypes))).Length; i++)
            {
                Resistances[(Magicians.DamageTypes)i] = MathHelper.Clamp(Resistances[(Magicians.DamageTypes)i], -99, 100);
                if (baseResistances[(Magicians.DamageTypes)i] < 100 && Resistances[(Magicians.DamageTypes)i] >= 100)
                {
                    Resistances[(Magicians.DamageTypes)i] = 99;
                }
            }
            if (isPC)
            {
                if (pc.Equips[2] == null)
                    Attributes[Magicians.Attributes.Dexterity] = MathHelper.Clamp(Attributes[Magicians.Attributes.Dexterity], 1, 10);
            }
            if (SP < 0)
                SP = 0;
            if (HP < 0)
                HP = 0;
        }
        public BattleStats(Battler b)
        {
            battler = b;
        }
    }
}
