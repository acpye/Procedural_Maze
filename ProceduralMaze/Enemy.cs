using MGGameLibrary.BehaviourTrees;
using MGGameLibrary.Collision;
using MGGameLibrary.Physics;
using MGGameLibrary.Shapes;
using MGGameLibrary.Targetable;
using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;
using System;
using System.Collections.Generic;
using System.Linq;

namespace ProceduralMaze
{
    public class Enemy : PhysicsObject, ICollidable, ITargetable
    {
        public enum BehaviourMode
        {
            Default = 1,
            Chase = 2,
            Investigate = 3,
            Explore = 4
        }

        private Texture2D _texture;
        private Texture2D _lineTexture;
        private readonly float _radius;
        private bool _isColliding;
        private bool _isGrounded;
        private int _wallDirection; // -1 left / 1 right / 0 none
        private int _jumpCount;
        private const int MAXJUMPS = 10;

        private BehaviourTree _behaviourTree;
        private ITargetable _target;
        private string _currentState;
        private Maze _maze;

        private Vector2? _lastKnownPlayerPosition;
        private bool _hasLineOfSight;
        private List<Vector2> _currentPath;
        private int _currentPathIndex;
        private Vector2? _lastPathTargetPosition;

        private const float MoveForce = 300f;
        private const float JumpVelocity = -250f;
        private const float SightRange = 300f;
        private const float MaxChaseDistance = 100f;

        private bool _needsWallJump;
        private Vector2? _wallJumpTarget;
        private float _jumpCooldown;
        private const float JumpCooldownDuration = 0.3f;

        public Vector2 TargetPosition => Position;

        public string CurrentState => _currentState;
        public bool HasLineOfSight => _hasLineOfSight;

        private BehaviourMode _mode = BehaviourMode.Default;
        public BehaviourMode ActiveBehaviourMode => _mode;

        private Random _random = new Random();

        public bool IsStationary => _mode == BehaviourMode.Default;

        public Enemy(Vector2 startPosition, float radius, ITargetable target, Maze maze) : base(startPosition, 1.0f)
        {
            UseGravity = true;
            AirResistance = 0.98f;
            _radius = radius;
            _target = target;
            _maze = maze;
            _currentState = "Idle";
            _currentPath = new List<Vector2>();
            _currentPathIndex = 0;
            _lastPathTargetPosition = null;

            InitializeBehaviourTree();
        }

        public void SetBehaviourMode(int modeIndex)
        {
            if (Enum.IsDefined(typeof(BehaviourMode), modeIndex))
            {
                _mode = (BehaviourMode)modeIndex;
            }
            else
            {
                _mode = BehaviourMode.Default;
            }
            InitializeBehaviourTree();
        }

        private void InitializeBehaviourTree()
        {
            switch (_mode)
            {
                case BehaviourMode.Default:
                    _behaviourTree = new BehaviourTreeBuilder()
                        .Selector()
                        .End()
                        .Build();
                    break;
                case BehaviourMode.Chase:
                    _behaviourTree = new BehaviourTreeBuilder()
                    .Selector()
                        // Chase
                        .Sequence()
                            .Action(CanSeeTarget)
                            .Selector()
                                .Sequence()
                                    .Action(ShouldJumpToReachTarget)
                                    .Action(PerformJump)
                                .End()
                                .Sequence()
                                    .Action(ShouldWallJump)
                                    .Action(PerformWallJump)
                                .End()
                                .Action(ChaseTarget)
                            .End()
                        .End()
                    .End()
                    .Build();
                    break;
                case BehaviourMode.Investigate:
                    _behaviourTree = new BehaviourTreeBuilder()
                    .Selector()
                        // Investigate
                        .Sequence()
                            .Action(HasLastKnownPosition)
                            .Selector()
                                .Sequence()
                                    .Action(ShouldWallJumpForWaypoint)
                                    .Action(PerformWallJump)
                                .End()
                                .Sequence()
                                    .Action(NeedsToReachWall)
                                    .Action(MoveToWall)
                                .End()
                                .Sequence()
                                    .Action(ShouldJumpToReachWaypoint)
                                    .Action(PerformJump)
                                .End()
                                .Action(ChaseLastKnownPosition)
                            .End()
                        .End()
                    .End()
                    .Build();
                    break;
                case BehaviourMode.Explore:
                    _behaviourTree = new BehaviourTreeBuilder()
                    .Selector()
                        // Explore
                        .Selector()
                            .Sequence()
                                .Action(ShouldWallJumpForWaypoint)
                                .Action(PerformWallJump)
                            .End()
                            .Sequence()
                                .Action(NeedsToReachWall)
                                .Action(MoveToWall)
                            .End()
                            .Sequence()
                                .Action(ShouldJumpToReachWaypoint)
                               .Action(PerformJump)
                            .End()
                            .Action(ExploreMap)
                        .End()
                    .End()
                    .Build();
                    break;
            }
        }

