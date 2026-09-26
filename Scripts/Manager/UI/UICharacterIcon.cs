using System.Collections;
using System.Collections.Generic;
using Game;
using UnityEngine;
using UnityEngine.UI;
using TMPro;

namespace Manager
{
    public class UICharacterIcon : MonoBehaviour
    {
        [SerializeField] private Image icon;
        [SerializeField] private TextMeshProUGUI nameText;

        [Header("Status Icons")]
        [SerializeField] private GameObject statusParent;
        [SerializeField] private Image statusImage;

        public void Initialize(Character c)
        {
            icon.sprite = c.sprite;

            // first name only
            string[] names = c.name.Split(' ');
            nameText.text = names[0];

            // Set status icon
            statusParent.SetActive(false);

            // Cash
            // LeveledUp
            // Waiting
            // Dead

            // if(c.status != Character.Status.NONE)
            // {
            //     statusParent.SetActive(true);
            //     statusImage.sprite = c.statusIcon;
            // }
            // else
            // {
            //     statusParent.SetActive(false);
            // }
        }

        public void SetCharacterStatus(Sprite sprite, int number = -1)
        {
            statusParent.SetActive(true);
            statusImage.sprite = sprite;
        }
    }
}
