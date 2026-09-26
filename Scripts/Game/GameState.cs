using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace Game
{
    public class GameState
    {
        public LinkedList<Character> characters { get; private set; }
        public LinkedList<Equipment> equipment { get; private set; }
        public LinkedList<DeliveryQuest> activeDeliveries { get; private set; }
        public LinkedList<Character> formingParty { get; private set; }
        public int day { get; private set; }
        public int money { get; private set; }
        public int upkeep { get; private set; }
        public Shop shop { get; private set;}
        
        // TODO: Deliveries
        // TODO: WorldState
        // TODO: Upkeep
        public Delivery availableDelivery { get; private set; }

        public delegate void OnStateModifiedDelegate(GameState state, Character c);
        public event OnStateModifiedDelegate onStateModifiedInvoked;

        // Delegates for updating formingParty
        public delegate void OnFormingPartyModifiedDelegate(LinkedList<Character> formingParty);
        public event OnFormingPartyModifiedDelegate onFormingPartyModifiedInvoked;

        // Delegates for adding ActiveDelivery
        public delegate void OnActiveDeliveryAddedDelegate(DeliveryQuest dq);
        public event OnActiveDeliveryAddedDelegate onActiveDeliveryAddedInvoked;

        // Delegates for removing ActiveDelivery
        public delegate void OnActiveDeliveryRemovedDelegate(DeliveryQuest dq);
        public event OnActiveDeliveryRemovedDelegate onActiveDeliveryRemovedInvoked;

        Func<Delivery> deliveryGenerator;
        PathPlanner pathPlanner;

        // static references
        public static GameState instance { get; private set; }
        public static Character lastSelectedCharacter { get; set; }

        // fun stats
        public int numCharactersRecruited { get; set; }
        public int numEquipmentPurchased { get; set; }
        public int numDeliveriesCompleted { get; set; }
        public int totalMoneyEarned { get; set; }

        // Party size range
        public Vector2Int partySizeRange { get; private set; }
        public int partyLimit { get; private set; }

        // Constructor
        public GameState(Func<Character> characterGenerator, Func<Equipment> equipmentGenerator, Func<Delivery> deliveryGenerator, Vector2Int partySizeRange, int money = 100, int upkeep = 5)
        {
            instance = this;
            characters = new LinkedList<Character>();
            equipment = new LinkedList<Equipment>();
            activeDeliveries = new LinkedList<DeliveryQuest>();
            formingParty = new LinkedList<Character>();
            
            this.deliveryGenerator = deliveryGenerator;
            this.pathPlanner = PathPlanner.instance;
            
            this.partySizeRange = partySizeRange;
            partyLimit = partySizeRange.x;

            this.day = 1;
            this.money = money;
            this.upkeep = upkeep;
            SampleAvailableDelivery();
            shop = new Shop(this, characterGenerator, equipmentGenerator);

            // fun stats
            numCharactersRecruited = 0;
            numEquipmentPurchased = 0;
            numDeliveriesCompleted = 0;
            totalMoneyEarned = 0;
        }

        // Get Total Characters Count
        public int GetTotalCharactersCount()
        {
            int questingCharactersCount = 0;
            foreach(DeliveryQuest dq in activeDeliveries)
                questingCharactersCount += dq.party.Count;

            return characters.Count + formingParty.Count + questingCharactersCount;
        }

        // Sample Available Delivery
        public void SampleAvailableDelivery()
        {
            availableDelivery = deliveryGenerator();
            pathPlanner.Initialize(availableDelivery.startLocation, availableDelivery.endLocation);
        }

        public void ResetAvailableDelivery()
        {
            pathPlanner.Initialize(availableDelivery.startLocation, availableDelivery.endLocation);
        }

        // UI refresh
        public void OnStateModified(Character selected = null)
        {
            onStateModifiedInvoked?.Invoke(this, selected);
        }

        // Helpers
        public LinkedList<Character> GetAvailableCharacters()
        {
            LinkedList<Character> freeCharacters = new LinkedList<Character>(characters);
            foreach(DeliveryQuest dq in activeDeliveries)
                foreach(Character c in dq.party)
                    freeCharacters.Remove(c);
            
            return freeCharacters;
        }

        public LinkedList<Character> GetWaitingCharacters()
        {
            LinkedList<Character> availableCharacters = GetAvailableCharacters();
            LinkedList<Character> waitingCharacters = new LinkedList<Character>(availableCharacters);
            foreach(Character c in formingParty)
                waitingCharacters.Remove(c);
            
            return waitingCharacters;
        }

        public void ModifyMoney(int delta)
        {
            money += delta;
        }

        // Functions for modifying the game state
        public void AddEntity(Entity e)
        {
            if (e is Character)
            {
                characters.AddLast((Character)e);
            }
            else if (e is Equipment)
            {
                equipment.AddLast((Equipment)e);
            }
        }

        public void RemoveEntity(Entity e)
        {
            if (e is Character)
            {
                Character c = (Character)e;
                characters.Remove(c);

                // Return item to inventory
                foreach(Equipment eq in c.equipment)
                    equipment.AddLast(eq);
            }
            else if (e is Equipment)
                equipment.Remove((Equipment)e);
        }

        // Add Character To Party
        public void AddCharacterToParty(Character c)
        {
            bool removed = characters.Remove(c);
            if(removed)
            {
                formingParty.AddLast(c);
                c.partyIndex = formingParty.Count - 1;
                onStateModifiedInvoked.Invoke(this, c);
                onFormingPartyModifiedInvoked.Invoke(formingParty);
            }
        }

        public void RemoveCharacterFromParty(Character c)
        {
            bool removed = formingParty.Remove(c);
            if(removed)
            {
                characters.AddLast(c);
                onStateModifiedInvoked.Invoke(this, c);
                onFormingPartyModifiedInvoked.Invoke(formingParty);
            }
        }

        public void ClearParty()
        {
            formingParty = new LinkedList<Character>();
            onFormingPartyModifiedInvoked.Invoke(formingParty);
        }

        public void AddDelivery(DeliveryQuest dq)
        {
            activeDeliveries.AddLast(dq);
            onActiveDeliveryAddedInvoked.Invoke(dq);
        }

        public void RemoveDelivery(DeliveryQuest dq)
        {
            foreach(Character c in dq.party)
                characters.AddLast(c);

            activeDeliveries.Remove(dq);
            onActiveDeliveryRemovedInvoked.Invoke(dq);
        }

        public void EquipItemToSelectedCharacter(Equipment e)
        {
            if(lastSelectedCharacter.equipment.Count >= lastSelectedCharacter.equipmentCapacity)
                return;

            bool removed = equipment.Remove(e);
            if(removed)
            {
                lastSelectedCharacter.equipment.AddLast(e);
                onStateModifiedInvoked.Invoke(this, lastSelectedCharacter);
            }
        }

        public void UnequipItemFromSelectedCharacter(Equipment e)
        {
            bool removed = lastSelectedCharacter.equipment.Remove(e);
            if(removed)
            {
                equipment.AddLast(e);
                onStateModifiedInvoked.Invoke(this, lastSelectedCharacter);
            }
        }

        public void UpdateUpkeep()
        {
            // Update upkeep with a probability
            float rng = UnityEngine.Random.value;
            if(rng < .1f)
                upkeep += 2;
            else if(rng < .6f)
                upkeep -= 2;

            upkeep = Mathf.Clamp(upkeep, 1, 40);
        }

        public void AdvanceDay()
        {
            ProcessDeliveryUpdates();
            day++;
            ModifyMoney(-upkeep);
            shop.UpdateShop();
            UpdateUpkeep();
            SampleAvailableDelivery();
            onStateModifiedInvoked.Invoke(this, lastSelectedCharacter);
        }

        void ProcessDeliveryUpdates()
        {
            LinkedList<DeliveryQuest> completedDeliveries = new LinkedList<DeliveryQuest>();

            foreach(DeliveryQuest dq in activeDeliveries)
            {
                dq.PassDay();

                bool success = dq.IsComplete();
                bool failure = dq.IsFailed();
                bool questOver = success || failure;

                if(questOver)
                    completedDeliveries.AddLast(dq);

                if(success)
                {
                    numDeliveriesCompleted++;
                    totalMoneyEarned += dq.delivery.reward;

                    ModifyMoney(dq.delivery.reward);
                }
            }

            foreach(DeliveryQuest dq in completedDeliveries)
                RemoveDelivery(dq);
        }

        public void StartDelivery()
        {
            if(formingParty.Count > 0 && pathPlanner.PathComplete())
            {
                DeliveryQuest dq = new DeliveryQuest(availableDelivery, formingParty, pathPlanner.Path);
                AddDelivery(dq);
                ClearParty();
                onStateModifiedInvoked.Invoke(this, lastSelectedCharacter);
            }
        }
    }
}
