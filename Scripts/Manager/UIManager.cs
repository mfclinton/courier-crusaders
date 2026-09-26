using System.Collections;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;
using UnityEngine.UI;
using TMPro;
using Game;
using UnityEngine.Profiling;
using System;

namespace Manager
{
    public class UIManager : MonoBehaviour
    {
        [Header("Game Stats")]
        [SerializeField] private TextMeshProUGUI cashText;
        [SerializeField] private TextMeshProUGUI dayText;
        [SerializeField] private TextMeshProUGUI upkeepText;

        [Header("Main Panels")]
        [SerializeField] private Transform[] panels;

        [Header("Delivery Panel")]
        [SerializeField] private TextMeshProUGUI deliveryText;
        [SerializeField] private TextMeshProUGUI deliveryDeadlineText;
        [SerializeField] private TextMeshProUGUI deliveryRewardText;
        [SerializeField] private Transform deliveryCrewPreparingParent;
        [SerializeField] private Transform deliveryCrewWaitingParent;
        [SerializeField] private GameObject validPathText;
        [SerializeField] private GameObject addPartyMemberText;
        [SerializeField] private Button startDeliveryButton;
        [SerializeField] private UIPartyStats partyStats;
        [SerializeField] private UIPartyQuestData partyQuestData;
        [SerializeField] private TextMeshProUGUI partyCountText;


        [Header("Crew Panel")]
        [SerializeField] private Transform crewParent;
        [SerializeField] private Transform crewRecruitParent;

        [Header("Character Panel")]
        [SerializeField] private Image characterImage;

        [SerializeField] private TextMeshProUGUI characterNameText;
        [SerializeField] private TextMeshProUGUI characterStrengthText;
        [SerializeField] private TextMeshProUGUI characterConstitutionText;
        [SerializeField] private TextMeshProUGUI characterDexterityText;
        [SerializeField] private TextMeshProUGUI characterIntelligenceText;
        [SerializeField] private TextMeshProUGUI characterCharismaText;

        [SerializeField] private Transform characterEquippedParent;
        [SerializeField] private Transform characterPurchaseParent;
        [SerializeField] private Transform characterInventoryParent;
        [SerializeField] private TextMeshProUGUI characterInventoryLimitText;
        
        [Header("Map Panel")]
        [SerializeField] private GameObject mapValidPathText;
        [SerializeField] private GameObject mapAddPartyMemberText;
        [SerializeField] private Button mapStartDeliveryButton;
        [SerializeField] Image mapStartDeliveryButtonBG;
        [SerializeField] Color disabledStartDeliveryButtonColor;
        [SerializeField] Color enabledStartDeliveryButtonColor;


        [Header("Delivery Status")]
        [SerializeField] private UIDeliveryStatus deliveryStatusPrefab;
        [SerializeField] private Transform deliveryStatusParent;

        [Header("UI Prefabs")]
        [SerializeField] private UIEntityIcon characterCardPrefab;
        [SerializeField] private UIEntityIcon equipmentCardPrefab; // TODO: special class for equipment cards

        [Header("Fun UI Texts")]
        [SerializeField] private TextMeshProUGUI totalCharactersText;
        [SerializeField] private TextMeshProUGUI totalEquipmentText;
        [SerializeField] private TextMeshProUGUI totalDeliveriesText;
        [SerializeField] private TextMeshProUGUI totalMoneyEarnedText;

        [Header("Success Probs")]
        [SerializeField] private TextMeshProUGUI battleSuccessProbText;
        [SerializeField] private TextMeshProUGUI dangerSuccessProbText;
        [SerializeField] private TextMeshProUGUI magicSuccessProbText;
        [SerializeField] private TextMeshProUGUI crimeSuccessProbText;
        [SerializeField] private TextMeshProUGUI randomSuccessProbText;

        [Header("World Event Counts Objects")]
        [SerializeField] private GameObject battleEventsCountObject;
        [SerializeField] private GameObject dangerEventsCountObject;
        [SerializeField] private GameObject magicEventsCountObject;
        [SerializeField] private GameObject crimeEventsCountObject;
        [SerializeField] private GameObject randomEventsCountObject;

        [Header("World Event Counts")]
        [SerializeField] private TextMeshProUGUI battleEventCountText;
        [SerializeField] private TextMeshProUGUI dangerEventCountText;
        [SerializeField] private TextMeshProUGUI magicEventCountText;
        [SerializeField] private TextMeshProUGUI crimeEventCountText;
        [SerializeField] private TextMeshProUGUI randomEventCountText;


