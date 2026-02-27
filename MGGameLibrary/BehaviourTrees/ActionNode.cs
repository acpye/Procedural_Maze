using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Microsoft.Xna.Framework;

namespace MGGameLibrary.BehaviourTrees
{
    public class ActionNode : BehaviourNode
    {
        public delegate BehaviourStatus ActionCallback(GameTime gameTime);

        private ActionCallback _action;

        public ActionNode(ActionCallback action)
        {
            _action = action;
        }

        public override BehaviourStatus update(GameTime gameTime)
        {
            return _action(gameTime);
        }
    }
}