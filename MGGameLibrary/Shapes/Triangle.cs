using MGGameLibrary.Collision;
using Microsoft.Xna.Framework;
using System;

namespace MGGameLibrary.Shapes
{
    public class Triangle : Shape
    {
        public float Size { get; }

        public Triangle(Vector2 position, float size) : base(position)
        {
            Size = size;
        }

        public override bool IsInside(Point point)
        {
            float ax = Position.X + Size / 2f;
            float ay = Position.Y;
            float bx = Position.X;
            float by = Position.Y + Size;
            float cx = Position.X + Size;
            float cy = Position.Y + Size;

            float px = point.X;
            float py = point.Y;

            float v0x = cx - ax, v0y = cy - ay;
            float v1x = bx - ax, v1y = by - ay;
            float v2x = px - ax, v2y = py - ay;

            float dot00 = v0x * v0x + v0y * v0y;
            float dot01 = v0x * v1x + v0y * v1y;
            float dot02 = v0x * v2x + v0y * v2y;
            float dot11 = v1x * v1x + v1y * v1y;
            float dot12 = v1x * v2x + v1y * v2y;

            float denom = dot00 * dot11 - dot01 * dot01;
            if (denom == 0.0f)
                return false;

            float u = (dot11 * dot02 - dot01 * dot12) / denom;
            float v = (dot00 * dot12 - dot01 * dot02) / denom;

            return u >= 0.0f && v >= 0.0f && (u + v) <= 1.0f;
        }

        public override bool Intersects(Shape other)
        {
            return false;
        }

        public override bool IntersectsCircle(Circle circle)
        {
            return false;
        }

        public override bool IntersectsSquare(Square square)
        {
            return false;
        }

        public override bool Intersects(Shape other, ref Vector2 collisionNormal)
        {
            throw new NotImplementedException();
        }

        public override bool IntersectsCircle(Circle circle, ref Vector2 collisionNormal)
        {
            throw new NotImplementedException();
        }

        public override bool IntersectsSquare(Square square, ref Vector2 collisionNormal)
        {
            throw new NotImplementedException();
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
    }
}