using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;
using Microsoft.Xna.Framework.Content;

namespace Magicians
{
	class SpriteEffect
	{
		Point position;
		public Sprite sprite { get; private set; }
		public Mover mover { get; private set; }
		public bool isFinished { get; private set; }
		bool selfTerminating; //set to false to have an effect that only terminates on command
		Sprite attachedSprite;
		float depthDiff; //the depth this sprite should be set at in relation to the sprite it is attached to

		public void Update(GameTime gameTime)
		{
			if (mover != null)
			{
				if (mover.Target != Point.Zero)
				{
					var movement = new Vector2(mover.Target.X - position.X, mover.Target.Y - position.Y);
					if (movement.Length() < mover.Speed)
					{
						position = mover.Target;
						isFinished = true;
					}
					movement.X = movement.X / Vector2.Distance(position.ToVector2(), mover.Target.ToVector2());
					movement.Y = movement.Y / Vector2.Distance(position.ToVector2(), mover.Target.ToVector2());
					movement.X = movement.X * mover.Speed;
					movement.Y = movement.Y * mover.Speed;
					movement.X = (int)Math.Round(movement.X, 0);
					movement.Y = (int)Math.Round(movement.Y, 0);
					position += movement.ToPoint();
					if (position == mover.Target)
						isFinished = true;
				}
			}
			if (selfTerminating == true)
			{
				if (sprite.ReachedEnd == true)
					this.isFinished = true;
			}
			sprite.ChangeDrawnPosition(position);
			sprite.Update(gameTime);
		}

		public void Draw(SpriteBatch spriteBatch)
		{
			sprite.Draw(spriteBatch);
		}
		public void SetDepth()
		{
			if (attachedSprite != null)
				sprite.ChangeDepth(attachedSprite.Depth + depthDiff);
		}

		public SpriteEffect(TextureLoader TextureLoader, Point pos, Point size, string filepath, bool b, Sprite attach)
		{
			position = pos;
			sprite = new Sprite(TextureLoader, filepath, pos, 0.0f, size, Sprite.OriginType.FromCentre);
			isFinished = false;
			selfTerminating = b;
			attachedSprite = attach;
			depthDiff = -0.00001f;
		}
		public SpriteEffect(TextureLoader TextureLoader, Point pos, Point size, string filepath, bool b)
		{
			position = pos;
			sprite = new Sprite(TextureLoader, filepath, pos, 0.0f, size, Sprite.OriginType.FromCentre);
			isFinished = false;
			selfTerminating = b;
		}
		public SpriteEffect(TextureLoader TextureLoader, Point pos, Point size, string filepath, bool b, int i)
		{
			position = pos;
			sprite = new Sprite(TextureLoader, filepath, pos, 0.0f, size, Sprite.OriginType.FromCentre);
			isFinished = false;
			selfTerminating = b;
			sprite.SetInterval(i);
		}
		public SpriteEffect(TextureLoader TextureLoader, Point pos, Point size, string filepath, bool b, bool i)
		{
			position = pos;
			sprite = new Sprite(TextureLoader, filepath, pos, 0.0f, size, Sprite.OriginType.FromCentre);
			sprite.SetIgnoreDepthSorting(i);
			isFinished = false;
			selfTerminating = b;
		}
		public void SetMovement(Mover mov, Point targ)
		{
			mover = mov;
			mover.SetTarget(targ);
		}
	}
}
