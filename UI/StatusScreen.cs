using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;

namespace Magicians
{
    class StatusWindow : BaseUIWindow //contains character stats, items, equipment, next exp to levelup, etc
    {
		readonly Game game;
		readonly  Texture2D statusWindow;
		readonly Texture2D equipIcon; //Indicating this item is equipped
		readonly Texture2D nameWindow; //used for single target spells/regen items
		readonly Texture2D castSpellTextBack;
		readonly Party playerParty;
		readonly PlayerCharacter playerCharacter; //the playerCharacter to display the details of
		readonly Texture2D tooltip;
		readonly Texture2D activeCharacterModel;
              
		readonly SpriteFont sfont;

		readonly  string[] strings = new string[15];

		readonly Texture2D[] arcanas = new Texture2D[6];

        string getDayString()
        {
            var day = 1;
            for (int i = 1; i < game.party.QuestStats.DayCounter; i++)
            {
                day += 1;
                if (day > 5)
                {
                    day = 1;
                }
            }
            return strings[8 + day];
        }
        public override void Update(GameTime gameTime)
        {
            base.Update(gameTime);
        }
        public override void Draw(SpriteBatch spriteBatch)
        {
            spriteBatch.DrawString(game.largeFont, playerCharacter.Name, new Vector2(TopLeft.X + 30, TopLeft.Y + 18), Color.Black, 0.0f, Vector2.Zero, TextMethods.ResizeText(game.largeFont, playerCharacter.Name, 258), SpriteEffects.None, 0.19f);
            spriteBatch.DrawString(game.mediumFont, strings[0] + playerCharacter.Level.ToString(), new Vector2(TopLeft.X + 30, TopLeft.Y + 88), Color.Black, 0.0f, Vector2.Zero, 1.0f, SpriteEffects.None, 0.19f);
            if (playerCharacter.NextLevelExp != -1)
            {
                spriteBatch.DrawString(game.mediumFont, strings[1] + (playerCharacter.NextLevelExp - playerCharacter.Exp).ToString(), new Vector2(TopLeft.X + 30, TopLeft.Y + 118), Color.Black, 0.0f, Vector2.Zero, 1.0f, SpriteEffects.None, 0.19f);
                spriteBatch.DrawString(game.mediumFont, strings[2] + playerCharacter.TotalExp.ToString(), new Vector2(TopLeft.X + 30, TopLeft.Y + 148), Color.Black, 0.0f, Vector2.Zero, 1.0f, SpriteEffects.None, 0.19f);
            }
            else
                spriteBatch.DrawString(game.mediumFont, strings[2] + playerCharacter.TotalExp.ToString(), new Vector2(TopLeft.X + 30, TopLeft.Y + 118), Color.Black, 0.0f, Vector2.Zero, 1.0f, SpriteEffects.None, 0.19f);         
            spriteBatch.DrawString(game.smallFont, playerCharacter.BattleStats.HP.ToString() + " /" + playerCharacter.BattleStats.MaxHP.ToString(), new Vector2(TopLeft.X + 494, TopLeft.Y + 28), Color.Black, 0.0f, TextMethods.CenterText(sfont, playerCharacter.BattleStats.HP.ToString() + "/" + playerCharacter.BattleStats.MaxHP.ToString()), 1.0f, SpriteEffects.None, 0.19f);
            spriteBatch.DrawString(game.smallFont, playerCharacter.BattleStats.SP.ToString() + " /" + playerCharacter.BattleStats.MaxSP.ToString(), new Vector2(TopLeft.X + 494, TopLeft.Y + 56), Color.Black, 0.0f, TextMethods.CenterText(sfont, playerCharacter.BattleStats.SP.ToString() + "/" + playerCharacter.BattleStats.MaxSP.ToString()), 1.0f, SpriteEffects.None, 0.19f);
            spriteBatch.DrawString(game.smallFont, playerCharacter.BattleStats.Attributes[Attributes.Strength].ToString(), new Vector2(TopLeft.X + 494, TopLeft.Y + 82), Color.Black, 0.0f, TextMethods.CenterText(sfont, playerCharacter.BattleStats.Attributes[Attributes.Strength].ToString()), 1.0f, SpriteEffects.None, 0.19f);
            spriteBatch.DrawString(game.smallFont, playerCharacter.BattleStats.Attributes[Attributes.Magic].ToString(), new Vector2(TopLeft.X + 494, TopLeft.Y + 110), Color.Black, 0.0f, TextMethods.CenterText(sfont, playerCharacter.BattleStats.Attributes[Attributes.Magic].ToString()), 1.0f, SpriteEffects.None, 0.19f);
            spriteBatch.DrawString(game.smallFont, playerCharacter.BattleStats.Attributes[Attributes.Dexterity].ToString(), new Vector2(TopLeft.X + 494, TopLeft.Y + 136), Color.Black, 0.0f, TextMethods.CenterText(sfont, playerCharacter.BattleStats.Attributes[Attributes.Dexterity].ToString()), 1.0f, SpriteEffects.None, 0.19f);
            spriteBatch.DrawString(game.smallFont, playerCharacter.BattleStats.Armour.ToString(), new Vector2(TopLeft.X + 494, TopLeft.Y + 164), Color.Black, 0.0f, TextMethods.CenterText(sfont, playerCharacter.BattleStats.Armour.ToString()), 1.0f, SpriteEffects.None, 0.19f);
            spriteBatch.DrawString(game.smallFont, playerCharacter.BattleStats.Luck.ToString(), new Vector2(TopLeft.X + 494, TopLeft.Y + 190), Color.Black, 0.0f, TextMethods.CenterText(sfont, playerCharacter.BattleStats.Luck.ToString()), 1.0f, SpriteEffects.None, 0.19f);         
            spriteBatch.DrawString(game.mediumFont, strings[8] + game.party.QuestStats.DayCounter + ", " + getDayString(), new Vector2(TopLeft.X + 112, TopLeft.Y + 246), Color.Black, 0.0f, TextMethods.CenterText(game.mediumFont, strings[8] + game.party.QuestStats.DayCounter + ", " + getDayString()), TextMethods.ResizeText(game.mediumFont, strings[8] + game.party.QuestStats.DayCounter + ", " + getDayString(), 120), SpriteEffects.None, 0.19f);
            spriteBatch.DrawString(game.mediumFont, playerParty.Gold.ToString(), new Vector2(TopLeft.X + 268, TopLeft.Y + 246), Color.Black, 0.0f, TextMethods.CenterText(game.mediumFont, playerParty.Gold.ToString()), 1.0f, SpriteEffects.None, 0.19f);         
			var drawVector = new Point(TopLeft.X + 373, TopLeft.Y + 109);
            spriteBatch.Draw(activeCharacterModel, new Rectangle((int)drawVector.X - (activeCharacterModel.Width / 2), (int)drawVector.Y - (activeCharacterModel.Height / 2), activeCharacterModel.Width, activeCharacterModel.Height), null, Color.White, 0, Vector2.Zero, SpriteEffects.None, 0.19f);         
            spriteBatch.Draw(arcanas[(int)playerCharacter.Arcana], new Rectangle(TopLeft.X + 371, TopLeft.Y + 213, 62, 62), null, Color.White, 0.0f, Vector2.Zero, SpriteEffects.None, 0.21f);
			base.Draw(spriteBatch);
		}
        public StatusWindow(Game game, PlayerCharacter pc)
			: base(game.Input, game.TextureLoader.RequestTexture("UI\\World\\StatusScreen"), 0.2f, new Point((game.GetScreenWidth() / 2) - 245, game.GetScreenHeight() / 2 - 197),
			    new Button(game, game.Input, game.TextureLoader.RequestTexture("UI\\Common\\BlankExitButton"), new Point(((game.GetScreenWidth() / 2) - 245) + 478, ((game.GetScreenHeight() / 2) - 197) + 233), "", game.TextureLoader.RequestTexture("UI\\Highlights\\ExitHighlight"), 0.029f))
        {
            sfont = game.smallFont;
            playerParty = game.party;
            this.game = game;
            statusWindow = game.TextureLoader.RequestTexture("UI\\World\\StatusScreen");
            equipIcon = game.TextureLoader.RequestTexture("UI\\Common\\equip");
            nameWindow = game.TextureLoader.RequestTexture("UI\\World\\NameWindow");
            tooltip = game.TextureLoader.RequestTexture("UI\\Common\\tooltipWindow");

            arcanas[0] = game.TextureLoader.RequestTexture("UI\\Common\\Arcana\\Light");
            arcanas[1] = game.TextureLoader.RequestTexture("UI\\Common\\Arcana\\Fire");
            arcanas[2] = game.TextureLoader.RequestTexture("UI\\Common\\Arcana\\Nature");
            arcanas[3] = game.TextureLoader.RequestTexture("UI\\Common\\Arcana\\Shadow");
            arcanas[4] = game.TextureLoader.RequestTexture("UI\\Common\\Arcana\\Water");
            arcanas[5] = game.TextureLoader.RequestTexture("UI\\Common\\Arcana\\Wind");

            strings[0] = game.LoadString("UI", "StatScreenLevel");
            strings[1] = game.LoadString("UI", "StatScreenNextLevelExp");
            strings[2] = game.LoadString("UI", "StatScreenTotalExp");
            strings[3] = game.LoadString("UI", "StatScreenInvDesc");
            strings[4] = game.LoadString("UI", "StatScreenSpellDesc");
            strings[5] = game.LoadString("UI", "StatScreenQuestDesc");
            strings[6] = game.LoadString("UI", "StatScreenSpellCast");
            strings[7] = game.LoadString("UI", "StatScreenPartyDesc");
            strings[8] = game.LoadString("UI", "DayCounter");
            strings[10] = game.LoadString("UI", "Mondas");
            strings[11] = game.LoadString("UI", "Tudas");
            strings[12] = game.LoadString("UI", "Wendas");
            strings[13] = game.LoadString("UI", "Fridas");
            strings[14] = game.LoadString("UI", "StatScreenCalendar");
            strings[9] = game.LoadString("UI", "Sundas");
            castSpellTextBack = game.TextureLoader.RequestTexture("UI\\World\\WorldSpellTextBack");
            playerCharacter = pc;
            activeCharacterModel = game.TextureLoader.RequestTexture("Sprites\\World\\Entity\\Walker\\" + playerCharacter.GraphicsFolderName + "\\Standing\\down");
        }
    }
    class PartyWindow: BaseUIWindow //contains active/inactive party members and allows the player to swap in and swap out members and choose spells to work towards learning
    {
		readonly Party party;
		readonly Game game;
		readonly string[] strings = new string[2];
        
