using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace Game
{
    public class Character : Entity
    {
        public enum Gender {
            MALE,
            FEMALE
        }

        public LinkedList<Equipment> equipment { get; set; }
        public Gender gender { get; private set; }
        public int equipmentCapacity { get; private set; }
        public int partyIndex { get; set; }
        
        public Character(Attributes attributes, string name, Gender gender, Sprite sprite, int inventorySlots) : base(attributes, name, sprite)
        {
            equipment = new LinkedList<Equipment>();
            this.gender = gender;
            this.equipmentCapacity = inventorySlots;
        }

        public Attributes GetAttributes()
        {
            (Attributes charAttr, Attributes equipAttr) = GetSplitAttributes();
            equipAttr.AddAttributes(charAttr);
            return equipAttr;
        }

        public (Attributes, Attributes) GetSplitAttributes()
        {
            Attributes equipmentAttributes = new Attributes(0, 0, 0, 0, 0);
            foreach (Equipment e in equipment)
            {
                equipmentAttributes.AddAttributes(e.attributes);
            }
            return (this.attributes, equipmentAttributes);
        }
    }
}
