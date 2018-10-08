using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;
using Microsoft.Xna.Framework.Input;

namespace Magicians
{
	class Shop : BaseUIWindow
	{
		public enum ActivePane
		{
			BuyPane, //display a grid containing the items the shop has on sale
			SellPane //display a grid containing the player character's inventory
		}
		ActivePane activePane = ActivePane.BuyPane;
		readonly Game game;
		float sellValue; //the percentage at which items are resold.
		float buyValue = 1;
		float itemValueDisplay;
		Item[] purchaseableItems;
		Party playerParty;
		PlayerCharacter activeCharacter; //the active character to buy/sell items for
		int activeCharacterIndex;
		readonly string[] strings = new string[3];

		Texture2D highlight;
		Texture2D activeCharacterHighlight;
		Texture2D charH;

		List<Item> activeItems;
		Button[] CharacterButtons;

		public string Name { get; private set; }//internal name of the shop
		public override void Update(GameTime gameTime)
		{
			base.Update(gameTime);
			var drawVector = new Point(TopLeft.X, TopLeft.Y);
			var buyPaneButton = new Rectangle(TopLeft.X + 16, TopLeft.Y + 104, 142, 38);
			if (game.Input.HasMouseClickedOnRectangle(buyPaneButton))
			{
				activePane = ActivePane.BuyPane;
				activeItems = purchaseableItems.ToList<Item>();
				itemValueDisplay = buyValue;
				for (int i = 0; i < activeItems.Count; i++)
				{
					if (activeItems[i] == null)
					{
						activeItems.RemoveAt(i);
						i--;
					}
				}
			}
			var sellPaneButton = new Rectangle(TopLeft.X + 164, TopLeft.Y + 104, 142, 38);
			if (game.Input.HasMouseClickedOnRectangle(sellPaneButton))
			{
				activePane = ActivePane.SellPane;
				activeItems = activeCharacter.Inventory;
				itemValueDisplay = sellValue;
				if (game.gameFlags.ContainsKey("bShopBonus"))
					if (game.gameFlags["bShopBonus"])
						itemValueDisplay += 0.2f;
			}
			drawVector = new Point(TopLeft.X + 32, TopLeft.Y + 29);
			for (int i = 0; i < CharacterButtons.Length; i++)
			{
				var chara = playerParty.GetPlayerCharacter(playerParty.ActiveCharacters[i]);
				if (game.Input.HasMouseClickedOnRectangle(CharacterButtons[i].Bounds))
				{
					activeCharacterIndex = i;
					activeCharacter = chara;
					if (activePane == ActivePane.SellPane)
					{
						activeItems = chara.Inventory;
					}
				}
				drawVector.X += 65;
			}
			drawVector = new Point(TopLeft.X + 30, TopLeft.Y + 166);
			var itemRecs = new Rectangle[activeItems.Count];
			for (int i = 0; i < activeItems.Count; i++)
			{
				itemRecs[i] = new Rectangle(drawVector.X, drawVector.Y, 80, 80);
				if (game.Input.HasMouseClickedOnRectangle(itemRecs[i]))
				{
					switch (activePane)
					{
						case (ActivePane.BuyPane): { BuyItem(activeItems[i]); break; }
						case (ActivePane.SellPane): { SellItem(i); break; }
					}
					break;
				}
				drawVector.X += 88;
				if (i == 3)
				{
					drawVector.X = TopLeft.X + 30;
					drawVector.Y = TopLeft.Y + 268;
				}
				if (i == 7)
				{
					drawVector.X = TopLeft.X + 30;
					drawVector.Y = TopLeft.Y + 370;
				}
				if (i == 11)
				{
					drawVector.X = TopLeft.X + 30;
					drawVector.Y = TopLeft.Y + 472;
				}
			}
		}
		public override void Draw(SpriteBatch spriteBatch)
		{
			base.Draw(spriteBatch);
			var drawVector = new Vector2(TopLeft.X, TopLeft.Y);
			for (int i = 0; i < CharacterButtons.Length; i++)
			{
				CharacterButtons[i].Draw(spriteBatch);
			}
			spriteBatch.Draw(activeCharacterHighlight, new Rectangle((int)drawVector.X + 32 + (65 * activeCharacterIndex), (int)drawVector.Y + 29, 62, 62), null, Color.NavajoWhite, 0, Vector2.Zero, SpriteEffects.None, 0.01f);
			if (activePane == ActivePane.BuyPane)
			{
				drawVector = new Vector2(TopLeft.X + 85, TopLeft.Y + 122);
				spriteBatch.DrawString(game.mediumFont, strings[0], drawVector, Color.RosyBrown, 0.0f, TextMethods.CenterText(game.mediumFont, "BUY"), 1.0f, SpriteEffects.None, 0.1f);
				drawVector = new Vector2(TopLeft.X + 234, TopLeft.Y + 122);
				spriteBatch.DrawString(game.mediumFont, strings[1], drawVector, Color.Black, 0.0f, TextMethods.CenterText(game.mediumFont, "SELL"), 1.0f, SpriteEffects.None, 0.1f);
			}
			else
			{
				drawVector = new Vector2(TopLeft.X + 85, TopLeft.Y + 122);
				spriteBatch.DrawString(game.mediumFont, strings[0], drawVector, Color.Black, 0.0f, TextMethods.CenterText(game.mediumFont, "BUY"), 1.0f, SpriteEffects.None, 0.1f);
				drawVector = new Vector2(TopLeft.X + 234, TopLeft.Y + 122);
				spriteBatch.DrawString(game.mediumFont, strings[1], drawVector, Color.RosyBrown, 0.0f, TextMethods.CenterText(game.mediumFont, "SELL"), 1.0f, SpriteEffects.None, 0.1f);
			}
			drawVector = new Vector2(TopLeft.X + 30, TopLeft.Y + 166);
			for (int i = 0; i < activeItems.Count; i++)
			{
				var drawRectangle = new Rectangle((int)drawVector.X, (int)drawVector.Y, 64, 64);
				if (activeItems[i].Value * buyValue > playerParty.Gold && activePane == ActivePane.BuyPane)
				{
					spriteBatch.Draw(activeItems[i].Icon, new Rectangle((int)drawVector.X, (int)drawVector.Y, 64, 64), null, Color.Gray, 0.0f, Vector2.Zero, SpriteEffects.None, 0.1f);
				}
				else
				{
					spriteBatch.Draw(activeItems[i].Icon, new Rectangle((int)drawVector.X, (int)drawVector.Y, 64, 64), null, Color.White, 0.0f, Vector2.Zero, SpriteEffects.None, 0.1f);
				}
				if (game.Input.IsMouseOver(drawRectangle))
				{
					spriteBatch.Draw(highlight, new Rectangle((int)drawVector.X, (int)drawVector.Y, 64, 64), Color.White);
					spriteBatch.DrawString(game.mediumFont, activeItems[i].DisplayName, new Vector2(TopLeft.X + 541, TopLeft.Y + 169), Color.Black, 0.0f, TextMethods.CenterText(game.mediumFont, activeItems[i].DisplayName), 1.0f, SpriteEffects.None, 0.1f);
					spriteBatch.Draw(activeItems[i].Icon, new Rectangle(TopLeft.X + 541, TopLeft.Y + 212, 64, 64), null, Color.White, 0.0f, new Vector2(activeItems[i].Icon.Width / 2, activeItems[i].Icon.Height / 2), SpriteEffects.None, 0.1f);
					spriteBatch.DrawString(game.smallFont, activeItems[i].Description, new Vector2(TopLeft.X + 541, TopLeft.Y + 310), Color.Black, 0.0f, TextMethods.CenterText(game.smallFont, activeItems[i].Description), 1.0f, SpriteEffects.None, 0.1f);
					if (activeItems[i] is EquippableItem)
					{
						var equip = (EquippableItem)activeItems[i];
						for (int e = 0; e < equip.equipEffects.Count; e++)
						{
							spriteBatch.DrawString(game.smallFont, equip.equipEffects[e].Description(), new Vector2(TopLeft.X + 541, TopLeft.Y + 390 + (12 * e)), Color.Black, 0.0f, TextMethods.CenterText(game.smallFont, equip.equipEffects[e].Description()), 1.0f, SpriteEffects.None, 0.17f);
						}
					}
					if (activeItems[i] is ConsumableItem)
					{
						var consum = (ConsumableItem)activeItems[i];
						if (consum.BattleAction != null)
						{
							for (int e = 0; e < consum.BattleAction.actionEffects.Count; e++)
							{
								spriteBatch.DrawString(game.smallFont, consum.BattleAction.ReturnInventoryDescription(e), new Vector2(TopLeft.X + 541, TopLeft.Y + 390 + (12 * e)), Color.Black, 0.0f, TextMethods.CenterText(game.smallFont, consum.BattleAction.ReturnInventoryDescription(e)), 1.0f, SpriteEffects.None, 0.17f);
							}
						}
					}
				}

				drawVector.X += 32;
				drawVector.Y += 73;
				spriteBatch.DrawString(game.smallFont, Math.Round((activeItems[i].Value * itemValueDisplay)).ToString(), drawVector, Color.Black, 0.0f, TextMethods.CenterText(game.smallFont, Math.Round((activeItems[i].Value * itemValueDisplay)).ToString()), 1.0f, SpriteEffects.None, 0.1f);
				drawVector.X -= 32;
				drawVector.Y -= 73;
				drawVector.X += 88;
				if (i == 3)
				{
					drawVector.X = TopLeft.X + 30;
					drawVector.Y = TopLeft.Y + 268;
				}
				if (i == 7)
				{
					drawVector.X = TopLeft.X + 30;
					drawVector.Y = TopLeft.Y + 370;
				}
				if (i == 11)
				{
					drawVector.X = TopLeft.X + 30;
					drawVector.Y = TopLeft.Y + 472;
				}
			}
			spriteBatch.DrawString(game.mediumFont, playerParty.Gold.ToString(), new Vector2(TopLeft.X + 589, TopLeft.Y + 566), Color.Black, 0.0f, TextMethods.CenterText(game.mediumFont, playerParty.Gold.ToString()), 1.0f, SpriteEffects.None, 0.1f);
		}
		public void BuyItem(Item item)
		{
			if (playerParty.Gold >= (int)(item.Value * buyValue) && activeCharacter.Inventory.Count < activeCharacter.Inventory.Capacity)
			{
				activeCharacter.Inventory.Add(item);
				playerParty.Gold -= (int)Math.Round(item.Value * buyValue);
				game.Audio.PlaySound("ButtonClick", true);
			}
			return;
		}
		public void SellItem(int itemno)
		{
			float bonus = 0;
			if (!game.gameFlags.ContainsKey("bShopBonus"))
				game.gameFlags.Add("bShopBonus", false);
			if (game.gameFlags["bShopBonus"])
				bonus = 0.2f;
			if (activeCharacter.Inventory[itemno].Value == -1)
			{
				return;
			}
			playerParty.Gold += (int)Math.Round(activeCharacter.Inventory[itemno].Value * (sellValue + bonus));
			activeCharacter.RemoveItemFromInventory(itemno);
			game.Audio.PlaySound("ButtonClick", true);
		}
		public Shop(Game g, string s, Item[] items, float b, float se)
			: base(g.Input, g.TextureLoader.RequestTexture("UI\\Shop\\ShopWindow"), 0.2f, new Point((g.graphics.PreferredBackBufferWidth / 2) - 350, (g.graphics.PreferredBackBufferHeight / 2) - 300),
				   new Button(g, g.Input, g.TextureLoader.RequestTexture("UI\\Common\\BlankExitButton"), new Point((g.graphics.PreferredBackBufferWidth / 2) - 350 + 632, (g.graphics.PreferredBackBufferHeight / 2) - 300 + 542), "", g.TextureLoader.RequestTexture("UI\\Highlights\\ExitHighlight"), 0.19f))
		{
			game = g;
			Name = s;
			purchaseableItems = items;
			sellValue = se;
			activeItems = purchaseableItems.ToList<Item>();
			for (int i = 0; i < activeItems.Count; i++)
			{
				if (activeItems[i] == null)
				{
					activeItems.RemoveAt(i);
					i--;
				}
			}
			buyValue = b;
			itemValueDisplay = b;
		}

