using MGGameLibrary.Behaviours;
using MGGameLibrary.Physics;
using Microsoft.Xna.Framework;
using System;

namespace MGGameLibrary
{
    public class Agent : GameComponent
    {
        private readonly PhysicsObject _physicsObject;
        public SteeringBehaviour Behaviour { get; set; }
        public float Heading { get; set; }
        public float MaxSpeed { get; set; } = 100f;
        public Vector2 Position { get => _physicsObject.Position; set => _physicsObject.Position = value; }
        public Vector2 Velocity { get => _physicsObject.Velocity; set => _physicsObject.Velocity = value; }
        public Vector2 PreviousPosition => _physicsObject.PreviousPosition;

        public Agent(Vector2 position, float heading, SteeringBehaviour behaviour, Game game) : base(game)
        {
            _physicsObject = new PhysicsObject(position, 1.0f);
            Heading = heading;
            Behaviour = behaviour;
        }

        public void RevertToPreviousPosition()
        {
            _physicsObject.RevertToPreviousPosition();
        }

        public void AlignHeadingToVelocity()
        {
            Vector2 velocity = _physicsObject.Velocity;
            if (velocity.LengthSquared() > 0)
            {
                velocity = Vector2.Normalize(velocity);
                Heading = MathF.Atan2(velocity.Y, velocity.X) + MathF.PI / 2;
            }
        }

        public override void Update(GameTime gameTime)
        {
            if (Behaviour != null)
            {
                Vector2 steeringForce = Behaviour.CalculateSteeringForce(this);
                _physicsObject.ApplyForce(steeringForce);
            }
            _physicsObject.Update((float)gameTime.ElapsedGameTime.TotalSeconds);

            AlignHeadingToVelocity();

            base.Update(gameTime);
        }
    }
}