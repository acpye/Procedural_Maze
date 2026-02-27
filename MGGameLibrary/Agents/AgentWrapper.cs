using Microsoft.Xna.Framework;

namespace MGGameLibrary.Targetable
{
    public class AgentWrapper : ITargetable
    {
        private readonly Agent _agent;

        public AgentWrapper(Agent agent)
        {
            _agent = agent;
        }

        public Vector2 TargetPosition => _agent.Position;
    }
}