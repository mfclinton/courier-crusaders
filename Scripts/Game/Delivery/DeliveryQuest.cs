using System.Collections;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;
using UIWidgets;

namespace Game
{
    public class DeliveryQuest
    {
        public Delivery delivery { get; private set; }
        public LinkedList<Character> party { get; private set; }
        public List<Node> nodePath { get; private set;}
        public List<Edge> edgePath { get; private set; }
        public float distanceMoved { get; private set; }
        public List<(string, int)> eventTexts { get; private set; }

        public int daysDelayed { get; set; }

        float speed;
        public float totalDistance { get; private set; }


        public enum Status {
            CASH,
            LEVELUP,
            WAITING,
            DEAD
        }

        public (Character, Status, int)? characterStatus { get; private set; }

        public DeliveryQuest(Delivery delivery, LinkedList<Character> party, List<Node> nodePath)
        {
            this.delivery = delivery;
            this.party = party;

            this.nodePath = nodePath;
            this.edgePath = PathPlanner.ConvertNodesToEdges(nodePath);

            distanceMoved = 0;
            totalDistance = this.edgePath.Sum(e => e.Distance);
            speed = CalculateSpeed();

            eventTexts = new List<(string, int)>();

            daysDelayed = 0;

            ResetCharacterStatus();
        }


        public void SetCharacterStatus(Character character, Status status, int value = -1)
        {
            characterStatus = (character, status, value);
        }

        public void ResetCharacterStatus()
        {
            characterStatus = null;
        }

        public void ProccessAddingEventText(string text, int eventResultValue)
        {
            eventTexts.Add((text, eventResultValue));
        }


        public float CalculateSpeed()
        {
            if(party == null || party.Count == 0)
                return 0;
                
            int averageDex = party.Select(c => c.GetAttributes().dexterity).Sum() / party.Count;

            int extraMembers = party.Count - 1;
            float extraPartyBonus = 1f;
            if(0 < extraMembers)
            {
                extraPartyBonus = 1f + Mathf.Log10(extraMembers + 1);
            }

            float speed = (averageDex / 5f) * extraPartyBonus;
            return speed;
        }

        // Estimate the number of days of travel
        public int EstimateDays()
        {
            return Mathf.CeilToInt((totalDistance - distanceMoved) / speed);
        }

        public float CalculateProgress()
        {
            return distanceMoved / totalDistance;
        }

        public void Move()
        {
            distanceMoved += speed;
        }

        public bool IsComplete()
        {
            return distanceMoved >= totalDistance;
        }

        public bool IsFailed()
        {
            return party.Count == 0 || delivery.deadline <= 0;
        }

        public void PassDay()
        {
            if(daysDelayed <= 0)
            {
                Move();
                ResetCharacterStatus();
            }
            else
            {
                daysDelayed--;
            }
            delivery.PassDay();
        }

        public (Node currentNode, float percentCompleted, Vector2 relativePosition) GetPathProgress()
        {
            // Calculate the total distance of the path
            float totalDistance = 0;
            foreach (Edge edge in edgePath)
            {
                totalDistance += edge.Distance;
            }

            // Calculate the percent completed
            float percentCompleted = distanceMoved / totalDistance;

            // Find the pair of nodes between which the distance lies
            int currentEdgeIndex = 0;
            float remainingDistance = distanceMoved;
            while (currentEdgeIndex < edgePath.Count)
            {
                float currentSegmentDistance = edgePath[currentEdgeIndex].Distance;
                if (remainingDistance <= currentSegmentDistance)
                {
                    break;
                }
                remainingDistance -= currentSegmentDistance;
                currentEdgeIndex++;
            }

            if(currentEdgeIndex == edgePath.Count)
                return (null, percentCompleted, nodePath.Last().RelativePosition);

            // Calculate the relative position on the UI image
            Node currentNode = nodePath[currentEdgeIndex];
            Node nextNode = nodePath[currentEdgeIndex + 1];
            
            float interpolationFactor = remainingDistance / edgePath[currentEdgeIndex].Distance;
            Vector2 relativePosition = Vector2.Lerp(currentNode.RelativePosition, nextNode.RelativePosition, interpolationFactor);

            return (currentNode, percentCompleted, relativePosition);
        }


        public static int EstimateDeliveryDays(Delivery delivery, LinkedList<Character> party, List<Node> nodePath)
        {
            int days;

            if(1 < nodePath.Count && 0 < party.Count)
            {
                DeliveryQuest tempQuest = new DeliveryQuest(delivery, party, nodePath);
                days = tempQuest.EstimateDays();
            }
            else
                days = 0;

            return days;
        }
    }
}
