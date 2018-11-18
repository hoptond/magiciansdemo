using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;
using Microsoft.Xna.Framework.Input;

namespace Magicians
{
	class Inventory : BaseUIWindow
	{
		readonly Game game;
		public bool dragAndDropActive; //rearranging inventory
		int draggedItemIndex = -1;
		int usedItemIndex = -1;
		Item usedItem;
		readonly Texture2D equipIcon;
		readonly Texture2D highlight;
		readonly Texture2D greyedOut;
		readonly Texture2D confirmWindow;

		readonly Texture2D nameWindowBack;
		NameWindow NameWindow;
		readonly Party party;
		readonly PlayerCharacter pc; //active character
		readonly bool inBattle;
		public bool drawHighlight = true;
		const float dragActivateTime = 0.15f; //how long the mouse must be held 
		Vector2 preMousePosition;
		int droppedItemIndex = -1;
		bool confirmDropItem;
		int givenItemIndex = -1;
		bool displayNameWindow;
		readonly string[] strings = new string[1];
		readonly Button[] ConfirmButtons;

		public override void Update(GameTime gameTime)
		{
			base.Update(gameTime);
			var baseOffset = new Vector2(64, 96);
			var recs = new Rectangle[pc.Inventory.Count];
			var drawVector = new Vector2(TopLeft.X + 23, TopLeft.Y + 153);
			if (displayNameWindow)
			{
				NameWindow.Update(gameTime);
				drawHighlight = false;
				bool removeItem = false;
				if (NameWindow != null)
				{
					var chara = NameWindow.GetPlayerCharacter();
					if (givenItemIndex != -1)
					{
						if (chara != null)
						{
							chara.Inventory.Add(pc.Inventory[givenItemIndex]);
							pc.RemoveItemFromInventory(givenItemIndex);
							givenItemIndex = -1;
							displayNameWindow = false;
							drawHighlight = true;
							NameWindow = null;
							return;
						}
					}
					if (usedItemIndex != -1)
					{
						var item = (ConsumableItem)usedItem;
						if (item.Usage == Usage.BothSame)
						{
							if (chara != null)
							{
								for (int b = 0; b < item.BattleAction.actionEffects.Count; b++)
								{
									item.BattleAction.actionEffects[b].DoAction(null, chara.BattleStats, chara.BattleStats);
								}
								if (item.BattleAction.sounds.Length == 3)
									game.Audio.PlaySound(item.BattleAction.sounds[2], true);
								else
									game.Audio.PlaySound(item.BattleAction.sounds[1], true);
								removeItem = true;
							}

						}
						else if (item.Usage == Usage.BothAsynchrous || item.Usage == Usage.World)
						{
							if (item.UseEffect is FleeDungeon)
							{
								item.UseEffect = new FleeDungeon(game);
								game.CloseUIWindow();
								item.UseEffect.UseEffects();
								removeItem = true;
							}
							if (item.UseEffect is CustomEventEffect)
							{
								item.UseEffect = new CustomEventEffect(game, (Map)game.Scene, ((CustomEventEffect)item.UseEffect).eventFile);
								game.CloseUIWindow();
								item.UseEffect.UseEffects();
								removeItem = true;
							}
						}
						if (removeItem)
						{
							if (!item.Reusable)
								pc.RemoveItemFromInventory(usedItemIndex);
							usedItemIndex = -1;
							drawHighlight = true;
							NameWindow = null;
							displayNameWindow = false;
							drawHighlight = true;
							usedItem = null;
							return;
						}
					}
				}
				if (NameWindow.CheckForExit())
				{
					usedItem = null;
					usedItemIndex = -1;
					displayNameWindow = false;
					drawHighlight = true;
					return;
				}
			}
			if (!inBattle && displayNameWindow == false && draggedItemIndex == -1)
			{
				usedItemIndex = GetItemNumber();
				if (usedItemIndex != -1)
				{
					usedItem = pc.Inventory[usedItemIndex];
					drawHighlight = false;
					if (usedItem is EquippableItem)
					{
						pc.EquipItem(usedItemIndex);
						usedItemIndex = -1;
						drawHighlight = true;
						usedItem = null;
					}
					else if (usedItem is BookItem)
					{
						usedItem = null;
						usedItemIndex = -1;
						drawHighlight = true;
					}
					else if (usedItem is ConsumableItem)
					{
						if (NameWindow == null)
							NameWindow = new NameWindow(game, new Point(TopLeft.X + 28, TopLeft.Y + 231));
						displayNameWindow = true;
					}
					//for items that do noooothing;
					else
					{
						drawHighlight = true;
						usedItem = null;
					}
				}
			}
			if (!displayNameWindow)
			{
				for (int i = 0; i < recs.Length; i++)
				{
					recs[i].X = (int)drawVector.X;
					recs[i].Y = (int)drawVector.Y;
					recs[i].Width = 67;
					recs[i].Height = 67;
					drawVector.X += 67;
					if (i == 3)
					{
						drawVector.X = TopLeft.X + 23;
						drawVector.Y = TopLeft.Y + 220;
					}
					if (i == 7)
					{
						drawVector.X = TopLeft.X + 23;
						drawVector.Y = TopLeft.Y + 287;
					}
					if (i == 11)
					{
						drawVector.X = TopLeft.X + 23;
						drawVector.Y = TopLeft.Y + 354;
					}
				}
				if (!dragAndDropActive)
				{
					for (int i = 0; i < recs.Length; i++)
					{
						if (game.Input.IsMouseOver(recs[i]) && game.Input.IsMouseButtonPressed())
						{
							var newMousePos = new Vector2(game.Input.oldMouseState.X, game.Input.oldMouseState.Y);
							if (Vector2.Distance(newMousePos, preMousePosition) > 1)
							{
								dragAndDropActive = true;
								draggedItemIndex = i;
								break;
							}
						}
					}
				}
				if (confirmDropItem)
				{
					var drawOffset = new Vector2(game.GetScreenWidth() / 2 - 192, game.GetScreenHeight() / 2 - 96);
					var confirm = new Rectangle((int)drawOffset.X + 34, (int)drawOffset.Y + 142, 150, 62);
					var cancel = new Rectangle((int)drawOffset.X + 200, (int)drawOffset.Y + 142, 150, 62);
					if (game.Input.HasMouseClickedOnRectangle(confirm))
					{
						confirmDropItem = false;
						pc.RemoveItemFromInventory(droppedItemIndex);
						droppedItemIndex = -1;
					}
					if (game.Input.HasMouseClickedOnRectangle(cancel))
					{
						confirmDropItem = false;
						droppedItemIndex = -1;
					}
					for (int i = 0; i < ConfirmButtons.Length; i++)
					{
						ConfirmButtons[i].Update(gameTime);
					}
				}
			}
			if (dragAndDropActive)
			{
				if (game.Input.IsMouseButtonReleased())
				{
					dragAndDropActive = false;
					if (!inBattle)
					{
						var dropRectangle = new Rectangle(TopLeft.X + 440, TopLeft.Y + 413, 86, 50);
						if (game.Input.IsMouseOver(dropRectangle))
						{
							if (pc.Inventory[draggedItemIndex] is BookItem || pc.Inventory[draggedItemIndex].Value == -1)
							{
								confirmDropItem = false;
								droppedItemIndex = -1;
							}
							else
							{
								droppedItemIndex = draggedItemIndex;
								draggedItemIndex = -1;
								confirmDropItem = true;
							}
						}
						var giveRectangle = new Rectangle(TopLeft.X + 348, TopLeft.Y + 413, 86, 50);
						if (game.Input.IsMouseOver(giveRectangle) && party.ActiveCharacters.Count > 1)
						{
							givenItemIndex = draggedItemIndex;
							draggedItemIndex = -1;
							displayNameWindow = true;
							NameWindow = new NameWindow(game, new Point(TopLeft.X + 28, TopLeft.Y + 231));
						}
					}
					for (int i = 0; i < recs.Length; i++)
					{
						if (game.Input.IsMouseOver(recs[i]))
						{
							if (draggedItemIndex == i)
							{
								draggedItemIndex = -1;
								break;
							}
							Item temp = pc.Inventory[i];
							pc.Inventory[i] = pc.Inventory[draggedItemIndex];
							pc.Inventory[draggedItemIndex] = temp;
							draggedItemIndex = -1;
							droppedItemIndex = -1;
							drawHighlight = true;
							break;
						}
					}
				}
			}
			preMousePosition = new Vector2(game.Input.oldMouseState.X, game.Input.oldMouseState.Y);
		}
		public override void Draw(SpriteBatch spriteBatch)
		{
			base.Draw(spriteBatch);
			var drawVector = new Vector2(TopLeft.X + 23, TopLeft.Y + 64);
			for (int i = 0; i < pc.Equips.Length; i++)
			{
				if (pc.Equips[i] != null)
				{
					spriteBatch.Draw(pc.Equips[i].Icon, new Rectangle((int)drawVector.X, (int)drawVector.Y, 64, 64), null, Color.White, 0.0f, Vector2.Zero, SpriteEffects.None, 0.17f);
				}
				drawVector.X += 67;
			}
			drawVector = new Vector2(TopLeft.X + 23, TopLeft.Y + 153);
			var firstEquips = new List<Item>();
			for (int i = 0; i < pc.Inventory.Count; i++)
			{
				var drawRectangle = new Rectangle((int)drawVector.X, (int)drawVector.Y, 64, 64);
				spriteBatch.Draw(pc.Inventory[i].Icon, new Rectangle((int)drawVector.X, (int)drawVector.Y, 64, 64), null, Color.White, 0.0f, Vector2.Zero, SpriteEffects.None, 0.17f);
				if ((pc.Inventory[i] is EquippableItem))
				{
					if (pc.Equips.Contains((EquippableItem)pc.Inventory[i]) && !firstEquips.Contains(pc.Inventory[i]))
					{
						spriteBatch.Draw(equipIcon, new Rectangle((int)drawVector.X + 4, (int)drawVector.Y + 4, 16, 16), null, Color.White, 0.0f, Vector2.Zero, SpriteEffects.None, 0.14f);
						firstEquips.Add(pc.Inventory[i]);
					}
				}
				if (game.Input.IsMouseOver(drawRectangle))
				{
					if (drawHighlight)
					{
						spriteBatch.Draw(highlight, new Rectangle((int)drawVector.X, (int)drawVector.Y, 64, 64), Color.White);
					}
					if (draggedItemIndex == -1 && droppedItemIndex == -1 && displayNameWindow == false)
					{
						spriteBatch.DrawString(game.mediumFont, TextMethods.WrapText(game.mediumFont, pc.Inventory[i].DisplayName, 400), new Vector2(TopLeft.X + 458, TopLeft.Y + 23), Color.Black, 0.0f, TextMethods.CenterText(game.mediumFont, TextMethods.WrapText(game.mediumFont, pc.Inventory[i].DisplayName, 400)), 1.0f, SpriteEffects.None, 0.17f);
						spriteBatch.Draw(pc.Inventory[i].Icon, new Rectangle(TopLeft.X + 458, TopLeft.Y + 100, 64, 64), null, Color.White, 0.0f, new Vector2(pc.Inventory[i].Icon.Width / 2, pc.Inventory[i].Icon.Height / 2), SpriteEffects.None, 0.17f);
						spriteBatch.DrawString(game.smallFont, pc.Inventory[i].Description, new Vector2(TopLeft.X + 458, TopLeft.Y + 164), Color.Black, 0.0f, TextMethods.CenterText(game.smallFont, pc.Inventory[i].Description), 1.0f, SpriteEffects.None, 0.17f);
						if (pc.Inventory[i] is EquippableItem)
						{
							var equip = (EquippableItem)pc.Inventory[i];
							for (int e = 0; e < equip.equipEffects.Count; e++)
							{
								spriteBatch.DrawString(game.smallFont, equip.equipEffects[e].Description(), new Vector2(TopLeft.X + 458, TopLeft.Y + 252 + (12 * e)), Color.Black, 0.0f, TextMethods.CenterText(game.smallFont, equip.equipEffects[e].Description()), 1.0f, SpriteEffects.None, 0.17f);
							}
						}
						if (pc.Inventory[i] is ConsumableItem)
						{
							var consum = (ConsumableItem)pc.Inventory[i];
							if (consum.BattleAction != null)
							{
								for (int e = 0; e < consum.BattleAction.actionEffects.Count; e++)
								{
									spriteBatch.DrawString(game.smallFont, consum.BattleAction.ReturnInventoryDescription(e), new Vector2(TopLeft.X + 458, TopLeft.Y + 252 + (12 * e)), Color.Black, 0.0f, TextMethods.CenterText(game.smallFont, consum.BattleAction.ReturnInventoryDescription(e)), 1.0f, SpriteEffects.None, 0.17f);
								}
							}

						}
					}
				}
				drawVector.X += 67;
				if (i == 3)
				{
					drawVector.X = TopLeft.X + 23;
					drawVector.Y = TopLeft.Y + 220;
				}
				if (i == 7)
				{
					drawVector.X = TopLeft.X + 23;
					drawVector.Y = TopLeft.Y + 287;
				}
				if (i == 11)
				{
					drawVector.X = TopLeft.X + 23;
					drawVector.Y = TopLeft.Y + 354;
				}
			}
			if (dragAndDropActive && !confirmDropItem)
			{
				spriteBatch.Draw(pc.Inventory[draggedItemIndex].Icon, new Rectangle(game.Input.oldMouseState.X, game.Input.oldMouseState.Y, 64, 64), null, Color.FromNonPremultiplied(225, 225, 225, 135), 0.0f, new Vector2(32, 32), SpriteEffects.None, 0.0f);
			}
			if (confirmDropItem)
			{
				spriteBatch.Draw(confirmWindow, new Rectangle(game.GetScreenWidth() / 2, game.GetScreenHeight() / 2, 384, 192), null, Color.White, 0.0f, new Vector2(192, 69), SpriteEffects.None, 0.02f);
				spriteBatch.DrawString(game.mediumFont, strings[0], new Vector2(game.GetScreenWidth() / 2, game.GetScreenHeight() / 2), Color.Black, 0.0f, TextMethods.CenterText(game.mediumFont, strings[0]), 1.0f, SpriteEffects.None, 0.01f);
				ConfirmButtons[0].Draw(spriteBatch);
				ConfirmButtons[1].Draw(spriteBatch);
			}
			if (displayNameWindow)
			{
				NameWindow.Draw(spriteBatch);
			}
			if (inBattle)
			{
				spriteBatch.Draw(greyedOut, new Rectangle(TopLeft.X + 348, TopLeft.Y + 413, 86, 50), null, Color.White, 0.0f, Vector2.Zero, SpriteEffects.None, 0.01f);
				spriteBatch.Draw(greyedOut, new Rectangle(TopLeft.X + 440, TopLeft.Y + 413, 86, 50), null, Color.White, 0.0f, Vector2.Zero, SpriteEffects.None, 0.01f);
			}
			if (inBattle)
			{
				if (party.ActiveCharacters.Count == 1)
				{
					spriteBatch.Draw(greyedOut, new Rectangle(TopLeft.X + 348, TopLeft.Y + 413, 86, 50), null, Color.White, 0.0f, Vector2.Zero, SpriteEffects.None, 0.01f);
				}
			}
		}
		public int GetItemNumber()
		{
			var baseOffset = new Vector2(64, 96);
			var recs = new Rectangle[pc.Inventory.Count];
			var drawVector = new Vector2(TopLeft.X + 23, TopLeft.Y + 153);
			for (int i = 0; i < recs.Length; i++)
			{
				recs[i].X = (int)drawVector.X;
				recs[i].Y = (int)drawVector.Y;
				recs[i].Width = 64;
				recs[i].Height = 64;
				drawVector.X += 67;
				if (i == 3)
				{
					drawVector.X = TopLeft.X + 23;
					drawVector.Y = TopLeft.Y + 220;
				}
				if (i == 7)
				{
					drawVector.X = TopLeft.X + 23;
					drawVector.Y = TopLeft.Y + 287;
				}
				if (i == 11)
				{
					drawVector.X = TopLeft.X + 23;
					drawVector.Y = TopLeft.Y + 354;
				}
				if (game.Input.IsMouseOver(recs[i]) && game.Input.IsMouseButtonReleased())
				{
					return i;
				}
				var key = Keys.None;
				if (i < 10)
				{
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
				}
				if (key != Keys.None)
				{
					if (i < 10)
					{
						if (game.Input.IsKeyReleased(key))
							return i;
					}
					else
					{
						if (game.Input.IsKeyReleased(key) && game.Input.IsKeyPressed(Keys.LeftShift))
							return i;
					}

				}
			}
			return -1;
		}
		public BattleAction GetBattleAction()
		{
			var baseOffset = new Vector2(64, 96);
			var recs = new Rectangle[pc.Inventory.Count];
			var drawVector = new Vector2(TopLeft.X + 23, TopLeft.Y + 64);
			for (int i = 0; i < recs.Length; i++)
			{
				recs[i].X = TopLeft.X;
				recs[i].Y = TopLeft.Y;
				recs[i].Width = 64;
				recs[i].Height = 64;
				drawVector.X += 67;
				if (i == 3)
				{
					drawVector.X = TopLeft.X + 23;
					drawVector.Y = TopLeft.Y + 220;
				}
				if (i == 7)
				{
					drawVector.X = TopLeft.X + 23;
					drawVector.Y = TopLeft.Y + 287;
				}
				if (i == 11)
				{
					drawVector.X = TopLeft.X + 23;
					drawVector.Y = TopLeft.Y + 354;
				}
				if (game.Input.IsMouseOver(recs[i]) && game.Input.IsMouseButtonReleased())
				{
					if (pc.Inventory[i] is ConsumableItem)
					{
						if (((ConsumableItem)pc.Inventory[i]).Usage == Usage.Battle || ((ConsumableItem)pc.Inventory[i]).Usage == Usage.BothSame)
							return ((ConsumableItem)pc.Inventory[i]).BattleAction;
					}
				}
			}
			return null;
		}
		public Inventory(Game game, Texture2D nameWindow, Party part, PlayerCharacter p, bool b)
			: base(game.Input, game.TextureLoader.RequestTexture("UI\\Battle\\BattleInventory"), 0.18f, new Point((game.GetScreenWidth() / 2) - 300, (game.GetScreenHeight() / 2) - 235),
                   new Button(game.Audio, game.Input, game.TextureLoader.RequestTexture("UI\\Common\\BlankExitButton"), new Point(((game.GetScreenWidth() / 2) - 300) + 531, ((game.GetScreenHeight() / 2) - 235) + 412), "", game.TextureLoader.RequestTexture("UI\\Highlights\\ExitHighlight"), 0.17f))
		{
			party = part;
			pc = p;
			inBattle = b;
			this.game = game;
			equipIcon = game.TextureLoader.RequestTexture("UI\\Common\\equip");
			highlight = game.TextureLoader.RequestTexture("UI\\Shop\\ShopHighlight");
			greyedOut = game.TextureLoader.RequestTexture("UI\\Common\\greyedOut");
			confirmWindow = game.TextureLoader.RequestTexture("UI\\Common\\ConfirmWindow");
			this.nameWindowBack = nameWindow;
			strings[0] = game.LoadString("UI", "InventoryConfirmDrop");
			var baseOffset = new Vector2(64, 96);
			ConfirmButtons = new Button[2];
			var drawOffset = new Vector2(game.GetScreenWidth() / 2 - 192, game.GetScreenHeight() / 2 - 96);
			var confirm = new Rectangle((int)drawOffset.X + 34, (int)drawOffset.Y + 142, 150, 64);
			var cancel = new Rectangle((int)drawOffset.X + 200, (int)drawOffset.Y + 142, 150, 64);
            ConfirmButtons[0] = new Button(game.Audio, game.Input, game.TextureLoader.RequestTexture("UI\\Common\\BlankConfirmButton"), new Point((int)drawOffset.X + 34, (int)drawOffset.Y + 141), "", game.TextureLoader.RequestTexture("UI\\Highlights\\ConfirmHighlight"), 0.001f);
            ConfirmButtons[1] = new Button(game.Audio, game.Input, game.TextureLoader.RequestTexture("UI\\Common\\BlankConfirmButton"), new Point((int)drawOffset.X + 200, (int)drawOffset.Y + 141), "", game.TextureLoader.RequestTexture("UI\\Highlights\\ConfirmHighlight"), 0.001f);
		}
	}
}
