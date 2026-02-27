using MGGameLibrary.Collision;
using MGGameLibrary.Shapes;
using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;
using System;
using System.Collections.Generic;
using System.Linq;

namespace ProceduralMaze
{
    public class Maze
    {
        public enum WallSide { Top, Bottom, Left, Right }

        private readonly int _width;
        private readonly int _height;
        private readonly int _cellSize;
        private readonly Cell[,] _cells;
        private readonly Random _random = new Random();
        private Color _wallColour;
        private Texture2D _wallTexture;
        public List<ICollidable> WallCollidables { get; } = new List<ICollidable>();

        private List<SlidingWall> _slidingWalls = new List<SlidingWall>();
        private const int MaxSlidingDoors = 20; 
        private const float DoorShiftInterval = 4f;
        private float _doorShiftTimer;

        public int Width => _width;
        public int Height => _height;
        public int CellSize => _cellSize;

        public Maze(GraphicsDevice graphicsDevice, int width, int height, int cellSize)
        {
            _width = width;
            _height = height;
            _cellSize = cellSize;
            _cells = new Cell[width, height];
            for (int x = 0; x < width; x++)
            {
                for (int y = 0; y < height; y++)
                {
                    _cells[x, y] = new Cell(x, y);
                }
            }
            _wallColour = Color.White;
            _wallTexture = new Texture2D(graphicsDevice, 1, 1);
            _wallTexture.SetData(new[] { Color.White });

            GenerateMaze();
            CreateWalls();
            SelectInitialSlidingDoors();
        }

        public void Update(GameTime gameTime)
        {
            float deltaTime = (float)gameTime.ElapsedGameTime.TotalSeconds;

            foreach (SlidingWall door in _slidingWalls)
            {
                door.Update(deltaTime);
            }

            _doorShiftTimer += deltaTime;
            if (_doorShiftTimer >= DoorShiftInterval)
            {
                _doorShiftTimer = 0f;
                TriggerDoorShifts();
            }
        }

        private void SelectInitialSlidingDoors()
        {
            List<SlidingWall> innerWalls = _allInnerWalls.OrderBy(_ => _random.Next()).Take(MaxSlidingDoors).ToList();
            foreach (SlidingWall wall in innerWalls)
            {
                _slidingWalls.Add(wall);
            }
        }

        private void TriggerDoorShifts()
        {
            foreach (SlidingWall door in _slidingWalls)
            {
                if (!door.IsMoving)
                {
                    door.StartShift();
                }
            }

            if (_random.NextDouble() < 0.3 && _allInnerWalls.Count > MaxSlidingDoors)
            {
                int indexToReplace = _random.Next(_slidingWalls.Count);
                SlidingWall currentDoor = _slidingWalls[indexToReplace];

                if (!currentDoor.IsMoving)
                {
                    List<SlidingWall> availableWalls = _allInnerWalls.Except(_slidingWalls).ToList();
                    if (availableWalls.Count > 0)
                    {
                        SlidingWall newDoor = availableWalls[_random.Next(availableWalls.Count)];
                        _slidingWalls[indexToReplace] = newDoor;
                    }
                }
            }
        }

        private List<SlidingWall> _allInnerWalls = new List<SlidingWall>();

        public Cell GetCellFromPosition(Vector2 position)
        {
            int x = (int)(position.X / _cellSize);
            int y = (int)(position.Y / _cellSize);
            if (x < 0 || x >= _width || y < 0 || y >= _height)
            {
                return null;
            }
            return _cells[x, y];
        }

        public Vector2 GetCellCenter(Cell cell)
        {
            return new Vector2(cell.X * _cellSize + _cellSize / 2, cell.Y * _cellSize + _cellSize / 2);
        }

