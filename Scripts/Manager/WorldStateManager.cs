using System.Collections.Generic;
using System.Linq;
using Game;
using UnityEngine;
using static Game.Attributes;

namespace Manager {
    [RequireComponent(typeof(QuestCalculator))]
    public class WorldStateManager : MonoBehaviour
    {
        [SerializeField] private string[] questFlavorText;
        [SerializeField] private GameObject[] worldEventIconPrefabs;
        [SerializeField] private GameObject characterGridIconPrefab;
        [SerializeField] private UICharacterIcon characterIconPrefab;
        [SerializeField] public List<WorldEvent> activeWorldEvents { get; private set;}
        [SerializeField] public Sprite[] characterStatusSprites;


        public Dictionary<Edge, List<WorldEvent>> edgeToEvents { get; private set; } // For Quick Reference
        
        [Header("Event Sampling Parameters")]
        [SerializeField] private Vector2Int lengthOfEventsRange;
        [SerializeField] private Vector2Int numEventRange;

        public static WorldStateManager instance;
        public int daysLeftForEvents { get; private set; }
        public float uiEventEdgeMaxTownDist = 0.5f;
        
        QuestCalculator questCalculator;

        private void Awake() {
            instance = this;
            questCalculator = GetComponent<QuestCalculator>();
            
            activeWorldEvents = new List<WorldEvent>();
            NewEventsWave();
        }

        public WorldEvent GenerateWorldEvent()
        {
            // Generate event type uniformly
            int eventTypeCount = System.Enum.GetValues(typeof(WorldEvent.EventType)).Length;
            WorldEvent.EventType eventType = (WorldEvent.EventType) UnityEngine.Random.Range(0, eventTypeCount);

            // Set iconPrefab for the event type
            GameObject iconPrefab = worldEventIconPrefabs[(int)eventType];
            
            List<Node> nodes = MapManager.instance.nodes.Values.ToList();

            // Gets a random edge
            Node randomNode = nodes[UnityEngine.Random.Range(0, nodes.Count)];
            Edge randomEdge = randomNode.Edges[UnityEngine.Random.Range(0, randomNode.Edges.Count)];

            // Create a new WorldEvent instance and return it
            return new WorldEvent(eventType, randomEdge, iconPrefab);
        }

        public Delivery SampleDeliveryEvent()
        {
            MapManager mm = MapManager.instance;

            // Uniformely sample a start and end node
            int startIndex = UnityEngine.Random.Range(0, mm.nodes.Count);
            int endIndex;
            do
            {
                endIndex = UnityEngine.Random.Range(0, mm.nodes.Count);
            }
            while (endIndex == startIndex);
            
            Node start = mm.nodes.Values.ElementAt(startIndex);
            Node end = mm.nodes.Values.ElementAt(endIndex);
            mm.InitializePathPlanner(start, end);

            // Calculate the minimum number of edges between the start and end nodes
            float minNumEdges = (float) PathPlanner.CalculateDistance(start, end);

            int numDays = questCalculator.CalculateMaxDays(minNumEdges);
            int totalReward = questCalculator.CalculateTotalReward(minNumEdges);

            // TODO: Flavour text
            string flavor = questFlavorText[UnityEngine.Random.Range(0, questFlavorText.Length)];
            return new Delivery(start, end, flavor, numDays, totalReward);
        }

        public void ProcessNewDay()
        {
            PlaceQuestingCharacters();

            if(daysLeftForEvents <= 0)
            {
                NewEventsWave();
                GameManager.instance.UpdateAllSuccessProbabilities();
                GameManager.instance.UpdateFormingPartySuccessProbability(GameState.instance.formingParty);
            }
            else
            {
                daysLeftForEvents--;
            }
        }

