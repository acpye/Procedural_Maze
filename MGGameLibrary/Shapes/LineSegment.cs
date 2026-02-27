using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Microsoft.Xna.Framework;

namespace MGGameLibrary.Shapes
{
    public class LineSegment : Shape
    {
        public Vector2 Start { get { return Position; } set { Position = value; } }
        public Vector2 End { get; set; }

        public LineSegment(Vector2 start, Vector2 end) : base(start)
        {
            End = end;
        }

        public static Vector2? Intersection(LineSegment line1, LineSegment line2)
        {
            float x1 = line1.Start.X, y1 = line1.Start.Y;
            float x2 = line1.End.X, y2 = line1.End.Y;
            float x3 = line2.Start.X, y3 = line2.Start.Y;
            float x4 = line2.End.X, y4 = line2.End.Y;

            float den = (x1 - x2) * (y3 - y4) - (y1 - y2) * (x3 - x4);
            if (den == 0) return null;

            float t = ((x1 - x3) * (y3 - y4) - (y1 - y3) * (x3 - x4)) / den;
            float u = -((x1 - x2) * (y1 - y3) - (y1 - y2) * (x1 - x3)) / den;

            if (t >= 0 && t <= 1 && u >= 0 && u <= 1)
            {
                return new Vector2(x1 + t * (x2 - x1), y1 + t * (y2 - y1));
            }

            return null;
        }

        public override bool Intersects(Shape other)
        {
            if (other is LineSegment otherLine)
            {
                return Intersection(this, otherLine).HasValue;
            }
            return false;
        }

        public override bool Intersects(Shape other, ref Vector2 collisionNormal)
        {
            return false;
        }

        public override bool IntersectsCircle(Circle circle)
        {
            Vector2 collisionNormal = Vector2.Zero;
            return IntersectsCircle(circle, ref collisionNormal);
        }

        public override bool IntersectsCircle(Circle circle, ref Vector2 collisionNormal)
        {
            Vector2 circleCenter = circle.Position;
            Vector2 closestPoint = ClosestPointOnLine(Start, End, circleCenter);
            float distanceSquared = Vector2.DistanceSquared(circleCenter, closestPoint);

            bool intersects = distanceSquared <= circle.Radius * circle.Radius;
            if (intersects)
            {
                if (distanceSquared > 0)
                {
                    collisionNormal = Vector2.Normalize(closestPoint - circleCenter);
                }
                else
                {
                    Vector2 lineDir = End - Start;
                    collisionNormal = new Vector2(-lineDir.Y, lineDir.X);
                    collisionNormal.Normalize();
                }
            }
            return intersects;
        }

        public override bool IntersectsSquare(Square square)
        {
            Vector2 collisionNormal = Vector2.Zero;
            return IntersectsSquare(square, ref collisionNormal);
        }

        public override bool IntersectsSquare(Square square, ref Vector2 collisionNormal)
        {
            Vector2 topLeft = square.Position;
            Vector2 topRight = new Vector2(square.Position.X + square.Size, square.Position.Y);
            Vector2 bottomLeft = new Vector2(square.Position.X, square.Position.Y + square.Size);
            Vector2 bottomRight = new Vector2(square.Position.X + square.Size, square.Position.Y + square.Size);

            if (IntersectsLine(Start, End, topLeft, topRight, ref collisionNormal) ||
                IntersectsLine(Start, End, topRight, bottomRight, ref collisionNormal) ||
                IntersectsLine(Start, End, bottomRight, bottomLeft, ref collisionNormal) ||
                IntersectsLine(Start, End, bottomLeft, topLeft, ref collisionNormal))
            {
                return true;
            }

            if (square.IsInside(new Point((int)Start.X, (int)Start.Y)) || square.IsInside(new Point((int)End.X, (int)End.Y)))
            {
                collisionNormal = Vector2.UnitY;
                return true;
            }

            return false;
        }

        public override bool Intersects(RectangleShape rect)
        {
            Vector2 collisionNormal = Vector2.Zero;
            return Intersects(rect, ref collisionNormal);
        }

        public override bool Intersects(RectangleShape rect, ref Vector2 collisionNormal)
        {
            Vector2 topLeft = rect.Position;
            Vector2 topRight = new Vector2(rect.Position.X + rect.Width, rect.Position.Y);
            Vector2 bottomLeft = new Vector2(rect.Position.X, rect.Position.Y + rect.Height);
            Vector2 bottomRight = new Vector2(rect.Position.X + rect.Width, rect.Position.Y + rect.Height);

            if (IntersectsLine(Start, End, topLeft, topRight, ref collisionNormal) ||
                IntersectsLine(Start, End, topRight, bottomRight, ref collisionNormal) ||
                IntersectsLine(Start, End, bottomRight, bottomLeft, ref collisionNormal) ||
                IntersectsLine(Start, End, bottomLeft, topLeft, ref collisionNormal))
            {
                return true;
            }

            if (rect.IsInside(new Point((int)Start.X, (int)Start.Y)) || rect.IsInside(new Point((int)End.X, (int)End.Y)))
            {
                collisionNormal = Vector2.UnitY;
                return true;
            }

            return false;
        }

        public override bool IsInside(Point point)
        {
            float distanceToStart = Vector2.Distance(point.ToVector2(), Start);
            float distanceToEnd = Vector2.Distance(point.ToVector2(), End);
            float lineLength = Vector2.Distance(Start, End);

            return Math.Abs(distanceToStart + distanceToEnd - lineLength) < 0.001f;
        }

        private static Vector2 ClosestPointOnLine(Vector2 lineStart, Vector2 lineEnd, Vector2 point)
        {
            Vector2 lineDir = lineEnd - lineStart;
            float lineLengthSquared = lineDir.LengthSquared();
            if (lineLengthSquared == 0.0f)
            {
                return lineStart;
            }

            float t = MathHelper.Clamp(Vector2.Dot(point - lineStart, lineDir) / lineLengthSquared, 0, 1);
            return lineStart + t * lineDir;
        }

        private static bool IntersectsLine(Vector2 line1point1, Vector2 line1point2, Vector2 line2point1, Vector2 line2point2, ref Vector2 normal)
        {
            Vector2? intersectionPoint = Intersection(new LineSegment(line1point1, line1point2), new LineSegment(line2point1, line2point2));
            if (intersectionPoint.HasValue)
            {
                Vector2 lineDir = line2point2 - line2point1;
                normal = new Vector2(-lineDir.Y, lineDir.X);
                normal.Normalize();
                return true;
            }
            return false;
        }
    }
}