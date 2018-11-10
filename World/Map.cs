using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Content;
using Microsoft.Xna.Framework.Graphics;
using Microsoft.Xna.Framework.Input;
using System;
using System.Xml;
using System.Xml.Linq;
using System.Collections.Generic;
using System.IO;
using System.IO.Compression;
using System.Globalization;
using System.Text.RegularExpressions;


namespace Magicians
{
	class Map : IScene
	{
		public Game game { get; private set; }
		XElement mapFile;
		IEnumerable<XElement> fileEntities; //the objectgroup containing the details for entities
		SortedList<string, Tileset> Tilesets = new SortedList<string, Tileset>();
		SortedList<string, Layer> Layers = new SortedList<string, Layer>();
		public List<Entity> Entities { get; set; }
		public List<SpriteEffect> effects = new List<SpriteEffect>();
		public List<WorldOverlay> WorldOverlays = new List<WorldOverlay>();
		public SortedList<string, Point> Waypoints = new SortedList<string, Point>();
		List<Line> Bounds = new List<Line>();
		List<Line> NonFlyingBounds = new List<Line>();
		public int shaderNumber = -1;

		public EventManager EventManager { get; private set; }

		public string DisplayName { get; private set; }
		public string AreaMapName { get; private set; }

		public string FleeDungeonPoint { get; private set; }
		public string FleeDungeonMap { get; private set; }
		public bool Moved { get; private set; }
		public bool CanMovePlayer { get; set; }

		string MusicFile;

		public int Width, Height;
		public int TileWidth, TileHeight;
		public string Filename { get; private set; }

		public bool Paused;

		Point upperLeft;
		Point bottomRight;
		public int NodeGap { get; private set; }
		int width;
		int height;
		Node[,] nodes;
		List<Node> lastNodes = new List<Node>();

		bool makeAutoSave = true;


		Rectangle interactBox = Rectangle.Empty;
		Texture2D MapBorder;
		Sprite interactHighlight;
		List<IEvent> mapEvents = new List<IEvent>();
		public List<Walker> partyEntities = new List<Walker>();
		List<Walker> Enemies = new List<Walker>();
		List<EnemySpawn> EnemySpawns = new List<EnemySpawn>();

		float walkSoundInterval = 1;
		float walkSoundTimer;
		List<KeyValuePair<Rectangle, string>> walkSoundRecs;

		int walkSpeed = 3;
		int runSpeed = 6;
		int sneakSpeed = 1;

