using Microsoft.Xna.Framework;
using System;

namespace MGGameLibrary.Shapes
{
    public class RectangleShape : Shape
    {
        public float Width { get; }
        public float Height { get; }

        public RectangleShape(Vector2 position, float width, float height) : base(position)
        {
            Width = width;
            Height = height;
        }

        public override bool IsInside(Point point)
        {
            return point.X >= Position.X && point.X <= Position.X + Width &&
                   point.Y >= Position.Y && point.Y <= Position.Y + Height;
        }

        public override bool Intersects(Shape other)
        {
            return other.Intersects(this);
        }

        public override bool Intersects(Shape other, ref Vector2 collisionNormal)
        {
            return other.Intersects(this, ref collisionNormal);
        }

        public override bool IntersectsCircle(Circle circle)
        {
            return circle.Intersects(this);
        }

        public override bool IntersectsCircle(Circle circle, ref Vector2 collisionNormal)
        {
            return circle.Intersects(this, ref collisionNormal);
        }

        public override bool IntersectsSquare(Square square)
        {
            bool noOverlap = Position.X + Width <= square.Position.X ||
                             square.Position.X + square.Size <= Position.X ||
                             Position.Y + Height <= square.Position.Y ||
                             square.Position.Y + square.Size <= Position.Y;

            return !noOverlap;
        }

        public override bool IntersectsSquare(Square square, ref Vector2 collisionNormal)
        {
            if (!IntersectsSquare(square))
            {
                return false;
            }

            Vector2 aCenter = Position + new Vector2(Width * 0.5f, Height * 0.5f);
            Vector2 bCenter = square.Position + new Vector2(square.Size * 0.5f);
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

        public override bool Intersects(RectangleShape rect)
        {
            // AABB intersection
            bool noOverlap = Position.X + Width <= rect.Position.X ||
                             rect.Position.X + rect.Width <= Position.X ||
                             Position.Y + Height <= rect.Position.Y ||
                             rect.Position.Y + rect.Height <= Position.Y;

            return !noOverlap;
        }

        public override bool Intersects(RectangleShape rect, ref Vector2 collisionNormal)
        {
            if (!Intersects(rect))
            {
                return false;
            }

            Vector2 aCenter = Position + new Vector2(Width * 0.5f, Height * 0.5f);
            Vector2 bCenter = rect.Position + new Vector2(rect.Width * 0.5f, rect.Height * 0.5f);
            Vector2 diff = aCenter - bCenter;

            float overlapX = (Width * 0.5f + rect.Width * 0.5f) - Math.Abs(diff.X);
            float overlapY = (Height * 0.5f + rect.Height * 0.5f) - Math.Abs(diff.Y);

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
    }
}