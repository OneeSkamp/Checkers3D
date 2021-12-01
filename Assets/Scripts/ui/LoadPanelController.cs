using System;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
using controller;

namespace ui {
    public class LoadPanelController : MonoBehaviour {
        public GameObject content;
        public GameObject loadItem;
        public GameObject thereIsNothingText;
        public CheckersController chController;
        public GameObject pagePanel;
        public Button pageButton;

        private List<Button> pageButtons;
        private int currentPage;
        private Dictionary<int, List<SaveInfo>> pages;
        private List<SaveInfo> allSaveInfos;

        private void Awake() {
            currentPage = 1;
            if (content == null) {
                Debug.LogError("This component requires content");
                this.enabled = false;
                return;
            }

            if (loadItem == null) {
                Debug.LogError("This component requires loadItem");
                this.enabled = false;
                return;
            }

            if (thereIsNothingText == null) {
                Debug.LogError("This component requires thereIsNothingText");
                this.enabled = false;
                return;
            }

            if (chController == null) {
                Debug.LogError("This component requires chController");
                this.enabled = false;
                return;
            }

            if (pagePanel == null) {
                Debug.LogError("This component requires pagePanel");
                this.enabled = false;
                return;
            }

            if (pageButton == null) {
                Debug.LogError("This component requires pageButton");
                this.enabled = false;
                return;
            }
        }

        public void FillPanel(int page) {
            foreach (Transform item in pagePanel.transform) {
                Destroy(item.gameObject);
            }

            pageButtons = new List<Button>();

            UpdatePages();

            foreach (var e in pages.Keys){
                var btn = Instantiate(pageButton, pagePanel.transform);
                pageButtons.Add(btn);
                btn.GetComponentInChildren<Text>().text = e.ToString();
                btn.onClick.AddListener(() => FillPage(e));
            }

            if (allSaveInfos.Count == 0) {
                thereIsNothingText.SetActive(true);
            } else {
                thereIsNothingText.SetActive(false);
                FillPage(currentPage);
            }
        }

        public void UpdatePages() {
            allSaveInfos = chController.GetSaveInfos(Application.persistentDataPath);
            var pageCount = Math.Ceiling(Convert.ToSingle(allSaveInfos.Count) / 4);
            pages = new Dictionary<int, List<SaveInfo>>();

            for (int i = 1; i <= pageCount; i++) {
                var page = new List<SaveInfo>();
                for (int j = (i - 1) * 4; j < (i - 1) * 4 + 4; j++) {
                    try {
                        page.Add(allSaveInfos[j]);
                    } catch {
                        break;
                    }
                }
                pages.Add(i, page);
            }
        }

        public void FillPage(int page) {
            currentPage = page;
            UpdatePages();
            foreach (Transform item in content.transform) {
                Destroy(item.gameObject);
            }

            for (int i = 0; i < pageButtons.Count; i++) {
                pageButtons[i].image.color = Color.white;
                if (i == currentPage - 1) {
                    pageButtons[currentPage - 1].image.color = Color.yellow;
                }
            }

            foreach (var saveInfo in pages[page]) {
                var loaderObj = Instantiate(loadItem, content.transform);
                loaderObj.transform.localScale = new Vector3(1f, 1f, 1f);

                Action loadAction = () => chController.LoadGame(saveInfo.savePath);

                Action deleteAction = () => {
                    var err = chController.DeleteFile(saveInfo.savePath);
                    if (err == ErrorType.DeleteError) {
                        Debug.LogError(err);
                        return;
                    }

                    Destroy(loaderObj);

                    if (content.transform.childCount == 1 && currentPage != 1) {
                        currentPage -= 1;
                        FillPanel(currentPage);
                    }
                    FillPanel(currentPage);
                };

                if (loaderObj.GetComponent<FillLoadElement>() == null) {
                    Debug.LogError("no component FillLoadElement");
                } else {
                    loaderObj.GetComponent<FillLoadElement>().Fill(
                        saveInfo,
                        loadAction,
                        deleteAction
                    );
                }
            }
        }
    }
}
