using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using System;
using System.Collections.Generic;
using UnityEngine;
using MathNet.Numerics.Distributions;
using System.Linq;
using Game;

using Attribute = Game.Attributes.Attribute;
using EventType = Manager.WorldEvent.EventType;

namespace Manager
{
    public class DeliveryEventGenerator : MonoBehaviour
    {
        public enum EventResult
        {
            BAD,
            GOOD,
            NOTHING
        }

        [Serializable]
        public class AttributeProbability
        {
            public Attribute attribute;
            public float prob;
        }

        [Serializable]
        public class EventTypeConfiguration
        {
            public EventType eventType;
            public List<AttributeProbability> attributeProbabilities;
        }

        [SerializeField] public List<EventTypeConfiguration> eventTypeConfigurations;
        public Dictionary<WorldEvent.EventType, float[]> eventToAttrProbDict;
        public Dictionary<WorldEvent.EventType, Dictionary<HashSet<Attribute>, float>> eventToAttrSubsetProbsDict;

        [SerializeField] private Vector2 eventStatCheckMeanRange;
        [SerializeField] private Vector2 eventStatCheckStdDevRange;

        public float eventStatCheckMean;
        public float eventStatCheckStdDev;

        DeliveryEventDataManager dedm;
        Dictionary<EventType, Dictionary<Attribute, Dictionary<EventResult, List<string>>>> data;

        public static DeliveryEventGenerator instance;
        void Awake() {
            instance = this;
            UpdateStats();
            dedm = FindObjectOfType<DeliveryEventDataManager>();
            ReadDeliveryEventDataManagerToDictionary();
            CreateHelperDict();
            InitializeEventToAttrSubsetProbsDict();
        }

        void CreateHelperDict()
        {
            eventToAttrProbDict = new Dictionary<WorldEvent.EventType, float[]>();
            foreach(EventTypeConfiguration eventTypeConfiguration in eventTypeConfigurations)
            {
                int numAttributes = Enum.GetValues(typeof(Attributes.Attribute)).Length;

                // iterate over attributes
                float[] probs = new float[numAttributes];
                foreach(AttributeProbability attributeProbability in eventTypeConfiguration.attributeProbabilities)
                {
                    Attribute attribute = attributeProbability.attribute;
                    float prob = attributeProbability.prob;

                    probs[(int)attribute] = prob;
                }

                eventToAttrProbDict[eventTypeConfiguration.eventType] = probs;
            }
        }

        public void InitializeEventToAttrSubsetProbsDict()
        {
            eventToAttrSubsetProbsDict = new Dictionary<WorldEvent.EventType, Dictionary<HashSet<Attribute>, float>>();
            foreach(EventTypeConfiguration eventTypeConfiguration in eventTypeConfigurations)
            {
                eventToAttrSubsetProbsDict[eventTypeConfiguration.eventType] = CreateEventAttrSubsetProbsDict(eventTypeConfiguration.eventType);
            }
        }

        public Dictionary<HashSet<Attribute>, float> CreateEventAttrSubsetProbsDict(WorldEvent.EventType eventType)
        {
            Dictionary<HashSet<Attribute>, float> attributeCombinationValues = new Dictionary<HashSet<Attribute>, float>(new AttributeSetEqualityComparer());

            // Get the number of values in the enum
            int numValues = System.Enum.GetValues(typeof(Attribute)).Length;

            // The total number of combinations is 2^n where n is the number of values in the enum
            int numCombinations = 1 << numValues;

            // Iterate over each possible combination, starting from 1 to exclude the empty set
            for (int i = 1; i < numCombinations; i++)
            {
                HashSet<Attribute> combination = new HashSet<Attribute>();

                // Iterate over each value in the enum
                for (int j = 0; j < numValues; j++)
                {
                    // Check if the j-th bit of i is set
                    if ((i & (1 << j)) != 0)
                    {
                        // If it is, add the corresponding enum value to the combination
                        combination.Add((Attribute)j);
                    }
                }

                // Iterate over attributes
                float combinationProb = 1f;
                for(int a = 0; a < numValues; a++)
                {
                    Attribute attr = (Attribute)a;
                    float attrProb;

                    if(combination.Contains(attr))
                        attrProb = eventToAttrProbDict[eventType][a]; // TODO: check
                    else
                        attrProb = 1 - eventToAttrProbDict[eventType][a];
                    
                    combinationProb *= attrProb;
                }

                // Add the combination to the dictionary with a default float value
                attributeCombinationValues[combination] = combinationProb;
            }

            return attributeCombinationValues;
        }


