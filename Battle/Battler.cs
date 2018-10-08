using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Xml;
using System.Xml.Linq;
using System.IO;
using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;
using Microsoft.Xna.Framework.Content;

namespace Magicians
{
    public enum BattlerTags
    {
        Null, //used if DoDamage has no specific bonus damage
        Organic, //will suffer from BEES
        Flying, //takes extra damage from whirlwind, dodges 25% of attacks
        Mechanic, //immune to most status effects, takes extra damage from acid attacks + rust
        Plant, //treants//hedge wizards and stuff
        Magical //this creature is a summon/magical entity and will take damage from dispel
    }
	//a battler represents a single character on the field: a player character, a boss, a minion, etc
    class Battler
    {
        public string battlerGraphicsFolder;
        public int Width = -1;
        public int Height = -1;
        public float Duration;
        public string worldGraphicsFolder; //in which the walker sprite is kept
        public int worldMovementSpeed; //the speed the entity representing this battler in the world moves at
        public SortedList<BattlerAnimType, Texture2D> Animations = new SortedList<BattlerAnimType, Texture2D>();
        BattlerAnimType currentAnim = BattlerAnimType.Idle;
        public Sprite Sprite;
        public BattleStats BattleStats;
        public SortedList<BattlerAnimType, string> Sounds = new SortedList<BattlerAnimType, string>();

        public List<BattleAction> battleActions; //set to null for the player
        public BattleAction lastAction;
        public BattleAction defaultAction;
        public BattleAction deathAction;
        public int actionNumber = -1;

        public bool turnOver;
        public bool twoTurns { get; private set; }
        public bool lastTurn;
        public bool inOrder;

        public Bounds Bounds { get; private set; }
        public int exp { get; private set; }
        public int gold { get; private set; }

        public bool displayStats = true;
        public bool useBossHealthBar;

        public List<Behaviours> worldBehaviours = new List<Behaviours>();
        
        public string Name; //the battler's display name, not its internal name
        public string internalName; //internal

        public string itemDrop;
        public int dropchance;

        public void AddBehaviour(Behaviours b)
        {
            worldBehaviours.Add(b);
        }

