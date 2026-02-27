using MGGameLibrary.Collision;
using Microsoft.Xna.Framework;
using System;

namespace MGGameLibrary.Shapes
{
    public class Square : Shape
    {
        public float Size { get; }

        public Square(Vector2 position, float size) : base(position)
        {
            Size = size;
        }

        public override bool IsInside(Point point)
        {
            return point.X >= Position.X && point.X <= Position.X + Size &&
                   point.Y >= Position.Y && point.Y <= Position.Y + Size;
        }

        public override bool Intersects(Shape other)
        {
            return other.IntersectsSquare(this);
        }

        public override bool IntersectsCircle(Circle circle)
        {
            return circle.IntersectsSquare(this);
        }

        public override bool IntersectsSquare(Square square)
        {
            bool noOverlap = Position.X + Size <= square.Position.X ||
                            square.Position.X + square.Size <= Position.X ||
                            Position.Y + Size <= square.Position.Y ||
                            square.Position.Y + square.Size <= Position.Y;

            return !noOverlap;
        }


        public override bool Intersects(Shape other, ref Vector2 collisionNormal)
        {
            return other.IntersectsSquare(this, ref collisionNormal);
        }

        public override bool Intersects(RectangleShape rect)
        {
            bool noOverlap = Position.X + Size <= rect.Position.X ||
                             rect.Position.X + rect.Width <= Position.X ||
                             Position.Y + Size <= rect.Position.Y ||
                             rect.Position.Y + rect.Height <= Position.Y;

            return !noOverlap;
        }

        public override bool Intersects(RectangleShape rect, ref Vector2 collisionNormal)
        {
            if (!Intersects(rect))
            {
                return false;
            }

            Vector2 aCenter = Position + new Vector2(Size * 0.5f);
            Vector2 bCenter = rect.Position + new Vector2(rect.Width * 0.5f, rect.Height * 0.5f);
            Vector2 diff = aCenter - bCenter;

            float overlapX = (Size * 0.5f + rect.Width * 0.5f) - Math.Abs(diff.X);
            float overlapY = (Size * 0.5f + rect.Height * 0.5f) - Math.Abs(diff.Y);

            if (overlapX < overlapY)
            {
                collisionNormal = new Vector2(diff.X > 0 ? 1 : -1, 0);
            }
            else
            {
                collisionNormal = new Vector2(0, diff.Y > 0 ? 1 : -1);
            }

            return true;
        }

        public override bool IntersectsCircle(Circle circle, ref Vector2 collisionNormal)
        {
            return circle.IntersectsSquare(this, ref collisionNormal);
        }

        public override bool IntersectsSquare(Square square, ref Vector2 collisionNormal)
        {
            Vector2 aMin = Position;
            Vector2 aMax = Position + new Vector2(Size);
            Vector2 bMin = square.Position;
            Vector2 bMax = square.Position + new Vector2(square.Size);

            if (aMax.X <= bMin.X || bMax.X <= aMin.X || aMax.Y <= bMin.Y || bMax.Y <= aMin.Y)
            {
                return false;
            }

            Vector2 aCenter = aMin + new Vector2(Size * 0.5f);
            Vector2 bCenter = bMin + new Vector2(square.Size * 0.5f);
            Vector2 diff = aCenter - bCenter;

            if (diff.LengthSquared() > 0f)
            {
                if (Math.Abs(diff.X) > Math.Abs(diff.Y))
                {
                    collisionNormal = new Vector2(diff.X >= 0f ? 1f : -1f, 0f);
                }
                else
                {
                    collisionNormal = new Vector2(0f, diff.Y >= 0f ? 1f : -1f);
                }
            }
            else
            {
                collisionNormal = Vector2.UnitY;
            }

            return true;
        }
    }
}