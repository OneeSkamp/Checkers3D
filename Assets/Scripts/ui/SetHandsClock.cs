using System;
using UnityEngine;

namespace ui {
    public class SetHandsClock : MonoBehaviour {
        public Transform hourHand;
        public Transform minuteHand;
        public Transform secondHand;

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
            minuteHand.rotation = Quaternion.Euler(0f, 0f, minute * -6);

            int second = date.Second;
            secondHand.rotation = Quaternion.Euler(0f, 0f, second * -6);

            int hour = date.Hour;
            if (hour > 12) {
                hour -= 12;
            }
            hourHand.rotation = Quaternion.Euler(0f, 0f, hour * -30 - minute / 2);
        }
    }
}
