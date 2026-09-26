using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace DataParsing
{
    [System.Serializable]
    public class EquipmentData : JsonData<EquipmentData>
    {
        public string name;
        public string equipment_type;
        public string[] primary_attributes;
        public string sprite_path; // TODO: Remove this
        public Sprite sprite;
    }
}
