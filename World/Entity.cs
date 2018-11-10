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
                Bounds.Update(Position);
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
            Bounds = new Bounds(this.Position, size.X, size.Y, PassThrough, offset);
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
}