        private bool CheckLineOfSight(Vector2 from, Vector2 to)
        {
            if (_maze == null)
            {
                return true;
            }

            LineSegment sightLine = new LineSegment(from, to);

            foreach (ICollidable wall in _maze.WallCollidables)
            {
                if (wall.Shape is RectangleShape rect)
                {
                    if (IsPointInsideRect(from, rect))
                    {
                        continue;
                    }

                    if (sightLine.Intersects(rect))
                    {
                        return false;
                    }

                    int samples = 5;
                    for (int i = 1; i < samples; i++)
                    {
                        float t = i / (float)samples;
                        Vector2 samplePoint = Vector2.Lerp(from, to, t);
                        if (IsPointInsideRect(samplePoint, rect))
                        {
                            return false;
                        }
                    }
                }
                else if (wall.Shape is Square square)
                {
                    if (square.IsInside(new Point((int)from.X, (int)from.Y)))
                    {
                        continue;
                    }

                    if (sightLine.IntersectsSquare(square))
                    {
                        return false;
                    }
                }
                else if (wall.Shape is Circle circle)
                {
                    if (sightLine.IntersectsCircle(circle))
                    {
                        return false;
                    }
                }
            }

            return true;
        }

        private bool IsPointInsideRect(Vector2 point, RectangleShape rect)
        {
            return point.X >= rect.Position.X &&
                   point.X <= rect.Position.X + rect.Width &&
                   point.Y >= rect.Position.Y &&
                   point.Y <= rect.Position.Y + rect.Height;
        }

        private BehaviourStatus CanSeeTarget(GameTime gameTime)
        {
            if (_target == null)
            {
                _hasLineOfSight = false;
                return BehaviourStatus.Failure;
            }

            float distance = Vector2.Distance(Position, _target.TargetPosition);
            if (distance >= SightRange)
            {
                _hasLineOfSight = false;
                return BehaviourStatus.Failure;
            }

            _hasLineOfSight = CheckLineOfSight(Position, _target.TargetPosition);

            if (_hasLineOfSight)
            {
                _lastKnownPlayerPosition = _target.TargetPosition;

                if (_maze != null)
                {
                    _currentPath = _maze.FindPath(Position, _target.TargetPosition) ?? new List<Vector2>();
                    _currentPathIndex = 0;
                    _lastPathTargetPosition = _target.TargetPosition;
                }
                else
                {
                    _currentPath.Clear();
                    _currentPathIndex = 0;
                    _lastPathTargetPosition = null;
                }

                return BehaviourStatus.Success;
            }

            return BehaviourStatus.Failure;
        }

        private BehaviourStatus HasLastKnownPosition(GameTime gameTime)
        {
            return _lastKnownPlayerPosition.HasValue ? BehaviourStatus.Success : BehaviourStatus.Failure;
        }

        private bool IsJumpRequired(Vector2 targetPos)
        {
            float verticalDiff = targetPos.Y - Position.Y;
            bool targetIsAbove = verticalDiff < -20f;
            bool canJump = _isGrounded || _jumpCount < MAXJUMPS;

            return targetIsAbove && canJump;
        }

        private BehaviourStatus ShouldJumpToReachTarget(GameTime gameTime)
        {
            if (_target == null)
            {
                return BehaviourStatus.Failure;
            }
            return IsJumpRequired(_target.TargetPosition) ? BehaviourStatus.Success : BehaviourStatus.Failure;
        }

