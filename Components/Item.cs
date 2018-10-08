using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Content;
using Microsoft.Xna.Framework.Graphics;

namespace Magicians
{
	class Item
	{
		public readonly string DisplayName;
		public readonly string InternalName;
		readonly string iconFile; //the file path to the icon that will appear. filename only, 
		public Texture2D Icon { get; private set; }

		public readonly int Value; //value in gold
		public string Description { get; private set; } //cosmetic description of the item
		public readonly Usage Usage;


		public void Load(TextureLoader tl, SpriteFont smallFont)
		{
			Icon = tl.RequestTexture("UI\\Icons\\Items\\" + iconFile);
			Description = TextMethods.WrapText(smallFont, Description, 200);
		}

		public Item(string n, string i, string g, int v, string d, Usage us)
		{
			DisplayName = n;
			InternalName = i;
			iconFile = g;
			Value = v;
			Description = d;
			Usage = us;
		}
	}
	class PlotItem : Item
	{
		public PlotItem(string n, string i, string g, int v, string d)
			: base(n, i, g, v, d, Usage.World)
		{

		}
	}
	class EquippableItem : Item
	{
		public readonly EquipSlot EquipSlot;
		public readonly List<IModifyEffect> equipEffects;
		public readonly IEffectPerTurn effectPerTurn;
		public EquippableItem(string n, string i, string g, int v, string d, EquipSlot es, List<IModifyEffect> equipEffects)
			: base(n, i, g, v, d, Usage.World)
		{
			EquipSlot = es;
			this.equipEffects = equipEffects;
		}
		public EquippableItem(string n, string i, string g, int v, string d, EquipSlot es, List<IModifyEffect> equipEffects, IEffectPerTurn ept)
			: base(n, i, g, v, d, Usage.World)
		{
			EquipSlot = es;
			this.equipEffects = equipEffects;
			effectPerTurn = ept;
		}
	}
	class ConsumableItem : Item
	{
		public IUseEffect UseEffect;
		public readonly BattleAction BattleAction;
		public readonly string Recipie;
		public readonly bool Reusable; //by default is set to false, must be specified in Items.xml with reusable ="true" attribute

		public ConsumableItem(string n, string i, string g, int v, string d, Usage us)
			: base(n, i, g, v, d, us)
		{
			Recipie = "";
		}
		public ConsumableItem(string n, string i, string g, int v, string d, Usage usage, BattleAction ba)
			: base(n, i, g, v, d, usage)
		{
			Recipie = "";
			BattleAction = ba;
		}
		public ConsumableItem(string n, string i, string g, int v, string d, Usage us, BattleAction ba, string recip)
			: base(n, i, g, v, d, us)
		{
			BattleAction = ba;
			Recipie = recip;
		}
		public ConsumableItem(string n, string i, string g, int v, string d, Usage us, IUseEffect useEffect)
			: base(n, i, g, v, d, us)
		{
			Recipie = "";
			UseEffect = useEffect;
		}
		public ConsumableItem(string n, string i, string g, int v, string d, Usage us, IUseEffect useEffect, string recip)
			: base(n, i, g, v, d, us)
		{

			UseEffect = useEffect;
			Recipie = recip;
		}
		public ConsumableItem(string n, string i, string g, int v, string d, Usage us, IUseEffect useEffect, BattleAction ba, string recip, bool reus)
			: base(n, i, g, v, d, us)
		{
			BattleAction = ba;
			UseEffect = useEffect;
			Recipie = recip;
			Reusable = reus;
		}
	}

	class BookItem : Item
	{
		public readonly Spell[] Spells;
		public readonly bool Library;
		public BookItem(string n, string i, string g, int v, string d, Spell[] spls, bool l) : base(n, i, g, v, d, Usage.World)
		{
			Library = l;
			Spells = spls;
		}

	}

	interface IUseEffect//the effects of using this item in the world
	{
		void UseEffects();
	}
	class AggroEffect : IUseEffect
	{
		readonly Game game;
		readonly Map map;
		readonly Walker target;
		readonly string caster;
		readonly Walker player;
		public void UseEffects()
		{
			var events = new List<IEvent>();
			map.FaceEntity((Walker)map.GetEntityFromName(caster), target);
			var s = map.GetEntityFromName(caster).Sprite.GraphicsDir;
			events.Add(new BeginEvent(game, map));
			//events.Add(new PlayCustomSprite(caster, "Sprites\\" + s + "\\Custom\\CastSpell" + map.GetEntityFromName(caster).Mover.direction.ToString()));
			events.Add(new PlaySound(game, "AggroCast"));
			events.Add(new EndEvent(game, map.EventManager, map));
			map.EventManager.SetEvents(events, true);
			map.EventManager.DoEvent();
			var enemybehav = (EnemyBehaviour)target.Behaviour;
			enemybehav.overrideRange = true;
			enemybehav.ToggleChasing();
		}
		public AggroEffect()
		{

		}
		public AggroEffect(Game g, Map m, string c, Walker e)
		{
			game = g;
			player = (Walker)m.Entities[1];
			caster = c;
			target = e;
			map = m;
		}
	}

