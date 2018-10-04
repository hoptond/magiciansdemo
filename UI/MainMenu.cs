using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using Microsoft.Xna.Framework.Graphics;
using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Content;
using Microsoft.Xna.Framework.Input;

namespace Magicians
{
	class MainMenu : IScene
	{
		Texture2D menuBackgroundBase;
		Texture2D button;
		Texture2D snowflake;
		Texture2D TitleTexture;
		List<Vector2> snowFlakes;

		List<Texture2D> IntroScreens;
		bool inMenu;
		int currentScreen;
		float screenTimer;
		float screenAlpha = 255;
		byte screenStage; // 0 = fade in, 2 fade out

		OptionsMenu options;
		readonly Game game;
		enum State { Main, Load, Options, Leave };
		State state;
		Point TopLeft = new Point(0, 0);
		public SaveGameMenu saveMenu { get; private set; }
		string[] strings;
		Button[] Buttons;


		const int flakeSize = 16;



		public void Update(GameTime gameTime)
		{
			if (inMenu)
			{
				if (game.IsActive)
				{
					switch (state)
					{
						case (State.Main):
							{
								if (!game.inTransition)
								{
									for (int i = 0; i < Buttons.Length; i++)
									{
										Buttons[i].Update(gameTime);
										if (Buttons[i].Activated())
										{
											switch (i)
											{
												case (0): { game.NewGame(false); break; }
												case (1): { state = State.Load; saveMenu = new SaveGameMenu(game, SaveGameMenu.State.Loading); saveMenu.Load(); break; }
												case (2):
													{
														state = State.Options; options = new OptionsMenu(game);
														break;
													}
												case (3):
													game.BeginSceneChange(Game.GameScenes.Credits);
													game.Audio.SetMusic("Credits", true);
													game.Audio.SetAmbience("silence", 0);
													break;
												case (4): { game.Exit(); break; }
											}
										}
									}
								}
								break;
							}
						case (State.Load):
							{
								var drawOffset = new Point(game.GetScreenWidth() / 2 - 300, game.GetScreenHeight() / 2 - 300);
								saveMenu.Update(gameTime);
								if (saveMenu.CheckForExit())
								{
									state = State.Main;
								}
								break;
							}
						case (State.Options):
							{
								options.Update(gameTime);
								if (options.CheckForExit())
								{
									options = null;
									state = State.Main;
								}
								break;
							}
					}
				}
				Vector2 windDir;
				//windDir = new Vector2(game.randomNumber.Next(minX, minY) + (float)game.randomNumber.NextDouble(), game.randomNumber.Next(maxX, maxY) + (float)game.randomNumber.NextDouble());
				windDir = new Vector2(1.5f, 4.5f);
				for (int i = 0; i < snowFlakes.Count; i++)
				{
					snowFlakes[i] = new Vector2(snowFlakes[i].X + (windDir.X - (float)game.randomNumber.NextDouble()), snowFlakes[i].Y + (windDir.Y - (float)game.randomNumber.NextDouble()));
					if (snowFlakes[i].X > game.GetScreenWidth() || snowFlakes[i].Y > game.GetScreenHeight())
					{
						snowFlakes[i] = new Vector2(game.randomNumber.Next(0, game.GetScreenWidth() + 512) - 512, -flakeSize);
					}
				}
			}
			else
			{
				if (screenStage == 0)
				{
					screenAlpha -= (float)gameTime.ElapsedGameTime.TotalSeconds * 200;
					if (screenAlpha <= 0)
					{
						screenAlpha = 0;
						screenStage = 1;
						screenTimer = 0;
					}
				}
				if (screenStage == 1)
				{
					screenTimer += (float)gameTime.ElapsedGameTime.TotalSeconds;
					if (screenTimer > 2.5f)
					{
						screenStage = 2;
					}
				}
				if (screenStage == 2)
				{
					screenAlpha += (float)gameTime.ElapsedGameTime.TotalSeconds * 200;
					if (screenAlpha >= 255)
					{
						screenAlpha = 255;
						screenStage = 0;
						currentScreen += 1;
						if (currentScreen > IntroScreens.Count - 1)
						{
							inMenu = true;
							game.Audio.SetMusic("MainMenu", false);
							game.Audio.SetAmbience("AmbWindOutside", 40);
						}

					}
				}
			}
		}