        private void EnsurePathExists()
        {
            if (_currentPath.Count == 0 && _maze != null && _lastKnownPlayerPosition.HasValue)
            {
                _currentPath = _maze.FindPathWithWallJumps(Position, _lastKnownPlayerPosition.Value, MAXJUMPS);
                _currentPathIndex = 0;
            }
        }

        private BehaviourStatus ShouldJumpToReachWaypoint(GameTime gameTime)
        {
            EnsurePathExists();

            if (_currentPath.Count > 0 && _currentPathIndex < _currentPath.Count)
            {
                return IsJumpRequired(_currentPath[_currentPathIndex]) ? BehaviourStatus.Success : BehaviourStatus.Failure;
            }
            return BehaviourStatus.Failure;
        }

        private BehaviourStatus ShouldWallJumpForWaypoint(GameTime gameTime)
        {
            if (!_isGrounded && _wallDirection != 0 && _jumpCooldown <= 0)
            {
                EnsurePathExists();
                if (_currentPath.Count > 0 && _currentPathIndex < _currentPath.Count)
                {
                    Vector2 targetWaypoint = _currentPath[_currentPathIndex];
                    float verticalDiff = targetWaypoint.Y - Position.Y;
                    if (verticalDiff < -20f)
                    {
                        return BehaviourStatus.Success;
                    }
                }
            }
            return BehaviourStatus.Failure;
        }

        private BehaviourStatus NeedsToReachWall(GameTime gameTime)
        {
            EnsurePathExists();

            if (_currentPath.Count > 0 && _currentPathIndex < _currentPath.Count)
            {
                Vector2 targetWaypoint = _currentPath[_currentPathIndex];
                float verticalDiff = targetWaypoint.Y - Position.Y;
                float heightNeeded = Math.Abs(verticalDiff);

                // Check if need more jumps
                float singleJumpHeight = 60f;
                bool needsMultipleJumps = verticalDiff < -singleJumpHeight;

                if (needsMultipleJumps && _isGrounded && _wallDirection == 0)
                {
                    // Find nearest wall to jump to
                    _wallJumpTarget = FindNearestWallPosition();
                    if (_wallJumpTarget.HasValue)
                    {
                        _needsWallJump = true;
                        return BehaviourStatus.Success;
                    }
                }
            }
            _needsWallJump = false;
            _wallJumpTarget = null;
            return BehaviourStatus.Failure;
        }

        private Vector2? FindNearestWallPosition()
        {
            if (_maze == null) return null;

            Cell currentCell = _maze.GetCellFromPosition(Position);
            if (currentCell == null) return null;

            float cellSize = _maze.CellSize;
            Vector2 cellCenter = _maze.GetCellCenter(currentCell);

            List<Vector2> wallPositions = new List<Vector2>();

            if (currentCell.HasLeftWall)
            {
                wallPositions.Add(new Vector2(cellCenter.X - cellSize / 2 + _radius + 5, Position.Y));
            }
            if (currentCell.HasRightWall)
            {
                wallPositions.Add(new Vector2(cellCenter.X + cellSize / 2 - _radius - 5, Position.Y));
            }

            int x = currentCell.X;
            int y = currentCell.Y;

            if (x > 0)
            {
                Cell leftCell = _maze.GetCellFromPosition(new Vector2((x - 1) * cellSize + cellSize / 2, y * cellSize + cellSize / 2));
                if (leftCell != null && leftCell.HasRightWall)
                {
                    wallPositions.Add(new Vector2((x - 1) * cellSize + cellSize - _radius - 5, Position.Y));
                }
            }
            if (x < _maze.Width - 1)
            {
                Cell rightCell = _maze.GetCellFromPosition(new Vector2((x + 1) * cellSize + cellSize / 2, y * cellSize + cellSize / 2));
                if (rightCell != null && rightCell.HasLeftWall)
                {
                    wallPositions.Add(new Vector2((x + 1) * cellSize + _radius + 5, Position.Y));
                }
            }

            if (wallPositions.Count == 0)
            {
                return null;
            }
            return wallPositions.OrderBy(w => Vector2.Distance(Position, w)).FirstOrDefault();
        }

