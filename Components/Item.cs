using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Content;
using Microsoft.Xna.Framework.Graphics;

namespace Magicians
{
    class Item
    {
		public readonly string DisplayName;
		public readonly string InternalName;
		readonly string iconFile; //the file path to the icon that will appear. filename only, 
		public Texture2D Icon { get; private set; }

		public readonly int Value; //value in gold
		public string Description { get; private set; } //cosmetic description of the item
		public readonly Usage Usage;


        public void Load(TextureLoader tl, SpriteFont smallFont)
        {
            Icon = tl.RequestTexture("UI\\Icons\\Items\\" + iconFile);
            Description = TextMethods.WrapText(smallFont, Description, 200);
        }

		public Item(string n, string i, string g, int v, string d, Usage us)
        {
            DisplayName = n;
            InternalName = i;
            iconFile = g;
            Value = v;
            Description = d;
			Usage = us;
        }
    }
}