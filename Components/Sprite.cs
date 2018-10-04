using System;
using System.Collections.Generic;
using System.Linq;
using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Audio;
using Microsoft.Xna.Framework.Content;
using Microsoft.Xna.Framework.Graphics;
using Microsoft.Xna.Framework.Media;

namespace Magicians
{
    class Sprite : IDisposable
    {
        public Texture2D spriteSheet { get; private set; }
        public int currentFrame { get; set; }
        Point drawnPosition; //the position at which the object is drawn.
        public Point DrawnPosition { get { return drawnPosition; } }
        public Point spriteSize;
        public int alpha = 255;

        Rectangle drawBounds;
        public Rectangle DrawnBounds { get { return drawBounds; } }
        public Rectangle spriteRect { get; private set; }

        float rotation;
        public float depth { get; private set; }
		float timer;
        public float interval { get; private set; }
        int bottomYOffset;
        public int bottomY; //the "bottom" of the sprite
        public bool ignoreDepthSorting;
        public string graphicsDir;


        public bool reachedEnd { get; private set; }
        SpriteEffects effects = SpriteEffects.None;
        bool looping = true;
        public enum OriginType { TopLeft, FromCentre, BottomLeft, BottomMiddle };
        OriginType originMode;
        public OriginType OriginMode { get { return originMode; }}
        public bool scaled; //if this is set to true, then the sprite is scaled up to the specified width and height
        public int scaleWidth, scaleHeight;

        bool flickering; //if this is set to true, the sprite will have a random alpha set to somewhere between the upper and lower bounds every update;
        int flickerLowerBound;
        int flickerUpperBound;
        Random randFlicker;

        public Color[] GetFrameData() //gets the current frame data as as an array
        {
            Color[] colors = new Color[spriteSize.X * spriteSize.Y];
            spriteSheet.GetData<Color>(0,spriteRect,colors, 0, colors.Length);
            return colors;
        }

		public Sprite(TextureLoader TextureLoader,string filename, Point pos, float d, Point spritesize, OriginType som)
        {
            drawnPosition = pos;
            currentFrame = 0;
            spriteRect = new Rectangle(currentFrame * spritesize.X, 0, spritesize.X, spritesize.Y);
            spriteSize = spritesize;
            rotation = MathHelper.Pi * 0.0f;
            depth = d;
            interval = 190;
            if (filename != null)
                Load(filename,TextureLoader);
            originMode = som;
            reachedEnd = false;
            graphicsDir = filename;
            if (graphicsDir != null)
            {
                for (int i = graphicsDir.Length - 2; i > 0; i--)
                {
                    var s = graphicsDir.Substring(i, 2);
                    if (s.Contains("\\"))
                    {
                        graphicsDir = graphicsDir.Substring(0, i);
                        break;
                    }
                }
            }
            if(spriteSize == Point.Zero)
                spriteSize = new Point(spriteSheet.Width, spriteSheet.Height);
            drawBounds = new Rectangle(drawnPosition.X, drawnPosition.Y, spriteSize.X, spriteSize.Y);
        }
        public void SetScale(int w, int h)
        {
            scaled = true;
            scaleWidth = w;
            scaleHeight = h;
            currentFrame = 0;
        }
        public void SetSpriteSize() 
        {
            if (spriteSheet != null)
            {
                spriteSize = new Point(spriteSheet.Width, spriteSheet.Height);
            }
        }
        public void SetSpriteSize(int x, int y)
        {
            spriteSize.X = x;
            spriteSize.Y = y;
        }

