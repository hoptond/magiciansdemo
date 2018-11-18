using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;
using Microsoft.Xna.Framework.Input;

namespace Magicians
{
	class Spellbook : BaseUIWindow
	{
		readonly Game game;
		readonly Input input;
		readonly PlayerCharacter pc;
		readonly Usage[] usages;
		readonly Button[] spells;
		readonly SpriteFont smallFont;

		Spell usedSpell;
		NameWindow nameWindow;
		bool castingSpell;
		readonly List<Entity> entities;
		readonly Texture2D castSpellTextBack;
		readonly string castWorldSpellText;

		public override void Update(GameTime gameTime)
		{
			if (!castingSpell)
			{
				base.Update(gameTime);
				if (nameWindow != null)
				{
					nameWindow.Update(gameTime);
					if (nameWindow.CheckForExit())
					{
						usedSpell = null;
						nameWindow = null;
						return;
					}
					var chara = nameWindow.GetPlayerCharacter();
					if (chara != null)
					{
						for (int b = 0; b < usedSpell.BattleAction.actionEffects.Count; b++)
						{
							usedSpell.BattleAction.actionEffects[b].DoAction(null, pc.BattleStats, chara.BattleStats);
						}
						if (usedSpell.BattleAction.sounds.Length == 3)
							game.Audio.PlaySound(usedSpell.BattleAction.sounds[2], true);
						else
							game.Audio.PlaySound(usedSpell.BattleAction.sounds[1], true);
						pc.BattleStats.SP -= usedSpell.ManaCost(pc);
						usedSpell = null;
						nameWindow = null;
						return;
					}
				}
				if (usages.Contains(Usage.World) && usedSpell == null)
				{
					usedSpell = GetSpell();
					if (usedSpell != null)
					{
						if (usedSpell.Usage == Usage.World || usedSpell.Usage == Usage.BothAsynchrous)
						{
							castingSpell = true;
						}
						else if (usedSpell.Usage == Usage.BothSame)
						{
							if (usedSpell.BattleAction.targetType == BattleAction.TargetType.All)
							{
								for (int i = 0; i < game.party.ActiveCharacters.Count; i++)
								{
									for (int b = 0; b < usedSpell.BattleAction.actionEffects.Count; b++)
									{
										usedSpell.BattleAction.actionEffects[b].DoAction(null, pc.BattleStats, game.party.GetPlayerCharacter(game.party.ActiveCharacters[i]).BattleStats);
									}
								}
								if (usedSpell.BattleAction.sounds.Length == 3)
									game.Audio.PlaySound(usedSpell.BattleAction.sounds[2], true);
								else
									game.Audio.PlaySound(usedSpell.BattleAction.sounds[1], true);
								pc.BattleStats.SP -= usedSpell.ManaCost(pc);
								usedSpell = null;
							}
							else
								nameWindow = new NameWindow(game, new Point(TopLeft.X + 28, TopLeft.Y + 231));
						}
					}
				}
				if (nameWindow == null)
				{
					for (int i = 0; i < spells.Length; i++)
					{
						if (usages.Contains(pc.Spells[i].Usage))
						{
							spells[i].Update(gameTime);
						}
					}
				}
			}
			else
			{
				if (usedSpell.UseEffect is RevealHiddenObjects)
				{
					usedSpell.UseEffect = new RevealHiddenObjects(game, (Map)game.Scene, "");
					usedSpell.UseEffect.UseEffects();
					game.CloseUIWindow();
				}
				if (usedSpell.UseEffect is CustomEventEffect)
					usedSpell.UseEffect = new CustomEventEffect(game, (Map)game.Scene, ((CustomEventEffect)usedSpell.UseEffect).eventFile);
				if (usedSpell.UseEffect is FleeDungeon)
				{
					usedSpell.UseEffect.UseEffects();
					game.CloseUIWindow();
				}
				for (int i = 0; i < entities.Count; i++)
				{
					Rectangle rectangle = entities[i].Sprite.DrawnBounds;
					if (entities[i].Sprite.OriginMode == Sprite.OriginType.BottomMiddle)
						rectangle = new Rectangle(rectangle.X - (entities[i].Sprite.SpriteSize.X / 2), rectangle.Y - entities[i].Sprite.SpriteSize.Y, rectangle.Width, rectangle.Height);
					if (game.Input.HasMouseClickedOnRectangle(rectangle, true, false))
					{
						if (entities[i].HasSpellCastEvents)
						{
							if (usedSpell.UseEffect is SpellTriggeredEvent)
							{
								var ue = (SpellTriggeredEvent)usedSpell.UseEffect;
								if (entities[i].GetEvents(ue.spellCond) != null)
								{
									var map = (Map)game.Scene;
									game.CloseUIWindow();
									map.FaceEntity((Walker)map.GetEntityFromName("ENT_" + pc.Name.ToUpperInvariant()), entities[i]);
									map.EventManager.SetEvents(entities[i].GetEvents(ue.spellCond));
									map.EventManager.DoEvent();
									pc.BattleStats.SP -= usedSpell.ManaCost(pc);
									return;
								}
							}
						}
						if (entities[i] is Walker)
						{
							var walker = (Walker)entities[i];
							if (usedSpell.UseEffect is BlindEntity && walker.Behaviour is EyeRotate)
							{
								game.CloseUIWindow();
								usedSpell.UseEffect = new BlindEntity(game, (Map)game.Scene, walker, "ENT_" + pc.Name.ToUpper());
								pc.BattleStats.SP -= usedSpell.ManaCost(pc);
								usedSpell.UseEffect.UseEffects();
								return;
							}
							if (usedSpell.UseEffect is AggroEffect && walker.Behaviour is EnemyBehaviour)
							{
								game.CloseUIWindow();
								usedSpell.UseEffect = new AggroEffect(game, (Map)game.Scene, "ENT_" + pc.Name.ToUpper(), walker);
								pc.BattleStats.SP -= usedSpell.ManaCost(pc);
								usedSpell.UseEffect.UseEffects();
								return;
							}
						}
						else
						{
							if (usedSpell.UseEffect is LightTorch && entities[i].EntBehaviour is LightableTorch)
							{
								game.CloseUIWindow();
								usedSpell.UseEffect = new LightTorch(game, (Map)game.Scene, entities[i], "ENT_" + pc.Name.ToUpper());
								pc.BattleStats.SP -= usedSpell.ManaCost(pc);
								usedSpell.UseEffect.UseEffects();
								return;
							}
						}
					}
				}
				if (game.Input.IsMouseButtonReleased())
				{
					usedSpell = null;
					castingSpell = false;
				}
			}
		}
		public override void Draw(SpriteBatch spriteBatch)
		{
			if (!castingSpell)
			{
				if (nameWindow != null)
					nameWindow.Draw(spriteBatch);
				base.Draw(spriteBatch);
				for (int i = 0; i < spells.Length; i++)
				{
					spells[i].Draw(spriteBatch);
					spriteBatch.DrawString(smallFont, pc.Spells[i].DisplayName, new Vector2(spells[i].Bounds.X, spells[i].Bounds.Y - 16), Color.Black, 0.0f, Vector2.Zero, 1.0f, SpriteEffects.None, 0.14f);
					spriteBatch.DrawString(smallFont, TextMethods.WrapText(smallFont, pc.Spells[i].Description, 182), new Vector2(spells[i].Bounds.X + 68, spells[i].Bounds.Y + 16), Color.Black, 0.0f, Vector2.Zero, 1.0f, SpriteEffects.None, 0.14f);
					spriteBatch.DrawString(smallFont, "SP Cost: " + pc.Spells[i].ManaCost(pc).ToString(), new Vector2(spells[i].Bounds.X + 192, spells[i].Bounds.Y + 64), Color.Blue, 0.0f, Vector2.Zero, 1.0f, SpriteEffects.None, 0.14f);
				}
			}
			else
			{
				var drawVector = new Vector2(game.GetScreenWidth() / 2, 96);
				spriteBatch.Draw(castSpellTextBack, new Rectangle((int)drawVector.X, (int)drawVector.Y, 400, 64), null, Color.White, 0, new Vector2(200, 32), SpriteEffects.None, 0.01f);
				spriteBatch.DrawString(game.mediumFont, castWorldSpellText, drawVector, Color.White, 0, TextMethods.CenterText(game.mediumFont, castWorldSpellText), 1, SpriteEffects.None, 0.00f);
			}

		}
		public BattleAction GetSpellBattleAction()
		{
			for (int i = 0; i < spells.Length; i++)
			{
                if (spells[i].Activated())
				{
					if (pc.Spells[i].ManaCost(pc) <= pc.BattleStats.SP)
					{
						return pc.Spells[i].BattleAction;
					}
				}
				if (usages.Contains(Usage.Battle))
				{
					Keys key = Keys.None;
					switch (i)
					{
						case (0): key = Keys.D1; break;
						case (1): key = Keys.D2; break;
						case (2): key = Keys.D3; break;
						case (3): key = Keys.D4; break;
						case (4): key = Keys.D5; break;
						case (5): key = Keys.D6; break;
						case (6): key = Keys.D7; break;
						case (7): key = Keys.D8; break;
						case (8): key = Keys.D9; break;
						case (9): key = Keys.D0; break;
					}
					if (key != Keys.None)
					{
						if (Input.IsKeyReleased(key) && pc.Spells[i].ManaCost(pc) <= pc.BattleStats.SP)
						{
							return pc.Spells[i].BattleAction;
						}
					}
				}
			}
			return null;
		}
		public Spell GetSpell()
		{
			for (int i = 0; i < spells.Length; i++)
			{
				if (spells[i].Activated())
				{
					if (pc.Spells[i].ManaCost(pc) <= pc.BattleStats.SP)
					{
						return pc.Spells[i];
					}
				}
			}
			return null;
		}
		public int GetSpellID()
		{
			for (int i = 0; i < spells.Length; i++)
			{
                if (spells[i].Activated())
                {
					return i;
				}
			}
			return -1;
		}
		public int ReturnLowestManaCost()
		{
			int lowestMana = 99999;
			for (int i = 0; i < pc.Spells.Count; i++)
			{
				if (pc.Spells[i].ManaCost(pc) < lowestMana)
					lowestMana = pc.Spells[i].ManaCost(pc);
			}
			return lowestMana;
		}
		public Spellbook(Game g, Texture2D tex, PlayerCharacter p, Usage[] u)
			: base(g.Input, tex, 0.15f, new Point((g.GetScreenWidth() / 2) - 275, (g.GetScreenHeight() / 2) - 220),
                   new Button(g.Audio, g.Input, g.TextureLoader.RequestTexture("UI\\Battle\\BlankSpellbookExit"), new Point(((g.GetScreenWidth() / 2) - 275) + 513, ((g.GetScreenHeight() / 2) - 220) + 408), "", g.TextureLoader.RequestTexture("UI\\Highlights\\SpellbookExitHighlight"), 0.06f))
		{
			game = g;
			this.input = g.Input;
			this.smallFont = g.smallFont;
			pc = p;
			usages = u;
			int x = TopLeft.X + 16;
			int y = TopLeft.Y + 24;
			spells = new Button[pc.Spells.Count];
			for (int i = 0; i < pc.Spells.Count; i++)
			{
                spells[i] = new Button(g.Audio, g.Input, pc.Spells[i].SpellIcon, new Point(x, y), "", g.TextureLoader.RequestTexture("UI\\Highlights\\SquareHighlight"), 0.14f);
				y += 88;
				if (i >= 3)
				{
					x = TopLeft.X + 16 + 268;
					y = TopLeft.Y + 24 + (88 * ((i - 1) - 2));
				}
			}
			castSpellTextBack = game.TextureLoader.RequestTexture("UI\\World\\WorldSpellTextBack");
			castWorldSpellText = game.LoadString("UI", "StatScreenSpellCast");
			if (game.Scene is Map)
			{
				entities = new List<Entity>();
				var Map = (Map)game.Scene;
				for (int i = 0; i < Map.Entities.Count; i++)
				{
					if (Map.Entities[i].Bounds != null)
					{
						if (Map.Entities[i].EntBehaviour is DispellableEntity || Map.Entities[i].EntBehaviour is LightableTorch || Map.Entities[i].EntBehaviour is HiddenEntity || Map.Entities[i].EntBehaviour is SpellCastEntity || Map.Entities[i].HasSpellCastEvents)
						{
							entities.Add(Map.Entities[i]);
							continue;
						}
						if (Map.Entities[i] is Walker)
						{
							var walker = (Walker)Map.Entities[i];
							if (walker.Behaviour is EnemyBehaviour || walker.Behaviour is EyeRotate || walker.Behaviour is Patrol)
							{
								entities.Add(Map.Entities[i]);
								continue;
							}
						}
					}
				}
			}
		}
	}
}