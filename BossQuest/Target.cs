using MGGameLibrary.Targetable;
using Microsoft.Xna.Framework;

namespace ProceduralMaze
{
    public class Target : SimpleTargetable
    {
        public Target(Vector2 position) : base(position)
        {
        }
    }

    public class PlayerTargetable : ITargetable
    {
        private readonly Player _player;

        public PlayerTargetable(Player player)
        {
            _player = player;
        }

        public Vector2 TargetPosition => _player.Position;
    }
}