        // Static instance
        public static UIManager instance { get; private set; }
        public Dictionary<DeliveryQuest, UIDeliveryStatus> deliveryStatuses { get; private set; }

        void Awake()
        {
            instance = this;
            deliveryStatuses = new Dictionary<DeliveryQuest, UIDeliveryStatus>();
        }

        public void UpdateGameStats(GameState state)
        {
            cashText.text = state.money.ToString();
            dayText.text = $"Day {state.day}";
            upkeepText.text = $"- {state.upkeep} gold";
        }

        public void SetActivePanel(int panelIndex)
        {
            for (int i = 0; i < panels.Length; i++)
                panels[i].gameObject.SetActive(i == panelIndex);
            
            UpdateUI(GameState.instance, GameState.lastSelectedCharacter);
        }

        public int GetActivePanel()
        {
            for (int i = 0; i < panels.Length; i++)
                if (panels[i].gameObject.activeSelf)
                    return i;

            return -1;
        }

        public void SetInventoryPanel(GameState state, Character c)
        {
            GameState.lastSelectedCharacter = c;
            SetActivePanel(2);
        }

        public void UpdateMapPanel(GameState state)
        {
            // Update the start delivery button
            bool validPath = MapManager.instance.pp != null && MapManager.instance.pp.PathComplete();
            mapValidPathText.gameObject.SetActive(!validPath);

            bool hasPartyMembers = state.formingParty.Count > 0;
            mapAddPartyMemberText.gameObject.SetActive(!hasPartyMembers);

            mapStartDeliveryButton.interactable = validPath && hasPartyMembers;
            mapStartDeliveryButtonBG.color = mapStartDeliveryButton.interactable ? enabledStartDeliveryButtonColor : disabledStartDeliveryButtonColor;
        }

        public void UpdateDeliveryPanel(GameState state)
        {
            ClearTransform(deliveryCrewPreparingParent);
            ClearTransform(deliveryCrewWaitingParent);

            // Shows waiting characters
            int index = 0;
            LinkedList<Character> waitingCharacters = state.GetWaitingCharacters();
            foreach(Character c in waitingCharacters)
            {
                UIEntityIcon charIcon = InstantiateEntityIcon(c, deliveryCrewWaitingParent, index: index);
                index++;
            }

            index = 0;
            LinkedList<Character> formingParty = state.formingParty;
            foreach(Character c in formingParty)
            {
                UIEntityIcon charIcon = InstantiateEntityIcon(c, deliveryCrewPreparingParent, index: index);
                index++;
            }

            // Update the party stats
            partyStats.SetAttributes(state.formingParty);
            partyStats.SetDeliveryTime(state.availableDelivery, state.formingParty, MapManager.instance.pp.Path);

            if(state.availableDelivery != null)
            {
                deliveryText.text = state.availableDelivery.note;
                deliveryDeadlineText.text = $"Deadline: {state.availableDelivery.deadline} days";
                deliveryRewardText.text = $"Reward: {state.availableDelivery.reward} gold";
            }
            
            // Set the start deliver button to disabled
            bool validPath = MapManager.instance.pp != null && MapManager.instance.pp.PathComplete();
            validPathText.gameObject.SetActive(!validPath);

            bool hasPartyMembers = state.formingParty.Count > 0;
            addPartyMemberText.gameObject.SetActive(!hasPartyMembers);

            startDeliveryButton.interactable = validPath && hasPartyMembers;

            // Update the party quest map data
            UpdatePartyQuestData();
        }


        public void UpdateCrewPanel(GameState state)
        {
            // getYourCrew
            int index = 0;
            ClearTransform(crewParent);
            foreach(Character c in state.characters)
            {
                UIEntityIcon charIcon = InstantiateEntityIcon(c, crewParent, index: index);
                index++;
            }

            // getShopCharacters
            index = 0;
            ClearTransform(crewRecruitParent);
            var shopCharacters = state.shop.items.Where(i => i.item is Character);
            foreach(ShopItem si in shopCharacters)
            {
                UIEntityIcon shopCharIcon = InstantiateEntityIcon(si.item, crewRecruitParent, si, index: index);
                index++;
            }

            SetPartyCountText();
        }


