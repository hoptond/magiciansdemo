using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using Microsoft.Xna.Framework.Graphics;
using Microsoft.Xna.Framework;


namespace Magicians
{
    class Spell
    {
        public string DisplayName { get; private set; }//the name of the spell.
        public string InternalName { get; private set; }
        public string Description { get; private set; } //the description when viewed from the spellbook
        int manaCost;
        public int Level; //the level at which this spell can be learned.
        string tex2dpath;
        public Texture2D SpellIcon { get; private set; } //the icon that will appear the UI
        public Arcana Arcana { get; private set; }
        public BattleAction BattleAction;
        public IUseEffect UseEffect;
        public Usage Usage { get; private set; }

        public Spell(string n, string i, string d,string texpath, Arcana a, Usage u, int l,int m)
        {
            DisplayName = n;
            InternalName = i;
            Description = d;
            Arcana = a;
            Level = l;
            manaCost = m;
            Usage = u;
            tex2dpath = texpath;
        }

        public void SetBattleAction(BattleAction ba)
        {
            BattleAction = ba;
            manaCost = BattleAction.ManaCost;
        }
        public void LoadIcon(Game game)
        {
            SpellIcon = game.TextureLoader.RequestTexture("UI\\Icons\\Spells\\" + tex2dpath);
        }

        public int ReturnRequiredLevel(Arcana a)
        {
            if (Arcana == a)
                return Level - 3;
            if ((int)Arcana == ((int)a - 3) || (int)Arcana == ((int)a - 3))
                return Level + 1;
            return Level;
        }

        public int ManaCost(PlayerCharacter pc)
        {
            float value = manaCost;
            if (pc.Arcana == Arcana)
                value = (value / 100) * 75f;
            else
            {
                var arc = (int)Arcana;
                if(arc == (int)pc.Arcana - 3 || arc == (int)pc.Arcana + 3)
                    value = (value / 100) * 165f;
            }
            return (int)value;
        }
    }
}