using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Content;
using Microsoft.Xna.Framework.Graphics;

namespace Magicians
{
    class Battle : IScene
    {
        float timer;
        public List<Battler> Battlers = new List<Battler>();

        //UISTUFF
        Button[] uiButtons = new Button[5];
        Texture2D background;
        Texture2D spellbookUI;
        Texture2D inventoryUI;
        Texture2D examineWindow;

        Texture2D HealthBar;
        Texture2D ManaBar;
        Texture2D RegularStatsBar;
        Texture2D BossStatsBar;
        Texture2D ResultsWindow;
        Texture2D experienceIcon;
        Texture2D goldIcon;
        Texture2D backgroundBorder;
        Texture2D resultsBox;
        Texture2D battleTextBack;
        Texture2D battleTextBackWide;
        Point BaseOffset;

        Modifier PCdeath;
        Modifier ShadowBoost;

        Game game;
        Spellbook spellBook;
        Inventory inventory;
        int currentActionUsedMana;

        List<SpriteEffect> actionEffects = new List<SpriteEffect>();
        public SortedList<string,SpriteEffect> BattlerEffects = new SortedList<string,SpriteEffect>(); //stuff like poison indicators, etc
        SortedList<int, int> levelUps = new SortedList<int, int>();

        //Waiting denotes a state where user input is required, ie selecting an action
        public enum State { Intro, Waiting, Input, PreAnimating, Animating, ActionResult, BattleResult };
        State currentState;
        Battler activeBattler;
        Battler examinedBattler;
        bool removeItem;
        int activeItemIndex = -1; // -1 indicates no item was used up this turn
        int activePlayerIndex = -1; //-1 indicates the active battler is not a player, 
        BattleAction turnAction; //the current action for this turn
        List<Battler> targets = new List<Battler>();
        List<BattleText> battleTexts = new List<BattleText>();

        List<bool> actionSuccesses = new List<bool>();
        PlayerCharacter[] playerCharacters;
        Party party;

        Sprite battlerArrow; //an arrow bobbing up and down over the head of the active battler
        string battleText = "";
        bool displayBattleUI;
        public enum UIState { NoActionChosen,Spellbook,Items,Analyse, ChooseTarget,Results };
        UIState uiState;
        public enum BattleEnd { Won, Lost, Fled, SpecialEnd };
        public BattleEnd EndState;
        bool continueAfterLoss;
        public string ContinueTag { get; private set; }
        bool BattleOver;
        bool canUseItems = true;
        bool canFlee;
        public int FleeChance { get; private set; }
        int expBounty;
        int goldBounty;
        Item droppedItem;
        string music = "";
        public string ReturnMusic ="";

        public string[] strings = new string[10];
        string[] modStrings = new string[16];
        string[] typeStrings = new string[7];

        DialogueManager dialogManager;
        List<PlayDialogue> Dialogs = new List<PlayDialogue>();
        int activeDialog = -1;
        bool dialogsActive;
        Spell ZeroManaSpell;

        public Random Random = new Random();
      
