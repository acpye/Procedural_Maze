using MGGameLibrary.Collision;
using MGGameLibrary.Physics;
using MGGameLibrary.Shapes;
using MGGameLibrary.Targetable;
using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;
using Microsoft.Xna.Framework.Input;
using System;
using System.Collections.Generic;

namespace ProceduralMaze
{
    public class Player : PhysicsObject, ICollidable, ITargetable
    {
        private Texture2D _texture;
        private readonly float _radius;
        private bool _isColliding;
        private bool _isGrounded;
        private int _wallDirection; // -1 left / 1 right / 0 none
        private int _jumpCount;
        private const int MAXJUMPS = 2;
        private KeyboardState _previousKeyboardState;

        private bool _wantsJump;
        private Vector2 _pendingJumpDelta;

        public Vector2 TargetPosition => Position;

        public Player(Vector2 startPosition, float radius) : base(startPosition, 1.0f)
        {
            UseGravity = true;
            AirResistance = 0.98f;
            _radius = radius;
        }

        public Shape Shape => BoundingCircle();

        public bool CollidesWith(ICollidable other)
        {
            return Shape.Intersects(other.Shape);
        }

        public bool CollidesWith(ICollidable other, ref Vector2 collisionNormal)
        {
            return Shape.Intersects(other.Shape, ref collisionNormal);
        }

        public Vector2 GetPosition()
        {
            return Position;
        }

        public Circle BoundingCircle()
        {
            return new Circle(Position, _radius);
        }

        public void LoadContent(GraphicsDevice graphicsDevice)
        {
            _texture = new Texture2D(graphicsDevice, (int)(_radius * 2), (int)(_radius * 2));
            Color[] data = new Color[(int)(_radius * 2) * (int)(_radius * 2)];
            for (int i = 0; i < data.Length; ++i)
            {
                int x = i % (int)(_radius * 2);
                int y = i / (int)(_radius * 2);
                Vector2 position = new Vector2(x - _radius, y - _radius);
                if (position.Length() <= _radius)
                {
                    data[i] = Color.White;
                }
            }
            _texture.SetData(data);
        }

        public void HandleInput()
        {
            KeyboardState keyboardState = Keyboard.GetState();
            Vector2 force = Vector2.Zero;
            const float moveForce = 500f; // Speed

            bool isJumpKeyPressed = (keyboardState.IsKeyDown(Keys.Space) && !_previousKeyboardState.IsKeyDown(Keys.Space)) ||
                                    (keyboardState.IsKeyDown(Keys.W) && !_previousKeyboardState.IsKeyDown(Keys.W)) ||
                                    (keyboardState.IsKeyDown(Keys.Up) && !_previousKeyboardState.IsKeyDown(Keys.Up));

            if (isJumpKeyPressed)
            {
                // Wall jump
                if (!_isGrounded && _wallDirection != 0)
                {
                    Vector2 targetVel = new Vector2(_wallDirection * -1 * moveForce * 0.6f, -250f); // Wall jump target velocity
                    _pendingJumpDelta = targetVel - Velocity;
                    _wantsJump = true;
                    _jumpCount = 1;
                    _isGrounded = false;
                }
                // Normal Jump
                else if (_jumpCount < MAXJUMPS)
                {
                    Vector2 targetVel = new Vector2(Velocity.X, -250f); // Jump height target velocity
                    _pendingJumpDelta = targetVel - Velocity;
                    _wantsJump = true;
                    _jumpCount++;
                    _isGrounded = false;
                }
            }

            if (keyboardState.IsKeyDown(Keys.S) || keyboardState.IsKeyDown(Keys.Down)) force.Y += moveForce;
            if (keyboardState.IsKeyDown(Keys.A) || keyboardState.IsKeyDown(Keys.Left)) force.X -= moveForce;
            if (keyboardState.IsKeyDown(Keys.D) || keyboardState.IsKeyDown(Keys.Right)) force.X += moveForce;

            ApplyForce(force);
            _previousKeyboardState = keyboardState;
        }

        public void Update(GameTime gameTime)
        {
            float deltaTime = (float)gameTime.ElapsedGameTime.TotalSeconds;
            if (deltaTime <= 0f)
            {
                deltaTime = 1f / 60f;
            }
            if (_wantsJump)
            {
                ApplyImpulse(_pendingJumpDelta, deltaTime);
                _wantsJump = false;
            }

            Velocity = new Vector2(Velocity.X * 0.95f, Velocity.Y);
            base.Update(deltaTime);
        }

