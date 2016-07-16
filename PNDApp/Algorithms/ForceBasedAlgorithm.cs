using System;
using System.Linq;
using System.Threading;
using System.Windows;
using PNDApp.ViewModels;

namespace PNDApp.Algorithms
{
    /// <summary>
    /// Force-directed graph drawing algorithm uses some physical
    /// simulation to arrange nodes that are trying to minimize the
    /// energy of the system.
    /// </summary>
    public static class ForceBasedAlgorithm
    {
        private const double AttractionConstant = 0.1;		// Spring constant.
        private const double RepulsionConstant = 50000;	    // Charge constant.

        private const int DefaultSpringLength = 200;
        private const int DefaultMaxIterations = 1000;

        /// <summary>
        /// Arranges nodes of the graph using FBA.
        /// </summary>
        /// <param name="net">Net to arrange.</param>
        /// <param name="maxWidth">Width of the canvas.</param>
        /// <param name="maxHeight">Height of the canvas.</param>
        /// <param name="gridNodes">Is true, grid nodes after arranging.</param>
        /// <param name="springLength">Desired spring length.</param>
        /// <param name="maxIterations">Maximum number of iteration.</param>
        /// <param name="animate">Delay for some ms after each iteration for animation effect.</param>
        public static void Arrange(NetViewModel net,
            double maxWidth, double maxHeight, bool gridNodes,
            double springLength=DefaultSpringLength, 
            int maxIterations=DefaultMaxIterations,
            bool animate=false)
        {
            // Make a list containing the layout data for each node.
            net.RandomizeLayout(0, 0, maxWidth, maxHeight);
            net.UngridNodes();
            var layout = net.Nodes.Select(node => new NodeLayoutInfo(node, new Point())).ToList();
            var stopCount = 0;
            var iterations = 0;

            // Get connected component. Set displacement for each component to zero.
            var components = ConnectedComponents.GetConnectedComponents(net).ToDictionary(c => c, c => 0.0);
            springLength = maxWidth / (4 * Math.Sqrt(layout.Count) * components.Count);

            while (true)
            {
                double totalDisplacement = 0;
                foreach (var component in components.Keys.ToList())
                    components[component] = 0.0;

                for (var i = 0; i < layout.Count; i++) {
                    var current = layout[i];
                    var currentPosition = new Vector(new Point(current.Node.Center.X, current.Node.Center.Y));
                    var netForce = new Vector(new Point());
                    var componentNetForce = new Vector(new Point());
                    var connectedNodes = net.Nodes.Where(node => net.AreNodesConnected(current.Node, node)).ToList();
                    var thisComponent = components.First(p => p.Key.Contains(current.Node)).Key;

                    // Determine repulsion between nodes.
                    foreach (var node in net.Nodes)
                        if (node != current.Node)
                        {
                            var rf = CalcRepulsionForce(current.Node, node, connectedNodes.Count);
                            if (thisComponent.Contains(current.Node) && thisComponent.Contains(node))
                                componentNetForce += rf;
                            else
                                rf *= 0.7;
                            
                            netForce += rf;
                        }

                    // Determine attraction.
                    foreach (var node in connectedNodes)
                    {
                        var af = CalcAttractionForce(current.Node, node, springLength) * Math.Sqrt(connectedNodes.Count);
                        netForce += af;
                        componentNetForce += af;
                    }

                    // Add to resultant position.
                    current.NextPosition = (currentPosition + netForce).toPoint();
                    CheckBorders(ref current.NextPosition, maxWidth, maxHeight, NodeViewModel.DefaultSize);

                    // Add to component's displacement.
                    components[thisComponent] += componentNetForce.Lenght;
                }

                // Move nodes to resultant positions (and calculate total displacement)
                foreach (var currentNode in layout)
                {
                    var currentPosition = new Point(currentNode.Node.Center.X, 
                                                    currentNode.Node.Center.Y);
                    var thisDisplacement = GetDistance(currentPosition, currentNode.NextPosition);
                    totalDisplacement += thisDisplacement;
                    currentNode.Node.SetCenter(currentNode.NextPosition);
                }

                iterations++;
                if (totalDisplacement < layout.Count * 4 || components.Count(c => c.Value > 10) == 0) stopCount++;
                if (stopCount > 15) break;
                if (iterations > maxIterations) break;

                // Delay after each iteration a little to animate the process.
                if (animate) Thread.Sleep(30);
            }

            // Shift the diagram to the center of the canvas.
            net.CentralizeLayout(maxWidth, maxHeight);
            if (gridNodes) net.GridNodes();
        }

