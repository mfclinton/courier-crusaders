using System;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.EventSystems;
using UnityEngine.UI;
using DataParsing;
using Game;

namespace Manager
{
    public class MapManager : MonoBehaviour
    {

        [SerializeField] private List<NodeData> nodesData;
        [SerializeField] private List<EdgeData> edgesData;
        [SerializeField] private Image mapImage;
        [SerializeField] private Transform[] mapLayers;
        
        [SerializeField] private TownNodeUIIcon nodePrefab;
        [SerializeField] private Image edgePrefab;


        // Color Data
        [Header("Colors")]
        [SerializeField] private Color defaultColor;
        [SerializeField] private Color startColor;
        [SerializeField] private Color endColor;
        [SerializeField] private Color pathColor;
        [SerializeField] private Color neighborColor;
        [SerializeField] private Color completedColor;
        
        [Header("UI Edge")]
        public float edgeUIScale = 500f;
        public float edgeWidth = 10f;
        public float edgeLength = 10f;
        public float edgeSpacing = 10f;

        public Dictionary<string, Node> nodes { get; private set; }
        public List<Edge> edges { get; private set; }

        public PathPlanner pp { get; private set; }

        public static MapManager instance { get; private set; }

        public enum MapLayer
        {
            PLAYER,
            EVENT,
            NODE,
            EDGE
        }

        Canvas canvas;

        private void Awake()
        {
            instance = this;
            nodes = new Dictionary<string, Node>();
            edges = new List<Edge>();
            pp = new PathPlanner();
            canvas = FindObjectOfType<Canvas>();

            CreateNodes();
            CreateEdges();
        }
        

        public void InitializePathPlanner(Node start, Node end)
        {
            pp.OnPathUpdated += ColorIcons;
            pp.OnPathUpdated += ColorWorldEvents;
            pp.OnPathUpdated += UIManager.instance.PathUpdated;
            pp.Initialize(start, end);
        }

        public void InitializeNodeIcon(NodeData nodeData, Node node)
        {
            TownNodeUIIcon nodeInstance = CreateIcon(nodePrefab.gameObject, node.RelativePosition, MapLayer.NODE, true).GetComponent<TownNodeUIIcon>();
            nodeInstance.Initialize(nodeData.level, nodeData.townName);
            SetIconClickEvent(nodeInstance.gameObject, () => pp.ProcessSelectedNode(node));
            node.SetIcon(nodeInstance.bgImage);
        }

        private void CreateNodes()
        {
            foreach (NodeData nodeData in nodesData)
            {
                Node node = new Node(nodeData.townName, GetRelativePosition(nodeData.pixelPosition));
                InitializeNodeIcon(nodeData, node);
                nodes.Add(nodeData.townName, node);
            }
        }

        // TODO
        private Image CreateEdgeUI(Node nodeA, Node nodeB, float t)
        {
            // Instantiate the edge prefab
            Vector2 halfWayPos = Vector2.Lerp(nodeA.RelativePosition, nodeB.RelativePosition, t);
            Image edgeInstance = CreateIcon(edgePrefab.gameObject, halfWayPos, MapLayer.EDGE, true).GetComponent<Image>();
            RectTransform edgeRectTransform = edgeInstance.GetComponent<RectTransform>();

            // Set the edge's rotation
            Vector2 direction = (nodeB.RelativePosition - nodeA.RelativePosition).normalized;
            float angle = Mathf.Atan2(direction.y, direction.x) * Mathf.Rad2Deg - 90f;  // subtract 90 to make the element's top point towards B
            edgeRectTransform.rotation = Quaternion.Euler(0, 0, angle);

            // Set the edges size
            edgeRectTransform.sizeDelta = new Vector2(edgeWidth, edgeLength) * edgeUIScale;

            return edgeInstance;
        }

        private UIEdgeCollection CreateTownEdgeUI(Node nodeA, Node nodeB)
        {
            float distance = Vector2.Distance(nodeA.RelativePosition, nodeB.RelativePosition);
            float edgeSpace = edgeLength + edgeSpacing;
            int numEdges = Mathf.FloorToInt(distance / edgeSpace);
            numEdges = Mathf.Max(numEdges, 1);

            float t = 1f / (numEdges + 1);
            float tOffset = t / 2f;

            Image[] edgeInstances = new Image[numEdges];
            for (int i = 0; i < numEdges; i++)
            {
                float tVal = t * (i + 1) + tOffset;
                edgeInstances[i] = CreateEdgeUI(nodeA, nodeB, tVal);
            }

            return new UIEdgeCollection(edgeInstances);
        }