        public override void Update(GameTime gameTime)
        {
            base.Update(gameTime);
			var drawVector = new Point(TopLeft.X + 89,TopLeft.Y + 178);
            for (int i = 1; i < party.ActiveCharacters.Count; i++)
            {
                var rec = new Rectangle((int)drawVector.X, (int)drawVector.Y, 62, 62);
                if (game.Input.HasMouseClickedOnRectangle(rec))
                {
                    var pc = party.GetPlayerCharacter(party.ActiveCharacters[i]);
                    ((Map)game.Scene).RemoveEntity("ENT_" + pc.Name.ToUpper());
                    party.RemoveCharacterFromParty(pc.ID, false);
                    if (i != party.ActiveCharacters.Count - 1)
                    {
                        for (int e = i; e == party.ActiveCharacters.Count - 1; e++)
                        {
                            if (party.GetPlayerCharacter(party.ActiveCharacters[e + 1]) != null)
                            {
                                party.ActiveCharacters[e] = party.ActiveCharacters[e + 1];
                            }
                        }
                    }
                }
                drawVector.Y += 83;
            }
			drawVector = new Point(TopLeft.X + 374, TopLeft.Y + 95);
            for (int i = 0; i < party.InactiveCharacters.Count; i++)
            {
                 var rec = new Rectangle((int)drawVector.X, (int)drawVector.Y, 62, 62);
                 if (game.Input.HasMouseClickedOnRectangle(rec))
                 {
                     if (party.ActiveCharacters.Count < 4)
                     {
                         party.AddCharacterToParty(party.InactiveCharacters[i], false,(Map)game.Scene,false);
                     }
                 }
                drawVector.Y += 83;
                if (i == 2)
                {
					drawVector = new Point(TopLeft.X + 476,TopLeft.Y + 95);
                }
            }
        }
        public override void Draw(SpriteBatch spriteBatch)
        {
            base.Draw(spriteBatch);
            spriteBatch.DrawString(game.mediumFont, strings[0], new Vector2(TopLeft.X + 120, TopLeft.Y + 43), Color.Black, 0, TextMethods.CenterText(game.mediumFont, strings[0]), 1, SpriteEffects.None, 0.09f);
            spriteBatch.DrawString(game.mediumFont, strings[1], new Vector2(TopLeft.X + 455, TopLeft.Y + 43), Color.Black, 0, TextMethods.CenterText(game.mediumFont, strings[1]), 1, SpriteEffects.None, 0.09f);
			var drawVector = new Point(TopLeft.X + 89, TopLeft.Y + 95);
            for (int i = 0; i < party.ActiveCharacters.Count; i++)
            {
                PlayerCharacter chara = party.GetPlayerCharacter(party.ActiveCharacters[i]);
                spriteBatch.Draw(chara.uiPortrait, new Rectangle(drawVector.X, drawVector.Y, 62, 62), null, Color.White, 0, Vector2.Zero, SpriteEffects.None, 0.09f);
                drawVector.Y += 83;
            }
			drawVector = new Point(TopLeft.X + 374, TopLeft.Y + 95);
            for (int i = 0; i < party.InactiveCharacters.Count; i++)
            {
                PlayerCharacter chara = party.GetPlayerCharacter(party.InactiveCharacters[i]);
                spriteBatch.Draw(chara.uiPortrait, new Rectangle(drawVector.X, drawVector.Y, 62, 62), null, Color.White, 0, Vector2.Zero, SpriteEffects.None, 0.09f);
                drawVector.Y += 83;
                if (i == 2)
					drawVector = new Point(TopLeft.X + 476, TopLeft.Y + 95);
            }
        }
        public PartyWindow(Game game)
			: base(game.Input,game.TextureLoader.RequestTexture("UI\\World\\PartyWindow"),0.1f, new Point(game.GetScreenWidth() / 2 - 300, game.GetScreenHeight() / 2 - 235),
			       new Button(game, game.Input, game.TextureLoader.RequestTexture("UI\\Common\\BlankExitButton"), new Point(game.GetScreenWidth() / 2 - 300 + 532, game.GetScreenHeight() / 2 - 235 + 412), "", game.TextureLoader.RequestTexture("UI\\Highlights\\ExitHighlight"), 0.09f))
        {
            party = game.party;
            strings[0] = game.LoadString("UI", "PartyMenu1");
            strings[1] = game.LoadString("UI", "PartyMenu2");
            this.game = game;
            for (int i = 0; i < party.InactiveCharacters.Count; i++)
            {
                PlayerCharacter chara = party.GetPlayerCharacter(party.InactiveCharacters[i]);
                if (chara.uiPortrait == null)
                    chara.uiPortrait = game.TextureLoader.RequestTexture("UI\\SmallPortraits\\" + chara.GraphicsFolderName);
            }
        }
    }
}
