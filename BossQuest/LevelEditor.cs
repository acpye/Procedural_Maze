using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;
using Microsoft.Xna.Framework.Input;
using System;
using System.Collections.Generic;
using System.Linq;

namespace ProceduralMaze
{
    public class LevelEditor
    {
        private readonly Maze _maze;
        private readonly List<Enemy> _enemies;
        private readonly SpriteFont _font;
        private readonly Action<Vector2, Enemy.BehaviourMode> _spawnAction;
        private readonly Action _saveAction;
        private Enemy.BehaviourMode _selectedEnemyMode = Enemy.BehaviourMode.Explore;

        public LevelEditor(Maze maze, List<Enemy> enemies, SpriteFont font, Action<Vector2, Enemy.BehaviourMode> spawnAction, Action saveAction)
        {
            _maze = maze;
            _enemies = enemies;
            _font = font;
            _spawnAction = spawnAction;
            _saveAction = saveAction;
        }

        public void Update(KeyboardState keyboardState, MouseState mouseState, KeyboardState previousKeyboardState, MouseState previousMouseState)
        {
            // Cycle behaviour
            if (keyboardState.IsKeyDown(Keys.E) && previousKeyboardState.IsKeyUp(Keys.E))
            {
                int currentMode = (int)_selectedEnemyMode;
                currentMode++;
                if (!Enum.IsDefined(typeof(Enemy.BehaviourMode), currentMode))
                {
                    currentMode = (int)Enemy.BehaviourMode.Default;
                }
                _selectedEnemyMode = (Enemy.BehaviourMode)currentMode;
            }

            // Place enemy
            if (mouseState.LeftButton == ButtonState.Pressed && previousMouseState.LeftButton == ButtonState.Released)
            {
                Cell cell = _maze.GetCellFromPosition(mouseState.Position.ToVector2());
                if (cell != null)
                {
                    Vector2 position = new Vector2(cell.X * _maze.CellSize + _maze.CellSize / 2f, cell.Y * _maze.CellSize + _maze.CellSize / 2f);
                    _spawnAction(position, _selectedEnemyMode);
                }
            }

            // Remove enemy
            if (mouseState.RightButton == ButtonState.Pressed && previousMouseState.RightButton == ButtonState.Released)
            {
                Enemy enemyToRemove = _enemies.FirstOrDefault(e => Vector2.Distance(e.Position, mouseState.Position.ToVector2()) < _maze.CellSize / 2f);
                if (enemyToRemove != null)
                {
                    _enemies.Remove(enemyToRemove);
                }
            }

            // Save level
            if (keyboardState.IsKeyDown(Keys.LeftControl) && keyboardState.IsKeyDown(Keys.S) && !(previousKeyboardState.IsKeyDown(Keys.LeftControl) && previousKeyboardState.IsKeyDown(Keys.S)))
            {
                _saveAction();
            }
        }

        public void Draw(SpriteBatch spriteBatch)
        {
            string text = $"EDIT MODE\n" +
                          $"Selected Enemy: {_selectedEnemyMode} (Press E to cycle)\n" +
                          $"Left-Click: Place Enemy\n" +
                          $"Right-Click: Remove Enemy\n" +
                          $"Ctrl+S: Save Level\n" +
                          $"F1: Exit Editor";
            spriteBatch.DrawString(_font, text, new Vector2(10, 10), Color.White);

            Vector2 mousePosition = Mouse.GetState().Position.ToVector2();
            Cell cell = _maze.GetCellFromPosition(mousePosition);
            if (cell != null)
            {
                Rectangle rectangle = new Rectangle(cell.X * _maze.CellSize, cell.Y * _maze.CellSize, _maze.CellSize, _maze.CellSize);
            }
        }
    }
}