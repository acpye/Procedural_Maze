using Microsoft.Xna.Framework;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace MGGameLibrary.BehaviourTrees
{
    public class TimedDecoratorNode : BehaviourNode
    {
        private BehaviourNode _child;
        private float _duration;
        private float _elapsedTime;

        public TimedDecoratorNode(BehaviourNode child, float duration)
        {
            _child = child;
            _duration = duration;
        }

        public override void onInitialise()
        {
            _elapsedTime = 0f;
        }

        public override BehaviourStatus update(GameTime gameTime)
        {
            if (_child == null)
            {
                return BehaviourStatus.Failure;
            }

            BehaviourStatus status = _child.Tick(gameTime);

            if (status == BehaviourStatus.Running)
            {
                _elapsedTime += (float)gameTime.ElapsedGameTime.TotalSeconds;

                if (_elapsedTime >= _duration)
                {
                    return BehaviourStatus.Success;
                }
                return BehaviourStatus.Running;
            }

            return status;
        }
    }
}
