using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;
using Microsoft.Xna.Framework.Input;

namespace Magicians
{
	class WorldUI
    {
        readonly Game game;
        Map map;
        Texture2D UIGreyCircle;
        Texture2D UIBottomLeft;
        Texture2D UIUpperLeft;
        Texture2D UIRight;
        Texture2D[] timeOfDayIcons = new Texture2D[2];
        SpriteFont spriteFont;
        Button[] Buttons;
        CharacterSelector charaSelector;
        byte nextWindow; //status screen = 0, spellbook = 1, inv = 2
        Usage[] usages = new Usage[3];

        public bool display = true;

        public void Update(GameTime gameTime)
        {
            bool closed = false;
            if (display)
            {
                if (charaSelector != null)
                {
                    charaSelector.Update(gameTime);
                    for (int i = 0; i < charaSelector.Buttons.Length; i++)
                    {
                        if (charaSelector.Buttons[i].Activated())
                        {
                            switch (nextWindow)
                            {
                                case 0: game.OpenUIWindow(new StatusWindow(game, game.party.GetPlayerCharacter(game.party.ActiveCharacters[i]))); break;
                                case 1: game.OpenUIWindow(new Spellbook(game, game.TextureLoader.RequestTexture("UI\\Battle\\BattleSpellbook"), game.party.GetPlayerCharacter(game.party.ActiveCharacters[i]), usages)); break;
                                case 2: game.OpenUIWindow(new Inventory(game, game.TextureLoader.RequestTexture("UI\\World\\NameWindow"), game.party, game.party.GetPlayerCharacter(game.party.ActiveCharacters[i]), false)); break;
                            }
                            charaSelector = null;
                            break;
                        }
                    }
                    if (game.Input.IsMouseButtonReleased())
                        charaSelector = null;
                    for (int b = 3; b < Buttons.Length; b++)
                    {
                        if (Buttons[b].Activated())
                        {
                            charaSelector = null;
                            closed = true;
                        }
                    }
                }
                for (int i = 0; i < Buttons.Length; i++)
                {
                    if (i == 6 && game.gameFlags["bAccessPartyWindow"] == false)
                        continue;
                    Buttons[i].Update(gameTime);
                }
                if (!closed)
                {
                    if (Buttons[0].Activated())
                        game.OpenUIWindow(new Calendar(game));
                    if (Buttons[1].Activated())
                        game.OpenUIWindow(new MapWindow(game));
                    if (Buttons[2].Activated())
                        game.OpenUIWindow(new OptionsMenu(game));
                    if (Buttons[3].Activated())
                    {
                        if (game.party.ActiveCharacters.Count == 1)
                        {
                            game.OpenUIWindow(new StatusWindow(game, game.party.GetPlayerCharacter(game.party.ActiveCharacters[0])));
                        }
                        else
                        {
                            charaSelector = new CharacterSelector(game, new Point(Buttons[3].Position.X - 290, Buttons[3].Position.Y));
                            nextWindow = 0;
                        }
                    }
                    if (Buttons[4].Activated())
                    {
                        if (game.party.ActiveCharacters.Count == 1)
                            game.OpenUIWindow(new Spellbook(game, game.TextureLoader.RequestTexture("UI\\Battle\\BattleSpellbook"), game.party.GetPlayerCharacter(game.party.ActiveCharacters[0]), usages));
                        else
                        {
                            charaSelector = new CharacterSelector(game, new Point(Buttons[4].Position.X - 290, Buttons[4].Position.Y));
                            nextWindow = 1;
                        }
                    }
                    if (Buttons[5].Activated())
                    {
                        if (game.party.ActiveCharacters.Count == 1)
                            game.OpenUIWindow(new Inventory(game, game.TextureLoader.RequestTexture("UI\\World\\NameWindow"), game.party, game.party.GetPlayerCharacter(game.party.ActiveCharacters[0]), false));
                        else
                        {
                            charaSelector = new CharacterSelector(game, new Point(Buttons[5].Position.X - 290, Buttons[5].Position.Y));
                            nextWindow = 2;
                        }
                    }
                    if (Buttons[6].Activated() && game.gameFlags["bAccessPartyWindow"] == true)
                    {
                        charaSelector = null;
                        game.OpenUIWindow(new PartyWindow(game));
                    }
                    if (Buttons[7].Activated())
                    {
                        charaSelector = null;
                        game.OpenUIWindow(new QuestWindow(game));
                    }
                }
            }
        }
        public void Draw(SpriteBatch spriteBatch)
        {
            if (charaSelector != null)
                charaSelector.Draw(spriteBatch);
            if (display)
            {
                map = (Map)game.Scene;
                int drawX = 0;
                int drawY = 0;
                spriteBatch.Draw(UIUpperLeft, new Rectangle(drawX, drawY, 200, 200), null, Color.White, 0.0f, Vector2.Zero, SpriteEffects.None, 0.1f);

                if (!game.gameFlags["bNight"])
                    spriteBatch.Draw(timeOfDayIcons[0], new Rectangle(drawX, drawY, 70, 70), null, Color.White, 0.0f, Vector2.Zero, SpriteEffects.None, 0.09f);
                else
                {
                    spriteBatch.Draw(timeOfDayIcons[1], new Rectangle(drawX, drawY, 70, 70), null, Color.White, 0.0f, Vector2.Zero, SpriteEffects.None, 0.09f);
                }
                for (int i = 0; i < Buttons.Length; i++)
                    Buttons[i].Draw(spriteBatch);

                drawX = game.GetScreenWidth() - 70;
                drawY = 48;
                spriteBatch.Draw(UIRight, new Rectangle(drawX, drawY, 70, 460), null, Color.White, 0.0f, Vector2.Zero, SpriteEffects.None, 0.1f);
                if (!game.gameFlags["bAccessPartyWindow"])
                    spriteBatch.Draw(UIGreyCircle, new Rectangle(drawX + 4, drawY + 294, 62, 62), null, Color.White, 0f, Vector2.Zero, SpriteEffects.None, 0.05f);
                drawX = 0;
                drawY = game.GetScreenHeight() - 100;
                if (map.DisplayName != null)
                {
                    spriteBatch.Draw(UIBottomLeft, new Rectangle(drawX, drawY, 212, 100), null, Color.White, 0.0f, Vector2.Zero, SpriteEffects.None, 0.12f);
                    spriteBatch.DrawString(spriteFont, "~" + map.DisplayName + "~", new Vector2(drawX + 103, drawY + 74), Color.White, 0.0f, TextMethods.CenterText(spriteFont, "~" + map.DisplayName + "~"), 1, SpriteEffects.None, 0.11f);
                }
            }
        }
        public WorldUI(Game game)
        {
            this.game = game;
            UIBottomLeft = game.TextureLoader.RequestTexture("UI\\World\\UIBottom");
            UIUpperLeft = game.TextureLoader.RequestTexture("UI\\World\\UITop");
            UIRight = game.TextureLoader.RequestTexture("UI\\World\\UIRight");
            UIGreyCircle = game.TextureLoader.RequestTexture("UI\\World\\UIRightButtonGreyed");
            timeOfDayIcons[0] = game.TextureLoader.RequestTexture("UI\\Common\\day");
            timeOfDayIcons[1] = game.TextureLoader.RequestTexture("UI\\Common\\night");
            spriteFont = game.mediumFont;
            usages[0] = Usage.BothSame;
            usages[1] = Usage.World;
            usages[2] = Usage.BothAsynchrous;
            SetButtons();

        }
        public void SetButtons()
        {
            Buttons = new Button[8];
            Buttons[0] = new Button(game.Audio, game.Input, game.TextureLoader.RequestTexture("UI\\Common\\BlankUICircleButton"), new Point(93, 93), "", game.TextureLoader.RequestTexture("UI\\Highlights\\WorldUICircleHighlight"), 0.09f);
            Buttons[1] = new Button(game.Audio, game.Input, game.TextureLoader.RequestTexture("UI\\Common\\BlankUICircleButton"), new Point(134, 4), "", game.TextureLoader.RequestTexture("UI\\Highlights\\WorldUICircleHighlight"), 0.09f);
            Buttons[2] = new Button(game.Audio, game.Input, game.TextureLoader.RequestTexture("UI\\Common\\BlankUICircleButton"), new Point(4, 134), "", game.TextureLoader.RequestTexture("UI\\Highlights\\WorldUICircleHighlight"), 0.09f);
            int drawX = game.GetScreenWidth() - 66;
            int drawY = 48;
            Buttons[3] = new Button(game.Audio, game.Input, game.TextureLoader.RequestTexture("UI\\Common\\BlankUICircleButton"), new Point(drawX, drawY + 16), "", game.TextureLoader.RequestTexture("UI\\Highlights\\WorldUICircleHighlight"), 0.09f);
            Buttons[4] = new Button(game.Audio, game.Input, game.TextureLoader.RequestTexture("UI\\Common\\BlankUICircleButton"), new Point(drawX, drawY + 110), "", game.TextureLoader.RequestTexture("UI\\Highlights\\WorldUICircleHighlight"), 0.09f);
            Buttons[5] = new Button(game.Audio, game.Input, game.TextureLoader.RequestTexture("UI\\Common\\BlankUICircleButton"), new Point(drawX, drawY + 202), "", game.TextureLoader.RequestTexture("UI\\Highlights\\WorldUICircleHighlight"), 0.09f);
            Buttons[6] = new Button(game.Audio, game.Input, game.TextureLoader.RequestTexture("UI\\Common\\BlankUICircleButton"), new Point(drawX, drawY + 294), "", game.TextureLoader.RequestTexture("UI\\Highlights\\WorldUICircleHighlight"), 0.09f);
            Buttons[7] = new Button(game.Audio, game.Input, game.TextureLoader.RequestTexture("UI\\Common\\BlankUICircleButton"), new Point(drawX, drawY + 386), "", game.TextureLoader.RequestTexture("UI\\Highlights\\WorldUICircleHighlight"), 0.09f);

        }
    }
}