        public void RemoveBattlerEffect(string s)
        {
            var args = s.Split('|');
            foreach (KeyValuePair<string, SpriteEffect> kvp in BattlerEffects)
            {
                if (s == kvp.Key)
                {
                    BattlerEffects.Remove(s);
                    break;
                }
            }
        }
        public void AddBattlerEffect(BattleStats target, string ModifierName, Point effectSize, string effectFile)
        {
            BattlerEffects.Add(target.battler.internalName + "|" + ModifierName, new SpriteEffect(game.TextureLoader, target.battler.Sprite.DrawnPosition, effectSize, effectFile, false));
        }
        void SortSpriteDepth()
        {
            float furthestDepth = 0.599f;
            float nearestDepth = 0.501f;
            var sprites = new List<Sprite>();
            for (int i = 0; i < Battlers.Count; i++)
            {
                if (Battlers[i].Sprite != null)
                {
                    if (Battlers[i].Sprite.IgnoreDepthSorting == false)
                    {
                        sprites.Add(Battlers[i].Sprite);
                    }
                }
            }
            sprites.Sort((y, z) => y.BottomY.CompareTo(z.BottomY));
            float depth = furthestDepth;
            foreach(Sprite sprite in sprites)
            {
                sprite.ChangeDepth(MathHelper.Clamp(depth, nearestDepth, furthestDepth));
                depth -= 0.001f;
            }
        }
        public Battle(Game game, Party p, BattleGroup battleGroup)            
        {
            this.game = game;
            dialogManager = new DialogueManager(game);
            dialogManager.Load(game);
            dialogsActive = false;
            try
            {
                this.background = game.TextureLoader.RequestTexture("BattleBackgrounds\\" + battleGroup.battleBackground);
            }
            catch
            {
                this.background = game.TextureLoader.RequestTexture("BattleBackgrounds\\battleBack");
            }
            if (battleGroup.music != null)
            {
                this.music = battleGroup.music;
            }
            else
                music = "null";
            ReturnMusic = game.ReturnMusic;
            IModifyEffect[] mods = new IModifyEffect[1];
            mods[0] = new DisableBattler();
            PCdeath = new Modifier("mod_dead","dead",-1,false,false);
            PCdeath.effects = mods.ToList();
            mods = new IModifyEffect[1];
            mods[0] = new GiveBonusSpellDamage(10);
            ShadowBoost = new Modifier("mod_shadowboost","placeholder" , -1, false, true);
            ShadowBoost.effects = mods.ToList();
            ShadowBoost.LoadIcon(game);
            ZeroManaSpell = game.Spells.Find(spl => spl.internalName == "spl_sparks");
            party = p;
            BaseOffset = new Point((game.GetScreenWidth() / 2) - 400, (game.GetScreenHeight() / 2) - 300);
            ////Create the player battlers
            var  partyLength = MathHelper.Clamp(party.ActiveCharacters.Count, 1, battleGroup.MaxPlayers);
            playerCharacters = new PlayerCharacter[partyLength];
            Point[] Positions = new Point[partyLength];
            switch (Positions.Length)
            {
                case 1: Positions[0] = new Point(BaseOffset.X + 128, BaseOffset.Y + 300); break;
                case 2: Positions[0] = new Point(BaseOffset.X + 128, BaseOffset.Y + 200);
                    Positions[1] = new Point(BaseOffset.X + 128, BaseOffset.Y + 400); break;
                case 3: Positions[0] = new Point(BaseOffset.X + 128, BaseOffset.Y + 220);
                    Positions[1] = new Point(BaseOffset.X + 128, BaseOffset.Y + 360);
                    Positions[2] = new Point(BaseOffset.X + 128, BaseOffset.Y + 500); break;
                case 4: Positions[0] = new Point(BaseOffset.X + 128, BaseOffset.Y + 160);
                    Positions[1] = new Point(BaseOffset.X + 128, BaseOffset.Y + 278);
                    Positions[2] = new Point(BaseOffset.X + 128, BaseOffset.Y + 396);
                    Positions[3] = new Point(BaseOffset.X + 128, BaseOffset.Y + 514); break;
            }
            int b = 0;           
            for (int i = 0; i < partyLength; i++)
            {
                var pc = p.GetPlayerCharacter(p.ActiveCharacters[i]);
                playerCharacters[i] = pc;
                Battlers.Add(new Battler(pc, pc.Name, pc.GraphicsFolderName, pc.BattleStats));
                Battlers[b].BattleStats.battler = Battlers[b];
                Battlers[b].SetPosition(game, Positions[i]);
                Battlers[b].SetTeam(Team.Player);        
                b++;
                pc.uiPortrait = game.TextureLoader.RequestTexture("UI\\SmallPortraits\\" + pc.GraphicsFolderName);
            }
            int pos = 0;
            foreach (string battler in battleGroup.battlers)
            {
                var bat = new Battler(game,battler);
                Battlers.Add(bat);
                Battlers[b].SetPosition(game, new Point(BaseOffset.X + battleGroup.Positions[pos].X, BaseOffset.Y + battleGroup.Positions[pos].Y));
                Battlers[b].SetTeam(Team.Enemy);
                b++;
                pos++;
            }
            b = 0;
            foreach (Battler battler in Battlers)
            {
                battler.turnOver = false;
                battler.BattleStats.RecalculateStats();
            }
            //write new method to get fastest battler
            activeBattler = BattleMethods.GetFastestBattler(Battlers);
            if (activeBattler.BattleStats.team != Team.Player)
            {
                activePlayerIndex = -1;
            }
            else
            {
                for (int i = 0; i < playerCharacters.Length; i++)
                {
                    if (activeBattler == Battlers[i])
                    {
                        activePlayerIndex = i;
                    }
                }
            }
            uiButtons[0] = new Button(game,game.Input,game.TextureLoader.RequestTexture("UI\\Battle\\BattleSpellIcon"), new Point(16, 20),"",game.TextureLoader.RequestTexture("UI\\Highlights\\BattleHighlight"),0.1f);
            if (canUseItems)
				uiButtons[1] = new Button(game, game.Input, game.TextureLoader.RequestTexture("UI\\Battle\\BattleItemIcon"), new Point(156, 20), "", game.TextureLoader.RequestTexture("UI\\Highlights\\BattleHighlight"), 0.1f);
            else
				uiButtons[1] = new Button(game, game.Input, game.TextureLoader.RequestTexture("UI\\Battle\\BattleNoItemsIcon"), new Point(156, 20), "", game.TextureLoader.RequestTexture("UI\\Highlights\\BattleHighlight"), 0.1f);
			uiButtons[2] = new Button(game, game.Input, game.TextureLoader.RequestTexture("UI\\Battle\\BattleExamineIcon"), new Point(296, 20), "", game.TextureLoader.RequestTexture("UI\\Highlights\\BattleHighlight"), 0.1f);
			uiButtons[3] = new Button(game, game.Input, game.TextureLoader.RequestTexture("UI\\Battle\\BattleWaitIcon"), new Point(436, 20), "", game.TextureLoader.RequestTexture("UI\\Highlights\\BattleHighlight"), 0.1f);
            if (battleGroup.encounterType == EncounterType.Boss)
            {
				uiButtons[4] = new Button(game, game.Input, game.TextureLoader.RequestTexture("UI\\Battle\\BattleNoFleeIcon"), new Point(576, 20), "", game.TextureLoader.RequestTexture("UI\\Highlights\\BattleHighlight"), 0.1f);
            }
            else
            {
				uiButtons[4] = new Button(game, game.Input, game.TextureLoader.RequestTexture("UI\\Battle\\BattleFleeIcon"), new Point(576, 20), "", game.TextureLoader.RequestTexture("UI\\Highlights\\BattleHighlight"), 0.1f);
            }
            spellbookUI = game.TextureLoader.RequestTexture("UI\\Battle\\BattleSpellbook");
            inventoryUI = game.TextureLoader.RequestTexture("UI\\Battle\\BattleInventory");
            examineWindow = game.TextureLoader.RequestTexture("UI\\Battle\\BattleExamineWindow");
            ResultsWindow = game.TextureLoader.RequestTexture("UI\\Battle\\BattleResultsWindow");
            HealthBar = game.TextureLoader.RequestTexture("UI\\Battle\\Health");
            ManaBar = game.TextureLoader.RequestTexture("UI\\Battle\\Mana");
            RegularStatsBar = game.TextureLoader.RequestTexture("UI\\Battle\\RegularHealthBar");
            BossStatsBar = game.TextureLoader.RequestTexture("UI\\Battle\\BossHealthBar");
            experienceIcon = game.TextureLoader.RequestTexture("UI\\Common\\experience");
            goldIcon = game.TextureLoader.RequestTexture("UI\\Common\\money");
            backgroundBorder = game.TextureLoader.RequestTexture("UI\\Battle\\Border");
            resultsBox = game.TextureLoader.RequestTexture("UI\\Battle\\BattleResultsBox");
            battleTextBack = game.TextureLoader.RequestTexture("UI\\Battle\\BattleTextBack");
            battleTextBackWide = game.TextureLoader.RequestTexture("UI\\Battle\\BattleTextBackWide");
            battlerArrow = new Sprite(game.TextureLoader, "UI\\Battle\\ActiveBattlerArrow", new Point(-80,-80), 0.9f, new Vector2(20, 50).ToPoint(), Sprite.OriginType.TopLeft);
            battlerArrow.SetInterval(20);
            battlerArrow.ChangeDepth(0.31f);
            if (battleGroup.encounterType == EncounterType.Boss)
                FleeChance = -1;
            currentState = State.Intro;
            uiState = UIState.NoActionChosen;
            strings[0] = game.LoadString("Battle", "Victory");
            strings[1] = game.LoadString("Battle", "Continue");
            strings[2] = game.LoadString("Battle", "LearnSpell");
            strings[3] = game.LoadString("Battle", "SelectTarget");
            strings[4] = game.LoadString("Battle", "ExamineTarget");
            strings[5] = game.LoadString("Battle", "Dodged");
            strings[6] = game.LoadString("Battle", "Wait");
            for (int i = 0; i < playerCharacters.Length; i++)
            {
                if (playerCharacters[i].Arcana == Arcana.Shadow && game.gameFlags["bNight"] == true)
                {
                    Battlers[i].BattleStats.Modifiers.Add(ShadowBoost);
                }
            }
            Console.WriteLine("BATTLE: NEW BATTLE HAS BEGUN, BATTLERS ARE:\r\n");
            for (int i = 0; i < Battlers.Count; i++)
            {
                Console.WriteLine(Battlers[i].Name);
            }
            modStrings[0] = game.LoadString("Battle", "ModDisable");
            modStrings[1] = game.LoadString("Battle", "ModArmour");
            modStrings[2] = game.LoadString("Battle", "ModMana");
            modStrings[3] = game.LoadString("Battle", "ModHealth");
            modStrings[4] = game.LoadString("Battle", "ModStrength");
            modStrings[5] = game.LoadString("Battle", "ModMagic");
            modStrings[6] = game.LoadString("Battle", "ModSpeed");
            modStrings[7] = game.LoadString("Battle", "ModSpellDamage");
            modStrings[8] = game.LoadString("Battle", "ModRestoreHealth");
            modStrings[9] = game.LoadString("Battle", "ModRemoveOnDamage");
            modStrings[10] = game.LoadString("Battle", "ModResistance");
            modStrings[11] = game.LoadString("Battle", "ModMissChance");
            modStrings[12] = game.LoadString("Battle", "ModDodgeChance");
            modStrings[13] = game.LoadString("Battle", "ModTarget");
            modStrings[14] = game.LoadString("Battle", "ModDamagePerTurn");
            modStrings[15] = game.LoadString("Battle", "ModLuck");
            typeStrings[0] = game.LoadString("Battle", "Phys");
            typeStrings[1] = game.LoadString("Battle", "Fire");
            typeStrings[2] = game.LoadString("Battle", "Cold");
            typeStrings[3] = game.LoadString("Battle", "Elec");
            typeStrings[4] = game.LoadString("Battle", "Light");
            typeStrings[5] = game.LoadString("Battle", "Poison");
            typeStrings[6] = game.LoadString("Battle", "Raw");
            canUseItems = battleGroup.CanUseItems;
            if(battleGroup.ContinueAfterDefeat)
            {
                continueAfterLoss = true;
                ContinueTag = battleGroup.ContinueTag;
            }
        }
        public void Load(ContentManager content, TextureLoader TextureLoader)
        {
			//TODO: move some of the stuff in the constructor to this method where appropriate
        }
        public void SetTargets(List<Battler> targets)
        {
            this.targets = targets;
        }
        public void Update(GameTime gameTime)
        {
            if(activeBattler != null)
            {
                battlerArrow.Update(gameTime);
                battlerArrow.ChangeDrawnPosition(new Point(activeBattler.Sprite.DrawnPosition.X, activeBattler.Sprite.DrawnPosition.Y - (activeBattler.Sprite.SpriteSize.Y / 2) - 58));
            }
            if (!game.settings.mutedMusic)
            {
                if (game.Audio.currentMusic != music && currentState != State.BattleResult)
                {
                    if (game.Audio.music.IsFinished())
                    {
                        game.Audio.SetMusic(music);
                    }
                }
            }
            switch (currentState)
            {
                case (State.Intro):
                    {
                        if (timer >= 1)
                        {
                            timer = 0;
                            currentState = State.Waiting;
                        }
                        break;
                    }
                case (State.Waiting):
                    {
                        if (!BattleOver)
                        {
                            if (activeBattler.BattleStats.team == Team.Player)
                            {
                                getPlayerInput(gameTime);
                            }
                            if (activeBattler.BattleStats.team == Team.Enemy || activeBattler.BattleStats.team == Team.Neutral)
                            {
                                //Get an enemy target battle action, then move to active   
                                displayBattleUI = false;
                                while (turnAction == null && targets.Count == 0)
                                {
                                    turnAction = BattleMethods.GetEnemyBattleAction(this, activeBattler, Battlers.ToList());
                                    activeBattler.lastAction = turnAction;
                                    if (targets.Count == 0)
                                        targets = new List<Battler>(BattleMethods.GetTargets(activeBattler, turnAction, this));
                                    if (targets.Count == 0)
                                        turnAction = GetWaitAction();


                                }
                                this.currentState = State.PreAnimating;
                                activeBattler.BattleStats.SP -= turnAction.ManaCost;
                                AddActionVisualEffectsToList();
                                activeBattler.ChangeAnimation(turnAction.animType);
                                timer = 0;
                                break;
                            }
                        }
                        break;
                    }
                case (State.PreAnimating):
                    {
                        battleText = turnAction.IntText;
                        if (activeBattler.Sprite.ReachedEnd)
                        {
                            this.currentState = State.Animating;
                            AddActionVisualEffectsToList();
                        }
                        break;
                    }
                case (State.Animating):
                    { //display the animation of whatever action was taken
                        //then, move to result once all states are over
                        if (actionEffects.Count == 0 && timer > 1)
                        {
                            actionSuccesses = new List<bool>();
                            for (int i = 0; i < targets.Count; i++)
                            {
                                for (int n = 0; n < turnAction.actionEffects.Count; n++)
                                {
                                    turnAction.actionEffects[n].DoAction(this, activeBattler.BattleStats, targets[i].BattleStats);
                                    battleTexts.Add(turnAction.actionEffects[n].ProduceBattleText(this, targets[i]));
                                }
                                actionSuccesses.Add(turnAction.WasSuccessful(targets[i]));
                            }
                            this.currentState = State.ActionResult;
                            AddActionVisualEffectsToList();
                            battleText = "";
                            for (int i = 0; i < turnAction.actionEffects.Count; i++)
                            {
                                for (int x = 0; x < targets.Count; x++)
                                {
                                    if (turnAction.actionEffects[i] is DoDamage)
                                    {
                                        if (!((DoDamage)turnAction.actionEffects[i]).dodged)
                                        {
                                            if (targets[x].BattleStats.HP <= 0 && targets[x].deathAction == null)
                                            {
                                                targets[x].ChangeAnimation(BattlerAnimType.Die);
                                                targets[x].PlaySound(Random, game.Audio, BattlerAnimType.Die);
                                            }
                                            else
                                            {
                                                targets[x].ChangeAnimation(BattlerAnimType.Recoil);
                                                targets[x].PlaySound(Random, game.Audio, BattlerAnimType.Recoil);
                                            }
                                        }
                                    }
                                }
                            }
                            timer = 0;
                        }
                        break;
                    }
                case (State.ActionResult):
                    {
                        if (timer >= 1.25f)
                        {
                            if(activeBattler.twoTurns)
                            {
                                if (!activeBattler.lastTurn)
                                    activeBattler.lastTurn = true;
                                else
                                    activeBattler.turnOver = true;
                            }
                            else
                                activeBattler.turnOver = true;
                            bool setDeathAction = false;
                            Battler deathBattler = null;
                            BattleAction deathAction = null;
                            for (int i = 0; i < Battlers.Count; i++)
                            {
                                if (Battlers[i].BattleStats.HP <= 0)
                                {
                                    switch (Battlers[i].BattleStats.team)
                                    {
                                        case (Team.Enemy):
                                            {
                                                if(Battlers[i].deathAction != null)
                                                {
                                                    setDeathAction = true;
                                                    deathBattler = Battlers[i];
                                                    deathAction = Battlers[i].deathAction;
                                                    activeBattler.deathAction = null;                                                 
                                                    continue;
                                                }
                                                expBounty += Battlers[i].exp;
                                                goldBounty += Battlers[i].gold;
                                                var rand = game.randomNumber.Next(0, 100);
                                                if (Battlers[i].itemDrop != "")
                                                {
                                                    if ((100 - rand) < Battlers[i].dropchance)
                                                    {
                                                        droppedItem = game.Items.Find(delegate (Item itm) { return itm.InternalName == Battlers[i].itemDrop; });
                                                        if (droppedItem == null)
                                                        {
                                                            Console.WriteLine("PANIC: DROPPED ITEM NOT FOUND:" + Battlers[i].itemDrop);
                                                        }
                                                    }
                                                }
                                                Battlers.RemoveAt(i);
                                                i--;
                                                break;
                                            }
                                        case (Team.Player):
                                            {
                                                if (!Battlers[i].BattleStats.Modifiers.Contains(PCdeath))
                                                {
                                                    if(Battlers[i].BattleStats.Modifiers.Count > 1)
                                                    {
                                                        Battlers[i].BattleStats.Modifiers.RemoveRange(1, Battlers[i].BattleStats.Modifiers.Count - 1);
                                                    }
                                                    Battlers[i].BattleStats.HP = 0;
                                                    Battlers[i].BattleStats.SP = (int)(Battlers[i].BattleStats.SP * 0.6f);
                                                    Battlers[i].turnOver = true;
                                                    Battlers[i].ChangeAnimation(BattlerAnimType.Die);
                                                    Battlers[i].BattleStats.Modifiers.Add(PCdeath);
                                                    Battlers[i].BattleStats.RecalculateStats();
                                                }
                                                continue;
                                            }
                                    }
                                }
                            }
                            battleTexts.Clear();
                            CheckTurn:
                            CheckBattleEndConditions();
                            if (BattleOver)
                                goto End;
                            if (BattleMethods.HaveAllBattlersTakenTurn(Battlers))
                            {
                                foreach (Battler battler in Battlers)
                                {
                                    if (battler.BattleStats.HP > 0)
                                        battler.turnOver = false;
                                    if (battler.twoTurns)
                                        battler.lastTurn = false;
                                }
                                GetFleeChance();
                            }
                            targets.Clear();
                            turnAction = null;
                            if (!setDeathAction)
                                activeBattler = BattleMethods.GetFastestBattler(Battlers);
                            else
                            {
                                turnAction = deathAction;
                                activeBattler = deathBattler;
                                targets = BattleMethods.GetTargets(activeBattler, turnAction, this);
                                deathAction = null;
                                deathBattler = null;
                            }
                            if (activeBattler == null)
                            {
                                goto CheckTurn;
                            }
                            if (activeBattler.BattleStats.team != Team.Player)
                            {
                                activePlayerIndex = -1;
                            }
                            else
                            {
                                for (int i = 0; i < playerCharacters.Length; i++)
                                {
                                    if (activeBattler == Battlers[i])
                                    {
                                        activePlayerIndex = i;
                                    }
                                }
                            }
                            if (activeBattler.BattleStats.team == Team.Player)
                            {
                                uiState = UIState.NoActionChosen;
                            }
                            End:;
                            if (!BattleOver)
                            {
                                currentState = State.Waiting;
                            }
                            else
                            {
                                currentState = State.BattleResult;
                                if (EndState != BattleEnd.Won)
                                    game.EndBattle(EndState);
                                timer = 0;
                            }
                        }
                        break;
                    }
                case (State.BattleResult):
                    {
                        battlerArrow.ChangeDrawnPosition(new Point(-200, -200));
                        if (Dialogs.Count > 0 && !dialogsActive)
                        {
                            dialogsActive = true;
                            if (activeDialog == -1)
                            {
                                activeDialog = 0;
                            }
                            dialogManager.SetText(Dialogs[activeDialog].text, null);
                            break;
                        }
                        if (dialogsActive)
                        {
                            dialogManager.Update(gameTime);
                        }
                        if (EndState == BattleEnd.Won && !game.inTransition)
                        {
                            displayBattleUI = true;
                            uiState = UIState.Results;
                            if (game.Input.IsMouseButtonReleased() || game.Input.oldKeyboardState.GetPressedKeys().Length > 0 || timer > 8)
                            {
                                if (Dialogs.Count > 0)
                                {
                                    if (dialogManager.State == DialogueManager.DiagStates.Inactive)
                                    {
                                        Dialogs.RemoveAt(0);
                                        if (Dialogs.Count > 0)
                                        {
                                            dialogManager.Clear();
                                            dialogManager.SetText(Dialogs[activeDialog].text, null);
                                        }
                                        else
                                        {
                                            dialogManager.Clear();
                                            game.EndBattle(this.EndState);
                                            if (game.Audio.music != null)
                                            {
                                                game.Audio.music.Stop();
                                                game.Audio.SetMusic(ReturnMusic);
                                                if (game.staticbattler == "")
                                                    game.enemySpawnManager.FlagSpawn(game.TemporaryMap.Filename, game.BattleGroupNumber);
                                                else
                                                    game.enemySpawnManager.UpdatetaticEnemySpawn(game.TemporaryMap.Filename, game.staticbattler);
                                                game.staticbattler = "";
                                            }
                                            return;
                                        }
                                    }
                                    break;
                                }
                                game.EndBattle(this.EndState);
                                if (game.Audio.music != null)
                                {
                                    game.Audio.music.Stop();
                                    game.Audio.SetMusic(ReturnMusic);
                                    if (game.staticbattler == "")
                                        game.enemySpawnManager.FlagSpawn(game.TemporaryMap.Filename, game.BattleGroupNumber);
                                    else
                                        game.enemySpawnManager.UpdatetaticEnemySpawn(game.TemporaryMap.Filename, game.staticbattler);
                                    game.staticbattler = "";
                                }
                                return;
                            }
                        }
                        if (EndState == BattleEnd.Fled)
                        {

                        }
                        break;
                    }
            }
            foreach (Battler battler in Battlers)
            {
                for (int e = 0; e < game.settings.combatSpeed; e++)
                    battler.Update(gameTime);
            }
            for (int i = 0; i < battleTexts.Count; i++)
            {
                battleTexts[i].Update(gameTime);
            }
            for (int i = 0; i < actionEffects.Count; i++)
            {
                for (int e = 0; e < game.settings.combatSpeed; e++)
                {
                    actionEffects[i].Update(gameTime);
                }
                if (actionEffects[i].isFinished)
                {
                    actionEffects.RemoveAt(i);
                    if (currentState == State.Animating && actionEffects.Count == 0)
                    {
                        timer = 1;
                        break;
                    }
                    i--;
                }
            }
            for (int e = 0; e < game.settings.combatSpeed; e++)
                timer += (float)gameTime.ElapsedGameTime.TotalSeconds;
        }
        public Color GetBattlerDrawColor(Battler battler)
        {
            if (displayBattleUI == true)
            {
                if (turnAction != null)
                {
                    if (turnAction.targetType == BattleAction.TargetType.Single)
                    {
                        if (turnAction.bDirection == BattleAction.Direction.Enemy && battler.BattleStats.team == Team.Player)
                        {
                            return Color.DarkGray;
                        }
                        if (turnAction.bDirection == BattleAction.Direction.Self && battler.BattleStats.team == Team.Enemy)
                            return Color.DarkGray;
                        if(turnAction.bDirection ==  BattleAction.Direction.Self && battler.BattleStats.team == Team.Player)
                        if (battler.BattleStats.HP < 1)
                        {
                            for (int i = 0; i < turnAction.actionEffects.Count; i++)
                            {
                                if (turnAction.actionEffects[i] is ResurrectBattler)
                                {
                                    return Color.White;
                                }
                            }
                            return Color.DarkGray;
                        }
                    }
                }
            }
            return Color.White;
        }
        string GetStringModifier(object obj)
        {
            try { }
            catch { return ""; }
            string val = "";
            string val2 = "";
            float f = 0;
            bool firstVal = true;
            bool secondVal = false;
            if (obj is DisableBattler)
            {
                val = modStrings[0];
            }
            if(obj is AlterArmour)
            {
                val = modStrings[1];
                f = ((AlterArmour)obj).value;
            }
            if(obj is GiveBonusMana)
            {
                val = modStrings[2];
                f = ((GiveBonusMana)obj).value;
            }
            if (obj is GiveBonusHealth)
            {
                val = modStrings[3];
                f = ((GiveBonusHealth)obj).value;
            }
            if(obj is AlterStat)
            {
                Attributes attr = ((AlterStat)obj).stat;
                f = ((AlterStat)obj).value;
                if(attr == Attributes.Strength)
                    val = modStrings[4];
                if (attr == Attributes.Magic)
                    val = modStrings[5];
                if (attr == Attributes.Dexterity)
                    val = modStrings[6];
            }
            if(obj is GiveBonusSpellDamage)
            {
                val = modStrings[7];
                f = ((GiveBonusSpellDamage)obj).value;
            }
            if (obj is RestoreHealthPerTurn)
            {
                val = modStrings[8];
                f = ((RestoreHealthPerTurn)obj).restoreValue;
            }
            if (obj is RemoveModifierOnDamage)
            {
                val = modStrings[9];
            }
            if(obj is AlterResistance)
            {
                val = modStrings[10];
                f = ((AlterResistance)obj).value;
                val2 = typeStrings[(int)((AlterResistance)obj).type];
                secondVal = true;
            }
            if(obj is AlterDodgeChance)
            {
                val = modStrings[12];
                f = ((AlterDodgeChance)obj).value;
            }
            if(obj is AlterMissChance)
            {
                val = modStrings[11];
                f = ((AlterMissChance)obj).value;
            }
            if(obj is ChangeTargetStatus)
            {
                val = modStrings[13];
            }
            if (obj is DamagePerTurn)
            {
                f = (int)((DamagePerTurn)obj).Damage;
                secondVal = true;
                val2 = typeStrings[(int)((DamagePerTurn)obj).damageType];
                val = modStrings[14];
            }
            if(obj is AlterLuck)
            {
                val = modStrings[15];
                f = ((AlterLuck)obj).value;
            }
            for (int i = 0; i < val.Length; i++)
            {
                if(val[i] == '$')
                {
                    if(firstVal)
                    {
                        if (val.Substring(i, 4) == "$val")
                        {
                            val = val.Remove(i, 4);
                            val = val.Insert(i, f.ToString());
                            firstVal = false;
                            if (!secondVal)
                                break;
                        }
                    }
                    if(secondVal)
                    {
                        if (val.Substring(i, 5) == "$val2")
                        {
                            val = val.Remove(i, 5);
                            val = val.Insert(i, val2);
                            secondVal = false;
                            if (!firstVal)
                                break;
                        }
                    }
                }
            }
            return val;
        }
        public void Draw(SpriteBatch spriteBatch)
        {
            spriteBatch.Draw(background, new Rectangle(BaseOffset.X, BaseOffset.Y, 800, 600), null, Color.White, 0.0f, (new Vector2(0, 0)), SpriteEffects.None, 1.0f);
            spriteBatch.Draw(backgroundBorder, new Rectangle(BaseOffset.X - 4, BaseOffset.Y - 4, 808, 608), null, Color.White, 0.0f, Vector2.Zero, SpriteEffects.None, 1.0f);
            //todo: add four black boxes around the edge of the border that hide missed shots and overlapping sprites (should also adjust border depth as well)
            battlerArrow.Draw(spriteBatch);
            if (dialogManager.State != DialogueManager.DiagStates.Null)
                dialogManager.Draw(spriteBatch);
            foreach (SpriteEffect effect in actionEffects)
            {
                effect.Draw(spriteBatch);
            }
            foreach (KeyValuePair<string, SpriteEffect> kvp in BattlerEffects)
            {
                kvp.Value.Draw(spriteBatch);
            }
            if (battleText != "")
            {
                spriteBatch.Draw(battleTextBack, new Rectangle(BaseOffset.X + 300, BaseOffset.Y + 388, 200, 64), null, Color.White, 0, Vector2.Zero, SpriteEffects.None, 0.32f);
                spriteBatch.DrawString(game.mediumFont, battleText, new Vector2(BaseOffset.X + 400, BaseOffset.Y + 420), Color.White, 0.0f, TextMethods.CenterText(game.mediumFont, battleText), 1, SpriteEffects.None, 0.31f);
            }
            foreach (Battler battler in Battlers)
            {
                if (displayBattleUI)
                    battler.Draw(spriteBatch, GetBattlerDrawColor(battler));
                else
                    battler.Draw(spriteBatch, Color.White);
                if (battler.useBossHealthBar && battler.BattleStats.canTarget)
                {
                    var point = new Point(battler.Sprite.DrawnPosition.X - 106, battler.Sprite.DrawnPosition.Y + (battler.Sprite.SpriteSize.Y / 2) + 8);
                    spriteBatch.Draw(BossStatsBar, new Rectangle(point.X, point.Y, 212, 8), null, Color.White, 0.0f, Vector2.Zero, SpriteEffects.None, 0.4f);
                    spriteBatch.Draw(HealthBar, new Rectangle(point.X + 3, point.Y + 3, (int)(((float)battler.BattleStats.HP / (float)battler.BattleStats.MaxHP) * 206), 3), null, Color.White, 0.0f, Vector2.Zero, SpriteEffects.None, 0.399f);

                }
                if (!battler.useBossHealthBar && (battler.BattleStats.canTarget || !battler.BattleStats.canTarget && battler.BattleStats.team == Team.Player))
                {
                    var point = new Point(battler.Sprite.DrawnPosition.X - 56, battler.Sprite.DrawnPosition.Y + (battler.Sprite.SpriteSize.Y / 2) + 8);
                    spriteBatch.Draw(RegularStatsBar, new Rectangle(point.X, point.Y, 112, 10), null, Color.White, 0.0f, Vector2.Zero, SpriteEffects.None, 0.4f);
                    spriteBatch.Draw(HealthBar, new Rectangle(point.X + 3, point.Y + 3, (int)(((float)battler.BattleStats.HP / (float)battler.BattleStats.MaxHP) * 106), 2), null, Color.White, 0.0f, Vector2.Zero, SpriteEffects.None, 0.39f);
                    spriteBatch.Draw(ManaBar, new Rectangle(point.X + 3, point.Y + 6, (int)(((float)battler.BattleStats.SP / (float)battler.BattleStats.MaxSP) * 106), 1), null, Color.White, 0.0f, Vector2.Zero, SpriteEffects.None, 0.39f);
                }
            }         
            switch (currentState)
            {
                case (State.ActionResult):
                    {
                        for (int i = 0; i < battleTexts.Count; i++)
                        {
                            spriteBatch.DrawString(game.mediumFont, battleTexts[i].s, new Vector2((float)Math.Round(battleTexts[i].Position.X, MidpointRounding.AwayFromZero), (float)Math.Round(battleTexts[i].Position.Y, MidpointRounding.AwayFromZero)), Color.FromNonPremultiplied(battleTexts[i].r, battleTexts[i].g, battleTexts[i].b, 255), 0.0f, Vector2.Zero, 1.0f, SpriteEffects.None, 0.0f);
                        }
                        break;
                    }
            }
            if (displayBattleUI == true)
            {
                if (currentState != State.BattleResult)
                {
                    if (playerCharacters[activePlayerIndex].Equips[0] == null)
                    {
                        spriteBatch.Draw(uiButtons[0].Icon, uiButtons[0].Bounds, Color.DarkGray);
                    }
                    else
                    {
                        uiButtons[0].Draw(spriteBatch);
                    }
                    for (int i = 1; i < 5; i++)
                    {
                        uiButtons[i].Draw(spriteBatch);
                    }
                    if (!canFlee)
                    {
                        spriteBatch.Draw(uiButtons[4].Icon, uiButtons[4].Bounds, null, Color.DarkGray, 0, Vector2.Zero, SpriteEffects.None, 0.001f);
                    }
                    if (canFlee)
                    {
                        uiButtons[4].Draw(spriteBatch);
                    }
                }
                switch (uiState)
                {
                    case (UIState.NoActionChosen):
                        {
                            break;
                        }
                    case (UIState.ChooseTarget):
                        {
                            spriteBatch.Draw(battleTextBackWide, new Rectangle(BaseOffset.X + 250, BaseOffset.Y + 500, 300, 64), null, Color.White, 0, Vector2.Zero, SpriteEffects.None, 0.32f);
                            spriteBatch.DrawString(game.mediumFont, strings[3] + turnAction.IntText, new Vector2(BaseOffset.X + 400, BaseOffset.Y + 532), Color.White, 0.0f, TextMethods.CenterText(game.mediumFont, strings[3] + turnAction.IntText), TextMethods.ResizeText(game.mediumFont, strings[3] + turnAction.IntText, 294), SpriteEffects.None, 0.31f);
                            break;
                        }
                    case (UIState.Spellbook):
                        {
                            spellBook.Draw(spriteBatch);
                            break;
                        }
                    case (UIState.Items):
                        {
                            inventory.Draw(spriteBatch);
                            break;
                        }
                    case (UIState.Analyse):
                        {
                            if (examinedBattler != null)
                            {
                                var baseOffset = new Point(BaseOffset.X + 400 - (examineWindow.Width / 2), BaseOffset.Y + 300 - (examineWindow.Height / 2));
                                var mfont = game.mediumFont;
                                var sfont = game.smallFont;
                                spriteBatch.Draw(examineWindow, new Rectangle(baseOffset.X, baseOffset.Y, 338, 438), null, Color.White, 0.0f, Vector2.Zero, SpriteEffects.None, 0.1f);

                                spriteBatch.DrawString(game.mediumFont, examinedBattler.Name, new Vector2(baseOffset.X + 168, baseOffset.Y + 38), Color.Black, 0.0f, TextMethods.CenterText(mfont, examinedBattler.Name), 1.0f, SpriteEffects.None, 0.09f);
                                spriteBatch.DrawString(game.smallFont, examinedBattler.BattleStats.HP.ToString() + " /" + examinedBattler.BattleStats.MaxHP.ToString(), new Vector2(baseOffset.X + 256, baseOffset.Y + 92), Color.Black, 0.0f, TextMethods.CenterText(sfont, examinedBattler.BattleStats.HP.ToString() + "/" + examinedBattler.BattleStats.MaxHP.ToString()), 1.0f, SpriteEffects.None, 0.09f);
                                spriteBatch.DrawString(game.smallFont, examinedBattler.BattleStats.SP.ToString() + " /" + examinedBattler.BattleStats.MaxSP.ToString(), new Vector2(baseOffset.X + 256, baseOffset.Y + 119), Color.Black, 0.0f, TextMethods.CenterText(sfont, examinedBattler.BattleStats.SP.ToString() + "/" + examinedBattler.BattleStats.MaxSP.ToString()), 1.0f, SpriteEffects.None, 0.09f);
                                spriteBatch.DrawString(game.smallFont, examinedBattler.BattleStats.Attributes[Attributes.Strength].ToString(), new Vector2(baseOffset.X + 256, baseOffset.Y + 146), Color.Black, 0.0f, TextMethods.CenterText(sfont, examinedBattler.BattleStats.Attributes[Attributes.Strength].ToString()), 1.0f, SpriteEffects.None, 0.09f);
                                spriteBatch.DrawString(game.smallFont, examinedBattler.BattleStats.Attributes[Attributes.Magic].ToString(), new Vector2(baseOffset.X + 256, baseOffset.Y + 173), Color.Black, 0.0f, TextMethods.CenterText(sfont, examinedBattler.BattleStats.Attributes[Attributes.Magic].ToString()), 1.0f, SpriteEffects.None, 0.09f);
                                spriteBatch.DrawString(game.smallFont, examinedBattler.BattleStats.Attributes[Attributes.Dexterity].ToString(), new Vector2(baseOffset.X + 256, baseOffset.Y + 200), Color.Black, 0.0f, TextMethods.CenterText(sfont, examinedBattler.BattleStats.Attributes[Attributes.Dexterity].ToString()), 1.0f, SpriteEffects.None, 0.09f);
                                spriteBatch.DrawString(game.smallFont, examinedBattler.BattleStats.Armour.ToString(), new Vector2(baseOffset.X + 256, baseOffset.Y + 227), Color.Black, 0.0f, TextMethods.CenterText(sfont, examinedBattler.BattleStats.Armour.ToString()), 1.0f, SpriteEffects.None, 0.09f);
                                spriteBatch.DrawString(game.smallFont, examinedBattler.BattleStats.Luck.ToString(), new Vector2(baseOffset.X + 256, baseOffset.Y + 254), Color.Black, 0.0f, TextMethods.CenterText(sfont, examinedBattler.BattleStats.Luck.ToString()), 1.0f, SpriteEffects.None, 0.09f);



                                spriteBatch.Draw(background, new Rectangle(baseOffset.X + 48, baseOffset.Y + 80, 135, 186), new Rectangle(300, 300, 135, 186), Color.DarkGray, 0, Vector2.Zero, SpriteEffects.None, 0.12f);
                                var rect = new Rectangle(baseOffset.X + 114, baseOffset.Y + 173, MathHelper.Clamp(examinedBattler.Sprite.DrawnBounds.Width, 0, 135), MathHelper.Clamp(examinedBattler.Sprite.DrawnBounds.Height, 0, 186));
								spriteBatch.Draw(examinedBattler.Sprite.SpriteSheet, rect, examinedBattler.Sprite.SpriteRect, Color.White, 0, new Vector2(examinedBattler.Sprite.SpriteSize.X / 2, examinedBattler.Sprite.SpriteSize.Y / 2), SpriteEffects.None, 0.11f);
                                //draw modifier icons
                                var drawVector = new Vector2(baseOffset.X + 48, baseOffset.Y + 311);
                                var start = 0;
                                if (examinedBattler.BattleStats.team == Team.Player)
                                    start = 1;
                                for (int i = start; i < examinedBattler.BattleStats.Modifiers.Count; i++)
                                {
                                    if (game.Input.IsMouseOver(new Rectangle((int)drawVector.X, (int)drawVector.Y, 32, 32)) && examinedBattler.BattleStats.Modifiers[i].visible)
                                    {
                                        var s = "";
                                        for (int m = 0; m < examinedBattler.BattleStats.Modifiers[i].effects.Count; m++)
                                        {
                                            s = s + GetStringModifier(examinedBattler.BattleStats.Modifiers[i].effects[m]) + " ";
                                        }
                                        if (examinedBattler.BattleStats.Modifiers[i].effectPerTurn != null)
                                            s = s + GetStringModifier(examinedBattler.BattleStats.Modifiers[i].effectPerTurn);
                                        DrawMethods.DrawToolTip(spriteBatch, game.Input.oldMouseState.Position.ToVector2(), sfont, game.TextureLoader.RequestTexture("UI\\Common\\tooltipWindow"), s);
                                    }
                                    if (examinedBattler.BattleStats.Modifiers[i].visible == true)
                                    {
                                        spriteBatch.Draw(examinedBattler.BattleStats.Modifiers[i].icon, new Rectangle((int)drawVector.X, (int)drawVector.Y, 32, 32), null, Color.White, 0, Vector2.Zero, SpriteEffects.None, 0.09f);
                                        drawVector.X += 35;
                                    }
                                }
                            }
                            else
                            {
                                spriteBatch.Draw(battleTextBackWide, new Rectangle(BaseOffset.X + 250, BaseOffset.Y + 500, 300, 64), null, Color.White, 0, Vector2.Zero, SpriteEffects.None, 0.32f);
                                spriteBatch.DrawString(game.mediumFont, strings[4], new Vector2(BaseOffset.X + 400, BaseOffset.Y + 532), Color.White, 0.0f, TextMethods.CenterText(game.mediumFont, strings[4]), 1, SpriteEffects.None, 0.31f);
                            }
                            break;
                        }
                    case (UIState.Results):
                        {
                            var offset = new Point(game.GetScreenWidth() / 2 - 300, game.GetScreenHeight() / 2 - 235);
                            spriteBatch.Draw(ResultsWindow, new Rectangle(offset.X, offset.Y, 600, 470), null, Color.White, 0.0f, Vector2.Zero, SpriteEffects.None, 0.2f);
                            spriteBatch.DrawString(game.largeFont, strings[0], new Vector2(offset.X + 299, offset.Y + 57), Color.Black, 0.0f, TextMethods.CenterText(game.largeFont, strings[0]), 1.0f, SpriteEffects.None, 0.19f);
                            var Y = 138;
                            if (goldBounty > 0)
                            {
                                spriteBatch.Draw(resultsBox, new Rectangle(offset.X + 50, offset.Y + Y, 65, 65), null, Color.White, 0.0f, Vector2.Zero, SpriteEffects.None, 0.18f);
                                spriteBatch.Draw(goldIcon, new Rectangle(offset.X + 60, offset.Y + Y, 44, 44), null, Color.White, 0.0f, Vector2.Zero, SpriteEffects.None, 0.18f);
                                spriteBatch.DrawString(game.mediumFont, "+" + goldBounty, new Vector2(offset.X + 120, offset.Y + 138 + 20), Color.Black, 0.0f, Vector2.Zero, 1, SpriteEffects.None, 0.17f);
                                Y += 80;
                            }
                            spriteBatch.Draw(resultsBox, new Rectangle(offset.X + 50, offset.Y + Y, 65, 65), null, Color.White, 0.0f, Vector2.Zero, SpriteEffects.None, 0.18f);
                            spriteBatch.Draw(experienceIcon, new Rectangle(offset.X + 60, offset.Y + Y + 10, 44, 44), null, Color.White, 0.0f, Vector2.Zero, SpriteEffects.None, 0.18f);
                            spriteBatch.DrawString(game.mediumFont, "+" + expBounty, new Vector2(offset.X + 120, offset.Y + Y + 20), Color.Black, 0.0f, Vector2.Zero, 1, SpriteEffects.None, 0.17f);
                            Y += 80;
                            if (droppedItem != null)
                            {
                                spriteBatch.Draw(resultsBox, new Rectangle(offset.X + 50, offset.Y + Y, 65, 65), null, Color.White, 0.0f, Vector2.Zero, SpriteEffects.None, 0.18f);
                                spriteBatch.Draw(droppedItem.Icon, new Rectangle(offset.X + 48, offset.Y + Y + 8, 64, 64), null, Color.White, 0.0f, Vector2.Zero, SpriteEffects.None, 0.19f);
                            }
                            var y = offset.Y + 138;
                            foreach (KeyValuePair<int, int> kvp in levelUps)
                            {
                                spriteBatch.Draw(resultsBox, new Rectangle(offset.X + 327, y, 65, 65), null, Color.White, 0.0f, Vector2.Zero, SpriteEffects.None, 0.18f);
                                spriteBatch.Draw(playerCharacters[kvp.Key].uiPortrait, new Rectangle(offset.X + 329, y + 2, 62, 62), null, Color.White, 0.0f, Vector2.Zero, SpriteEffects.None, 0.19f);
                                spriteBatch.DrawString(game.mediumFont, "+" + kvp.Value.ToString(), new Vector2(offset.X + 396, y + 20), Color.Black, 0.0f, Vector2.Zero, 1, SpriteEffects.None, 0.17f);
                                y += 70;
                            }
                            spriteBatch.DrawString(game.mediumFont, strings[1], new Vector2(offset.X + 300, offset.Y + 428), Color.White, 0, TextMethods.CenterText(game.mediumFont, strings[1]), 1, SpriteEffects.None, 0.17f);
                            break;
                        }
                }
            }
        }
        public BattleAction GetWaitAction()
        {
            return new BattleAction("ACTION_WAITING", BattleAction.Direction.Self, BattleAction.TargetType.Self, 0, strings[6], null, null, null, ActionFXType.CenterOnTarget, BattlerAnimType.Idle, null, "MISC");
        }
        void getPlayerInput(GameTime gameTime)
        {
            displayBattleUI = true;
            for (int i = 0; i < uiButtons.Length; i++)
            {
                if (i == 0 && playerCharacters[activePlayerIndex].Equips[0] == null)
                {
                    continue;
                }
                if (i == 1 && !canUseItems)
                    continue;
                if (i == 4 && !canFlee)
                {
                    continue;
                }
                uiButtons[i].Update(gameTime);
                if (game.Input.IsMouseOver(uiButtons[i].Bounds) && game.Input.IsMouseButtonReleased())
                {
                    switch (i)
                    {
                        case (0): 
                        {
                                if (playerCharacters[activePlayerIndex].Equips[0] != null)
                            {
                                Usage[] usages = new Usage[3];
                                usages[0] = Usage.BothSame;
                                usages[1] = Usage.Battle;
                                usages[2] = Usage.BothAsynchrous;
                                turnAction = null; uiState = UIState.Spellbook; spellBook = new Spellbook(game,spellbookUI, playerCharacters[activePlayerIndex], usages) ;
                                    if (playerCharacters[activePlayerIndex].BattleStats.SP < spellBook.ReturnLowestManaCost())
                                    {
                                        turnAction = ZeroManaSpell.battleAction;
                                        uiState = UIState.ChooseTarget;
                                        spellBook = null;
                                        timer = 0;
                                    }
                                }
                            break; 
                        }
                        case (1): { turnAction = null; game.Audio.PlaySound("ButtonClick",true); uiState = UIState.Items; inventory = new Inventory(game, null, game.party, playerCharacters[activePlayerIndex], true); break; }
                        case (2): { turnAction = null; game.Audio.PlaySound("ButtonClick", true); uiState = UIState.Analyse; break; }
                        case (3): { turnAction = GetWaitAction(); break; }
                        case (4): { if (canFlee) {
                                    for (int e = 0; e < playerCharacters.Length; e++)
                                    {
                                        for (int x = 0; x < playerCharacters[e].BattleStats.Modifiers.Count; x++)
                                        {
                                            if (playerCharacters[e].BattleStats.Modifiers[x] is EquipmentModifier)
                                            {
                                                continue;
                                            }
                                            playerCharacters[e].BattleStats.Modifiers.RemoveAt(x);
                                            x--;
                                        }
                                        if (playerCharacters[e].BattleStats.HP < 1)
                                        {
                                            playerCharacters[e].BattleStats.HP = 1;
                                            playerCharacters[e].BattleStats.Modifiers.Remove(PCdeath);
                                            playerCharacters[e].BattleStats.RecalculateStats();
                                        }
                                    }
                                    game.Audio.PlaySound("ButtonClick", true); game.Audio.PlaySound("RunAway", true); EndState = BattleEnd.Fled; currentState = State.BattleResult; game.EndBattle(EndState); displayBattleUI = false; game.Audio.SetMusic(ReturnMusic); game.enemySpawnManager.FlagSpawn(game.TemporaryMap.Filename, game.BattleGroupNumber); } break; }

                    }
                }
                if (uiState == UIState.NoActionChosen)
                {
                    if (game.Input.IsKeyReleased(game.settings.spellKey))
                    {
                        if (playerCharacters[activePlayerIndex].Equips[0] != null)
                        {
                            Usage[] usages = new Usage[3];
                            usages[0] = Usage.BothSame;
                            usages[1] = Usage.Battle;
                            usages[2] = Usage.BothAsynchrous;
                            turnAction = null; uiState = UIState.Spellbook; spellBook = new Spellbook(game, spellbookUI, playerCharacters[activePlayerIndex], usages);
                        }
                    }
                    if (game.Input.IsKeyReleased(game.settings.inventoryKey) && canUseItems)
                    {
                        inventory = new Inventory(game, null, game.party, playerCharacters[activePlayerIndex], true);
                        turnAction = null; uiState = UIState.Items;
                    }
                }
            }
            if (turnAction == null)
            {
                switch (uiState)
                {
                    case (UIState.NoActionChosen):
                        {                           
                            break;
                        }
                    case (UIState.Spellbook):
                        {
                            {
                                spellBook.Update(gameTime);
                                var spell = spellBook.GetSpell();
                                if(spell != null)
                                {
                                    if(spell.usage != Usage.World)
                                    {
                                        currentActionUsedMana = spell.ManaCost(playerCharacters[activePlayerIndex]);
                                    }
                                        turnAction = spellBook.GetSpellBattleAction();
                                }                               
                                if (turnAction != null)
                                {
                                    uiState = UIState.ChooseTarget;
                                    spellBook = null;
                                    timer = 0;
                                    return;
                                }
                                if (spellBook.CheckForExit())
                                {
                                    uiState = UIState.NoActionChosen;
                                    spellBook = null;
                                    timer = 0;
                                }
                            }
                            break;
                        }
                    case (UIState.Items):
                        {
                            var activeItemno = inventory.GetItemNumber();
                            if (activeItemno != -1)
                            {
                                if (playerCharacters[activePlayerIndex].Inventory[activeItemno] is ConsumableItem && playerCharacters[activePlayerIndex].Inventory[activeItemno].Usage != Usage.World)
                                {
                                    activeItemIndex = activeItemno;
                                    turnAction = ((ConsumableItem)(playerCharacters[activePlayerIndex].Inventory[activeItemno])).BattleAction;
                                    removeItem = true;
                                    uiState = UIState.ChooseTarget;
                                    inventory = null;
                                    return;
                                }
                            }
                            if (inventory.CheckForExit())
                            {
                                this.uiState = UIState.NoActionChosen;
                                inventory = null;
                            }
                            break;
                        }
                    case (UIState.Analyse):
                        {
                            if (examinedBattler == null)
                            {
                                for (int i = Battlers.Count - 1; i > -1; i--)
                                {
                                    if (game.Input.IsMouseOver(Battlers[i].Bounds.Box) && game.Input.IsMouseButtonReleased())
                                    {
                                        if(Battlers[i].BattleStats.team == Team.Enemy && Battlers[i].BattleStats.canTarget)
                                        {
                                            examinedBattler = Battlers[i];
                                            break;
                                        }
                                        if (Battlers[i].BattleStats.team == Team.Player)
                                        {
                                            examinedBattler = Battlers[i];
                                            break;
                                        }

                                    }
                                }
                            }
                            if (examinedBattler != null)
                            {
                                var baseOffset = new Point(BaseOffset.X + 400 - (examineWindow.Width / 2), BaseOffset.Y + 300 - (examineWindow.Height / 2));
                                var exit = new Rectangle(baseOffset.X + 264, baseOffset.Y + 375, 74, 63);
                                if (game.Input.IsMouseOver(exit) && game.Input.IsMouseButtonReleased())
                                    examinedBattler = null;
                            }
                            break;
                        }
                }
            }
            if (turnAction != null)
            {
                switch (turnAction.targetType)
                {
                    case(BattleAction.TargetType.Single):
                        {
                            if (timer > 0)
                            {
                                for (int i = Battlers.Count - 1; i > -1; i--)
                                {
                                    if (game.Input.IsMouseOver(Battlers[i].Bounds.Box) && game.Input.IsMouseButtonReleased())
                                    {
                                        if (isValidAction(Battlers[i]))
                                        {
                                            targets.Add(Battlers[i]);
                                            activeBattler.BattleStats.SP -= currentActionUsedMana;
                                            currentActionUsedMana = 0;
                                            this.currentState = State.PreAnimating;
                                            AddActionVisualEffectsToList();
                                            displayBattleUI = false;
                                            activeBattler.ChangeAnimation(turnAction.animType);
                                            activeBattler.PlaySound(Random, game.Audio, turnAction.animType);
                                            timer = 0;
                                            if (activeItemIndex != -1)
                                            {
                                                playerCharacters[activePlayerIndex].RemoveItemFromInventory(activeItemIndex);
                                                activeItemIndex = -1;
                                            }
                                        }
                                        break;
                                    }
                                }
                            }
                            break;
                        }
                    case(BattleAction.TargetType.All):
                        {
                            targets = BattleMethods.GetTargets(activeBattler, turnAction, this);
                            activeBattler.BattleStats.SP -= currentActionUsedMana;
                            currentActionUsedMana = 0;
                            this.currentState = State.PreAnimating;
                            AddActionVisualEffectsToList();
                            displayBattleUI = false;
                            activeBattler.ChangeAnimation(turnAction.animType);
                            activeBattler.PlaySound(Random, game.Audio, turnAction.animType);
                            timer = 0;
                            if (activeItemIndex != -1 && removeItem == true)
                            {
                                playerCharacters[activePlayerIndex].Inventory.RemoveAt(activeItemIndex);
                                activeItemIndex = -1;
                                removeItem = false;
                            }
                            break;
                        }
                    case (BattleAction.TargetType.Self):
                        {
                            targets.Add(activeBattler);
                            activeBattler.BattleStats.SP -= currentActionUsedMana;
                            currentActionUsedMana = 0;
                            this.currentState = State.PreAnimating;
                            AddActionVisualEffectsToList();
                            displayBattleUI = false;
                            activeBattler.ChangeAnimation(turnAction.animType);
                            activeBattler.PlaySound(Random, game.Audio, turnAction.animType);
                            if (activeItemIndex != -1)
                            {
                                playerCharacters[activePlayerIndex].Inventory.RemoveAt(activeItemIndex);
                                activeItemIndex = -1;
                            }
                            timer = 0;
                            break;
                        }
                    case (BattleAction.TargetType.Random):
                        {
                            targets = BattleMethods.GetTargets(activeBattler, turnAction ,this);
                            activeBattler.BattleStats.SP -= currentActionUsedMana;
                            currentActionUsedMana = 0;
                            this.currentState = State.PreAnimating;
                            AddActionVisualEffectsToList();
                            displayBattleUI = false;
                            activeBattler.ChangeAnimation(turnAction.animType);
                            activeBattler.PlaySound(Random, game.Audio, turnAction.animType);
                            if (activeItemIndex != -1)
                            {
                                playerCharacters[activePlayerIndex].Inventory.RemoveAt(activeItemIndex);
                                activeItemIndex = -1;
                            }
                            timer = 0;
                            break;
                        }
                }

            }

        }
        bool isValidAction(Battler bat)
        {          
            switch (turnAction.bDirection)
            {
                case (BattleAction.Direction.Enemy):
                    {
                        if (activeBattler.BattleStats.team != bat.BattleStats.team)
                        {
                            if ((turnAction.targetType == BattleAction.TargetType.Single || turnAction.targetType == BattleAction.TargetType.Random) && !bat.BattleStats.canTarget)
                            {
                                return false;
                            }
                            return true;
                        }
                        if (activeBattler.BattleStats.team == bat.BattleStats.team)
                        {
                            return false;
                        }
                        break;
                    }
                case (BattleAction.Direction.Self):
                    {
                        if (activeBattler.BattleStats.team != bat.BattleStats.team)
                        {
                            return false;
                        }
                        if (activeBattler.BattleStats.team == bat.BattleStats.team)
                        {
                            if(bat.BattleStats.HP < 1)
                            {
                                for(int i = 0; i < turnAction.actionEffects.Count; i++)
                                {
                                    if(turnAction.actionEffects[i] is ResurrectBattler)
                                    {
                                        return true;
                                    }
                                }
                                return false;
                            }
                            return true;
                        }
                        break;
                    }
                case (BattleAction.Direction.Both):
                    {
                        if (bat.BattleStats.canTarget)
                        {
                            return true;
                        }
                        break;
                    }
            }

            return false;
        }
        void CheckBattleEndConditions()
        {
            var playerBattlers = new List<Battler>();
            var enemyBattlers = new List<Battler>();
            foreach (Battler battler in Battlers)
            {
                if (battler.BattleStats.team == Team.Player)
                {
                    playerBattlers.Add(battler);
                }
                if (battler.BattleStats.team == Team.Enemy)
                {
                    enemyBattlers.Add(battler);
                }
            }
            foreach(Battler playerBattler in playerBattlers)
            {
                if (playerBattler.BattleStats.HP > 0)
                {
                    goto CheckWin;
                }
            }
            BattleOver = true;
            if (!continueAfterLoss)
                EndState = BattleEnd.Lost;
            else
                EndState = BattleEnd.SpecialEnd;
            game.Audio.SetMusic("BattleLost", false);
            if (game.Audio.music != null)
            {
                game.Audio.music.IsLooped = false;
            }
            return;
            CheckWin:
            if (enemyBattlers.Count == 0)
            {
                BattleOver = true;
                CalculateBattleResult();
                EndState = BattleEnd.Won;
                uiState = UIState.Results;
            }
        }
        void AddActionVisualEffectsToList()
        {
            var pan = 0f;
            if (activeBattler.BattleStats.team == Team.Player)
                pan = -0.5f;
            else
                pan = 0.5f;
            if (turnAction.actionSpriteEffects == null)
            {
                return;
            }
            switch (currentState)
            {
                case(State.PreAnimating):
                    {
                        game.Audio.PlaySound(turnAction.sounds[0], true, pan);
                        break;
                    }
                case(State.Animating):
                    {
                        if (turnAction.sounds.Length == 3)
                        {
                            game.Audio.StopSound(turnAction.sounds[0]);
                            game.Audio.PlaySound(turnAction.sounds[1], true);
                        }
                        if (turnAction.actionSpriteEffects.Length > 1)
                        {
                            switch (turnAction.fxType)
                            {
                                case (ActionFXType.CenterOnTarget):
                                    {
                                        for (int i = 0; i < targets.Count; i++)
                                        {
                                            actionEffects.Add(new SpriteEffect(game.TextureLoader, targets[i].Sprite.DrawnPosition, turnAction.actionSpriteSizes[1],"Sprites\\BattleEffects\\" + turnAction.actionSpriteEffects[1], true));
											actionEffects[actionEffects.Count - 1].sprite.SetScale(turnAction.actionSpriteSizes[1].X, turnAction.actionSpriteSizes[1].Y);
                                            actionEffects[actionEffects.Count - 1].sprite.SetInterval(turnAction.actionSpriteSpeeds[1], true);
                                        }
                                        break;
                                    }
                                case (ActionFXType.Projectile):
                                    {
                                        for (int i = 0; i < targets.Count; i++)
                                        {
                                            actionEffects.Add(new SpriteEffect(game.TextureLoader, activeBattler.Sprite.DrawnPosition, turnAction.actionSpriteSizes[1], "Sprites\\BattleEffects\\" + turnAction.actionSpriteEffects[1], false));
											actionEffects[actionEffects.Count - 1].sprite.SetScale(turnAction.actionSpriteSizes[1].X, turnAction.actionSpriteSizes[1].Y);
                                            actionEffects[actionEffects.Count - 1].SetMovement(new Mover(null, 28, Mover.MovementType.Linear), targets[i].Sprite.DrawnPosition);
                                            actionEffects[actionEffects.Count - 1].sprite.ChangeRotation(GetRadians(activeBattler.Sprite.DrawnPosition, targets[i].Sprite.DrawnPosition));
                                        }
                                        break;
                                    }
                                case (ActionFXType.WholeScreen):
                                    {
                                        actionEffects.Add(new SpriteEffect(game.TextureLoader, new Point(BaseOffset.X + 400, BaseOffset.Y + 300), turnAction.actionSpriteSizes[1], "Sprites\\BattleEffects\\" + turnAction.actionSpriteEffects[1], true));
                                        break;
                                    }
                            }
                        }                       
                        break;
                    }
                case (State.ActionResult):
                    {
                        if (targets != null)
                        {
                            if (targets.Count > 0)
                                if (targets[0].BattleStats.team == Team.Player)
                                    pan = -0.5f;
                                else
                                    pan = 0.5f;
                        }
                        //todo: add more things that affect playImpactSound
                        var playImpactSound = false;
                        for(int i = 0; i < actionSuccesses.Count; i++)
                        {
                            if (actionSuccesses[i])
                            {
                                playImpactSound = true;
                                continue;
                            }                       
                        }
                        if (turnAction.actionSpriteEffects.Length > 1)
                        {
                            for (int i = 0; i < targets.Count; i++)
                            {
                                if(actionSuccesses[i])
                                {
                                    actionEffects.Add(new SpriteEffect(game.TextureLoader, targets[i].Sprite.DrawnPosition, turnAction.actionSpriteSizes[2], "Sprites\\BattleEffects\\" + turnAction.actionSpriteEffects[2], true));
                                }                                    
                                else
                                {
                                    if(turnAction.fxType == ActionFXType.Projectile)
                                    {
                                        actionEffects.Add(new SpriteEffect(game.TextureLoader, targets[i].Sprite.DrawnPosition, turnAction.actionSpriteSizes[1], "Sprites\\BattleEffects\\" + turnAction.actionSpriteEffects[1], false));
                                        actionEffects[actionEffects.Count - 1].sprite.SetInterval(turnAction.actionSpriteSpeeds[1], true);
										actionEffects[actionEffects.Count - 1].sprite.SetScale(turnAction.actionSpriteSizes[1].X, turnAction.actionSpriteSizes[1].Y); actionEffects[actionEffects.Capacity].sprite.SetScale(turnAction.actionSpriteSizes[1].X, turnAction.actionSpriteSizes[1].Y);
                                        var targetVec = new Point((int)MathHelper.Distance(activeBattler.Sprite.DrawnPosition.X, targets[i].Sprite.DrawnPosition.X), (int)MathHelper.Distance(activeBattler.Sprite.DrawnPosition.Y, targets[i].Sprite.DrawnPosition.Y));
                                        if (activeBattler.Sprite.DrawnPosition.X > targets[i].Sprite.DrawnPosition.X)
                                            targetVec.X = -targetVec.X;
                                        if (activeBattler.Sprite.DrawnPosition.Y > targets[i].Sprite.DrawnPosition.Y)
                                            targetVec.Y = -targetVec.Y;
                                        targetVec.X += targets[i].Sprite.DrawnPosition.X;
                                        targetVec.Y += targets[i].Sprite.DrawnPosition.Y;
                                        actionEffects[actionEffects.Count - 1].SetMovement(new Mover(null, 28, Mover.MovementType.Linear), targetVec);
                                        actionEffects[actionEffects.Count - 1].sprite.ChangeRotation(GetRadians(targets[i].Sprite.DrawnPosition, targetVec));
                                    }
                                }
                               
                            }
                        }
                        if (playImpactSound)
                            {
                                if (turnAction.sounds.Length == 3)
                                    game.Audio.PlaySound(turnAction.sounds[2], true, pan);
                                if (turnAction.sounds.Length == 2)
                                    game.Audio.PlaySound(turnAction.sounds[1], true, pan);
                            }
                        }
                        break;
                    }
            }        
        void CalculateBattleResult() //dole out exp, adjust player health to be the health their battler was at upon return to the world
        {
            var aliveCharacters = 0;
            for (int i = 0; i < playerCharacters.Length; i++)
            {
                if (playerCharacters[i].BattleStats.HP > 0)
                {
                    aliveCharacters++;
                }
                for (int e = 0; e < playerCharacters[i].BattleStats.Modifiers.Count; e++)
                {
                    if (playerCharacters[i].BattleStats.Modifiers[e].Name == "mod_lightboost" || playerCharacters[i].BattleStats.Modifiers[e].Name == "mod_shadowboost")
                    {
                        playerCharacters[i].BattleStats.Modifiers.RemoveAt(e);
                        e--;
                    }
                }
            }
            expBounty = expBounty / aliveCharacters;
            party.Gold += goldBounty;
            for (int i = 0; i < playerCharacters.Length; i++)
            {
                if (droppedItem != null)
                {
                    if (playerCharacters[i].Inventory.Count < 16)
                    {
                        playerCharacters[i].Inventory.Add(droppedItem);
                        break;
                    }
                }
            }
            for (int i = 0; i < playerCharacters.Length; i++)
            {
                if (Battlers[i].BattleStats.HP > 0)
                {
                    playerCharacters[i].Exp += expBounty;
                    playerCharacters[i].TotalExp += (uint)expBounty;
                    if (playerCharacters[i].Exp >= playerCharacters[i].NextLevelExp && playerCharacters[i].Level != 40)
                    {
                        levelUps.Add(i, 0);
                        while (playerCharacters[i].Exp >= playerCharacters[i].NextLevelExp && playerCharacters[i].Level != 40)
                        {
                            if (playerCharacters[i].LevelUp(game))
                            {
                                Dialogs.Add(new PlayDialogue(game,null,playerCharacters[i].Name + strings[2] + playerCharacters[i].Spells[playerCharacters[i].Spells.Count -1].displayName + "!"));
                            }
                            levelUps[i] += 1;
                        }
                    }
                }
            }
            for (int i = 0; i < playerCharacters.Length; i++)
            {
                for (int x = 0; x < playerCharacters[i].BattleStats.Modifiers.Count; x++)
                {
                    if (playerCharacters[i].BattleStats.Modifiers[x] is EquipmentModifier)
                        continue;
                    playerCharacters[i].BattleStats.Modifiers.RemoveAt(x);
                    x--;
                }
                if (playerCharacters[i].BattleStats.HP < 1)
                {
                    playerCharacters[i].BattleStats.HP = 1;
                    playerCharacters[i].BattleStats.RecalculateStats();
                }
            }
        }
        void GetFleeChance()
        {
            var enemies = new List<Battler>();
            for (int i = 0; i < Battlers.Count; i++)
            {
                if (Battlers[i].BattleStats.team == Team.Enemy)
                {
                    enemies.Add(Battlers[i]);
                }
            }
            if(enemies.TrueForAll(b => b.BattleStats.canAct == false) && FleeChance != -1)
            {
                canFlee = true;
            }
            if (FleeChance == -1 || canFlee == true)
            {
                return;
            }
            FleeChance += game.randomNumber.Next(1, 7);
            if (FleeChance > 7)
            {
                canFlee = true;
            }
        }
        float GetRadians(Point vec1, Point vec2)
        {
            var f = (float)Math.Atan2(vec2.Y - vec1.Y, vec2.X - vec1.X);
            return f;
        }
        
    }
}