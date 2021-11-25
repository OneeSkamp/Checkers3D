using System;
using UnityEngine;
using UnityEngine.UI;

namespace ui {
    public class SetHandsClock : MonoBehaviour {
        public Image hoursHand;
        public Image minutesHand;
        public Image secondsHand;

        private void Awake() {
            // Debug.Log(DateTime.Now);
            // SetHands(DateTime.Now.ToString());
        }

        public void SetHands(string dateStr) {
            if (!DateTime.TryParse(dateStr, out DateTime date)) {
                Debug.LogError("Attempted conversion of date string to date failed");
                return;
            }

            int minute = date.Minute;
            minutesHand.transform.rotation = Quaternion.Euler(0f, 0f, minute * -6);

            int second = date.Second;
            secondsHand.transform.rotation = Quaternion.Euler(0f, 0f, second * -6);

            int hour = date.Hour;
            if (hour > 12) {
                hour -= 12;
            }
            hoursHand.transform.rotation = Quaternion.Euler(0f, 0f, hour * -30 - minute / 2);
        }
    }
}