        public List<Vector2> FindPath(Vector2 startPos, Vector2 endPos)
        {
            Cell startCell = GetCellFromPosition(startPos);
            Cell endCell = GetCellFromPosition(endPos);

            if (startCell == null || endCell == null)
            {
                return new List<Vector2>();
            }

            List<Cell> openSet = new List<Cell> { startCell };
            Dictionary<Cell, Cell> cameFrom = new Dictionary<Cell, Cell>();

            Dictionary<Cell, float> gScore = new Dictionary<Cell, float>();
            for (int x = 0; x < _width; x++)
            {
                for (int y = 0; y < _height; y++)
                {
                    gScore[_cells[x, y]] = float.MaxValue;
                }
                gScore[startCell] = 0;
            }

            Dictionary<Cell, float> fScore = new Dictionary<Cell, float>();
            for (int x = 0; x < _width; x++)
            {
                for (int y = 0; y < _height; y++)
                {
                    fScore[_cells[x, y]] = float.MaxValue;
                }
                fScore[startCell] = Heuristic(startCell, endCell);
            }

            while (openSet.Any())
            {
                Cell current = openSet.OrderBy(c => fScore[c]).First();

                if (current == endCell)
                {
                    return ReconstructPath(cameFrom, current);
                }
                openSet.Remove(current);

                foreach (Cell neighbour in GetNavigableNeighbours(current))
                {
                    float tentativeGScore = gScore[current] + 1;
                    if (tentativeGScore < gScore[neighbour])
                    {
                        cameFrom[neighbour] = current;
                        gScore[neighbour] = tentativeGScore;
                        fScore[neighbour] = gScore[neighbour] + Heuristic(neighbour, endCell);
                        if (!openSet.Contains(neighbour))
                        {
                            openSet.Add(neighbour);
                        }
                    }
                }
            }

            return new List<Vector2>();
        }

        private List<Vector2> ReconstructPath(Dictionary<Cell, Cell> cameFrom, Cell current)
        {
            List<Vector2> totalPath = new List<Vector2> { GetCellCenter(current) };
            while (cameFrom.ContainsKey(current))
            {
                current = cameFrom[current];
                totalPath.Insert(0, GetCellCenter(current));
            }
            return totalPath;
        }

        private float Heuristic(Cell a, Cell b)
        {
            return Math.Abs(a.X - b.X) + Math.Abs(a.Y - b.Y);
        }

        private IEnumerable<Cell> GetNavigableNeighbours(Cell cell)
        {
            int x = cell.X;
            int y = cell.Y;

            if (!cell.HasTopWall && y > 0)
            {
                yield return _cells[x, y - 1];
            }
            if (!cell.HasBottomWall && y < _height - 1)
            {
                yield return _cells[x, y + 1];
            }
            if (!cell.HasLeftWall && x > 0)
            {
                yield return _cells[x - 1, y];
            }
            if (!cell.HasRightWall && x < _width - 1)
            {
                yield return _cells[x + 1, y];
            }
        }

        private void GenerateMaze()
        {
            Stack<Cell> stack = new Stack<Cell>();
            Cell current = _cells[0, 0];
            current.Visited = true;

            stack.Push(current);

            while (stack.Count > 0)
            {
                current = stack.Pop();
                List<Cell> neighbours = GetUnvisitedNeighbours(current);
                if (neighbours.Any())
                {
                    stack.Push(current);
                    Cell next = neighbours[_random.Next(neighbours.Count)];
                    RemoveWalls(current, next);
                    next.Visited = true;
                    stack.Push(next);
                }
            }
        }

        private List<Cell> GetUnvisitedNeighbours(Cell cell)
        {
            List<Cell> neighbours = new List<Cell>();
            int x = cell.X;
            int y = cell.Y;

            if (x > 0 && !_cells[x - 1, y].Visited)
            {
                neighbours.Add(_cells[x - 1, y]);
            }
            if (x < _width - 1 && !_cells[x + 1, y].Visited)
            {
                neighbours.Add(_cells[x + 1, y]);
            }
            if (y > 0 && !_cells[x, y - 1].Visited)
            {
                neighbours.Add(_cells[x, y - 1]);
            }
            if (y < _height - 1 && !_cells[x, y + 1].Visited)
            {
                neighbours.Add(_cells[x, y + 1]);
            }
            return neighbours;
        }

        private bool IsOuterWall(int cellX, int cellY, WallSide side)
        {
            return side switch
            {
                WallSide.Top => cellY == 0,
                WallSide.Bottom => cellY == _height - 1,
                WallSide.Left => cellX == 0,
                WallSide.Right => cellX == _width - 1,
                _ => false
            };
        }

