using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;
using Microsoft.Xna.Framework.Input;

namespace Magicians
{
	class Input
	{
		public KeyboardState oldKeyboardState { get; private set; }
		public MouseState oldMouseState { get; private set; }
		Game game;
		Point MonitorWidth;
		public Input(Game g)
		{
			game = g;
			MonitorWidth = new Point();
			MonitorWidth.X = g.GraphicsDevice.DisplayMode.Width;
			MonitorWidth.Y = g.GraphicsDevice.DisplayMode.Height;
		}
		public void Update()
		{
			oldKeyboardState = Keyboard.GetState();
			oldMouseState = Mouse.GetState();
		}
		public int GetNumberOfKeysPressed()
		{
			var state = Keyboard.GetState();
			return state.GetPressedKeys().Length;
		}
		public bool IsMouseOver(Rectangle rec)
		{
			if (game.IsActive)
			{
				var state = Mouse.GetState();
				var point = new Vector2(state.X, state.Y);
				if (game.graphics.IsFullScreen)
				{
					var scale = new Vector2();
					scale.X = (float)game.graphics.PreferredBackBufferWidth / MonitorWidth.X;
					scale.Y = (float)game.graphics.PreferredBackBufferHeight / MonitorWidth.Y;
					point.X = point.X * scale.X;
					point.Y = point.Y * scale.Y;
				}
				if (rec.Contains(point.X, point.Y))
					return true;
			}
			return false;
		}
		public bool IsOldMouseOver(Rectangle rec)
		{
			if (game.IsActive)
			{
				if (rec.Contains(oldMouseState.X, oldMouseState.Y))
				{
					return true;
				}
			}
			return false;
		}
		public bool IsMouseButtonReleased()
		{
			if (game.IsActive)
			{
				var newState = Mouse.GetState();
				if (oldMouseState.LeftButton == ButtonState.Pressed && newState.LeftButton == ButtonState.Released)
					return true;
			}
			return false;
		}
		public bool IsMouseButtonReleased(bool rightClick)
		{
			if (game.IsActive)
			{
				var newState = Mouse.GetState();
				if (rightClick)
				{
					if (oldMouseState.RightButton == ButtonState.Pressed && newState.RightButton == ButtonState.Released)
						return true;
				}
				else return IsMouseButtonReleased();
			}
			return false;
		}
		public bool IsMouseButtonPressed()
		{
			if (game.IsActive)
			{
				var newState = Mouse.GetState();
				if (newState.LeftButton == ButtonState.Pressed)
				{
					return true;
				}
			}
			return false;
		}
		public bool HasMouseClickedOnRectangle(Rectangle rec)
		{
			if (IsMouseOver(rec) && IsMouseButtonReleased())
			{
				return true;
			}
			return false;
		}
		public bool HasMouseClickedOnRectangle(Rectangle rec, bool rightClick)
		{
			if (IsMouseOver(rec) && IsMouseButtonReleased(rightClick))
				return true;
			return false;
		}
		public bool HasMouseClickedOnRectangle(Rectangle rec, bool adjustedForCameraOffset, bool rightClick)
		{
			if (adjustedForCameraOffset)
			{
				if (game.IsActive)
				{
					var state = Mouse.GetState();
					var point = new Vector2(state.X, state.Y);
					if (game.graphics.IsFullScreen)
					{
						var scale = new Vector2();
						scale.X = (float)game.graphics.PreferredBackBufferWidth / MonitorWidth.X;
						scale.Y = (float)game.graphics.PreferredBackBufferHeight / MonitorWidth.Y;
						point.X = point.X * scale.X;
						point.Y = point.Y * scale.Y;
					}
					var test = new Rectangle(rec.X, rec.Y, rec.Width, rec.Height);
					float x = point.X + (game.Camera.Position.X - (game.GetScreenWidth() / 2));
					float y = point.Y + (game.Camera.Position.Y - (game.GetScreenHeight() / 2));
					if (test.Contains(x, y))
						return IsMouseButtonReleased(rightClick);
				}
				return false;
			}
			return IsMouseButtonReleased(rightClick);
		}
		//not self explanatory and definitely worth a comment
		public bool IsKeyPressed(Keys key)
		{
			if (game.IsActive)
			{
				var state = Keyboard.GetState();
				if (state.IsKeyDown(key))
					return true;
			}
			return false;
		}
		public bool IsKeyReleased(Keys key)
		{
			if (game.IsActive)
			{
				var state = Keyboard.GetState();
				if (oldKeyboardState.IsKeyDown(key) && !state.IsKeyDown(key))
					return true;
			}
			return false;
		}
		public bool AreKeysPressed(Keys[] keys)
		{
			if (game.IsActive)
			{
				var state = Keyboard.GetState();
				for (int i = 0; i < keys.Length; i++)
				{
					if (!state.IsKeyDown(keys[i]))
						return false;
				}
				return true;
			}
			return false;
		}
		public bool AreAnyKeysPressed(Keys[] keys)
		{
			if (game.IsActive)
			{
				var state = Keyboard.GetState();
				for (int i = 0; i < keys.Length; i++)
				{
					if (state.IsKeyDown(keys[i]))
						return true;
				}
				return true;
			}
			return false;
		}
		public bool IsFirstKeyPressed(Keys key1, Keys key2)
		{
			if (game.IsActive)
			{
				var state = Keyboard.GetState();
				if (state.IsKeyDown(key1) && !state.IsKeyDown(key2))
					return true;
			}
			return false;
		}
		//returns true if no keys on the keyboard are being pressed
		public bool NoKeysPressed()
		{
			var state = Keyboard.GetState();
			if (state.GetPressedKeys().Length == 0)
				return true;
			return false;
		}
	}
}