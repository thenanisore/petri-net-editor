using System.Collections.Generic;
using PetriNetLib.NetStructure;

namespace PetriNetLib.Algorithms
{
    /// <summary>
    /// Handles finding algorithm to find handles of
    /// the certain curcuit. Based on some DFS.
    /// A handle of an elementary circuit
    /// is a path which intersects this circuit with its first and
    /// last node.
    /// </summary>
    public static class HandlesFindingAlgorithm
    {
        private static List<List<Node>> _handles;
        private static List<Node> _circuit;
        private static bool _badHandles;

        /// <summary>
        /// Returns all handles of a certain circuit.
        /// </summary>
        /// <param name="badHandles">If true, find only PT-TP handles.</param>
        public static List<List<Node>> FindHandles(List<Node> circuit, bool badHandles=false)
        {
            _handles = new List<List<Node>>();
            _circuit = circuit;
            _badHandles = badHandles;

            foreach (var node in _circuit)
                Dfs(node, node, new List<Node>());

            return _handles;
        }

        /// <summary>
        /// Builds a path by deep-first searching.
        /// </summary>
        /// <param name="thisNode">Current node.</param>
        /// <param name="startNode">Start node of the path.</param>
        /// <param name="path">Path, that is building.</param>
        private static void Dfs(Node thisNode, Node startNode, List<Node> path)
        {
            path.Add(thisNode);
            var successors = KosarajuAlgorithm.GetSuccessors(thisNode);
            foreach (var successor in successors)
            {
                if (thisNode != startNode && _circuit.Contains(successor)) {
                    if (_badHandles && ((startNode is Place && successor is Place)
                            || (startNode is Transition && successor is Transition)))
                        // If only bad handles (PT-TP) are being searched, than
                        // good ones are skipped.
                        continue;
                    path.Add(successor);
                    _handles.Add(new List<Node>(path));
                    path.RemoveAt(path.Count - 1);
                } else if (!path.Contains(successor) && !_circuit.Contains(successor))
                    Dfs(successor, startNode, new List<Node>(path));
            }
        }
    }
}
