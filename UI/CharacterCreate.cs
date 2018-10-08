using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using Microsoft.Xna.Framework.Graphics;
using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Input;


namespace Magicians
{

	class CharacterCreateScreen : BaseUIWindow
	{
		Game game;
		Texture2D maleIcon;
		Texture2D femaleIcon;
		Texture2D genderCircle;
		Texture2D genderHighlight;
		Texture2D nameBar;
		Texture2D ArcanaWheel;
		Texture2D arcanaHighlight;
		Texture2D tooltipWindow;
		Texture2D greyedOut;
		Texture2D[] arcanas = new Texture2D[6];
		Texture2D finishTick;

		Texture2D attributesWindow;
		Texture2D pointsRemainingBox;

		Texture2D FinishPanel;

		Button NextButton;
		Button BackButton;
		Button[] AttributeButtons;

		Arcana selectedArcana = Arcana.Null;
		Rectangle[] arcanaRecs;
		Gender gender = Gender.Other;
		enum Stage { Naming = 1, Arcana = 2, Attributes = 3, Finish = 4 }
		Point baseOffset;
		Stage stage = Stage.Naming;

		string[] strings;
		int pointsToAllocate = 12;
		int Health;
		int Mana;
		public SortedList<Attributes, int> charAttributes = new SortedList<Attributes, int>();
		string attributeDesc;

		Rectangle helpRectangle;

		//player naming stuff.
		bool nameInputActive = true;
		int cursorPosition;
		float cursorTimer;
		bool displayCursor = true;
		string playerName = "";
		Keys[] keys = new Keys[]
		{
			Keys.A, Keys.B, Keys.C, Keys.D, Keys.E,
			Keys.F, Keys.G, Keys.H, Keys.I, Keys.J,
			Keys.K, Keys.L, Keys.M, Keys.N, Keys.O,
			Keys.P, Keys.Q, Keys.R, Keys.S, Keys.T,
			Keys.U, Keys.V, Keys.W, Keys.X, Keys.Y,
			Keys.Z
		};

		bool displayTutHighlight;
		float highlightAlpha;
		Texture2D tutHighlight;
		bool tutHighlightIncr = true;

