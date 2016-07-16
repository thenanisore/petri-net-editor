using System.Collections.Generic;
using System.Linq;
using PetriNetLib.NetStructure;

namespace PetriNetLib.Algorithms
{
    /// <summary>
    /// Kosaraju's algorithm is a linear time algorithm to find the
    /// strongly connected components of a directed graph.
    /// Published by S. Rao Kosaraju in 1978.
    /// </summary>
    public static class KosarajuAlgorithm
    {
        /// <summary>
        /// Nodes of the graph. True if used; False otherwise.
        /// </summary>
        private static Dictionary<Node, bool> _nodes;

        /// <summary>
        /// _order - Nodes in the specific order for the second DF-search.
        /// _component - Current strongly connected component.
        /// </summary>
        private static List<Node> _order, _component;

        /// <summary>
        /// Returns a list of the strongly connected components of a net.
        /// </summary>
        public static List<List<Node>> GetStronglyConnectedComponents(Net net)
        {
            // Get the node dictionary. True if the node is visited.
            _nodes = net.GetNodeList.ToDictionary(n => n, n => false);

            // Run DFS and get the order vector.
            _order = new List<Node>();
            foreach (var node in _nodes.Keys.ToList().Where(node => !_nodes[node]))
                Dfs(node);

            _order.Reverse();
            foreach (var node in _nodes.Keys.ToList()) _nodes[node] = false;

            // Second DFS-run through the reversed net in the reversed order. 
            // Build the components this run.
            var components = new List<List<Node>>();
            foreach (var node in _order.Where(node => !_nodes[node]))
            {
                _component = new List<Node>();
                TransposedDfs(node);
                components.Add(_component);
            }

            return components;
        }

        /// <summary>
        /// Recursive depth-first search, starting from the given node.
        /// </summary>
        /// <param name="node">Starting node.</param>
        private static void Dfs(Node node)
        {
            _nodes[node] = true;

            // Run DFS to all successors of the node, if is not visited.
            foreach (var neighbour in GetSuccessors(node).Where(neighbour => !_nodes[neighbour]))
                Dfs(neighbour);

            _order.Add(node);
        }

        /// <summary>
        /// Recursive depth-first search, starting from the given node and
        /// searching in the opposite direction (i.e. by using in-arcs).
        /// </summary>
        /// <param name="node"></param>
        private static void TransposedDfs(Node node)
        {
            _nodes[node] = true;
            _component.Add(node);

            // Run DFS to all predecessors of the node, if is not visited.
            foreach (var neighbour in GetPredecessors(node).Where(neighbour => !_nodes[neighbour]))
                TransposedDfs(neighbour);
        }

        /// <summary>
        /// Returns the list of all successors of the node.
        /// </summary>
        /// <param name="node">Current node.</param>
        /// <returns>List of successors.</returns>
        public static List<Node> GetSuccessors(Node node)
        {
            var successors = new List<Node>();
            if (node is Place)
                successors = (node as Place).OutArcs.Select(a => a.Target).ToList();
            else if (node is Transition)
                successors = (node as Transition).OutArcs.Select(a => a.Target).ToList();
            return successors;
        }

        /// <summary>
        /// Returns the list of all predecessors of the node.
        /// </summary>
        /// <param name="node">Current node.</param>
        /// <returns>List of predecessors.</returns>
        public static List<Node> GetPredecessors(Node node)
        {
            var predecessors = new List<Node>();
            if (node is Place)
                predecessors = (node as Place).InArcs.Select(a => a.Source).ToList();
            else if (node is Transition)
                predecessors = (node as Transition).InArcs.Select(a => a.Source).ToList();
            return predecessors;
        }
    }
}
