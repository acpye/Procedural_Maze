using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace MGGameLibrary.BehaviourTrees
{
    public class BehaviourTreeBuilder
    {
        private readonly Stack<CompositeBehaviourNode> _compositeStack = new Stack<CompositeBehaviourNode>();
        private BehaviourNode _currentNode;

        public BehaviourTreeBuilder Selector()
        {
            SelectorNode selector = new SelectorNode();
            if (_compositeStack.Count > 0)
            {
                _compositeStack.Peek().addChild(selector);
            }
            _compositeStack.Push(selector);
            return this;
        }

        public BehaviourTreeBuilder Sequence()
        {
            SequenceNode sequence = new SequenceNode();
            if (_compositeStack.Count > 0)
            {
                _compositeStack.Peek().addChild(sequence);
            }
            _compositeStack.Push(sequence);
            return this;
        }

        public BehaviourTreeBuilder Action(ActionNode.ActionCallback action)
        {
            ActionNode actionNode = new ActionNode(action);
            if (_compositeStack.Count > 0)
            {
                _compositeStack.Peek().addChild(actionNode);
            }
            else
            {
                _currentNode = actionNode;
            }
            return this;
        }

        public BehaviourTreeBuilder Not(ActionNode.ActionCallback action)
        {
            return Not(new ActionNode(action));
        }

        public BehaviourTreeBuilder Not(BehaviourNode node)
        {
            NotNode notNode = new NotNode(node);
            if (_compositeStack.Count > 0)
            {
                _compositeStack.Peek().addChild(notNode);
            }
            else
            {
                _currentNode = notNode;
            }
            return this;
        }

        public BehaviourTreeBuilder Timed(float duration, ActionNode.ActionCallback action)
        {
            return Timed(duration, new ActionNode(action));
        }

        public BehaviourTreeBuilder Timed(float duration, BehaviourNode child)
        {
            TimedDecoratorNode timedNode = new TimedDecoratorNode(child, duration);
            if (_compositeStack.Count > 0)
            {
                _compositeStack.Peek().addChild(timedNode);
            }
            else
            {
                _currentNode = timedNode;
            }
            return this;
        }

        public BehaviourTreeBuilder End()
        {
            CompositeBehaviourNode finished = _compositeStack.Pop();
            if (_compositeStack.Count == 0)
            {
                _currentNode = finished;
            }
            return this;
        }

        public BehaviourTree Build()
        {
            return new BehaviourTree(_currentNode);
        }
    }
}
