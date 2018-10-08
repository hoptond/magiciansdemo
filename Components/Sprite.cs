using System;
using System.Collections.Generic;
using System.Linq;
using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Audio;
using Microsoft.Xna.Framework.Content;
using Microsoft.Xna.Framework.Graphics;

namespace Magicians
{
	class Sprite
	{
		public Texture2D SpriteSheet { get; private set; }
		public int CurrentFrame { get; set; }
		public Point DrawnPosition { get; private set; } //the position at which the object is drawn.
		public Point SpriteSize { get; private set; }
		public Rectangle DrawnBounds { get; private set; }
		public Rectangle SpriteRect { get; private set; }
		public float Depth { get; private set; }
		public float Interval { get; private set; }
		public int BottomY { get; private set; } //the "bottom" of the sprite      
		public bool ReachedEnd { get; private set; }
		public bool IgnoreDepthSorting { get; private set; }
		public string GraphicsDir { get; private set; }
		public enum OriginType { TopLeft, FromCentre, BottomLeft, BottomMiddle };
		public OriginType OriginMode { get; private set; }
		public bool Scaled { get; private set; } //if this is set to true, then the sprite is Scaled up to the specified width and height
		public int scaleWidth { get; private set; }
		public int scaleHeight { get; private set; }

		int alpha = 255;
		float rotation;
		float timer;
		int bottomYOffset;
		SpriteEffects effects = SpriteEffects.None;
		bool looping = true;
		OriginType originMode;
		bool flickering; //if this is set to true, the sprite will have a random alpha set to somewhere between the upper and lower bounds every update;
		int flickerLowerBound;
		int flickerUpperBound;
		Random randFlicker;

		public Sprite(TextureLoader TextureLoader, string filename, Point pos, float d, Point spritesize, OriginType som)
		{
			DrawnPosition = pos;
			CurrentFrame = 0;
			SpriteRect = new Rectangle(CurrentFrame * SpriteSize.X, 0, SpriteSize.X, SpriteSize.Y);
			SpriteSize = spritesize;
			rotation = MathHelper.Pi * 0.0f;
			Depth = d;
			Interval = 190;
			if (filename != null)
				Load(filename, TextureLoader);
			originMode = som;
			ReachedEnd = false;
			GraphicsDir = filename;
			if (GraphicsDir != null)
			{
				for (int i = GraphicsDir.Length - 2; i > 0; i--)
				{
					var s = GraphicsDir.Substring(i, 2);
					if (s.Contains("\\"))
					{
						GraphicsDir = GraphicsDir.Substring(0, i);
						break;
					}
				}
			}
			if (SpriteSize == Point.Zero && SpriteSheet != null)
				SpriteSize = new Point(SpriteSheet.Width, SpriteSheet.Height);
			DrawnBounds = new Rectangle(DrawnPosition.X, DrawnPosition.Y, SpriteSize.X, SpriteSize.Y);
		}

		public Color[] GetFrameData() //gets the current frame data as as an array
		{
			Color[] colors = new Color[SpriteSize.X * SpriteSize.Y];
			SpriteSheet.GetData<Color>(0, SpriteRect, colors, 0, colors.Length);
			return colors;
		}

		public void SetScale(int w, int h)
		{
			Scaled = true;
			scaleWidth = w;
			scaleHeight = h;
			CurrentFrame = 0;
		}
		public void SetSpriteSize()
		{
			if (SpriteSheet != null)
				SpriteSize = new Point(SpriteSheet.Width, SpriteSheet.Height);
		}
		public void SetSpriteSize(int x, int y)
		{
			SpriteSize = new Point(x, y);
		}

