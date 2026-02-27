using Microsoft.Xna.Framework;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace MGGameLibrary.BehaviourTrees
{
    public class BehaviourTree
    {
        protected BehaviourNode Root { get; set; }

        public BehaviourTree(BehaviourNode root)
        {
            Root = root;
        }

        public void Tick(GameTime gameTime)
        {
            Root.Tick(gameTime);
        }
    }
}