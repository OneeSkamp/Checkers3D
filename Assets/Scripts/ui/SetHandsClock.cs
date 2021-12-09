using System;
using UnityEngine;

namespace ui {
    public class SetHandsClock : MonoBehaviour {
        public Transform hourHand;
        public Transform minuteHand;
        public Transform secondHand;

        public void SetHands(DateTime date) {
            int minute = date.Minute;
            if (minuteHand == null) {
                Debug.LogError("This component requires minutesHand");
                return;
            }
            minuteHand.rotation = Quaternion.Euler(0f, 0f, minute * -6);

            int second = date.Second;
            if (secondHand == null) {
                Debug.LogError("This component requires secondsHand");
                return;
            }
            secondHand.rotation = Quaternion.Euler(0f, 0f, second * -6);

            int hour = date.Hour;
            if (hour > 12) {
                hour -= 12;
            }

            if (hourHand == null) {
                Debug.LogError("This component requires hoursHand");
                return;
            }
            hourHand.rotation = Quaternion.Euler(0f, 0f, hour * -30 - minute / 2);
        }
    }
}