        public void UpdateCharacterPanel(GameState state, Character c = null)
        {
            int index = 0;

            // Specific to the character UI
            if(c != null)
            {
                characterImage.sprite = c.sprite;
                characterInventoryLimitText.text = $"{c.equipment.Count} / {c.equipmentCapacity}";

                (Attributes charAttr, Attributes equipAttr) = c.GetSplitAttributes();

                characterNameText.text = c.name;
                characterStrengthText.text = $"Strength: {c.attributes.strength}";
                characterConstitutionText.text = $"Constitution: {c.attributes.constitution}";
                characterDexterityText.text = $"Dexterity: {c.attributes.dexterity}";
                characterIntelligenceText.text = $"Intelligence: {c.attributes.intelligence}";
                characterCharismaText.text = $"Charisma: {c.attributes.charisma}";

                if(equipAttr.strength != 0)
                    characterStrengthText.text += $"+{equipAttr.strength}= {charAttr.strength + equipAttr.strength}";
                if(equipAttr.constitution != 0)
                    characterConstitutionText.text += $"+{equipAttr.constitution}= {charAttr.constitution + equipAttr.constitution}";
                if(equipAttr.dexterity != 0)
                    characterDexterityText.text += $"+{equipAttr.dexterity}= {charAttr.dexterity + equipAttr.dexterity}";
                if(equipAttr.intelligence != 0)
                    characterIntelligenceText.text += $"+{equipAttr.intelligence}= {charAttr.intelligence + equipAttr.intelligence}";
                if(equipAttr.charisma != 0)
                    characterCharismaText.text += $"+{equipAttr.charisma}= {charAttr.charisma + equipAttr.charisma}";

                // Update character equiped ui
                index = 0;
                ClearTransform(characterEquippedParent);
                foreach(Equipment e in c.equipment)
                {
                    UIEntityIcon charEquIcon = InstantiateEntityIcon(e, characterEquippedParent, index: index);
                    index++;
                }
            }

            // Update equipment shop ui
            index = 0;
            ClearTransform(characterPurchaseParent);
            var shopEquipment = state.shop.items.Where(i => i.item is Equipment);
            foreach(ShopItem si in shopEquipment)
            {
                UIEntityIcon shopEquIcon = InstantiateEntityIcon(si.item, characterPurchaseParent, si, index: index);
                index++;
            }

            // Update inventory ui
            index = 0;
            ClearTransform(characterInventoryParent);
            foreach(Equipment e in state.equipment)
            {
                UIEntityIcon invEquIcon = InstantiateEntityIcon(e, characterInventoryParent, index: index);
                index++;
            }
        }

        // TODO: not efficient
        void ClearTransform(Transform t)
        {
            foreach(Transform child in t)
                ObjectPooler.Instance.ReturnToPool(child.gameObject);
        }

        UIEntityIcon InstantiateEntityIcon(Entity e, Transform parent, ShopItem item = null, int index = -1)
        {
            UIEntityIcon.State state = UIEntityIcon.State.NONE;
            if(parent == crewRecruitParent || parent == characterPurchaseParent)
                state = UIEntityIcon.State.SHOP;
            else if(parent == crewParent || parent == characterInventoryParent)
                state = UIEntityIcon.State.HOME;
            else if(parent == characterEquippedParent)
                state = UIEntityIcon.State.ACTIVE;
            else if(parent == deliveryCrewWaitingParent)
                state = UIEntityIcon.State.WAITING;
            else if(parent == deliveryCrewPreparingParent)
                state = UIEntityIcon.State.PREPARING;
            else if(parent == deliveryCrewPreparingParent)
                state = UIEntityIcon.State.NONE;
            

            UIEntityIcon prefab = e is Character ? characterCardPrefab : equipmentCardPrefab;
            UIEntityIcon icon = ObjectPooler.Instance.SpawnFromPool(prefab.gameObject, parent: parent, childIndex: index).GetComponent<UIEntityIcon>();
            icon.UpdateEntity(e, state, item);

            return icon;
        }

        public void UpdateUI(GameState state, Character c = null)
        {
            int activePanel = GetActivePanel();

            if(activePanel == 0)
                UpdateDeliveryPanel(state);
            else if(activePanel == 1)
                UpdateCrewPanel(state);
            else if(activePanel == 2)
                UpdateCharacterPanel(state, c);
            else if(activePanel == 3)
                UpdateMapPanel(state);

            UpdateGameStats(state);
            UpdateDeliveryQuests();
        }

        public void PathUpdated(List<Node> path)
        {
            UpdateDeliveryPanel(GameState.instance);
            UpdateMapPanel(GameState.instance);
        }

        public void UpdateDeliveryQuests()
        {
            // Rebuild
            foreach(DeliveryQuest dq in GameState.instance.activeDeliveries)
            {
                NewDeliveryQuest(dq);
            }

            foreach(var pair in deliveryStatuses)
            {
                bool isComplete = pair.Value.HandleQuestCompletion();
            }
        }

