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

        private void Awake() {
            var newGamePath = Path.Combine(Application.streamingAssetsPath, "newgame.csv");
            newGameBtn.onClick.AddListener(() => {
                chController.selHighlight.SetActive(false);
                chController.map.board = chController.BoardFromCSV(newGamePath);
                chController.FillCheckers(chController.map.board);
                chController.enabled = true;
            });

            saveBtn.onClick.AddListener(async () => {
                var date = DateTime.Now.ToString("dd.MM.yyyy HH-mm-ss");
                var filePath = Path.Combine(Application.persistentDataPath, date);
                chController.boardToCSV(chController.map.board, filePath);
                chController.Screenshot(filePath);

                chController.enabled = true;

                await Manifestation();
                await Decay();
            });

            chController.gameOver += OpenMenu;
        }

        public void OpenMenu() {
            menu.SetActive(!menu.activeSelf);
            chController.enabled = true;
            if (menu.activeSelf) {
                chController.enabled = false;
            }
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
                    chController.selHighlight.SetActive(false);
                    chController.map.board = chController.BoardFromCSV(saveInfo.csvPath);
                    chController.FillCheckers(chController.map.board);
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

        public async Task Manifestation() {
            var color = saveComplete.color;
            for (float i = 0f; i <= 1f; i += 2f * Time.deltaTime) {
                await Task.Yield();
                saveComplete.color = new Color(color.r, color.g, color.b, i);
            }
        }

        public async Task Decay() {
            var color = saveComplete.color;
            for (float i = 1f; i > 0f; i -= 2f * Time.deltaTime) {
                await Task.Yield();
                saveComplete.color = new Color(color.r, color.g, color.b, i);
            }
        }
    }
}