        public void CheckCollisions(IEnumerable<ICollidable> collidables)
        {
            _isColliding = false;
            _isGrounded = false;
            _wallDirection = 0;

            for (int i = 0; i < 4; i++)
            {
                bool collisionDetected = false;

                foreach (ICollidable collidable in collidables)
                {
                    if (collidable.Shape is RectangleShape rectangle)
                    {
                        if (ResolveCircleRectangleCollision(rectangle))
                        {
                            _isColliding = true;
                            collisionDetected = true;
                        }
                    }
                    else if (collidable.Shape is Circle otherCircle)
                    {
                        Vector2 difference = Position - otherCircle.Position;
                        float distance = difference.Length();
                        float totalRadius = _radius + otherCircle.Radius;

                        if (distance < totalRadius)
                        {
                            _isColliding = true;
                            collisionDetected = true;

                            float penetration = totalRadius - distance;
                            Vector2 normal = Vector2.Normalize(difference);
                            if (distance == 0)
                            {
                                normal = new Vector2(1, 0);
                            }

                            Position += normal * penetration * 0.5f;

                            if (normal.Y < -0.5f)
                            {
                                _isGrounded = true;
                                _jumpCount = 0;
                            }

                            if (Math.Abs(normal.X) > 0.5f)
                            {
                                _wallDirection = normal.X > 0 ? -1 : 1;
                            }

                            float velocityDot = Vector2.Dot(Velocity, normal);
                            if (velocityDot < 0)
                            {
                                Velocity -= 2 * normal * velocityDot;
                            }
                        }
                    }
                }
                if (!collisionDetected)
                {
                    break;
                }
            }
        }

        private bool ResolveCircleRectangleCollision(RectangleShape rectangle)
        {
            Vector2 circleCenter = Position;
            float radius = _radius;

            Vector2 rectMinimum = rectangle.Position;
            Vector2 rectMaximum = rectangle.Position + new Vector2(rectangle.Width, rectangle.Height);

            float closestX = MathHelper.Clamp(circleCenter.X, rectMinimum.X, rectMaximum.X);
            float closestY = MathHelper.Clamp(circleCenter.Y, rectMinimum.Y, rectMaximum.Y);

            Vector2 closestPoint = new Vector2(closestX, closestY);
            Vector2 difference = circleCenter - closestPoint;
            float distanceSquared = difference.LengthSquared();

            if (distanceSquared >= radius * radius)
            {
                return false;
            }

            float distance = MathF.Sqrt(distanceSquared);
            Vector2 normal;
            float penetration;

            if (distance > 0)
            {
                normal = difference / distance;
                penetration = radius - distance;
            }
            else
            {
                float distanceLeft = circleCenter.X - rectMinimum.X;
                float distanceRight = rectMaximum.X - circleCenter.X;
                float distanceTop = circleCenter.Y - rectMinimum.Y;
                float distanceBottom = rectMaximum.Y - circleCenter.Y;

                float minimum = Math.Min(Math.Min(distanceLeft, distanceRight), Math.Min(distanceTop, distanceBottom));

                if (minimum == distanceLeft)
                {
                    normal = new Vector2(-1, 0);
                }
                else if (minimum == distanceRight)
                {
                    normal = new Vector2(1, 0);
                }
                else if (minimum == distanceTop)
                {
                    normal = new Vector2(0, -1);
                }
                else
                {
                    normal = new Vector2(0, 1);
                }
                penetration = minimum + radius;
            }

            if (normal.Y < -0.5f)
            {
                _isGrounded = true;
                _jumpCount = 0;
            }

            if (Math.Abs(normal.X) > 0.5f)
            {
                _wallDirection = normal.X > 0 ? -1 : 1;
            }

            Position += normal * penetration;

            float velocityDot = Vector2.Dot(Velocity, normal);
            if (velocityDot < 0)
            {
                Velocity -= normal * velocityDot;
            }

            return true;
        }

        public void Draw(SpriteBatch spriteBatch)
        {
            Color tint = Color.Blue;
            spriteBatch.Draw(_texture, Position - new Vector2(_radius), tint);
        }
    }
}