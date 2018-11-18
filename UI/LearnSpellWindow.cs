using System;
using System.Collections.Generic;
using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;

namespace Magicians
{
	class LearnSpellWindow : BaseUIWindow
	{
		readonly Game game;
		List<Spell> learnableSpells = new List<Spell>();
		QuestStats questStats;
		int newSpellIndex;
		Spellbook spellBook;
		bool confirmOverwriteLearnedSpell;
		PlayerCharacter pc;
		string[] strings = new string[8];
		Texture2D confirmWindow;
		Spell learnedSpell;
		Texture2D progressBar;
		Map map;

		public LearnSpellWindow(Game game)
			: base(game.Input, game.TextureLoader.RequestTexture("UI\\World\\LearnSpellWindow"), 0.19f, new Point(game.GetScreenWidth() / 2 - 300, game.GetScreenHeight() / 2 - 235),
			new Button(game.Audio, game.Input, game.TextureLoader.RequestTexture("UI\\Common\\BlankExitButton"), new Point(game.GetScreenWidth() / 2 - 300 + 531, (game.GetScreenHeight() / 2 - 235 + 412)), "", game.TextureLoader.RequestTexture("UI\\Highlights\\ExitHighlight"), 0.18f))
		{
			this.game = game;
			map = (Map)game.Scene;
			pc = game.party.GetPlayerCharacter(0);
			confirmWindow = game.TextureLoader.RequestTexture("UI\\Common\\ConfirmWindow");
			progressBar = game.debugSquare;

			questStats = game.party.QuestStats;
			learnableSpells.Capacity = 25;
			for (int i = 0; i < pc.Inventory.Count; i++)
			{
				if (pc.Inventory[i] is BookItem)
				{
					var book = (BookItem)pc.Inventory[i];
					for (int s = 0; s < book.Spells.Length; s++)
					{
						if (!pc.Spells.Contains(book.Spells[s]) && !learnableSpells.Contains(book.Spells[s]))
							learnableSpells.Add(book.Spells[s]);
					}
				}
			}
			learnedSpell = game.Spells.Find(l => l.InternalName == questStats.learntSpellString);
			if (!learnableSpells.Contains(learnedSpell))
			{
				questStats.learnSpellProgress = 1;
				questStats.learntSpellString = "";
			}
			strings[0] = game.LoadString("UI", "LearnSpellRequiredLevel");
			strings[1] = game.LoadString("UI", "LearnSpellOverwriteSpell");
			strings[2] = game.LoadString("UI", "LearnSpellOverwriteExistingSpell");
		}

