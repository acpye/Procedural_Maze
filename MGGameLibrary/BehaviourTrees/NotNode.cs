using Microsoft.Xna.Framework;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace MGGameLibrary.BehaviourTrees
{
    public class NotNode : BehaviourNode
    {
        private BehaviourNode _child;

        public NotNode(BehaviourNode child)
        {
            _child = child;
        }

        public override BehaviourStatus update(GameTime gameTime)
        {
            if (_child == null)
            {
                return BehaviourStatus.Failure;
            }

            BehaviourStatus status = _child.Tick(gameTime);
            switch (status)
            {
                case BehaviourStatus.Success:
                    return BehaviourStatus.Failure;
                case BehaviourStatus.Failure:
                    return BehaviourStatus.Success;
                case BehaviourStatus.Running:
                default:
                    return BehaviourStatus.Running;
            }
        }
    }
}
