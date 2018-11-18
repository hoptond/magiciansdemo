using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Runtime.Serialization.Formatters.Binary;
using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Content;
using Microsoft.Xna.Framework.Graphics;
using Microsoft.Xna.Framework.Input;


namespace Magicians
{
	class OptionsMenu : BaseUIWindow
	{
		readonly Game game;
		readonly Texture2D inputActive;
		readonly Texture2D muted;
		readonly SpriteFont spriteFont;
		readonly SpriteFont titleFont;
		OptionsSettings newSettings;
		readonly Texture2D dragSquare;
		readonly Texture2D checkBoxActive;
		bool reboundActive;
		readonly Button ConfirmChangesButton;

		readonly List<DisplayMode> resolutions = new List<DisplayMode>();
		int activeResolution;

		int activeKey = -1;
		readonly Rectangle[] buttonRecs = new Rectangle[16];

		readonly string[] strings = new string[21];


		public override bool CheckForExit()
		{
			if (base.CheckForExit() || ConfirmChangesButton.Activated())
				return true;
			return false;
		}
		public override void Update(GameTime gameTime)
		{
			if (ConfirmChangesButton.Activated())
				applySettings();
			if (!reboundActive)
			{
				var soundRec = new Rectangle(TopLeft.X + 406, TopLeft.Y + 44, 28, 28);
				var musicRec = new Rectangle(TopLeft.X + 406, TopLeft.Y + 110, 28, 28);
				if (game.Input.HasMouseClickedOnRectangle(soundRec))
				{
					newSettings.mutedSound = !newSettings.mutedSound;
					if (newSettings.mutedSound)
					{
						if (game.Audio.ambience != null)
							game.Audio.ambience.Volume = 0;
					}
					else
					{
						if (game.Audio.ambience != null)
							game.Audio.ambience.Volume = (game.settings.soundVolume / 100) * 60;
					}
				}
				if (game.Input.HasMouseClickedOnRectangle(musicRec))
				{
					newSettings.mutedMusic = !newSettings.mutedMusic;
					if (newSettings.mutedMusic)
					{
						if (game.Audio.music != null)
							game.Audio.music.Volume = 0;
					}
					else
					{
						if (game.Audio.music != null)
							game.Audio.music.Volume = newSettings.musVolume;
					}
				}
				var soundDragRec = new Rectangle(TopLeft.X + 442, TopLeft.Y + 48, 120, 20);
				var musDragRec = new Rectangle(TopLeft.X + 442, TopLeft.Y + 114, 120, 20);
				if (game.Input.IsMouseOver(soundDragRec) && game.Input.IsMouseButtonPressed())
				{
					newSettings.soundVolume = (game.Input.oldMouseState.X - musDragRec.Left) / 100f;
					newSettings.soundVolume = MathHelper.Clamp(newSettings.soundVolume, 0, 1);
					if (game.Audio.ambience != null && !newSettings.mutedSound)
						game.Audio.ambience.Volume = MathHelper.Clamp(((((game.Input.oldMouseState.X - musDragRec.Left) / 100f) * 60f) / 100f), 0, 0.6f);
				}
				if (game.Input.IsMouseOver(musDragRec) && game.Input.IsMouseButtonPressed())
				{
					newSettings.musVolume = (game.Input.oldMouseState.X - musDragRec.Left) / 100f;
					newSettings.musVolume = MathHelper.Clamp(newSettings.musVolume, 0, 1);
					if (!newSettings.mutedMusic)
						game.Audio.music.Volume = newSettings.musVolume;
				}
				var fullscreenRec = new Rectangle(TopLeft.X + 23, TopLeft.Y + 89, 24, 24);
				var windowedRectangle = new Rectangle(TopLeft.X + 23, TopLeft.Y + 130, 24, 24);
				if (game.Input.HasMouseClickedOnRectangle(windowedRectangle))
					newSettings.fullScreen = false;
				if (game.Input.HasMouseClickedOnRectangle(fullscreenRec))
					newSettings.fullScreen = true;
				var resolutionRectangle = new Rectangle(TopLeft.X + 23, TopLeft.Y + 48, 168, 28);
				if (game.Input.HasMouseClickedOnRectangle(resolutionRectangle))
				{
					activeResolution++;
					if (activeResolution == resolutions.Count)
						activeResolution = 0;
					if (activeResolution == -1)
						activeResolution = resolutions.Count - 1;
					newSettings.horzRez = resolutions[activeResolution].Width;
					newSettings.vertRez = resolutions[activeResolution].Height;
				}
				if (game.Input.HasMouseClickedOnRectangle(resolutionRectangle, false, true))
				{
					activeResolution--;
					if (activeResolution == resolutions.Count)
						activeResolution = 0;
					if (activeResolution == -1)
						activeResolution = resolutions.Count - 1;
					newSettings.horzRez = resolutions[activeResolution].Width;
					newSettings.vertRez = resolutions[activeResolution].Height;
				}
				for (int i = 0; i < buttonRecs.Length; i++)
				{
					if (game.Input.HasMouseClickedOnRectangle(buttonRecs[i]))
					{
						reboundActive = true;
						activeKey = i;
					}
				}
				for (int i = 1; i < 4; i++)
				{
					var rec = new Rectangle(TopLeft.X + 212, TopLeft.Y + 48 + (41 * (i - 1)), 24, 24);
					if (game.Input.HasMouseClickedOnRectangle(rec))
						newSettings.textSpeed = (TextSpeed)i;
				}
				int x = TopLeft.X + 36;
				for (int i = 1; i < 5; i++)
				{
					var rec = new Rectangle(x, TopLeft.Y + 203, 24, 24);
					if (game.Input.HasMouseClickedOnRectangle(rec))
					{
						newSettings.combatSpeed = (byte)i;
						break;
					}
					x += 63;
				}
			}
			if (reboundActive)
			{
				var keyboard = Keyboard.GetState();
				if (keyboard.GetPressedKeys().Length > 0)
				{
					var keys = keyboard.GetPressedKeys();
					var newKey = keys[0];
					if (!newSettings.IsKeyInUse(newKey))
					{
						switch (activeKey)
						{
							case (0): { newSettings.upKey = newKey; break; }
							case (1): { newSettings.leftKey = newKey; break; }
							case (2): { newSettings.downKey = newKey; break; }
							case (3): { newSettings.rightKey = newKey; break; }
							case (4): { newSettings.mapKey = newKey; break; }
							case (5): { newSettings.interactKey = newKey; break; }
							case (6): { newSettings.runKey = newKey; break; }
							case (7): { newSettings.pauseKey = newKey; break; }
							case (8): { newSettings.statusKey = newKey; break; }
							case (9): { newSettings.spellKey = newKey; break; }
							case (10): { newSettings.inventoryKey = newKey; break; }
							case (11): { newSettings.sneakKey = newKey; break; }
						}
					}
					reboundActive = false;
				}
				if (game.Input.IsMouseButtonPressed())
					reboundActive = false;
			}
			ConfirmChangesButton.Update(gameTime);
			base.Update(gameTime);
		}
		void applySettings()
		{
			game.settings = newSettings;
			game.graphics.PreferredBackBufferWidth = game.settings.horzRez;
			game.graphics.PreferredBackBufferHeight = game.settings.vertRez;
			game.graphics.IsFullScreen = game.settings.fullScreen;
			game.graphics.ApplyChanges();
			game.Window.Position = new Point((GraphicsAdapter.DefaultAdapter.CurrentDisplayMode.Width / 2) - (game.settings.horzRez / 2), (GraphicsAdapter.DefaultAdapter.CurrentDisplayMode.Height / 2) - (game.settings.vertRez / 2));
			bool clamp = game.Camera.clampViewport;
			game.Camera = new Camera2D(game);
			game.Camera.clampViewport = clamp;

			var binaryFormatter = new BinaryFormatter();
			var output = File.Create(game.userFolder + game.PathSeperator + "options");
			binaryFormatter.Serialize(output, newSettings);
			output.Close();
			output.Dispose();
			game.worldUI.SetButtons();
			if (game.Scene is MainMenu)
			{
				var menu = (MainMenu)game.Scene;
				menu.CreateSnowflakes();
				menu.ReloadOptionsMenu();
				menu.CreateMenuButtons();
			}
			else if (game.Scene is Map)
			{
				game.CloseUIWindow();
				game.OpenUIWindow(new OptionsMenu(game));
			}

		}
		public override void Draw(SpriteBatch spriteBatch)
		{
			base.Draw(spriteBatch);
			ConfirmChangesButton.Draw(spriteBatch);
			var soundRec = new Rectangle(TopLeft.X + 406, TopLeft.Y + 44, 28, 28);
			var musicRec = new Rectangle(TopLeft.X + 406, TopLeft.Y + 110, 28, 28);
			if (newSettings.mutedSound)
				spriteBatch.Draw(muted, soundRec, null, Color.White, 0.0f, Vector2.Zero, SpriteEffects.None, 0.18f);
			if (newSettings.mutedMusic)
				spriteBatch.Draw(muted, musicRec, null, Color.White, 0.0f, Vector2.Zero, SpriteEffects.None, 0.18f);
			spriteBatch.DrawString(spriteFont, strings[0], new Vector2(TopLeft.X + 102, TopLeft.Y + 26), Color.Black, 0, TextMethods.CenterText(spriteFont, strings[0]), 1, SpriteEffects.None, 0.18f);
			spriteBatch.DrawString(spriteFont, strings[1], new Vector2(TopLeft.X + 302, TopLeft.Y + 26), Color.Black, 0, TextMethods.CenterText(spriteFont, strings[1]), 1, SpriteEffects.None, 0.18f);
			spriteBatch.DrawString(spriteFont, strings[2], new Vector2(TopLeft.X + 502, TopLeft.Y + 26), Color.Black, 0, TextMethods.CenterText(spriteFont, strings[2]), 1, SpriteEffects.None, 0.18f);

			spriteBatch.DrawString(spriteFont, newSettings.horzRez + " x " + newSettings.vertRez, new Vector2(TopLeft.X + 28, TopLeft.Y + 54), Color.Black, 0, Vector2.Zero, 1, SpriteEffects.None, 0.18f);
			spriteBatch.DrawString(spriteFont, strings[3], new Vector2(TopLeft.X + 53, TopLeft.Y + 89), Color.Black, 0, Vector2.Zero, 1, SpriteEffects.None, 0.18f);
			spriteBatch.DrawString(spriteFont, strings[4], new Vector2(TopLeft.X + 53, TopLeft.Y + 130), Color.Black, 0, Vector2.Zero, 1, SpriteEffects.None, 0.18f);
			var windowedRectangle = new Rectangle(TopLeft.X + 23, TopLeft.Y + 130, 24, 24);
			if (!newSettings.fullScreen)
				spriteBatch.Draw(checkBoxActive, windowedRectangle, null, Color.White, 0.0f, Vector2.Zero, SpriteEffects.None, 0.18f);
			var fullscreenRec = new Rectangle(TopLeft.X + 23, TopLeft.Y + 89, 24, 24);
			if (newSettings.fullScreen)
				spriteBatch.Draw(checkBoxActive, fullscreenRec, null, Color.White, 0.0f, Vector2.Zero, SpriteEffects.None, 0.18f);
			spriteBatch.Draw(checkBoxActive, new Rectangle(TopLeft.X + 212, TopLeft.Y + 48 + (41 * ((int)newSettings.textSpeed - 1)), 24, 24), null, Color.White, 0.0f, Vector2.Zero, SpriteEffects.None, 0.18f);
			spriteBatch.DrawString(spriteFont, strings[5], new Vector2(TopLeft.X + 242, TopLeft.Y + 48), Color.Black, 0, Vector2.Zero, 1, SpriteEffects.None, 0.18f);
			spriteBatch.DrawString(spriteFont, strings[6], new Vector2(TopLeft.X + 242, TopLeft.Y + 89), Color.Black, 0, Vector2.Zero, 1, SpriteEffects.None, 0.18f);
			spriteBatch.DrawString(spriteFont, strings[7], new Vector2(TopLeft.X + 242, TopLeft.Y + 130), Color.Black, 0, Vector2.Zero, 1, SpriteEffects.None, 0.18f);

			spriteBatch.Draw(dragSquare, new Rectangle(TopLeft.X + 452, TopLeft.Y + 48, (int)(newSettings.soundVolume * 100f), 20), null, Color.MistyRose, 0.0f, Vector2.Zero, SpriteEffects.None, 0.18f);
			spriteBatch.Draw(dragSquare, new Rectangle(TopLeft.X + 452, TopLeft.Y + 114, (int)(newSettings.musVolume * 100f), 20), null, Color.MistyRose, 0.0f, Vector2.Zero, SpriteEffects.None, 0.18f);

			spriteBatch.DrawString(spriteFont, newSettings.upKey.ToString(), new Vector2(TopLeft.X + 172, TopLeft.Y + 272), Color.Black, 0, TextMethods.CenterText(spriteFont, newSettings.upKey.ToString()), TextMethods.ResizeText(spriteFont, newSettings.upKey.ToString(), 60), SpriteEffects.None, 0.18f);
			spriteBatch.DrawString(spriteFont, newSettings.leftKey.ToString(), new Vector2(TopLeft.X + 172, TopLeft.Y + 314), Color.Black, 0, TextMethods.CenterText(spriteFont, newSettings.leftKey.ToString()), TextMethods.ResizeText(spriteFont, newSettings.downKey.ToString(), 60), SpriteEffects.None, 0.18f);
			spriteBatch.DrawString(spriteFont, newSettings.downKey.ToString(), new Vector2(TopLeft.X + 172, TopLeft.Y + 354), Color.Black, 0, TextMethods.CenterText(spriteFont, newSettings.downKey.ToString()), TextMethods.ResizeText(spriteFont, newSettings.leftKey.ToString(), 60), SpriteEffects.None, 0.18f);
			spriteBatch.DrawString(spriteFont, newSettings.rightKey.ToString(), new Vector2(TopLeft.X + 172, TopLeft.Y + 394), Color.Black, 0, TextMethods.CenterText(spriteFont, newSettings.rightKey.ToString()), TextMethods.ResizeText(spriteFont, newSettings.rightKey.ToString(), 60), SpriteEffects.None, 0.18f);

			spriteBatch.DrawString(spriteFont, strings[8], new Vector2(TopLeft.X + 38, TopLeft.Y + 262), Color.Black, 0, Vector2.Zero, 1, SpriteEffects.None, 0.18f);
			spriteBatch.DrawString(spriteFont, strings[9], new Vector2(TopLeft.X + 38, TopLeft.Y + 300), Color.Black, 0, Vector2.Zero, 1, SpriteEffects.None, 0.18f);
			spriteBatch.DrawString(spriteFont, strings[10], new Vector2(TopLeft.X + 38, TopLeft.Y + 344), Color.Black, 0, Vector2.Zero, 1, SpriteEffects.None, 0.18f);
			spriteBatch.DrawString(spriteFont, strings[11], new Vector2(TopLeft.X + 38, TopLeft.Y + 384), Color.Black, 0, Vector2.Zero, 1, SpriteEffects.None, 0.18f);

			spriteBatch.DrawString(spriteFont, newSettings.mapKey.ToString(), new Vector2(TopLeft.X + 344, TopLeft.Y + 272), Color.Black, 0, TextMethods.CenterText(spriteFont, newSettings.mapKey.ToString()), TextMethods.ResizeText(spriteFont, newSettings.mapKey.ToString(), 60), SpriteEffects.None, 0.18f);
			spriteBatch.DrawString(spriteFont, newSettings.interactKey.ToString(), new Vector2(TopLeft.X + 344, TopLeft.Y + 314), Color.Black, 0, TextMethods.CenterText(spriteFont, newSettings.interactKey.ToString()), TextMethods.ResizeText(spriteFont, newSettings.interactKey.ToString(), 60), SpriteEffects.None, 0.18f);
			spriteBatch.DrawString(spriteFont, newSettings.runKey.ToString(), new Vector2(TopLeft.X + 344, TopLeft.Y + 354), Color.Black, 0, TextMethods.CenterText(spriteFont, newSettings.runKey.ToString()), TextMethods.ResizeText(spriteFont, newSettings.runKey.ToString(), 60), SpriteEffects.None, 0.18f);
			spriteBatch.DrawString(spriteFont, newSettings.pauseKey.ToString(), new Vector2(TopLeft.X + 344, TopLeft.Y + 394), Color.Black, 0, TextMethods.CenterText(spriteFont, newSettings.pauseKey.ToString()), TextMethods.ResizeText(spriteFont, newSettings.pauseKey.ToString(), 60), SpriteEffects.None, 0.18f);

			spriteBatch.DrawString(spriteFont, strings[12], new Vector2(TopLeft.X + 220, TopLeft.Y + 262), Color.Black, 0, Vector2.Zero, 1, SpriteEffects.None, 0.18f);
			spriteBatch.DrawString(spriteFont, strings[13], new Vector2(TopLeft.X + 220, TopLeft.Y + 300), Color.Black, 0, Vector2.Zero, 1, SpriteEffects.None, 0.18f);
			spriteBatch.DrawString(spriteFont, strings[14], new Vector2(TopLeft.X + 220, TopLeft.Y + 344), Color.Black, 0, Vector2.Zero, 1, SpriteEffects.None, 0.18f);
			spriteBatch.DrawString(spriteFont, strings[15], new Vector2(TopLeft.X + 220, TopLeft.Y + 384), Color.Black, 0, Vector2.Zero, 1, SpriteEffects.None, 0.18f);

			spriteBatch.DrawString(spriteFont, newSettings.statusKey.ToString(), new Vector2(TopLeft.X + 520, TopLeft.Y + 272), Color.Black, 0, TextMethods.CenterText(spriteFont, newSettings.statusKey.ToString()), TextMethods.ResizeText(spriteFont, newSettings.statusKey.ToString(), 60), SpriteEffects.None, 0.18f);
			spriteBatch.DrawString(spriteFont, newSettings.spellKey.ToString(), new Vector2(TopLeft.X + 520, TopLeft.Y + 314), Color.Black, 0, TextMethods.CenterText(spriteFont, newSettings.spellKey.ToString()), TextMethods.ResizeText(spriteFont, newSettings.spellKey.ToString(), 60), SpriteEffects.None, 0.18f);
			spriteBatch.DrawString(spriteFont, newSettings.inventoryKey.ToString(), new Vector2(TopLeft.X + 520, TopLeft.Y + 354), Color.Black, 0, TextMethods.CenterText(spriteFont, newSettings.inventoryKey.ToString()), TextMethods.ResizeText(spriteFont, newSettings.inventoryKey.ToString(), 60), SpriteEffects.None, 0.18f);
			spriteBatch.DrawString(spriteFont, newSettings.sneakKey.ToString(), new Vector2(TopLeft.X + 520, TopLeft.Y + 394), Color.Black, 0, TextMethods.CenterText(spriteFont, newSettings.sneakKey.ToString()), TextMethods.ResizeText(spriteFont, newSettings.sneakKey.ToString(), 60), SpriteEffects.None, 0.18f);

			spriteBatch.DrawString(spriteFont, strings[16], new Vector2(TopLeft.X + 386, TopLeft.Y + 262), Color.Black, 0, Vector2.Zero, 1, SpriteEffects.None, 0.18f);
			spriteBatch.DrawString(spriteFont, strings[17], new Vector2(TopLeft.X + 386, TopLeft.Y + 300), Color.Black, 0, Vector2.Zero, 1, SpriteEffects.None, 0.18f);
			spriteBatch.DrawString(spriteFont, strings[18], new Vector2(TopLeft.X + 386, TopLeft.Y + 344), Color.Black, 0, Vector2.Zero, 1, SpriteEffects.None, 0.18f);
			spriteBatch.DrawString(spriteFont, strings[20], new Vector2(TopLeft.X + 386, TopLeft.Y + 384), Color.Black, 0, Vector2.Zero, 1, SpriteEffects.None, 0.18f);


			spriteBatch.DrawString(spriteFont, strings[19], new Vector2(TopLeft.X + 36, TopLeft.Y + 176), Color.Black, 0, Vector2.Zero, 1, SpriteEffects.None, 0.18f);

			spriteBatch.Draw(checkBoxActive, new Rectangle(TopLeft.X + 36 + ((newSettings.combatSpeed * 62) - 62), TopLeft.Y + 203, 24, 24), null, Color.White, 0.0f, Vector2.Zero, SpriteEffects.None, 0.18f);
			for (int i = 1; i < 5; i++)
			{
				spriteBatch.DrawString(spriteFont, i.ToString(), new Vector2(TopLeft.X + 63 + ((i * 62) - 62), TopLeft.Y + 203), Color.Black, 0, Vector2.Zero, 1, SpriteEffects.None, 0.18f);
			}

			if (reboundActive)
			{
				spriteBatch.Draw(inputActive, buttonRecs[activeKey], null, Color.White, 0.0f, Vector2.Zero, SpriteEffects.None, 0.17f);
			}
		}
		public OptionsMenu(Game g)
			: base(g.Input, g.TextureLoader.RequestTexture("UI\\MainMenu\\Options"), 0.19f, new Point(g.GetScreenWidth() / 2 - 300, g.GetScreenHeight() / 2 - 245),
				   new Button(g.Audio, g.Input, g.TextureLoader.RequestTexture("UI\\Common\\BlankExitButton"), new Point((g.GetScreenWidth() / 2 - 300) + 531, (g.GetScreenHeight() / 2 - 300) + 497), "", g.TextureLoader.RequestTexture("UI\\Highlights\\ExitHighlight"), 0.01f))
		{
			game = g;
			newSettings = (OptionsSettings)game.settings.Clone();
			spriteFont = game.mediumFont;
			titleFont = game.largeFont;
			dragSquare = game.debugSquare;
			checkBoxActive = game.TextureLoader.RequestTexture("UI\\Common\\CheckBoxActive");
			muted = game.TextureLoader.RequestTexture("UI\\MainMenu\\muted");
			inputActive = game.TextureLoader.RequestTexture("UI\\MainMenu\\inputActive");
			var baseOffset = new Vector2(TopLeft.X, TopLeft.Y);
			ConfirmChangesButton = new Button(g.Audio, g.Input, game.TextureLoader.RequestTexture("UI\\Common\\BlankExitButton"), new Point((g.GetScreenWidth() / 2 - 300) + 6, (g.GetScreenHeight() / 2 - 300) + 497), "", g.TextureLoader.RequestTexture("UI\\Highlights\\ExitHighlightMirrored"), 0.01f);

			strings[0] = game.LoadString("UI", "OptionsVideo");
			strings[1] = game.LoadString("UI", "OptionsTextSpeed");
			strings[2] = game.LoadString("UI", "OptionsAudio");
			strings[3] = game.LoadString("UI", "OptionsFullscreen");
			strings[4] = game.LoadString("UI", "OptionsWindowed");
			strings[5] = game.LoadString("UI", "OptionsSlow");
			strings[6] = game.LoadString("UI", "OptionsMedium");
			strings[7] = game.LoadString("UI", "OptionsFast");
			strings[8] = game.LoadString("UI", "OptionsUp");
			strings[9] = game.LoadString("UI", "OptionsLeft");
			strings[10] = game.LoadString("UI", "OptionsDown");
			strings[11] = game.LoadString("UI", "OptionsRight");
			strings[12] = game.LoadString("UI", "OptionsMap");
			strings[13] = game.LoadString("UI", "OptionsInteract");
			strings[14] = game.LoadString("UI", "OptionsRun");
			strings[15] = game.LoadString("UI", "OptionsPause");
			strings[16] = game.LoadString("UI", "OptionsStatus");
			strings[17] = game.LoadString("UI", "OptionsSpellbook");
			strings[18] = game.LoadString("UI", "OptionsInventory");
			strings[19] = game.LoadString("UI", "OptionsCombatSpeed");
			strings[20] = game.LoadString("UI", "OptionsSneak");

			foreach (DisplayMode mode in game.GraphicsDevice.Adapter.SupportedDisplayModes)
			{
				if (mode.Width < 800 || mode.Height < 600)
					continue;
				resolutions.Add(mode);
			}
			for (int i = 0; i < resolutions.Count; i++)
			{
				if (i > 1)
				{
					if (resolutions[i].Width == resolutions[i - 1].Width && resolutions[i].Height == resolutions[i - 1].Height)
					{
						resolutions.RemoveAt(i);
						i--;
						continue;
					}
					if (resolutions[i].Width > GraphicsAdapter.DefaultAdapter.CurrentDisplayMode.Width || resolutions[i].Height > GraphicsAdapter.DefaultAdapter.CurrentDisplayMode.Height)
					{
						resolutions.RemoveAt(i);
						i--;
						continue;
					}
				}
			}
			for (int i = 0; i < resolutions.Count; i++)
			{
				if (newSettings.horzRez == resolutions[i].Width && newSettings.vertRez == resolutions[i].Height)
				{
					activeResolution = i;
					break;
				}
			}
			int vert = 256;
			for (int i = 0; i < 4; i++)
			{

				buttonRecs[i] = new Rectangle(TopLeft.X + 134, TopLeft.Y + vert, 80, 32);
				vert += 41;
			}
			vert = 256;
			for (int i = 4; i < 8; i++)
			{
				buttonRecs[i] = new Rectangle(TopLeft.X + 305, TopLeft.Y + vert, 80, 32);
				vert += 41;
			}
			vert = 256;
			for (int i = 8; i < 12; i++)
			{
				buttonRecs[i] = new Rectangle(TopLeft.X + 481, TopLeft.Y + vert, 80, 32);
				vert += 41;
			}
		}
	}
	[Serializable]
	struct OptionsSettings : ICloneable
	{
		public int horzRez;
		public int vertRez;
		public TextSpeed textSpeed;
		public bool fullScreen;


