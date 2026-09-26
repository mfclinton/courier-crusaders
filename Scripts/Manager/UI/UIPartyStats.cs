using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using TMPro;
using Game;

namespace Manager
{
    public class UIPartyStats : MonoBehaviour
    {
        [SerializeField] private TextMeshProUGUI deliveryTimeText;

        [Header("Attribute Fields")]
        [SerializeField] private TextMeshProUGUI strengthText;
        [SerializeField] private TextMeshProUGUI constitutionText;
        [SerializeField] private TextMeshProUGUI dexterityText;
        [SerializeField] private TextMeshProUGUI intelligenceText;
        [SerializeField] private TextMeshProUGUI charismaText;
        [SerializeField] private TextMeshProUGUI speedText;

        public void SetAttributes(LinkedList<Character> party)
        {
            Attributes totalAttr = new Attributes(0,0,0,0,0);
            foreach(Character c in party)
                totalAttr.AddAttributes(c.GetAttributes());

            strengthText.text = $"Strength: {totalAttr.strength}";
            constitutionText.text = $"Constitution: {totalAttr.constitution}";
            dexterityText.text = $"Dexterity: {totalAttr.dexterity}";
            intelligenceText.text = $"Intelligence: {totalAttr.intelligence}";
            charismaText.text = $"Charisma: {totalAttr.charisma}";

            // Set Speed Value
            Delivery delivery = GameState.instance.availableDelivery;
            List<Node> nodePath = MapManager.instance.pp.Path;
            DeliveryQuest tempQuest = new DeliveryQuest(delivery, party, nodePath);
            speedText.text = $"Speed: {tempQuest.CalculateSpeed().ToString("F2")}";
        }

        public void SetDeliveryTime(Delivery delivery, LinkedList<Character> party, List<Node> nodePath)
        {
            int days = DeliveryQuest.EstimateDeliveryDays(delivery, party, nodePath);

            deliveryTimeText.text = $"Delivery Time: {days} day";
            if(days > 1)
                deliveryTimeText.text += "s";
        }
    }
}
