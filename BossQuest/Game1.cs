using MGGameLibrary.Collision;
using MGGameLibrary.Physics;
using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;
using Microsoft.Xna.Framework.Input;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text.Json;

namespace ProceduralMaze
{
    public enum GameState
    {
        Play,
        Edit
    }

    public class Game1 : Game
    {
        private GraphicsDeviceManager _graphics;
        private SpriteBatch _spriteBatch;
        private Maze _maze;
        private Player _player;
        private List<Enemy> _enemies;
        private SpriteFont _font;
        private KeyboardState _previousKeyboardState;
        private MouseState _previousMouseState;

        private GameState _gameState = GameState.Play;
        private LevelEditor _levelEditor;

        public Game1()
        {
            _graphics = new GraphicsDeviceManager(this);
            Content.RootDirectory = "Content";
            IsMouseVisible = true;
        }

        protected override void Initialize()
        {
            _previousKeyboardState = Keyboard.GetState();
            _previousMouseState = Mouse.GetState();

            _maze = new Maze(GraphicsDevice, 16, 10, 48);

            _graphics.PreferredBackBufferWidth = _maze.Width * _maze.CellSize;
            _graphics.PreferredBackBufferHeight = _maze.Height * _maze.CellSize;
            _graphics.ApplyChanges();

            _player = new Player(MakeCellCenter(0, 0), 6);
            _enemies = new List<Enemy>();

            LevelManager.LoadLevel(SpawnEnemy, MakeCellCenter, CreateDefaultLevel);

            base.Initialize();
        }

        protected override void LoadContent()
        {
            _spriteBatch = new SpriteBatch(GraphicsDevice);
            _maze.LoadContent(GraphicsDevice);
            _player.LoadContent(GraphicsDevice);

            foreach (Enemy enemy in _enemies)
            {
                enemy.LoadContent(GraphicsDevice);
            }

            _font = Content.Load<SpriteFont>("font");

            _levelEditor = new LevelEditor(_maze, _enemies, _font, SpawnEnemy, () => LevelManager.SaveLevel(_enemies, _maze));
        }

        protected override void Update(GameTime gameTime)
        {
            KeyboardState keyboardState = Keyboard.GetState();
            MouseState mouseState = Mouse.GetState();

            if (GamePad.GetState(PlayerIndex.One).Buttons.Back == ButtonState.Pressed || keyboardState.IsKeyDown(Keys.Escape))
                Exit();

            if (keyboardState.IsKeyDown(Keys.F1) && _previousKeyboardState.IsKeyUp(Keys.F1))
            {
                _gameState = _gameState == GameState.Play ? GameState.Edit : GameState.Play;
                IsMouseVisible = _gameState == GameState.Edit;
            }

            switch (_gameState)
            {
                case GameState.Play:
                    UpdatePlayMode(gameTime);
                    break;
                case GameState.Edit:
                    _levelEditor.Update(keyboardState, mouseState, _previousKeyboardState, _previousMouseState);
                    break;
            }

            _previousKeyboardState = keyboardState;
            _previousMouseState = mouseState;

            base.Update(gameTime);
        }

        protected override void Draw(GameTime gameTime)
        {
            GraphicsDevice.Clear(Color.CornflowerBlue);

            _spriteBatch.Begin();
            _maze.Draw(_spriteBatch);
            _player.Draw(_spriteBatch);

            foreach (Enemy enemy in _enemies)
            {
                enemy.Draw(_spriteBatch);
                _spriteBatch.DrawString(_font, enemy.CurrentState, enemy.Position - new Vector2(20, 20), Color.White);
            }

            if (_gameState == GameState.Edit)
            {
                _levelEditor.Draw(_spriteBatch);
            }

            _spriteBatch.End();

            base.Draw(gameTime);
        }

        private void CreateDefaultLevel()
        {
            SpawnEnemy(MakeCellCenter(0, 0), Enemy.BehaviourMode.Explore);
            SpawnEnemy(MakeCellCenter(15, 9), Enemy.BehaviourMode.Explore);
            LevelManager.SaveLevel(_enemies, _maze);
        }

        private Vector2 MakeCellCenter(int cellX, int cellY)
        {
            float centerX = cellX * _maze.CellSize + _maze.CellSize / 2f;
            float centerY = cellY * _maze.CellSize + _maze.CellSize / 2f;
            return new Vector2(centerX, centerY);
        }

        private void SpawnEnemy(Vector2 position, Enemy.BehaviourMode mode)
        {
            Enemy enemy = new Enemy(position, 6, _player, _maze);
            enemy.SetBehaviourMode((int)mode);
            if (_spriteBatch != null)
            {
                enemy.LoadContent(GraphicsDevice);
            }
            _enemies.Add(enemy);
        }

        private void UpdatePlayMode(GameTime gameTime)
        {
            _maze.Update(gameTime);

            _player.HandleInput();
            _player.ApplyGravity();
            _player.Update(gameTime);

            List<ICollidable> playerCollidables = new List<ICollidable>(_maze.WallCollidables);
            playerCollidables.AddRange(_enemies);
            _player.CheckCollisions(playerCollidables);

            foreach (Enemy enemy in _enemies)
            {
                enemy.ApplyGravity();
                enemy.Update(gameTime);

                List<ICollidable> enemyCollidables = new List<ICollidable>(_maze.WallCollidables);
                enemyCollidables.Add(_player);
                enemyCollidables.AddRange(_enemies.Where(e => e != enemy));

                enemy.CheckCollisions(enemyCollidables);
            }
        }
    }
}