        public void NewDeliveryQuest(DeliveryQuest dq)
        {
            if(deliveryStatuses.ContainsKey(dq))
            {
                deliveryStatuses[dq].UpdateUI();
                return;
            }

            UIDeliveryStatus status = ObjectPooler.Instance.SpawnFromPool(deliveryStatusPrefab.gameObject, parent: deliveryStatusParent).GetComponent<UIDeliveryStatus>();
            status.Initialize(dq);
            deliveryStatuses.Add(dq, status);
        }

        public void UpdatePartyQuestData()
        {
            Delivery delivery = GameState.instance.availableDelivery;
            LinkedList<Character> party = GameState.instance.formingParty;
            List<Node> nodePath = MapManager.instance.pp.Path;

            WorldStateManager wsm = WorldStateManager.instance;

            partyQuestData.SetEventData(wsm.daysLeftForEvents, DeliveryEventGenerator.instance.GetSkillCheckRange());

            if(delivery != null)
            {
                int estimatedDeliveryDays = DeliveryQuest.EstimateDeliveryDays(delivery, party, nodePath);
                partyQuestData.SetQuestData(estimatedDeliveryDays, delivery.deadline, party.Count);
            }
        }

        public void SetGameOverStats()
        {
            totalCharactersText.text = $"Total Characters Recruited: {GameState.instance.numCharactersRecruited}";
            totalEquipmentText.text = $"Total Equipment Purchased: {GameState.instance.numEquipmentPurchased}";
            totalDeliveriesText.text = $"Total Deliveries Completed: {GameState.instance.numDeliveriesCompleted}";
            totalMoneyEarnedText.text = $"Total Money Earned: {GameState.instance.totalMoneyEarned}";
        }

        // On new day delivery statuses
        public void OnNewDay()
        {
            foreach(var pair in deliveryStatuses)
            {
                pair.Value.OnNewDay();
            }
        }

        public void SetSuccessRates()
        {
            GameManager gm = GameManager.instance;
            Dictionary<WorldEvent.EventType, float> successProbs = gm.formingPartySuccessProbabilities;

            if(successProbs == null)
            {
                battleSuccessProbText.text = "0%";
                dangerSuccessProbText.text = "0%";
                magicSuccessProbText.text = "0%";
                crimeSuccessProbText.text = "0%";
                randomSuccessProbText.text = "0%";
            }
            else
            {
                battleSuccessProbText.text = $"{Mathf.RoundToInt(successProbs[WorldEvent.EventType.BATTLE] * 100)}%";
                dangerSuccessProbText.text = $"{Mathf.RoundToInt(successProbs[WorldEvent.EventType.DANGER] * 100)}%";
                magicSuccessProbText.text = $"{Mathf.RoundToInt(successProbs[WorldEvent.EventType.MAGIC] * 100)}%";
                crimeSuccessProbText.text = $"{Mathf.RoundToInt(successProbs[WorldEvent.EventType.CRIME] * 100)}%";
                randomSuccessProbText.text = $"{Mathf.RoundToInt(successProbs[WorldEvent.EventType.RANDOM] * 100)}%";
            }
        }

        public void UpdateWorldEventCounts(int[] eventTypeCounts)
        {
            int battleIndex = (int)WorldEvent.EventType.BATTLE;
            int dangerIndex = (int)WorldEvent.EventType.DANGER;
            int magicIndex = (int)WorldEvent.EventType.MAGIC;
            int crimeIndex = (int)WorldEvent.EventType.CRIME;
            int randomIndex = (int)WorldEvent.EventType.RANDOM;

            battleEventCountText.text = eventTypeCounts[battleIndex].ToString();
            dangerEventCountText.text = eventTypeCounts[dangerIndex].ToString();
            magicEventCountText.text = eventTypeCounts[magicIndex].ToString();
            crimeEventCountText.text = eventTypeCounts[crimeIndex].ToString();
            randomEventCountText.text = eventTypeCounts[randomIndex].ToString();

            battleEventsCountObject.SetActive(eventTypeCounts[battleIndex] > 0);
            dangerEventsCountObject.SetActive(eventTypeCounts[dangerIndex] > 0);
            magicEventsCountObject.SetActive(eventTypeCounts[magicIndex] > 0);
            crimeEventsCountObject.SetActive(eventTypeCounts[crimeIndex] > 0);
            randomEventsCountObject.SetActive(eventTypeCounts[randomIndex] > 0);
        }

        public void SetPartyCountText()
        {
            int partyCount = GameState.instance.GetTotalCharactersCount();
            int partyLimit = GameState.instance.partyLimit;

            partyCountText.text = $"{partyCount} / {partyLimit}";
        }
    }
}
