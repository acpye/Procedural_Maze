using Microsoft.Xna.Framework;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace MGGameLibrary.Physics
{
    public class PhysicsObject
    {
        protected Vector2 _position;
        protected Vector2 _previousPosition;
        protected Vector2 _velocity;
        protected Vector2 _force;
        protected float _mass;

        public bool UseGravity { get; set; } = false;
        public float AirResistance { get; set; } = 1.0f;

        public Vector2 Position
        {
            get => _position;
            set => _position = value;
        }

        public Vector2 Velocity
        {
            get => _velocity;
            set => _velocity = value;
        }

        public float Mass
        {
            get => _mass;
            set => _mass = value;
        }

        public Vector2 PreviousPosition => _previousPosition;

        public PhysicsObject(Vector2 position, float mass)
        {
            _position = position;
            _previousPosition = position;
            _velocity = Vector2.Zero;
            _force = Vector2.Zero;
            _mass = mass;
        }

        public void ApplyForce(Vector2 force)
        {
            _force += force;
        }

        public void ApplyGravity()
        {
            const float GRAVITY = 250f; // 250
            _force += new Vector2(0, GRAVITY * _mass);
        }

        public void ApplyImpulse(Vector2 changeInVelocity, float deltaTime)
        {
            Vector2 force = _mass * changeInVelocity / deltaTime;
            ApplyForce(force);
        }

        public virtual void Update(float deltaTime)
        {
            if (UseGravity)
            {
                ApplyGravity();
            }

            _previousPosition = _position;
            Vector2 acceleration = _force / _mass;
            _velocity += acceleration * deltaTime;
            _position += _velocity * deltaTime;

            if (AirResistance < 1.0f)
            {
                _velocity *= MathF.Pow(AirResistance, deltaTime);
            }

            _force = Vector2.Zero;
        }

        public void RevertToPreviousPosition()
        {
            _position = _previousPosition;
        }
    }
}