		public void Update(GameTime gameTime)
		{
			if (!Paused)
			{
				interactBox = getInteractBox();
				//Check for input
				GetPlayerInput();
				//update entities
				for (int i = 1; i < Entities.Count; i++)
				{
					if (EventManager.Events.Count == 0)
					{
						if (Entities[i].HasInteractableEvents)
						{
							if (interactBox.Intersects(Entities[i].Bounds.Box) && game.Input.IsKeyReleased(game.settings.interactKey) && EventManager.Events.Count == 0 && !EventManager.hadRecentEvents)
							{
								for (int x = 0; x < partyEntities.Count; x++)
								{
									partyEntities[x].Mover.ChangeMovement(Directions.None);
									partyEntities[x].ChangeWalkerState(WalkerState.Standing);
									partyEntities[x].Mover.SetTarget(Point.Zero);
									partyEntities[x].Mover.Waypoints.Clear();
									partyEntities[x].Mover.ChangeSpeed(walkSpeed);
								}
								if (Entities[i] is Walker)
								{
									FaceEntity((Walker)Entities[i], Entities[1]);
								}
								for (int e = 0; e < Enemies.Count; e++)
								{
									Enemies[e].Mover.ChangeMovement(Directions.None);
									Enemies[e].Mover.SetTarget(Point.Zero);
								}
								EventManager.SetEvents(Entities[i].GetEvents("INTERACT"));
								EventManager.DoEvent();
								if (Entities.Count >= i || Entities.Count < i)
									goto EndOfLoop;
								continue;
							}
						}
						if (Entities[i].HasPartyCollideEvents)
						{
							for (int p = 0; p < partyEntities.Count; p++)
							{
								if (partyEntities[p].Bounds.Box.Intersects(Entities[i].Bounds.Box))
								{
									for (int x = 0; x < partyEntities.Count; x++)
									{
										partyEntities[x].Mover.ChangeMovement(Directions.None);
										partyEntities[x].ChangeWalkerState(WalkerState.Standing);
										partyEntities[x].Mover.SetTarget(Point.Zero);
										partyEntities[x].Mover.Waypoints.Clear();
										partyEntities[x].Mover.ChangeSpeed(walkSpeed);
									}
									if (Entities[i] is Walker)
									{
										var walker = (Walker)Entities[i];
										walker.Mover.SetTarget(Point.Zero);
										walker.ChangeWalkerState(WalkerState.Standing);
										walker.Mover.ChangeMovement(Directions.None);
									}
									for (int e = 0; e < Enemies.Count; e++)
									{
										Enemies[e].Mover.ChangeMovement(Directions.None);
										Enemies[e].Mover.SetTarget(Point.Zero);
									}
									if (Entities[i].StaticBattler)
										game.staticbattler = Entities[i].Name;
									EventManager.SetEvents(Entities[i].GetEvents("COLLIDEPARTY"));
									EventManager.DoEvent();
									if (Entities.Count >= i || Entities.Count < i)
										goto EndOfLoop;
									continue;
								}
							}
						}
						if (Entities[i].HasPlayerCollideEvents)
						{
							for (int p = 0; p < 1; p++)
							{
								if (partyEntities[p].Bounds.Box.Intersects(Entities[i].Bounds.Box))
								{
									for (int x = 0; x < partyEntities.Count; x++)
									{
										partyEntities[x].Mover.ChangeMovement(Directions.None);
										partyEntities[x].ChangeWalkerState(WalkerState.Standing);
										partyEntities[x].Mover.SetTarget(Point.Zero);
										partyEntities[x].Mover.Waypoints.Clear();
										partyEntities[x].Mover.ChangeSpeed(walkSpeed);
									}
									if (Entities[i] is Walker)
									{
										var walker = (Walker)Entities[i];
										walker.Mover.SetTarget(Point.Zero);
										walker.ChangeWalkerState(WalkerState.Standing);
										walker.Mover.ChangeMovement(Directions.None);
									}
									for (int e = 0; e < Enemies.Count; e++)
									{
										Enemies[e].Mover.ChangeMovement(Directions.None);
										Enemies[e].Mover.SetTarget(Point.Zero);
									}
									EventManager.SetEvents(Entities[i].GetEvents("COLLIDE"));
									EventManager.DoEvent();
									if (Entities.Count >= i || Entities.Count < i)
										goto EndOfLoop;
									continue;
								}
							}
						}
						if (Entities[i].HasEntityCollideEvents)
						{
							if (Entities[i] is Walker)
							{
								var walker = (Walker)Entities[i];
								if (walker.Behaviour is EnemyBehaviour || walker.Behaviour is Patrol || walker.Behaviour == null)
								{
									var s = walker.GetInteractEntityName();
									var interactEntities = new List<Entity>();
									for (int search = 0; search < Entities.Count; search++)
									{
										if (Entities[search].Name.StartsWith(s))
										{
											interactEntities.Add(Entities[search]);
										}
									}
									if (interactEntities.Count != 0)
									{
										for (int ie = 0; ie < interactEntities.Count; ie++)
										{
											if (Entities[i].Bounds.Box.Intersects(interactEntities[ie].Bounds.Box))
											{
												for (int x = 0; x < partyEntities.Count; x++)
												{
													partyEntities[x].Mover.ChangeMovement(Directions.None);
													partyEntities[x].ChangeWalkerState(WalkerState.Standing);
													partyEntities[x].Mover.SetTarget(Point.Zero);
													partyEntities[x].Mover.Waypoints.Clear();
													partyEntities[x].Mover.ChangeSpeed(walkSpeed);
												}
												walker.Mover.SetTarget(Point.Zero);
												walker.ChangeWalkerState(WalkerState.Standing);
												walker.Mover.ChangeMovement(Directions.None);
												if (interactEntities[ie] is Walker)
												{
													var intWalker = (Walker)interactEntities[ie];
													intWalker.Mover.SetTarget(Point.Zero);
													intWalker.ChangeWalkerState(WalkerState.Standing);
													intWalker.Mover.ChangeMovement(Directions.None);
												}
												for (int e = 0; e < Enemies.Count; e++)
												{
													Enemies[e].Mover.ChangeMovement(Directions.None);
													Enemies[e].Mover.SetTarget(Point.Zero);
												}
												EventManager.SetEvents(Entities[i].GetEvents("INTERACT-" + s), Entities[i], interactEntities[ie]);
												EventManager.DoEvent();
												if (Entities.Count >= i || Entities.Count < i)
													goto EndOfLoop;
												continue;
											}
										}
									}
								}
							}
							else
							{
								var s = Entities[i].GetInteractEntityName();
								var interactEntities = new List<Entity>();
								for (int search = 0; search < Entities.Count; search++)
								{
									if (Entities[search].Name.StartsWith(s))
									{
										interactEntities.Add(Entities[search]);
									}
								}
								if (interactEntities.Count != 0)
								{
									for (int ie = 0; ie < interactEntities.Count; ie++)
									{
										if (Entities[i].Bounds.Box.Intersects(interactEntities[ie].Bounds.Box))
										{
											for (int x = 0; x < partyEntities.Count; x++)
											{
												partyEntities[x].Mover.ChangeMovement(Directions.None);
												partyEntities[x].ChangeWalkerState(WalkerState.Standing);
												partyEntities[x].Mover.SetTarget(Point.Zero);
												partyEntities[x].Mover.Waypoints.Clear();
												partyEntities[x].Mover.ChangeSpeed(walkSpeed);
											}
											for (int e = 0; e < Enemies.Count; e++)
											{
												Enemies[e].Mover.ChangeMovement(Directions.None);
												Enemies[e].Mover.SetTarget(Point.Zero);
											}
											EventManager.SetEvents(Entities[i].GetEvents("INTERACT-" + s), Entities[i], interactEntities[ie]);
											EventManager.DoEvent();
											if (Entities.Count >= i || Entities.Count < i)
												goto EndOfLoop;
											continue;
										}
									}
								}
							}
						}
						if (Entities[i].EntBehaviour is PushableEntity)
						{
							if (Entities[i].Bounds.Box.Intersects(partyEntities[0].Bounds.Box))
							{
								int movement = ((PushableEntity)Entities[i].EntBehaviour).MoveSpeed;
								var pushDirs = new List<Directions>(((PushableEntity)Entities[i].EntBehaviour).Directions);

								Directions dir = partyEntities[0].Mover.direction;
								if (MathHelper.Distance(partyEntities[0].Bounds.Box.Center.Y, Entities[i].Bounds.Box.Center.Y) > MathHelper.Distance(partyEntities[0].Bounds.Box.Center.X, Entities[i].Bounds.Box.Center.X))
								{
									if (partyEntities[0].Bounds.Box.Center.Y > Entities[i].Bounds.Box.Center.Y)
									{
										if (isSpaceFree(Entities[i], new Rectangle(Entities[i].Bounds.Box.X, Entities[i].Bounds.Box.Y - movement, Entities[i].Bounds.Box.Width, Entities[i].Bounds.Box.Height)) && dir == Directions.Up && pushDirs.Contains(Directions.Up))
											Entities[i].ChangePosition(new Point(Entities[i].Position.X, Entities[i].Position.Y - movement));
									}
									else
									{
										if (isSpaceFree(Entities[i], new Rectangle(Entities[i].Bounds.Box.X, Entities[i].Bounds.Box.Y + movement, Entities[i].Bounds.Box.Width, Entities[i].Bounds.Box.Height)) && dir == Directions.Down && pushDirs.Contains(Directions.Down))
											Entities[i].ChangePosition(new Point(Entities[i].Position.X, Entities[i].Position.Y + movement));
									}
								}
								else
								{
									if (partyEntities[0].Bounds.Box.Center.X > Entities[i].Bounds.Box.Center.X)
									{
										if (isSpaceFree(Entities[i], new Rectangle(Entities[i].Bounds.Box.X - movement, Entities[i].Bounds.Box.Y, Entities[i].Bounds.Box.Width, Entities[i].Bounds.Box.Height)) && dir == Directions.Left && pushDirs.Contains(Directions.Left))
											Entities[i].ChangePosition(new Point(Entities[i].Position.X - movement, Entities[i].Position.Y));
									}
									else
									{
										if (isSpaceFree(Entities[i], new Rectangle(Entities[i].Bounds.Box.X + movement, Entities[i].Bounds.Box.Y, Entities[i].Bounds.Box.Width, Entities[i].Bounds.Box.Height)) && dir == Directions.Right && pushDirs.Contains(Directions.Right))
											Entities[i].ChangePosition(new Point(Entities[i].Position.X + movement, Entities[i].Position.Y));
									}
								}
							}
						}
					}
					Entities[i].Update(gameTime);
				EndOfLoop:;
				}
				for (int i = 1; i < Entities.Count; i++)
				{
					if (Entities[i] is Walker)
					{
						var walker = (Walker)Entities[i];
						dislodgeMovement(walker);
					}
				}
				Entities[0].Update(gameTime); //update camera last
				for (int i = 0; i < effects.Count; i++)
				{
					effects[i].Update(gameTime);
					if (effects[i].isFinished)
					{
						effects.RemoveAt(i);
						i--;
					}
				}
				EventManager.Update(gameTime);
				if (walkSoundRecs != null)
				{
					int volume = 75;
					if (partyEntities[0].walkerState == WalkerState.Walking)
						walkSoundInterval = 0.40f;
					if (partyEntities[0].walkerState == WalkerState.Running)
					{
						walkSoundInterval = 0.25f;
						volume = 100;
					}
					if (partyEntities[0].Mover.Speed == sneakSpeed)
					{
						walkSoundInterval = 0.75f;
						volume = 35;
					}
					if (partyEntities[0].walkerState != WalkerState.Standing && partyEntities[0].walkerState != WalkerState.Custom && partyEntities[0].walkerState != WalkerState.Talking)
					{
						walkSoundTimer += (float)gameTime.ElapsedGameTime.TotalSeconds;
						if (walkSoundTimer > walkSoundInterval)
						{
							foreach (KeyValuePair<Rectangle, string> kvp in walkSoundRecs)
							{
								if (kvp.Key.Contains(new Point(partyEntities[0].Bounds.Box.Center.X, partyEntities[0].Bounds.Box.Bottom)))
								{
									var args = kvp.Value.Split('|');
									game.Audio.PlaySound(args[game.randomNumber.Next(0, args.Length)], true, volume);
									break;
								}
							}
							walkSoundTimer = 0;
						}
					}
					else
					{
						walkSoundTimer = 1;
					}
				}
			}
		}
		public void Draw(SpriteBatch spriteBatch)
		{
			var drawHighlight = true;
			setSpriteDepth();
			for (int i = 0; i < Entities.Count; i++)
			{
				Entities[i].Draw(spriteBatch);
				if (drawHighlight)
				{
					if (Entities[i].Bounds != null && Entities[i].HasInteractableEvents && i > 1)
					{
						if (interactBox.Intersects(Entities[i].Bounds.Box))
						{
							if (Entities[i].Sprite != null)
							{
								if (Entities[i] is Walker)
									interactHighlight.ChangeDrawnPosition(new Point(Entities[i].Position.X, Entities[i].Position.Y - (Entities[i].Sprite.SpriteSize.Y / 2)));
								else
									interactHighlight.ChangeDrawnPosition(new Point(Entities[i].Sprite.DrawnBounds.Center.X, Entities[i].Sprite.DrawnBounds.Center.Y));
							}
							else
								interactHighlight.ChangeDrawnPosition(new Point(Entities[i].Bounds.Box.Center.X, Entities[i].Bounds.Box.Center.Y));
							if (EventManager.Events.Count == 0)
							{
								interactHighlight.Draw(spriteBatch);
								drawHighlight = false;
							}
						}
					}
				}
				if (game.debug)
				{
					if (Entities[i].Bounds != null)
						spriteBatch.Draw(game.debugSquare, Entities[i].Bounds.Box, null, Color.Purple, 0, Vector2.Zero, SpriteEffects.None, 0.01f);
					spriteBatch.Draw(game.debugSquare, interactBox, Color.Yellow);
				}

			}
			if (game.debug)
			{
				for (int i = 0; i < Bounds.Count; i++)
				{
					var edge = Bounds[i].End.ToVector2() - Bounds[i].Start.ToVector2();
					// calculate angle to rotate line
					var angle = (float)Math.Atan2(edge.Y, edge.X);
					spriteBatch.Draw(game.debugSquare, new Rectangle(Bounds[i].Start, new Point(Bounds[i].Length(), 1)), null, Color.Green, angle, Vector2.Zero, SpriteEffects.None, 0f);
				}
				spriteBatch.Draw(game.debugSquare, interactBox, Color.Green);
				if (lastNodes.Count > 0)
				{
					for (int i = 0; i < lastNodes.Count; i++)
					{
						if (lastNodes[i].SearchData != null)
						{
							if (lastNodes[i].SearchData.Used)
								spriteBatch.Draw(game.debugSquare, new Rectangle((lastNodes[i].RealLocation.X), (lastNodes[i].RealLocation.Y), 4, 4), null, Color.Gold, 0, Vector2.Zero, SpriteEffects.None, 0.01f);
							else if (lastNodes[i].SearchData.Closed)
								spriteBatch.Draw(game.debugSquare, new Rectangle(lastNodes[i].RealLocation.X, lastNodes[i].RealLocation.Y, 4, 4), null, Color.Blue, 0, Vector2.Zero, SpriteEffects.None, 0.01f);
							else if (lastNodes[i].SearchData.Open)
								spriteBatch.Draw(game.debugSquare, new Rectangle(lastNodes[i].RealLocation.X, lastNodes[i].RealLocation.Y, 4, 4), null, Color.Red, 0, Vector2.Zero, SpriteEffects.None, 0.01f);
							else
								spriteBatch.Draw(game.debugSquare, new Rectangle(lastNodes[i].RealLocation.X, lastNodes[i].RealLocation.Y, 4, 4), null, Color.Green, 0, Vector2.Zero, SpriteEffects.None, 0.01f);
						}
					}
				}
				for (int x = 0; x < width; x++)
				{
					for (int y = 0; y < height; y++)
					{
						if (nodes[x, y].Type != NodeType.Clear)
							spriteBatch.Draw(game.debugSquare, new Rectangle(upperLeft.X + (x * NodeGap), upperLeft.Y + (y * NodeGap), 3, 3), null, Color.Pink, 0, Vector2.Zero, SpriteEffects.None, 0f);
						else
							spriteBatch.Draw(game.debugSquare, new Rectangle(upperLeft.X + (x * NodeGap), upperLeft.Y + (y * NodeGap), 3, 3), null, Color.Turquoise, 0, Vector2.Zero, SpriteEffects.None, 0f);

					}
				}
			}
			for (int i = 0; i < WorldOverlays.Count; i++)
			{
				WorldOverlays[i].Draw(spriteBatch, new Rectangle(0, 0, Width * 64, Height * 64));
			}
			for (int i = 0; i < effects.Count; i++)
			{
				effects[i].Draw(spriteBatch);
			}
			foreach (Layer layers in Layers.Values)
			{
				layers.Draw(spriteBatch, Tilesets.Values, new Rectangle(0, 0, TileWidth * Width, TileHeight * Height), Vector2.Zero, TileWidth, TileHeight);
			}
			if (game.Camera.clampViewport)
			{
				spriteBatch.Draw(MapBorder, new Rectangle(-120, 0, 120, Height * 64), Color.Black);
				spriteBatch.Draw(MapBorder, new Rectangle(Width * 64, 0, 120, Height * 64), Color.Black);
			}
		}
		public void GetPlayerInput()
		{
			if (game.Input.IsKeyReleased(game.settings.mapKey) && EventManager.Events.Count == 0)
				game.OpenUIWindow(new MapWindow(game));
			if (game.Input.IsKeyPressed(Keys.Escape))
			{
				if (EventManager.Events.Count == 0)
				{
					game.OpenUIWindow(new ExitToMenuConfirm(game));
					Paused = true;
				}
			}
		}
		Point getPointFromKeysPressed()
		{
			var moveKeys = new Keys[2];
			moveKeys[0] = game.settings.upKey;
			moveKeys[1] = game.settings.rightKey;
			if (game.Input.AreKeysPressed(moveKeys))
				return new Point(((Walker)Entities[1]).Mover.Speed, -((Walker)Entities[1]).Mover.Speed);
			moveKeys[0] = game.settings.upKey;
			moveKeys[1] = game.settings.leftKey;
			if (game.Input.AreKeysPressed(moveKeys))
				return new Point(-((Walker)Entities[1]).Mover.Speed, -((Walker)Entities[1]).Mover.Speed);
			moveKeys[0] = game.settings.downKey;
			moveKeys[1] = game.settings.rightKey;
			if (game.Input.AreKeysPressed(moveKeys))
				return new Point(((Walker)Entities[1]).Mover.Speed, ((Walker)Entities[1]).Mover.Speed);
			moveKeys[0] = game.settings.downKey;
			moveKeys[1] = game.settings.leftKey;
			if (game.Input.AreKeysPressed(moveKeys))
				return new Point(-((Walker)Entities[1]).Mover.Speed, ((Walker)Entities[1]).Mover.Speed);
			if (game.Input.IsKeyPressed(game.settings.upKey))
				return new Point(0, -((Walker)Entities[1]).Mover.Speed);
			if (game.Input.IsKeyPressed(game.settings.rightKey))
				return new Point(((Walker)Entities[1]).Mover.Speed, 0);
			if (game.Input.IsKeyPressed(game.settings.downKey))
				return new Point(0, ((Walker)Entities[1]).Mover.Speed);
			if (game.Input.IsKeyPressed(game.settings.leftKey))
				return new Point(-((Walker)Entities[1]).Mover.Speed, 0);
			return Point.Zero;
		}
		public Point GetPlayerMovement()
		{
			var point = new Point(0);
			if (CanMovePlayer)
			{
				var moveKeys = new Keys[4];
				moveKeys[0] = game.settings.upKey;
				moveKeys[1] = game.settings.downKey;
				moveKeys[2] = game.settings.leftKey;
				moveKeys[3] = game.settings.rightKey;
				if (!game.Input.AreAnyKeysPressed(moveKeys))
				{
					((Walker)Entities[1]).ChangeWalkerState(WalkerState.Standing);
					((Walker)Entities[1]).Mover.SetTarget(Point.Zero);
					((Walker)Entities[1]).Mover.Waypoints.Clear();
					return new Point(0);
				}
				if (game.Input.IsKeyPressed(game.settings.runKey))
				{
					((Walker)Entities[1]).Mover.ChangeSpeed(runSpeed);
					((Walker)Entities[1]).ChangeWalkerState(WalkerState.Running);
				}
				else if (game.Input.IsKeyPressed(game.settings.sneakKey))
				{
					((Walker)Entities[1]).Mover.ChangeSpeed(sneakSpeed);
					((Walker)Entities[1]).ChangeWalkerState(WalkerState.Walking);
				}
				else
				{
					((Walker)Entities[1]).Mover.ChangeSpeed(walkSpeed);
					((Walker)Entities[1]).ChangeWalkerState(WalkerState.Walking);
				}
				if (game.Input.IsKeyPressed(game.settings.upKey) || game.Input.IsKeyPressed(game.settings.rightKey) || game.Input.IsKeyPressed(game.settings.downKey) || game.Input.IsKeyPressed(game.settings.leftKey))
					Moved = true;
				point = getPointFromKeysPressed();
				var player = (Walker)Entities[1];
				if (point.X != 0 || point.Y != 0)
				{
					if (point.X > 0 && point.Y > 0)
						player.Mover.ChangeDirection(Directions.DownRight);
					if (point.X < 0 && point.Y > 0)
						player.Mover.ChangeDirection(Directions.DownLeft);
					if (point.X == 0 && point.Y > 0)
						player.Mover.ChangeDirection(Directions.Down);
					if (point.X == 0 && point.Y < 0)
						player.Mover.ChangeDirection(Directions.Up);
					if (point.X > 0 && point.Y < 0)
						player.Mover.ChangeDirection(Directions.UpRight);
					if (point.X < 0 && point.Y < 0)
						player.Mover.ChangeDirection(Directions.UpLeft);
					if (point.X > 0 && point.Y == 0)
						player.Mover.ChangeDirection(Directions.Right);
					if (point.X < 0 && point.Y == 0)
						player.Mover.ChangeDirection(Directions.Left);
					bool both = true;
					bool vert = false;
					bool horz = false;
					var bounds_ = new List<Line>();
					var rectangles = new List<Rectangle>();

					bounds_.AddRange(Bounds);
					bounds_.AddRange(NonFlyingBounds);
					for (int i = 2; i < Entities.Count; i++)
					{
						if (Entities[i].Bounds != null)
						{
							if (Entities[i].Bounds.CanPassThrough == false && (Entities[i].EntBehaviour is PushableEntity) == false)
							{
								if (Entities[i].Bounds.Box.Width > 0)
									rectangles.Add(Entities[i].Bounds.Box);
							}
						}
					}
					int x = point.X;
					int y = point.Y;
				SetOffset:
					var rect = new Rectangle(new Point(player.Bounds.Box.X, player.Bounds.Box.Y), new Point(player.Bounds.Box.Width, player.Bounds.Box.Height));
					if (both)
					{
						rect.X += x; rect.Y += y;
					}

					if (vert)
						rect.Y += y;
					if (horz)
						rect.X += x;
					foreach (Rectangle rec in rectangles)
					{
						if (rect.Intersects(rec))
						{
							rect = new Rectangle(new Point(player.Bounds.Box.X, player.Bounds.Box.Y), new Point(player.Bounds.Box.Width, player.Bounds.Box.Width));
							if (both)
							{
								both = false;
								horz = true;
								vert = false;
								goto SetOffset;
							}
							if (horz)
							{
								vert = true;
								horz = false;
								goto SetOffset;
							}
							if (vert)
								vert = false;
							break;
						}
					}
					foreach (Line line in bounds_)
					{
						if (line.End.X != line.Start.X || line.End.Y != line.Start.Y)
						{
							if (Magicians.Bounds.IntersectsLine(rect, line.Start, line.End))
							{
								rect = new Rectangle(new Point(player.Bounds.Box.X, player.Bounds.Box.Y), new Point(player.Bounds.Box.Width, player.Bounds.Box.Width));
								if (both)
								{
									both = false;
									horz = true;
									vert = false;
									goto SetOffset;
								}
								if (horz)
								{
									vert = true;
									horz = false;
									goto SetOffset;
								}
								if (vert)
									vert = false;
								break;
							}
						}
						if (vert)
							point = new Point(0, y);
						if (horz)
							point = new Point(x, 0);
						if (!both && !vert && !horz)
						{
							((Walker)Entities[1]).ChangeWalkerState(WalkerState.Standing);
							((Walker)Entities[1]).Mover.SetTarget(Point.Zero);
							((Walker)Entities[1]).Mover.Waypoints.Clear();
							return new Point(0);
						}
					}
				}
			}
			else
			{
				return Point.Zero;
			}
			return new Point(this.partyEntities[0].Position.X + point.X, this.partyEntities[0].Position.Y + point.Y);
		}
		public List<Point> GetWaypoints(Walker ent, Point target)
		{
			var search = SearchType.Normal;
			///use the flying nodes if the entity is a flying enemy
			if (ent.Behaviour is EnemyBehaviour)
			{
				var behaviour = (EnemyBehaviour)ent.Behaviour;
				if (behaviour.behaviours.Contains(Behaviours.Flying))
					search = SearchType.Flying;
			}
			var start = ent.Position;
			var points = new List<Point>();
			///Determine if we can walk in a (mostly) straight line to our target. if we can, there is no need to work out a path, so we simply return the beginning and the end point;
			if (CanGetToTargetWithoutPathfinding(ent, target))
			{
				points.Add(target);
				return points;
			}
			///if the start or finish does not align to the node grid, we must find the nearest point to each
			///this possibly also causes midmovement spazzing, when the ent first backtracks to the selected node and then continues the path normally
			var Start = new Point(start.X - upperLeft.X, start.Y - upperLeft.Y);
			var End = new Point(target.X - upperLeft.X, target.Y - upperLeft.Y);
			var EndDivisibleByNodeGap = true;
			if (Start.X % NodeGap != 0 || Start.Y % NodeGap != 0)
			{
				Start.X = (int)Math.Round(Start.X / (double)NodeGap, MidpointRounding.ToEven) * NodeGap;
				Start.Y = (int)Math.Round(Start.Y / (double)NodeGap, MidpointRounding.ToEven) * NodeGap;
			}
			if (End.X % NodeGap != 0 || End.Y % NodeGap != 0)
			{
				End.X = (int)Math.Round(End.X / (double)NodeGap, MidpointRounding.ToEven) * NodeGap;
				End.Y = (int)Math.Round(End.Y / (double)NodeGap, MidpointRounding.ToEven) * NodeGap;
				EndDivisibleByNodeGap = false;
			}
			Start = new Point(Start.X / NodeGap, Start.Y / NodeGap);
			End = new Point(End.X / NodeGap, End.Y / NodeGap);
			///if the start or finish is out of the level, we stop the pathfinding as it will crash
			if (Start.X < 0 || Start.Y < 0 || Start.X >= nodes.GetLength(0) || Start.Y >= nodes.GetLength(1) || End.X < 0 || End.Y < 0 || End.X >= nodes.GetLength(0) || End.Y >= nodes.GetLength(1))
			{
				points.Add(target);
				return points;
			}
			//if the start node is set to nonpassable, we move it to the nearest passable node
			//TODO: THIS SOMETIMES PRODUCES NO RESULTS NODES WHEN THERE ARE NODES, OR A WRONG NODE, THUS RESULTING IN GETTING STUCK ON THINGS AND SPAZZING OUT.
			//THE SAME IS TRUE OF THE END NODE CHECK
			if (!PathFinder.IsWalkable(search, nodes[Start.X, Start.Y]))
			{
				var startAdjNodes = PathFinder.GetAdjacentWalkableNodes(nodes, nodes[Start.X, Start.Y], search, true);
				for (int i = 0; i < startAdjNodes.Count; i++)
				{
					if (!CanTravelOverTwoPoints(ent, startAdjNodes[i].RealLocation, start))
					{
						startAdjNodes.RemoveAt(i);
						i--;
					}
				}
				//how did it come to this
				startAdjNodes.Sort((node1, node2) => (Vector2.Distance(node1.RealLocation.ToVector2(), End.ToVector2()).CompareTo(Vector2.Distance(node2.RealLocation.ToVector2(), End.ToVector2()))));
				if (startAdjNodes.Count == 0)
				{
					points.Add(target);
					return points;
				}
				Start = startAdjNodes[0].ArrayLocation;
			}
			//same for the end node
			if (!PathFinder.IsWalkable(search, nodes[End.X, End.Y]))
			{
				var endAdjNodes = PathFinder.GetAdjacentWalkableNodes(nodes, nodes[End.X, End.Y], search, true);
				for (int i = 0; i < endAdjNodes.Count; i++)
				{
					if (!CanTravelOverTwoPoints(ent, endAdjNodes[i].RealLocation, target))
					{
						endAdjNodes.RemoveAt(i);
						i--;
					}
				}
				endAdjNodes.Sort((node1, node2) => (Vector2.Distance(node1.RealLocation.ToVector2(), Start.ToVector2()).CompareTo(Vector2.Distance(node2.RealLocation.ToVector2(), Start.ToVector2()))));
				if (endAdjNodes.Count == 0)
				{
					points.Add(target);
					return points;
				}
				End = endAdjNodes[0].ArrayLocation;
			}
			var temp = new List<Node>();
			var pathFinder = new PathFinder(nodes, nodes[Start.X, Start.Y], nodes[End.X, End.Y], search);
			temp = pathFinder.FindPath();
			if (temp.Count == 0)
			{
				points.Add(target);
				return points;
			}
			for (int i = 0; i < temp.Count; i++)
			{
				if (Vector2.Distance(temp[i].RealLocation.ToVector2(), target.ToVector2()) < NodeGap)
				{
					temp.RemoveAt(i);
					i--;
				}
				points.Add(temp[i].RealLocation);
			}
			//smooth the path
			//from the start point, continue along until we find a point that is directly diagonal to the check point. if we can cut across without bumping into anything, remove all points from the point after
			//the checkpoint to the just before the diagonal, and set this checkpoint to the point we just cut to.
			//if we can't, set the checkpoint to this point and continue onward. the thing ends when the checkpoint hits the end.
			var checkPointIndex = 0;
			var removeRange = 1;
			while (checkPointIndex != points.Count - 1)
			{
				try
				{
					if (checkPointIndex + removeRange >= points.Count)
					{
						checkPointIndex += 1;
						removeRange = checkPointIndex + 1;
						continue;
					}
					if (ArePointsDiagonal(points[checkPointIndex], points[checkPointIndex + removeRange]))
					{
						if (CanTravelOverTwoPoints(ent, points[checkPointIndex], points[checkPointIndex + removeRange]))
						{
							points.RemoveRange(checkPointIndex + 1, removeRange - 1);
							checkPointIndex += 1;
							removeRange = 1;
						}
						else
						{
							checkPointIndex += 1;
							removeRange = 1;
						}
					}
					else
						removeRange++;
				}
				catch
				{

				}

			}
			if (!CanTravelOverTwoPoints(ent, start, points[0]))
			{
				var b = points[0];
				points.RemoveAt(0);
				if (CanTravelOverTwoPoints(ent, new Point(start.X, start.Y), new Point(start.X, b.Y)))
				{
					points.Insert(0, new Point(start.X, b.Y));
					points.Insert(1, new Point(b.X, b.Y));
				}
				else
				{
					points.Insert(0, new Point(b.X, start.Y));
					points.Insert(1, new Point(b.X, b.Y));
				}
			}
			if (game.debug)
			{
				lastNodes.Clear();
				for (int x = 0; x < nodes.GetLength(0); x++)
				{
					for (int y = 0; y < nodes.GetLength(1); y++)
					{
						if (nodes[x, y].SearchData != null)
							lastNodes.Add(nodes[x, y]);
					}
				}
			}
			if (!EndDivisibleByNodeGap)
				points.Add(target);
			return points;
		}
		//if the entity can reach their target without bumping into anything
		public bool CanGetToTargetWithoutPathfinding(Entity e, Point t)
		{
			if (e.Bounds == null)
				return true;
			//if the entity is lined up vertically or horizontally, we only need to make one set of lines. Otherwise, we have to make two.
			var diagonal = false;
			if (e.Position.X != t.X && e.Position.Y != t.Y)
			{
				diagonal = true;
			}
			var s = e.Position;
			var midPoint = t;
			//if diagonal, the midpoint will be set by starting from the end position and incrementing/decrementing both x and y until one hits the start x or y. this will then be set as the mid point
			if (diagonal)
			{
				while (WhileOneIsNotTheOther(t, midPoint))
				{
					if (midPoint.X < t.X)
						midPoint.X += 1;
					if (midPoint.Y < t.Y)
						midPoint.Y += 1;
					if (midPoint.X > t.X)
						midPoint.X -= 1;
					if (midPoint.Y > t.Y)
						midPoint.Y -= 1;
				}
			}
			if (!CanTravelOverTwoPoints(e, e.Position, midPoint))
				return false;
			if (diagonal)
				if (!CanTravelOverTwoPoints(e, midPoint, t))
					return false;
			return true;
		}
		//this is the worst thing you have ever done
		bool WhileOneIsNotTheOther(Point a, Point b)
		{
			if (a.X == b.X)
				return false;
			if (a.Y == b.Y)
				return false;
			return true;
		}
		public bool CanTravelOverTwoPoints(Entity e, Point a, Point b)
		{
			var startRect = new Rectangle(a.X + e.Bounds.offset.X, a.Y + e.Bounds.offset.Y, e.Bounds.Box.Width, e.Bounds.Box.Height);
			var targetRect = new Rectangle(b.X + e.Bounds.offset.X, b.Y + e.Bounds.offset.Y, e.Bounds.Box.Width, e.Bounds.Box.Height);
			var bounds_ = new List<Line>();
			var rectangles = new List<Rectangle>();
			var lines = new Line[4];
			lines[0] = new Line(new Point(startRect.Left + 1, startRect.Top + 1), new Point(targetRect.Left + 1, targetRect.Top + 1)); //top left
			lines[1] = new Line(new Point(startRect.Right - 1, startRect.Top + 1), new Point(targetRect.Right - 1, targetRect.Top + 1));  //top right
			lines[2] = new Line(new Point(startRect.Left + 1, startRect.Bottom - 1), new Point(targetRect.Left + 1, targetRect.Bottom - 1));  //bottom left
			lines[3] = new Line(new Point(startRect.Right - 1, startRect.Bottom - 1), new Point(targetRect.Right - 1, targetRect.Bottom - 1));  //bottom right
			bounds_.AddRange(Bounds);
			bounds_.AddRange(NonFlyingBounds);
			for (int i = 2; i < Entities.Count; i++)
			{
				if (Entities[i].Bounds != null)
				{
					if (Entities[i].Bounds.CanPassThrough == false)
					{
						if (Entities[i] != e)
							if (Entities[i].Bounds.Box.Width > 0)
								rectangles.Add(Entities[i].Bounds.Box);
					}
				}
			}
			for (int i = 0; i < lines.Length; i++)
			{
				foreach (Rectangle rec in rectangles)
				{
					if (Magicians.Bounds.IntersectsLine(rec, lines[i].Start, lines[i].End))
					{
						return false;
					}
				}
				foreach (Line line in bounds_)
				{
					if (Magicians.Bounds.Intersects(lines[i].Start.ToVector2(), lines[i].End.ToVector2(), line.Start.ToVector2(), line.End.ToVector2()))
					{
						return false;
					}
				}
			}
			return true;
		}
		public void FaceEntity(Walker face, Entity target)//causes a walker entity to face another  entity
		{
			if (face == null || target == null)
			{
				return;
			}
			face.Mover.ChangeDirection(GetFaceEntityDirection(face, target));
		}
		public Directions GetFaceEntityDirection(Walker face, Entity target)
		{
			var t = new Point(target.Position.X, target.Position.Y);
			if ((target is Walker) == false)
			{
				if (target.Sprite != null)
				{
					t.X += target.Sprite.SpriteSize.X / 2;
					t.Y += target.Sprite.SpriteSize.Y;
				}
			}
			var f = new Point(face.Position.X, face.Position.Y);
			var radians = (float)Math.Atan2(t.X - f.X, t.Y - f.Y);
			return GetDirectionFromRadians(radians);
		}
		public Directions GetFaceEntityDirection(Walker face, string target)
		{
			var t = new Point(Waypoints[target].X, Waypoints[target].Y);
			var f = new Point(face.Position.X, face.Position.Y);
			f.X += face.Sprite.SpriteSize.X / 2;
			f.Y += face.Sprite.SpriteSize.Y;
			var radians = (float)Math.Atan2(t.X - f.X, t.Y - f.Y);
			return GetDirectionFromRadians(radians);
		}
		Directions GetDirectionFromRadians(float radians)
		{
			if (radians > -3.2 && radians < -2.8 || radians > 2.8 && radians < 3.2f)
				return Directions.Up;
			if (radians > -2.8 && radians < -2.0)
				return Directions.UpLeft;
			if (radians > -2.0 && radians < -1.2)
				return Directions.Left;
			if (radians > -1.2 && radians < -0.4)
				return Directions.DownLeft;
			if (radians > -0.4 && radians < 0.4)
				return Directions.Down;
			if (radians > 0.4 && radians < 1.2)
				return Directions.DownRight;
			if (radians > -1.2 && radians < 2)
				return Directions.Right;
			if (radians > 2 && radians < 2.8)
				return Directions.UpRight;
			return Directions.None;
		}
		public void FaceEntity(Walker face, string targ)//causes a walker entity to face a point in waypoints
		{
			if (face == null || !Waypoints.ContainsKey(targ))
				return;
			var t = Waypoints[targ];
			var f = new Point(face.Position.X, face.Position.Y);
			f.X += face.Sprite.SpriteSize.X / 2;
			f.Y += face.Sprite.SpriteSize.Y;
			var radians = (float)Math.Atan2(t.X - f.X, t.Y - f.Y);
			face.Mover.ChangeDirection(GetDirectionFromRadians(radians));
		}
		bool ArePointsDiagonal(Point a, Point b)
		{
			if (a.X == b.X || a.Y == b.Y)
				return false;
			int m = (a.Y - b.Y) / (a.X - b.X);
			if (m == -1 || m == 1)
				return true;
			return false;
		}
		public Entity GetEntityFromName(string name)
		{
			if (name.StartsWith("PARTY_"))
			{
				int i = 0;
				if (name.Split('_')[1] == "LAST")
				{
					i = game.party.ActiveCharacters.Count - 1;
					name = partyEntities[i].Name;
				}
				else
				{
					i = int.Parse(name.Substring(6, 1));
					try { name = partyEntities[i].Name; }
					catch { }
				}
			}
			for (int i = 0; i < Entities.Count; i++)
			{
				if (Entities[i].Name == name)
				{
					return Entities[i];
				}
			}
			return null;
		}
		void setSpriteProperties(int entNumber, SortedList<string, string> Properties)
		{
			if (Properties.Keys.Contains("SpriteSize"))
			{
				var args = Properties["SpriteSize"].Split(',');
				Entities[entNumber].Sprite.SetSpriteSize(int.Parse(args[0]), int.Parse(args[1]));
			}
			if (Properties.Keys.Contains("Animated"))
			{
				var args = Properties["Animated"].Split(',');
				Entities[entNumber].Sprite.SetSpriteSize(int.Parse(args[0]), int.Parse(args[1]));
				Entities[entNumber].Sprite.SetInterval(float.Parse(args[2]));
				if (!Properties.Keys.Contains("StartAtFrameZero"))
					Entities[entNumber].Sprite.RandomizeFrame(game.randomNumber);
			}
			if (Properties.Keys.Contains("Mirrored"))
				Entities[entNumber].Sprite.ChangeSpriteEffects(SpriteEffects.FlipHorizontally);
			if (Properties.Keys.Contains("Inverted"))
				Entities[entNumber].Sprite.ChangeSpriteEffects(SpriteEffects.FlipVertically);
			if (Properties.Keys.Contains("Scaled"))
			{
				int spriteWidth, spriteHeight;
				var args = Properties["Scaled"].Split(',');
				spriteWidth = int.Parse(args[0]);
				spriteHeight = int.Parse(args[1]);
				Entities[entNumber].Sprite.SetScale(spriteWidth, spriteHeight);
			}
			if (Properties.Keys.Contains("Depth"))
			{
				Entities[entNumber].Sprite.SetIgnoreDepthSorting(true);
				Entities[entNumber].Sprite.ChangeDepth(float.Parse(Properties["Depth"]));
			}
			if (Properties.Keys.Contains("BottomYOffset"))
			{
				try
				{
					Entities[entNumber].Sprite.SetBottomYOffset(int.Parse(Properties["BottomYOffset"]));
					Entities[entNumber].Sprite.SetIgnoreDepthSorting(false);
				}
				catch
				{
					Console.WriteLine("PANIC: TRIED TO SET THE BOTTOMY FOR ENTITY " + Entities[entNumber].Name + ", BUT THE ENTITY HAD NO SPRITE OR THERE WAS AN ERROR IN PARSING THE ARGUMENT");
				}
			}
			if (Properties.Keys.Contains("Flicker"))
			{
				var sprite = Entities[entNumber].Sprite;
				if (sprite != null)
				{
					try
					{
						var args = Properties["Flicker"].Split(',');
						sprite.SetFlicker(int.Parse(args[0]), int.Parse(args[1]));
					}
					catch
					{
						Console.WriteLine("PANIC: ENTITY " + Entities[entNumber].Name + "WAS SET TO FLICKER, BUT THERE WAS AN ERROR PARSING THE ARGUMENTS");
					}
				}
				else
					Console.WriteLine("PANIC: ENTITY " + Entities[entNumber].Name + "WAS SET TO FLICKER, BUT IT HAD NO SPRITE");
			}
		}
		public void SpawnEntity(string ID, bool overrideSpawn)
		{
			XElement obj = null;
			foreach (XElement elem in fileEntities)
			{
				if (elem.Attribute("name").Value == ID)
				{
					obj = elem;
					break;
				}
			}
			if (obj == null)
			{
				return;
			}
			for (int i = 0; i < Entities.Count; i++)
			{
				//prevents the spawning of duplicate enemies
				if (Entities[i].Name == obj.Attribute("name").Value)
				{
					return;
				}
			}
			if (obj.Element("properties") == null)
			{
				Entities.Add(new Entity(findNewId(), obj.Attribute("name").Value, new Point((int)obj.Attribute("x"), (int)obj.Attribute("y"))));
				return;
			}
			var properties = obj.Element("properties").Elements("property");
			var Properties = new SortedList<string, string>();
			foreach (XElement elem in properties)
			{
				if (elem.Attribute("value") == null)
					Properties.Add(elem.Attribute("name").Value, elem.Value);
				else
					Properties.Add(elem.Attribute("name").Value, elem.Attribute("value").Value);
			}
			if (Properties.Keys.Contains("StaticBattler"))
			{
				if (!game.enemySpawnManager.CanSpawnStaticEnemy(Filename, ID))
				{
					return;
				}
			}
			if (Properties.Keys.Contains("KillFlag") && !overrideSpawn)
			{
				if (game.gameFlags.ContainsKey(Properties["KillFlag"]))
				{
					if (game.gameFlags[Properties["KillFlag"]] == true)
					{
						return;
					}
				}
			}
			if (Properties.Keys.Contains("DoNotSpawn"))
			{
				if (!overrideSpawn)
					return;
			}
			bool spawn = true;
			if (Properties.Keys.Contains("KillFlag"))
			{
				if (!game.gameFlags.ContainsKey(Properties["KillFlag"]))
				{
					game.gameFlags.Add(Properties["KillFlag"], false);
				}
				if (game.gameFlags[Properties["KillFlag"]])
					spawn = false;
			}
			if (overrideSpawn)
				spawn = true;
			if (Properties.Keys.Contains("SpawnFlag") && !overrideSpawn)
			{
				if (!game.gameFlags.ContainsKey(Properties["SpawnFlag"]))
				{
					game.gameFlags.Add(Properties["SpawnFlag"], false);
					return;
				}
				if (game.gameFlags[Properties["SpawnFlag"]] == true)
					spawn = true;
				if (game.gameFlags[Properties["SpawnFlag"]] == false)
					return;
			}
			if (!Properties.Keys.Contains("SpawnFlag") && !Properties.Keys.Contains("KillFlag"))
				spawn = true;
			if (spawn)
			{
				if (Properties.Keys.Contains("SpriteFolder") || Properties.Keys.Contains("Mover"))
					Entities.Add(new Walker(findNewId(), obj.Attribute("name").Value, new Point((int)float.Parse(obj.Attribute("x").Value) + ((int)float.Parse(obj.Attribute("width").Value) / 2), (int)float.Parse(obj.Attribute("y").Value) + ((int)float.Parse(obj.Attribute("height").Value))), 160));
				else
					Entities.Add(new Entity(findNewId(), obj.Attribute("name").Value, new Point((int)float.Parse(obj.Attribute("x").Value), (int)float.Parse(obj.Attribute("y").Value))));
			}
			else
				return;
			int entNumber = Entities.Count - 1;
			//TODO: replace this with a giant switch statement
			//TODO: DO THIS BEFORE CODE SOURCE
			foreach (KeyValuePair<string, string> prop in Properties)
			{
				switch (prop.Key)
				{
					case "StaticBattler":
						{
							Entities[entNumber].StaticBattler = true;
							break;
						}
					case "Sprite":
						{
							Entities[entNumber].SetSprite(game.TextureLoader, "Sprites\\" + Properties["Sprite"], new Point(0), Sprite.OriginType.TopLeft);
							setSpriteProperties(entNumber, Properties);
							break;
						}
					case "Prefab":
						{
							Entities[entNumber].AddEvents("INTERACT", (Events.CreatePrefabEvents(game, Entities[entNumber].Name, this, Properties["Prefab"].Split('|'))));
							break;
						}
					case "Teleport":
						{
							var args = Properties["Teleport"].Split('|');
							var events = new List<IEvent>();
							events = new List<IEvent>();
							events.Add(new BeginEvent(game, this));
							events.Add(new ChangeMap(game, args[0], args[1]));
							events.Add(new EndEvent(game, EventManager, this));
							Entities[entNumber].AddEvents("INTERACT", events);
							break;
						}
					case "SpriteFolder":
						{
							var walker = (Walker)Entities[entNumber];
							walker.SetSprite(game.TextureLoader, null, new Point(34, 90), Sprite.OriginType.BottomMiddle);
							walker.LoadWalkerSprites(Properties["SpriteFolder"], game.Content, game.TextureLoader);
							setSpriteProperties(entNumber, Properties);
							break;
						}
					case "Mover":
						{
							((Walker)Entities[entNumber]).SetMover(int.Parse(Properties["Mover"]));
							if (Properties.Keys.Contains("Talking"))
							{
								var walker = (Walker)Entities[entNumber];
								var dir = Directions.Down;
								switch (Properties["Talking"])
								{
									case ("Up"): { dir = Directions.Up; break; }
									case ("UpLeft"): { dir = Directions.UpLeft; break; }
									case ("UpRight"): { dir = Directions.UpRight; break; }
									case ("Left"): { dir = Directions.Left; break; }
									case ("Right"): { dir = Directions.Right; break; }
									case ("Down"): { dir = Directions.Down; break; }
									case ("DownRight"): { dir = Directions.DownRight; break; }
									case ("DownLeft"): { dir = Directions.DownLeft; break; }
								}
								walker.Behaviour = new TalkAtDirection(this, (Walker)Entities[entNumber], dir);
								walker.Mover.ChangeDirection(dir);
							}
							break;
						}
					case "LookAtPlayer":
						{
							var walker = (Walker)Entities[entNumber];
							walker.Behaviour = new LookAtPlayer((Walker)Entities[entNumber], this);
							break;
						}
					case "SpellEvents":
						{
							var events = Properties["SpellEvents"].Split('|');
							for (int i = 0; i < events.Length; i++)
							{
								var args = events[i].Split(',');
								try
								{
									Entities[entNumber].AddEvents(args[0], Events.ParseEventFromFile(game.Content.RootDirectory + "\\Events\\" + args[1] + ".txt", game, this, Entities[entNumber], true));
									Entities[entNumber].HasSpellCastEvents = true;
								}
								catch
								{
									Console.WriteLine("PANIC: ERROR WHEN PARSING SPELL EVENTS FOR " + Entities[entNumber].Name);
								}
							}
							break;
						}
					case "Lightable":
						{
							Entities[entNumber].EntBehaviour = new LightableTorch(Properties["Lightable"]);
							break;
						}
					case "TurnInterval":
						{
							try
							{
								var walker = (Walker)Entities[entNumber];
								walker.Mover.SetTurnInterval(float.Parse(Properties["TurnInterval"]));
							}
							catch
							{
								Console.WriteLine("PANIC: TRIED TO ADD A TURN INTERVAL TO " + Entities[entNumber].Name + ", BUT THE ENTITY HAD NO MOVER COMPONENT OR THERE WAS AN ERROR IN PARSING THE VALUE");
							}
							break;
						}
					case "Display":
						{
							var name = game.LoadString("Names", Properties["Display"]);
							if (name != "TEXT_NOT_FOUND")
								((Walker)Entities[entNumber]).DisplayName = name;
							break;
						}
					case "Pushable":
						{
							Directions[] dirs = new Directions[4];
							dirs[0] = Directions.Up;
							dirs[1] = Directions.Down;
							dirs[2] = Directions.Left;
							dirs[3] = Directions.Right;
							if (Properties.Keys.Contains("PushableDirs"))
							{
								var args = Properties["PushableDirs"].Split(',');
								dirs = new Directions[args.Length];
								for (int i = 0; i < args.Length; i++)
								{
									dirs[i] = (Directions)int.Parse(args[i]);
								}
							}
							Entities[entNumber].EntBehaviour = new PushableEntity(int.Parse(Properties["Pushable"]), dirs);
							break;
						}
					case "InteractEntity":
						{
							var args = Properties["InteractEntity"].Split(',');
							if (args.Length == 2)
								Entities[entNumber].AddEvents("INTERACT-" + args[1], Events.ParseEventFromFile(game.Content.RootDirectory + "\\Events\\" + args[0] + ".txt", game, this, Entities[entNumber], true));
							else
								try
								{
									Entities[entNumber].AddEvents("INTERACT-" + args[0], Events.ParseEventFromProperty(Properties["NonFileEvent"], game, this, Entities[entNumber]));
								}
								catch
								{
									Console.WriteLine("PANIC: ERROR WHEN TRYING TO PARSE PROPERTY EVENTS FOR INTERACT ENTITY" + Entities[entNumber]);
								}
							break;
						}
					case "Events":
						{
							if (Entities[entNumber].EntBehaviour is SpellCastEntity)
							{
								Entities[entNumber].AddEvents("SPELL", Events.ParseEventFromFile(game.Content.RootDirectory + "\\Events\\" + Properties["Events"] + ".txt", game, this, Entities[entNumber], true));
							}
							else if (!Properties.Keys.Contains("Hotspot"))
								Entities[entNumber].AddEvents("INTERACT", Events.ParseEventFromFile(game.Content.RootDirectory + "\\Events\\" + Properties["Events"] + ".txt", game, this, Entities[entNumber], true));
							break;
						}
					case "TimedEvents":
						{
							var args = Properties["TimedEvents"].Split(',');
							var time = float.Parse(args[0]);
							Entities[entNumber].EntBehaviour = new TimedEventEntity(Entities[entNumber], this, time);
							Entities[entNumber].AddEvents("TIMED", Events.ParseEventFromFile(game.Content.RootDirectory + "\\Events\\" + args[1] + ".txt", game, this, Entities[entNumber], true));
							break;
						}
					case "NonFileEvent":
						{
							if (Entities[entNumber].Events == null)
								Entities[entNumber].AddEvents("INTERACT", Events.ParseEventFromProperty(Properties["NonFileEvent"], game, this, Entities[entNumber]));
							break;
						}
					case "LinearMovement":
						{
							//first arg is the waypoint, second is the speed at which the entity moves, third is the interval
							var args = Properties["LinearMovement"].Split(',');
							var p = Waypoints[args[0]];
							var s = int.Parse(args[1]);
							var f = float.Parse(args[2]);
							Entities[entNumber].EntBehaviour = new LinearMovingEntity(Entities[entNumber], p, s, f);
							break;
						}
					case "HatesLight":
						{
							var walker = (Walker)Entities[entNumber];
							var behav = (EnemyBehaviour)walker.Behaviour;
							behav.behaviours.Add(Behaviours.FreezeOnLight);
							for (int i = 0; i < WorldOverlays.Count; i++)
							{
								if (WorldOverlays[i] is Overlay)
								{
									var overlay = (Overlay)WorldOverlays[i];
									//default alpha in a room with darkness is is 225. therefore, 125 should be speed -1, and -25 should be speed minus 2, and 0 is speed minus 1                  
									if (overlay.red == 0 && overlay.blue == 0 && overlay.green == 0)
									{
										switch (overlay.alpha)
										{
											case 125: walker.Mover.ChangeSpeed(walker.Mover.Speed - 1); break;
											case 25: walker.Mover.ChangeSpeed(walker.Mover.Speed - 2); break;
											case 0: walker.Mover.ChangeSpeed(walker.Mover.Speed - 3); break;

										}
									}
									break;
								}
							}
							break;
						}
				}
			}
			if (Properties.Keys.Contains("Bounds"))
			{
				var args = Properties["Bounds"].Split(',');
				if (Entities[entNumber].Sprite == null)
					Entities[entNumber].SetBounds(new Point(int.Parse(args[0]), int.Parse(args[1])), new Point(0), false);
				else
				{
					int xOffset = 0;
					int yOffset = 0;
					var spriteWidth = int.Parse(args[0]);
					if (Entities[entNumber] is Walker)
					{
						if (Entities[entNumber].Sprite.Scaled)
						{
							//TODO: ENSURE OFFSETS ARE SET CORRECTLY
						}
						else
						{
							xOffset = -(spriteWidth / 2);
							yOffset = -int.Parse(args[1]);
							Entities[entNumber].SetBounds(new Point(int.Parse(args[0]), int.Parse(args[1])), new Point(xOffset, yOffset), false);
						}
					}
					else
					{
						if (Entities[entNumber].Sprite.Scaled)
						{
							if (int.Parse(args[0]) < Entities[entNumber].Sprite.scaleWidth)
							{
								xOffset += Entities[entNumber].Sprite.SpriteSize.X - spriteWidth;
								xOffset = xOffset / 2;
							}
							if (Entities[entNumber].Sprite.scaleHeight > Entities[entNumber].Sprite.SpriteSize.Y)
								yOffset -= Entities[entNumber].Sprite.scaleHeight;
						}
						else
						{
							if (int.Parse(args[0]) < Entities[entNumber].Sprite.SpriteSize.X)
							{
								xOffset += Entities[entNumber].Sprite.SpriteSize.X - spriteWidth;
								xOffset = xOffset / 2;
							}
						}
						if (args.Length == 3)
							yOffset += int.Parse(args[2]);
						Entities[entNumber].SetBounds(new Point(int.Parse(args[0]), int.Parse(args[1])), new Point(xOffset, Entities[entNumber].Sprite.SpriteSize.Y - int.Parse(args[1]) - yOffset), false);
					}
				}
				if (Properties.Keys.Contains("CanPassThrough"))
					Entities[entNumber].Bounds.ChangeCanPassThrough(true);
				if (Properties.Keys.Contains("CanFlyOver"))
					Entities[entNumber].Bounds.ChangeCanFlyOver(true);
			}
			if (Properties.Keys.Contains("Direction"))
			{
				var walker = (Walker)Entities[entNumber];
				Directions dir = Directions.Down;
				switch (Properties["Direction"])
				{
					case ("Up"): { dir = Directions.Up; break; }
					case ("UpLeft"): { dir = Directions.UpLeft; break; }
					case ("UpRight"): { dir = Directions.UpRight; break; }
					case ("Left"): { dir = Directions.Left; break; }
					case ("Right"): { dir = Directions.Right; break; }
					case ("Down"): { dir = Directions.Down; break; }
					case ("DownRight"): { dir = Directions.DownRight; break; }
					case ("DownLeft"): { dir = Directions.DownLeft; break; }
				}
				walker.Mover.ChangeDirection(dir);
			}
			if (Properties.Keys.Contains("Movement"))
			{
				var walker = new Walker(-1, "", Point.Zero, 160);
				if (Entities[entNumber] is Walker)
					walker = (Walker)Entities[entNumber];
				switch (Properties["Movement"])
				{
					case ("ApproachPlayer"):
						{
							int distance = 400;
							if (Properties.ContainsKey("Events"))
							{
								Entities[entNumber].AddEvents("COLLIDE", Events.ParseEventFromFile(game.Content.RootDirectory + "\\Events\\" + Properties["Events"] + ".txt", game, this, Entities[entNumber], true));
							}
							if (Properties.ContainsKey("NonFileEvent"))
							{
								Entities[entNumber].AddEvents("COLLIDE", Events.ParseEventFromProperty(Properties["NonFileEvent"], game, this, Entities[entNumber]));
							}
							if (Properties.Keys.Contains("ApproachDistance"))
								distance = int.Parse(Properties["ApproachDistance"]);
							Entities[entNumber].Bounds.ChangeCanPassThrough(true);
							walker.Behaviour = new EnemyBehaviour(this, walker, distance, new List<Behaviours>());
							if (Properties.ContainsKey("Flying"))
								((EnemyBehaviour)walker.Behaviour).behaviours.Add(Behaviours.Flying);
							((EnemyBehaviour)walker.Behaviour).overrideRange = true;
							walker.SetMover(int.Parse(Properties["Mover"]));
							break;
						}
					case ("RandomWalk"):
						{
							walker.Behaviour = new RandomWalk(this, walker, game.randomNumber);
							if (Properties.ContainsKey("Events"))
							{
								Entities[entNumber].AddEvents("INTERACT", Events.ParseEventFromFile(game.Content.RootDirectory + "\\Events\\" + Properties["Events"] + ".txt", game, this, Entities[entNumber], true));
							}
							break;
						}
					case ("RotateLine"):
						{
							if (Properties.ContainsKey("RotateDirections"))
							{
								var args = Properties["RotateDirections"].Split(',');
								walker.Behaviour = new EyeRotate(walker, this, (Directions)int.Parse(args[0]), (Directions)int.Parse(args[1]));
							}
							else
								walker.Behaviour = new EyeRotate(walker, this);
							if (Properties.ContainsKey("Events"))
								Entities[entNumber].AddEvents("SPECIAL", Events.ParseEventFromFile(game.Content.RootDirectory + "\\Events\\" + Properties["Events"] + ".txt", game, this, Entities[entNumber], true));
							if (Properties.ContainsKey("Reverse"))
							{
								var behaviour = (EyeRotate)walker.Behaviour;
								behaviour.Reverse();
							}
							walker.ChangeWalkerState(WalkerState.Standing);
							walker.Sprite.SetInterval(350);
							break;
						}
					case ("Patrol"):
						{
							string[] args;
							var waypoints = new List<Vector2>();
							if (Properties.ContainsKey("PatrolWaypoints"))
							{
								args = Properties["PatrolWaypoints"].Split(',');

								for (int i = 0; i < args.Length; i++)
								{
									if (Waypoints.ContainsKey(args[i]))
										waypoints.Add(Waypoints[args[i]].ToVector2());
								}
							}
							bool canHear = true;
							bool canSee = true;
							float reactTime = 0.5f;
							float waitTime = 3;
							bool ros = false;
							if (Properties.ContainsKey("WaitTime"))
								waitTime = float.Parse(Properties["WaitTime"]);
							if (Properties.ContainsKey("ReactTime"))
								reactTime = float.Parse(Properties["ReactTime"]);
							if (Properties.ContainsKey("CanSee"))
								canSee = bool.Parse(Properties["CanSee"]);
							if (Properties.ContainsKey("CanHear"))
								canSee = bool.Parse(Properties["CanHear"]);
							if (Properties.ContainsKey("ActivateOnSight"))
								ros = true;
							if (waypoints.Count == 0)
								waypoints.Add(new Vector2(Entities[entNumber].Position.X, Entities[entNumber].Position.Y));
							((Walker)Entities[entNumber]).Behaviour = new Patrol((Walker)Entities[entNumber], (Walker)Entities[1], this, waypoints, waitTime, reactTime, canHear, canSee, ros);
							((Walker)Entities[entNumber]).Bounds.ChangeCanPassThrough(true);
							((Walker)Entities[entNumber]).Mover.SetTurnInterval(0.065f);
							if (Properties.ContainsKey("Events"))
							{
								Entities[entNumber].AddEvents("COLLIDEPARTY", Events.ParseEventFromFile(game.Content.RootDirectory + "\\Events\\" + Properties["Events"] + ".txt", game, this, Entities[entNumber], true));
							}
							if (Properties.ContainsKey("NonFileEvent"))
							{
								Entities[entNumber].AddEvents("COLLIDEPARTY", Events.ParseEventFromProperty(Properties["NonFileEvent"], game, this, Entities[entNumber]));
							}

							break;
						}
					case ("BurstFromWall"):
						{
							if (Properties.ContainsKey("BurstSprites") && Properties.ContainsKey("BurstBattlegroup"))
							{
								var args = Properties["BurstSprites"].Split(',');
								var Burst = game.TextureLoader.RequestTexture("Sprites\\" + args[0]);
								var Out = game.TextureLoader.RequestTexture("Sprites\\" + args[1]);
								var Retreat = game.TextureLoader.RequestTexture("Sprites\\" + (args[2]));
								var events = new List<IEvent>();
								var timer = 2.2f;
								int spriteWidth = 64;
								var sounds = new string[1];
								if (Properties.ContainsKey("BurstWidth"))
									spriteWidth = int.Parse(Properties["BurstWidth"]);
								if (Properties.ContainsKey("BurstTimer"))
									timer = int.Parse(Properties["BurstTimer"]);
								if (Properties.ContainsKey("BurstSounds"))
									sounds = Properties["BurstSounds"].Split('|');
								Entities[entNumber].EntBehaviour = new BurstFromWall(this, Entities[entNumber], (Walker)Entities[1], Burst, Out, Retreat, timer, spriteWidth, sounds);
								events = new List<IEvent>();
								events.Add(new BeginEvent(game, this));
								events.Add(new StartBattle(game, Properties["BurstBattlegroup"]));
								events.Add(new KillEntity(this, Entities[entNumber].Name));
								events.Add(new EndEvent(game, this.EventManager, this));
								Entities[entNumber].AddEvents("COLLIDEPARTY", events);
								if (Properties.ContainsKey("Events"))
									Entities[entNumber].AddEvents("COLLIDEPARTY", Events.ParseEventFromFile(game.Content.RootDirectory + "\\Events\\" + Properties["Events"] + ".txt", game, this, Entities[entNumber], true));
							}
							else
							{
								Console.Write("PANIC: TRIED TO CREATE 'BurstFromWall' BEHAVIOUR, BUT OBJ IN MAP DID NOT HAVE 'BurstSprites' or 'BurstBattlegroup' PROPERTY");
								break;
							}
							break;
						}
				}
			}
			if (Properties.Keys.Contains("ActivateOnCollision"))
			{
				if (Properties.ContainsKey("Events"))
				{
					Entities[entNumber].AddEvents("COLLIDEPARTY", Events.ParseEventFromFile(game.Content.RootDirectory + "\\Events\\" + Properties["Events"] + ".txt", game, this, Entities[entNumber], true));
				}
				if (Properties.ContainsKey("NonFileEvent"))
				{
					Entities[entNumber].AddEvents("COLLIDEPARTY", Events.ParseEventFromProperty(Properties["NonFileEvent"], game, this, Entities[entNumber]));
				}
				Entities[entNumber].Bounds.ChangeCanPassThrough(true);
			}
			if (Properties.Keys.Contains("Hotspot"))
			{
				if (Entities[entNumber].Bounds == null)
					Entities[entNumber].SetBounds(new Point((int)obj.Attribute("width"), (int)obj.Attribute("height")), new Point(0), true);
				if (Properties.ContainsKey("Events"))
				{
					Entities[entNumber].AddEvents("COLLIDE", Events.ParseEventFromFile(game.Content.RootDirectory + "\\Events\\" + Properties["Events"] + ".txt", game, this, Entities[entNumber], true));
				}
				if (Properties.Keys.Contains("NonFileEvent"))
				{
					Entities[entNumber].AddEvents("COLLIDE", Events.ParseEventFromProperty(Properties["NonFileEvent"], game, this, Entities[entNumber]));
				}
			}
		}
		public void RemoveEntity(string name)
		{
			for (int i = 0; i < Entities.Count; i++)
			{
				if (Entities[i].Name == name)
				{
					if (Entities[i] is Walker)
					{
						if (partyEntities.Contains((Walker)Entities[i]))
						{
							partyEntities.Remove((Walker)Entities[i]);
							for (int e = 1; e < partyEntities.Count; e++)
							{
								partyEntities[e].Behaviour = new FollowerEntity(partyEntities[e], partyEntities[e - 1], this);
							}
						}
					}
					bool recrNodes = false;
					if (Entities[i].Bounds != null && (Entities[i] is Walker) == false && (Entities[i].EntBehaviour is PushableEntity) == false)
					{
						if (Entities[i].Bounds.CanPassThrough == false)
							recrNodes = true;
					}
					Entities[i] = null;
					Entities.RemoveAt(i);
					if (recrNodes)
					{
						RecreatePathfindingNodes();
					}

					return;
				}
			}
		}
		public void SpawnPlayerEntity(PlayerCharacter pc)
		{
			Entities.Add(new Walker(findNewId(), "ENT_" + pc.Name.ToUpper(), new Point(Entities[1].Position.X, Entities[1].Position.Y), game.randomNumber.Next(145, 175)));
			//Entities.Add(new OldEntity("ENT_" + (pc.Name.ToUpper()), Entities[0].Position, null, new Mover(3, Mover.MovementType.Directional), new Bounds(new Vector2(Entities[0].Position.X, Entities[0].Position.Y), 34, 18, true)));
			//Entities[Entities.Count - 1].SetSprite(new Sprite(null, new Vector2(Entities[0].Position.X, Entities[0].Position.Y), 0.5f, new Vector2(38, 90), Sprite.OriginType.TopLeft));
			var walker = (Walker)Entities[Entities.Count - 1];
			walker.SetSprite(game.TextureLoader, null, new Point(34, 90), Sprite.OriginType.BottomMiddle);
			walker.Sprite.SetInterval(walker.baseWalkerInterval);
			walker.SetMover(3);
			walker.LoadWalkerSprites(pc.GraphicsFolderName, game.Content, game.TextureLoader);
			walker.SetBounds(new Point(32, 16), new Point(-16, -16), true);
			walker.DisplayName = game.LoadString("Names", pc.Name);
			walker.Mover.ChangeDirection(((Walker)Entities[1]).Mover.direction);
			partyEntities.Add(walker);
			walker.Behaviour = new FollowerEntity(walker, partyEntities[partyEntities.Count - 2], this);
		}