		public void Load(Game g)
		{
			highlight = g.TextureLoader.RequestTexture("UI\\Shop\\ShopHighlight");
			playerParty = g.party; //
			activeCharacter = playerParty.GetPlayerCharacter(0);
			for (int i = 0; i < g.party.ActiveCharacters.Count; i++)
			{
				var chara = playerParty.GetPlayerCharacter(playerParty.ActiveCharacters[i]);
				if (chara.uiPortrait == null)
				{
					chara.uiPortrait = g.TextureLoader.RequestTexture("UI\\SmallPortraits\\" + chara.GraphicsFolderName);
				}
			}
			strings[0] = g.LoadString("UI", "ShopBuy");
			strings[1] = g.LoadString("UI", "ShopSell");
			charH = g.TextureLoader.RequestTexture("UI\\Highlights\\SquareHighlight");
			CharacterButtons = new Button[g.party.ActiveCharacters.Count];
			var drawVector = new Point(TopLeft.X, TopLeft.Y);
			drawVector.X += 32;
			drawVector.Y += 29;
			for (int i = 0; i < CharacterButtons.Length; i++)
			{
				var chara = playerParty.GetPlayerCharacter(playerParty.ActiveCharacters[i]);
				CharacterButtons[i] = new Button(g, g.Input, chara.uiPortrait, new Point(drawVector.X, drawVector.Y), 62, 62, "", g.TextureLoader.RequestTexture("UI\\Highlights\\SquareHighlight"), 0.19f);
				drawVector.X += 65;
			}
			activeCharacterHighlight = g.TextureLoader.RequestTexture("UI\\Shop\\ShopCharacterHighlight");
		}
	}
}