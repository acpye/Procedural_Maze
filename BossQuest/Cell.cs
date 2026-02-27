using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace ProceduralMaze
{
    public class Cell
    {
        public int X { get; }
        public int Y { get; }
        public bool Visited { get; set; }
        public bool HasTopWall { get; set; } = true;
        public bool HasBottomWall { get; set; } = true;
        public bool HasLeftWall { get; set; } = true;
        public bool HasRightWall { get; set; } = true;
        public Cell(int x, int y)
        {
            X = x;
            Y = y;
        }
    }
}