        public void NewEventsWave()
        {
            // Pass a day for each active world event
            for (int i = activeWorldEvents.Count - 1; i >= 0; i--)
            {
                activeWorldEvents[i].EndEvent();
            }

            activeWorldEvents = new List<WorldEvent>();

            // initialize edgeToEvents dictionary
            edgeToEvents = new Dictionary<Edge, List<WorldEvent>>();
            
            int num_events = UnityEngine.Random.Range(numEventRange.x, numEventRange.y + 1);
            for (int i = 0; i < num_events; i++)
            {
                WorldEvent newWorldEvent = GenerateWorldEvent();
                activeWorldEvents.Add(newWorldEvent);

                // add to edge to events dictionary
                if(!edgeToEvents.ContainsKey(newWorldEvent.edge))
                    edgeToEvents[newWorldEvent.edge] = new List<WorldEvent>();
                
                edgeToEvents[newWorldEvent.edge].Add(newWorldEvent);
            }

            // get the number of days left for events
            daysLeftForEvents = UnityEngine.Random.Range(lengthOfEventsRange.x, lengthOfEventsRange.y + 1);
            DeliveryEventGenerator.instance.UpdateStats();
        }

        public WorldEvent SampleWorldEvent(DeliveryQuest dq, Node curNode, float percentCompleted, Vector2 relativePosition, Node prevCurNode, float prevPercentCompleted, Vector2 prevRelativePosition)
        {
            List<WorldEvent> possibleEvents = new List<WorldEvent>();
            foreach(WorldEvent we in activeWorldEvents)
            {
                (bool eventOnPath, float eventT) = we.CalculateQuestT(dq);
                if(eventOnPath && (prevPercentCompleted <= eventT && eventT <= percentCompleted))
                {
                    possibleEvents.Add(we);
                }
            }

            if(possibleEvents.Count == 0)
                return null;

            WorldEvent selectedEvent = possibleEvents[UnityEngine.Random.Range(0, possibleEvents.Count)];
            return selectedEvent;
        }

        public void PlaceQuestingCharacters()
        {
            MapManager mapManager = MapManager.instance;
            var activeDeliveries = GameState.instance.activeDeliveries;

            mapManager.ClearMapLayer(MapManager.MapLayer.PLAYER);

            foreach(DeliveryQuest dq in activeDeliveries)
            {
                (_,_, Vector2 relativePartyPosition) = dq.GetPathProgress();
                Transform partyGrid = mapManager.CreateIcon(characterGridIconPrefab, relativePartyPosition, MapManager.MapLayer.PLAYER, true).transform;
                
                var characterStatus = dq.characterStatus;
                bool activeCharacterStatus = characterStatus != null;

                int index = 0;
                foreach(Character c in dq.party)
                {
                    UICharacterIcon icon = ObjectPooler.Instance.SpawnFromPool(characterIconPrefab.gameObject, parent: partyGrid, childIndex:index).GetComponent<UICharacterIcon>();
                    icon.Initialize(c);
                    index++;

                    if(activeCharacterStatus && c == characterStatus.Value.Item1)
                        icon.SetCharacterStatus(characterStatusSprites[(int) characterStatus.Value.Item2], characterStatus.Value.Item3);
                }

                // if characterStatus is dead
                if(activeCharacterStatus && characterStatus.Value.Item2 == DeliveryQuest.Status.DEAD)
                {
                    Character c = characterStatus.Value.Item1;

                    UICharacterIcon icon = ObjectPooler.Instance.SpawnFromPool(characterIconPrefab.gameObject, parent: partyGrid, childIndex: c.partyIndex).GetComponent<UICharacterIcon>();
                    icon.Initialize(c);
                    icon.SetCharacterStatus(characterStatusSprites[(int) characterStatus.Value.Item2], characterStatus.Value.Item3);

                    UIDeliveryStatus.FixCharacterIndexes(c, dq);
                }
            }
        }

