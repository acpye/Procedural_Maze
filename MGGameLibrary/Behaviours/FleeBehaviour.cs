using MGGameLibrary.Targetable;
using Microsoft.Xna.Framework;

namespace MGGameLibrary.Behaviours
{
    public class FleeBehaviour : SteeringBehaviour
    {
        public ITargetable Target { get; set; }

        public FleeBehaviour(ITargetable target)
        {
            Target = target;
        }

        public override Vector2 CalculateSteeringForce(Agent agent)
        {
            Vector2 desiredVelocity = agent.Position - Target.TargetPosition;
            if (desiredVelocity.LengthSquared() > 0)
            {
                desiredVelocity.Normalize();
                desiredVelocity *= agent.MaxSpeed;
                Vector2 steering = desiredVelocity - agent.Velocity;
                return steering;
            }
            return Vector2.Zero;
        }
    }
}