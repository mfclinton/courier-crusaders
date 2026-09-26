using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using Game;
using Generators;
using System.Linq;
using UnityEngine.SceneManagement;
using MathNet.Numerics.Distributions;
using UnityEngine.Profiling;
using static Game.Attributes;

namespace Manager
{
    public class GameManager : MonoBehaviour
    {
        [SerializeField] int money = 100;
        [SerializeField] float moneyEventPercentage = .5f;
        [SerializeField] Vector2Int moneyEventRange = new Vector2Int(1, 10);
        [SerializeField] Vector2Int daysDelayedRange = new Vector2Int(1, 5);
        [SerializeField] int maxLevelUpValue = 2;
        [SerializeField] float characterDiePercentage = .5f;
        // Inclusive
        [SerializeField] Vector2Int partySizeRange = new Vector2Int(5, 8);


        public UIManager uiManager { get; private set; }
        public WorldStateManager wsm { get; private set;}
        public GameState state { get; private set;}
        public GameObject gameOverScreen;

        public Dictionary<WorldEvent.EventType, float> formingPartySuccessProbabilities;
        public Dictionary<DeliveryQuest, Dictionary<WorldEvent.EventType, Dictionary<Attribute, float>>> successProbabilities;

        public static GameManager instance;

        private void Awake()
        {
            instance = this;
            successProbabilities = new Dictionary<DeliveryQuest, Dictionary<WorldEvent.EventType, Dictionary<Attribute, float>>>();
        }

        void Start()
        {

            uiManager = FindObjectOfType<UIManager>();
            wsm = FindObjectOfType<WorldStateManager>();

            CharacterGenerator cg = FindObjectOfType<CharacterGenerator>();
            EquipmentGenerator eg = FindObjectOfType<EquipmentGenerator>();

            state = new GameState(cg.GenerateCharacter, eg.GenerateEquipment, wsm.SampleDeliveryEvent, partySizeRange, money: money);
            uiManager.UpdateUI(state);
            state.onStateModifiedInvoked += uiManager.UpdateUI;

            // Success Probability Functions
            state.onActiveDeliveryAddedInvoked += UpdateSuccessProbability;
            state.onActiveDeliveryRemovedInvoked += RemoveSuccessProbability;
            state.onFormingPartyModifiedInvoked += UpdateFormingPartySuccessProbability;
        }

        public void AdvanceDay()
        {
            LinkedList<(DeliveryQuest, Node, float, Vector2)> deliveryQuestInfo = new LinkedList<(DeliveryQuest, Node, float, Vector2)>();
            foreach(DeliveryQuest dq in state.activeDeliveries)
            {
                // Gets info about the party's progress
                (Node curNode, float percentCompleted, Vector2 relativePosition) = dq.GetPathProgress();
                deliveryQuestInfo.AddLast((dq, curNode, percentCompleted, relativePosition));
            }

            state.AdvanceDay();

            // Handles if an event is triggered for a town
            bool eventTriggered = false;
            foreach((DeliveryQuest dq, Node prevCurNode, float prevPercentCompleted, Vector2 prevRelativePosition) in deliveryQuestInfo)
            {
                // if quest was completed, skip
                bool success = dq.IsComplete();
                bool failure = dq.IsFailed();
                bool questOver = success || failure;
                if(questOver)
                    continue;

                (Node curNode, float percentCompleted, Vector2 relativePosition) = dq.GetPathProgress();
                var worldEvent = wsm.SampleWorldEvent(dq, curNode, percentCompleted, relativePosition, prevCurNode, prevPercentCompleted, prevRelativePosition);
                if(worldEvent == null)
                    continue;
                
                eventTriggered = true;
                (var result, var attributeValue, string flavor) = RunQuestEvent(worldEvent, dq, curNode);
                ProcessDeliveryEvent(dq, result, attributeValue, flavor);
            }

            if(eventTriggered)
                uiManager.UpdateUI(state);

            // Instantiates UI elements on world map, generates new events
            // Places questing characters on world map
            wsm.ProcessNewDay();

            if(state.money < 0)
            {
                // Game Over
                uiManager.SetGameOverStats();
                gameOverScreen.SetActive(true);
            }

            // uiManager.UpdateUI(state);

            // Updates the general party quest data info
            uiManager.UpdatePartyQuestData(); // quick fix
            uiManager.OnNewDay();
            uiManager.SetSuccessRates();
        }


