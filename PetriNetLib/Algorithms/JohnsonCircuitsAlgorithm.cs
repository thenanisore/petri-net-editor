using System.Collections.Generic;
using System.Linq;
using PetriNetLib.NetStructure;

namespace PetriNetLib.Algorithms
{
    /// <summary>
    /// Johnson's algorithm to find all elementary circuits 
    /// in a directed graph. 
    /// An elementary circuit is a closed pathwhere no node appears
    /// twice, except that the first and last node are the same.
    /// Two elementary circuits are distinct if they are not cyclic 
    /// permutations of each other.
    /// </summary>
    public static class JohnsonCircuitsAlgorithm
    {
        private static Dictionary<Node, bool> _blocked;   // True if node is blocked.
        private static Dictionary<Node, Stack<Node>> _b;  // Auxiliary list of pathes.
        private static Stack<Node> _currentPath;          
        private static List<Node> _currentComponent; 
        private static List<List<Node>> _circuits; 

        /// <summary>
        /// Returns a list of all elementary circuits of the net.
        /// </summary>
        public static List<List<Node>> FindElementaryCircuits(Net sourceNet)
        {
            // Clone to prevent modifications of the given net.
            var net = sourceNet.DeepClone();

            // Initialize.
            var nodeList = new List<Node>(net.GetNodeList);

            _currentPath = new Stack<Node>();
            _currentComponent = new List<Node>();
            _blocked = nodeList.ToDictionary(n => n, n => false);
            _b = nodeList.ToDictionary(n => n, n => new Stack<Node>());
            _circuits = new List<List<Node>>();
            

            // Find the elementary circuits.
            // Each iteration the least node of the net if removed. Then
            // the SCC that contains the least node of the new graph is found.
            while (nodeList.Count > 0)
            {
                var currentNode = nodeList[0];
                var scc = KosarajuAlgorithm.GetStronglyConnectedComponents(net);
                _currentComponent = scc.First(c => c.Contains(currentNode));
                
                if (_currentComponent.Count > 1)
                {
                    // Unblock and start the path from this node.
                    foreach (var node in _currentComponent)
                    {
                        _blocked[node] = false;
                        _b[node] = new Stack<Node>();
                    }
                    Circuit(currentNode, currentNode);
                }

                nodeList.Remove(currentNode);
                net.RemoveNode(currentNode);
            }

            // Replace copies with the original nodes.
            _circuits = _circuits.Select(circuit => circuit.Select(node => sourceNet
                                                           .FindNodeById(node.Id))
                                                           .ToList()).ToList();
            return _circuits;
        }


        /// <summary>
        /// Builds a path through the net.
        /// </summary>
        /// <param name="thisNode">Current node.</param>
        /// <param name="startNode">Start of the path.</param>
        /// <returns>True if a circuit is found, False otherwise.</returns>
        private static bool Circuit(Node thisNode, Node startNode)
        {
            var closed = false;     // True when the elementary circuit is closed.
            _currentPath.Push(thisNode);
            _blocked[thisNode] = true;
            var successors = KosarajuAlgorithm.GetSuccessors(thisNode)
                .Where(n => _currentComponent.Contains(n))
                .ToList();
            
            foreach (var nextNode in successors)
            {
                if (nextNode == startNode)
                {
                    // Curcuit found, close and push.
                    var circuit = _currentPath.ToList();
                    circuit.Reverse();
                    _circuits.Add(circuit);
                    closed = true;
                }
                else if (!_blocked[nextNode] && Circuit(nextNode, startNode))
                    closed = true;
            }

            if (closed)
                Unblock(thisNode);
            else
                // Push the node to all the auxiliry lists, 
                // if no curcuit was found.
                foreach (var nextNode in successors.Where(nextNode => !_b[nextNode].Contains(thisNode)))
                    _b[nextNode].Push(thisNode);

            _currentPath.Pop();
            return closed;
        }

        /// <summary>
        /// Recursively unblock and removes nodes from the auxilary path.
        /// </summary>
        private static void Unblock(Node node)
        {
            if (!_blocked[node]) return;
            _blocked[node] = false;
            while (_b[node].Count > 0)
                Unblock(_b[node].Pop());
        }
    }
}
