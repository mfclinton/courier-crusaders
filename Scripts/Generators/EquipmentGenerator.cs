using System;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;
using DataParsing;
using Game;

#if UNITY_EDITOR
using UnityEditor;
#endif

namespace Generators {
    public class EquipmentGenerator : MonoBehaviour
    {
        // Equipment Data
        public TextAsset equipmentJson;
        public EquipmentData[] equipmentData;

        #if UNITY_EDITOR
        private void OnValidate()
        {
            ParseEquipment();
        }

        void ParseEquipment()
        {
            equipmentData = JsonReader.ParseJson<EquipmentData>(equipmentJson).Cast<EquipmentData>().ToArray();

            // Load sprites
            foreach(EquipmentData equipmentData in equipmentData)
            {
                // Load the sprite using AssetDatabase.LoadAssetAtPath
                Sprite sprite = AssetDatabase.LoadAssetAtPath<Sprite>(equipmentData.sprite_path);
                if(sprite == null)
                    Debug.LogWarning("Failed to load sprite from path: " + equipmentData.sprite_path);
                
                equipmentData.sprite = sprite;
            }
        }
        #endif


        private Attributes GenAttributes(EquipmentData equipmentFromFile)
        {
            int primaryAttributeBuff = 2;
            double attributeProb = 0.75;
            var primaryAttributes = equipmentFromFile.primary_attributes;

            Func<int> baseAttributeValue = () => UnityEngine.Random.value < attributeProb ? 0 : 1; // TODO

            var attributes = new Dictionary<string, int>
            {
                {"strength", baseAttributeValue()},
                {"constitution", baseAttributeValue()},
                {"dexterity", baseAttributeValue()},
                {"intelligence", baseAttributeValue()},
                {"wisdom", baseAttributeValue()},
                {"charisma", baseAttributeValue()}
            };

            foreach (string primaryAttribute in primaryAttributes)
            {
                attributes[primaryAttribute] += primaryAttributeBuff;
            }

            return new Attributes(
                attributes["strength"],
                attributes["constitution"],
                attributes["dexterity"],
                attributes["intelligence"],
                attributes["charisma"]
            );
        }


        // TODO: Make this make sense
        private Equipment.Rarity GenRarity()
        {
            var sampleProbs = new Dictionary<Equipment.Rarity, float>
            {
                {Equipment.Rarity.COMMON, 0.75f},
                {Equipment.Rarity.RARE, 0.15f},
                {Equipment.Rarity.EPIC, 0.075f},
                {Equipment.Rarity.LEGENDARY, 0.025f}
            };

            // Assumes totalProb = 1
            float rng = UnityEngine.Random.value;
            float cumulativeWeight = 0.0f;
            Equipment.Rarity rarity = Equipment.Rarity.COMMON;

            foreach (var kvp in sampleProbs)
            {
                cumulativeWeight += kvp.Value;

                if (rng <= cumulativeWeight)
                {
                    rarity = kvp.Key;
                    break;
                }
            }

            return rarity;
        }


        public Equipment GenerateEquipment()
        {
            EquipmentData equipmentFromFile = equipmentData[UnityEngine.Random.Range(0, equipmentData.Length)];
            Equipment.Type equipmentType = (Equipment.Type) Enum.Parse(typeof(Equipment.Type), equipmentFromFile.equipment_type.ToUpper());
            Equipment.Rarity equipmentRarity = GenRarity();
            var attributes = GenAttributes(equipmentFromFile);
            
            for (int i = 0; i < (int)equipmentRarity; i++)
            {
                var extraAttributes = GenAttributes(equipmentFromFile);
                attributes.AddAttributes(extraAttributes);
            }

            return new Equipment(
                attributes,
                equipmentFromFile.name,
                equipmentType,
                equipmentRarity,
                equipmentFromFile.sprite
            );
        }
    }
}