using Microsoft.Xna.Framework;

namespace MGGameLibrary.Shapes
{
    public abstract class Shape
    {
        public Vector2 Position { get; set; }

        public Shape(Vector2 position)
        {
            Position = position;
        }

        public abstract bool IsInside(Point point);
        public abstract bool Intersects(Shape other);
        public abstract bool IntersectsCircle(Circle circle);
        public abstract bool IntersectsSquare(Square square);

        public abstract bool Intersects(Shape other, ref Vector2 collisionNormal);
        public abstract bool Intersects(RectangleShape rect);
        public abstract bool Intersects(RectangleShape rect, ref Vector2 collisionNormal);
        public abstract bool IntersectsCircle(Circle circle, ref Vector2 collisionNormal);
        public abstract bool IntersectsSquare(Square square, ref Vector2 collisionNormal);

        public static bool Intersects(Circle c1, Circle c2)
        {
            Vector2 center1 = c1.Position + new Vector2(c1.Radius);
            Vector2 center2 = c2.Position + new Vector2(c2.Radius);
            float distanceSquared = Vector2.DistanceSquared(center1, center2);
            float radiusSum = c1.Radius + c2.Radius;
            return distanceSquared <= radiusSum * radiusSum;
        }

        public static bool Intersects(Circle c1, Circle c2, ref Vector2 collisionNormal)
        {
            Vector2 center1 = c1.Position + new Vector2(c1.Radius);
            Vector2 center2 = c2.Position + new Vector2(c2.Radius);
            float distanceSquared = Vector2.DistanceSquared(center1, center2);
            float radiusSum = c1.Radius + c2.Radius;
            bool collision = distanceSquared <= radiusSum * radiusSum;

            if (collision)
            {
                collisionNormal = Vector2.Normalize(center1 - center2);
            }

            return collision;
        }
    }
}