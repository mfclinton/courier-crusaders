using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace DataParsing
{
    // NodeData class
    [System.Serializable]
    public class NodeData
    {
        public enum TownLevel
        {
            SETTLEMENT,
            TOWN,
            CITY
        }

        public string townName;
        public Vector2Int pixelPosition;
        public TownLevel level;
    }
}