        // Given a delivery quest, and a party, calculate the success probability
        public Dictionary<WorldEvent.EventType, Dictionary<Attribute, float>> CalculateSuccessProbability(LinkedList<Character> party)
        {
            if(party.Count == 0)
                return null;

            DeliveryEventGenerator deg = DeliveryEventGenerator.instance;
            Dictionary<WorldEvent.EventType, Dictionary<HashSet<Attribute>, float>> eventToAttrSubsetProbsDict = deg.eventToAttrSubsetProbsDict;
            Dictionary<WorldEvent.EventType, Dictionary<HashSet<Attribute>, Dictionary<Attribute, float>>> eventToAttrChoiceProbsDict = deg.CreateEventToAttrChoiceProbsDict(party);
            float[] skillCheckProbs = GetSkillCheckProbs(party);

            Dictionary<WorldEvent.EventType, Dictionary<Attribute, float>> successProbability = new Dictionary<WorldEvent.EventType, Dictionary<Attribute, float>>();
    
            // Initialize attrChoiceProbability
            foreach (WorldEvent.EventType eventType in System.Enum.GetValues(typeof(WorldEvent.EventType)))
            {
                successProbability[eventType] = new Dictionary<Attribute, float>();
                foreach (Attribute attribute in System.Enum.GetValues(typeof(Attribute)))
                {
                    successProbability[eventType][attribute] = 0;
                }
            }

            // Update probabilities
            foreach (WorldEvent.EventType eventType in System.Enum.GetValues(typeof(WorldEvent.EventType)))
            {                
                foreach (var attributeSubset in eventToAttrSubsetProbsDict[eventType])
                {
                    float subsetProbability = attributeSubset.Value;

                    foreach (var attribute in attributeSubset.Key)
                    {
                        float choiceProb = eventToAttrChoiceProbsDict[eventType][attributeSubset.Key][attribute];
                        float passSkillCheckProb = skillCheckProbs[(int)attribute];

                        successProbability[eventType][attribute] += subsetProbability * choiceProb * passSkillCheckProb;
                    }
                }
            }

            return successProbability;
        }

        public float[] GetSkillCheckProbs(LinkedList<Character> party)
        {
            int numAttributes = System.Enum.GetValues(typeof(Attributes.Attribute)).Length;
            DeliveryEventGenerator deg = DeliveryEventGenerator.instance;

            // Get Stat check info
            float eventStatCheckMean = deg.eventStatCheckMean;
            int roundedStatCheckMean = Mathf.RoundToInt(deg.eventStatCheckMean);

            float eventStatCheckStdDev = deg.eventStatCheckStdDev;

            // Get total party stats
            Attributes totalAttributes = new Attributes(0, 0, 0, 0, 0);
            foreach(Character c in party)
            {
                Attributes a = c.GetAttributes();
                totalAttributes.AddAttributes(a);
            }

            float[] skillCheckProbs = new float[numAttributes];
            for(int i = 0; i < numAttributes; i++)
            {
                Attributes.Attribute attribute = (Attributes.Attribute)i;
                
                // Skill Check Probability
                float skillCheckP = deg.SkillCheckProb(totalAttributes.GetAttribute(attribute), roundedStatCheckMean);
                skillCheckProbs[i] = skillCheckP;
            }

            return skillCheckProbs;
        }

        public float GetEventSuccessProbability(WorldEvent.EventType et, Dictionary<WorldEvent.EventType, Dictionary<Attribute, float>> successProbability)
        {
            return successProbability[et].Values.Sum();
        }

        public Attribute SampleAttributeAttempt(WorldEvent.EventType et, Dictionary<WorldEvent.EventType, Dictionary<Attribute, float>> successProbability)
        {
            float rng = UnityEngine.Random.value;
            float cumulativeWeight = 0.0f;
            Attribute attribute = Attribute.Strength;

            foreach (var kvp in successProbability[et])
            {
                cumulativeWeight += kvp.Value;

                if (rng <= cumulativeWeight)
                {
                    attribute = kvp.Key;
                    break;
                }
            }

            return attribute;
        }
    }
}