        private void CreateWalls()
        {
            int wallThickness = 5;
            int offset = wallThickness / 2;

            for (int x = 0; x < _width; x++)
            {
                for (int y = 0; y < _height; y++)
                {
                    Cell cell = _cells[x, y];
                    int wallX = x * _cellSize;
                    int wallY = y * _cellSize;

                    if (cell.HasTopWall)
                    {
                        Rectangle wallRect = new Rectangle(wallX - offset, wallY - offset, _cellSize + wallThickness, wallThickness);
                        RectangleShape shape = new RectangleShape(new Vector2(wallRect.X, wallRect.Y), wallRect.Width, wallRect.Height);
                        Obstacle obstacle = new Obstacle(shape);
                        WallCollidables.Add(obstacle);

                        if (!IsOuterWall(x, y, WallSide.Top))
                        {
                            int maxSlide = 1 * _cellSize;
                            _allInnerWalls.Add(new SlidingWall(shape, SlidingWall.SlideDirection.Horizontal, maxSlide));
                        }
                    }
                    if (cell.HasBottomWall)
                    {
                        Rectangle wallRect = new Rectangle(wallX - offset, wallY + _cellSize - offset, _cellSize + wallThickness, wallThickness);
                        RectangleShape shape = new RectangleShape(new Vector2(wallRect.X, wallRect.Y), wallRect.Width, wallRect.Height);
                        Obstacle obstacle = new Obstacle(shape);
                        WallCollidables.Add(obstacle);

                        if (!IsOuterWall(x, y, WallSide.Bottom))
                        {
                            int maxSlide = 1 * _cellSize;
                            _allInnerWalls.Add(new SlidingWall(shape, SlidingWall.SlideDirection.Horizontal, maxSlide));
                        }
                    }
                    if (cell.HasLeftWall)
                    {
                        Rectangle wallRect = new Rectangle(wallX - offset, wallY - offset, wallThickness, _cellSize + wallThickness);
                        RectangleShape shape = new RectangleShape(new Vector2(wallRect.X, wallRect.Y), wallRect.Width, wallRect.Height);
                        Obstacle obstacle = new Obstacle(shape);
                        WallCollidables.Add(obstacle);

                        if (!IsOuterWall(x, y, WallSide.Left))
                        {
                            int maxSlide = 1 * _cellSize;
                            _allInnerWalls.Add(new SlidingWall(shape, SlidingWall.SlideDirection.Vertical, maxSlide));
                        }
                    }
                    if (cell.HasRightWall)
                    {
                        Rectangle wallRect = new Rectangle(wallX + _cellSize - offset, wallY - offset, wallThickness, _cellSize + wallThickness);
                        RectangleShape shape = new RectangleShape(new Vector2(wallRect.X, wallRect.Y), wallRect.Width, wallRect.Height);
                        Obstacle obstacle = new Obstacle(shape);
                        WallCollidables.Add(obstacle);

                        if (!IsOuterWall(x, y, WallSide.Right))
                        {
                            int maxSlide = 1 * _cellSize;
                            _allInnerWalls.Add(new SlidingWall(shape, SlidingWall.SlideDirection.Vertical, maxSlide));
                        }
                    }
                }
            }
        }

        private void RemoveWalls(Cell current, Cell next)
        {
            int x = current.X - next.X;
            if (x == 1)
            {
                current.HasLeftWall = false;
                next.HasRightWall = false;
            }
            else if (x == -1)
            {
                current.HasRightWall = false;
                next.HasLeftWall = false;
            }

            int y = current.Y - next.Y;
            if (y == 1)
            {
                current.HasTopWall = false;
                next.HasBottomWall = false;
            }
            else if (y == -1)
            {
                current.HasBottomWall = false;
                next.HasTopWall = false;
            }
        }

