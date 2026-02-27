using Microsoft.Xna.Framework;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace MGGameLibrary.BehaviourTrees
{
    public abstract class BehaviourNode
    {
        protected BehaviourStatus _status;

        public virtual void onInitialise() { }

        public abstract BehaviourStatus update(GameTime gameTime);

        public virtual void onTerminate(BehaviourStatus status) { }

        public virtual BehaviourStatus Tick(GameTime gameTime)
        {
            if (_status != BehaviourStatus.Running)
            {
                onInitialise();
            }
            _status = update(gameTime);
            if (_status != BehaviourStatus.Running)
            {
                onTerminate(_status);
            }
            return _status;
        }
    }
}