        public Battler(Game g, string name)
        {
            var mod = new Modifier();
            var data = XDocument.Load(g.Content.RootDirectory + g.PathSeperator + "Data" + g.PathSeperator + "Battlers.xml", LoadOptions.None);
            XElement el = null;
            foreach (XElement elem in data.Descendants("Battler"))
            {
                if (elem.Attribute("internal").Value == name)
                {
                    el = elem;
                    break;
                }
            }
            this.Name = g.LoadString("Battlers", el.Attribute("internal").Value);
            this.internalName = (string)el.Attribute("internal");
            this.battlerGraphicsFolder = (string)el.Attribute("battlegraphics");
            this.worldGraphicsFolder = (string)el.Attribute("worldgraphics");
            useBossHealthBar = bool.Parse(el.Attribute("boss").Value);
            displayStats = !bool.Parse(el.Attribute("boss").Value);
            exp = int.Parse(el.Attribute("exp").Value);
            gold = int.Parse(el.Attribute("gold").Value);
            dropchance = int.Parse(el.Attribute("chance").Value);
            itemDrop = el.Attribute("item").Value;
            twoTurns = false;
            if (el.Attribute("twoturns") != null)
                twoTurns = true;
            if (el.Attribute("inorder") != null)
                inOrder = true;
            if (el.Attribute("behaviour") != null)
            {
                var args = el.Attribute("behaviour").Value.Split(',');
                for (int i = 0; i < args.Length; i++)
                {
                    switch (args[i])
                    {
                        case ("avoidlight"):
                            {
                                AddBehaviour(Behaviours.FreezeOnLight);
                                break;
                            }
                        case ("flying"):
                            {
                                AddBehaviour(Behaviours.Flying);
                                break;
                            }
                        case ("interactwithent"):
                            {
                                AddBehaviour(Behaviours.InteractWithEntity);
                                break;
                            }
                        case ("sentry"):
                            {
                                AddBehaviour(Behaviours.Sentry);
                                break;
                            }
                        case ("blind"):
                            {
                                AddBehaviour(Behaviours.Blind);
                                break;
                            }
                        case ("wanders"):
                            {
                                AddBehaviour(Behaviours.Wanders);
                                break;
                            }
                    }
                }
            }

            if (el.Element("Sprite") != null)
            {
                Width = (int)el.Attribute("width");
                Height = (int)el.Attribute("height");
                Duration = (float)el.Attribute("interval");
            }
            if (el.Element("Sounds") != null)
            {
                if (el.Element("Sounds").Attribute("attack") != null)
                    Sounds.Add(BattlerAnimType.Attack, el.Element("Sounds").Attribute("attack").Value);
                if (el.Element("Sounds").Attribute("die") != null)
                    Sounds.Add(BattlerAnimType.Die, el.Element("Sounds").Attribute("die").Value);
                if (el.Element("Sounds").Attribute("recoil") != null)
                    Sounds.Add(BattlerAnimType.Recoil, el.Element("Sounds").Attribute("recoil").Value);
                if (el.Element("Sounds").Attribute("spell") != null)
                    Sounds.Add(BattlerAnimType.CastSpell, el.Element("Sounds").Attribute("spell").Value);
            }
            if (el.Element("ScaleFromPlayer") != null)
            {
                BattleStats = new BattleStats(this);
                var playerID = int.Parse(el.Element("ScaleFromPlayer").Attribute("id").Value);
                var pc = g.party.GetPlayerCharacter(playerID);
                BattleStats.MaxHP += pc.BattleStats.MaxHP;
                BattleStats.HP += pc.BattleStats.MaxHP;
                BattleStats.MaxSP += pc.BattleStats.MaxSP;
                BattleStats.SP += pc.BattleStats.MaxSP;
                BattleStats.baseLuck = pc.BattleStats.baseLuck;
                if (BattleStats.baseAttributes.Count == 0)
                {
                    BattleStats.baseAttributes.Add(Attributes.Strength, 0);
                    BattleStats.baseAttributes.Add(Attributes.Magic, 0);
                    BattleStats.baseAttributes.Add(Attributes.Dexterity, 0);
                }
                BattleStats.baseAttributes[Attributes.Strength] += pc.BattleStats.Attributes[Attributes.Strength];
                BattleStats.baseAttributes[Attributes.Magic] += pc.BattleStats.Attributes[Attributes.Magic];
                BattleStats.baseAttributes[Attributes.Dexterity] += pc.BattleStats.Attributes[Attributes.Dexterity];
                BattleStats.baseArmour += pc.BattleStats.Armour;
                BattleStats.baseLuck += pc.BattleStats.Luck;
                BattleStats.baseResistances.Add(DamageTypes.Physical, pc.BattleStats.Resistances[DamageTypes.Physical]);
                BattleStats.baseResistances.Add(DamageTypes.Fire, pc.BattleStats.Resistances[DamageTypes.Fire]);
                BattleStats.baseResistances.Add(DamageTypes.Electricity, pc.BattleStats.Resistances[DamageTypes.Electricity]);
                BattleStats.baseResistances.Add(DamageTypes.Cold, pc.BattleStats.Resistances[DamageTypes.Cold]);
                BattleStats.baseResistances.Add(DamageTypes.Light, pc.BattleStats.Resistances[DamageTypes.Light]);
                BattleStats.baseResistances.Add(DamageTypes.Poison, pc.BattleStats.Resistances[DamageTypes.Poison]);
            }
            if (el.Element("World") != null)
            {
                if (el.Element("World").Attribute("speed") != null)
                    worldMovementSpeed = int.Parse(el.Element("World").Attribute("speed").Value);
            }
            else
            {
                worldMovementSpeed = 4;
            }

            if (BattleStats == null)
                BattleStats = new BattleStats(this);
            BattleStats.MaxHP += int.Parse(el.Element("Stats").Attribute("health").Value);
            BattleStats.HP += int.Parse(el.Element("Stats").Attribute("health").Value);
            BattleStats.MaxSP += int.Parse(el.Element("Stats").Attribute("mana").Value);
            BattleStats.SP += int.Parse(el.Element("Stats").Attribute("mana").Value);
            if (BattleStats.baseAttributes.Count == 0)
            {
                BattleStats.baseAttributes.Add(Attributes.Strength, 0);
                BattleStats.baseAttributes.Add(Attributes.Magic, 0);
                BattleStats.baseAttributes.Add(Attributes.Dexterity, 0);
            }
            BattleStats.baseAttributes[Attributes.Strength] += int.Parse(el.Element("Stats").Attribute("strength").Value);
            BattleStats.baseAttributes[Attributes.Magic] += int.Parse(el.Element("Stats").Attribute("magic").Value);
            BattleStats.baseAttributes[Attributes.Dexterity] += int.Parse(el.Element("Stats").Attribute("agility").Value);
            BattleStats.baseArmour += int.Parse(el.Element("Stats").Attribute("armour").Value);
            BattleStats.baseLuck += int.Parse(el.Element("Stats").Attribute("luck").Value);
            if (BattleStats.baseResistances.Count == 0)
            {
                BattleStats.baseResistances.Add(DamageTypes.Physical, int.Parse(el.Element("Resistances").Attribute("phys").Value));
                BattleStats.baseResistances.Add(DamageTypes.Fire, int.Parse(el.Element("Resistances").Attribute("fire").Value));
                BattleStats.baseResistances.Add(DamageTypes.Electricity, int.Parse(el.Element("Resistances").Attribute("elec").Value));
                BattleStats.baseResistances.Add(DamageTypes.Cold, int.Parse(el.Element("Resistances").Attribute("cold").Value));
                BattleStats.baseResistances.Add(DamageTypes.Light, int.Parse(el.Element("Resistances").Attribute("light").Value));
                BattleStats.baseResistances.Add(DamageTypes.Poison, int.Parse(el.Element("Resistances").Attribute("poison").Value));
                if (el.Element("Resistances").Attribute("raw") != null)
                    BattleStats.baseResistances.Add(DamageTypes.Raw, int.Parse(el.Element("Resistances").Attribute("raw").Value));
                else
                    BattleStats.baseResistances.Add(DamageTypes.Raw, 0);
            }
            if (el.Element("Invulns") != null)
                BattleStats.modInvulnerabilities = el.Element("Invulns").Value.Split(',');
            if (el.Element("Vulns") != null)
                BattleStats.modVulnerabilities = el.Element("Vulns").Value.Split(',');
           
            battleActions = new List<BattleAction>();
            var actions = el.Element("Actions").Value.Split(',');
            if (actions[0] == "copy_from_player")
            {
                for (int i = 0; i < g.party.PlayerCharacters[0].Spells.Count; i++)
                {
                    if (g.party.PlayerCharacters[0].Spells[i].usage == Usage.Battle || g.party.PlayerCharacters[0].Spells[i].usage == Usage.BothSame)
                    {
                        if (g.party.PlayerCharacters[0].Spells[i].battleAction.tag != "")
                            battleActions.Add(g.party.PlayerCharacters[0].Spells[i].battleAction);
                    }
                }
                for (int i = 0; i < g.party.PlayerCharacters[0].Inventory.Count; i++)
                {
                    if (g.party.PlayerCharacters[0].Inventory[i].Usage == Usage.Battle || g.party.PlayerCharacters[0].Inventory[i].Usage == Usage.BothSame)
                    {
                        if (g.party.PlayerCharacters[0].Inventory[i] is ConsumableItem)
                        {                     
                            var consum = (ConsumableItem)g.party.PlayerCharacters[0].Inventory[i];
                            if (consum.BattleAction.tag != "")
                                if (!battleActions.Contains(consum.BattleAction))
                                {
                                    battleActions.Add(consum.BattleAction);
                                }
                        }

                    }
                }
            }
            else
                for (int i = 0; i < actions.Length; i++)
                {
                    try { battleActions.Add(g.battleActions.Find(b => b.Name == actions[i])); }
                    catch { Console.WriteLine("PANIC: BATTLER " + internalName + " IS SET TO HAVE ACTION " + actions[i] + " BUT THE ACTION WAS NOT FOUND"); }

                }
            if (el.Element("DefaultAction") != null)
            {
                try {  defaultAction = g.battleActions.Find(b => b.Name == el.Element("DefaultAction").Value); }
                catch { Console.WriteLine("PANIC: BATTLER " + internalName + " IS SET TO HAVE DEFAULT ACTION " + el.Element("DefaultAction").Value + " BUT THE ACTION WAS NOT FOUND"); }
            }
            if (el.Element("DeathAction") != null)
            {
                try { deathAction = g.battleActions.Find(b => b.Name == el.Element("DeathAction").Value); }
                catch { Console.WriteLine("PANIC: BATTLER " + internalName + " IS SET TO HAVE DEATH ACTION " + el.Element("DeathAction").Value + " BUT THE ACTION WAS NOT FOUND"); }
            }
            if (el.Attribute("behaviour").Value == "flying")
            {
                BattleStats.Modifiers.Add(g.modifiers.Find(m => m.Name == "mod_flying"));
            }
            if (el.Element("Modifiers") != null)
            {
                var args = el.Element("Modifiers").Value.Split(',');
                foreach (string arg in args)
                {
                    var modi = g.modifiers.Find(m => m.Name == arg);
                    if (modi != null)
                        BattleStats.Modifiers.Add(modi);
                }
            }
            if (el.Attribute("tags") != null)
            {
                var args = el.Attribute("tags").Value.Split(',');
                for (int i = 0; i < args.Length; i++)
                {
                    switch (args[i])
                    {
                        case ("flying"): BattleStats.tags.Add(BattlerTags.Flying); break;
                        case ("organic"): BattleStats.tags.Add(BattlerTags.Organic); break;
                        case ("machine"): BattleStats.tags.Add(BattlerTags.Mechanic); break;
                        case ("plant"): BattleStats.tags.Add(BattlerTags.Plant); break;
                        case ("magical"): BattleStats.tags.Add(BattlerTags.Magical); break;
                        default: Console.WriteLine("PANIC: ATTEMPTED TO ADD BATTLER TAG:" + args[i] + "TO " + internalName + ", BUT BATTLER TAG DOES NOT EXIST"); break;
                    }
                }
            }
        }
        public Battler()
        {

        }
        public string[] ReturnSpecialEntArgs(Game g)
        {
            string[] args = new string[2];
            var data = XDocument.Load(g.Content.RootDirectory + "//Data//Battlers.xml", LoadOptions.None);
            XElement el = null;
            foreach (XElement elem in data.Descendants("Battler"))
            {
                if (elem.Attribute("internal").Value == internalName)
                {
                    el = elem;
                    break;
                }
            }
            args[0] = el.Attribute("events").Value;
            args[1] = el.Attribute("entity").Value;
            return args;
        }
        public List<Battler> ReturnInList()
        {
            var List = new List<Battler>();
            List.Add(this);
            return List;
        }
        public Battler(PlayerCharacter pc, string n, string gra, BattleStats bs)
        {
            Name = n;         
            battlerGraphicsFolder = gra;         
            BattleStats = bs;
            Sounds = new SortedList<BattlerAnimType, string>();
            Sounds.Add(BattlerAnimType.Recoil, "");
            Sounds.Add(BattlerAnimType.Dead, "");
            if(pc.Gender == Gender.Male)
            {
                Sounds[BattlerAnimType.Recoil] = "MalePain1|MalePain2|MalePain3|MalePain4|MalePain5";
                Sounds[BattlerAnimType.Die] = "MaleDeath";
            }
            if (pc.Gender == Gender.Female)
            {
                Sounds[BattlerAnimType.Recoil] = "FemalePain1|FemalePain2|FemalePain3|FemalePain4|FemalePain5";
                Sounds[BattlerAnimType.Die] = "FemaleDeath";
            }
        }
        public Battler(string n, string gra, int hp, int sp, SortedList<DamageTypes,int> r, SortedList<Attributes,int> s)
        {
            Name = n;         
            battlerGraphicsFolder = gra;         
            BattleStats = new BattleStats(this);
            BattleStats.MaxHP = hp;
            BattleStats.MaxSP = sp;
            BattleStats.HP = BattleStats.MaxHP;
            BattleStats.SP = BattleStats.MaxSP;         
            BattleStats.baseResistances = r;
            BattleStats.baseAttributes = s;
            foreach (KeyValuePair<Attributes, int> kvp in BattleStats.baseAttributes)
            {
                BattleStats.Attributes.Add(kvp.Key, kvp.Value);
            }
            foreach (KeyValuePair<DamageTypes, int> kvp in BattleStats.baseResistances)
            {
                BattleStats.Resistances.Add(kvp.Key, kvp.Value);
            }
        }
        public void SetPosition(Game g, Point vec)
        {
            if (File.Exists(g.Content.RootDirectory + g.PathSeperator + "Sprites" + g.PathSeperator + "Battlers" + g.PathSeperator + battlerGraphicsFolder + g.PathSeperator + "Idle.xnb"))
                Animations[BattlerAnimType.Idle] = g.TextureLoader.RequestTexture("Sprites\\Battlers\\" + battlerGraphicsFolder + "\\Idle");
            if (File.Exists(g.Content.RootDirectory + g.PathSeperator + "Sprites" + g.PathSeperator + "Battlers" + g.PathSeperator + battlerGraphicsFolder + g.PathSeperator + "Attack.xnb"))
                Animations[BattlerAnimType.CastSpell] = g.TextureLoader.RequestTexture("Sprites\\Battlers\\" + battlerGraphicsFolder + "\\Attack");
            if (File.Exists(g.Content.RootDirectory + g.PathSeperator + "Sprites" + g.PathSeperator + "Battlers" + g.PathSeperator + battlerGraphicsFolder + g.PathSeperator + "Cast.xnb"))
                Animations[BattlerAnimType.CastSpell] = g.TextureLoader.RequestTexture("Sprites\\Battlers\\" + battlerGraphicsFolder + "\\Cast");
            if (File.Exists(g.Content.RootDirectory + g.PathSeperator + "Sprites" + g.PathSeperator + "Battlers" + g.PathSeperator + battlerGraphicsFolder + g.PathSeperator + "UseItem.xnb"))
                Animations[BattlerAnimType.UseItem] = g.TextureLoader.RequestTexture("Sprites\\Battlers\\" + battlerGraphicsFolder + "\\UseItem");
            if (File.Exists(g.Content.RootDirectory + g.PathSeperator + "Sprites" + g.PathSeperator + "Battlers" + g.PathSeperator + battlerGraphicsFolder + g.PathSeperator + "Recoil.xnb"))
                Animations[BattlerAnimType.Recoil] = g.TextureLoader.RequestTexture("Sprites\\Battlers\\" + battlerGraphicsFolder + "\\Recoil");
            if (File.Exists(g.Content.RootDirectory + g.PathSeperator + "Sprites" + g.PathSeperator + "Battlers" + g.PathSeperator + battlerGraphicsFolder + g.PathSeperator + "Die.xnb"))
                Animations[BattlerAnimType.Die] = g.TextureLoader.RequestTexture("Sprites\\Battlers\\" + battlerGraphicsFolder + "\\Die");
            if (File.Exists(g.Content.RootDirectory + g.PathSeperator + "Sprites" + g.PathSeperator + "Battlers" + g.PathSeperator + battlerGraphicsFolder + g.PathSeperator + "Dead.xnb"))
                Animations[BattlerAnimType.Dead] = g.TextureLoader.RequestTexture("Sprites\\Battlers\\" + battlerGraphicsFolder + "\\Dead");
            if (File.Exists(g.Content.RootDirectory + g.PathSeperator + "Sprites" + g.PathSeperator + "Battlers" + g.PathSeperator + battlerGraphicsFolder + g.PathSeperator + "Injured.xnb"))
                Animations[BattlerAnimType.Dead] = g.TextureLoader.RequestTexture("Sprites\\Battlers\\" + battlerGraphicsFolder + "\\Injured");
            if (File.Exists(g.Content.RootDirectory + g.PathSeperator + "Sprites" + g.PathSeperator + "Battlers" + g.PathSeperator + battlerGraphicsFolder + g.PathSeperator + "PostCast.xnb"))
                Animations[BattlerAnimType.CastSpell] = g.TextureLoader.RequestTexture("Sprites\\Battlers\\" + battlerGraphicsFolder + "\\PostCast");
            if (File.Exists(g.Content.RootDirectory + g.PathSeperator + "Sprites" + g.PathSeperator + "Battlers" + g.PathSeperator + battlerGraphicsFolder + g.PathSeperator + "PostAttack.xnb"))
                Animations[BattlerAnimType.PostAttack] = g.TextureLoader.RequestTexture("Sprites\\Battlers\\" + battlerGraphicsFolder + "\\PostAttack");
			//TODO: replace these with properties loaded from the xml file
			if (battlerGraphicsFolder == "placeholder")
            {
				Sprite = new Sprite(g.TextureLoader, null, vec, 0.5f, new Point(Animations[BattlerAnimType.Idle].Width / 2, Animations[BattlerAnimType.Idle].Height), Sprite.OriginType.FromCentre);
                Sprite.SetInterval(280);
            }
            else if(Width == -1)
            {
				Sprite = new Sprite(g.TextureLoader, null, vec, 0.5f, new Point(Animations[BattlerAnimType.Idle].Width / 4, Animations[BattlerAnimType.Idle].Height), Sprite.OriginType.FromCentre);
                Sprite.SetInterval(140);
            }
            else
            {
				Sprite = new Sprite(g.TextureLoader, null, vec, 0.5f, new Point(Width, Height), Sprite.OriginType.FromCentre);
                Sprite.SetInterval(140);
            }
            Sprite.ChangeTexture2D(Animations[BattlerAnimType.Idle]);         
            Bounds = new Bounds(null, new Point(Sprite.DrawnPosition.X - (Sprite.SpriteSize.X / 2), Sprite.DrawnPosition.Y - (Sprite.SpriteSize.Y / 2)), Sprite.SpriteSize.X, Sprite.SpriteSize.Y, true, new Point(0));
            Sprite.ChangeLooping(true);
        }

