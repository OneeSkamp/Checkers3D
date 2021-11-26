using System;
using UnityEngine;
using UnityEngine.UI;

namespace ui {
    public class SetHandsClock : MonoBehaviour {
        public Image hourHand;
        public Image minuteHand;
        public Image secondHand;

        private void Awake() {
            if (hourHand == null) {
                Debug.LogError("This component requires hoursHand");
                this.enabled = false;
                return;
            }

            if (minuteHand == null) {
                Debug.LogError("This component requires minutesHand");
                this.enabled = false;
                return;
            }

            if (secondHand == null) {
                Debug.LogError("This component requires secondsHand");
                this.enabled = false;
                return;
            }
        }

        public void SetHands(DateTime date) {
            int minute = date.Minute;
            minuteHand.transform.rotation = Quaternion.Euler(0f, 0f, minute * -6);

            int second = date.Second;
            secondHand.transform.rotation = Quaternion.Euler(0f, 0f, second * -6);

            int hour = date.Hour;
            if (hour > 12) {
                hour -= 12;
            }
            hourHand.transform.rotation = Quaternion.Euler(0f, 0f, hour * -30 - minute / 2);
        }
    }
}
