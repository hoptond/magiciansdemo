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
        public string displayName { get; private set; }//the name of the spell.
        public string internalName { get; private set; }
        public string description { get; private set; } //the description when viewed from the spellbook
        int manaCost;
        public int level; //the level at which this spell can be learned.
        string tex2dpath;
        public Texture2D SpellIcon { get; private set; } //the icon that will appear the UI
        public Arcana Arcana { get; private set; }
        public BattleAction battleAction;
        public IUseEffect useEffect;
        public Usage usage { get; private set; }

        public Spell(string n, string i, string d,string texpath, Arcana a, Usage u, int l,int m)
        {
            displayName = n;
            internalName = i;
            description = d;
            Arcana = a;
            level = l;
            manaCost = m;
            usage = u;
            tex2dpath = texpath;

        }

        public Spell()
        {

        }
        public void SetBattleAction(BattleAction ba)
        {
            battleAction = ba;
            manaCost = battleAction.ManaCost;
        }
        public void LoadIcon(Game game)
        {
            SpellIcon = game.TextureLoader.RequestTexture("UI\\Icons\\Spells\\" + tex2dpath);
        }
        public int ReturnRequiredLevel(Arcana a)
        {
            if (Arcana == a)
                return level - 3;
            if ((int)Arcana == ((int)a - 3) || (int)Arcana == ((int)a - 3))
                return level + 1;
            return level;
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