using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
using TMPro;
using Game;

namespace Manager
{
    public class UIEntityIcon : MonoBehaviour
    {
        public enum State
        {
            SHOP,
            HOME,
            ACTIVE,
            WAITING,
            PREPARING,
            NONE
        }

        [SerializeField] private TextMeshProUGUI nameText;
        [SerializeField] private Image mainImage;
        [SerializeField] private Image backgroundImage;
        [SerializeField] private TextMeshProUGUI shopText;
        [SerializeField] private Image deleteButton;

        [Header("Attribute Fields")]
        [SerializeField] private TextMeshProUGUI strengthText;
        [SerializeField] private TextMeshProUGUI constitutionText;
        [SerializeField] private TextMeshProUGUI dexterityText;
        [SerializeField] private TextMeshProUGUI intelligenceText;
        [SerializeField] private TextMeshProUGUI charismaText;

        [Header("Sub Image Elements")]
        [SerializeField] private Sprite[] subSprites;
        [SerializeField] private Image subImage;
        [SerializeField] private TextMeshProUGUI subText;

        [Header("Equipment Specific")]
        [SerializeField] private Color[] rarityColors;

        [Header("Character Specific")]
        [SerializeField] private TextMeshProUGUI characterInventoryText;

        Entity entityRef;
        ShopItem shopItemRef;
        State curState;
        

        void UpdateGenericAttrFields(Attributes attributes)
        {
            if(strengthText == null || constitutionText == null || dexterityText == null || intelligenceText == null || charismaText == null)
                return;

            strengthText.text = $"Strength: {attributes.strength}";
            constitutionText.text = $"Constitution: {attributes.constitution}";
            dexterityText.text = $"Dexterity: {attributes.dexterity}";
            intelligenceText.text = $"Intelligence: {attributes.intelligence}";
            charismaText.text = $"Charisma: {attributes.charisma}";
        }

        void UpdateCharacterAttrFields(Character character)
        {
            if(strengthText == null || constitutionText == null || dexterityText == null || intelligenceText == null || charismaText == null)
                return;

            (Attributes charAttr, Attributes equipAttr) = character.GetSplitAttributes();

            UpdateGenericAttrFields(charAttr);

            if(equipAttr.strength != 0)
                strengthText.text += $"+{equipAttr.strength}";
            if(equipAttr.constitution != 0)
                constitutionText.text += $"+{equipAttr.constitution}";
            if(equipAttr.dexterity != 0)
                dexterityText.text += $"+{equipAttr.dexterity}";
            if(equipAttr.intelligence != 0)
                intelligenceText.text += $"+{equipAttr.intelligence}";
            if(equipAttr.charisma != 0)
                charismaText.text += $"+{equipAttr.charisma}";
            
        }

        public void UpdateAttrFields()
        {
            if(entityRef is Character)
                UpdateCharacterAttrFields((Character)entityRef);
            else if(entityRef is Equipment)
                UpdateGenericAttrFields(entityRef.attributes);
        }

        public void UpdateEntity(Entity entity, State state, ShopItem shopItem)
        {
            entityRef = entity;
            shopItemRef = shopItem; // TODO: unsure if needed
            SetState(state);

            // Update name and image
            if (nameText != null)
                nameText.text = entity.name;

            if (mainImage != null)
                mainImage.sprite = entity.sprite;
            
            // Update shop
            if (shopItem != null && shopText != null)
                shopText.text = $"{shopItem.cost}";

            // Update fields
            UpdateAttrFields();
            UpdateRarityColor();
            UpdateInventoryText();
        }

        public void UpdateInventoryText()
        {
            if(!(entityRef is Character))
                return;
            Character c = (Character) entityRef;
            characterInventoryText.text = $"{c.equipment.Count}/{c.equipmentCapacity}";
        }

        public void UpdateRarityColor()
        {
            if(entityRef is Equipment)
            {
                Equipment equipment = (Equipment)entityRef;
                backgroundImage.color = rarityColors[(int)equipment.rarity];
            }
        }

        void SetState(State state)
        {
            curState = state;

            // Update sub image
            bool isNormalState = state != State.NONE;
            if(isNormalState)
                subImage.sprite = subSprites[(int)state];
            subImage.gameObject.SetActive(isNormalState);

            // Update shop text
            bool isShopState = state == State.SHOP;
            shopText.transform.parent.gameObject.SetActive(isShopState);

            // Update delete button
            bool isHomeState = state == State.HOME;
            deleteButton.gameObject.SetActive(isHomeState);

            // Set sub-text to buy if in shop
            if(isShopState)
                subText.text = "Buy";
            else if(entityRef is Character)
            {
                if(isHomeState)
                    subText.text = "Inv";
                else if(state == State.WAITING)
                    subText.text = "Assign";
                else if(state == State.PREPARING)
                    subText.text = "Unassign";
            }
            else if(entityRef is Equipment)
            {
                if(isHomeState)
                    subText.text = "Equip";
                else if(state == State.ACTIVE)
                    subText.text = "Unequip";
            }

        }

        public void ButtonClicked()
        {
            if(curState == State.SHOP)
                shopItemRef?.Purchase();
            
            else if(curState == State.HOME && entityRef is Character)
                UIManager.instance.SetInventoryPanel(GameState.instance, (Character)entityRef);
            
            else if(curState == State.HOME && entityRef is Equipment)
                GameState.instance.EquipItemToSelectedCharacter((Equipment)entityRef);

            else if(curState == State.ACTIVE && entityRef is Equipment)
                GameState.instance.UnequipItemFromSelectedCharacter((Equipment)entityRef);
            
            else if(curState == State.WAITING && entityRef is Character)
                GameState.instance.AddCharacterToParty((Character)entityRef);

            else if(curState == State.PREPARING && entityRef is Character)
                GameState.instance.RemoveCharacterFromParty((Character)entityRef);
        }

        public void DeleteButtonPressed()
        {
            GameState.instance.RemoveEntity(entityRef);
            GameState.instance.OnStateModified();
        }
    }
}