		public float musVolume;
		public bool mutedMusic;
		public bool mutedSound;
		public float soundVolume;


		public Keys mapKey;
		public Keys statusKey;
		public Keys inventoryKey;
		public Keys pauseKey;
		public Keys spellKey;

		public Keys upKey;
		public Keys downKey;
		public Keys leftKey;
		public Keys rightKey;

		public Keys runKey;
		public Keys interactKey;
		public Keys sneakKey;

		public byte combatSpeed;

		public object Clone()
		{
			var newSettings = new OptionsSettings();

			newSettings.horzRez = this.horzRez;
			newSettings.vertRez = this.vertRez;
			newSettings.textSpeed = this.textSpeed;
			newSettings.musVolume = this.musVolume;
			newSettings.soundVolume = this.soundVolume;
			newSettings.fullScreen = this.fullScreen;

			newSettings.mapKey = this.mapKey;
			newSettings.statusKey = this.statusKey;
			newSettings.inventoryKey = this.inventoryKey;
			newSettings.spellKey = this.spellKey;
			newSettings.upKey = this.upKey;
			newSettings.downKey = this.downKey;
			newSettings.leftKey = this.leftKey;
			newSettings.rightKey = this.rightKey;
			newSettings.pauseKey = this.pauseKey;
			newSettings.sneakKey = this.sneakKey;
			newSettings.interactKey = this.interactKey;
			newSettings.runKey = this.runKey;

			newSettings.mutedMusic = this.mutedMusic;
			newSettings.mutedSound = this.mutedSound;

			newSettings.combatSpeed = this.combatSpeed;

			return newSettings;
		}
		public bool IsKeyInUse(Keys k)
		{
			//todo: condense these into an array
			if (k == upKey || k == leftKey || k == downKey || k == rightKey)
				return true;
			if (k == runKey || k == interactKey || k == pauseKey)
				return true;
			if (k == mapKey || k == statusKey || k == inventoryKey || k == sneakKey)
				return true;
			return false;
		}
	}
}