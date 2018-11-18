using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Xml.Linq;
using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Input;
using Microsoft.Xna.Framework.Content;
using Microsoft.Xna.Framework.Graphics;

namespace Magicians
{
	class Calendar : BaseUIWindow
	{
		Game game;

		Texture2D MainQuestWarningIcon;
		Texture2D SideQuestWarningIcon;
		Texture2D dayRimShine; //shines around the rim to highlight the current day;
		Texture2D tooltip;

		Button NextButton;
		Button PreviousButton;

		int monthID;
		string[] months;
		string[] days;
		string[] strings;

		int viewedMonth;

		public Calendar(Game g) :
        base(g.Input, g.TextureLoader.RequestTexture("UI\\World\\Calendar"), 0.18f, new Point((g.GetScreenWidth() / 2) - 300, g.GetScreenHeight() / 2 - 235), new Button(g.Audio, g.Input, g.TextureLoader.RequestTexture("UI\\Common\\BlankExitButton"), new Point((g.GetScreenWidth() / 2) - 300 + 532, g.GetScreenHeight() / 2 - 235 + 412), "", g.TextureLoader.RequestTexture("UI\\Highlights\\ExitHighlight"), 0.17f))
		{
			game = g;
			tooltip = game.TextureLoader.RequestTexture("UI\\Common\\tooltipWindow");
			MainQuestWarningIcon = game.TextureLoader.RequestTexture("UI\\World\\CalendarMQuestWarn");
			SideQuestWarningIcon = game.TextureLoader.RequestTexture("UI\\World\\CalendarSQuestWarn");
			dayRimShine = game.TextureLoader.RequestTexture("UI\\World\\CalendarActiveDay");
            NextButton = new Button(g.Audio, g.Input, game.TextureLoader.RequestTexture("UI\\Common\\BlankExitButton"), new Point(TopLeft.X + 222, TopLeft.Y + 406), 40, 40, "", game.TextureLoader.RequestTexture("UI\\Highlights\\CurvedRightEdgeHighlight"), 0.16f);
            PreviousButton = new Button(g.Audio, g.Input, game.TextureLoader.RequestTexture("UI\\Common\\BlankExitButton"), new Point(TopLeft.X + 31, TopLeft.Y + 406), 40, 40, "", game.TextureLoader.RequestTexture("UI\\Highlights\\SquareHighlight"), 0.16f);
			if (game.party.QuestStats.DayCounter > 15)
				monthID = (game.party.QuestStats.DayCounter - 1) / 15;
			else
				monthID = 0;
			viewedMonth = monthID;
			months = new string[12];
			months[0] = game.LoadString("UI", "Augas");
			months[1] = game.LoadString("UI", "Sempas");
			months[2] = game.LoadString("UI", "Vonembras");
			months[3] = game.LoadString("UI", "Ferabas");
			months[4] = game.LoadString("UI", "Aprillas");
			months[5] = game.LoadString("UI", "Fullas");
			months[6] = game.LoadString("UI", "Mondolas");
			months[7] = game.LoadString("UI", "Areyte");
			months[8] = game.LoadString("UI", "Griznas");
			months[9] = game.LoadString("UI", "Monmas");
			months[10] = game.LoadString("UI", "Veirhnon");
			months[11] = game.LoadString("UI", "Juynas");

			days = new string[5];
			days[0] = game.LoadString("UI", "Sundas");
			days[1] = game.LoadString("UI", "Mondas");
			days[2] = game.LoadString("UI", "Tudas");
			days[3] = game.LoadString("UI", "Wendas");
			days[4] = game.LoadString("UI", "Fridas");


			strings = new string[9];
			strings[0] = game.LoadString("UI", "CalendarIncantation");
			strings[1] = game.LoadString("UI", "CalendarHistory");
			strings[2] = game.LoadString("UI", "CalendarPyromancy");
			strings[3] = game.LoadString("UI", "CalendarConjuration");
			strings[4] = game.LoadString("UI", "CalendarMedicine");
			strings[5] = game.LoadString("UI", "CalendarCombat");
			strings[6] = game.LoadString("UI", "CalendarExam");
			strings[7] = game.LoadString("UI", "CalendarNoLesson");
			strings[8] = game.LoadString("UI", "CalendarQuestWarning");
		}
		public override void Update(GameTime gameTime)
		{
			NextButton.Update(gameTime);
			if (NextButton.Activated())
				viewedMonth++;
			PreviousButton.Update(gameTime);
			if (PreviousButton.Activated() && viewedMonth > 0)
				viewedMonth--;
			base.Update(gameTime);
		}
		public override void Draw(SpriteBatch spriteBatch)
		{
			NextButton.Draw(spriteBatch);
			PreviousButton.Draw(spriteBatch);
			var drawVect = new Vector2(TopLeft.X + 80, TopLeft.Y + 56);
			for (int i = 0; i < 5; i++)
			{
				spriteBatch.DrawString(game.mediumFont, days[i], drawVect, Color.Black, 0, TextMethods.CenterText(game.mediumFont, days[i]), 1, SpriteEffects.None, 0.16f);
				drawVect.X = drawVect.X + 109;
			}
			drawVect = new Vector2(TopLeft.X + 30, TopLeft.Y + 79);
			for (int i = (viewedMonth * 15) + 1; i < (viewedMonth * 15) + 16; i++)
			{
				spriteBatch.DrawString(game.mediumFont, i - (15 * viewedMonth) + ".", new Vector2(drawVect.X + 2, drawVect.Y + 2), Color.Black, 0, Vector2.Zero, 1, SpriteEffects.None, 0.16f);
				if (game.party.QuestStats.DayCounter == i)
					spriteBatch.Draw(dayRimShine, new Rectangle((int)drawVect.X, (int)drawVect.Y, 107, 102), null, Color.White, 0, Vector2.Zero, SpriteEffects.None, 0.17f);
				if (game.calendarEvents.ContainsKey(i))
				{
					if (game.calendarEvents[i].NoClass)
						spriteBatch.DrawString(game.smallFont, strings[7], new Vector2(drawVect.X + 2, drawVect.Y + 21), Color.Black, 0, Vector2.Zero, 1, SpriteEffects.None, 0.16f);
					if (game.calendarEvents[i].Exam)
						spriteBatch.DrawString(game.smallFont, strings[6], new Vector2(drawVect.X + 2, drawVect.Y + 21), Color.Black, 0, Vector2.Zero, 1, SpriteEffects.None, 0.16f);
					if (game.calendarEvents[i].Classes != null)
					{
						var classes = "~";
						if (game.calendarEvents[i].Classes.Length == 1)
							classes = classes + strings[game.calendarEvents[i].Classes[0]] + "~";
						else
							classes = classes + strings[game.calendarEvents[i].Classes[0]] + "/" + strings[game.calendarEvents[i].Classes[1]] + "~";
						spriteBatch.DrawString(game.smallFont, classes, new Vector2(drawVect.X + 2, drawVect.Y + 21), Color.Black, 0, Vector2.Zero, 1, SpriteEffects.None, 0.16f);
					}
					if (game.calendarEvents[i].ExpiredQuests != null)
					{
						var ExpQuestVector = new Point((int)drawVect.X + 2, (int)drawVect.Y + 70);
						var rect = new Rectangle(ExpQuestVector.X, ExpQuestVector.Y, 28, 28);
						for (int e = 0; e < game.calendarEvents[i].ExpiredQuests.Length; e++)
						{
							var quest = game.party.QuestStats.Quests.Find(q => q.questID == game.calendarEvents[i].ExpiredQuests[e]);
							if (quest != null)
							{
								if (rect.X < drawVect.X + 108)
								{
									if (quest.mainQuest)
										spriteBatch.Draw(MainQuestWarningIcon, rect, null, Color.White, 0, Vector2.Zero, SpriteEffects.None, 0.16f);
									else
										spriteBatch.Draw(SideQuestWarningIcon, rect, null, Color.White, 0, Vector2.Zero, SpriteEffects.None, 0.16f);
									if (Input.IsMouseOver(rect))
										DrawMethods.DrawToolTip(spriteBatch, game.Input.oldMouseState.Position.ToVector2(), game.smallFont, tooltip, strings[8] + '"' + quest.Title + '"');
									rect.X += 28;
								}
							}
						}
					}
				}
				//this goes at the end of the loop to set the next line
				drawVect.X += 108;
				if (i - (15 * viewedMonth) == 5)
				{
					drawVect = new Vector2(TopLeft.X + 30, TopLeft.Y + 184);
					continue;
				}
				if (i - (15 * viewedMonth) == 10)
					drawVect = new Vector2(TopLeft.X + 30, TopLeft.Y + 289);
			}
			drawVect = new Vector2(TopLeft.X + 146, TopLeft.Y + 428);
			var month = GetMonthStringIndex(viewedMonth);
			spriteBatch.DrawString(game.mediumFont, months[month], drawVect, Color.Black, 0, TextMethods.CenterText(game.mediumFont, months[month]), 1, SpriteEffects.None, 0.16f);
			base.Draw(spriteBatch);
		}
		public static int ReturnClassStringIndex(string arg)
		{
			switch (arg)
			{
				case "INCAN": return 0;
				case "HIST": return 1;
				case "PYRO": return 2;
				case "CONJ": return 3;
				case "MEDI": return 4;
				case "COMB": return 5;
			}
			return 0;
		}
		int GetMonthStringIndex(int i)
		{
			while (i > 11)
			{
				i -= 12;
			}
			return i;
		}
	}
	class CalendarData
	{
		public bool NoClass { get; private set; }
		public int[] Classes { get; private set; }
		public bool Exam { get; private set; }
		public string[] ExpiredQuests { get; private set; }
		public CalendarData(bool nc, int[] c, bool e, string[] exp)
		{
			NoClass = nc;
			Classes = c;
			Exam = e;
			ExpiredQuests = exp;
		}
	}
}
