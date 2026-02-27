using Microsoft.Xna.Framework;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace MGGameLibrary.BehaviourTrees
{
    public class SelectorNode : CompositeBehaviourNode
    {
        private int _current;

        public override void onInitialise()
        {
            _current = 0;
        }

        public override BehaviourStatus Tick(GameTime gameTime)
        {
            _current = 0;

            while (_current < _children.Count)
            {
                BehaviourStatus status = _children[_current].Tick(gameTime);
                if (status == BehaviourStatus.Running)
                {
                    return BehaviourStatus.Running;
                }
                if (status == BehaviourStatus.Success)
                {
                    _current = 0;
                    return BehaviourStatus.Success;
                }
                _current++;
            }
            _current = 0;
            return BehaviourStatus.Failure;
        }

        public override void onTerminate(BehaviourStatus status)
        {
            _current = 0;
        }
    }
}
