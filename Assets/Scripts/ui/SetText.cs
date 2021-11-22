using System.Net.Mime;
using UnityEngine;
using UnityEngine.UI;

namespace ui {
    public class SetText : MonoBehaviour {
        public void Set(string text) {
            var currentText = GetComponent<Text>().text;
            currentText = text;
        }
    }
}
