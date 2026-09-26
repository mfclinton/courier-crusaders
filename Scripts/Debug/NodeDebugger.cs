using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
using Game;
using Manager;

public class NodeDebugger : MonoBehaviour
{
    public Color c;
    void Start() {
        MapManager mm = MapManager.instance;
        var start = mm.nodes["start"];
        var end = mm.nodes["end"];

        mm.pp.OnPathUpdated += ColorIcons;
        mm.InitializePathPlanner(start, end);
    }

    void ColorIcons(List<Node> path) {
        Node lastNode = path[path.Count - 1];
        foreach(Node node in MapManager.instance.nodes.Values)
        {
            node.Icon.GetComponent<Image>().color = Color.white;

            if(PathPlanner.IsNeighbor(lastNode, node))
                node.Icon.GetComponent<Image>().color = Color.red;
        }

        foreach (Node node in path) {
            node.Icon.GetComponent<Image>().color = c;
        }
    }
}
