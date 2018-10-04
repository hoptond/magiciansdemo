using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;

namespace Magicians
{
    static class DrawMethods
    {
        public static void DrawToolTip(SpriteBatch spriteBatch, Vector2 vec, SpriteFont font, Texture2D window, string s)
        {
            spriteBatch.Draw(window, new Rectangle((int)vec.X, (int)vec.Y, 400, 76), null, Color.White, 0.0f, Vector2.Zero, SpriteEffects.None, 0.002f);
            spriteBatch.DrawString(font, TextMethods.WrapText(font,s,380), new Vector2(vec.X + 12, vec.Y + 8), Color.Black, 0.0f, Vector2.Zero, 1.0f, SpriteEffects.None, 0.001f);
        }
    }

}