        public List<Vector2> FindPathWithWallJumps(Vector2 startPos, Vector2 endPos, int maxJumps)
        {
            Cell startCell = GetCellFromPosition(startPos);
            Cell endCell = GetCellFromPosition(endPos);

            if (startCell == null || endCell == null)
            {
                return new List<Vector2>();
            }

            List<(Cell cell, int jumpsLeft)> openSet = new List<(Cell cell, int jumpsLeft)> { (startCell, maxJumps) };
            Dictionary<(Cell, int), (Cell, int)?> cameFrom = new Dictionary<(Cell, int), (Cell, int)?>();
            Dictionary<(Cell, int), float> gScore = new Dictionary<(Cell, int), float>();
            Dictionary<(Cell, int), float> fScore = new Dictionary<(Cell, int), float>();

            (Cell, int) startState = (startCell, maxJumps);
            gScore[startState] = 0;
            fScore[startState] = Heuristic(startCell, endCell);
            cameFrom[startState] = null;

            while (openSet.Count > 0)
            {
                (Cell cell, int jumpsLeft) current = openSet.OrderBy(s => fScore.GetValueOrDefault(s, float.MaxValue)).First();

                if (current.cell == endCell)
                {
                    return ReconstructPathWithJumps(cameFrom, current);
                }

                openSet.Remove(current);

                foreach ((Cell cell, int jumpsRemaining, float cost) neighbour in GetNavigableNeighboursWithWallJumps(current.cell, current.jumpsLeft))
                {
                    float tentativeGScore = gScore.GetValueOrDefault(current, float.MaxValue) + neighbour.cost;
                    (Cell, int) neighbourState = (neighbour.cell, neighbour.jumpsRemaining);

                    if (tentativeGScore < gScore.GetValueOrDefault(neighbourState, float.MaxValue))
                    {
                        cameFrom[neighbourState] = current;
                        gScore[neighbourState] = tentativeGScore;
                        fScore[neighbourState] = tentativeGScore + Heuristic(neighbour.cell, endCell);

                        if (!openSet.Any(s => s.cell == neighbourState.Item1 && s.jumpsLeft == neighbourState.Item2))
                        {
                            openSet.Add(neighbourState);
                        }
                    }
                }
            }

            return new List<Vector2>();
        }

        private IEnumerable<(Cell cell, int jumpsRemaining, float cost)> GetNavigableNeighboursWithWallJumps(Cell cell, int jumpsLeft)
        {
            int x = cell.X;
            int y = cell.Y;

            if (!cell.HasTopWall && y > 0)
                yield return (_cells[x, y - 1], jumpsLeft, 1f);
            if (!cell.HasBottomWall && y < _height - 1)
                yield return (_cells[x, y + 1], jumpsLeft, 1f);
            if (!cell.HasLeftWall && x > 0)
                yield return (_cells[x - 1, y], jumpsLeft, 1f);
            if (!cell.HasRightWall && x < _width - 1)
                yield return (_cells[x + 1, y], jumpsLeft, 1f);

            if (jumpsLeft > 0)
            {
                if (cell.HasLeftWall && y > 0)
                    yield return (_cells[x, y - 1], jumpsLeft - 1, 2f);
                if (cell.HasRightWall && y > 0)
                    yield return (_cells[x, y - 1], jumpsLeft - 1, 2f);
                if (cell.HasLeftWall && x > 0 && y > 0)
                    yield return (_cells[x - 1, y - 1], jumpsLeft - 1, 2.5f);
                if (cell.HasRightWall && x < _width - 1 && y > 0)
                    yield return (_cells[x + 1, y - 1], jumpsLeft - 1, 2.5f);
            }
        }

        private List<Vector2> ReconstructPathWithJumps(Dictionary<(Cell, int), (Cell, int)?> cameFrom, (Cell cell, int jumpsLeft) current)
        {
            List<Vector2> totalPath = new List<Vector2> { GetCellCenter(current.cell) };
            while (cameFrom.TryGetValue(current, out (Cell, int)? prev) && prev.HasValue)
            {
                current = prev.Value;
                totalPath.Insert(0, GetCellCenter(current.cell));
            }
            return totalPath;
        }

        public void LoadContent(GraphicsDevice graphicsDevice)
        {
            _wallTexture = new Texture2D(graphicsDevice, 1, 1);
            _wallTexture.SetData(new[] { _wallColour });
        }

        public void Draw(SpriteBatch spriteBatch)
        {
            foreach (ICollidable collidable in WallCollidables)
            {
                if (collidable.Shape is RectangleShape rectangleShape)
                {
                    Rectangle wallRectangle = new Rectangle((int)rectangleShape.Position.X, (int)rectangleShape.Position.Y, (int)rectangleShape.Width, (int)rectangleShape.Height);
                    spriteBatch.Draw(_wallTexture, wallRectangle, _wallColour);
                }
            }
        }
    }
}