		internal void Load(string filename, TextureLoader tl)
        {
            spriteSheet = tl.RequestTexture(filename);
        }
        public void SetFlicker(int l, int u)
        {
            flickering = true;
            flickerLowerBound = MathHelper.Clamp(l,0,255);
            flickerUpperBound = MathHelper.Clamp(u, 0, 255);
            randFlicker = new Random(drawnPosition.X);
        }
        public void Update(GameTime gameTime)
        {
            if(flickering)
                alpha = randFlicker.Next(flickerLowerBound,flickerUpperBound);
            timer += (float)gameTime.ElapsedGameTime.TotalMilliseconds;
            if (timer > interval)
            {
                if (this.spriteSheet.Width == spriteSize.X)
                {
                    timer = 0;
                    reachedEnd = true;
                    goto UpdateSprite;
                }
                    currentFrame++;
                    if (currentFrame * spriteSize.X == spriteSheet.Width - spriteSize.X)
                    {
                        reachedEnd = true;
                    }
                if (currentFrame * spriteSize.X > spriteSheet.Width - spriteSize.X)
                {
                    currentFrame = 0;
                }
                timer = 0;
            }
            UpdateSprite:
            if (!scaled)
                bottomY = (drawnPosition.Y + spriteSize.Y) + bottomYOffset;
            else
                bottomY = (drawnPosition.Y + scaleHeight) + bottomYOffset;
            if(originMode == OriginType.BottomMiddle)
            {
                bottomY -= spriteSize.Y;
            }
        }
        public void Draw(SpriteBatch batch)
        {
            spriteRect = new Rectangle(currentFrame * spriteSize.X, 0, spriteSize.X, spriteSize.Y);            
            if (scaled)
                drawBounds = new Rectangle(drawnPosition.X, drawnPosition.Y, scaleWidth, scaleHeight);
            else
                drawBounds = new Rectangle(drawnPosition.X, drawnPosition.Y, spriteSize.X, spriteSize.Y);
            switch (originMode)
            {
                case (OriginType.TopLeft):
                    {
                        batch.Draw(spriteSheet, drawBounds, spriteRect, new Color(255,255,255,alpha), rotation, new Vector2(0, 0), effects, depth);
                        break;
                    }
                case (OriginType.FromCentre):
                    {
                        batch.Draw(spriteSheet, drawBounds, spriteRect, new Color(255, 255, 255, alpha), rotation, new Vector2(spriteSize.X / 2, spriteSize.Y / 2), effects, depth);
                        break;
                    }
                case (OriginType.BottomMiddle):
                    {
                        batch.Draw(spriteSheet, drawBounds, spriteRect, new Color(255, 255, 255, alpha), rotation, new Vector2(spriteSize.X / 2, spriteSize.Y), effects, depth);
                        break;
                    }
            }
        }
        public void ResetTimer()
        {
            timer = 0;
        }
        public void Draw(SpriteBatch batch,Color color)
        {
            spriteRect = new Rectangle(currentFrame * spriteSize.X, 0, spriteSize.X, spriteSize.Y);
            drawBounds = new Rectangle(drawnPosition.X, drawnPosition.Y, spriteSize.X, spriteSize.Y);
            switch (originMode)
            {
                case (OriginType.TopLeft):
                    {
                        batch.Draw(spriteSheet, drawBounds, spriteRect, Color.FromNonPremultiplied(color.R, color.G, color.B, alpha), rotation, new Vector2(0, 0), effects, depth);
                        break;
                    }
                case (OriginType.FromCentre):
                    {
                        batch.Draw(spriteSheet, drawBounds, spriteRect, Color.FromNonPremultiplied(color.R, color.G, color.B, alpha), rotation, new Vector2(spriteSize.X / 2, spriteSize.Y / 2), effects, depth);
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
            drawnPosition = point;
        }
        public void ChangeSprite(TextureLoader TextureLoader, string filename)
        {
            filename = filename.TrimStart('\\');
            this.Load(filename, TextureLoader);
            currentFrame = 0;
            reachedEnd = false;
            if (graphicsDir == null)
            {
                graphicsDir = filename;
            }
        }
        public void ChangeTexture2D(Texture2D tex)
        {
            this.spriteSheet = tex;
            if (spriteSheet.Width == spriteSize.X)
            {
                currentFrame = 0;
                reachedEnd = false;
            }
            if(currentFrame * spriteSize.X > spriteSheet.Width)
            {
                currentFrame = 0;
                reachedEnd = false;
            }
        }
        public void ChangeTexture2D(Texture2D tex, bool resetFrame)
        {
            this.spriteSheet = tex;
            if (resetFrame)
            {
                timer = 0;
                currentFrame = 0;
                reachedEnd = false;
            }
        }
        public void ChangeSpriteEffects(SpriteEffects se)
        {
            effects = se;
        }
        public void Dispose()
        {
            this.spriteSheet.Dispose();
        }
        public void ChangeLooping(bool b)
        {
            looping = b;
        }
        public void ChangeDepth(float f)
        {
            depth = f;
        }
        public void SetInterval(float f)
        {
            interval = f;
        }
        public void SetInterval(float f, bool resetTimer)
        {
            interval = f;
            if (resetTimer)
                timer = 0;
        }
        public void RandomizeFrame(Random random)
        {
			if (spriteSheet.Width % spriteSize.X == 0)
                currentFrame = random.Next(0, (spriteSheet.Width / spriteSize.X) - 1);
        }
        public void SetBottomYOffset(int i)
        {
            bottomYOffset = i;
        }
    }
}
