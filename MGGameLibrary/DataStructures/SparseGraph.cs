using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace MGGameLibrary.DataStructures
{
    public class SparseGraph<TNode, TEdge>
    {
        private Dictionary<TNode, List<(TEdge edge, TNode node)>> _adjacency = new();
        public IEnumerable<TNode> Nodes => _adjacency.Keys;

        public void AddNode(TNode node)
        {
            if (!_adjacency.ContainsKey(node))
            {
                _adjacency[node] = new List<(TEdge, TNode)>();
            }
        }

        public void AddEdge(TNode from, TEdge edge, TNode to)
        {
            AddNode(from);
            AddNode(to);
            _adjacency[from].Add((edge, to));
        }

        public List<(TEdge edge, TNode node)> GetEdges(TNode node)
        {
            if (_adjacency.ContainsKey(node))
            {
                return _adjacency[node];
            }
            return new List<(TEdge edge, TNode node)>();
        }
    }
}
