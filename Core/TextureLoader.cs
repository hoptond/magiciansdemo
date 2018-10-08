using System;
using System.Collections.Generic;
using System.Text;
using Microsoft.Xna.Framework.Graphics;
using Microsoft.Xna.Framework.Content;

namespace Magicians
{
	class TextureLoader
	{
		readonly ContentManager Content;
		readonly SortedList<string, Texture2D> Textures;

		public Texture2D RequestTexture(string textureFilePath)
		{
			if (Textures.ContainsKey(textureFilePath))
			{
				return Textures[textureFilePath];
			}
			Textures.Add(textureFilePath, Content.Load<Texture2D>(textureFilePath));
			return Textures[textureFilePath];
		}
		public TextureLoader(ContentManager c)
		{
			Content = c;
			Textures = new SortedList<string, Texture2D>();
		}
	}
}