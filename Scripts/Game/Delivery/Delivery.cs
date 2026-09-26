using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace Game
{
    public class Delivery
    {
        // TODO: location of delivery
        public Node startLocation { get; private set; }
        public Node endLocation { get; private set; }
        public string note { get; private set; }
        public int deadline { get; private set; }
        public int reward { get; private set; }

        public Delivery(Node startLocation, Node endLocation, string note, int deadline, int reward)
        {
            this.startLocation = startLocation;
            this.endLocation = endLocation;
            this.note = note;
            this.deadline = deadline;
            this.reward = reward;
        }

        public void PassDay()
        {
            deadline--;
        }

    }
}