	class CustomEventEffect : IUseEffect
	{
		readonly Game game;
		readonly Map map;
		public readonly string eventFile;
		public void UseEffects()
		{
			map.EventManager.SetEvents(Events.ParseEventFromFile(eventFile, game, map, null, true), true);
			map.EventManager.DoEvent();
		}
		public CustomEventEffect(Game g, Map m, string e)
		{
			game = g;
			map = m;
			eventFile = e;
		}
		public CustomEventEffect(string e)
		{
			eventFile = e;
		}
	}
	class SpellTriggeredEvent : IUseEffect
	{
		public readonly string spellCond;
		public SpellTriggeredEvent(string s)
		{
			spellCond = s;
		}
		public void UseEffects()
		{
			//this method doesn't do anything, as this UseEffect is held as a comparison. all the events will be associated with the entitty, not the useeffect. shitcode
			//at its finest
		}
	}
	class LightTorch : IUseEffect
	{
		readonly Game game;
		readonly Map map;
		readonly Entity torch;
		readonly string caster;
		public LightTorch()
		{

		}
		public void UseEffects()
		{
			map.FaceEntity((Walker)map.GetEntityFromName(caster), torch);
			var s = map.GetEntityFromName(caster).Sprite.GraphicsDir;
			//events.Add(new PlayCustomSprite(caster, "Sprites\\" + s + "\\Custom\\CastSpell" + map.GetEntityFromName(caster).Mover.direction.ToString()));
			game.Audio.PlaySound("TorchLight", true);
			var lt = torch.EntBehaviour as LightableTorch;
			s = "Sprites\\" + lt.litTorchPath;
			torch.Sprite.ChangeSprite(game.TextureLoader, s);
			for (int i = 0; i < map.WorldOverlays.Count; i++)
			{
				if (map.WorldOverlays[i] is Overlay)
				{
					var over = (Overlay)map.WorldOverlays[i];
					over.AlterAlpha(-100);
				}
			}
			for (int i = 0; i < map.Entities.Count; i++)
			{
				if (map.Entities[i] is Walker)
				{
					if (((Walker)map.Entities[i]).Behaviour is EnemyBehaviour)
					{
						var walker = (Walker)map.Entities[i];
						var behav = (EnemyBehaviour)walker.Behaviour;
						if (behav.behaviours.Contains(Behaviours.FreezeOnLight))
							((Walker)map.Entities[i]).Mover.ChangeSpeed(walker.Mover.Speed - 1);
					}
				}
			}
		}
		public LightTorch(Game g, Map m, Entity e, string c)
		{
			game = g;
			map = m;
			torch = e;
			caster = c;
		}
	}
	class BlindEntity : IUseEffect
	{
		readonly Game game;
		readonly Map map;
		readonly string caster;
		readonly Walker target;
		public BlindEntity()
		{

		}
		public BlindEntity(Game g, Map m, Walker e, string s)
		{
			game = g;
			map = m;
			target = e;
			caster = s;
		}
		public void UseEffects()
		{
			var events = new List<IEvent>();
			map.FaceEntity((Walker)map.GetEntityFromName(caster), target);
			var s = map.GetEntityFromName(caster).Sprite.GraphicsDir;
			events.Add(new BeginEvent(game, map));
			//events.Add(new PlayCustomSprite(caster, "Sprites\\" + s + "\\Custom\\CastSpell" + map.GetEntityFromName(caster).Mover.direction.ToString()));
			events.Add(new PlaySound(game, "BlindCast"));
			events.Add(new EndEvent(game, map.EventManager, map));
			map.EventManager.SetEvents(events);
			var behav = (EyeRotate)target.Behaviour;
			behav.OnBlindSpell();
		}
	}
	class RevealHiddenObjects : IUseEffect
	{
		readonly Game game;
		readonly Map map;
		readonly string caster;
		readonly List<Entity> revealedEnts = new List<Entity>();
		public void UseEffects()
		{
			var events = new List<IEvent>();
			var s = map.GetEntityFromName(caster).Sprite.GraphicsDir;
			events.Add(new PlaySound(game, "RevealWorld"));
			events.Add(new PlayWorldEffect(game, "OverlayFlashBlue", map.Entities[0].Name, map, new Point(game.GetScreenWidth(), game.GetScreenHeight())));
		}
		public RevealHiddenObjects(Game g, Map m, string c)
		{
			game = g;
			map = m;
			caster = c;
			for (int i = 0; i < m.Entities.Count; i++)
			{
				if (m.Entities[i].EntBehaviour is HiddenEntity)
				{
					revealedEnts.Add(m.Entities[i]);
				}
			}
		}
		public RevealHiddenObjects()
		{

		}
	}
	class FleeDungeon : IUseEffect
	{
		readonly Game game;
		List<IEvent> events = new List<IEvent>();
		public void UseEffects()
		{
			var map = (Map)game.Scene;
			if (map.FleeDungeonMap != null || map.FleeDungeonMap != null)
			{
				events.Add(new BeginEvent(game, map));
				events.Add(new PlayDialogue(game, map, game.LoadString("Common", "EscapeSuccess")));
				events.Add(new PlaySound(game, "MagicFlare"));
				events.Add(new PlayWorldEffect(game, "MagicFlare", map.Entities[0].Name, map, new Point(64)));
				events.Add(new ChangeMap(game, map.FleeDungeonMap, map.FleeDungeonPoint));
				events.Add(new EndEvent(game, map.EventManager, map));
				map.EventManager.SetEvents(events);
				map.EventManager.DoEvent();
				return;
			}
			events.Add(new BeginEvent(game, map));
			events.Add(new PlayDialogue(game, map, game.LoadString("Common", "EscapeFail")));
			events.Add(new PlaySound(game, "MagicFlare"));
			events.Add(new PlayWorldEffect(game, "MagicFlare", map.Entities[1].Name, map, new Point(64)));
			events.Add(new EndEvent(game, map.EventManager, map));
			map.EventManager.SetEvents(events);
			map.EventManager.DoEvent();
		}
		public FleeDungeon()
		{

		}
		public FleeDungeon(Game g)
		{
			game = g;
		}
	}
}