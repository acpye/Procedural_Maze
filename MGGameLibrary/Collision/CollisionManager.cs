using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Microsoft.Xna.Framework;
using MGGameLibrary.Physics;

namespace MGGameLibrary.Collision
{
    public static class CollisionManager
    {
        public static void HandleDiscObstacleCollisions(List<PhysicsCircle> circles, List<Obstacle> obstacles, float deltaTime)
        {
            Vector2 collisionNormal = Vector2.Zero;
            foreach (PhysicsCircle circle in circles)
            {
                foreach (Obstacle obstacle in obstacles)
                {
                    if (obstacle.CollidesWith(circle, ref collisionNormal))
                    {
                        circle.RevertToPreviousPosition();
                        Vector2 desiredVelocity = Vector2.Reflect(circle.Velocity, collisionNormal);
                        circle.ApplyImpulse(desiredVelocity - circle.Velocity, deltaTime);
                    }
                }
            }
        }

        public static void HandleDiscDiscCollisions(List<PhysicsCircle> circles)
        {
            Vector2 collisionNormal = Vector2.Zero;
            for (int i = 0; i < circles.Count; i++)
            {
                for (int j = i + 1; j < circles.Count; j++)
                {
                    PhysicsCircle circle1 = circles[i];
                    PhysicsCircle circle2 = circles[j];

                    if (circle1.CollidesWith(circle2, ref collisionNormal))
                    {
                        circle1.RevertToPreviousPosition();
                        circle2.RevertToPreviousPosition();

                        // Ensure normal is valid and normalized
                        if (collisionNormal == Vector2.Zero)
                        {
                            // Fallback to vector between centers
                            Vector2 diff = circle1.Position - circle2.Position;
                            if (diff == Vector2.Zero) collisionNormal = new Vector2(1, 0);
                            else collisionNormal = Vector2.Normalize(diff);
                        }
                        else
                        {
                            collisionNormal.Normalize();
                        }

                        // Project current velocities onto normal (scalars)
                        float v1n = Vector2.Dot(circle1.Velocity, collisionNormal);
                        float v2n = Vector2.Dot(circle2.Velocity, collisionNormal);

                        // Perpendicular components remain unchanged
                        Vector2 perp1 = circle1.Velocity - collisionNormal * v1n;
                        Vector2 perp2 = circle2.Velocity - collisionNormal * v2n;

                        // Use 1D elastic collision formula along the normal, preserving momentum & energy.
                        // For equal masses this reduces to swapping the normal components.
                        float m1 = Math.Max(circle1.Mass, 0.0001f);
                        float m2 = Math.Max(circle2.Mass, 0.0001f);

                        float v1nAfter = (v1n * (m1 - m2) + 2f * m2 * v2n) / (m1 + m2);
                        float v2nAfter = (v2n * (m2 - m1) + 2f * m1 * v1n) / (m1 + m2);

                        Vector2 parallel1After = collisionNormal * v1nAfter;
                        Vector2 parallel2After = collisionNormal * v2nAfter;

                        circle1.Velocity = perp1 + parallel1After;
                        circle2.Velocity = perp2 + parallel2After;
                    }
                }
            }
        }
    }
}