        private BehaviourStatus MoveToWall(GameTime gameTime)
        {
            if (!_wallJumpTarget.HasValue)
            {
                return BehaviourStatus.Failure;
            }

            _currentState = "Moving to Wall";
            Vector2 target = _wallJumpTarget.Value;
            float distance = Vector2.Distance(Position, target);

            if (distance < 15f || _wallDirection != 0)
            {
                _needsWallJump = false;
                _wallJumpTarget = null;

                if (_jumpCount < MAXJUMPS && _jumpCooldown <= 0)
                {
                    float deltaTime = (float)gameTime.ElapsedGameTime.TotalSeconds;
                    if (deltaTime <= 0f) deltaTime = 1f / 60f;

                    Vector2 targetVel = new Vector2(Velocity.X, JumpVelocity);
                    Vector2 deltaV = targetVel - Velocity;
                    ApplyImpulse(deltaV, deltaTime);

                    _jumpCount++;
                    _isGrounded = false;
                    _jumpCooldown = JumpCooldownDuration;
                    return BehaviourStatus.Success;
                }
                return BehaviourStatus.Failure;
            }

            float horizontalDirection = Math.Sign(target.X - Position.X);
            ApplyForce(new Vector2(horizontalDirection * MoveForce, 0));

            return BehaviourStatus.Running;
        }

        private void EnsureChasePathExists(Vector2 destination)
        {
            if (_maze == null) return;

            bool needRecompute = false;

            if (_currentPath == null || _currentPath.Count == 0)
            {
                needRecompute = true;
            }
            else if (!_lastPathTargetPosition.HasValue)
            {
                needRecompute = true;
            }
            else
            {
                float moved = Vector2.Distance(_lastPathTargetPosition.Value, destination);
                if (moved > 16f)
                {
                    needRecompute = true;
                }
            }

            if (needRecompute)
            {
                List<Vector2> newPath = _maze.FindPath(Position, destination) ?? new List<Vector2>();
                _currentPath = newPath;
                _currentPathIndex = 0;
                _lastPathTargetPosition = destination;
            }
        }

        private BehaviourStatus ChaseLastKnownPosition(GameTime gameTime)
        {
            if (!_lastKnownPlayerPosition.HasValue)
            {
                return BehaviourStatus.Failure;
            }

            _currentState = "Investigating";
            EnsurePathExists();

            if (_currentPath.Count > 0 && _currentPathIndex < _currentPath.Count)
            {
                Vector2 targetWaypoint = _currentPath[_currentPathIndex];
                float distanceToWaypoint = Vector2.Distance(Position, targetWaypoint);

                if (distanceToWaypoint < 20f)
                {
                    _currentPathIndex++;
                    if (_currentPathIndex >= _currentPath.Count)
                    {
                        _lastKnownPlayerPosition = null;
                        _currentPath.Clear();
                        _currentState = "Idle";
                        return BehaviourStatus.Success;
                    }
                    targetWaypoint = _currentPath[_currentPathIndex];
                }

                Vector2 direction = targetWaypoint - Position;
                float horizontalDirection = Math.Sign(direction.X);
                ApplyForce(new Vector2(horizontalDirection * MoveForce, 0));

                return BehaviourStatus.Running;
            }

            _lastKnownPlayerPosition = null;
            _currentState = "Idle";
            return BehaviourStatus.Success;
        }

        private BehaviourStatus ShouldWallJump(GameTime gameTime)
        {
            if (_target == null)
            {
                return BehaviourStatus.Failure;
            }

            bool canWallJump = !_isGrounded && _wallDirection != 0;
            float verticalDiff = _target.TargetPosition.Y - Position.Y;
            bool targetIsAbove = verticalDiff < 0f;

            return (canWallJump && targetIsAbove) ? BehaviourStatus.Success : BehaviourStatus.Failure;
        }

