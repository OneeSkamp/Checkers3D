using UnityEngine;
using UnityEngine.UI;

public class SetText : MonoBehaviour {
    public Text textComponent;

    public void Set(string text) {
        if (textComponent == null) {
            Debug.LogError("Text component isn't provided");
            return;
        }

        textComponent.text = text;
    }
}
