using MGGameLibrary.Collision;
using MGGameLibrary.Shapes;
using Microsoft.Xna.Framework;
using System.Collections.Generic;
using System.Linq;

namespace MGGameLibrary.Behaviours
{
    public class AvoidCollidableWithWhiskersBehaviour : SteeringBehaviour
    {
        private readonly List<ICollidable> _collidables;
        private readonly List<Vector2> _whiskers;

        public AvoidCollidableWithWhiskersBehaviour(List<ICollidable> collidables, List<Vector2> whiskers)
        {
            _collidables = collidables;
            _whiskers = whiskers;
        }

        public override Vector2 CalculateSteeringForce(Agent agent)
        {
            foreach (Vector2 whisker in _whiskers)
            {
                Vector2 rotatedWhisker = Vector2.Transform(whisker, Matrix.CreateRotationZ(agent.Heading));
                Vector2 whiskerEnd = agent.Position + rotatedWhisker;
                LineSegment whiskerSegment = new Shapes.LineSegment(agent.Position, whiskerEnd);

                bool intersection = _collidables.Any(c => whiskerSegment.Intersects(c.Shape));

                if (intersection)
                {
                    Vector2 desiredVelocity = -rotatedWhisker;
                    desiredVelocity.Normalize();
                    desiredVelocity *= agent.MaxSpeed;
                    return desiredVelocity - agent.Velocity;
                }
            }

            return Vector2.Zero;
        }
    }
}