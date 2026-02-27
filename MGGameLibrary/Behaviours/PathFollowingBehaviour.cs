using MGGameLibrary.Targetable;
using Microsoft.Xna.Framework;
using System.Collections.Generic;
using System.Linq;

namespace MGGameLibrary.Behaviours
{
    public class PathFollowingBehaviour : SteeringBehaviour
    {
        public List<ITargetable> PathPoints { get; private set; }
        public int _currentTargetIndex;
        private readonly float _arrivalThreshold;
        private readonly SeekBehaviour _seekBehaviour;
        private const float LookAheadDistance = 15.0f;

        public PathFollowingBehaviour(List<ITargetable> pathPoints, float arrivalThreshold)
        {
            PathPoints = pathPoints;
            _arrivalThreshold = arrivalThreshold;
            _currentTargetIndex = 0;
            _seekBehaviour = new SeekBehaviour(PathPoints.FirstOrDefault());
        }

        public void SetPath(List<ITargetable> newPath)
        {
            if (newPath != null && newPath.Count > 0)
            {
                PathPoints = newPath;
                _currentTargetIndex = 0;
                _seekBehaviour.Target = PathPoints[_currentTargetIndex];
            }
            else
            {
                PathPoints = new List<ITargetable>();
                _currentTargetIndex = 0;
                _seekBehaviour.Target = null;
            }
        }

        public override Vector2 CalculateSteeringForce(Agent agent)
        {
            if (PathPoints == null || PathPoints.Count == 0 || _seekBehaviour.Target == null)
            {
                agent.Velocity = Vector2.Zero;
                return Vector2.Zero;
            }

            if (_currentTargetIndex == PathPoints.Count - 1 && Vector2.Distance(agent.Position, PathPoints.Last().TargetPosition) < _arrivalThreshold)
            {
                agent.Velocity = Vector2.Zero;
                _seekBehaviour.Target = PathPoints.Last();
                return Vector2.Zero;
            }

            Vector2 currentPathPosition = FindClosestPointOnPath(agent.Position);

            Vector2 lookAheadPoint = FindLookAheadPoint(currentPathPosition);

            _seekBehaviour.Target = new SimpleTargetable(lookAheadPoint);

            return _seekBehaviour.CalculateSteeringForce(agent);
        }

        private Vector2 FindClosestPointOnPath(Vector2 agentPosition)
        {
            float minDistanceSq = float.MaxValue;
            Vector2 closestPoint = Vector2.Zero;
            int closestSegmentIndex = 0;

            for (int i = 0; i < PathPoints.Count - 1; i++)
            {
                Vector2 p1 = PathPoints[i].TargetPosition;
                Vector2 p2 = PathPoints[i + 1].TargetPosition;
                Vector2 segment = p2 - p1;
                float segmentLengthSq = segment.LengthSquared();

                if (segmentLengthSq == 0.0f) continue;

                float t = MathHelper.Clamp(Vector2.Dot(agentPosition - p1, segment) / segmentLengthSq, 0, 1);
                Vector2 projection = p1 + t * segment;
                float distanceSq = Vector2.DistanceSquared(agentPosition, projection);

                if (distanceSq < minDistanceSq)
                {
                    minDistanceSq = distanceSq;
                    closestPoint = projection;
                    closestSegmentIndex = i;
                }
            }
            _currentTargetIndex = closestSegmentIndex;
            return closestPoint;
        }

        private Vector2 FindLookAheadPoint(Vector2 currentPathPosition)
        {
            Vector2 lookAheadPoint = currentPathPosition;
            float remainingDistance = LookAheadDistance;

            for (int i = _currentTargetIndex; i < PathPoints.Count - 1; i++)
            {
                Vector2 p1 = (i == _currentTargetIndex) ? currentPathPosition : PathPoints[i].TargetPosition;
                Vector2 p2 = PathPoints[i + 1].TargetPosition;
                Vector2 segment = p2 - p1;
                float segmentLength = segment.Length();

                if (segmentLength > remainingDistance)
                {
                    lookAheadPoint = p1 + (segment / segmentLength) * remainingDistance;
                    return lookAheadPoint;
                }

                remainingDistance -= segmentLength;
            }

            return PathPoints.Last().TargetPosition;
        }
    }
}