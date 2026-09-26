using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace Game
{
    public abstract class Entity
    {
        public Attributes attributes { get; private set; }
        public string name { get; private set; }
        public Sprite sprite { get; private set; }

        public Entity(Attributes attributes, string name, Sprite sprite)
        {
            this.attributes = attributes;
            this.name = name;
            this.sprite = sprite;
        }
    }
}
