using Microsoft.Xna.Framework;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace MGGameLibrary.BehaviourTrees
{
    public class SequenceNode : CompositeBehaviourNode
    {
        private int _current;

        public SequenceNode(params BehaviourNode[] children)
        {
            foreach (BehaviourNode child in children)
            {
                addChild(child);
            }
        }

        public override void onInitialise()
        {
            _current = 0;
        }

        public override BehaviourStatus Tick(GameTime gameTime)
        {
            while (_current < _children.Count)
            {
                BehaviourStatus status = _children[_current].Tick(gameTime);
                if (status == BehaviourStatus.Running)
                    return BehaviourStatus.Running;
                if (status == BehaviourStatus.Failure)
                {
                    onInitialise();
                    return BehaviourStatus.Failure;
                }
                _current++;
            }
            onInitialise();
            return BehaviourStatus.Success;
        }
    }
}
