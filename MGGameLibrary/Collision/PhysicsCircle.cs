using MGGameLibrary.Physics;
using MGGameLibrary.Shapes;
using Microsoft.Xna.Framework;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace MGGameLibrary.Collision
{
    public class PhysicsCircle : PhysicsObject, ICollidable
    {
        private readonly Circle _circle;

        public PhysicsCircle(Vector2 position, float radius, float mass) : base(position, mass)
        {
            _circle = new Circle(position, radius);
        }

        public Shape Shape => _circle;

        public bool CollidesWith(ICollidable other)
        {
            return Shape.Intersects(other.Shape);
        }

        public bool CollidesWith(ICollidable other, ref Vector2 collisionNormal)
        {
            return Shape.Intersects(other.Shape, ref collisionNormal);
        }

        public override void Update(float deltaTime)
        {
            base.Update(deltaTime);
            _circle.Position = Position;
        }
    }
}
