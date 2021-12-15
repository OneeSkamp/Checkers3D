using UnityEngine;
using UnityEngine.UI;

[ExecuteInEditMode, RequireComponent(typeof(HorizontalLayoutGroup))]
public class HorizontalAutoSizeSameChilds : MonoBehaviour {
    public GameObject defoultObj;
    private Rect rect;
    private void Awake() {
        rect = defoultObj.GetComponent<RectTransform>().rect;
    }

    private void Update() {
        var childCount = gameObject.transform.childCount;
        var size = new Vector2(rect.width * childCount, rect.height);
        gameObject.GetComponent<RectTransform>().sizeDelta = size;
    }
}