        private BehaviourStatus PerformJump(GameTime gameTime)
        {
            if (_jumpCount < MAXJUMPS && _jumpCooldown <= 0)
            {
                _currentState = "Jumping";

                float deltaTime = (float)gameTime.ElapsedGameTime.TotalSeconds;
                if (deltaTime <= 0f) deltaTime = 1f / 60f;

                Vector2 targetVel = new Vector2(Velocity.X, JumpVelocity);
                Vector2 deltaV = targetVel - Velocity;
                ApplyImpulse(deltaV, deltaTime);

                _jumpCount++;
                _isGrounded = false;
                _jumpCooldown = JumpCooldownDuration;
                return BehaviourStatus.Success;
            }
            return BehaviourStatus.Failure;
        }

        private BehaviourStatus PerformWallJump(GameTime gameTime)
        {
            if (!_isGrounded && _wallDirection != 0 && _jumpCooldown <= 0)
            {
                _currentState = "Wall Jumping";

                float deltaTime = (float)gameTime.ElapsedGameTime.TotalSeconds;
                if (deltaTime <= 0f)
                {
                    deltaTime = 1f / 60f;
                }
                Vector2 targetVel = new Vector2(_wallDirection * -1 * MoveForce * 0.6f, JumpVelocity);
                Vector2 deltaV = targetVel - Velocity;
                ApplyImpulse(deltaV, deltaTime);
                _jumpCount = 1;
                _jumpCooldown = JumpCooldownDuration;
                return BehaviourStatus.Success;
            }
            return BehaviourStatus.Failure;
        }

        private BehaviourStatus ChaseTarget(GameTime gameTime)
        {
            if (_target == null)
            {
                return BehaviourStatus.Failure;
            }

            float distance = Vector2.Distance(Position, _target.TargetPosition);

            if (distance > MaxChaseDistance)
            {
                _currentState = "Lost Target";
                return BehaviourStatus.Failure;
            }

            _currentState = "Chasing";
            Vector2 direction = _target.TargetPosition - Position;
            EnsureChasePathExists(_target.TargetPosition);

            float horizontalDirection = Math.Sign(direction.X);
            ApplyForce(new Vector2(horizontalDirection * MoveForce, 0));

            return BehaviourStatus.Running;
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

        public Circle BoundingCircle()
        {
            return new Circle(Position, _radius);
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

                            bool otherIsStationary = false;
                            if (collidable is Enemy otherEnemy)
                            {
                                otherIsStationary = otherEnemy.IsStationary;
                            }

                            if (IsStationary && !otherIsStationary)
                            {
                                Position += normal * penetration;
                            }
                            else
                            {
                                Position += normal * penetration * 0.5f;
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

                            float bounce = 0.1f;
                            float velocityDot = Vector2.Dot(Velocity, normal);
                            if (velocityDot < 0)
                            {
                                Velocity -= (1 + bounce) * normal * velocityDot;
                            }
                        }
                    }
                }
                if (!collisionDetected) break;
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

        private void PickRandomDestination()
        {
            if (_maze == null) return;

            int x = _random.Next(0, _maze.Width);
            int y = _random.Next(0, _maze.Height);

            Vector2 targetPos = new Vector2(
                x * _maze.CellSize + _maze.CellSize / 2f,
                y * _maze.CellSize + _maze.CellSize / 2f
            );

            _currentPath = _maze.FindPath(Position, targetPos);
            _currentPathIndex = 0;
        }

        private BehaviourStatus ExploreMap(GameTime gameTime)
        {
            _currentState = "Exploring";

            if (_currentPath.Count == 0)
            {
                PickRandomDestination();
                if (_currentPath.Count == 0) return BehaviourStatus.Failure;
            }

            if (_currentPath.Count > 0 && _currentPathIndex < _currentPath.Count)
            {
                Vector2 targetWaypoint = _currentPath[_currentPathIndex];
                float distanceToWaypoint = Vector2.Distance(Position, targetWaypoint);

                if (distanceToWaypoint < 20f)
                {
                    _currentPathIndex++;
                    if (_currentPathIndex >= _currentPath.Count)
                    {
                        _currentPath.Clear();
                        return BehaviourStatus.Success;
                    }
                }

                Vector2 direction = targetWaypoint - Position;
                float horizontalDirection = Math.Sign(direction.X);
                ApplyForce(new Vector2(horizontalDirection * MoveForce, 0));

                return BehaviourStatus.Running;
            }

            return BehaviourStatus.Success;
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

            _lineTexture = new Texture2D(graphicsDevice, 1, 1);
            _lineTexture.SetData(new[] { Color.White });
        }