        public void ProcessDeliveryEvent(DeliveryQuest dq, DeliveryEventGenerator.EventResult eventResult, Attributes.Attribute attribute, string flavor)
        {
            if(dq.party.Count == 0)
                return;

            // sample a party character based on the attribute
            List<int> attrList = dq.party.Select(c => c.GetAttributes().GetAttribute(attribute)).ToList();
            int totalIntelligence = dq.party.Select(c => c.attributes.intelligence).Sum();

            double[] probs = SoftmaxIntelligence(attrList.Select(x => (double)x).ToArray(), totalIntelligence);
            int index = new Categorical(probs).Sample();

            // random character
            Character randomCharacter = dq.party.ElementAt(index);

            string day_header = $"[Day {GameState.instance.day - 1}]";
            dq.ProccessAddingEventText(day_header, 3); // 3 is hard coded

            if(flavor == null || flavor.Equals(""))
                flavor = "Something happened to ~!?";
            flavor = flavor.Replace("~", randomCharacter.name);

            int badEventInt = (int) DeliveryEventGenerator.EventResult.BAD;
            int goodEventInt = (int) DeliveryEventGenerator.EventResult.GOOD;
            int nothingEventInt = (int) DeliveryEventGenerator.EventResult.NOTHING;

            dq.ProccessAddingEventText(flavor, nothingEventInt);

            float rng = UnityEngine.Random.value;
            if(eventResult == DeliveryEventGenerator.EventResult.GOOD)
            {
                if(rng < moneyEventPercentage)
                {
                    int reward = UnityEngine.Random.Range(moneyEventRange.x, moneyEventRange.y + 1); // TODO MAKE BETTER
                    string rewardFlavor = $"The party has been rewarded with {reward} gold.";
                    dq.ProccessAddingEventText(rewardFlavor, goodEventInt);
                    state.ModifyMoney(reward);
                    
                    dq.SetCharacterStatus(randomCharacter, DeliveryQuest.Status.CASH, reward);
                }
                else
                {
                    string levelUp = $"{randomCharacter.name} has become more skilled.";
                    dq.ProccessAddingEventText(levelUp, goodEventInt);

                    // Create random attributes
                    Attributes randomAttrs = Attributes.RandomAttributes(maxLevelUpValue);
                    randomCharacter.attributes.AddAttributes(randomAttrs);

                    // Update success stats
                    UpdateSuccessProbability(dq);
                    
                    dq.SetCharacterStatus(randomCharacter, DeliveryQuest.Status.LEVELUP);
                }
            }
            else if(eventResult == DeliveryEventGenerator.EventResult.BAD)
            {
                if(rng < characterDiePercentage)
                {
                    string deathFlavor = $"Sadly, {randomCharacter.name} has died.";
                    dq.ProccessAddingEventText(deathFlavor, badEventInt);
                    dq.party.Remove(randomCharacter);

                    dq.SetCharacterStatus(randomCharacter, DeliveryQuest.Status.DEAD);
                }
                else
                {
                    int daysDelayed = UnityEngine.Random.Range(daysDelayedRange.x, daysDelayedRange.y + 1);
                    string plural = daysDelayed > 1 ? "s" : "";
                    string daysDelayedFlavor = $"The party loses {daysDelayed} day{plural} of progress to recover.";
                    dq.ProccessAddingEventText(daysDelayedFlavor, badEventInt); // TODO
                    dq.daysDelayed = daysDelayed;

                    dq.SetCharacterStatus(randomCharacter, DeliveryQuest.Status.WAITING, daysDelayed);
                }
            }
        }

