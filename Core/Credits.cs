using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Microsoft.Xna.Framework.Content;
using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;
using Microsoft.Xna.Framework.Input;
using System.Xml.Linq;

namespace Magicians
{
	class Credits : IScene
	{
		readonly Game game;
		readonly Input input;
		int linebreakSize;
		int upwardMovement = 1;
		List<CreditObject> CreditObjects;
		float timer;
		float totalTimer;
		float moveInterval;

		Texture2D TitleLogo;
		Texture2D EndLogo;

		readonly int screenMiddle;
		int titleY;
		int titleAlpha;
		byte creditStage; //0 includes the title screen, 1 is where credits move up normally, 2 is "thank you for playing this game", 3 is fadeout

		public Credits(Game g, Input i)
		{
			game = g;
			input = i;
			screenMiddle = g.GetScreenWidth() / 2;
			moveInterval = 0.01f;
			linebreakSize = (int)g.largeFont.MeasureString("|").Y;
		}
		public void Load(ContentManager cm, TextureLoader tl)
		{
			TitleLogo = cm.Load<Texture2D>("UI\\MainMenu\\Title");
			EndLogo = cm.Load<Texture2D>("UI\\MainMenu\\EndCreditLogo");
			titleY = game.GetScreenHeight() / 2;
			CreditObjects = new List<CreditObject>();
			var bottomY = game.GetScreenHeight();
			var data = XDocument.Load("Content\\Strings\\English\\Credits.xml", LoadOptions.None);
			var elements = data.Element("Credits").Descendants().ToList();
			var positionY = bottomY;
			foreach (XElement elem in elements)
			{
				var type = elem.Name.ToString();
				switch (type)
				{
					case "Header":
						positionY += lineSpace(game.largeFont);
						CreditObjects.Add(new CreditObject(elem.Value, new Color(255, 212, 104, 255), game.largeFont, new Vector2(screenMiddle, positionY)));
						positionY += 4;
						break;
					case "Subheader":
						positionY += lineSpace(game.mediumFont);
						CreditObjects.Add(new CreditObject(elem.Value, new Color(255, 255, 255, 255), game.mediumFont, new Vector2(screenMiddle, positionY)));
						break;
					case "Descriptor":
						positionY += lineSpace(game.mediumFont);
						CreditObjects.Add(new CreditObject(elem.Value, new Color(188, 188, 188, 255), game.mediumFont, new Vector2(screenMiddle, positionY)));
						break;
					case "Gap":
						var lines = int.Parse(elem.Value);
						for (int i = 0; i < lines; i++)
						{
							positionY += linebreakSize / 2;
						}
						break;
				}
			}
		}
		int lineSpace(SpriteFont font)
		{
			return (int)(font.MeasureString("|").Y);
		}
		public void Update(GameTime gameTime)
		{
			int repeats = 1;
			if (input.GetNumberOfKeysPressed() > 0)
				repeats = 4;
			for (int r = 0; r < repeats; r++)
			{
				timer += (float)gameTime.ElapsedGameTime.TotalSeconds;
				totalTimer += (float)gameTime.ElapsedGameTime.TotalSeconds;
				switch (creditStage)
				{
					case 0:
						{
							timer += (float)gameTime.ElapsedGameTime.TotalSeconds;
							if (timer >= moveInterval)
							{
								titleAlpha += 3;
								titleAlpha = MathHelper.Clamp(titleAlpha, 0, 255);
							}
							if (totalTimer >= 10)
								creditStage = 1;
							break;
						}
					case 1:
						{
							timer += (float)gameTime.ElapsedGameTime.TotalSeconds;
							if (timer >= moveInterval)
							{
								timer = 0;
								titleY -= upwardMovement;
								for (int i = 0; i < CreditObjects.Count; i++)
								{
									CreditObjects[i].DrawnPosition.Y -= upwardMovement;
									if (CreditObjects[i].DrawnPosition.Y + CreditObjects[i].Center.Y * 2 < 0)
									{
										if (i == CreditObjects.Count - 1)
										{
											totalTimer = 0;
											titleY = game.GetScreenHeight() / 2;
											creditStage = 2;
											titleAlpha = 0;
										}
									}
								}
							}
							break;
						}
					case 2:
						{
							if (timer > moveInterval)
							{
								titleAlpha += 3;
								titleAlpha = MathHelper.Clamp(titleAlpha, 0, 255);
							}
							if (totalTimer > 20)
							{
								creditStage = 3;
								return;
							}
							break;
						}
					case 3:
						{
							if (timer > moveInterval)
							{
								titleAlpha -= 3;
								titleAlpha = MathHelper.Clamp(titleAlpha, 0, 255);
							}
							if (totalTimer > 10 && !game.inTransition)
							{
								creditStage = 3;
								game.BeginSceneChange(Game.GameScenes.MainMenu);
								return;
							}
							break;
						}
				}
			}
		}
		public void Draw(SpriteBatch spriteBatch)
		{
			if (creditStage == 1 || creditStage == 0)
			{
				if (titleY + TitleLogo.Bounds.Height > 0)
					spriteBatch.Draw(TitleLogo, new Vector2(screenMiddle, titleY), null, Color.FromNonPremultiplied(255, 255, 255, titleAlpha), 0, new Vector2(TitleLogo.Bounds.Width / 2, TitleLogo.Bounds.Height / 2), 1, SpriteEffects.None, 0.05f);
			}
			if (creditStage == 1)
			{
				for (int i = 0; i < CreditObjects.Count; i++)
				{
					if (CreditObjects[i].DrawnPosition.Y + CreditObjects[i].Center.Y <= 0)
						continue;
					spriteBatch.DrawString(CreditObjects[i].Font, CreditObjects[i].Text, CreditObjects[i].DrawnPosition, CreditObjects[i].Color, 0, CreditObjects[i].Center, 1, SpriteEffects.None, 0.5f);
					if (CreditObjects[i].DrawnPosition.Y - CreditObjects[i].Center.Y >= game.GetScreenHeight())
						break;
				}
				return;
			}
			if (creditStage == 2)
				spriteBatch.Draw(EndLogo, new Vector2(screenMiddle, titleY), null, Color.FromNonPremultiplied(255, 255, 255, titleAlpha), 0, new Vector2(TitleLogo.Bounds.Width / 2, TitleLogo.Bounds.Height / 2), 1, SpriteEffects.None, 0.05f);
		}
		class CreditObject
		{
			public SpriteFont Font;
			public string Text;
			public Color Color;
			public Vector2 DrawnPosition;
			public Vector2 Center;
			public CreditObject(string text, Color color, SpriteFont font, Vector2 position)
			{
				Text = text;
				Color = color;
				Font = font;
				DrawnPosition = position;
				Center = Font.MeasureString(Text);
				Center = new Vector2(Center.X / 2, Center.Y / 2);
			}
		}
	}
}
