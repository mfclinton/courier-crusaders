using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

namespace Game
{
    public class Edge
    {
        public Node TownA { get; private set; }
        public Node TownB { get; private set; }
        public float Distance { get; private set; }
        public Image Icon { get; private set; }

        Action<Color> setColor;

        public Edge(Node townA, Node townB, Action<Color> setColor, float distanceScalar = 1f)
        {
            TownA = townA;
            TownB = townB;
            Distance = Vector2.Distance(TownA.RelativePosition, TownB.RelativePosition) * distanceScalar;
            this.setColor = setColor;
        }

        public void SetColor(Color color)
        {
            if(setColor != null)
                setColor(color);
        }
    }
}