        public void StartDelivery()
        {
            state.StartDelivery();
            AdvanceDay();
        }


        public (Manager.DeliveryEventGenerator.EventResult, Game.Attributes.Attribute, string) RunQuestEvent(WorldEvent worldEvent, DeliveryQuest dq, Node curNode)
        {
            Attributes totalAttributes = new Attributes(0, 0, 0, 0, 0);
            foreach(Character c in dq.party)
            {
                Attributes a = c.GetAttributes();
                totalAttributes.AddAttributes(a);
            }

            DeliveryEventGenerator deg = DeliveryEventGenerator.instance;
            (var result, var attributeValue, string flavor) = deg.ProcessEvent(dq, worldEvent.Type, totalAttributes);

            return (result, attributeValue, flavor);
        }

        public void Restart()
        {
            SceneManager.LoadScene(SceneManager.GetActiveScene().name);
        }

        public static double[] SoftmaxFunction(double[] input, double temperature = 2.5)
        {
            double maxElement = input.Max();
            double[] shiftedInput = input.Select(x => x - maxElement).ToArray(); // Shift input to avoid overflow
            double[] tempAdjustedInput = shiftedInput.Select(x => x / temperature).ToArray(); // Adjust for temperature
            double[] expInput = tempAdjustedInput.Select(x => System.Math.Exp(x)).ToArray(); // Calculate exponent of input
            double expInputSum = expInput.Sum(); // Sum of exponentiated input values

            // Normalize input using the exponentiated input and sum
            double[] softmaxOutput = expInput.Select(x => x / expInputSum).ToArray();
            return softmaxOutput;
        }

        public static double[] SoftmaxIntelligence(double[] input, int intelligence)
        {
            double t = Mathf.Clamp(9/Mathf.Pow(intelligence, 0.5f), 0.5f, 9);
            return GameManager.SoftmaxFunction(input, temperature: t);
        }

        public Dictionary<WorldEvent.EventType, Dictionary<Attribute, float>> GetSuccessProbability(DeliveryQuest dq)
        {
            if(!successProbabilities.ContainsKey(dq))
                UpdateSuccessProbability(dq);
            
            return successProbabilities[dq];
        }

        public void UpdateSuccessProbability(DeliveryQuest dq)
        {
            var successProbability = wsm.CalculateSuccessProbability(dq.party);
            if(successProbabilities.ContainsKey(dq))
                successProbabilities[dq] = successProbability;
            else if(successProbability != null)
                successProbabilities.Add(dq, successProbability);
        }

        public void RemoveSuccessProbability(DeliveryQuest dq)
        {
            if(successProbabilities.ContainsKey(dq))
                successProbabilities.Remove(dq);
        }

        public void UpdateFormingPartySuccessProbability(LinkedList<Character> party)
        {
            //if null or empty, set to null
            if(party == null || party.Count == 0)
            {
                formingPartySuccessProbabilities = null;
                uiManager.SetSuccessRates();
                return;
            }

            var successProbs = wsm.CalculateSuccessProbability(party);

            // Set formingPartySuccessProbabilities
            formingPartySuccessProbabilities = new Dictionary<WorldEvent.EventType, float>();
            foreach(WorldEvent.EventType type in System.Enum.GetValues(typeof(WorldEvent.EventType)))
            {
                formingPartySuccessProbabilities[type] = wsm.GetEventSuccessProbability(type, successProbs);
            }

            uiManager.SetSuccessRates();
        }

        public void UpdateAllSuccessProbabilities()
        {
            // foreach(DeliveryQuest dq in successProbabilities.Keys)
            //     UpdateSuccessProbability(dq);
            foreach(DeliveryQuest dq in state.activeDeliveries)
                UpdateSuccessProbability(dq);
        }

        public void ClearCurrentPath()
        {
            state.ResetAvailableDelivery();
        }
    }
}
