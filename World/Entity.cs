using System;
using System.Linq;
using System.Text;
using System.Collections.Generic;
using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;
using Microsoft.Xna.Framework.Content;
using System.IO;

namespace Magicians
{
	public enum WalkerState { None, Standing, Walking, Running, Talking, Custom }

	class Entity
	{
		public int ID { get; protected set; }
		public string Name { get; protected set; }
		public Point Position { get; protected set; }
		public Sprite Sprite { get; protected set; }
		public Bounds Bounds { get; set; }
		public SortedList<string, List<IEvent>> Events { get; private set; }
		public IEntityBehaviour EntBehaviour { get; set; }
		public bool HasInteractableEvents { get; private set; }
		public bool HasPlayerCollideEvents { get; private set; }
		public bool HasEntityCollideEvents { get; private set; }
		public bool HasSpellCastEvents { get; set; }
		public bool HasPartyCollideEvents { get; private set; }
		public bool HasSpecialEvents { get; private set; }
		public bool StaticBattler;
		public Entity(int id, string name, Point pos)
		{
			ID = id;
			Name = name;
			Position = pos;
		}
		public void ChangePosition(Point point)
		{
			Position = point;
		}
		public virtual void Update(GameTime gameTime)
		{
			if (EntBehaviour != null)
				EntBehaviour.Update(gameTime);
			if (Bounds != null)
				Bounds.Update();
			if (Sprite != null)
			{
				Sprite.Update(gameTime);
			}
		}
		public virtual void Draw(SpriteBatch spriteBatch)
		{
			if (Sprite != null)
			{
				Sprite.ChangeDrawnPosition(Position);
				Sprite.Draw(spriteBatch);
			}
		}
		public void SetSprite(TextureLoader TextureLoader, string filename, Point size, Sprite.OriginType ot)
		{
			Sprite = new Sprite(TextureLoader, filename, Position, 0.5f, size, ot);
		}
		public void SetBounds(Point size, Point offset, bool PassThrough)
		{
			Bounds = new Bounds(this, this.Position, size.X, size.Y, PassThrough, offset);
		}
		public void AddEvents(string key, List<IEvent> events)
		{
			if (Events == null)
				Events = new SortedList<string, List<IEvent>>();
			if (events == null)
				return;
			switch (key)
			{
				case "INTERACT": HasInteractableEvents = true; break;
				case "COLLIDE": HasPlayerCollideEvents = true; break;
				case "SPELL": HasSpellCastEvents = true; break;
				case "COLLIDEPARTY": HasPartyCollideEvents = true; break;
				case "SPECIAL": HasSpecialEvents = true; break;
			}
			if (key.StartsWith("INTERACT-"))
				HasEntityCollideEvents = true;
			if (!Events.ContainsKey(key))
				Events.Add(key, events);
		}
		public List<IEvent> GetEvents(string key)
		{
			try
			{
				return Events[key];
			}
			catch
			{
				return null;
			}
		}
		public string GetInteractEntityName()
		{
			foreach (string s in Events.Keys)
			{
				if (s.StartsWith("INTERACT-"))
				{
					return s.Split('-')[1];
				}
			}
			return "";
		}
	}

	class Walker : Entity
	{
		Map map;
		public Mover Mover { get; private set; }
		public WalkerState walkerState { get; private set; }
		public SortedList<Directions, Texture2D> StandingSprites = new SortedList<Directions, Texture2D>();
		public SortedList<Directions, Texture2D> WalkingSprites = new SortedList<Directions, Texture2D>();
		public SortedList<Directions, Texture2D> RunningSprites = new SortedList<Directions, Texture2D>();
		public SortedList<Directions, Texture2D> TalkingSprites = new SortedList<Directions, Texture2D>();
		public bool playingCustomAnim;
		public string DisplayName;
		public IWalkerBehaviour Behaviour { get; set; }
		public int baseWalkerInterval { get; set; }

