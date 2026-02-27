using MGGameLibrary.Collision;
using Microsoft.Xna.Framework;
using System;
using System.Linq;

namespace MGGameLibrary.Shapes
{
    public class Circle : Shape
    {
        public float Radius { get; }

        public Circle(Vector2 position, float radius) : base(position)
        {
            Radius = radius;
        }

        public override bool IsInside(Point point)
        {
            float centerX = Position.X + Radius;
            float centerY = Position.Y + Radius;
            float dx = point.X - centerX;
            float dy = point.Y - centerY;
            return dx * dx + dy * dy <= Radius * Radius;
        }

        public override bool Intersects(Shape other)
        {
            return other.IntersectsCircle(this);
        }

        public override bool IntersectsCircle(Circle circle)
        {
            return Shape.Intersects(this, circle);
        }

        public override bool IntersectsSquare(Square square)
        {
            Vector2 circleCenter = Position + new Vector2(Radius);
            Vector2 rectMin = square.Position;
            Vector2 rectMax = square.Position + new Vector2(square.Size);
            float closestX = MathHelper.Clamp(circleCenter.X, rectMin.X, rectMax.X);
            float closestY = MathHelper.Clamp(circleCenter.Y, rectMin.Y, rectMax.Y);
            Vector2 closestPoint = new Vector2(closestX, closestY);
            float distSq = Vector2.DistanceSquared(circleCenter, closestPoint);
            return distSq <= Radius * Radius;
        }

        public override bool Intersects(Shape other, ref Vector2 collisionNormal)
        {
            return other.IntersectsCircle(this, ref collisionNormal);
        }

        public override bool Intersects(RectangleShape rect)
        {
            Vector2 circleCenter = Position + new Vector2(Radius);
            Vector2 rectMin = rect.Position;
            Vector2 rectMax = rect.Position + new Vector2(rect.Width, rect.Height);
            float closestX = MathHelper.Clamp(circleCenter.X, rectMin.X, rectMax.X);
            float closestY = MathHelper.Clamp(circleCenter.Y, rectMin.Y, rectMax.Y);
            Vector2 closestPoint = new Vector2(closestX, closestY);
            float distSq = Vector2.DistanceSquared(circleCenter, closestPoint);
            return distSq <= Radius * Radius;
        }

        public override bool Intersects(RectangleShape rect, ref Vector2 collisionNormal)
        {
            if (!Intersects(rect))
                return false;

            Vector2 circleCenter = Position + new Vector2(Radius);
            Vector2 rectMin = rect.Position;
            Vector2 rectMax = rect.Position + new Vector2(rect.Width, rect.Height);
            float closestX = MathHelper.Clamp(circleCenter.X, rectMin.X, rectMax.X);
            float closestY = MathHelper.Clamp(circleCenter.Y, rectMin.Y, rectMax.Y);
            Vector2 closestPoint = new Vector2(closestX, closestY);

            Vector2 diff = closestPoint - circleCenter;
            if (diff.LengthSquared() > 0)
            {
                collisionNormal = Vector2.Normalize(diff);
            }
            else
            {
                // Circle center is inside the rectangle, find the shallowest axis of penetration
                float[] distances = {
                    circleCenter.X - rectMin.X,
                    rectMax.X - circleCenter.X,
                    circleCenter.Y - rectMin.Y,
                    rectMax.Y - circleCenter.Y
                };
                float minDistance = distances.Min();
                if (minDistance == distances[0]) collisionNormal = new Vector2(-1, 0);
                else if (minDistance == distances[1]) collisionNormal = new Vector2(1, 0);
                else if (minDistance == distances[2]) collisionNormal = new Vector2(0, -1);
                else collisionNormal = new Vector2(0, 1);
            }
            return true;
        }

        public override bool IntersectsCircle(Circle other, ref Vector2 collisionNormal)
        {
            return Shape.Intersects(this, other, ref collisionNormal);
        }

        public override bool IntersectsSquare(Square square, ref Vector2 collisionNormal)
        {
            Vector2 circleCenter = Position + new Vector2(Radius);
            Vector2 rectMin = square.Position;
            Vector2 rectMax = square.Position + new Vector2(square.Size);
            float closestX = MathHelper.Clamp(circleCenter.X, rectMin.X, rectMax.X);
            float closestY = MathHelper.Clamp(circleCenter.Y, rectMin.Y, rectMax.Y);
            Vector2 closestPoint = new Vector2(closestX, closestY);
            Vector2 diff = closestPoint - circleCenter;
            float distSq = diff.LengthSquared();
            bool intersects = distSq <= Radius * Radius;

            if (intersects)
            {
                if (distSq > 0f)
                {
                    collisionNormal = Vector2.Normalize(diff);
                }
                else
                {
                    collisionNormal = Vector2.UnitY;
                }
            }
            return intersects;
        }
    }
}