		internal void Load(string filename, TextureLoader tl)
		{
			SpriteSheet = tl.RequestTexture(filename);
		}
		public void SetFlicker(int l, int u)
		{
			flickering = true;
			flickerLowerBound = MathHelper.Clamp(l, 0, 255);
			flickerUpperBound = MathHelper.Clamp(u, 0, 255);
			randFlicker = new Random(DrawnPosition.X);
		}
		public void Update(GameTime gameTime)
		{
			if (flickering)
				alpha = randFlicker.Next(flickerLowerBound, flickerUpperBound);
			timer += (float)gameTime.ElapsedGameTime.TotalMilliseconds;
			if (timer > Interval)
			{
				if (SpriteSheet.Width == SpriteSize.X)
				{
					timer = 0;
					ReachedEnd = true;
					goto UpdateSprite;
				}
				CurrentFrame++;
				if (CurrentFrame * SpriteSize.X == SpriteSheet.Width - SpriteSize.X)
				{
					ReachedEnd = true;
				}
				if (CurrentFrame * SpriteSize.X > SpriteSheet.Width - SpriteSize.X)
					CurrentFrame = 0;
				timer = 0;
			}
		UpdateSprite:
			if (!Scaled)
				BottomY = (DrawnPosition.Y + SpriteSize.Y) + bottomYOffset;
			else
				BottomY = (DrawnPosition.Y + scaleHeight) + bottomYOffset;
			if (originMode == OriginType.BottomMiddle)
			{
				BottomY -= SpriteSize.Y;
			}
		}
		public void Draw(SpriteBatch batch)
		{
			SpriteRect = new Rectangle(CurrentFrame * SpriteSize.X, 0, SpriteSize.X, SpriteSize.Y);
			if (Scaled)
				DrawnBounds = new Rectangle(DrawnPosition.X, DrawnPosition.Y, scaleWidth, scaleHeight);
			else
				DrawnBounds = new Rectangle(DrawnPosition.X, DrawnPosition.Y, SpriteSize.X, SpriteSize.Y);
			switch (originMode)
			{
				case (OriginType.TopLeft):
					{
						batch.Draw(SpriteSheet, DrawnBounds, SpriteRect, new Color(255, 255, 255, alpha), rotation, new Vector2(0, 0), effects, Depth);
						break;
					}
				case (OriginType.FromCentre):
					{
						batch.Draw(SpriteSheet, DrawnBounds, SpriteRect, new Color(255, 255, 255, alpha), rotation, new Vector2(SpriteSize.X / 2, SpriteSize.Y / 2), effects, Depth);
						break;
					}
				case (OriginType.BottomMiddle):
					{
						batch.Draw(SpriteSheet, DrawnBounds, SpriteRect, new Color(255, 255, 255, alpha), rotation, new Vector2(SpriteSize.X / 2, SpriteSize.Y), effects, Depth);
						break;
					}
			}
		}

		public void ResetTimer()
		{
			timer = 0;
		}

		public void Draw(SpriteBatch batch, Color color)
		{
			SpriteRect = new Rectangle(CurrentFrame * SpriteSize.X, 0, SpriteSize.X, SpriteSize.Y);
			DrawnBounds = new Rectangle(DrawnPosition.X, DrawnPosition.Y, SpriteSize.X, SpriteSize.Y);
			switch (originMode)
			{
				case (OriginType.TopLeft):
					{
						batch.Draw(SpriteSheet, DrawnBounds, SpriteRect, Color.FromNonPremultiplied(color.R, color.G, color.B, alpha), rotation, new Vector2(0, 0), effects, Depth);
						break;
					}
				case (OriginType.FromCentre):
					{
						batch.Draw(SpriteSheet, DrawnBounds, SpriteRect, Color.FromNonPremultiplied(color.R, color.G, color.B, alpha), rotation, new Vector2(SpriteSize.X / 2, SpriteSize.Y / 2), effects, Depth);
						break;
					}
			}
		}

		public void ChangeRotation(float f)
		{
			rotation = f;
		}
		public void ChangeDrawnPosition(Point point)
		{
			DrawnPosition = point;
		}
		public void ChangeSprite(TextureLoader TextureLoader, string filename)
		{
			filename = filename.TrimStart('\\');
			this.Load(filename, TextureLoader);
			CurrentFrame = 0;
			ReachedEnd = false;
			if (GraphicsDir == null)
			{
				GraphicsDir = filename;
			}
		}
		public void ChangeTexture2D(Texture2D tex)
		{
			SpriteSheet = tex;
			if (SpriteSheet.Width == SpriteSize.X)
			{
				CurrentFrame = 0;
				ReachedEnd = false;
			}
			if (CurrentFrame * SpriteSize.X > SpriteSheet.Width)
			{
				CurrentFrame = 0;
				ReachedEnd = false;
			}
		}
		public void ChangeTexture2D(Texture2D tex, bool resetFrame)
		{
			this.SpriteSheet = tex;
			if (resetFrame)
			{
				timer = 0;
				CurrentFrame = 0;
				ReachedEnd = false;
			}
		}
		public void ChangeSpriteEffects(SpriteEffects se)
		{
			effects = se;
		}
		public void ChangeLooping(bool b)
		{
			looping = b;
		}
		public void ChangeDepth(float f)
		{
			Depth = f;
		}
		public void SetInterval(float f)
		{
			Interval = f;
		}
		public void SetInterval(float f, bool resetTimer)
		{
			Interval = f;
			if (resetTimer)
				timer = 0;
		}
		public void RandomizeFrame(Random random)
		{
			if (SpriteSheet.Width % SpriteSize.X == 0)
				CurrentFrame = random.Next(0, (SpriteSheet.Width / SpriteSize.X) - 1);
		}
		public void SetBottomYOffset(int i)
		{
			bottomYOffset = i;
		}
		public void SetAlpha(int i)
		{
			alpha = MathHelper.Clamp(i, 0, 255);
		}
		public void SetIgnoreDepthSorting(bool ignore)
		{
			IgnoreDepthSorting = ignore;
		}
		public void SetGraphicsDir(string dir)
		{
			GraphicsDir = dir;
		}
	}
}