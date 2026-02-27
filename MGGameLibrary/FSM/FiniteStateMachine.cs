using MGGameLibrary.DataStructures;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace MGGameLibrary.FSM
{
    public class FiniteStateMachine<TNode, TEdge>
            where TNode : IState
            where TEdge : IStateTransition
    {
        private SparseGraph<TNode, TEdge> _fsm = new();

        public TNode CurrentState { get; private set; }

        public FiniteStateMachine(SparseGraph<TNode, TEdge> fsm, TNode currentState)
        {
            _fsm = fsm;
            CurrentState = currentState;
        }

        public void Update(float seconds)
        {
            List<(TEdge edge, TNode node)> edges = _fsm.GetEdges(CurrentState);
            
            foreach ((TEdge, TNode) edge in edges)
            {
                if (edge.Item1.ToTransition())
                {
                    CurrentState.OnExit();
                    CurrentState = edge.Item2;
                    CurrentState.OnEnter();

                }
            }

            if (CurrentState != null)
            {
                CurrentState.OnUpdate(seconds);
            }
        }
    }
}