		void dislodgeMovement(Walker ent)
		{
			bool flies = false;
			if (ent.Behaviour is EnemyBehaviour)
			{
				var behaviour = (EnemyBehaviour)ent.Behaviour;
				if (behaviour.behaviours.Contains(Behaviours.Flying))
					flies = true;
			}
			if (ent.Bounds == null)
				return;
			var bounds_ = new List<Line>();
			var rectangles = new List<Rectangle>();
			bounds_.AddRange(Bounds);
			if (!flies)
			{
				for (int i = 0; i < NonFlyingBounds.Count; i++)
				{
					if (!bounds_.Contains(NonFlyingBounds[i]))
						bounds_.Add(NonFlyingBounds[i]);
				}
			}
			for (int i = 0; i < Entities.Count; i++)
			{
				if (i == 1)
				{
					if (ent.Behaviour is RandomWalk && !ent.Bounds.CanPassThrough)
					{
						rectangles.Add(Entities[i].Bounds.Box);
						continue;
					}
				}
				if (Entities[i].Bounds != null)
				{
					if (Entities[i].Bounds.CanFlyOver && flies)
						continue;
					if (!Entities[i].Bounds.CanPassThrough && Entities[i] != ent)
					{
						if (Entities[i].Bounds.Box.Width > 0)
						{
							rectangles.Add(Entities[i].Bounds.Box);
						}
					}
				}
			}
		CheckBounds:
			var original = ent.Position;
			var entCenter = new Vector2(ent.Bounds.Box.X + (ent.Bounds.Box.Width / 2), ent.Bounds.Box.Y + (ent.Bounds.Box.Height / 2));
			var wayPointTarget = ent.Mover.Target;
			var blockingRec = Rectangle.Empty;
			var blockingLine = new Line(Point.Zero, Point.Zero);
			foreach (Rectangle rec in rectangles)
			{
				if (ent.Bounds.Box.Intersects(rec))
				{
					///this for loop is designed to prevent entities pushing us out of the map if they have a random walk behaviour
					for (int i = 0; i < Entities.Count; i++)
					{
						if (Entities[i] is Walker)
						{
							var walker = (Walker)Entities[i];
							if (walker.Mover.Movement != Vector2.Zero && walker.Behaviour is RandomWalk && rec == walker.Bounds.Box)
							{
								var wp = walker.Mover.ReturnDirectionPoint();
								walker.Mover.SetTarget(Point.Zero);
                                walker.Bounds.Update(walker.Position);
								walker.Sprite.ResetTimer();
								var rw = (RandomWalk)walker.Behaviour;
								rw.findNewPosition = false;
								break;
							}
						}
					}
					var p = ent.Mover.ReturnDirectionPoint();
					ent.ChangePosition(new Point(ent.Position.X + -p.X, ent.Position.Y + -p.Y));
					ent.Bounds.Update(ent.Position);
					if (ent.Behaviour is RandomWalk)
					{
						var rWalk = (RandomWalk)ent.Behaviour;
						rWalk.findNewPosition = true;
					}
					ent.Sprite.CurrentFrame = 0;
					ent.Sprite.ResetTimer();
					ent.ChangeWalkerState(WalkerState.Standing);
					ent.Mover.Waypoints.Clear();
					ent.Mover.SetTarget(Point.Zero);
					//needsNewWaypoints = true;
					blockingRec = rec;
					goto CheckBounds;
				}
			}
			foreach (Line line in bounds_)
			{
				if (Magicians.Bounds.IntersectsLine(ent.Bounds.Box, line.Start, line.End))
				{
					switch (ent.Mover.direction)
					{
						case Directions.Right: ent.ChangePosition(new Point(ent.Position.X - 1, ent.Position.Y)); break;
						case Directions.Left: ent.ChangePosition(new Point(ent.Position.X + 1, ent.Position.Y)); break;
						case Directions.Up: ent.ChangePosition(new Point(ent.Position.X, ent.Position.Y + 1)); break;
						case Directions.Down: ent.ChangePosition(new Point(ent.Position.X, ent.Position.Y - 1)); break;
						case Directions.UpRight: ent.ChangePosition(new Point(ent.Position.X - 1, ent.Position.Y + 1)); break;
						case Directions.DownRight: ent.ChangePosition(new Point(ent.Position.X - 1, ent.Position.Y - 1)); break;
						case Directions.UpLeft: ent.ChangePosition(new Point(ent.Position.X + 1, ent.Position.Y + 1)); break;
						case Directions.DownLeft: ent.ChangePosition(new Point(ent.Position.X + 1, ent.Position.Y - 1)); break;
					}
					ent.Sprite.CurrentFrame = 0;
					ent.Sprite.ResetTimer();
					ent.ChangeWalkerState(WalkerState.Standing);
					ent.Bounds.Update(ent.Position);
					ent.Mover.Waypoints.Clear();
					ent.Mover.SetTarget(Point.Zero);
					blockingLine = line;
					goto CheckBounds;
				}
			}
		}
		bool isSpaceFree(Entity ent, Rectangle newRec)
		{
			if (ent.Bounds == null)
				return true;
			var bounds_ = new List<Line>();
			var rectangles = new List<Rectangle>();
			var addFlying = true;
			if (ent is Walker)
				if (((Walker)ent).Behaviour is EnemyBehaviour)
				{
					var behav = (EnemyBehaviour)((Walker)ent).Behaviour;
					if (behav.behaviours.Contains(Behaviours.Flying))
						addFlying = false;

				}
			if (ent.EntBehaviour is PushableEntity)
				addFlying = false;
			if (addFlying)
				bounds_.AddRange(NonFlyingBounds);
			for (int i = 0; i < Bounds.Count; i++)
			{
				if (!bounds_.Contains(Bounds[i]))
					bounds_.Add(Bounds[i]);
			}
			rectangles.Remove(ent.Bounds.Box);
			for (int i = 0; i < Entities.Count; i++)
			{
				if (Entities[i].Bounds != null)
				{
					if (Entities[i].Bounds.CanFlyOver && !addFlying)
					{
						continue;
					}
					if (Entities[i].Bounds.CanPassThrough == false && Entities[i] != ent)
					{
						if (Entities[i].Bounds.Box.Width > 0)
						{
							rectangles.Add(Entities[i].Bounds.Box);
						}
					}
				}
			}
			foreach (Rectangle rec in rectangles)
			{
				if (rec.Intersects(newRec))
				{
					return false;
				}
			}
			foreach (Line line in bounds_)
			{
				if (Magicians.Bounds.IntersectsLine(newRec, line.Start, line.End))
				{
					return false;
				}
			}
			return true;
		}
		Rectangle getInteractBox()
		{
			var player = (Walker)Entities[1];
			var rec = new Rectangle((Entities[1].Bounds.Position.X + (Entities[1].Bounds.Box.Width / 2) - 8), (Entities[1].Bounds.Position.Y + (Entities[1].Bounds.Box.Height / 2)) - 8, 16, 16);
			switch (player.Mover.direction)
			{
				case (Directions.Up): { rec.Offset(0, -16); break; }
				case (Directions.UpRight): { rec.Offset(16, -16); break; }
				case (Directions.Right): { rec.Offset(16, 0); break; }
				case (Directions.DownRight): { rec.Offset(16, 16); break; }
				case (Directions.Down): { rec.Offset(0, 16); break; }
				case (Directions.DownLeft): { rec.Offset(-16, 16); break; }
				case (Directions.Left): { rec.Offset(-16, 0); break; }
				case (Directions.UpLeft): { rec.Offset(-16, -16); break; }
			}
			return rec;
		}
		void setSpriteDepth() //goes through each entity, setting its depth depending upon Y position.
		{
			var furthestDepth = 0.599f;
			var nearestDepth = 0.501f;
			var sprites = new List<Sprite>();
			for (int i = 0; i < Entities.Count; i++)
			{
				if (Entities[i].Sprite != null)
				{
					if (!Entities[i].Sprite.IgnoreDepthSorting)
					{
						sprites.Add(Entities[i].Sprite);
					}
				}
			}
			for (int i = 0; i < effects.Count; i++)
			{
				if (effects[i].sprite.IgnoreDepthSorting == false)
				{
					effects[i].SetDepth();
				}
			}
			sprites.Sort((y, z) => y.BottomY.CompareTo(z.BottomY));
			var depth = furthestDepth;
			foreach (Sprite sprite in sprites)
			{
				sprite.ChangeDepth(MathHelper.Clamp(depth, nearestDepth, furthestDepth));
				depth -= 0.001f;
			}
		}
		int findNewId()
		{
			var id = 0;
			for (int i = 0; i < Entities.Count; i++)
			{
				if (Entities[i].ID == id)
				{
					id += 1;
				}
			}
			return id;
		}
		void createDoors()
		{
			var elements = mapFile.Descendants("objectgroup");
			foreach (XElement elem in elements)
			{
				switch (elem.Attribute("name").Value)
				{
					case ("Doors"):
						{
							var doors = elem.Descendants("object");
							foreach (XElement door in doors)
							{
								Entities.Add(new Entity(findNewId(), "DOOR", new Point((int)door.Attribute("x"), (int)door.Attribute("y"))));
								Entities[Entities.Count - 1].Bounds = new Magicians.Bounds(Entities[Entities.Count - 1].Position, (int)door.Attribute("width"), (int)door.Attribute("height"), true, Point.Zero);
								Entity ent = Entities[Entities.Count - 1];
								var events = new List<IEvent>();
								var doorProperties = door.Element("properties").Elements("property");
								string destination = "";
								string position = "";
								string sound = "";
								foreach (XElement property in doorProperties)
								{
									if (property.Attribute("name").Value == "Destination")
										destination = property.Attribute("value").Value;
									if (property.Attribute("name").Value == "Position")
										position = property.Attribute("value").Value;
									if (property.Attribute("name").Value == "Sound")
										sound = property.Attribute("value").Value;
								}
								if (sound != "")
								{
									events.Add(new PlaySound(game, sound));
								}
								events.Add(new ChangeMap(game, destination, position));
								ent.AddEvents("COLLIDE", events);
							}
							return;
						}
				}
			}
		}
		//gets a position relative to the nearest equivalent pathfinding node
		Point getNodePosition(Point p)
		{
			//first, remove topleft from the original point
			//then, divide by eight (and round to nearest) to find equivalent position in the nodegrid
			return new Point((p.X - upperLeft.X) / NodeGap, ((p.Y - upperLeft.Y) / NodeGap));
		}
		public void RecreatePathfindingNodes()
		{
			var rectangles = new List<Rectangle>();
			var nonFlyingRectangles = new List<Rectangle>();
			for (int i = 1; i < Entities.Count; i++)
			{
				if (Entities[i].Bounds != null && (Entities[i] is Walker) == false && (Entities[i].EntBehaviour is PushableEntity) == false)
				{
                    Entities[i].Bounds.Update(Entities[i].Position);
					if (Entities[i].Bounds.CanPassThrough == false)
					{
						if (Entities[i].Bounds.Box.Width > 0)
							rectangles.Add(Entities[i].Bounds.Box);
						if (Entities[i].Bounds.CanFlyOver)
						{
							nonFlyingRectangles.Add(rectangles[rectangles.Count - 1]);
						}
						continue;
					}
				}
			}
			NodeGap = 16;
			if (Waypoints.ContainsKey("UpperLeft") && Waypoints.ContainsKey("BottomRight"))
			{
				upperLeft = new Point(Waypoints["UpperLeft"].X, Waypoints["UpperLeft"].Y);
				bottomRight = new Point(Waypoints["BottomRight"].X, Waypoints["BottomRight"].Y);
			}
			else
			{
				upperLeft = new Point(0);
				bottomRight = new Point(TileWidth * Width, (TileHeight * Height));
			}
			width = (bottomRight.X - upperLeft.X) / NodeGap;
			height = (bottomRight.Y - upperLeft.Y) / NodeGap;
			nodes = new Node[width, height];
			var rect = new Rectangle();
			for (int x = 0; x < width; x++)
			{
				for (int y = 0; y < height; y++)
					nodes[x, y] = new Node(new Point(upperLeft.X + (x * NodeGap), upperLeft.Y + (y * NodeGap)), new Point(x, y), NodeType.Clear);
			}
			foreach (Line line in NonFlyingBounds)
			{
				//first, work out the area around the line the player could concievably bump into. we do this by working out the highest and lowest x/y values the line occupies, then ensuring those values
				//conform to the node grid.
				int Left = ((Math.Min(line.Start.X, line.End.X) - upperLeft.X) / NodeGap) - 1;
				int Top = ((Math.Min(line.Start.Y, line.End.Y) - upperLeft.Y) / NodeGap) - 1;
				int Right = ((Math.Max(line.Start.X, line.End.X) - upperLeft.X) / NodeGap) + 1;
				int Bottom = ((Math.Max(line.Start.Y, line.End.Y) - upperLeft.Y) / NodeGap) + 1;
				//then, iterate
				for (int x = Left; x <= Right; x++)
				{
					for (int y = Top; y <= Bottom; y++)
					{
						if (x >= nodes.GetLength(0) || y >= nodes.GetLength(1) || x < 0 || y < 0)
							continue;
						rect = new Rectangle(nodes[x, y].RealLocation.X - 16, nodes[x, y].RealLocation.Y, 32, -16);
						if (Magicians.Bounds.IntersectsLine(rect, line.Start, line.End))
						{
							nodes[x, y].Type = NodeType.BlocksNonFlying;
						}
					}
				}
			}
			foreach (Line line in Bounds)
			{
				//first, work out the area around the line the player could concievably bump into. we do this by working out the highest and lowest x/y values the line occupies, then ensuring those values
				//conform to the node grid.
				int Left = ((Math.Min(line.Start.X, line.End.X) - upperLeft.X) / NodeGap) - 1;
				int Top = ((Math.Min(line.Start.Y, line.End.Y) - upperLeft.Y) / NodeGap) - 1;
				int Right = ((Math.Max(line.Start.X, line.End.X) - upperLeft.X) / NodeGap) + 1;
				int Bottom = ((Math.Max(line.Start.Y, line.End.Y) - upperLeft.Y) / NodeGap) + 1;
				//then, iterate
				for (int x = Left; x <= Right; x++)
				{
					for (int y = Top; y <= Bottom; y++)
					{
						if (x >= nodes.GetLength(0) || y >= nodes.GetLength(1) || x < 0 || y < 0)
							continue;
						rect = new Rectangle(nodes[x, y].RealLocation.X - 16, nodes[x, y].RealLocation.Y, 32, -16);
						if (Magicians.Bounds.IntersectsLine(rect, line.Start, line.End))
						{
							nodes[x, y].Type = NodeType.BlocksAll;
						}
					}
				}
			}
			foreach (Rectangle rec in rectangles)
			{
				//first, work out the area around the line the player could concievably bump into. we do this by working out the highest and lowest x/y values the line occupies, then ensuring those values
				//conform to the node grid.
				int Left = ((rec.Left - upperLeft.X) / NodeGap) - 1;
				int Top = ((rec.Top - upperLeft.Y) / NodeGap) - 1;
				int Right = ((rec.Right - upperLeft.X) / NodeGap) + 1;
				int Bottom = ((rec.Bottom - upperLeft.Y) / NodeGap) + 1;
				for (int x = Left; x <= Right; x++)
				{
					for (int y = Top; y <= Bottom; y++)
					{
						if (x >= nodes.GetLength(0) || y >= nodes.GetLength(1) || x < 0 || y < 0)
							continue;
						if (nonFlyingRectangles.Contains(rec))
							nodes[x, y].Type = NodeType.BlocksNonFlying;
						else
							nodes[x, y].Type = NodeType.BlocksAll;
					}
				}
			}
		}
		//map loading stuff
		public Map(string filename, Game g, string SpawnPoint, bool spawnEnemies, Directions dir)
		{
			game = g;
			MapBorder = game.debugSquare;
			DisplayName = "";
			Waypoints = new SortedList<string, Point>();
			Entities = new List<Entity>();
			this.Filename = filename;

			CanMovePlayer = true;
			EventManager = new EventManager(g);
			Entities.Add(new Entity(findNewId(), "ENT_CAMERA", new Point(0, 0)));
			Entities[0].EntBehaviour = new CameraEntity(Entities[0]);
			Load(g.Content, g.TextureLoader);
			var point = new Point();
			try
			{
				if (SpawnPoint.Contains(","))
				{
					var args = SpawnPoint.Split(',');
					point = new Point(int.Parse(args[0]), int.Parse(args[1]));
				}
				else
				{
					point = Waypoints[SpawnPoint];
				}
			}
			catch
			{
				point = Point.Zero;
			}
			Waypoints.Add("StartingPoint", point);
			interactHighlight = new Sprite(g.TextureLoader, "UI\\World\\InteractHighlight", new Point(-999, -999), 0.39f, new Point(20, 20), Sprite.OriginType.FromCentre);
			interactHighlight.SetIgnoreDepthSorting(true);
			///Spawn player
			Entities.Add(new Walker(findNewId(), "ENT_PLAYER", point, 160));
			var player = (Walker)Entities[1];
			player.SetSprite(g.TextureLoader, null, new Point(34, 90), Sprite.OriginType.BottomMiddle);
			player.SetBounds(new Point(32, 16), new Point(-16, -16), true);
			player.DisplayName = g.party.PlayerCharacters[0].Name;
			player.Behaviour = new PlayerBehaviour(this);
			player.SetMover(walkSpeed);
			player.Mover.ChangeDirection(dir);
			player.Sprite.SetInterval(160);
			player.LoadWalkerSprites(g.party.PlayerCharacters[0].GraphicsFolderName, g.Content, g.TextureLoader);
			partyEntities.Add((Walker)Entities[1]);
			((CameraEntity)Entities[0].EntBehaviour).EntFocus = partyEntities[0];
			if (makeAutoSave)
				game.SaveGame(this);
			for (int i = 1; i < game.party.ActiveCharacters.Count; i++)
			{
				var pc = game.party.GetPlayerCharacter(game.party.ActiveCharacters[i]);
				SpawnPlayerEntity(pc);
			}
			if (game.party.ActiveCharacters.Count > 1)
				player.ChangePosition(new Point(player.Position.X, player.Position.Y + 1));
			createDoors();
			int id = 1;
			foreach (XElement elem in fileEntities)
			{
				if (elem.Attribute("name") == null)
				{
					elem.Add(new XAttribute("name", id));
					id++;
				}
				SpawnEntity(elem.Attribute("name").Value, false);
			}
			RecreatePathfindingNodes();

			if (spawnEnemies)
			{
				for (int ei = 0; ei < EnemySpawns.Count; ei++)
				{
					var spawn = EnemySpawns[ei];
					g.enemySpawnManager.CheckEnemySpawn(Filename, EnemySpawns);
					var p = new Point(spawn.SpawnBounds.X, spawn.SpawnBounds.Y);
					var battleGroup = game.battleGroups.Find(bg => bg.Name == spawn.BattleGroupName);
					if (battleGroup == null)
					{
						Console.WriteLine("PANIC: BATTLEGROUP NOT FOUND:" + EnemySpawns[ei].BattleGroupName);
						continue;
					}
					if (spawn.SpawnFlag != "")
					{
						if (game.gameFlags.ContainsKey(spawn.SpawnFlag))
						{
							if (!game.gameFlags[spawn.SpawnFlag])
							{
								continue;
							}
						}
						else
							continue;
					}
					if (spawn.KillFlag != "")
					{
						if (game.gameFlags.ContainsKey(spawn.KillFlag))
						{
							if (game.gameFlags[spawn.KillFlag])
							{
								continue;
							}
						}
					}
					if (g.enemySpawnManager.CanSpawnEnemy(Filename, ei))
					{
						int entNumber = 0;
						var entityNames = new List<string>();
						var events = new List<IEvent>();
						events.Add(new BeginEvent(game, this));
						events.Add(new StartBattle(game, battleGroup.Name, ei));
						var number = 1;
						var rectangles_ = new List<Rectangle>();
						for (int r = 0; r < Entities.Count; r++)
						{
							if (Entities[r].Bounds != null)
							{
								if (!Entities[r].Bounds.CanPassThrough)
									rectangles_.Add(Entities[r].Bounds.Box);
							}
						}
						for (int i = 0; i < battleGroup.battlers.Length; i++)
						{
							var battler = new Battler(game, battleGroup.battlers[i]);
							Point SpawnPos;
							var xOffset = 0;
							var yOffset = 0;
							if (spawn.SpawnOnPoint)
								SpawnPos = new Point(p.X + xOffset, p.Y + yOffset);
							else
								SpawnPos = Point.Zero;
							string name = "ENT_" + battler.internalName.ToUpper();
							for (int e = 0; e < Entities.Count; e++)
							{
								if (name == Entities[e].Name)
								{
									name = name + "_" + number;
									number++;
								}
							}
							entNumber = Entities.Count;
							Entities.Add(new Walker(findNewId(), name, SpawnPos, 160));
							var walker = (Walker)Entities[entNumber];
							walker.SetSprite(game.TextureLoader, null, new Point(34, 90), Sprite.OriginType.BottomMiddle);
							walker.SetMover(battler.worldMovementSpeed);
							var direction = game.randomNumber.Next(1, 8);
							walker.Mover.ChangeDirection((Directions)direction);
							walker.Mover.SetTurnInterval(0.075f);
							walker.LoadWalkerSprites(battler.worldGraphicsFolder, game.Content, game.TextureLoader);
							walker.SetBounds(new Point(32, 16), new Point(-(walker.Sprite.SpriteSize.X / 2), -16), true);
							var behavs = new List<Behaviours>();
							behavs.AddRange(battler.worldBehaviours);
							walker.Behaviour = new EnemyBehaviour(this, walker, 200, behavs);
							entityNames.Add(name);
							Enemies.Add(walker);
							walker.Bounds.Update(walker.Position);
							if (walker.Position == Point.Zero)
							{
								walker.ChangePosition(new Point(game.randomNumber.Next(spawn.SpawnBounds.Left, spawn.SpawnBounds.Right) + xOffset, game.randomNumber.Next(spawn.SpawnBounds.Top, spawn.SpawnBounds.Bottom) + yOffset));
								walker.Bounds.Update(walker.Position);
								var behav = (EnemyBehaviour)walker.Behaviour;
								behav.UpdateOriginalPosition();
								while (!isSpaceFree(walker, walker.Bounds.Box))
								{
									walker.ChangePosition(new Point(game.randomNumber.Next(spawn.SpawnBounds.Left, spawn.SpawnBounds.Right) + xOffset, game.randomNumber.Next(spawn.SpawnBounds.Top, spawn.SpawnBounds.Bottom) + yOffset));
									walker.Bounds.Update(walker.Position);
									behav.UpdateOriginalPosition();
								}
							}
							rectangles_.Add(walker.Bounds.Box);
							if (battler.worldBehaviours.Contains(Behaviours.InteractWithEntity))
							{
								var args = battler.ReturnSpecialEntArgs(game);
								walker.AddEvents("INTERACT-" + args[1], Events.ParseEventFromFile(game.Content.RootDirectory + "\\Events\\" + args[0] + ".txt", game, this, walker, true));
							};
						}
						for (int n = 0; n < entityNames.Count; n++)
						{
							events.Add(new KillEntity(this, entityNames[n]));
						}
						events.Add(new EndEvent(game, EventManager, this));
						for (int e = 0; e < Entities.Count; e++)
						{
							for (int n = 0; n < entityNames.Count; n++)
							{
								if (Entities[e].Name == entityNames[n])
								{
									Entities[e].AddEvents("COLLIDEPARTY", events);
								}
							}
						}
					}
				}
			}
			if (MusicFile != null)
				game.Audio.SetMusic(MusicFile);
			if (mapEvents != null)
			{
				if (mapEvents.Count > 0)
				{
					game.worldUI.display = false;
					EventManager.SetEvents(mapEvents);
					EventManager.DoEvent();
				}
			}
			else
				game.worldUI.display = true;
		}
		public void Load(ContentManager content, TextureLoader TextureLoader)
		{
			var settings = new XmlReaderSettings();
			settings.DtdProcessing = DtdProcessing.Ignore;
			Filename = Filename.Replace('\\', game.PathSeperator);
			//TODO: change this to use XDocument and generally make it not terrible
			using (var stream = File.OpenText(content.RootDirectory + game.PathSeperator + "Maps" + game.PathSeperator + Filename + ".tmx"))
			using (var reader = XmlReader.Create(stream, settings))
				while (reader.Read())
				{
					var name = reader.Name;
					switch (reader.NodeType)
					{
						case XmlNodeType.Element:
							switch (name)
							{
								case "map":
									{
										Width = int.Parse(reader.GetAttribute("width"));
										Height = int.Parse(reader.GetAttribute("height"));
										TileWidth = int.Parse(reader.GetAttribute("tilewidth"));
										TileHeight = int.Parse(reader.GetAttribute("tileheight"));
									}
									break;
								case "tileset":
									{
										using (var st = reader.ReadSubtree())
										{
											st.Read();
											var tileset = Tileset.Load(st);
											Tilesets.Add(tileset.Name, tileset);
										}
									}
									break;
								case "layer":
									{
										using (var st = reader.ReadSubtree())
										{
											st.Read();
											var layer = Layer.Load(st);
											if (null != layer)
											{
												if (layer.Properties.ContainsKey("DrawFlag"))
												{
													if (game.gameFlags.ContainsKey(layer.Properties["DrawFlag"]))
													{
														if (game.gameFlags[layer.Properties["DrawFlag"]] == true)
														{
															this.Layers.Add(layer.Name, layer);
															break;
														}
													}
												}
												else
													this.Layers.Add(layer.Name, layer);
											}
										}
									}
									break;
							}
							break;
					}
				}
			foreach (var tileset in Tilesets.Values)
			{
				var s = Path.Combine("Maps\\", Path.GetDirectoryName(tileset.Image), Path.GetFileNameWithoutExtension(tileset.Image));
				s = s.Replace('\\', game.PathSeperator);
				if (!s.StartsWith("Maps" + game.PathSeperator + "Tilesets"))
				{
					for (int i = s.Length - 1; i > 0; i--)
					{
						if (s[i] == game.PathSeperator)
						{
							s = s.Remove(0, i);
							s = s.Insert(0, "Maps" + game.PathSeperator + "Tilesets");
							break;
						}
					}
				}
				tileset.Texture = TextureLoader.RequestTexture(s);
			}
			mapFile = XDocument.Load(game.Content.RootDirectory + game.PathSeperator + "Maps" + game.PathSeperator + Filename + ".tmx", LoadOptions.None).Element("map");
			var elements = mapFile.Descendants("objectgroup");
			foreach (XElement elem in elements)
			{
				switch (elem.Attribute("name").Value)
				{
					case ("Collision"):
						{
							var bounds = elem.Descendants("object");
							foreach (XElement bound in bounds)
							{
								if (bound.Element("polyline") != null)
								{
									//the values are parsed as float, then interpreted as int for compatability where certain old maps have
									//bounds set to non-whole numbers and fixing them would be a pain
									var s = new Point((int)float.Parse(bound.Attribute("x").Value), (int)float.Parse(bound.Attribute("y").Value));
									var difference = bound.Element("polyline");
									var args = difference.Attribute("points").Value.Split(' ');
									if (args.Length == 1)
										continue;
									args = args[1].Split(',');
									if (args.Length == 1)
										continue;
									var e = new Point(s.X + (int)float.Parse(args[0]), s.Y + (int)float.Parse(args[1]));
									Bounds.Add(new Line(s, e));
								}
							}
							break;
						}
					case ("NonFlyingCollision"):
						{
							var bounds = elem.Descendants("object");
							foreach (XElement bound in bounds)
							{
								if (bound.Element("polyline") != null)
								{
									var s = new Point((int)bound.Attribute("x"), (int)bound.Attribute("y"));
									var difference = bound.Element("polyline");
									var args = difference.Attribute("points").Value.Split(' ');
									if (args.Length == 1)
										continue;
									args = args[1].Split(',');
									if (args.Length == 1)
										continue;
									var e = new Point(s.X + int.Parse(args[0]), s.Y + int.Parse(args[1]));
									Bounds.Add(new Line(s, e));
									NonFlyingBounds.Add(new Line(s, e));
								}
							}
							break;
						}
					case ("Waypoints"):
						{
							var waypoints = elem.Descendants("object");
							foreach (XElement point in waypoints)
							{
								try { Waypoints.Add(point.Attribute("name").Value, new Point((int)point.Attribute("x"), (int)point.Attribute("y"))); }
								catch { }

							}
							break;
						}
					case ("Entities"):
						{
							fileEntities = elem.Elements("object");
							break;
						}
					case ("Spawns"):
						{
							var spawns = elem.Descendants("object");
							foreach (XElement spawn in spawns)
							{
								int spawnWidth = 80; int spawnHeight = 80;
								if (spawn.Attribute("width") != null)
								{
									spawnWidth = (int)spawn.Attribute("width");
									spawnHeight = (int)spawn.Attribute("height");
								}
								EnemySpawns.Add(new EnemySpawn(new Rectangle((int)spawn.Attribute("x"), (int)spawn.Attribute("y"), spawnWidth, spawnHeight),
									spawn.Attribute("name").Value, "", "", 100));
								var newSpawn = EnemySpawns[EnemySpawns.Count - 1];
								newSpawn.Chance = 100; //if there is no spawnchance set, this enemy will always spawn.
								if (spawn.Element("properties") != null)
								{
									var properties = spawn.Element("properties").Descendants("property");
									foreach (XElement property in properties)
									{
										if (property.Attribute("name").Value == "SpawnChance")
											newSpawn.Chance = (int)property.Attribute("value");
										if (property.Attribute("name").Value == "SpawnFlag")
											newSpawn.SpawnFlag = (string)property.Attribute("value");
										if (property.Attribute("name").Value == "KillFlag")
											newSpawn.KillFlag = (string)property.Attribute("value");
										if (property.Attribute("name").Value == "SpawnOnPoint")
											newSpawn.SpawnOnPoint = true;
									}
								}
							}
							break;
						}
					case ("Footsteps"):
						{
							walkSoundRecs = new List<KeyValuePair<Rectangle, string>>();
							var steps = elem.Descendants("object");
							foreach (XElement step in steps)
							{
								walkSoundRecs.Add(new KeyValuePair<Rectangle, string>(new Rectangle(new Point((int)step.Attribute("x"), (int)step.Attribute("y")), new Point((int)step.Attribute("width"), (int)step.Attribute("height"))), step.Attribute("name").Value));
							}
							break;
						}
				}
			}
			for (int i = 0; i < NonFlyingBounds.Count; i++)
			{
				for (int e = 0; e < Bounds.Count; e++)
				{
					if (Bounds[e].CompareAgainst(NonFlyingBounds[i]))
					{
						Bounds.RemoveAt(e);
						e--;
					}
				}
			}
			if (mapFile.Element("properties") != null)
			{
				var mapProperties = mapFile.Element("properties").Descendants("property");
				game.Camera.clampViewport = true;
				bool disableAmbience = true;
				foreach (XElement elem in mapProperties)
				{
					if (elem.Attribute("name").Value == "WalkSpeed")
						walkSpeed = int.Parse(elem.Attribute("value").Value);
					if (elem.Attribute("name").Value == "RunSpeed")
						runSpeed = int.Parse(elem.Attribute("value").Value);
					if (elem.Attribute("name").Value == "SneakSpeed")
						sneakSpeed = int.Parse(elem.Attribute("value").Value);
					if (elem.Attribute("name").Value == "Events")
						mapEvents = (Events.ParseEventFromFile(game.Content.RootDirectory + "\\Events\\" + elem.Attribute("value").Value + ".txt", game, this, null, true));
					if (elem.Attribute("name").Value == "NonFileEvent")
					{
						if (elem.Attribute("value") != null)
							mapEvents = Events.ParseEventFromProperty(elem.Attribute("value").Value, game, this, null);
						else
							mapEvents = Events.ParseEventFromProperty(elem.Value, game, this, null);
					}
					if (elem.Attribute("name").Value == "Aether")
						shaderNumber = 0;
					if (elem.Attribute("name").Value == "Flashback")
						shaderNumber = 1;
					if (elem.Attribute("name").Value == "Name")
						DisplayName = game.LoadString("Names", elem.Attribute("value").Value);
					if (elem.Attribute("name").Value == "Map")
						AreaMapName = elem.Attribute("value").Value;
					if (elem.Attribute("name").Value == "Music")
						MusicFile = elem.Attribute("value").Value;
					if (elem.Attribute("name").Value == "Ambience")
					{
						game.Audio.SetAmbience(elem.Attribute("value").Value, 60);
						disableAmbience = false;
					}
					if (elem.Attribute("name").Value == "DayMusic")
						if (game.gameFlags["bDay"])
							MusicFile = elem.Attribute("value").Value;
					if (elem.Attribute("name").Value == "NightMusic")
						if (game.gameFlags["bNight"])
							MusicFile = elem.Attribute("value").Value;
					if (elem.Attribute("name").Value == "Night" && game.gameFlags["bNight"] == true)
						WorldOverlays.Add(new Overlay(game, 0, 5, 2, 225));
					if (elem.Attribute("name").Value == "RedMist")
					{
						if (!game.gameFlags.ContainsKey("bRedMist"))
							game.gameFlags.Add("bRedMist", false);
						if (game.gameFlags["bRedMist"] == true)
						{
							WorldOverlays.Add(new MistOverlay(game, this));
						}
					}
					if (elem.Attribute("name").Value == "Snowfall")
					{
						if (!game.gameFlags.ContainsKey("bSnowing"))
							game.gameFlags.Add("bSnowing", false);
						if (game.gameFlags["bSnowing"] == true)
							WorldOverlays.Add(new Blizzard(game, this));
					}
					if (elem.Attribute("name").Value == "Darkness")
						WorldOverlays.Add(new Overlay(game, 0, 0, 0, 225));
					if (elem.Attribute("name").Value == "UnclampCamera")
						game.Camera.clampViewport = false;
					if (elem.Attribute("name").Value == "Cave")
						WorldOverlays.Add(new CaveOverlay(game, this));
					if (elem.Attribute("name").Value == "DoNotMakeTempSave")
						makeAutoSave = false;
					if (elem.Attribute("name").Value == "FleeMap")
						FleeDungeonMap = elem.Attribute("value").Value;
					if (elem.Attribute("name").Value == "FleePoint")
						FleeDungeonPoint = elem.Attribute("value").Value;
					if (elem.Attribute("name").Value == "Footstep")
					{
						walkSoundRecs = new List<KeyValuePair<Rectangle, string>>();
						walkSoundRecs.Add(new KeyValuePair<Rectangle, string>(new Rectangle(0, 0, Width * TileWidth, Height * TileHeight), elem.Attribute("value").Value));
					}
				}
				if (disableAmbience)
				{
					game.Audio.SetAmbience("null", 40);
				}
			}
		}
	}
}