        public Dictionary<WorldEvent.EventType, Dictionary<HashSet<Attribute>, Dictionary<Attribute, float>>> CreateEventToAttrChoiceProbsDict(LinkedList<Character> party)
        {
            Dictionary<WorldEvent.EventType, Dictionary<HashSet<Attribute>, Dictionary<Attribute, float>>> eventToAttrChoiceProbsDict = new Dictionary<WorldEvent.EventType, Dictionary<HashSet<Attribute>, Dictionary<Attribute, float>>>();
            foreach(EventTypeConfiguration eventTypeConfiguration in eventTypeConfigurations)
            {
                eventToAttrChoiceProbsDict[eventTypeConfiguration.eventType] = CreatePartyAttrChoiceProbsDict(party, eventTypeConfiguration.eventType);
            }

            return eventToAttrChoiceProbsDict;
        }


        public Dictionary<HashSet<Attribute>, Dictionary<Attribute, float>> CreatePartyAttrChoiceProbsDict(LinkedList<Character> party, WorldEvent.EventType eventType)
        {
            // Get total party stats
            Attributes totalAttributes = new Attributes(0, 0, 0, 0, 0);
            foreach(Character c in party)
            {
                Attributes a = c.GetAttributes();
                totalAttributes.AddAttributes(a);
            }

            var attrSubsetProbs = eventToAttrSubsetProbsDict[eventType];
            var choiceProbsDict = new Dictionary<HashSet<Attribute>, Dictionary<Attribute, float>>();
            foreach(var attrSubset in attrSubsetProbs.Keys)
            {
                var probs = GetDecisionProbs(attrSubset, totalAttributes);

                var attrProbsDict = new Dictionary<Attribute, float>();
                for(int i = 0; i < attrSubset.Count; i++)
                {
                    attrProbsDict[attrSubset.ElementAt(i)] = (float)probs[i];
                }

                choiceProbsDict[attrSubset] = attrProbsDict;
            }

            return choiceProbsDict;
        }


        public double[] GetDecisionProbs(HashSet<Attribute> attrSubset, Attributes totalAttributes)
        {
            var attrValues = attrSubset.Select(attr => (double)totalAttributes.GetAttribute(attr)).ToArray();
            var probs = GameManager.SoftmaxIntelligence(attrValues, totalAttributes.intelligence);
            return probs;
        }


        public void UpdateStats()
        {
            // Logarithmic growth of mean and standard deviation with daysPassed
            // Modify this as needed to fit your specific requirements
            int daysPassed = GameState.instance == null ? 0 : GameState.instance.day;
            float temp = Mathf.Log10((float)daysPassed + 1) * 5 + 3;
            float temp2 = Mathf.Log10((float)daysPassed + 1);
            // log temps
            // Debug.Log($"daysPassed: {daysPassed}");
            // Debug.Log($"temp: {temp}");
            // Debug.Log($"temp2: {temp2}");
            
            // eventStatCheckMean = Mathf.Clamp(Mathf.Log10(2 * daysPassed + 1) * 5 + 5, eventStatCheckMeanRange.x, eventStatCheckMeanRange.y);
            eventStatCheckMean = Mathf.Clamp(sigmoid(daysPassed), eventStatCheckMeanRange.x, eventStatCheckMeanRange.y);
            eventStatCheckStdDev = Mathf.Clamp(Mathf.Log10(daysPassed + 1), eventStatCheckStdDevRange.x, eventStatCheckStdDevRange.y);
        }

