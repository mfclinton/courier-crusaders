using System.Collections.Generic;
using UnityEngine;

namespace Manager
{
    [System.Serializable]
    public class DeliveryEventParser
    {
        public TextAsset textFile;
        public List<string> goodLines = new List<string>();
        public List<string> neutralLines = new List<string>();
        public List<string> badLines = new List<string>();

        public DeliveryEventParser(TextAsset textAsset)
        {
            textFile = textAsset;
            ParseTextAsset(textFile);
        }

        private void ParseTextAsset(TextAsset textAsset)
        {
            if(textAsset == null)
            {
                Debug.LogError("Text asset is null");
                return;
            }
            // Debug.Log("Parsing text asset: " + textAsset.name);
            string[] lines = textAsset.text.Split(new[] { '\r', '\n' }, System.StringSplitOptions.RemoveEmptyEntries);
            // Debug.Log("Done");

            foreach (string line in lines)
            {
                if (line.StartsWith("Good:"))
                {
                    goodLines.Add(line.Substring(5).Trim());
                }
                else if (line.StartsWith("Neutral:"))
                {
                    neutralLines.Add(line.Substring(8).Trim());
                }
                else if (line.StartsWith("Bad:"))
                {
                    badLines.Add(line.Substring(4).Trim());
                }
            }
        }

        // Add accessor methods to get the lists if needed
    }
}
