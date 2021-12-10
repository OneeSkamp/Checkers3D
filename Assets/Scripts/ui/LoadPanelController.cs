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

        private List<Button> pageButtons;
        private List<Button> arrowsList;

        private void Awake() {
            pageButtons = new List<Button>();
            arrowsList = new List<Button>();

            // arrowsList.Add(pageArrows.first);
            // arrowsList.Add(pageArrows.previous);
            // arrowsList.Add(pageArrows.next);
            // arrowsList.Add(pageArrows.last);
        }

        public void FillPanel(int page) {
            if (content == null) {
                Debug.LogError("This component requires content");
                return;
            }

            if (content.GetComponent<GridLayoutGroup>() == null) {
                Debug.LogError("This component requires GridLayoutGroup");
                return;
            }
            content.GetComponent<GridLayoutGroup>().constraintCount = columns;

            foreach (var arrow in arrowsList) {
                arrow.interactable = false;
            }
            // pageArrows.next.interactable = false;
            // pageArrows.last.interactable = false;
            // pageArrows.first.interactable = false;
            // pageArrows.previous.interactable = false;

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
            if (allSaveInfos.Count == 0) return;

            if (pagePanel == null) {
                Debug.LogError("This component requires pagePanel");
                return;
            }

            var count = rows * columns;
            var pageCount = Mathf.CeilToInt(allSaveInfos.Count / count);

            if (pageCountOnPanel != pageButtons.Count) {
                for (int i = 2; i < pagePanel.transform.childCount - 2; i++) {
                    Destroy(pagePanel.transform.GetChild(i).gameObject);
                }

                pageButtons.Clear();

                if (pageButton == null) {
                    Debug.LogError("This component requires pageButton");
                    return;
                }

                if (pageButton.GetComponent<SetText>() == null) {
                    Debug.LogError("This component requires set text");
                    return;
                }

                for (int i = 0; i < pageCountOnPanel; i++) {
                    if (i >= pageCount) break;

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

            var curPage = startPage;
            for (int i = curPage; i <= pageCount && i - curPage < pageButtons.Count; i++) {
                var btn = pageButtons[i - startPage];

                var clr = Color.white;
                if (i == page) {
                    clr = Color.yellow;
                }
                btn.image.color = clr;
                btn.GetComponent<SetText>().Set(i.ToString());
                btn.onClick.RemoveAllListeners();
                var buttonPage = i;
                btn.onClick.AddListener(() => FillPanel(buttonPage));
            }

            for (int i = 0; i < arrowsList.Count; i++) {
                arrowsList[i].onClick.RemoveAllListeners();
                if (page != pageCount && i > 1 || page != 1 && i < 2) {
                    arrowsList[i].interactable = true;
                }
            }

            if (page != pageCount) {
                pageArrows.next.onClick.AddListener(() => FillPanel(page + 1));
                pageArrows.last.onClick.AddListener(() => FillPanel((int)pageCount));
            }

            if (page != 1) {
                pageArrows.previous.onClick.AddListener(() => FillPanel(page - 1));
                pageArrows.first.onClick.AddListener(() => FillPanel(1));
            }

            if (loadItem == null) {
                Debug.LogError("This component requires loadItem");
                return;
            }
            var startSave = (page - 1) * count;
            for (int i = startSave; i < startSave + count && i < allSaveInfos.Count; i++) {

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
