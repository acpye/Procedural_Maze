namespace MGGameLibrary.Behaviours
{
    public abstract class SteeringBehaviour
    {
        public abstract Microsoft.Xna.Framework.Vector2 CalculateSteeringForce(Agent agent);
    }
}