        int sigmoid(int x)
        {
            float a = 1.6f;
            float b = -0.03f;
            float k = 30f;
            return Mathf.RoundToInt(k / (1 + Mathf.Exp(a+b*x)));
        }


        void ReadDeliveryEventDataManagerToDictionary()
        {
            data = new Dictionary<EventType, Dictionary<Attribute, Dictionary<EventResult, List<string>>>>();
            foreach (var eventType in System.Enum.GetValues(typeof(EventType)))
            {
                data[(EventType)eventType] = new Dictionary<Attribute, Dictionary<EventResult, List<string>>>();
                foreach (var attribute in System.Enum.GetValues(typeof(Attribute)))
                {
                    data[(EventType)eventType][(Attribute)attribute] = new Dictionary<EventResult, List<string>>();
                    foreach(var eventResult in System.Enum.GetValues(typeof(EventResult)))
                    {
                        data[(EventType)eventType][(Attribute)attribute][(EventResult)eventResult] = new List<string>();
                    }
                }
            }

            foreach(var eventData in dedm.deliveryEventParsers)
            {

                foreach(var attributeEventData in eventData.aed)
                {
                    data[eventData.eventType][attributeEventData.attribute][EventResult.BAD] = attributeEventData.badLines;
                    data[eventData.eventType][attributeEventData.attribute][EventResult.GOOD] = attributeEventData.goodLines;
                    data[eventData.eventType][attributeEventData.attribute][EventResult.NOTHING] = attributeEventData.neutralLines;
                }
            }
        }

        public string SampleFlavorTextForAttribute(Attribute attribute, EventType eventType, EventResult eventResult)
        {
            try
            {
                var outcomes = data[eventType][attribute][eventResult];
                return outcomes[UnityEngine.Random.Range(0, outcomes.Count)];
            }
            catch (Exception e)
            {
                Debug.Log($"Exception: {e}");
                return "";
            }
        }


        public float SkillCheckProb(int playerSkill, int checkDifficulty)
        {
            // Assuming standard deviation is the same for check difficulty
            var dist = new Normal(checkDifficulty, eventStatCheckStdDev);
            // Probability of success is the cumulative distribution function evaluated at playerSkill
            return (float) dist.CumulativeDistribution(playerSkill);
        }

        public (EventResult, Attribute, string) ProcessEvent(DeliveryQuest dq, EventType eventType, Attributes party)
        {
            var successProbsDict = GameManager.instance.successProbabilities;
            if(successProbsDict.ContainsKey(dq) == false)
            {

                foreach(Character c in dq.party)
                {
                    Debug.Log($"Character: {c.name}");
                }
                Debug.Log($"Issue with the delivery quest: {dq}");
                return (EventResult.NOTHING, Attribute.Strength, "Nothing happens");
            }
                
            var successProbs = successProbsDict[dq];

            WorldStateManager wsm = WorldStateManager.instance;
            float successProb = wsm.GetEventSuccessProbability(eventType, successProbs);

            EventResult er = UnityEngine.Random.value < successProb ? EventResult.GOOD : EventResult.BAD;

            bool nothingHappens = UnityEngine.Random.value < 0.15f; // 10% chance nothing happens
            er = nothingHappens ? EventResult.NOTHING : er;

            // Sample attribute
            Attribute attributeValue = wsm.SampleAttributeAttempt(eventType, successProbs);

            string flavor = SampleFlavorTextForAttribute(attributeValue, eventType, er);
            return (er, attributeValue, flavor);
        }


        public Vector2Int GetSkillCheckRange()
        {
            return new Vector2Int((int)(eventStatCheckMean - 2 * eventStatCheckStdDev), (int)(eventStatCheckMean + 2 * eventStatCheckStdDev));
        }
    }
}
