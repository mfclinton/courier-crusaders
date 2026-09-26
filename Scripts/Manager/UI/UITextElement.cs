using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
using TMPro;

namespace Manager
{
    public class UITextElement : MonoBehaviour
    {
        // TMP_Text text;
        public TextMeshProUGUI text;

        void Awake() {
            RectTransform parentRectTransform = GetComponent<RectTransform>();
            RectTransform childRectTransform = text.GetComponent<RectTransform>();

            parentRectTransform.sizeDelta = childRectTransform.sizeDelta;
        }
    }
}
