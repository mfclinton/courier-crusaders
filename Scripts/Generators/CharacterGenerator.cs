using System.Collections;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;
using DataParsing;
using Game;
using System;
using MathNet.Numerics.Distributions;
using System.IO;

#if UNITY_EDITOR
using UnityEditor;
#endif

namespace Generators
{
    public class CharacterGenerator : MonoBehaviour
    {
       // First Names
        public TextAsset firstNamesJson;
        public FirstNameData[] firstNames;

        // Last Names
        public TextAsset lastNamesJson;
        public LastNameData[] lastNames;

        public string maleSpritesFolderPath = "Assets/Textures/UI/Characters/Male";
        public string femaleSpritesFolderPath = "Assets/Textures/UI/Characters/Female";
        public Sprite[] maleSprites;
        public Sprite[] femaleSprites;

        public Vector2Int inventorySlotsRange;

        #if UNITY_EDITOR
       private void OnValidate()
        {
            ParseFirstNames();
            ParseLastNames();
            maleSprites = LoadSpritesFromFolder(maleSpritesFolderPath);
            femaleSprites = LoadSpritesFromFolder(femaleSpritesFolderPath);
        }

        void ParseFirstNames()
        {
            firstNames = JsonReader.ParseJson<FirstNameData>(firstNamesJson).Cast<FirstNameData>().ToArray();
        }

        void ParseLastNames()
        {
            lastNames = JsonReader.ParseJson<LastNameData>(lastNamesJson).Cast<LastNameData>().ToArray();
        }


        private Sprite[] LoadSpritesFromFolder(string spritesFolderPath)
        {
            if (string.IsNullOrEmpty(spritesFolderPath))
            {
                Debug.LogError("Sprites folder path not set.");
                return null;
            }

            DirectoryInfo directoryInfo = new DirectoryInfo(spritesFolderPath);
            if (!directoryInfo.Exists)
            {
                Debug.LogError($"Directory '{spritesFolderPath}' does not exist.");
                return null;
            }

            FileInfo[] spriteFiles = directoryInfo.GetFiles("*.png", SearchOption.TopDirectoryOnly);
            
            Sprite[] spriteArray = new Sprite[spriteFiles.Length];
            for (int i = 0; i < spriteFiles.Length; i++)
            {
                string filePath = spriteFiles[i].FullName;
                string assetPath = filePath.Substring(filePath.IndexOf("Assets"));

                Sprite sprite = UnityEditor.AssetDatabase.LoadAssetAtPath<Sprite>(assetPath);
                if(sprite == null)
                    Debug.LogWarning("Failed to load sprite from path: " + assetPath);

                spriteArray[i] = sprite;
            }

            return spriteArray;
        }
        #endif

        public (string, string, Game.Character.Gender) GenNames()
        {
            FirstNameData firstName = firstNames[UnityEngine.Random.Range(0, firstNames.Length)];
            LastNameData lastName = lastNames[UnityEngine.Random.Range(0, lastNames.Length)];
            var gender = Enum.Parse<Game.Character.Gender>(firstName.gender.ToUpper());

            return (firstName.first_name, lastName.last_name, gender);
        }


        public Attributes GenAttributes()
        {
            int mu = 5;
            float sigma = 2.3f;
            int lowerBound = 1;
            int upperBound = 10;

            Normal normalDist = Normal.WithMeanStdDev(mu, sigma);
            Func<int> sampleNormal = () => Mathf.RoundToInt(Mathf.Clamp((float) normalDist.Sample(), lowerBound, upperBound));

            return new Attributes(
                sampleNormal(),
                sampleNormal(),
                sampleNormal(),
                sampleNormal(),
                sampleNormal()
            );
        }


        public Character GenerateCharacter()
        {
            var (firstName, lastName, gender) = GenNames();
            string name = $"{firstName} {lastName}";
            Attributes attributes = GenAttributes();
            
            Sprite[] genderSprites = gender == Character.Gender.MALE ? maleSprites : femaleSprites;
            Sprite sprite = genderSprites[UnityEngine.Random.Range(0, genderSprites.Length)];

            int inventorySlots = UnityEngine.Random.Range(inventorySlotsRange.x, inventorySlotsRange.y + 1);

            return new Character(attributes, name, gender, sprite, inventorySlots);
        }
    }
}
