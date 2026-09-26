using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using Game;
using Manager;

public class DebugShop : MonoBehaviour
{
    private void Start() {
        GameManager gm = FindObjectOfType<GameManager>();
        GameState gs = gm.state;
        
        Shop shop = gs.shop;
        foreach(ShopItem item in shop.items) {
            Debug.Log(item.item.name + " " + item.cost);
        }
    }
}
