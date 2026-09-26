using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using MathNet.Numerics.Distributions;

namespace Game
{
    public class Shop
    {
        public LinkedList<ShopItem> items { get; private set; } // Might want to differentiate shop items

        Func<Character> characterGenerator;
        Func<Equipment> equipmentGenerator;
        public int numCharacters { get; private set; }
        public int numEquipment { get; private set; }

        // Static reference to the game state
        public static Shop instance { get; private set; }

        public Shop(GameState state, Func<Character> characterGenerator, Func<Equipment> equipmentGenerator, int numCharacters = 3, int numEquipment = 3)
        {
            // Set Statics
            instance = this;

            this.characterGenerator = characterGenerator;
            this.equipmentGenerator = equipmentGenerator;
            this.numCharacters = numCharacters;
            this.numEquipment = numEquipment;
            UpdateShop();
        }

        public void UpdateShop()
        {
            items = new LinkedList<ShopItem>();

            // Add Characters To Shop
            for (int i = 0; i < numCharacters; i++)
            {
                Character c = characterGenerator();
                int cost = SampleCost(c); // TODO


                ShopItem item = new ShopItem(c, cost);
                items.AddLast(item);
            }

            // Add Equipment To Shop
            for (int i = 0; i < numEquipment; i++)
            {
                Equipment e = equipmentGenerator();
                int cost = SampleCost(e); // TODO


                ShopItem item = new ShopItem(e, cost);
                items.AddLast(item);
            }
        }

        int SampleCost(Character character)
        {
            int flatCost = 20;
            int mu = 10;
            float sigma = 5f;
            int lowerBound = 3;
            int upperBound = 50;
            float skillCostMultiplier = 1.25f;

            // Start cost = 0
            int cost = 0;
            cost += flatCost;

            float skillCost = 0;
            // Add cost for each attribute
            foreach (int attributeValue in character.attributes)
            {
                skillCost += attributeValue * skillCostMultiplier;
            }
            cost += Mathf.RoundToInt(skillCost);

            // Add cost for each piece of equipment
            foreach (Equipment e in character.equipment)
            {
                // Divide the equipment cost by 2
                cost += Mathf.RoundToInt(SampleCost(e) / 2);
            }

            // Add random cost from a normal distribution
            Normal normalDist = Normal.WithMeanStdDev(mu, sigma);
            Func<int> sampleNormal = () => Mathf.RoundToInt(Mathf.Clamp((float)normalDist.Sample(), lowerBound, upperBound));
            cost += sampleNormal();

            return cost;
        }

        int SampleCost(Equipment equipment)
        {
            int flatCost = 15;
            int mu = 7;
            float sigma = 3f;
            int lowerBound = 1;
            int upperBound = 20;
            float skillCostMultiplier = 1.25f;
            
            
            // Start cost = 0
            int cost = 0;
            cost += flatCost;

            float skillCost = 0;
            // Add cost for each attribute
            foreach (int attributeValue in equipment.attributes)
            {
                skillCost += attributeValue * skillCostMultiplier;
            }
            cost += Mathf.RoundToInt(skillCost);

            // Rarity increases cost
            switch (equipment.rarity)
            {
                case Equipment.Rarity.COMMON:
                    cost += 0;
                    break;
                case Equipment.Rarity.RARE:
                    cost += 0;
                    break;
                case Equipment.Rarity.EPIC:
                    cost += 0;
                    break;
                case Equipment.Rarity.LEGENDARY:
                    cost += 0;
                    break;
            }

            // Add random cost from a normal distribution
            Normal normalDist = Normal.WithMeanStdDev(mu, sigma);
            Func<int> sampleNormal = () => Mathf.RoundToInt(Mathf.Clamp((float)normalDist.Sample(), lowerBound, upperBound));
            cost += sampleNormal();

            return cost;
        }

        public void Purchase(ShopItem item)
        {
            GameState state = GameState.instance;
            if(state.money < item.cost)
                return; // Nothing Happens
            
            if((item.item is Character) && (state.GetTotalCharactersCount() >= state.partyLimit))
                return; // Nothing Happens

            // increment state totalCharacters and totalEquipment
            if (item.item is Character)
                state.numCharactersRecruited++;
            else if (item.item is Equipment)
                state.numEquipmentPurchased++;

            items.Remove(item);
            state.AddEntity(item.item);
            state.ModifyMoney(-item.cost);
            state.OnStateModified();
        }
    }
}
