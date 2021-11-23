using System;
using UnityEngine;
using UnityEngine.UI;
using UnityEngine.Events;

namespace ui {
    public class SetButtonAction : MonoBehaviour {
        public void SetAction(Action action) {
            if (gameObject.GetComponent<Button>() == null) {
                Debug.LogError("no component Button");
            } else {
                gameObject.GetComponent<Button>().onClick.AddListener(new UnityAction(action));
            }
        }
    }
}
