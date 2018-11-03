using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Content;
using Microsoft.Xna.Framework.Graphics;
using Microsoft.Xna.Framework.Input;

namespace Magicians
{
    class DialogueManager
    {
        readonly Game game;
        public enum DiagStates { Parsing, Inactive, Null };
        public DiagStates State { get; private set; }
        public string Text { get; private set; }
        public float FastSkipTimer { get; private set; }

        TimeSpan timer = TimeSpan.Zero;

        float originDelay;
        float delay; //the delay in milliseconds between each letter
        Vector2 drawnPosition;
        int textPosition;
        StringBuilder sb = new StringBuilder();
        Walker activeSpeaker; //the active speaker

        int nextDiagFrame;
        bool skippedText;
        int steps;
        const int minSkipTime = 2;
        bool usingBigNameWindow;

        Texture2D dialogueWindow;
        Texture2D nameWindow;
        Texture2D bigNameWindow;
        Texture2D[] nextIcon = new Texture2D[2];

        public DialogueManager(Game game)
        {
            FastSkipTimer = 0;
            this.game = game;
            textPosition = 1;
            drawnPosition = new Vector2(50, 50);
            State = DiagStates.Null;
            Text = "";
        }
        public void Update(GameTime gameTime)
        {
            timer += gameTime.ElapsedGameTime;
            if (!game.Input.IsKeyPressed(game.settings.interactKey))
            {
                FastSkipTimer = 0;
            }
            else
            {
                FastSkipTimer += (float)gameTime.ElapsedGameTime.TotalSeconds;
            }
            switch (State)
            {
                case (DiagStates.Parsing):
                    {
                        steps += 1;
                        if (Text.Substring(0, textPosition).Length != Text.Length && (game.Input.IsKeyReleased(game.settings.interactKey) || game.Input.IsMouseButtonReleased() && game.debug == false || FastSkipTimer > 1f))
                        {
                            if (steps > minSkipTime)
                            {
                                textPosition = Text.Length;
                                timer = TimeSpan.Zero;
                                delay = 50;
                                Console.WriteLine("Displayed all text at tick: " + game.tick);
                                skippedText = true;
                                break;
                            }
                        }
                        if (timer.Milliseconds >= delay && Text.Substring(0, textPosition).Length != Text.Length)
                        {
                            if (activeSpeaker != null)
                            {
                                if (Text[textPosition] != ' ')
                                    game.Audio.PlaySound("DialogueTick", false);
                            }
                            if ((Text.Substring(textPosition, 1)) == "|")
                            {
                                timer = TimeSpan.Zero;
                                delay = 600;
                                textPosition++;
                                break;
                            }
                            if (Text.Substring(textPosition, 1) == "#")
                            {
                                Text = Text.Replace("#", "\n");
                            }
                            textPosition++;
                            timer = TimeSpan.Zero;
                            delay = originDelay;
                            break;
                        }
                        if (Text.Substring(0, textPosition).Length == Text.Length)
                        {
                            if (skippedText)
                            {
                                if (timer.Milliseconds >= delay)
                                {
                                    State = DiagStates.Inactive;
                                }
                                else
                                {
                                    break;
                                }
                            }
                            else
                            {
                                State = DiagStates.Inactive;
                            }
                            break;
                        }
                        break;
                    }
                case (DiagStates.Inactive):
                    {
                        if (activeSpeaker != null)
                        {
                            activeSpeaker.ChangeWalkerState(WalkerState.Standing);
                        }
                        if (timer.Milliseconds >= 150)
                        {
                            if (nextDiagFrame == 0)
                            {
                                nextDiagFrame = 1;
                            }
                            else
                            {
                                nextDiagFrame = 0;
                            }
                            timer = TimeSpan.Zero;
                        }
                        break;
                    }
            }
        }
        public void Draw(SpriteBatch spriteBatch)
        {
            var baseOffset = new Point((game.GetScreenWidth() / 2) - (dialogueWindow.Width / 2), game.GetScreenHeight() - dialogueWindow.Height - 48);
            if (State != DiagStates.Null)
            {
                var drawText = Text.Substring(0, textPosition);
                for (int i = 0; i < drawText.Length; i++)
                {
                    if (drawText[i] == '|')
                    {
                        drawText = drawText.Remove(i, 1);
                    }
                }
                spriteBatch.DrawString(game.mediumFont, drawText, new Vector2(baseOffset.X + 20, baseOffset.Y + 20), Color.White);
                spriteBatch.Draw(dialogueWindow, new Rectangle(baseOffset.X, baseOffset.Y, 800, 156), null, Color.White, 0.0f, Vector2.Zero, SpriteEffects.None, 0.12f);
                if (activeSpeaker != null)
                {
                    if (activeSpeaker.DisplayName != null)
                    {
                        if (usingBigNameWindow)
                        {
                            spriteBatch.DrawString(game.mediumFont, activeSpeaker.DisplayName, new Vector2(baseOffset.X + 184, baseOffset.Y - 16), Color.White, 0.0f, TextMethods.CenterText(game.mediumFont, activeSpeaker.DisplayName), TextMethods.ResizeText(game.mediumFont, activeSpeaker.DisplayName, 185), SpriteEffects.None, 0.10f);
                            spriteBatch.Draw(bigNameWindow, new Rectangle(baseOffset.X + 88, baseOffset.Y - 48, 200, 64), null, Color.White, 0.0f, Vector2.Zero, SpriteEffects.None, 0.11f);
                        }
                        else
                        {
                            spriteBatch.DrawString(game.mediumFont, activeSpeaker.DisplayName, new Vector2(baseOffset.X + 128, baseOffset.Y - 16), Color.White, 0.0f, TextMethods.CenterText(game.mediumFont, activeSpeaker.DisplayName), 1.0f, SpriteEffects.None, 0.10f);
                            spriteBatch.Draw(nameWindow, new Rectangle(baseOffset.X + 48, baseOffset.Y - 48, 160, 64), null, Color.White, 0.0f, Vector2.Zero, SpriteEffects.None, 0.11f);
                        }
                    }
                }
                if (this.State == DiagStates.Inactive)
                    spriteBatch.Draw(nextIcon[nextDiagFrame], new Rectangle(baseOffset.X + 754, baseOffset.Y + 113, 28, 28), null, Color.White, 0, Vector2.Zero, SpriteEffects.None, 0.10f);
            }
        }
        public void Load(Game game)
        {
            dialogueWindow = game.TextureLoader.RequestTexture("UI\\World\\DialogueWindow");
            nameWindow = game.TextureLoader.RequestTexture("UI\\MainMenu\\MainMenuButton");
            nextIcon[0] = game.TextureLoader.RequestTexture("UI\\World\\DialogueNext");
            nextIcon[1] = game.TextureLoader.RequestTexture("UI\\World\\DialogueNext2");
            bigNameWindow = game.TextureLoader.RequestTexture("UI\\Battle\\BattleTextBack");
        }
        public void Clear()
        {
            textPosition = 0;
            this.State = DiagStates.Null;
            usingBigNameWindow = false;
            Text = "";
            if (game.CurrentScene == Game.GameScenes.World)
            {
                if (activeSpeaker != null)
                {
                    activeSpeaker.ChangeWalkerState(WalkerState.Standing);
                }
            }
            activeSpeaker = null;
        }
        public void SetText(string t, Walker e)
        {
            var chara = game.party.GetPlayerCharacter(0);
            switch (game.settings.textSpeed)
            {
                case (TextSpeed.Fast):
                    {
                        originDelay = 7f;
                        break;
                    }
                case (TextSpeed.Medium):
                    {
                        originDelay = 14f;
                        break;
                    }
                case (TextSpeed.Slow):
                    {
                        originDelay = 20f;
                        break;
                    }
            }
            if (Text == "")
            {
                Text = t;
                this.State = DiagStates.Parsing;
                textPosition = 0;
            }
            int i = 0;
            //todo: replace this horrible shitcode with regex at some point
            while (i + 4 < Text.Length)
            {
                if (Text[i] == '$')
                {
                    Text = Text.Remove(i, 1);
                    if (Text[i] == 'c')
                    {
                        for (int c = i; c < Text.Length; c++)
                        {
                            if (c == ' ')
                            {
                                var counter = Text.Substring(i, c - 1);
                                if (game.gameCounters.ContainsKey(counter))
                                {
                                    Text = Text.Remove(i, c - 1);
                                    Text = Text.Insert(i, game.gameCounters[counter].ToString());
                                }
                                break;
                            }
                        }
                    }
                    if (Text.Substring(i, 2) == "up")
                    {
                        var s = game.settings.upKey.ToString();
                        Text = Text.Remove(i, 2);
                        Text = Text.Insert(i, s);
                    }
                    if (Text.Substring(i, 4) == "down")
                    {
                        var s = game.settings.downKey.ToString();
                        Text = Text.Remove(i, 4);
                        Text = Text.Insert(i, s);
                    }
                    if (Text.Substring(i, 4) == "left")
                    {
                        var s = game.settings.leftKey.ToString();
                        Text = Text.Remove(i, 4);
                        Text = Text.Insert(i, s);
                    }
                    if (Text.Substring(i, 4) == "rght")
                    {
                        var s = game.settings.rightKey.ToString();
                        Text = Text.Remove(i, 4);
                        Text = Text.Insert(i, s);
                    }
                    if (Text.Substring(i, 3) == "map")
                    {
                        var s = game.settings.upKey.ToString();
                        Text = Text.Remove(i, 3);
                        Text = Text.Insert(i, game.settings.mapKey.ToString());
                    }
                    if (Text.Substring(i, 3) == "run")
                    {
                        var s = game.settings.upKey.ToString();
                        Text = Text.Remove(i, 3);
                        Text = Text.Insert(i, game.settings.runKey.ToString());
                    }
                    if (Text.Substring(i, 4) == "stat")
                    {
                        var s = game.settings.statusKey.ToString();
                        Text = Text.Remove(i, 4);
                        Text = Text.Insert(i, s);
                    }
                    if (Text.Substring(i, 3) == "snk")
                    {
                        var s = game.settings.sneakKey.ToString();
                        Text = Text.Remove(i, 3);
                        Text = Text.Insert(i, s);
                    }
                    if (Text.Substring(i, 4) == "spll")
                    {
                        Text = Text.Remove(i, 5);
                        Text = Text.Insert(i, game.party.QuestStats.learntSpellString);
                    }
                    if (Text.Substring(i, 4) == "scre")
                    {
                        Text = Text.Remove(i, 4);
                        Text = Text.Insert(i, game.party.QuestStats.Grade.ToString());
                        i = 0;
                    }
                    if (Text.Substring(i, 4) == "plyr")
                    {
                        Text = Text.Remove(i, 4);
                        Text = Text.Insert(i, chara.Name);
                        i = 0;
                    }
                    if (Text.Substring(i, 4) == "PLYR")
                    {
                        Text = Text.Remove(i, 4);
                        Text = Text.Insert(i, chara.Name.ToUpper());
                        i = 0;
                    }
                    if (Text.Substring(i, 4).ToLower() == "prn1")
                    {
                        var caps = false;
                        if (char.IsUpper(Text[i + 1]))
                            caps = true;
                        Text = Text.Remove(i, 4);
                        switch (chara.Gender)
                        {
                            case (Gender.Male):
                                {
                                    Text = Text.Insert(i, game.LoadString("Common", "MalePronoun1"));
                                    break;
                                }
                            case (Gender.Female):
                                {
                                    Text = Text.Insert(i, game.LoadString("Common", "FemalePronoun1"));
                                    break;
                                }
                            case (Gender.Other):
                                {
                                    Text = Text.Insert(i, game.LoadString("Common", "NeutralPronoun1"));
                                    break;
                                }
                        }
                        if (caps)
                        {
                            var s = Text.Substring(i, 1);
                            Text.Remove(i, 1);
                            Text.Insert(i, s.ToUpper());
                        }
                        i = 0;
                    }
                    if (Text.Substring(i, 3) == "spl")
                    {
                        var number = int.Parse(Text.Substring(i + 3, 1));
                        Text = Text.Remove(i, 4);
                        try
                        {
                            Text = Text.Insert(i, chara.Spells[number].displayName);
                        }
                        catch
                        {
                            Text = Text.Insert(i, chara.Spells[chara.Spells.Count - 1].displayName);
                        }
                    }
                    if (Text.Substring(i, 4) == "item")
                    {
                        var word = "";
                        for (int w = i; w < Text.Length; w++)
                        {
                            if (Text[w] == ' ' || Text[w] == '!' || Text[w] == '?' || Text[w] == '.' | Text[w] == ',')
                            {
                                word = Text.Substring(i, w - i);
                                break;
                            }
                        }
                        if (word != "")
                        {
                            var itemname = "";
                            itemname = game.Items.Find(itm => itm.InternalName == word).DisplayName;
                            if (itemname != "")
                            {
                                Text = Text.Remove(i, word.Length);
                                Text = Text.Insert(i, itemname);
                            }
                        }
                    }
                    if (Text.Substring(i, 4) == "prn2")
                    {
                        bool caps = false;
                        if (char.IsUpper(Text[i + 1]))
                            caps = true;
                        Text = Text.Remove(i, 4);
                        switch (chara.Gender)
                        {
                            case (Gender.Male):
                                {
                                    Text = Text.Insert(i, game.LoadString("Common", "MalePronoun2"));
                                    break;
                                }
                            case (Gender.Female):
                                {
                                    Text = Text.Insert(i, game.LoadString("Common", "FemalePronoun2"));
                                    break;
                                }
                            case (Gender.Other):
                                {
                                    Text = Text.Insert(i, game.LoadString("Common", "NeutralPronoun2"));
                                    break;
                                }
                        }
                        if (caps)
                        {
                            var s = Text.Substring(i, 1);
                            Text.Remove(i, 1);
                            Text.Insert(i, s.ToUpper());
                        }
                        i = 0;
                    }
                    if (Text.Substring(i, 4) == "prn3")
                    {
                        var caps = false;
                        if (char.IsUpper(Text[i + 1]))
                            caps = true;
                        Text = Text.Remove(i, 4);
                        switch (chara.Gender)
                        {
                            case (Gender.Male):
                                {
                                    Text = Text.Insert(i, game.LoadString("Common", "MalePronoun3"));
                                    break;
                                }
                            case (Gender.Female):
                                {
                                    Text = Text.Insert(i, game.LoadString("Common", "FemalePronoun3"));
                                    break;
                                }
                            case (Gender.Other):
                                {
                                    Text = Text.Insert(i, game.LoadString("Common", "NeutralPronoun3"));
                                    break;
                                }
                        }
                        if (caps)
                        {
                            var s = Text.Substring(i, 1);
                            Text.Remove(i, 1);
                            Text.Insert(i, s.ToUpper());
                        }
                        i = 0;
                    }
                    try
                    {
                        if (Text.Substring(i - 2, 2) == ". ")
                        {
                            if (Char.IsLetter(Text[i - 2]))
                            {
                                if (Char.IsLower(Text[i - 2]))
                                {
                                    var letter = Char.ToUpper(Text[i - 2]);
                                    Text = Text.Remove(i - 2, 1);
                                    Text = Text.Insert(i - 2, letter.ToString());
                                }
                            }
                        }
                    }
                    catch
                    {

                    }
                    if (i < Text.Length - 2)
                    {
                        if (Text.Substring(i, 2) == "\\n")
                        {
                            Text = Text.Remove(i, 2);
                            Text = Text.Insert(i, Environment.NewLine);
                        }
                    }
                }
                i++;
            }
            if (Text.Length > 0)
            {
                if (Char.IsLower(Text[0]))
                {
                    var letter = Char.ToUpper(Text[0]);
                    Text = Text.Remove(0, 1);
                    Text = Text.Insert(0, letter.ToString());
                }
            }
            if (e != null)
            {
                activeSpeaker = e;
                if (activeSpeaker != null)
                {
                    activeSpeaker.ChangeWalkerState(WalkerState.Talking);
                }
                if (activeSpeaker.DisplayName != null)
                {

                    if (game.mediumFont.MeasureString(activeSpeaker.DisplayName).X > 150)
                    {
                        usingBigNameWindow = true;
                    }
                }
            }
            steps = 0;
            Text = TextMethods.WrapText(game.mediumFont, Text, 700);
        }
    }


}