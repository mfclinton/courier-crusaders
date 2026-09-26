using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
using TMPro;
using UIWidgets;
using Game;
using System.Linq;

namespace Manager
{
    public class UIDeliveryStatus : MonoBehaviour
    {
        [SerializeField] private TextMeshProUGUI headerText;
        [SerializeField] private TextMeshProUGUI rewardText;
        [SerializeField] private Slider progressSlider;

        [SerializeField] private ScrollRect scrollRect;
        [SerializeField] private UITextElement textElementPrefab;
        [SerializeField] private Transform textParent;
        [SerializeField] private Color[] textColors;


        [Header("Character Icons")]
        [SerializeField] private UICharacterIcon characterIconPrefab;
        [SerializeField] private Transform characterIconParent;

        [Header("Completed Object")]
        [SerializeField] private GameObject completedObject;
        [SerializeField] private TextMeshProUGUI deliveryCompletedText;

        public DeliveryQuest dq { get; private set; }
        int lastTextCount;

        public void Initialize(DeliveryQuest dq)
        {
            this.dq = dq;
            completedObject.SetActive(false);
            UpdateUI();
        }


        public void UpdateUI()
        {
            Delivery d = dq.delivery;
            headerText.text = d.note;
            rewardText.text = d.reward.ToString();
            progressSlider.value = dq.CalculateProgress();
            UpdateCharacterIcons();

            UpdateTexts();
        }

        public void OnNewDay()
        {
            if(lastTextCount != dq.eventTexts.Count)
            {
                scrollRect.normalizedPosition = new Vector2(0, 0);
                scrollRect.StopMovement();
            }
            lastTextCount = dq.eventTexts.Count;
        }

        private IEnumerator ResetScrollRect()
        {
            // Set the position first
            scrollRect.normalizedPosition = new Vector2(0, 0);

            // Wait for one frame
            yield return null;

            // Now set the velocity to zero
            scrollRect.velocity = Vector2.zero;
        }

        public void UpdateTexts()
        {
            ObjectPooler op = ObjectPooler.Instance;
            
            // Clear existing texts
            foreach (Transform child in textParent)
            {
                op.ReturnToPool(child.gameObject);
            }

            // Add new texts
            int index = 0;
            foreach ((string s, int i) in dq.eventTexts)
            {
                UITextElement textElement = op.SpawnFromPool(textElementPrefab.gameObject, parent: textParent, childIndex: index).GetComponent<UITextElement>();
                textElement.text.text = s;
                textElement.text.color = textColors[i];

                if(i != (int) DeliveryEventGenerator.EventResult.NOTHING)
                {
                    // center the text
                    textElement.text.alignment = TextAlignmentOptions.Center;
                    // make the text bold
                    textElement.text.fontStyle = FontStyles.Bold;
                }
                else
                {
                    // left align the text
                    textElement.text.alignment = TextAlignmentOptions.Left;
                    // make the text normal
                    textElement.text.fontStyle = FontStyles.Normal;
                }

                index++;
            }
        }

        void UpdateCharacterIcons()
        {
            // Clear existing icons
            foreach (Transform child in characterIconParent)
            {
                ObjectPooler.Instance.ReturnToPool(child.gameObject);
            }

            var characterStatus = dq.characterStatus;
            bool activeCharacterStatus = characterStatus != null;
            Sprite[] characterStatusSprites = WorldStateManager.instance.characterStatusSprites;

            int index = 0;
            foreach (Character c in dq.party)
            {
                UICharacterIcon icon = ObjectPooler.Instance.SpawnFromPool(characterIconPrefab.gameObject, parent: characterIconParent, childIndex: index).GetComponent<UICharacterIcon>();
                icon.Initialize(c);
                index++;
                
                if(activeCharacterStatus && c == characterStatus.Value.Item1)
                    icon.SetCharacterStatus(characterStatusSprites[(int) characterStatus.Value.Item2], characterStatus.Value.Item3);
            }

            // if characterStatus is dead
            if(activeCharacterStatus && characterStatus.Value.Item2 == DeliveryQuest.Status.DEAD)
            {
                Character c = characterStatus.Value.Item1;

                UICharacterIcon icon = ObjectPooler.Instance.SpawnFromPool(characterIconPrefab.gameObject, parent: characterIconParent, childIndex: c.partyIndex).GetComponent<UICharacterIcon>();
                icon.Initialize(c);
                icon.SetCharacterStatus(characterStatusSprites[(int) characterStatus.Value.Item2], characterStatus.Value.Item3);

                FixCharacterIndexes(c, dq);
            }
        }

        public static void FixCharacterIndexes(Character killedCharacter, DeliveryQuest dq)
        {
            // Fix other character indexes
            LinkedListNode<Character> currentNode = dq.party.First;
            int currentIndex = 0;
            while (currentNode != null && currentIndex < killedCharacter.partyIndex)
            {
                currentNode = currentNode.Next;
                currentIndex++;
            }

            while (currentNode != null)
            {
                currentNode.Value.partyIndex--;
                currentNode = currentNode.Next;
            }
        }

        public bool HandleQuestCompletion()
        {
            bool success = dq.IsComplete();
            bool failure = dq.IsFailed();
            bool questOver = success || failure;

            if(!questOver)
                return false;

            completedObject.SetActive(true);
            foreach (Transform child in characterIconParent)
            {
                ObjectPooler.Instance.ReturnToPool(child.gameObject);
            }

            if(success)
            {
                progressSlider.value = 1;
                deliveryCompletedText.text = "Delivery Success";
            }
            else
            {
                deliveryCompletedText.text = "Delivery Failed";
            }

            if(failure && dq.characterStatus != null)
            {
                if(dq.characterStatus.Value.Item2 == DeliveryQuest.Status.DEAD)
                {
                    Character c = dq.characterStatus.Value.Item1;

                    UICharacterIcon icon = ObjectPooler.Instance.SpawnFromPool(characterIconPrefab.gameObject, parent: characterIconParent, childIndex: c.partyIndex).GetComponent<UICharacterIcon>();
                    icon.Initialize(c);
                    icon.SetCharacterStatus(WorldStateManager.instance.characterStatusSprites[(int) dq.characterStatus.Value.Item2], dq.characterStatus.Value.Item3);
                }
            }

            return true;
        }

        public void DeleteStatusClicked()
        {
            UIManager.instance.deliveryStatuses.Remove(dq);
            ObjectPooler.Instance.ReturnToPool(gameObject);
        }
    }
}
