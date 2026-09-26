using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace Game
{
    public class ShopItem
    {
        public Entity item { get; private set; }
        public int cost { get; private set; }

        public ShopItem(Entity item, int cost)
        {
            this.item = item;
            this.cost = cost;
        }

        public void Purchase()
        {
            Shop.instance.Purchase(this);
        }
    }
}