        /// <summary>
        /// Checks if the node is inside the visible area.
        /// </summary>
        /// <param name="position">Position of the node.</param>
        /// <param name="maxWidth">Width of the canvas.</param>
        /// <param name="maxHeight">Height of the canvas.</param>
        /// <param name="size">Size of the node.</param>
        private static void CheckBorders(ref Point position, double maxWidth, double maxHeight, double size)
        {
            if (position.X < size/2.0) position = new Point(size/2.0, position.Y);
            if (position.X > maxWidth - size/2.0) position = new Point(maxWidth - size/2.0, position.Y);
            if (position.Y < size/2.0) position = new Point(position.X, size/2.0);
            if (position.Y > maxHeight - size/2.0) position = new Point(position.X, maxHeight - size/2.0);
        }

        /// <summary>
        /// Calculates the attraction force between two connected nodes, 
        /// using the specified spring length.
        /// </summary>
        /// <param name="attractingNode">The node that the force is acting on.</param>
        /// <param name="node">The node creating the force.</param>
        /// /// <param name="springLength">The length of the spring.</param>
        /// <returns>A Vector representing the attraction force.</returns>
        private static Vector CalcAttractionForce(NodeViewModel attractingNode,
            NodeViewModel node, double springLength)
        {
            var attractingNodeLocation = new Point(attractingNode.Center.X, attractingNode.Center.Y);
            var nodeLocation = new Point(node.Center.X, node.Center.Y);

            var proximity = Math.Max((int)GetDistance(attractingNodeLocation, nodeLocation), 1);

            // Hooke's Law: F = -kx.
            var force = AttractionConstant * Math.Max(proximity - springLength, 0);
            var angle = GetAngle(attractingNodeLocation, nodeLocation);

            return new Vector(force, angle);
        }

        /// <summary>
        /// Calculates the repulsion force between two connected nodes.
        /// </summary>
        /// <param name="repulsingNode">The node that the force is acting on.</param>
        /// <param name="node">The node creating the force.</param>
        /// <param name="connectedNum">How many nodes conneced to the repulsing node. Used for optimisation.</param>
        /// <returns>A Vector representing the repulsion force.</returns>
        private static Vector CalcRepulsionForce(NodeViewModel repulsingNode,
            NodeViewModel node, int connectedNum)
        {
            var repulsingNodeLocation = new Point(repulsingNode.Center.X, repulsingNode.Center.Y);
            var nodeLocation = new Point(node.Center.X, node.Center.Y);

            var proximity = Math.Max((int)GetDistance(repulsingNodeLocation, nodeLocation), 1);

            // Coulomb's Law: F = k(Qq/r^2)
            var force = -(RepulsionConstant / Math.Pow(proximity, 2));
            force = (connectedNum != 0) ? force*5.0/connectedNum : force/3.0;

            var angle = GetAngle(repulsingNodeLocation, nodeLocation);
            return new Vector(force, angle);
        }

        /// <summary>
        /// Calculates the distance between two points.
        /// </summary>
        private static double GetDistance(Point p1, Point p2)
        {
            var xDiff = p2.X - p1.X;
            var yDiff = p2.Y - p1.Y;
            return Math.Sqrt(xDiff*xDiff + yDiff*yDiff);
        }

        /// <summary>
        /// Calculates the angle between two points (x-axis is 0 degrees, clockwise).
        /// </summary>
        private static double GetAngle(Point p1, Point p2)
        {
            var xDiff = p2.X - p1.X;
            var yDiff = p2.Y - p1.Y;
            return Math.Atan2(yDiff, xDiff) * 180.0 / Math.PI;
        }

        /// <summary>
        /// Private inner class used to track the node's position and velocity during arrangement.
        /// </summary>
        private class NodeLayoutInfo
        {

            public readonly NodeViewModel Node;			// Reference to the node.
            public Point NextPosition;	        // The node's position after the next iteration.

            /// <summary>
            /// Initialises a new instance of the Diagram.NodeLayoutInfo class.
            /// </summary>
            public NodeLayoutInfo(NodeViewModel node, Point nextPosition)
            {
                Node = node;
                NextPosition = nextPosition;
            }
        }
    }
}
