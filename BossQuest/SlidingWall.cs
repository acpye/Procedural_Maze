using MGGameLibrary.Shapes;
using Microsoft.Xna.Framework;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace ProceduralMaze
{
    public class SlidingWall
    {
        public enum SlideDirection { Horizontal, Vertical }

        private readonly Shape _shape;
        private readonly Vector2 _originalPosition;
        private readonly SlideDirection _direction;
        private readonly int _maxSlideDistance;
        private readonly float _slideSpeed = 80f;

        private Vector2 _targetPosition;
        private bool _isMoving;
        private bool _isAtOriginal = true;

        public bool IsMoving => _isMoving;

        public SlidingWall(Shape shape, SlideDirection direction, int maxSlideDistance)
        {
            _shape = shape;
            _originalPosition = shape.Position;
            _direction = direction;
            _maxSlideDistance = maxSlideDistance;
            _targetPosition = _originalPosition;
        }

        public void StartShift()
        {
            if (_isMoving) return;

            _isMoving = true;

            if (_isAtOriginal)
            {
                // Move to shifted position
                Vector2 offset = _direction == SlideDirection.Horizontal
                    ? new Vector2(_maxSlideDistance, 0)
                    : new Vector2(0, _maxSlideDistance);
                _targetPosition = _originalPosition + offset;
            }
            else
            {
                // Return to original position
                _targetPosition = _originalPosition;
            }

            _isAtOriginal = !_isAtOriginal;
        }

        public void Update(float deltaTime)
        {
            if (!_isMoving) return;

            Vector2 currentPos = _shape.Position;
            Vector2 direction = _targetPosition - currentPos;
            float distance = direction.Length();

            if (distance < 1f)
            {
                // Reached target
                _shape.Position = _targetPosition;
                _isMoving = false;
            }
            else
            {
                // Move towards target smoothly
                direction.Normalize();
                float moveAmount = _slideSpeed * deltaTime;
                if (moveAmount > distance) moveAmount = distance;

                _shape.Position = currentPos + direction * moveAmount;
            }
        }
    }

}
