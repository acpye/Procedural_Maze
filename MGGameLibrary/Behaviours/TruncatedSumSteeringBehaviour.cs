using Microsoft.Xna.Framework;
using System.Collections.Generic;

namespace MGGameLibrary.Behaviours
{
    public class TruncatedSumSteeringBehaviour : SteeringBehaviour
    {
        private readonly List<SteeringBehaviour> _behaviours;
        private readonly float _maxForce;

        public TruncatedSumSteeringBehaviour(List<SteeringBehaviour> behaviours, float maxForce)
        {
            _behaviours = behaviours;
            _maxForce = maxForce;
        }

        public override Vector2 CalculateSteeringForce(Agent agent)
        {
            Vector2 totalForce = Vector2.Zero;
            foreach (SteeringBehaviour behaviour in _behaviours)
            {
                Vector2 force = behaviour.CalculateSteeringForce(agent);
                if (totalForce.Length() + force.Length() > _maxForce)
                {
                    float remainingForce = _maxForce - totalForce.Length();
                    force.Normalize();
                    totalForce += force * remainingForce;
                    break;
                }
                totalForce += force;
            }
            return totalForce;
        }
    }
}