        private void UpdateBehaviourMode(GameTime gameTime)
        {
            if (_mode == BehaviourMode.Default) return;

            bool canSeeTarget = CanSeeTarget(gameTime) == BehaviourStatus.Success;

            if (canSeeTarget)
            {
                if (_mode != BehaviourMode.Chase)
                {
                    SetBehaviourMode((int)BehaviourMode.Chase);
                }
            }
            else
            {
                if (_mode == BehaviourMode.Chase)
                {
                    SetBehaviourMode((int)BehaviourMode.Investigate);
                }
                else if (_mode == BehaviourMode.Investigate && !_lastKnownPlayerPosition.HasValue)
                {
                    SetBehaviourMode((int)BehaviourMode.Explore);
                }
            }
        }

        public void Update(GameTime gameTime)
        {
            float deltaTime = (float)gameTime.ElapsedGameTime.TotalSeconds;

            if (_jumpCooldown > 0)
            {
                _jumpCooldown -= deltaTime;
            }

            UpdateBehaviourMode(gameTime);
            _behaviourTree?.Tick(gameTime);

            if (_isColliding && Velocity.LengthSquared() < 1f && _isGrounded && !IsStationary)
            {
                PerformJump(gameTime);
            }

            Velocity = new Vector2(Velocity.X * 0.95f, Velocity.Y);
            base.Update(deltaTime);
        }

        public void Draw(SpriteBatch spriteBatch)
        {
            Color tint;
            if (ActiveBehaviourMode == BehaviourMode.Chase)
            {
                tint = Color.Red;
            }
            else if (ActiveBehaviourMode == BehaviourMode.Investigate)
            {
                tint = Color.Orange;
            }
            else if (ActiveBehaviourMode == BehaviourMode.Explore)
            {
                tint = Color.Green;
            }
            else
            {
                tint = Color.Yellow;
            }

            DrawPathOverlay(spriteBatch);

            spriteBatch.Draw(_texture, Position - new Vector2(_radius), tint);
        }

        private void DrawPathOverlay(SpriteBatch spriteBatch)
        {
            Color pathColour;
            if (ActiveBehaviourMode == BehaviourMode.Chase)
            {
                pathColour = Color.Red * 0.65f;
            }
            else if (ActiveBehaviourMode == BehaviourMode.Investigate)
            {
                pathColour = Color.Orange * 0.65f;
            }
            else if (ActiveBehaviourMode == BehaviourMode.Explore)
            {
                pathColour = Color.Green * 0.65f;
            }
            else
            {
                pathColour = Color.Yellow * 0.65f;
            }

            if (_wallJumpTarget.HasValue)
            {
                DrawLine(spriteBatch, Position, _wallJumpTarget.Value, pathColour, 2f);
                return;
            }

            if (_currentPath == null || _currentPathIndex >= _currentPath.Count)
            {
                return;
            }

            Vector2 previousPoint = Position;
            for (int i = _currentPathIndex; i < _currentPath.Count; i++)
            {
                Vector2 nextPoint = _currentPath[i];
                DrawLine(spriteBatch, previousPoint, nextPoint, pathColour, 2f);
                previousPoint = nextPoint;
            }
        }

        private void DrawLine(SpriteBatch spriteBatch, Vector2 start, Vector2 end, Color colour, float thickness)
        {
            Vector2 edge = end - start;
            float length = edge.Length();

            if (length <= 0f)
            {
                return;
            }

            float rotation = (float)Math.Atan2(edge.Y, edge.X);
            spriteBatch.Draw(_lineTexture, start, null, colour, rotation, Vector2.Zero, new Vector2(length, thickness), SpriteEffects.None, 0f);
        }
    }
}