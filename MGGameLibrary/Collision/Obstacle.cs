using MGGameLibrary.Shapes;
using Microsoft.Xna.Framework;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace MGGameLibrary.Collision
{
    public class Obstacle : ICollidable
    {
        private readonly Shape _shape;
        public Shape Shape => _shape;

        public Obstacle(Shape shape)
        {
            _shape = shape;
        }

        public bool CollidesWith(ICollidable other)
        {
            return _shape.Intersects(other.Shape);
        }

        public bool CollidesWith(ICollidable other, ref Vector2 collisionNormal)
        {
            return _shape.Intersects(other.Shape, ref collisionNormal);
        }
    }
}