		public override void Update(GameTime gameTime)
		{
			var drawVector = new Point(game.GetScreenWidth() / 2 - 350, game.GetScreenHeight() / 2 - 295);
			if (displayTutHighlight)
			{
				if (tutHighlightIncr)
				{
					highlightAlpha += 0.5f;
					if (highlightAlpha > 255)
						tutHighlightIncr = false;
				}
				if (!tutHighlightIncr)
				{
					highlightAlpha -= 0.5f;
					if (highlightAlpha < 1)
						tutHighlightIncr = true;
				}
			}
			if (canAdvanceToNextStage())
				NextButton.Update(gameTime);
			BackButton.Update(gameTime);
			switch (stage)
			{
				case (Stage.Naming):
					{
						if (nameInputActive)
						{
							bool capslock = Console.CapsLock;
							cursorTimer += (float)gameTime.ElapsedGameTime.TotalSeconds;
							if (cursorTimer > 0.4)
							{
								displayCursor = !displayCursor;
								cursorTimer = 0;
							}
							if (playerName.Length < 10)
							{
								foreach (Keys key in keys)
								{
									if (game.Input.IsKeyReleased(key))
									{
										var s = key.ToString();
										if (!game.Input.IsKeyPressed(Keys.LeftShift) && !game.Input.IsKeyPressed(Keys.RightShift) && !capslock)
										{
											s = s.ToLower();
										}
										playerName = playerName.Insert(cursorPosition, s);
										cursorPosition++;
										game.Audio.PlaySound("Type", true);
										break;
									}
								}
								if (game.Input.IsKeyReleased(Keys.Space))
								{
									playerName = playerName.Insert(cursorPosition, " ");
									cursorPosition++;
								}
								if (game.Input.IsKeyReleased(Keys.OemTilde))
								{
									playerName = playerName.Insert(cursorPosition, "'");
									cursorPosition++;
								}
							}
							if (game.Input.IsKeyReleased(Keys.Back) && cursorPosition > 0)
							{
								playerName = playerName.Remove(cursorPosition - 1, 1);
								cursorPosition--;
							}
							if (game.Input.IsKeyReleased(Keys.Left) && cursorPosition != 0)
							{
								cursorPosition--;
							}
							if (game.Input.IsKeyReleased(Keys.Right) && cursorPosition != playerName.Length)
							{
								cursorPosition++;
							}
						}
						var mRectangle = new Rectangle(baseOffset.X + 160, baseOffset.Y + 282, 70, 70);
						var fRectangle = new Rectangle(baseOffset.X + 460, baseOffset.Y + 282, 70, 70);
						if (game.Input.HasMouseClickedOnRectangle(mRectangle))
						{
							gender = Gender.Male;
						}
						if (game.Input.HasMouseClickedOnRectangle(fRectangle))
						{
							gender = Gender.Female;
						}
						var nameBarRec = new Rectangle(drawVector.X + 100, drawVector.Y + 132, 500, 68);
						var background = new Rectangle(drawVector.X, drawVector.Y, 600, 470);
						if (game.Input.HasMouseClickedOnRectangle(background))
						{
							nameInputActive = false;
						}
						if (game.Input.HasMouseClickedOnRectangle(nameBarRec))
						{
							nameInputActive = true;
						}
						break;
					}
				case (Stage.Arcana):
					{
						for (int i = 0; i < arcanaRecs.Length; i++)
						{
							if (game.Input.HasMouseClickedOnRectangle(arcanaRecs[i]))
							{
								selectedArcana = (Arcana)i;
							}
						}
						break;
					}
				case (Stage.Attributes):
					{
						drawVector = new Point(game.GetScreenWidth() / 2 - 240, game.GetScreenHeight() / 2 - 230);
						for (int i = 0; i < AttributeButtons.Length; i++)
						{
							if (AttributeButtons[i].Activated())
							{
								switch (i)
								{
									case (0): { PlusAttribute(Attributes.Strength); break; }
									case (1): { MinusAttribute(Attributes.Strength); break; }
									case (2): { PlusAttribute(Attributes.Magic); break; }
									case (3): { MinusAttribute(Attributes.Magic); break; }
									case (4): { PlusAttribute(Attributes.Dexterity); break; }
									case (5): { MinusAttribute(Attributes.Dexterity); break; }
								}
							}
						}
						break;
					}
			}
			drawVector = new Point(game.GetScreenWidth() / 2 - 350, game.GetScreenHeight() / 2 - 295);
			var navigationButtons = new Rectangle[2];
			navigationButtons[0] = new Rectangle(drawVector.X + 565, drawVector.Y + 533, 61, 50);
			navigationButtons[1] = new Rectangle(drawVector.X + 631, drawVector.Y + 533, 61, 50);
			if (game.Input.HasMouseClickedOnRectangle(navigationButtons[0]))
			{
				this.stage--;
			}
			if (game.Input.HasMouseClickedOnRectangle(navigationButtons[1]) && canAdvanceToNextStage())
			{
				if (this.stage == Stage.Finish)
				{
					CreateMainCharacter();
					game.CloseUIWindow();
					game.worldUI.display = false;
				}
				this.stage++;
			}
			if (stage == 0)
			{
				game.BeginSceneChange(Game.GameScenes.MainMenu);
				((Map)game.Scene).EventManager.Events.Clear();
				game.Audio.SetAmbience("AmbWindOutside", 40);
				game.CloseUIWindow();
				game.worldUI.display = false;
			}
			stage = (Stage)MathHelper.Clamp((int)stage, 1, 5);
		}
		public override void Draw(SpriteBatch spriteBatch)
		{
			base.Draw(spriteBatch);
			var drawVector = new Point(game.GetScreenWidth() / 2 - 350, game.GetScreenHeight() / 2 - 295);
			if (displayTutHighlight)
				spriteBatch.Draw(tutHighlight, new Rectangle(drawVector.X + 7, drawVector.Y + 533, 63, 52), null, new Color(255, 255, 255, highlightAlpha), 0, Vector2.Zero, SpriteEffects.FlipHorizontally, 0f);
			BackButton.Draw(spriteBatch);
			if (Input.IsMouseOver(helpRectangle))
			{
				DrawMethods.DrawToolTip(spriteBatch, new Vector2(game.Input.oldMouseState.X, game.Input.oldMouseState.Y), game.smallFont, tooltipWindow, strings[16 + (int)stage]);
				displayTutHighlight = false;
			}
			if (!canAdvanceToNextStage())
			{
				spriteBatch.Draw(greyedOut, new Rectangle(drawVector.X + 632, drawVector.Y + 533, 61, 50), Color.White);
			}
			else
			{
				NextButton.Draw(spriteBatch);
			}
			switch (stage)
			{
				case (Stage.Naming):
					{
						spriteBatch.DrawString(game.mediumFont, strings[0], new Vector2(drawVector.X + 350, drawVector.Y + 32), Color.Black, 0.0f, TextMethods.CenterText(game.mediumFont, strings[0]), 1.0f, SpriteEffects.None, 0.0f);
						drawVector.X += 125;
						drawVector.Y += 132;
						spriteBatch.Draw(nameBar, new Rectangle(drawVector.X, drawVector.Y, 500, 68), null, Color.White, 0.0f, Vector2.Zero, SpriteEffects.None, 0.001f);
						drawVector.X += 6;
						drawVector.Y += 6;
						spriteBatch.DrawString(game.largeFont, playerName, drawVector.ToVector2(), Color.Black, 0.0f, new Vector2(0, 0), 1.0f, SpriteEffects.None, 0.0f);
						if (displayCursor && nameInputActive)
						{
							spriteBatch.DrawString(game.largeFont, "|", new Vector2(drawVector.X + game.largeFont.MeasureString(playerName.Substring(0, cursorPosition)).X - 12, drawVector.Y), Color.Black, 0.0f, new Vector2(0, 0), 1.0f, SpriteEffects.None, 0.0f);
						}
						spriteBatch.Draw(genderCircle, new Rectangle(baseOffset.X + 160, baseOffset.Y + 282, 70, 70), null, Color.White, 0.0f, Vector2.Zero, SpriteEffects.None, 0.001f);
						spriteBatch.Draw(maleIcon, new Rectangle(baseOffset.X + 172, baseOffset.Y + 296, 44, 44), null, Color.White, 0.0f, Vector2.Zero, SpriteEffects.None, 0f);
						spriteBatch.Draw(genderCircle, new Rectangle(baseOffset.X + 460, baseOffset.Y + 282, 70, 70), null, Color.White, 0.0f, Vector2.Zero, SpriteEffects.None, 0.001f);
						spriteBatch.Draw(femaleIcon, new Rectangle(baseOffset.X + 474, baseOffset.Y + 296, 44, 44), null, Color.White, 0.0f, Vector2.Zero, SpriteEffects.None, 0f);
						if (gender == Gender.Male)
						{
							spriteBatch.Draw(genderHighlight, new Rectangle(baseOffset.X + 160, baseOffset.Y + 282, 70, 70), null, Color.White, 0.0f, Vector2.Zero, SpriteEffects.None, 0f);
						}
						if (gender == Gender.Female)
						{
							spriteBatch.Draw(genderHighlight, new Rectangle(baseOffset.X + 460, baseOffset.Y + 282, 70, 70), null, Color.White, 0.0f, Vector2.Zero, SpriteEffects.None, 0.0f);
						}
						break;
					}
				case (Stage.Arcana):
					{
						spriteBatch.DrawString(game.mediumFont, strings[1], new Vector2(drawVector.X + 350, drawVector.Y + 32), Color.Black, 0.0f, TextMethods.CenterText(game.mediumFont, strings[1]), 1.0f, SpriteEffects.None, 0.0f);
						drawVector = new Point(game.GetScreenWidth() / 2 - 200, game.GetScreenHeight() / 2 - 180);
						spriteBatch.Draw(ArcanaWheel, new Rectangle(drawVector.X, drawVector.Y, 400, 400), null, Color.White, 0.0f, Vector2.Zero, SpriteEffects.None, 0.02f);
						for (int i = 0; i < arcanaRecs.Length; i++)
						{
							if (game.Input.IsMouseOver(arcanaRecs[i]))
							{
								DrawMethods.DrawToolTip(spriteBatch, new Vector2(game.Input.oldMouseState.X, game.Input.oldMouseState.Y), game.smallFont, tooltipWindow, strings[4 + i]);
								break;
							}
						}
						if (selectedArcana != Arcana.Null)
						{
							spriteBatch.Draw(arcanaHighlight, arcanaRecs[(int)selectedArcana], null, Color.White, 0.0f, Vector2.Zero, SpriteEffects.None, 0.01f);
						}
						break;
					}
				case (Stage.Attributes):
					{
						spriteBatch.DrawString(game.mediumFont, strings[2], new Vector2(drawVector.X + 300, drawVector.Y + 32), Color.Black, 0.0f, TextMethods.CenterText(game.mediumFont, strings[2]), 1.0f, SpriteEffects.None, 0.0f);
						drawVector = new Point(game.GetScreenWidth() / 2 - 240, game.GetScreenHeight() / 2 - 230);
						spriteBatch.Draw(attributesWindow, new Rectangle(drawVector.X, drawVector.Y, 508, 426), null, Color.White, 0.0f, Vector2.Zero, SpriteEffects.None, 0.02f);
						spriteBatch.DrawString(game.mediumFont, Health.ToString(), new Vector2(drawVector.X + 94, drawVector.Y + 339), Color.Black, 0.0f, TextMethods.CenterText(game.mediumFont, Health.ToString()), 1.0f, SpriteEffects.None, 0.01f);
						spriteBatch.DrawString(game.mediumFont, Mana.ToString(), new Vector2(drawVector.X + 198, drawVector.Y + 339), Color.Black, 0.0f, TextMethods.CenterText(game.mediumFont, Mana.ToString()), 1.0f, SpriteEffects.None, 0.01f);
						spriteBatch.DrawString(game.mediumFont, charAttributes[Attributes.Strength].ToString(), new Vector2(drawVector.X + 124, drawVector.Y + 99), Color.Black, 0.0f, TextMethods.CenterText(game.mediumFont, charAttributes[Attributes.Strength].ToString()), 1.0f, SpriteEffects.None, 0.01f);
						spriteBatch.DrawString(game.mediumFont, charAttributes[Attributes.Magic].ToString(), new Vector2(drawVector.X + 124, drawVector.Y + 181), Color.Black, 0.0f, TextMethods.CenterText(game.mediumFont, charAttributes[Attributes.Magic].ToString()), 1.0f, SpriteEffects.None, 0.01f);
						spriteBatch.DrawString(game.mediumFont, charAttributes[Attributes.Dexterity].ToString(), new Vector2(drawVector.X + 124, drawVector.Y + 262), Color.Black, 0.0f, TextMethods.CenterText(game.mediumFont, charAttributes[Attributes.Dexterity].ToString()), 1.0f, SpriteEffects.None, 0.01f);

						var recs = new Rectangle[5];
						recs[0] = new Rectangle(drawVector.X + 15, drawVector.Y + 66, 204, 70);
						recs[1] = new Rectangle(drawVector.X + 15, drawVector.Y + 147, 204, 70);
						recs[2] = new Rectangle(drawVector.X + 15, drawVector.Y + 228, 204, 70);
						recs[3] = new Rectangle(drawVector.X + 13, drawVector.Y + 305, 101, 63);
						recs[4] = new Rectangle(drawVector.X + 118, drawVector.Y + 305, 101, 63);
						for (int i = 0; i < recs.Length; i++)
						{
							if (game.Input.IsMouseOver(recs[i]))
							{
								attributeDesc = strings[10 + i];
								goto DrawRemainingPoints;
							}
						}
						if (pointsToAllocate > 0)
						{
							for (int i = 0; i < AttributeButtons.Length; i++)
							{
								AttributeButtons[i].Draw(spriteBatch);
							}
						}

						attributeDesc = strings[15];
					DrawRemainingPoints:
						spriteBatch.DrawString(game.mediumFont, TextMethods.WrapText(game.mediumFont, attributeDesc, 214), new Vector2(drawVector.X + 263, drawVector.Y + 75), Color.Black, 0.0f, Vector2.Zero, 1.0f, SpriteEffects.None, 0.01f);
						drawVector = new Point(game.GetScreenWidth() / 2 - 350, game.GetScreenHeight() / 2 - 295);
						drawVector.X += 73;
						drawVector.Y += 548;
						spriteBatch.Draw(pointsRemainingBox, new Rectangle(drawVector.X, drawVector.Y, 210, 36), null, Color.White, 0.0f, Vector2.Zero, SpriteEffects.None, 0.02f);
						drawVector.X += 4;
						drawVector.Y += 8;
						spriteBatch.DrawString(game.mediumFont, strings[16] + pointsToAllocate, drawVector.ToVector2(), Color.Black, 0.0f, Vector2.Zero, 1.0f, SpriteEffects.None, 0.01f);
						break;
					}
				case (Stage.Finish):
					{
						spriteBatch.DrawString(game.mediumFont, strings[3], new Vector2(drawVector.X + 300, drawVector.Y + 32), Color.Black, 0.0f, TextMethods.CenterText(game.mediumFont, strings[3]), 1.0f, SpriteEffects.None, 0.0f);
						drawVector = new Point(game.GetScreenWidth() / 2 - 240, game.GetScreenHeight() / 2 - 230);
						spriteBatch.Draw(FinishPanel, new Rectangle(drawVector.X, drawVector.Y, 508, 426), null, Color.White, 0.0f, Vector2.Zero, SpriteEffects.None, 0.02f);
						spriteBatch.DrawString(game.largeFont, playerName, new Vector2(drawVector.X + 18, drawVector.Y + 8), Color.Black, 0.0f, Vector2.Zero, 1, SpriteEffects.None, 0.01f);
						if (gender == Gender.Male)
						{
							spriteBatch.Draw(maleIcon, new Rectangle(drawVector.X + 54, drawVector.Y + 244, 44, 44), null, Color.White, 0.0f, Vector2.Zero, SpriteEffects.None, 0.0f);
						}
						if (gender == Gender.Female)
						{
							spriteBatch.Draw(femaleIcon, new Rectangle(drawVector.X + 54, drawVector.Y + 244, 44, 44), null, Color.White, 0.0f, Vector2.Zero, SpriteEffects.None, 0.0f);
						}
						spriteBatch.DrawString(game.mediumFont, Health + "/" + Health, new Vector2(drawVector.X + 256, drawVector.Y + 113), Color.Black, 0.0f, TextMethods.CenterText(game.mediumFont, Health + "/" + Health), 1.0f, SpriteEffects.None, 0.01f);
						spriteBatch.DrawString(game.mediumFont, Mana + "/" + Mana, new Vector2(drawVector.X + 256, drawVector.Y + 171), Color.Black, 0.0f, TextMethods.CenterText(game.mediumFont, Mana + "/" + Mana), 1.0f, SpriteEffects.None, 0.01f);
						spriteBatch.DrawString(game.mediumFont, charAttributes[Attributes.Strength].ToString(), new Vector2(drawVector.X + 256, drawVector.Y + 229), Color.Black, 0.0f, TextMethods.CenterText(game.mediumFont, charAttributes[Attributes.Strength].ToString()), 1.0f, SpriteEffects.None, 0.01f);
						spriteBatch.DrawString(game.mediumFont, charAttributes[Attributes.Magic].ToString(), new Vector2(drawVector.X + 256, drawVector.Y + 287), Color.Black, 0.0f, TextMethods.CenterText(game.mediumFont, charAttributes[Attributes.Magic].ToString()), 1.0f, SpriteEffects.None, 0.01f);
						spriteBatch.DrawString(game.mediumFont, charAttributes[Attributes.Dexterity].ToString(), new Vector2(drawVector.X + 256, drawVector.Y + 345), Color.Black, 0.0f, TextMethods.CenterText(game.mediumFont, charAttributes[Attributes.Dexterity].ToString()), 1.0f, SpriteEffects.None, 0.01f);
						spriteBatch.Draw(arcanas[(int)selectedArcana], new Rectangle(drawVector.X + 15, drawVector.Y + 94, 120, 120), null, Color.White, 0.0f, Vector2.Zero, SpriteEffects.None, 0.01f);
						drawVector = new Point(game.GetScreenWidth() / 2 - 350, game.GetScreenHeight() / 2 - 295);
						spriteBatch.Draw(finishTick, new Rectangle(drawVector.X + 632, drawVector.Y + 533, 61, 50), null, Color.White, 0, Vector2.Zero, SpriteEffects.None, 0.029f);
						break;
					}
			}
		}
		int returnStatGain(int val)
		{
			if (val < 3)
				return 1;
			if (val < 5)
				return 2;
			if (val < 9)
				return 3;
			return 4;
		}
		void CreateMainCharacter()
		{
			var stats = new int[5];
			var statGain = new int[5];
			var i = 0;
			var hs = new int[2];
			hs[0] = Health;
			hs[1] = Mana;
			foreach (KeyValuePair<Attributes, int> kvp in charAttributes)
			{
				statGain[i] = returnStatGain(kvp.Value);
				stats[i] = kvp.Value;
				i++;
			}
			var gra = "";
			switch (gender)
			{
				case (Gender.Male):
					{
						gra = "mPlayerStart";
						game.gameFlags.Add("bPlayerIsMale", true);
						break;
					}
				case (Gender.Female):
					{
						gra = "fPlayerStart";
						game.gameFlags.Add("bPlayerIsFemale", true);
						break;
					}
			}
			var pc = new PlayerCharacter(game, 0, playerName, hs, stats, statGain, gra, gender, selectedArcana);
			pc.BattleStats.isPC = true;
			pc.BattleStats.pc = pc;
			pc.Inventory.Add(game.Items.Find(itm => itm.InternalName == "item_practicewand"));
			pc.Inventory.Add(game.Items.Find(itm => itm.InternalName == "item_academyrobe"));
			pc.Inventory.Add(game.Items.Find(itm => itm.InternalName == "item_winterboots"));
			pc.EquipItem(0);
			pc.EquipItem(1);
			pc.EquipItem(2);
			switch (selectedArcana)
			{
				case (Arcana.Fire): { pc.BattleStats.baseResistances[DamageTypes.Fire] = 10; pc.BattleStats.baseResistances[DamageTypes.Cold] = -10; break; }
				case (Arcana.Water): { pc.BattleStats.baseResistances[DamageTypes.Cold] = 10; pc.BattleStats.baseResistances[DamageTypes.Fire] = -10; break; }
				case (Arcana.Earth): { pc.BattleStats.baseArmour += 2; break; }
				case (Arcana.Light): { pc.BattleStats.MaxSP += 10; pc.BattleStats.SP += 10; break; }
				case (Arcana.Wind): { pc.BattleStats.baseResistances[DamageTypes.Electricity] = 20; break; }
			}
			switch (pc.Arcana)
			{
				case (Arcana.Earth): pc.LearnSpell(game.Spells.Find(spl => spl.internalName == "spl_magicfist")); break;
				case (Arcana.Light): pc.LearnSpell(game.Spells.Find(spl => spl.internalName == "spl_magicfist")); break;
				case (Arcana.Fire): pc.LearnSpell(game.Spells.Find(spl => spl.internalName == "spl_firearrow")); break;
				case (Arcana.Shadow): pc.LearnSpell(game.Spells.Find(spl => spl.internalName == "spl_darkbolt")); break;
				case (Arcana.Water): pc.LearnSpell(game.Spells.Find(spl => spl.internalName == "spl_icebolt")); break;
				case (Arcana.Wind): pc.LearnSpell(game.Spells.Find(spl => spl.internalName == "spl_electricvolt")); break;
			}
			game.party.PlayerCharacters[0] = pc;
		}
		bool canAdvanceToNextStage()
		{
			switch (stage)
			{
				case (Stage.Naming):
					{
						string tempName = playerName;
						for (int i = 0; i < tempName.Length; i++)
						{
							if (tempName[i] == ' ')
							{
								tempName = tempName.Remove(i, 1);
								i--;
							}
						}
						if (tempName == "")
							return false;
						if (tempName != "" && gender != Gender.Other)
						{
							for (int i = 1; i < game.party.PlayerCharacters.Count; i++)
							{
								if (tempName == game.party.PlayerCharacters[i].Name)
								{
									return false;
								}
							}
							return true;
						}
						return false;
					}
				case (Stage.Arcana):
					{
						if (selectedArcana != Arcana.Null)
						{
							return true;
						}
						return false;
					}
				case (Stage.Attributes):
					{
						if (pointsToAllocate == 0)
						{
							return true;
						}
						return false;
					}
				case Stage.Finish: { return true; }
			}
			return true;
		}
		public CharacterCreateScreen(Game game)
			: base(game.Input, game.TextureLoader.RequestTexture("UI\\CharCreate\\CharacterCreateWindow"), 0.03f, new Point(game.GetScreenWidth() / 2 - 350, game.GetScreenHeight() / 2 - 295),
			null)
		{
			this.game = game;
			nameBar = game.TextureLoader.RequestTexture("UI\\CharCreate\\nameBar");
			ArcanaWheel = game.TextureLoader.RequestTexture("UI\\CharCreate\\ArcanaWheel");
			arcanaHighlight = game.TextureLoader.RequestTexture("UI\\CharCreate\\ArcanaSelected");
			tooltipWindow = game.TextureLoader.RequestTexture("UI\\Common\\tooltipWindow");
			maleIcon = game.TextureLoader.RequestTexture("UI\\Common\\male");
			femaleIcon = game.TextureLoader.RequestTexture("UI\\Common\\female");
			genderCircle = game.TextureLoader.RequestTexture("UI\\CharCreate\\ChooseGender");
			genderHighlight = game.TextureLoader.RequestTexture("UI\\CharCreate\\ChooseGenderHighlighted");
			greyedOut = game.TextureLoader.RequestTexture("UI\\CharCreate\\NextGreyedOut");
			attributesWindow = game.TextureLoader.RequestTexture("UI\\CharCreate\\CharacterCreateAttributesWindow");
			pointsRemainingBox = game.TextureLoader.RequestTexture("UI\\CharCreate\\AttributePointsBox");
			FinishPanel = game.TextureLoader.RequestTexture("UI\\CharCreate\\FinishWindow");
			finishTick = game.TextureLoader.RequestTexture("UI\\CharCreate\\TickNext");
			displayTutHighlight = true;
			tutHighlight = game.TextureLoader.RequestTexture("UI\\Common\\BlankExitButton");



			strings = new string[21];
			strings[0] = TextMethods.WrapText(game.mediumFont, game.LoadString("UI", "CharCreateHeaderText1"), 540);
			strings[1] = TextMethods.WrapText(game.mediumFont, game.LoadString("UI", "CharCreateHeaderText2"), 540);
			strings[2] = game.LoadString("UI", "CharCreateHeaderText3");
			strings[3] = game.LoadString("UI", "CharCreateHeaderText4");
			strings[4] = game.LoadString("UI", "LightArcanaDesc");
			strings[5] = game.LoadString("UI", "FireArcanaDesc");
			strings[6] = game.LoadString("UI", "EarthArcanaDesc");
			strings[7] = game.LoadString("UI", "ShadowArcanaDesc");
			strings[8] = game.LoadString("UI", "WaterArcanaDesc");
			strings[9] = game.LoadString("UI", "WindArcanaDesc");
			strings[10] = game.LoadString("UI", "StrengthDesc");
			strings[11] = game.LoadString("UI", "MagicDesc");
			strings[12] = game.LoadString("UI", "SpeedDesc");
			strings[13] = game.LoadString("UI", "HealthDesc");
			strings[14] = game.LoadString("UI", "ManaDesc");
			strings[15] = game.LoadString("UI", "CharCreateAttributeDescBox");
			strings[16] = game.LoadString("UI", "PointsRemaining");
			strings[17] = game.LoadString("UI", "CharCreateHelp1");
			strings[18] = game.LoadString("UI", "CharCreateHelp2");
			strings[19] = game.LoadString("UI", "CharCreateHelp3");
			strings[20] = game.LoadString("UI", "CharCreateHelp4");

			baseOffset = new Point(game.GetScreenWidth() / 2 - 350, game.GetScreenHeight() / 2 - 295);
			arcanaRecs = new Rectangle[6];
			helpRectangle = new Rectangle(baseOffset.X + 7, baseOffset.Y + 533, 61, 50);
			var drawVector = new Point(game.GetScreenWidth() / 2 - 200, game.GetScreenHeight() / 2 - 180);
			arcanaRecs[0] = new Rectangle(drawVector.X + 136, drawVector.Y + 1, 128, 128);
			arcanaRecs[1] = new Rectangle(drawVector.X + 254, drawVector.Y + 68, 128, 128);
			arcanaRecs[2] = new Rectangle(drawVector.X + 254, drawVector.Y + 204, 128, 128);
			arcanaRecs[3] = new Rectangle(drawVector.X + 136, drawVector.Y + 271, 128, 128);
			arcanaRecs[4] = new Rectangle(drawVector.X + 18, drawVector.Y + 204, 128, 128);
			arcanaRecs[5] = new Rectangle(drawVector.X + 18, drawVector.Y + 68, 128, 128);

			arcanas[0] = game.TextureLoader.RequestTexture("UI\\Common\\Arcana\\Light");
			arcanas[1] = game.TextureLoader.RequestTexture("UI\\Common\\Arcana\\Fire");
			arcanas[2] = game.TextureLoader.RequestTexture("UI\\Common\\Arcana\\Nature");
			arcanas[3] = game.TextureLoader.RequestTexture("UI\\Common\\Arcana\\Shadow");
			arcanas[4] = game.TextureLoader.RequestTexture("UI\\Common\\Arcana\\Water");
			arcanas[5] = game.TextureLoader.RequestTexture("UI\\Common\\Arcana\\Wind");

			NextButton = new Button(game, game.Input, game.TextureLoader.RequestTexture("UI\\Common\\BlankExitButton"), new Point(TopLeft.X + 631, TopLeft.Y + 532), 63, 52, "", game.TextureLoader.RequestTexture("UI\\Highlights\\ExitHighlight"), 0.028f);
			BackButton = new Button(game, game.Input, game.TextureLoader.RequestTexture("UI\\Common\\BlankSquareButton"), new Point(TopLeft.X + 565, TopLeft.Y + 533), 61, 50, "", game.TextureLoader.RequestTexture("UI\\Highlights\\SquareHighlight"), 0.029f);
			AttributeButtons = new Button[6];
			drawVector = new Point(game.GetScreenWidth() / 2 - 240, game.GetScreenHeight() / 2 - 230);
			var recs = new Rectangle[6];
			AttributeButtons[0] = new Button(game, game.Input, game.TextureLoader.RequestTexture("UI\\Common\\BlankSquareButton"), new Point(drawVector.X + 164, drawVector.Y + 69), 52, 32, "", game.TextureLoader.RequestTexture("UI\\Highlights\\SquareHighlight"), 0.019f);
			AttributeButtons[1] = new Button(game, game.Input, game.TextureLoader.RequestTexture("UI\\Common\\BlankSquareButton"), new Point(drawVector.X + 164, drawVector.Y + 101), 52, 32, "", game.TextureLoader.RequestTexture("UI\\Highlights\\SquareHighlight"), 0.019f);
			AttributeButtons[2] = new Button(game, game.Input, game.TextureLoader.RequestTexture("UI\\Common\\BlankSquareButton"), new Point(drawVector.X + 164, drawVector.Y + 150), 52, 32, "", game.TextureLoader.RequestTexture("UI\\Highlights\\SquareHighlight"), 0.019f);
			AttributeButtons[3] = new Button(game, game.Input, game.TextureLoader.RequestTexture("UI\\Common\\BlankSquareButton"), new Point(drawVector.X + 164, drawVector.Y + 182), 52, 32, "", game.TextureLoader.RequestTexture("UI\\Highlights\\SquareHighlight"), 0.019f);
			AttributeButtons[4] = new Button(game, game.Input, game.TextureLoader.RequestTexture("UI\\Common\\BlankSquareButton"), new Point(drawVector.X + 164, drawVector.Y + 231), 52, 32, "", game.TextureLoader.RequestTexture("UI\\Highlights\\SquareHighlight"), 0.019f);
			AttributeButtons[5] = new Button(game, game.Input, game.TextureLoader.RequestTexture("UI\\Common\\BlankSquareButton"), new Point(drawVector.X + 164, drawVector.Y + 263), 52, 32, "", game.TextureLoader.RequestTexture("UI\\Highlights\\SquareHighlight"), 0.019f);
			charAttributes.Add(Magicians.Attributes.Strength, 1);
			charAttributes.Add(Magicians.Attributes.Magic, 1);
			charAttributes.Add(Magicians.Attributes.Dexterity, 1);
			Health = 45;
			Mana = 25;
		}
		void PlusAttribute(Attributes atr)
		{
			if (pointsToAllocate > 0 && charAttributes[atr] < 10)
			{
				pointsToAllocate--;
				charAttributes[atr]++;
				if (atr == Attributes.Strength)
				{
					Health += 4;
				}
				if (atr == Attributes.Magic)
				{
					Mana += 3;
				}
			}
		}
		void MinusAttribute(Attributes atr)
		{
			if (charAttributes[atr] > 1)
			{
				pointsToAllocate++;
				charAttributes[atr]--;
				if (atr == Attributes.Strength)
				{
					Health -= 4;
				}
				if (atr == Attributes.Magic)
				{
					Mana -= 3;
				}
			}
		}
	}
}
