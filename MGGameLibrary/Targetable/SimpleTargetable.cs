using Microsoft.Xna.Framework;

namespace MGGameLibrary.Targetable
{
    public class SimpleTargetable : ITargetable
    {
        public Vector2 TargetPosition { get; }

        public SimpleTargetable(Vector2 position)
        {
            TargetPosition = position;
        }
    }
}