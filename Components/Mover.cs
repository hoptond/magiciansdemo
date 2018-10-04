using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using Microsoft.Xna.Framework;

namespace Magicians
{
    class Mover
    {
        Entity Entity;
        public Vector2 Movement;
        public int Speed { get; private set; }
        public enum MovementType { Directional, Linear }
        MovementType MoverType;
        public Point Target { get; private set; }
        public Directions direction { get; private set; }
        public Directions lastDirection { get; private set; }
        //public Directions lastDir { get; private set; } //the last direction this mover moved in. 
        public List<Point> Waypoints = new List<Point>();

        //used if we want our walker to turn around realistically, and not on a dime.
        float turnInterval;
        bool canTurn;
        public float turnTimer;
        bool clockwise = true; //is the entity turning clockwise or counterclockwise?

        public void SetTurnInterval(float f)
        {
            turnInterval = f;
            turnTimer = 0;
        }

        public bool DiagonalMovement()
        {
			if ((int)Movement.X != 0 && (int)Movement.Y != 0)
            {
                return true;
            }
            return false;
        }
		public void Update(GameTime gameTime)
        {
            if (turnInterval > 0)
            {
                if (turnTimer >= 0)
                {
                    turnTimer -= (float)gameTime.ElapsedGameTime.TotalSeconds;
                    if (turnTimer <= 0)
                    {
                        turnTimer = 0;
                        canTurn = true;
                    }
                }
            }
            if (this.Target != Point.Zero)
            {
                if (this.MoverType == MovementType.Directional)
                {
                    if ((int)Movement.X != 0)
                    {
                        for (int i = 0; i != (int)Movement.X;)
                        {
                            if (Movement.X < 0)
                            {
                                Entity.ChangePosition(new Point(Entity.Position.X - 1, Entity.Position.Y));
                                i--;
                            }
                            else
                            {
                                Entity.ChangePosition(new Point(Entity.Position.X + 1, Entity.Position.Y));
                                i++;
                            }
                            if (Entity.Position.X == Target.X)
                            {
                                Movement = new Vector2(0, Movement.Y);
                                break;
                            }
                        }
                    }
                    if ((int)Movement.Y != 0)
                    {
                        for (int i = 0; i != (int)Movement.Y;)
                        {
                            if (Movement.Y < 0)
                            {
                                Entity.ChangePosition(new Point(Entity.Position.X, Entity.Position.Y - 1));
                                i--;
                            }
                            else
                            {
                                Entity.ChangePosition(new Point(Entity.Position.X, Entity.Position.Y + 1));
                                i++;
                            }
                            if (Entity.Position.Y == Target.Y)
                            {
                                Movement = new Vector2(Movement.X, 0);
                                break;
                            }
                        }
                    }
                }
            }
        }
        public Vector2 ReturnLinearMovement(Point position)
        {
            Vector2 movement = (Target.ToVector2() - position.ToVector2());
            if (movement.Length() < Speed)
            {
                return Vector2.Zero;
            }
            movement.X = movement.X / Vector2.Distance(position.ToVector2(), Target.ToVector2());
            movement.Y = movement.Y / Vector2.Distance(position.ToVector2(), Target.ToVector2());
            movement.X = movement.X * Speed;
            movement.Y = movement.Y * Speed;
            movement.X = (float)Math.Round(movement.X, 0);
            movement.Y = (float)Math.Round(movement.Y, 0);
            return movement;
        }
        public Mover(Entity ent,int sp, MovementType movtype)
        {
            Entity = ent;
            Speed = sp;
            MoverType = movtype;
            Target = Point.Zero;
        }
        public void ChangeSpeed(int s)
        {
            Speed = s;
        }
        public void ChangeDirection(Directions dir)
        {
            lastDirection = direction;
            if ((int)dir == 9)
                dir = Directions.Up;
            if (dir == 0)
                dir = Directions.UpLeft;
			if (turnInterval <= 0 || (turnInterval > 0 && canTurn))
			{
				direction = dir;
				return;
			}
        }
        public void ChangeMovement(Directions dir)
        {
            lastDirection = direction;
            if (turnInterval <= 0)
            {
                changeMovement(dir);
                return;
            }
			if (dir == Directions.None)
            {
                changeMovement(dir);
                return;
            }
            if (canTurn)
            {
                if (clockwise)
                    ChangeDirection(direction + 1);
                else
                    ChangeDirection(direction - 1);
                canTurn = false;
            }
            if (MathHelper.Distance((int)lastDirection, (int)dir) <= 1)
            {
                canTurn = false;
                changeMovement(dir);
                return;
            }
			turnTimer = turnInterval;
            if (direction == Directions.Left && dir == Directions.Right)
            {
                clockwise = true;
            }
            if ((int)direction == 8 && (int)dir < 5 || (int)direction < 2 && (int)dir > 5)
            {
                if ((int)dir > (int)direction)
                    clockwise = false;
                else
                    clockwise = true;
            }
            else
            {
                if ((int)dir < (int)direction)
                    clockwise = false;
                else
                    clockwise = true;
            }
        }
        void changeMovement(Directions dir)
        {
            if (dir != Directions.None)
                direction = dir;
            switch (dir)
            {
                case (Directions.Up): { Movement.X = 0; Movement.Y = -Speed; break; }
                case (Directions.UpRight): { Movement.X = Speed; Movement.Y = -Speed; break; }
                case (Directions.Right): { Movement.X = Speed; Movement.Y = 0; break; }
                case (Directions.DownRight): { Movement.X = Speed; Movement.Y = Speed; break; }
                case (Directions.Down): { Movement.X = 0; Movement.Y = Speed; break; }
                case (Directions.DownLeft): { Movement.X = -Speed; Movement.Y = Speed; break; }
                case (Directions.Left): { Movement.X = -Speed; Movement.Y = 0; break; }
                case (Directions.UpLeft): { Movement.X = -Speed; Movement.Y = -Speed; break; }
                case (Directions.None): { Movement.X = 0; Movement.Y = 0; break; }
            }
        }
        public void SetTarget(Point point)
        {
            if (point == Point.Zero)
            {
                Movement = Vector2.Zero;
                Target = Point.Zero;
                Waypoints.Clear();
                return;
            }
            Target = point;
        }
        public Point ReturnDirectionPoint()
        {
            var p = new Point(0);
            switch (direction)
            {
                case Directions.Right: p = new Point(1, 0); break;
                case Directions.Left: p = new Point(-1, 0); break;
                case Directions.Up: p = new Point(0, -1); break;
                case Directions.Down: p = new Point(0, 1); break;
                case Directions.UpRight: p = new Point(1, -1); break;
                case Directions.DownRight: p = new Point(1, 1); break;
                case Directions.UpLeft: p = new Point(-1, -1); break;
                case Directions.DownLeft: p = new Point(-1, 1); break;
            }
            return p;
        }
        public static Point ReturnDirectionPoint(Directions dir)
        {
            var p = new Point(0);
            switch (dir)
            {
                case Directions.Right: p = new Point(1, 0); break;
                case Directions.Left: p = new Point(-1, 0); break;
                case Directions.Up: p = new Point(0, -1); break;
                case Directions.Down: p = new Point(0, 1); break;
                case Directions.UpRight: p = new Point(1, -1); break;
                case Directions.DownRight: p = new Point(1, 1); break;
                case Directions.UpLeft: p = new Point(-1, -1); break;
                case Directions.DownLeft: p = new Point(-1, 1); break;
            }
            return p;
        }
    }
}