		public override void Update(GameTime gameTime)
		{
			if (spellBook != null)
			{
				spellBook.Update(gameTime);
				var spell = spellBook.GetSpellID();
				if (spell != -1)
				{
					pc.Spells[spellBook.GetSpellID()] = learnedSpell;
					questStats.learntSpellString = "";
					questStats.learnSpellProgress = 1;
					var events = new List<IEvent>();
					events.Add(new BeginEvent(game, map));
					events.Add(new PlayDialogue(game, (Map)game.Scene, game.LoadString("Common", "LearnSpell3") + learnedSpell.DisplayName + "!"));
					events.Add(new EndEvent(game, map.EventManager, map));
					map.EventManager.SetEvents(events, true);
					game.CloseUIWindow();
					spellBook = null;
					return;
				}
				if (spellBook.CheckForExit())
				{
					questStats.learntSpellString = "";
					questStats.learnSpellProgress = 1;
					spellBook = null;
				}
				return;
			}
			base.Update(gameTime);
			var drawVector = new Vector2(TopLeft.X + 40, TopLeft.Y + 40);
			for (int i = 0; i < learnableSpells.Count; i++)
			{
				if (i == 5 || i == 10 || i == 15 || i == 20)
				{
					drawVector.X = TopLeft.X + 40 + ((i / 5) * 64);
					drawVector.Y = TopLeft.Y + 40;
				}
				if (game.Input.HasMouseClickedOnRectangle(new Rectangle((int)drawVector.X, (int)drawVector.Y, 64, 63)))
				{
					if (learnedSpell == null && learnableSpells[i].ReturnRequiredLevel(pc.Arcana) <= pc.Level)
					{
						questStats.learntSpellString = learnableSpells[i].InternalName;
						questStats.learnSpellProgress = 1;
						newSpellIndex = i;
						learnedSpell = game.Spells.Find(l => l.InternalName == questStats.learntSpellString);
					}
					else
					{
						if (learnableSpells[i].ReturnRequiredLevel(pc.Arcana) <= pc.Level && learnedSpell != learnableSpells[i])
						{
							newSpellIndex = i;
							confirmOverwriteLearnedSpell = true;
						}
					}
				}
				drawVector.Y += 63;
			}
			if (learnedSpell != null)
			{
				drawVector = new Vector2(TopLeft.X + 103, TopLeft.Y + 390);
				if (game.Input.HasMouseClickedOnRectangle(new Rectangle((int)drawVector.X, (int)drawVector.Y, 64, 64)) && game.gameFlags["bUsedLearnSpell"] == false)
				{
					LearnSpell();
				}
			}
			if (confirmOverwriteLearnedSpell)
			{
				var drawOffset = new Vector2(game.GetScreenWidth() / 2 - 192, game.GetScreenHeight() / 2 - 96);
				var confirm = new Rectangle((int)drawOffset.X + 34, (int)drawOffset.Y + 114, 152, 64);
				var cancel = new Rectangle((int)drawOffset.X + 222, (int)drawOffset.Y + 114, 152, 64);
				if (game.Input.HasMouseClickedOnRectangle(confirm))
				{
					if (newSpellIndex == -1)
					{
						confirmOverwriteLearnedSpell = false;
						return;
					}
					confirmOverwriteLearnedSpell = false;
					questStats.learntSpellString = learnableSpells[newSpellIndex].InternalName;
					learnedSpell = learnableSpells[newSpellIndex];
					questStats.learnSpellProgress = 1;
				}
				if (game.Input.HasMouseClickedOnRectangle(cancel))
				{
					confirmOverwriteLearnedSpell = false;
					newSpellIndex = -1;
				}
			}
			if (!confirmOverwriteLearnedSpell)
				base.Update(gameTime);
		}
		public void LearnSpell()
		{
			bool hasNewEvents = false;
			int value = 40 - learnedSpell.Level;
			value += pc.Spells.Count;
			string[] flags = new string[5];
			flags[0] = "bPlayerHasNoKnowledge";
			flags[1] = "bPlayerHasLowKnowledge";
			flags[2] = "bPlayerHasMediumKnowledge";
			flags[3] = "bPlayerHasHighKnowledge";
			flags[4] = "bPlayerHasMaxKnowledge";
			for (int i = 0; i < flags.Length; i++)
			{
				if (!game.gameFlags.ContainsKey(flags[i]))
				{
					game.gameFlags.Add(flags[i], false);
				}
			}
			if (game.gameFlags.ContainsKey("bDemoPrologue"))
			{
				if (game.gameFlags["bDemoPrologue"])
					value += 100;
			}
			if (game.gameFlags["bPlayerHasLowKnowledge"])
				value += 5;
			if (game.gameFlags["bPlayerHasMediumKnowledge"])
				value += 10;
			if (game.gameFlags["bPlayerHasHighKnowledge"])
				value += 15;
			if (game.gameFlags["bPlayerHasMaxKnowledge"])
				value += 20;
			if (pc.Arcana == learnedSpell.Arcana)
			{
				var arc = (int)learnedSpell.Arcana;
				if (arc == (int)pc.Arcana - 3 || arc == (int)pc.Arcana + 3)
					value = (int)(((float)value / 100) * 165f);
			}
			else
			{
				value = (int)(((float)value / 100) * 75f);
			}

			if (game.gameFlags["bDay"])
			{
				value = value * 2;
			}
			questStats.learnSpellProgress += value - (MathHelper.Max(learnedSpell.Level, pc.Level) - MathHelper.Min(pc.Level, learnedSpell.Level));
			if (questStats.learnSpellProgress >= 100)
			{
				if (pc.Spells.Count == 8)
				{
					var usages = new Usage[3];
					usages[0] = Usage.BothSame;
					usages[1] = Usage.World;
					usages[2] = Usage.BothAsynchrous;
					spellBook = new Spellbook(game, game.TextureLoader.RequestTexture("UI\\Battle\\BattleSpellbook"), pc, usages);
				}
				else
				{
					var events = new List<IEvent>();
					events.Add(new BeginEvent(game, map));
					events.Add(new PlayDialogue(game, (Map)game.Scene, game.LoadString("Common", "LearnSpell3") + learnedSpell.DisplayName + "!"));
					events.Add(new EndEvent(game, map.EventManager, map));
					map.EventManager.SetEvents(events, true);
					pc.Spells.Add(learnedSpell);
					learnedSpell = null;
					questStats.learnSpellProgress = 1;
					questStats.learntSpellString = "";
					hasNewEvents = true;
				}
			}
			else
			{
				var events = new List<IEvent>();
				events.Add(new BeginEvent(game, map));
				events.Add(new PlayDialogue(game, map, game.LoadString("Common", "LearnSpell1") + learnedSpell.DisplayName + game.LoadString("Common", "LearnSpell2")));
				events.Add(new EndEvent(game, map.EventManager, map));
				map.EventManager.SetEvents(events, true);
				hasNewEvents = true;
			}
			game.gameFlags["bDay"] = false;
			game.gameFlags["bNight"] = true;
			game.gameFlags["bUsedLearnSpell"] = true;
			if (hasNewEvents)
			{
				game.CloseUIWindow();
			}
		}
		public override void Draw(SpriteBatch spriteBatch)
		{
			base.Draw(spriteBatch);
			var drawVector = new Vector2(TopLeft.X + 40, TopLeft.Y + 40);
			for (int i = 0; i < learnableSpells.Count; i++)
			{
				if (i == 5 || i == 10 || i == 15 || i == 20)
				{
					drawVector.X = TopLeft.X + 40 + ((i / 5) * 63);
					drawVector.Y = TopLeft.Y + 40;
				}
				spriteBatch.Draw(learnableSpells[i].SpellIcon, new Rectangle((int)drawVector.X, (int)drawVector.Y, 64, 64), null, Color.White, 0, Vector2.Zero, SpriteEffects.None, 0.18f);
				if (spellBook == null)
				{
					if (game.Input.IsMouseOver(new Rectangle((int)drawVector.X, (int)drawVector.Y, 64, 63)))
					{
						var vec = new Vector2(TopLeft.X + 483, TopLeft.Y + 40);
						spriteBatch.DrawString(game.mediumFont, learnableSpells[i].DisplayName, vec, Color.Black, 0, TextMethods.CenterText(game.mediumFont, learnableSpells[i].DisplayName), 1, SpriteEffects.None, 0.18f);
						vec = new Vector2(TopLeft.X + 483, TopLeft.Y + 90);
						spriteBatch.Draw(learnableSpells[i].SpellIcon, new Rectangle((int)vec.X, (int)vec.Y, 64, 64), null, Color.White, 0, new Vector2(32, 32), SpriteEffects.None, 0.18f);
						vec = new Vector2(TopLeft.X + 380, TopLeft.Y + 128);
						spriteBatch.DrawString(game.smallFont, TextMethods.WrapText(game.smallFont, learnableSpells[i].Description, 200), vec, Color.Black, 0, Vector2.Zero, 1, SpriteEffects.None, 0.18f);
						vec = new Vector2(TopLeft.X + 380, TopLeft.Y + 380);
						if (pc.Level >= learnableSpells[i].ReturnRequiredLevel(pc.Arcana))
						{
							spriteBatch.DrawString(game.smallFont, TextMethods.WrapText(game.smallFont, strings[0] + MathHelper.Clamp(learnableSpells[i].ReturnRequiredLevel(pc.Arcana), 1, 50).ToString(), 200), vec, Color.Black, 0.0f, Vector2.Zero, 1, SpriteEffects.None, 0.18f);
						}
						else
						{
							spriteBatch.DrawString(game.smallFont, TextMethods.WrapText(game.smallFont, strings[0] + MathHelper.Clamp(learnableSpells[i].ReturnRequiredLevel(pc.Arcana), 1, 50).ToString(), 200), vec, Color.Red, 0.0f, Vector2.Zero, 1, SpriteEffects.None, 0.18f);
						}
					}
				}
				drawVector.Y += 63;
			}
			if (confirmOverwriteLearnedSpell)
			{
				spriteBatch.Draw(confirmWindow, new Rectangle(game.GetScreenWidth() / 2, game.GetScreenHeight() / 2, 384, 192), null, Color.White, 0.0f, new Vector2(192, 69), SpriteEffects.None, 0.15f);
				spriteBatch.DrawString(game.mediumFont, TextMethods.WrapText(game.mediumFont, strings[1], 350), new Vector2(game.GetScreenWidth() / 2, game.GetScreenHeight() / 2), Color.White, 0.0f, TextMethods.CenterText(game.mediumFont, TextMethods.WrapText(game.mediumFont, strings[1], 350)), 1.0f, SpriteEffects.None, 0.13f);
			}
			if (learnedSpell != null)
			{
				drawVector = new Vector2(TopLeft.X + 103, TopLeft.Y + 390);
				spriteBatch.Draw(learnedSpell.SpellIcon, new Rectangle((int)drawVector.X, (int)drawVector.Y, 56, 56), new Rectangle(4, 4, 56, 56), Color.White, 0.0f, Vector2.Zero, SpriteEffects.None, 0.18f);
				drawVector = new Vector2(TopLeft.X + 163, TopLeft.Y + 390);
				spriteBatch.Draw(progressBar, new Rectangle((int)drawVector.X, (int)drawVector.Y, 9, (questStats.learnSpellProgress * 56) / 100), null, Color.White, 0.0f, Vector2.Zero, SpriteEffects.None, 0.18f);
			}
			if (spellBook != null)
			{
				spellBook.Draw(spriteBatch);
			}
		}
		public override bool CheckForExit()
		{
			var exit = new Rectangle(TopLeft.X + 532, TopLeft.Y + 413, 61, 50);
			if (game.Input.HasMouseClickedOnRectangle(exit))
			{
				return true;
			}
			return false;
		}
	}
}
