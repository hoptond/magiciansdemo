using System;
using System.Collections.Generic;
using Microsoft.Xna.Framework;
using System.IO;
using Microsoft.Xna.Framework.Content;
using Microsoft.Xna.Framework.Graphics;

namespace Magicians
{
    public enum WalkerState
    {
        None,
        Standing,
        Walking,
        Running,
        Talking,
        Custom
    }

    class Walker : Entity
    {
        public Mover Mover { get; private set; }
        public WalkerState walkerState { get; private set; }
        public SortedList<Directions, Texture2D> StandingSprites = new SortedList<Directions, Texture2D>();
        public SortedList<Directions, Texture2D> WalkingSprites = new SortedList<Directions, Texture2D>();
        public SortedList<Directions, Texture2D> RunningSprites = new SortedList<Directions, Texture2D>();
        public SortedList<Directions, Texture2D> TalkingSprites = new SortedList<Directions, Texture2D>();
        public bool playingCustomAnim;
        public string DisplayName;
        public WalkerBehaviour Behaviour { get; set; }
        public int baseWalkerInterval { get; set; }

        public Walker(int id, string name, Point pos, int bWI)
            : base(id, name, pos)
        {
            baseWalkerInterval = bWI;
            SetMover(0);
        }

        public override void Update(GameTime gameTime)
        {
            if (playingCustomAnim)
            {
                if (Sprite.ReachedEnd)
                {
                    playingCustomAnim = false;
                    ChangeWalkerSpriteTexture();
                    Sprite.SetInterval(160);
                }
            }
            List<Point> list = null;
            if (Behaviour != null)
            {
                Behaviour.Update(gameTime);
                list = Behaviour.GetMovement();
            }
            GetMovement(list);
            if (Mover.Movement == Vector2.Zero && walkerState != WalkerState.Talking && !playingCustomAnim)
                ChangeWalkerState(WalkerState.Standing);
            if (Mover.Movement != Vector2.Zero && walkerState == WalkerState.Standing)
                ChangeWalkerState(WalkerState.Walking);
            if (Mover.Movement != Vector2.Zero && walkerState == WalkerState.Running)
                ChangeWalkerSpriteTexture();

            Mover.Update(gameTime);
            ChangeWalkerSpriteTexture();
            if (Mover.Target != Point.Zero)
            {
                if (Mover.Target == this.Position)
                {
                    if (Mover.Waypoints.Count > 0)
                    {
                        Mover.Waypoints.RemoveAt(0);
                        if (Mover.Waypoints.Count > 0)
                            Mover.SetTarget(Mover.Waypoints[0]);
                        else
                            Mover.SetTarget(Point.Zero);
                    }
                }
            }
            base.Update(gameTime);
        }
        public override void Draw(SpriteBatch spriteBatch)
        {
            if (Behaviour is IViewTextureGetter)
            {
                var behav = (IViewTextureGetter)Behaviour;
                spriteBatch.Draw(behav.GetViewTexture(Mover.direction), behav.GetDrawRectangle(Mover.direction), null, Color.White, 0, Vector2.Zero, SpriteEffects.None, 0.6f);
            }
            base.Draw(spriteBatch);
        }
        public void LoadWalkerSprites(string s, ContentManager Content, TextureLoader TextureLoader)
        {
            StandingSprites.Clear();
            WalkingSprites.Clear();
            TalkingSprites.Clear();
            RunningSprites.Clear();
            char seperator = Path.DirectorySeparatorChar;
            string directory = Content.RootDirectory + seperator + "Sprites" + seperator + "World" + seperator + "Entity" + seperator + "Walker" + seperator + s;
            Sprite.SetGraphicsDir("\\Sprites\\World\\Entity\\Walker\\" + s);
            walkerState = WalkerState.Standing;
            if (!Directory.Exists(directory))
            {
                s = "Template";
                directory = Content.RootDirectory + seperator + "Sprites" + seperator + "World" + seperator + "Entity" + seperator + "Walker" + seperator + s;
            }
            if (Directory.Exists(directory + seperator + "Standing"))
            {
                StandingSprites.Add(Directions.Up, TextureLoader.RequestTexture("Sprites\\World\\Entity\\Walker\\" + s + "\\Standing\\up"));
                StandingSprites.Add(Directions.UpRight, TextureLoader.RequestTexture("Sprites\\World\\Entity\\Walker\\" + s + "\\Standing\\upright"));
                StandingSprites.Add(Directions.Right, TextureLoader.RequestTexture("Sprites\\World\\Entity\\Walker\\" + s + "\\Standing\\right"));
                StandingSprites.Add(Directions.DownRight, TextureLoader.RequestTexture("Sprites\\World\\Entity\\Walker\\" + s + "\\Standing\\downright"));
                StandingSprites.Add(Directions.Down, TextureLoader.RequestTexture("Sprites\\World\\Entity\\Walker\\" + s + "\\Standing\\down"));
                StandingSprites.Add(Directions.DownLeft, TextureLoader.RequestTexture("Sprites\\World\\Entity\\Walker\\" + s + "\\Standing\\downleft"));
                StandingSprites.Add(Directions.Left, TextureLoader.RequestTexture("Sprites\\World\\Entity\\Walker\\" + s + "\\Standing\\left"));
                StandingSprites.Add(Directions.UpLeft, TextureLoader.RequestTexture("Sprites\\World\\Entity\\Walker\\" + s + "\\Standing\\upleft"));
            }
            if (Directory.Exists(directory + seperator + "Walking"))
            {
                WalkingSprites.Add(Directions.Up, TextureLoader.RequestTexture("Sprites\\World\\Entity\\Walker\\" + s + "\\Walking\\up"));
                WalkingSprites.Add(Directions.UpRight, TextureLoader.RequestTexture("Sprites\\World\\Entity\\Walker\\" + s + "\\Walking\\upright"));
                WalkingSprites.Add(Directions.Right, TextureLoader.RequestTexture("Sprites\\World\\Entity\\Walker\\" + s + "\\Walking\\right"));
                WalkingSprites.Add(Directions.DownRight, TextureLoader.RequestTexture("Sprites\\World\\Entity\\Walker\\" + s + "\\Walking\\downright"));
                WalkingSprites.Add(Directions.Down, TextureLoader.RequestTexture("Sprites\\World\\Entity\\Walker\\" + s + "\\Walking\\down"));
                WalkingSprites.Add(Directions.DownLeft, TextureLoader.RequestTexture("Sprites\\World\\Entity\\Walker\\" + s + "\\Walking\\downleft"));
                WalkingSprites.Add(Directions.Left, TextureLoader.RequestTexture("Sprites\\World\\Entity\\Walker\\" + s + "\\Walking\\left"));
                WalkingSprites.Add(Directions.UpLeft, TextureLoader.RequestTexture("Sprites\\World\\Entity\\Walker\\" + s + "\\Walking\\upleft"));
            }
            if (Directory.Exists(directory + seperator + "Running"))
            {
                RunningSprites.Add(Directions.Up, TextureLoader.RequestTexture("Sprites\\World\\Entity\\Walker\\" + s + "\\Running\\up"));
                RunningSprites.Add(Directions.UpRight, TextureLoader.RequestTexture("Sprites\\World\\Entity\\Walker\\" + s + "\\Running\\upright"));
                RunningSprites.Add(Directions.Right, TextureLoader.RequestTexture("Sprites\\World\\Entity\\Walker\\" + s + "\\Running\\right"));
                RunningSprites.Add(Directions.DownRight, TextureLoader.RequestTexture("Sprites\\World\\Entity\\Walker\\" + s + "\\Running\\downright"));
                RunningSprites.Add(Directions.Down, TextureLoader.RequestTexture("Sprites\\World\\Entity\\Walker\\" + s + "\\Running\\down"));
                RunningSprites.Add(Directions.DownLeft, TextureLoader.RequestTexture("Sprites\\World\\Entity\\Walker\\" + s + "\\Running\\downleft"));
                RunningSprites.Add(Directions.Left, TextureLoader.RequestTexture("Sprites\\World\\Entity\\Walker\\" + s + "\\Running\\left"));
                RunningSprites.Add(Directions.UpLeft, TextureLoader.RequestTexture("Sprites\\World\\Entity\\Walker\\" + s + "\\Running\\upleft"));
            }
            if (Directory.Exists(directory + seperator + "Talking"))
            {
                TalkingSprites.Add(Directions.Up, TextureLoader.RequestTexture("Sprites\\World\\Entity\\Walker\\" + s + "\\Talking\\up"));
                TalkingSprites.Add(Directions.UpRight, TextureLoader.RequestTexture("Sprites\\World\\Entity\\Walker\\" + s + "\\Talking\\upright"));
                TalkingSprites.Add(Directions.Right, TextureLoader.RequestTexture("Sprites\\World\\Entity\\Walker\\" + s + "\\Talking\\right"));
                TalkingSprites.Add(Directions.DownRight, TextureLoader.RequestTexture("Sprites\\World\\Entity\\Walker\\" + s + "\\Talking\\downright"));
                TalkingSprites.Add(Directions.Down, TextureLoader.RequestTexture("Sprites\\World\\Entity\\Walker\\" + s + "\\Talking\\down"));
                TalkingSprites.Add(Directions.DownLeft, TextureLoader.RequestTexture("Sprites\\World\\Entity\\Walker\\" + s + "\\Talking\\downleft"));
                TalkingSprites.Add(Directions.Left, TextureLoader.RequestTexture("Sprites\\World\\Entity\\Walker\\" + s + "\\Talking\\left"));
                TalkingSprites.Add(Directions.UpLeft, TextureLoader.RequestTexture("Sprites\\World\\Entity\\Walker\\" + s + "\\Talking\\upleft"));
            }
            if (Mover != null)
            {
                if (Mover.direction == Directions.None)
                {
                    Mover.ChangeDirection(Directions.Down);
                }
            }
            ChangeWalkerSpriteTexture();
            Sprite.SetSpriteSize();
        }
        public void SetMover(int speed)
        {
            Mover = new Mover(this, speed, Mover.MovementType.Directional);
        }
        void GetMovement(List<Point> list)
        {
            if (Mover.Waypoints.Count == 0)
            {
                if (list == null)
                    return;
                if (list.Count == 0)
                    return;
                if (list[0] != Point.Zero)
                {
                    Mover.Waypoints.AddRange(list);
                    Mover.SetTarget(Mover.Waypoints[0]);
                }
            }
            else
            {
                if (Mover.Target == Point.Zero)
                    Mover.SetTarget(Mover.Waypoints[0]);
            }
            if (Mover.Target == Point.Zero)
                return;
            var HorzDistance = (int)MathHelper.Distance(Position.X, Mover.Target.X);
            var VertDistance = (int)MathHelper.Distance(Position.Y, Mover.Target.Y);
            Mover.ChangeMovement(Directions.None);
            if (Position == Mover.Target)
                return;
            while (Mover.Movement == Vector2.Zero && Mover.turnTimer <= 0)
            {
                if (HorzDistance > VertDistance)
                {
                    if (Mover.Target.X < this.Position.X && Mover.Target.Y == this.Position.Y)
                        Mover.ChangeMovement(Magicians.Directions.Left);
                    if (Mover.Target.X > this.Position.X && Mover.Target.Y == this.Position.Y)
                        Mover.ChangeMovement(Magicians.Directions.Right);
                }
                if (Mover.Movement != Vector2.Zero)
                {
                    break;
                }
                if (VertDistance > HorzDistance)
                {
                    if (Mover.Target.X == this.Position.X && Mover.Target.Y > this.Position.Y)
                        Mover.ChangeMovement(Magicians.Directions.Down);
                    if (Mover.Target.X == this.Position.X && Mover.Target.Y < this.Position.Y)
                        Mover.ChangeMovement(Magicians.Directions.Up);
                }
                if (Mover.Movement != Vector2.Zero)
                {
                    break;
                }
                if (Mover.Target.X > this.Position.X && Mover.Target.Y < this.Position.Y)
                    Mover.ChangeMovement(Magicians.Directions.UpRight);
                if (Mover.Target.X < this.Position.X && Mover.Target.Y < this.Position.Y)
                    Mover.ChangeMovement(Magicians.Directions.UpLeft);
                if (Mover.Target.X < this.Position.X && Mover.Target.Y > this.Position.Y)
                    Mover.ChangeMovement(Magicians.Directions.DownLeft);
                if (Mover.Target.X > this.Position.X && Mover.Target.Y > this.Position.Y)
                    Mover.ChangeMovement(Magicians.Directions.DownRight);
            }
        }
        void ChangeWalkerSpriteTexture()
        {
            if (playingCustomAnim && walkerState == WalkerState.Standing)
                return;
            if (Mover.direction == Directions.None)
                Mover.ChangeDirection(Directions.Down);
            switch (walkerState)
            {
                case (WalkerState.Standing):
                    {
                        Sprite.SetInterval(baseWalkerInterval);
                        if (Sprite.SpriteSheet != null)
                        {
                            if (Sprite.SpriteSheet.Width < (Sprite.CurrentFrame * Sprite.SpriteSize.X))
                                Sprite.CurrentFrame = 0;
                        }
                        if (StandingSprites.Count != 0 && Mover.direction != Directions.None)
                        {
                            Sprite.ChangeTexture2D(StandingSprites[Mover.direction]);
                        }
                        break;
                    }
                case (WalkerState.Walking):
                    {
                        if (Mover.Speed == 1)
                            Sprite.SetInterval(baseWalkerInterval * 1.95f);
                        else
                            Sprite.SetInterval(baseWalkerInterval);
                        if (WalkingSprites.Count != 0)
                        {
                            Sprite.ChangeTexture2D(WalkingSprites[Mover.direction]);
                        }
                        break;
                    }
                case (WalkerState.Running):
                    {
                        Sprite.SetInterval(baseWalkerInterval * 0.7f);
                        if (RunningSprites.Count != 0)
                        {
                            Sprite.ChangeTexture2D(RunningSprites[Mover.direction]);
                        }
                        else
                            Sprite.ChangeTexture2D(WalkingSprites[Mover.direction]);
                        break;
                    }
                case (WalkerState.Talking):
                    {
                        Sprite.SetInterval(baseWalkerInterval);
                        if (TalkingSprites.Count != 0)
                        {
                            Sprite.ChangeTexture2D(TalkingSprites[Mover.direction], false);
                        }
                        else
                            Sprite.ChangeTexture2D(StandingSprites[Mover.direction]);
                        break;
                    }
            }
        }
        public void ChangeWalkerState(WalkerState state)
        {
            walkerState = state;
            ChangeWalkerSpriteTexture();
        }
    }
}