		public void CloseSaveMenu()
		{
			state = State.Main;
		}
		public void Draw(SpriteBatch spriteBatch)
		{
			if (inMenu)
			{
				var drawPoint = new Point((game.GetScreenWidth() / 2) - (menuBackgroundBase.Width / 2), (game.GetScreenHeight() / 2) - (menuBackgroundBase.Height / 2));
				var width = game.GetScreenWidth();
				var height = game.GetScreenHeight();
				spriteBatch.Draw(menuBackgroundBase, drawPoint.ToVector2(), null, Color.White, 0, Vector2.Zero, 1, SpriteEffects.None, 1);
				spriteBatch.Draw(TitleTexture, new Vector2((width / 2) - 300, (height / 100) * 8), null, Color.White, 0, Vector2.Zero, 1, SpriteEffects.None, 0.54f);
				for (int i = 0; i < snowFlakes.Count; i++)
				{
					if (snowFlakes[i].X + 16 > 0)
						spriteBatch.Draw(snowflake, new Vector2(snowFlakes[i].X, snowFlakes[i].Y), null, Color.FromNonPremultiplied(255, 255, 255, 125), 0, Vector2.Zero, 1, SpriteEffects.None, 0.55f);
				}
				switch (state)
				{
					case (State.Main):
						{
							if (!game.inTransition)
							{
								for (int i = 0; i < Buttons.Length; i++)
								{
									Buttons[i].Draw(spriteBatch);
									spriteBatch.DrawString(game.mediumFont, strings[i], new Vector2(Buttons[i].Bounds.X + 80, Buttons[i].Bounds.Y + 32), Color.White, 0.0f, TextMethods.CenterText(game.mediumFont, strings[i]), 1.0f, SpriteEffects.None, 0.49f);
								}
							}
							break;
						}
					case (State.Load):
						{
							saveMenu.Draw(spriteBatch);
							break;
						}
					case (State.Options):
						{
							options.Draw(spriteBatch);
							break;
						}
				}
			}
			else
			{
				spriteBatch.Draw(game.debugSquare, new Rectangle(0, 0, game.GetScreenWidth(), game.GetScreenHeight()), null, Color.FromNonPremultiplied(0, 0, 0, (int)screenAlpha), 0, Vector2.Zero, SpriteEffects.None, 0);
				spriteBatch.Draw(IntroScreens[currentScreen], new Vector2(game.GetScreenWidth() / 2, game.GetScreenHeight() / 2), null, Color.White, 0, new Vector2(IntroScreens[currentScreen].Width / 2, IntroScreens[currentScreen].Height / 2), 1, SpriteEffects.None, 1);
			}
		}

		public MainMenu(Game g, bool skipIntro)
		{
			game = g;
			saveMenu = new SaveGameMenu(game, SaveGameMenu.State.Loading);
			CreateSnowflakes();
			IntroScreens = new List<Texture2D>();
			inMenu = skipIntro;
		}
		public void Load(ContentManager content, TextureLoader TextureLoader)
		{
			button = game.TextureLoader.RequestTexture("UI\\MainMenu\\MainMenuButton");
			menuBackgroundBase = game.TextureLoader.RequestTexture("UI\\MainMenu\\MainMenuBackground");
			IntroScreens = new List<Texture2D>();
			IntroScreens.Add(game.TextureLoader.RequestTexture("UI\\MainMenu\\Intro1"));
			IntroScreens.Add(game.TextureLoader.RequestTexture("UI\\MainMenu\\Intro2"));
			TitleTexture = game.TextureLoader.RequestTexture("UI\\MainMenu\\Title");
			strings = new string[5];
			strings[0] = game.LoadString("UI", "MMNewGame");
			strings[1] = game.LoadString("UI", "MMLoadGame");
			strings[2] = game.LoadString("UI", "MMOptions");
			strings[3] = game.LoadString("UI", "MMCredits");
			strings[4] = game.LoadString("UI", "MMQuit");
			snowflake = TextureLoader.RequestTexture("UI\\MainMenu\\snowflake");
			CreateMenuButtons();
		}
		public void CreateMenuButtons()
		{
			Buttons = new Button[5];
			var drawOffset = new Point(TopLeft.X + 96, TopLeft.Y + 256);
			drawOffset.Y = MathHelper.Clamp(drawOffset.Y, 128, game.GetScreenWidth() - 600);
			for (int i = 0; i < Buttons.Length; i++)
			{
				Buttons[i] = new Button(game, game.Input, button, new Point(drawOffset.X, drawOffset.Y), "", game.TextureLoader.RequestTexture("UI\\Highlights\\MainMenuHighlight"), 0.5f);
				drawOffset.Y += 80;
			}
		}
		public void ReloadOptionsMenu()
		{
			options = null;
			options = new OptionsMenu(game);
		}
		public void CreateSnowflakes()
		{
			if (snowFlakes == null)
				snowFlakes = new List<Vector2>();
			snowFlakes.Clear();
			var width = game.GetScreenWidth();
			var height = game.GetScreenHeight();
			for (int x = -64; x < width; x += 32)
			{
				for (int y = -64; y < height; y += 32)
				{
					snowFlakes.Add(new Vector2(x + (game.randomNumber.Next(0, 32) - 32), y + (game.randomNumber.Next(0, 32) - 32)));
				}
			}
		}
	}
}