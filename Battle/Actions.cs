using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using Microsoft.Xna.Framework;

namespace Magicians
{
	class BattleAction
	{
		public string Name; //the internal name of an attack.
							//whether the action is a friendly or hostile action
		public enum Direction { Self = 1, Enemy = 2, Both = 3 }
		Direction direction;
		public Direction bDirection { get { return direction; } }
		public enum TargetType { Single = 1, All = 2, Random = 3, Self = 4 } //self denotes the effect be placed upon the caster
		TargetType type;
		public TargetType targetType { get { return type; } }
		//the initial
		public string IntText { get; set; }
		public int ManaCost { get; private set; }
		public List<IActionEffect> actionEffects = new List<IActionEffect>();
		public string[] actionSpriteEffects; //the filename to use for the graphic effect
		public Point[] actionSpriteSizes;
		public int[] actionSpriteSpeeds;
		public ActionFXType fxType { get; private set; }
		public BattlerAnimType animType;
		public string[] sounds;
		public string tag;

		public BattleAction(string name, Direction dir, TargetType targ, int man, string t, string[] g, Point[] SpriteSizes, int[] SpriteSpeeds, ActionFXType afx, BattlerAnimType at, string[] sounds, string tag)
		{
			Name = name;
			direction = dir;
			type = targ;
			IntText = t;
			ManaCost = man;
			actionSpriteEffects = g;
			actionSpriteSizes = SpriteSizes;
			actionSpriteSpeeds = SpriteSpeeds;
			fxType = afx;
			animType = at;
			this.sounds = sounds;
			this.tag = tag;
			if (this.tag == null)
				this.tag = "";
		}
		public string ReturnInventoryDescription(int number)
		{
			//TODO: add more descriptions
			if (actionEffects[number] is RestoreHealth)
			{
				var effect = (RestoreHealth)actionEffects[number];
				return "Restores " + effect.baseRestore + " health";
			}
			if (actionEffects[number] is RestoreSpellpoints)
			{
				var effect = (RestoreSpellpoints)actionEffects[number];
				return "Restores " + effect.baseRestore + " mana";
			}
			return "";
		}
		public void AddActionEffect(IActionEffect action)
		{
			actionEffects.Add(action);
		}
		public bool WasSuccessful(Battler target)
		{
			bool hasDamageEffect = false;
			for (int i = 0; i < actionEffects.Count; i++)
			{
				if (actionEffects[i] is DoDamage)
				{
					hasDamageEffect = true;
					var dmg = (DoDamage)actionEffects[i];
					if (dmg.dodged)
						return false;
					continue;
				}
				if (actionEffects[i] is ApplyModifier)
				{
					var mod = (ApplyModifier)actionEffects[i];
					if (target.BattleStats.Modifiers.Find(modif => modif.Name == mod.mod.Name) != null)
						return true;
					if (!hasDamageEffect)
						return false;
				}
				if (actionEffects[i] is DispelSpecificModifiers)
				{
					var dispel = (DispelSpecificModifiers)actionEffects[i];
					if (!dispel.Effective)
						return false;
				}
				if (actionEffects[i] is DispelModifiers)
				{
					var dispel = (DispelModifiers)actionEffects[i];
					if (!dispel.effective)
						return false;
				}
			}
			return true;
		}
	}
}