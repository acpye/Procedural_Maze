using MGGameLibrary.Shapes;
using Microsoft.Xna.Framework;

namespace MGGameLibrary.Collision
{
    public interface ICollidable
    {
        Shape Shape { get; }
        bool CollidesWith(ICollidable other);
        bool CollidesWith(ICollidable other, ref Vector2 collisionNormal);
    }
}