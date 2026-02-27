using Microsoft.Xna.Framework;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace MGGameLibrary.BehaviourTrees
{
    public abstract class CompositeBehaviourNode : BehaviourNode
    {
        protected List<BehaviourNode> _children = new List<BehaviourNode>();

        public void addChild(BehaviourNode node)
        {
            _children.Add(node);
        }

        public void removeChild(BehaviourNode node)
        {
            _children.Remove(node);
        }

        public void clearChildren()
        {
            _children.Clear();
        }

        public override BehaviourStatus update(GameTime gameTime)
        {
            return BehaviourStatus.Success;
        }
    }
}
