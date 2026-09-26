using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
using TMPro;

namespace Manager
{
    public class UIPartyQuestData : MonoBehaviour
    {
        [SerializeField] private TextMeshProUGUI eventRefreshDaysText, statCheckIntensityText, partyTravelTimeText, partyTravelDeadlineText;

        public void SetEventData(int refreshDays, Vector2 statCheckIntensityRange)
        {
            if(refreshDays < 1)
                eventRefreshDaysText.text = "New Events Tomorrow";
            else
                eventRefreshDaysText.text = $"{refreshDays} days";

            statCheckIntensityText.text = $"{statCheckIntensityRange.x} - {statCheckIntensityRange.y}";
        }


        public void SetQuestData(int travelTimeDays, int questDeadlineDays, int partySize)
        {
            if(partySize < 1)
                partyTravelTimeText.text = "Party Empty";
            else
                partyTravelTimeText.text = $"{travelTimeDays} days";

            partyTravelDeadlineText.text = $"{questDeadlineDays} days";
        }
    }
}
