using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
using TMPro;
using static DataParsing.NodeData;

namespace Manager
{
    public class TownNodeUIIcon : MonoBehaviour
    {
        [SerializeField] public Image imageHitbox;
        [SerializeField] public Image bgImage;
        [SerializeField] public Image mainImage;
        [SerializeField] public TextMeshProUGUI townNameText;

        [SerializeField] public Sprite[] townLevelImages;

        // Set icon data
        public void Initialize(TownLevel level, string townName)
        {
            mainImage.sprite = townLevelImages[(int)level];
            townNameText.text = townName;
        }
    }
}
