using System;
using System.Collections.Generic;
using System.IO;
using System.Text.Json;
using Microsoft.Xna.Framework;

namespace ProceduralMaze
{
    public static class LevelManager
    {
        private const string LevelFileName = "level.json";

        public static void SaveLevel(List<Enemy> enemies, Maze maze)
        {
            LevelData levelData = new LevelData();
            foreach (Enemy enemy in enemies)
            {
                Cell cell = maze.GetCellFromPosition(enemy.Position);
                if (cell != null)
                {
                    levelData.Enemies.Add(new EnemySpawnData
                    {
                        CellX = cell.X,
                        CellY = cell.Y,
                        BehaviourMode = enemy.ActiveBehaviourMode
                    });
                }
            }

            JsonSerializerOptions options = new JsonSerializerOptions { WriteIndented = true };
            string json = JsonSerializer.Serialize(levelData, options);
            File.WriteAllText(LevelFileName, json);
        }

        public static void LoadLevel(Action<Vector2, Enemy.BehaviourMode> spawnAction, Func<int, int, Vector2> makeCellCenterAction, Action createDefaultLevelAction)
        {
            if (!File.Exists(LevelFileName))
            {
                createDefaultLevelAction();
                return;
            }

            string json = File.ReadAllText(LevelFileName);
            LevelData levelData = JsonSerializer.Deserialize<LevelData>(json);

            foreach (EnemySpawnData enemyData in levelData.Enemies)
            {
                spawnAction(makeCellCenterAction(enemyData.CellX, enemyData.CellY), enemyData.BehaviourMode);
            }
        }
    }
}
