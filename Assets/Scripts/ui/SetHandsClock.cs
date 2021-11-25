using System;
using UnityEngine;
using UnityEngine.UI;

namespace ui {
    public class SetHandsClock : MonoBehaviour {
        public Image hoursHand;
        public Image minutesHand;
        public Image secondsHand;

        private void Awake() {
            if (hoursHand == null) {
                Debug.LogError("This component requires hoursHand");
                this.enabled = false;
                return;
            }

            if (minutesHand == null) {
                Debug.LogError("This component requires minutesHand");
                this.enabled = false;
                return;
            }

            if (secondsHand == null) {
                Debug.LogError("This component requires secondsHand");
                this.enabled = false;
                return;
            }
        }

        public void SetHands(DateTime date) {
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
