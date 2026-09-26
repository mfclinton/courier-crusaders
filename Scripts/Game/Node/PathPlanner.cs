using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace Game
{
    public class PathPlanner
    {
        public Node HomeNode { get; private set; }
        public Node DestinationNode { get; private set; }
        public List<Node> Path { get; private set; }

        // Delegate for when Path is updated
        public delegate void PathUpdatedDelegate(List<Node> Path);
        public PathUpdatedDelegate OnPathUpdated;

        public static PathPlanner instance { get; private set; }

        public PathPlanner()
        {
            instance = this;
        }

        public void Initialize(Node HomeNode, Node DestinationNode)
        {
            this.HomeNode = HomeNode;
            this.DestinationNode = DestinationNode;
            Path = new List<Node>() { HomeNode };
            OnPathUpdated?.Invoke(Path);
        }

        public void ProcessSelectedNode(Node selectedNode)
        {
            if(this.HomeNode == null || this.DestinationNode == null)
            {
                Debug.LogError("PathPlanner not initialized");
                return;
            }

            if(selectedNode == HomeNode)
                return;

            bool nodeAlreadyAdded = Path.Contains(selectedNode);
            Node lastNode = Path[Path.Count - 1];
            if(1 < Path.Count && lastNode == selectedNode)
            {
                Path.RemoveAt(Path.Count - 1);
                OnPathUpdated?.Invoke(Path);
                return;
            }
            else if(1 < Path.Count && nodeAlreadyAdded)
                return;
            else if(lastNode == DestinationNode)
                return;

            if (IsNeighbor(lastNode, selectedNode))
            {
                Path.Add(selectedNode);
                Debug.Log("Added " + selectedNode.TownName + " to path");
                OnPathUpdated?.Invoke(Path);
            }
        }

        public bool PathComplete()
        {
            return Path[0] == HomeNode && Path[Path.Count - 1] == DestinationNode;
        }

        public static bool IsNeighbor(Node nodeA, Node nodeB)
        {
            foreach (Edge edge in nodeA.Edges)
            {
                if (edge.TownA == nodeB || edge.TownB == nodeB)
                {
                    return true;
                }
            }
            return false;
        }

        public static Edge FindEdge(Node nodeA, Node nodeB)
        {
            foreach (Edge edge in nodeA.Edges)
            {
                if (edge.TownA == nodeB || edge.TownB == nodeB)
                {
                    return edge;
                }
            }
            return null;
        }

        public static List<Edge> ConvertNodesToEdges(List<Node> nodes)
        {
            List<Edge> edges = new List<Edge>();

            for (int i = 0; i < nodes.Count - 1; i++)
            {
                Node currentNode = nodes[i];
                Node nextNode = nodes[i + 1];

                Edge edge = FindEdge(currentNode, nextNode);

                if (edge != null)
                {
                    edges.Add(edge);
                }
                else
                {
                    Debug.LogError("No edge found between nodes: " + currentNode.TownName + " and " + nextNode.TownName);
                }
            }

            return edges;
        }

        // Calculate the minimum number of edges between 2 nodes
        public static int CalculateDistance(Node nodeA, Node nodeB)
        {
            List<Node> visited = new List<Node>();
            Queue<Node> queue = new Queue<Node>();
            Dictionary<Node, int> distance = new Dictionary<Node, int>();

            queue.Enqueue(nodeA);
            distance.Add(nodeA, 0);

            while (queue.Count > 0)
            {
                Node currentNode = queue.Dequeue();
                visited.Add(currentNode);

                if (currentNode == nodeB)
                {
                    return distance[currentNode];
                }

                foreach (Edge edge in currentNode.Edges)
                {
                    Node neighbor = edge.TownA == currentNode ? edge.TownB : edge.TownA;

                    if (!visited.Contains(neighbor))
                    {
                        queue.Enqueue(neighbor);

                        int newDistance = distance[currentNode] + 1;
                        if (distance.ContainsKey(neighbor))
                        {
                            distance[neighbor] = Mathf.Min(distance[neighbor], newDistance);
                        }
                        else
                        {
                            distance.Add(neighbor, newDistance);
                        }
                    }
                }
            }

            return -1;
        }
    }
}
