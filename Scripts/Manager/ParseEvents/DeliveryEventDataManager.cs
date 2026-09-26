using System.Collections.Generic;
using UnityEngine;
using Game;

using Attribute = Game.Attributes.Attribute;
using EventType = Manager.WorldEvent.EventType;

namespace Manager
{
    public class DeliveryEventDataManager : MonoBehaviour
    {
        [System.Serializable]
        public struct EventTypeAttributePair
        {
            public EventType eventType;
            public Attribute attribute;
            public TextAsset textAsset;
        }

        [System.Serializable]
        public struct DeliveryEventData
        {
            public EventType eventType;
            public List<AttributeEventData> aed;
        }

        [System.Serializable]
        public struct AttributeEventData
        {
            public Attribute attribute;
            
            public List<string> goodLines;
            public List<string> neutralLines;
            public List<string> badLines;
        }

        [SerializeField]
        public List<EventTypeAttributePair> textAssetPairs;
        [SerializeField]
        public List<DeliveryEventData> deliveryEventParsers;

        private void OnValidate()
        {
            Dictionary<EventType, Dictionary<Attribute, DeliveryEventParser>> parsers;
            parsers = new Dictionary<EventType, Dictionary<Attribute, DeliveryEventParser>>();


            foreach (var eventType in System.Enum.GetValues(typeof(EventType)))
            {
                parsers[(EventType)eventType] = new Dictionary<Attribute, DeliveryEventParser>();
            }

            foreach (EventTypeAttributePair pair in textAssetPairs)
            {
                DeliveryEventParser parser = new DeliveryEventParser(pair.textAsset);
                parsers[pair.eventType][pair.attribute] = parser;
            }

            // Read the dictionary into the DeliveryEventData list
            deliveryEventParsers = new List<DeliveryEventData>();
            foreach (var eventType in System.Enum.GetValues(typeof(EventType)))
            {
                var eventDict = parsers[(EventType)eventType];

                DeliveryEventData ded = new DeliveryEventData();
                ded.eventType = (EventType)eventType;

                var aeds = new List<AttributeEventData>();
                foreach (var attribute in System.Enum.GetValues(typeof(Attribute)))
                {
                    if(!eventDict.ContainsKey((Attribute)attribute))
                        continue;
                    
                    var aed = new AttributeEventData();
                    aed.attribute = (Attribute)attribute;
                    aed.goodLines = eventDict[(Attribute)attribute].goodLines;
                    aed.neutralLines = eventDict[(Attribute)attribute].neutralLines;
                    aed.badLines = eventDict[(Attribute)attribute].badLines;

                    aeds.Add(aed);
                }

                ded.aed = aeds;
                deliveryEventParsers.Add(ded);
            }
        }
    }
}
