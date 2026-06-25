using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace ProceduralMaze
{
    public class EnemySpawnData
    {
        public int CellX { get; set; }
        public int CellY { get; set; }
        public Enemy.BehaviourMode BehaviourMode { get; set; }
    }

    public class LevelData
    {
        public List<EnemySpawnData> Enemies { get; set; } = new List<EnemySpawnData>();
    }
}