        private void CreateEdges()
        {
            float globalDistanceScalar = 10f;
            foreach (EdgeData edgeData in edgesData)
            {
                Node nodeA = nodes[edgeData.townNameA];
                Node nodeB = nodes[edgeData.townNameB];
                UIEdgeCollection uIEdgeCollection = CreateTownEdgeUI(nodeA, nodeB);
                Action<Color> setColor = (color) => uIEdgeCollection.SetColor(color);

                Edge edge = new Edge(nodeA, nodeB, setColor, edgeData.distanceScalar * globalDistanceScalar);
                // Debug.Log("Edge: " + edgeData.townNameA + " -> " + edgeData.townNameB + " : " + edge.Distance);
                edges.Add(edge);

                // Add the edge to the list of edges for both nodes
                nodeA.Edges.Add(edge);
                nodeB.Edges.Add(edge);
            }
        }

        private Vector2 GetRelativePosition(Vector2Int pixelPosition)
        {
            float relativeX = (float)pixelPosition.x / mapImage.sprite.texture.width;
            float relativeY = 1 - ((float)pixelPosition.y / mapImage.sprite.texture.height);
            return new Vector2(relativeX, relativeY);
        }

        public GameObject CreateIcon(GameObject prefab, Vector2 relativePosition, MapLayer layer, bool staticIcon = false)
        {
            GameObject iconInstance;

            if(staticIcon)
                iconInstance = Instantiate(prefab, mapLayers[(int) layer]);
            else
                iconInstance = ObjectPooler.Instance.SpawnFromPool(prefab, parent: mapLayers[(int) layer]);
    
            RectTransform iconRectTransform = iconInstance.GetComponent<RectTransform>();
            iconRectTransform.anchorMin = relativePosition;
            iconRectTransform.anchorMax = relativePosition;
            iconRectTransform.anchoredPosition = Vector2.zero;

            return iconInstance;
        }

        public void ClearMapLayer(MapLayer layer)
        {
            foreach (Transform child in mapLayers[(int) layer])
            {
                ObjectPooler.Instance.ReturnToPool(child.gameObject);
            }
        }

        private void SetIconClickEvent(GameObject iconInstance, Action onIconClick)
        {
            // Add event trigger component to handle clicks
            EventTrigger eventTrigger = iconInstance.AddComponent<EventTrigger>();
            EventTrigger.Entry entry = new EventTrigger.Entry();
            entry.eventID = EventTriggerType.PointerClick;
            entry.callback.AddListener((eventData) => onIconClick());
            eventTrigger.triggers.Add(entry);
        }

        void ColorIcons(List<Node> path) 
        {
            bool pathComplete = pp.PathComplete();

            // Set all edges to defaultColor
            foreach(Edge edge in edges)
            {
                edge.SetColor(defaultColor);
            }

            // Colors the Node Icons
            Node lastNode = path[path.Count - 1];
            foreach(Node node in nodes.Values)
            {
                node.Icon.color = defaultColor;
                
                if(PathPlanner.IsNeighbor(lastNode, node) && !path.Contains(node) && !pathComplete)
                {
                    node.Icon.color = neighborColor;
                    // Color the edge between these nodes
                    Edge edge = PathPlanner.FindEdge(lastNode, node);
                    edge.SetColor(neighborColor);
                }
            }

            Node prevNode = null;
            foreach (Node node in path) {
                if(pathComplete)
                    node.Icon.color = completedColor;
                else
                    node.Icon.color = pathColor;

                // Color the Edge
                if(prevNode != null)
                {
                    // Color the edge between these nodes
                    Edge edge = PathPlanner.FindEdge(prevNode, node);

                    if(pathComplete)
                        edge.SetColor(completedColor);
                    else
                        edge.SetColor(pathColor);
                }
                prevNode = node;
            }

            if(!pathComplete)
            {
                pp.HomeNode.Icon.color = startColor;
                pp.DestinationNode.Icon.color = endColor;
            }
        }

        public void ColorWorldEvents(List<Node> nodes)
        {
            int[] eventTypeCounts = new int[5];

            WorldStateManager wsm = WorldStateManager.instance;
            foreach (WorldEvent we in wsm.activeWorldEvents)
            {
                we.SetHighlighted(false);
            }

            List<Edge> edges = PathPlanner.ConvertNodesToEdges(nodes);
            foreach (Edge edge in edges)
            {
                if(!wsm.edgeToEvents.ContainsKey(edge))
                    continue;

                foreach (WorldEvent we in wsm.edgeToEvents[edge])
                {
                    we.SetHighlighted(true);
                    eventTypeCounts[(int)we.Type]++;
                }
            }

            // Update the UI
            UIManager.instance.UpdateWorldEventCounts(eventTypeCounts);
        }

    }
}