		public Walker(int id, string name, Point pos, Map m, int bWI)
			: base(id, name, pos)
		{
			map = m;
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

			if (Behaviour != null)
				Behaviour.Update(gameTime);
			GetMovement();
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
			if (Behaviour is EyeRotate)
			{
				var behav = (EyeRotate)Behaviour;
				switch (Mover.direction)
				{
					case Directions.Up: spriteBatch.Draw(map.game.TextureLoader.RequestTexture("Sprites\\World\\Effects\\ViewTriangleU"), new Rectangle((int)behav.view.A.X - 95, (int)behav.view.A.Y - 192, 190, 192), null, Color.White, 0, Vector2.Zero, SpriteEffects.None, 0.6f); break;
					case Directions.Left: spriteBatch.Draw(map.game.TextureLoader.RequestTexture("Sprites\\World\\Effects\\ViewTriangleL"), new Rectangle((int)behav.view.A.X - 190, (int)behav.view.A.Y - 96, 190, 196), null, Color.White, 0, Vector2.Zero, SpriteEffects.None, 0.6f); break;
					case Directions.Down: spriteBatch.Draw(map.game.TextureLoader.RequestTexture("Sprites\\World\\Effects\\ViewTriangleD"), new Rectangle((int)behav.view.A.X - 95, (int)behav.view.A.Y, 190, 192), null, Color.White, 0, Vector2.Zero, SpriteEffects.None, 0.6f); break;
					case Directions.Right: spriteBatch.Draw(map.game.TextureLoader.RequestTexture("Sprites\\World\\Effects\\ViewTriangleR"), new Rectangle((int)behav.view.A.X, (int)behav.view.A.Y - 96, 190, 192), null, Color.White, 0, Vector2.Zero, SpriteEffects.None, 0.6f); break;

					case Directions.UpRight: spriteBatch.Draw(map.game.TextureLoader.RequestTexture("Sprites\\World\\Effects\\ViewTriangleUR"), new Rectangle((int)behav.view.A.X, (int)behav.view.A.Y - 202, 202, 202), null, Color.White, 0, Vector2.Zero, SpriteEffects.None, 0.6f); break;
					case Directions.DownRight: spriteBatch.Draw(map.game.TextureLoader.RequestTexture("Sprites\\World\\Effects\\ViewTriangleDR"), new Rectangle((int)behav.view.A.X, (int)behav.view.A.Y, 202, 202), null, Color.White, 0, Vector2.Zero, SpriteEffects.None, 0.6f); break;
					case Directions.DownLeft: spriteBatch.Draw(map.game.TextureLoader.RequestTexture("Sprites\\World\\Effects\\ViewTriangleDL"), new Rectangle((int)behav.view.A.X - 202, (int)behav.view.A.Y, 202, 202), null, Color.White, 0, Vector2.Zero, SpriteEffects.None, 0.6f); break;
					case Directions.UpLeft: spriteBatch.Draw(map.game.TextureLoader.RequestTexture("Sprites\\World\\Effects\\ViewTriangleUL"), new Rectangle((int)behav.view.A.X - 202, (int)behav.view.A.Y - 202, 202, 202), null, Color.White, 0, Vector2.Zero, SpriteEffects.None, 0.6f); break;
				}
			}
			if (Behaviour is Patrol)
			{
				var behav = (Patrol)Behaviour;
				if (behav.canSee)
				{
					switch (Mover.direction)
					{
						case Directions.Up: spriteBatch.Draw(map.game.TextureLoader.RequestTexture("Sprites\\World\\Effects\\ViewTriangleU"), new Rectangle((int)behav.view.A.X - 95, (int)behav.view.A.Y - 192, 190, 192), null, Color.White, 0, Vector2.Zero, SpriteEffects.None, 0.6f); break;
						case Directions.Left: spriteBatch.Draw(map.game.TextureLoader.RequestTexture("Sprites\\World\\Effects\\ViewTriangleL"), new Rectangle((int)behav.view.A.X - 190, (int)behav.view.A.Y - 96, 190, 196), null, Color.White, 0, Vector2.Zero, SpriteEffects.None, 0.6f); break;
						case Directions.Down: spriteBatch.Draw(map.game.TextureLoader.RequestTexture("Sprites\\World\\Effects\\ViewTriangleD"), new Rectangle((int)behav.view.A.X - 95, (int)behav.view.A.Y, 190, 192), null, Color.White, 0, Vector2.Zero, SpriteEffects.None, 0.6f); break;
						case Directions.Right: spriteBatch.Draw(map.game.TextureLoader.RequestTexture("Sprites\\World\\Effects\\ViewTriangleR"), new Rectangle((int)behav.view.A.X, (int)behav.view.A.Y - 96, 190, 192), null, Color.White, 0, Vector2.Zero, SpriteEffects.None, 0.6f); break;

						case Directions.UpRight: spriteBatch.Draw(map.game.TextureLoader.RequestTexture("Sprites\\World\\Effects\\ViewTriangleUR"), new Rectangle((int)behav.view.A.X, (int)behav.view.A.Y - 202, 202, 202), null, Color.White, 0, Vector2.Zero, SpriteEffects.None, 0.6f); break;
						case Directions.DownRight: spriteBatch.Draw(map.game.TextureLoader.RequestTexture("Sprites\\World\\Effects\\ViewTriangleDR"), new Rectangle((int)behav.view.A.X, (int)behav.view.A.Y, 202, 202), null, Color.White, 0, Vector2.Zero, SpriteEffects.None, 0.6f); break;
						case Directions.DownLeft: spriteBatch.Draw(map.game.TextureLoader.RequestTexture("Sprites\\World\\Effects\\ViewTriangleDL"), new Rectangle((int)behav.view.A.X - 202, (int)behav.view.A.Y, 202, 202), null, Color.White, 0, Vector2.Zero, SpriteEffects.None, 0.6f); break;
						case Directions.UpLeft: spriteBatch.Draw(map.game.TextureLoader.RequestTexture("Sprites\\World\\Effects\\ViewTriangleUL"), new Rectangle((int)behav.view.A.X - 202, (int)behav.view.A.Y - 202, 202, 202), null, Color.White, 0, Vector2.Zero, SpriteEffects.None, 0.6f); break;
					}
				}
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
		void GetMovement()
		{
			if (Mover.Waypoints.Count == 0)
			{
				var list = new List<Point>();
				if (Behaviour != null)
					list = Behaviour.GetMovement(map);
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
