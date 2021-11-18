using System.Threading.Tasks;
using UnityEngine;
using controller;
using System;
using System.IO;
using UnityEngine.UI;

namespace ui {
    public class UiController : MonoBehaviour {
        public CheckersController chController;
        public GameObject loadPanel;
        public GameObject content;
        public GameObject menu;
        public GameObject loadItem;
        public Button newGameBtn;
        public Button saveBtn;
        public Text saveComplete;
        public AnimationCurve curve1;
        public AnimationCurve curve2;
        public AnimationCurve curve3;

        private void Awake() {
            if (chController == null) {
                Debug.LogError("chController isn't provided");
                this.enabled = false;
                return;
            }

            if (loadPanel == null) {
                Debug.LogError("loadPanel isn't provided");
                this.enabled = false;
                return;
            }

            if (content == null) {
                Debug.LogError("content isn't provided");
                this.enabled = false;
                return;
            }

            if (menu == null) {
                Debug.LogError("menu isn't provided");
                this.enabled = false;
                return;
            }

            if (loadItem == null) {
                Debug.LogError("loadItem isn't provided");
                this.enabled = false;
                return;
            }

            if (newGameBtn == null) {
                Debug.LogError("newGameBtn isn't provided");
                this.enabled = false;
                return;
            }

            if (saveBtn == null) {
                Debug.LogError("Black lady isn't provided");
                this.enabled = false;
                return;
            }

            if (saveComplete == null) {
                Debug.LogError("saveComplete isn't provided");
                this.enabled = false;
                return;
            }

            newGameBtn.onClick.AddListener(() => { chController.enabled = true; });

            saveBtn.onClick.AddListener(
                async () => {
                    chController.enabled = true;

                    await Manifestation(curve1, 1f);
                    await Manifestation(curve3, 1f);
                    await Manifestation(curve2, 2f);
                }
            );

            chController.gameOver += OpenMenu;
        }

        public void OpenMenu() {
            menu.SetActive(!menu.activeSelf);
            chController.enabled = !menu.activeSelf;
        }

        public void OpenLoadPanel() {
            chController.enabled = false;

            foreach (Transform item in content.transform) {
                Destroy(item.gameObject);
            }

            var saveInfos = chController.GetSaveInfos(Application.persistentDataPath);
            foreach (var saveInfo in saveInfos) {
                var loaderObj = Instantiate(loadItem);
                loaderObj.transform.SetParent(content.transform);
                loaderObj.transform.localScale = new Vector3(1f, 1f, 1f);

                var textObj = loaderObj.transform.GetChild(0);
                var text = textObj.GetComponent<Text>();
                text.text = saveInfo.name;

                var imageObj = loaderObj.transform.GetChild(1);
                var image = imageObj.GetComponent<RawImage>();

                image.texture = saveInfo.texture2D;

                var loadTransform = loaderObj.transform.GetChild(2);
                var loadBtn = loadTransform.GetComponent<Button>();
                loadBtn.onClick.AddListener(() => {
                    chController.LoadGame(saveInfo.csvPath);
                    menu.SetActive(false);
                    loadPanel.SetActive(false);
                    chController.enabled = true;
                });

                var deleteTransform = loaderObj.transform.GetChild(3);
                var deleteBtn = deleteTransform.GetComponent<Button>();

                deleteBtn.onClick.AddListener(() => {
                    Destroy(loaderObj);
                    try {
                        File.Delete(saveInfo.csvPath);
                    } catch (Exception e) {
                        Debug.LogError(e);
                    }

                    try {
                        File.Delete(saveInfo.pngPath);
                    } catch (Exception e) {
                        Debug.LogError(e);
                    }
                });
            }
        }

        public async Task Manifestation(AnimationCurve curve, float speed) {
            var color = saveComplete.color;
            for (float currentTime = 0f; currentTime <= 1f; currentTime += Time.deltaTime * speed) {
                await Task.Yield();
                saveComplete.color = new Color(color.r, color.g, color.b, curve.Evaluate (currentTime));
            }
        }
    }
}
