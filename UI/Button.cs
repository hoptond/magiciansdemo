using System;
using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Content;
using Microsoft.Xna.Framework.Graphics;

namespace Magicians
{
    //if you click the button something magical happens
    class Button
    {
        readonly Audio Audio;
        readonly Input Input;
        readonly Texture2D Icon;
        readonly Texture2D Highlight;
        readonly string Text;
        readonly float Depth;

        public Rectangle Bounds { get; private set; }
        public Point Position { get; private set; }

        Color baseColor = Color.White;
        Color highlightColor = Color.White;
        bool drawHighlight;

        public Button(Audio audio, Input input, Texture2D ico, Point position, string text, Texture2D highlight, float d)
        {
            Audio = audio;
            Input = input;
            Icon = ico;
            Position = position;
            Bounds = new Rectangle(Position.X, Position.Y, ico.Width, ico.Height);
            Text = text;
            Highlight = highlight;
            Depth = d;
            Bounds = new Rectangle(Position.X, Position.Y, Icon.Width, Icon.Height);
        }
        public Button(Audio a, Input i, Texture2D ico, Point position, int width, int height, string text, Texture2D highlight, float d)
        {
            Audio = a;
            Input = i;
            Icon = ico;
            Position = position;
            Bounds = new Rectangle(Position.X, Position.Y, width, height);
            Text = text;
            Depth = d;
            Highlight = highlight;
        }
        public bool Activated()
        {
            if (Input.HasMouseClickedOnRectangle(Bounds))
                return true;
            return false;
        }

        public void SetBaseColor(Color clr)
        {
            baseColor = clr;
        }

        public void Update(GameTime gameTime)
        {
            if (Input.IsMouseOver(Bounds) && !Input.IsOldMouseOver(Bounds))
                Audio.PlaySound("MouseOverButton", false);
            if (Input.HasMouseClickedOnRectangle(Bounds))
                Audio.PlaySound("ButtonClick", true);
            if (Input.IsMouseOver(Bounds))
            {
                drawHighlight = true;
            }
            if (Input.IsMouseButtonPressed())
                highlightColor = Color.LightGray;
            else
                highlightColor = Color.White;
        }
        public void Draw(SpriteBatch spriteBatch)
        {
            spriteBatch.Draw(Icon, Bounds, null, baseColor, 0, Vector2.Zero, SpriteEffects.None, Depth);
            if (drawHighlight)
                spriteBatch.Draw(Highlight, Bounds, null, highlightColor, 0, Vector2.Zero, SpriteEffects.None, Depth - 0.001f);
            drawHighlight = false;
        }
    }
}