        public void Update(GameTime gameTime)
        {           
            if (currentAnim != BattlerAnimType.Idle)
            {
                if (Sprite.ReachedEnd && currentAnim != BattlerAnimType.Die)
                {
                    if (BattleStats.HP < BattleStats.MaxHP / 3)
                        ChangeAnimation(BattlerAnimType.Injured);
                    else
                        ChangeAnimation(BattlerAnimType.Idle);
                }
            }
            Sprite.Update(gameTime);
        }
        public void ChangeAnimation(BattlerAnimType at)
        {            
            if (Animations.ContainsKey(at))
            {
                Sprite.ChangeTexture2D(Animations[at],true);
                if (at != BattlerAnimType.Idle || at != BattlerAnimType.Injured)
                    Sprite.ChangeLooping(false);
                if (at == BattlerAnimType.Attack || at == BattlerAnimType.CastSpell || at == BattlerAnimType.UseItem)
                    Sprite.SetInterval(100);
                if (at == BattlerAnimType.Idle)
                {
                    for(int i = 0; i < BattleStats.Modifiers.Count; i++)
                    {
                        for(int e = 0; e < BattleStats.Modifiers[i].effects.Count; e++)
                        {
                            if(BattleStats.Modifiers[i].effects[e] is PlayInjuredAnimWhileIdle)
                            {
                                var playAnim = (PlayInjuredAnimWhileIdle)BattleStats.Modifiers[i].effects[e];
                                if(BattleStats.Modifiers.Contains(playAnim.Modifier))
                                    if(Animations.ContainsKey(BattlerAnimType.Injured))
                                    {
                                        Sprite.ChangeTexture2D(Animations[BattlerAnimType.Injured], true);
                                        Sprite.SetInterval(140);
                                        currentAnim = at;
                                        return;
                                    }
                            }
                        }
                    }
                    Sprite.SetInterval(140);
                }
                else
                    Sprite.ChangeLooping(true);
                currentAnim = at;
            }
            if(at == BattlerAnimType.Injured && !Animations.ContainsKey(BattlerAnimType.Injured))
            {
                Sprite.ChangeTexture2D(Animations[BattlerAnimType.Idle], true);
                Sprite.ChangeLooping(true);
            }
        }
        public void PlaySound(Random random, Audio audio, BattlerAnimType at)
        {
            if (Sounds.ContainsKey(at))
            {
                float pan = 0f;
                if (BattleStats.team == Team.Player)
                    pan = -0.5f;
                else
                    pan = 0.5f;
                var sounds = Sounds[at].Split('|');
                audio.PlaySound(sounds[random.Next(0, sounds.Length)], true, pan);
            }
        }
        public void Draw(SpriteBatch spriteBatch,Color color)
        {
            Sprite.Draw(spriteBatch,color);
        }
        public void AlterBounds(Bounds b)
        {
            Bounds = b;
        }
        public void SetTeam(Team t)
        {
            BattleStats.team = t;
        }      
        public void OnBeginTurn()
        {
            if(inOrder && BattleStats.canAct)
            {
                actionNumber += 1;
            }
            for (int x = 0; x < BattleStats.Modifiers.Count; x++)
            {
                if (BattleStats.Modifiers[x].turnsLeft == 0)
                {
                    Console.WriteLine(Name + " loses " + BattleStats.Modifiers[x].Name);
                    BattleStats.Modifiers.RemoveAt(x);
                    x--;
                    BattleStats.RecalculateStats();
                    break;
                }
                if (BattleStats.Modifiers[x].effectPerTurn != null)
                    BattleStats.Modifiers[x].effectPerTurn.DoTick(BattleStats);
                if (BattleStats.Modifiers[x].turnsLeft != -1)
                    BattleStats.Modifiers[x].turnsLeft--;
            }
        }
    }
}
    
