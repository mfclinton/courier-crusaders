using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

namespace Game
{
    public class Node
    {
        public string TownName { get; private set; }
        public Vector2 RelativePosition { get; private set; }
        public List<Edge> Edges { get; private set; }
        public Image Icon { get; private set; }

        public Node(string townName, Vector2 relativePosition)
        {
            TownName = townName;
            RelativePosition = relativePosition;
            Edges = new List<Edge>();
        }

        public void SetIcon(Image icon)
        {
            this.Icon = icon;
        }
    }
}
