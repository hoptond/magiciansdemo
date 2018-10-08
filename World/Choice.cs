using System;
using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;
using Microsoft.Xna.Framework.Content;

namespace Magicians
{
	class Choice
	{
		readonly Game game;
		readonly string[] choices;
		readonly string[] destinations;
		readonly Rectangle[] rectangles;
		readonly EventManager eventManager;
		readonly SpriteFont font;
		readonly Texture2D dialogueWindow;
		readonly Texture2D point;
		string text = "";
		int highlightedChoice = -1;
		Point lastMousePoint;

		public void Update()
		{
			if (game.Input.IsKeyReleased(game.settings.upKey))
			{
				if (highlightedChoice == 0)
					highlightedChoice = 10;
				highlightedChoice = MathHelper.Clamp(highlightedChoice -= 1, 0, choices.Length - 1);
			}
			if (game.Input.IsKeyReleased(game.settings.downKey))
			{
				if (highlightedChoice == choices.Length - 1)
					highlightedChoice = -1;
				highlightedChoice = MathHelper.Clamp(highlightedChoice += 1, 0, choices.Length - 1);
			}
			if (game.Input.IsKeyReleased(game.settings.interactKey) && highlightedChoice > -1)
			{
				eventManager.SetActiveEvent(destinations[highlightedChoice]);
				eventManager.DoEvent();
				return;
			}
			if (game.Input.oldMouseState.Position != lastMousePoint)
				mouseOverHighlightedChoices();
			if (game.Input.IsMouseButtonReleased())
			{
				mouseOverHighlightedChoices();
				if (highlightedChoice > -1)
				{
					eventManager.SetActiveEvent(destinations[highlightedChoice]);
					eventManager.DoEvent();
				}
			}
			lastMousePoint = game.Input.oldMouseState.Position;
		}

		public void Draw(SpriteBatch spriteBatch)
		{
			var baseOffset = new Point((game.GetScreenWidth() / 2) - (dialogueWindow.Width / 2), game.GetScreenHeight() - dialogueWindow.Height - 20);
			spriteBatch.Draw(dialogueWindow, new Rectangle(baseOffset.X, baseOffset.Y, 560, 260), null, Color.White, 0.0f, Vector2.Zero, SpriteEffects.None, 0.12f);
			var position = new Point(baseOffset.X + 32, baseOffset.Y);
			if (text != "")
			{
				spriteBatch.DrawString(game.mediumFont, text, new Vector2(baseOffset.X + 22, baseOffset.Y + 24), Color.White);
				position.Y += (int)game.mediumFont.MeasureString(text).Y + 2 + 24;
			}
			for (int i = 0; i < choices.Length; i++)
			{
				if (highlightedChoice == i)
					spriteBatch.DrawString(game.mediumFont, choices[i], new Vector2(position.X + 18, position.Y), Color.White);
				else
					spriteBatch.DrawString(game.mediumFont, choices[i], new Vector2(position.X + 18, position.Y), Color.Gray);
				spriteBatch.Draw(point, new Rectangle(position.X, position.Y, 14, 14), null, Color.White, 0.0f, Vector2.Zero, SpriteEffects.None, 0.11f);
				position.Y += rectangles[i].Height + 2;
			}
		}

		public Choice(Game g, string[] s, string[] d, EventManager em, string t)
		{
			game = g;
			dialogueWindow = game.TextureLoader.RequestTexture("UI\\World\\ChoiceWindow");
			point = game.TextureLoader.RequestTexture("UI\\Common\\bulletpoint");
			font = game.mediumFont;
			var baseOffset = new Point((game.GetScreenWidth() / 2) - (dialogueWindow.Width / 2), game.GetScreenHeight() - dialogueWindow.Height - 20);
			text = TextMethods.WrapText(game.mediumFont, t, 530);
			choices = s;
			destinations = d;
			var position = new Point(baseOffset.X + 32, baseOffset.Y);
			if (text != "")
			{
				int height = (int)game.mediumFont.MeasureString(text).Y + 2 + 24;
				position.Y += height;
			}
			rectangles = new Rectangle[choices.Length];
			int limit = 7;
			if (text.Contains("\r\n"))
				limit = 6;
			for (int i = 0; i < choices.Length && i < limit; i++)
			{
				choices[i] = TextMethods.WrapText(game.mediumFont, choices[i], 494);
				rectangles[i] = new Rectangle(position.X, position.Y, (int)font.MeasureString(choices[i]).X + 40, (int)font.MeasureString(choices[i]).Y);
				position.Y += rectangles[i].Height + 2;
			}
			eventManager = em;
			font = game.mediumFont;
		}

		void mouseOverHighlightedChoices()
		{
			for (int i = 0; i < choices.Length; i++)
			{
				if (game.Input.IsMouseOver(rectangles[i]))
				{
					highlightedChoice = i;
					return;
				}
			}
		}

	}
}
