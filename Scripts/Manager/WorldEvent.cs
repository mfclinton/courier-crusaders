using System.Collections.Generic;
using UnityEngine;
using Game;
using UnityEngine.UI;

namespace Manager {
    [System.Serializable]
    public class WorldEvent
    {
        public enum EventType
        {
            BATTLE,
            CRIME,
            MAGIC,
            DANGER,
            RANDOM
        }

        [SerializeField] public EventType Type;
        [SerializeField] public Vector2 RelativeLocation;
        public GameObject instantiatedPrefab { get; private set; }
        public Edge edge { get; private set; }
        public float t { get; private set;}

        Image img; 
        float initialAlpha;

        public WorldEvent(EventType type, Edge edge, GameObject iconPrefab)
        {
            Type = type;
            this.edge = edge;

            float minT = WorldStateManager.instance.uiEventEdgeMaxTownDist / edge.Distance;
            float maxT = 1 - minT;
            t = Random.Range(minT, maxT);
            
            RelativeLocation = Vector2.Lerp(edge.TownA.RelativePosition, edge.TownB.RelativePosition, t);
            InitializeIcon(iconPrefab);

            img = instantiatedPrefab.GetComponentInChildren<Image>();
            if(img != null)
                initialAlpha = img.color.a;
        }

        public void SetHighlighted(bool isHighlighed)
        {
            if(img != null)
            {
                Color c = img.color;
                c.a = isHighlighed ? 1 : initialAlpha;
                img.color = c;
            }
        }

        public void EndEvent()
        {
            ObjectPooler.Instance.ReturnToPool(instantiatedPrefab);
        }

        public void InitializeIcon(GameObject iconPrefab)
        {
            if (instantiatedPrefab == null)
                instantiatedPrefab = MapManager.instance.CreateIcon(iconPrefab, RelativeLocation, MapManager.MapLayer.EVENT);
        }

        public (bool, float) CalculateQuestT(DeliveryQuest dq)
        {
            bool isOnPath = false;
            float questDistance = 0;

            for (int i = 0; i < dq.edgePath.Count; i++)
            {
                Edge e = dq.edgePath[i];
                Node n = dq.nodePath[i];

                if(e == edge)
                {
                    if(n == e.TownA)
                        questDistance += e.Distance * t;
                    else
                        questDistance += e.Distance * (1 - t);
                    
                    isOnPath = true;
                    break;
                }
                else
                    questDistance += e.Distance;
            }

            return (isOnPath, questDistance / dq.totalDistance);
        }
    }
}