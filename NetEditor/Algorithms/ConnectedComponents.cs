using System.Collections.Generic;
using System.Linq;
using NetEditor.ViewModels;

namespace NetEditor.Algorithms
{
    /// <summary>
    /// A simple algorithm to find weakly-connected components.
    /// </summary>
    public static class ConnectedComponents
    {
        private static NetViewModel _net;
        private static Dictionary<NodeViewModel, int> _componentsDictionary;
        private static int _componentsNum;

        /// <summary>
        /// Returns a list of the weakly-connected components of the net.
        /// </summary>
        public static List<List<NodeViewModel>> GetConnectedComponents(NetViewModel net)
        {
            _net = net;
            var nodes = net.Nodes;
            _componentsDictionary = nodes.ToDictionary(k => k, k => -1);    // -1 if the node is not used yet.
            _componentsNum = 0;

            foreach (var node in nodes)
            {
                MarkNode(node);
                _componentsNum++;
            }

            var groupedNodes = _componentsDictionary.GroupBy(x => x.Value).OrderBy(grouping => grouping.Key);
            return groupedNodes.Select(grouping => grouping.Select(pair => pair.Key).ToList()).ToList();
        }

        /// <summary>
        /// Revursively marks all the node and builds the components.
        /// </summary>
        /// <param name="node">Node to mark.</param>
        private static void MarkNode(NodeViewModel node)
        {
            if (_componentsDictionary[node] != -1) return;
            _componentsDictionary[node] = _componentsNum;
            var neigbours = _net.Nodes.Where(n => _net.AreNodesConnected(node, n));
            foreach (var neighbour in neigbours)
            {
                MarkNode(neighbour);
            }
        }
    }
}
