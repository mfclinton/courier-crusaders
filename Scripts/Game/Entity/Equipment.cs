using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace Game
{
    public class Equipment : Entity
    {
        public enum Rarity
        {
            COMMON,
            RARE,
            EPIC,
            LEGENDARY
        }

        public enum Type
        {
            WEAPON,
            ARMOR,
            ACCESSORY
        }

        public Type type { get; private set; }
        public Rarity rarity { get; private set; }

        public Equipment(Attributes attributes, string name, Type type, Rarity rarity, Sprite sprite) : base(attributes, name, sprite)
        {
            this.type = type;
            this.rarity = rarity;
        }
    }
}
