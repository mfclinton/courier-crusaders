using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace Game
{
    public class Attributes
    {
        public enum Attribute
        {
            Strength,
            Constitution,
            Dexterity,
            Intelligence,
            Charisma
        }

        public int strength { get; private set; }
        public int constitution { get; private set; }
        public int dexterity { get; private set; }
        public int intelligence { get; private set; }
        public int charisma { get; private set; }

        // A constructor
        public Attributes(int strength, int constitution, int dexterity, int intelligence, int charisma)
        {
            this.strength = strength;
            this.constitution = constitution;
            this.dexterity = dexterity;
            this.intelligence = intelligence;
            this.charisma = charisma;
        }

        public IEnumerator<int> GetEnumerator()
        {
            yield return strength;
            yield return constitution;
            yield return dexterity;
            yield return intelligence;
            yield return charisma;
        }

        public void AddAttributes(Attributes attributes)
        {
            strength += attributes.strength;
            constitution += attributes.constitution;
            dexterity += attributes.dexterity;
            intelligence += attributes.intelligence;
            charisma += attributes.charisma;
        }

        public double[] GetAttributes()
        {
            return new double[] { strength, constitution, dexterity, intelligence, charisma };
        }

        public int GetAttribute(Attribute a)
        {
            switch (a)
            {
                case Attribute.Strength:
                    return strength;
                case Attribute.Constitution:
                    return constitution;
                case Attribute.Dexterity:
                    return dexterity;
                case Attribute.Intelligence:
                    return intelligence;
                case Attribute.Charisma:
                    return charisma;
                default:
                    return 0;
            }
        }

        // generate a random attribute vector
        public static Attributes RandomAttributes(int max)
        {
            int maxExclusive = max + 1;
            int strength = Random.Range(0, maxExclusive);
            int constitution = Random.Range(0, maxExclusive);
            int dexterity = Random.Range(0, maxExclusive);
            int intelligence = Random.Range(0, maxExclusive);
            int charisma = Random.Range(0, maxExclusive);
            return new Attributes(strength, constitution, dexterity, intelligence, charisma);
        }
    }
}
