using System;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
using controller;
using UnityEngine.Events;

namespace ui {
    [Serializable]
    public struct PageArrows {
        public Button next;
        public Button previous;
        public Button last;
        public Button first;
    }

    public class LoadPanelController : MonoBehaviour {
        public GameObject content;
        public GameObject loadItem;
        public GameObject thereIsNothingText;
        public CheckersController chController;
        public GameObject pagePanel;
        public PageArrows pageArrows;
        public Button pageButton;

        public int rows;
        public int columns;
        public int pageCountOnPanel;

        private List<Button> pageButtons = new List<Button>();

        public void FillPanel(int page) {
            if (content == null) {
                Debug.LogError("This component requires content");
                return;
            }
            content.GetComponent<GridLayoutGroup>().constraintCount = columns;

            pageArrows.next.interactable = false;
            pageArrows.last.interactable = false;
            pageArrows.first.interactable = false;
            pageArrows.previous.interactable = false;

            foreach (Transform item in content.transform) {
                Destroy(item.gameObject);
            }

            if (chController == null) {
                Debug.LogError("This component requires chController");
                return;
            }
            var allSaveInfos = chController.GetSaveInfos();

            if (thereIsNothingText == null) {
                Debug.LogError("This component requires thereIsNothingText");
                return;
            }
            thereIsNothingText.SetActive(allSaveInfos.Count == 0);

            if (pagePanel == null) {
                Debug.LogError("This component requires pagePanel");
                return;
            }

            var count = rows * columns;
            var pageCount = Math.Ceiling(Convert.ToSingle(allSaveInfos.Count) / count);

            if (pageCountOnPanel != pageButtons.Count) {
                for (int i = 2; i < pagePanel.transform.childCount - 2; i++) {
                    Destroy(pagePanel.transform.GetChild(i).gameObject);
                }

                pageButtons.Clear();
                for (int i = 0; i < pageCountOnPanel; i++) {
                    if (i >= pageCount) break;

                    if (pageButton == null) {
                        Debug.LogError("This component requires pageButton");
                        return;
                    }

                    var btn = Instantiate(pageButton, pagePanel.transform);
                    btn.transform.SetSiblingIndex(i + 2);
                    pageButtons.Add(btn);
                }
            }

            var startPage = 1;
            if (page != 1 && page >= pageCountOnPanel) {
                startPage = page - 1;
                if (page <= pageCount && page > pageCount - pageCountOnPanel + 1) {
                    startPage = (int)pageCount - pageCountOnPanel + 1;
                }
            }

            foreach (var btn in pageButtons) {
                if (startPage > pageCount) break;
                var i = startPage;
                btn.image.color = Color.white;
                if (i == page) {
                    btn.image.color = Color.yellow;
                }
                btn.GetComponentInChildren<Text>().text = (i).ToString();
                btn.onClick.RemoveAllListeners();
                btn.onClick.AddListener(() => FillPanel(i));
                startPage++;
            }

            if (page != pageCount && pageCount > 0) {
                pageArrows.next.interactable = true;
                pageArrows.last.interactable = true;

                pageArrows.next.onClick.RemoveAllListeners();
                pageArrows.next.onClick.AddListener(() => FillPanel(page + 1));

                pageArrows.last.onClick.RemoveAllListeners();
                pageArrows.last.onClick.AddListener(() => FillPanel((int)pageCount));
            }

            if (page != 1) {
                pageArrows.previous.interactable = true;
                pageArrows.first.interactable = true;

                pageArrows.previous.onClick.RemoveAllListeners();
                pageArrows.previous.onClick.AddListener(() => FillPanel(page - 1));

                pageArrows.first.onClick.RemoveAllListeners();
                pageArrows.first.onClick.AddListener(() => FillPanel(1));
            }

            var start = (page - 1) * count;
            for (int i = start; i < start + count; i++) {
                if (i >= allSaveInfos.Count || allSaveInfos.Count == 0) break;

                if (loadItem == null) {
                    Debug.LogError("This component requires loadItem");
                    return;
                }

                var loaderObj = Instantiate(loadItem, content.transform);
                loaderObj.transform.localScale = new Vector3(1f, 1f, 1f);

                var j = i;
                Action loadAction = () => chController.LoadGame(allSaveInfos[j].text);

                Action deleteAction = () => {
                    var err = chController.DeleteFile(allSaveInfos[j].savePath);
                    if (err == ErrorType.DeleteError) {
                        Debug.LogError(err);
                        return;
                    }

                    Destroy(loaderObj);

                    if (content.transform.childCount == 1 && page != 1) {
                        page -= 1;
                        pageButtons.Clear();
                        FillPanel(page);
                    }
                    FillPanel(page);
                };

                if (loaderObj.GetComponent<FillLoadElement>() == null) {
                    Debug.LogError("no component FillLoadElement");
                } else {
                    var loadElem = loaderObj.GetComponent<FillLoadElement>();
                    loadElem.buttons.loadBtn.onClick.AddListener(new UnityAction(loadAction));
                    loadElem.buttons.deleteBtn.onClick.AddListener(new UnityAction(deleteAction));

                    loadElem.Fill(allSaveInfos[i]);
                }
            }
        }
    }
}
