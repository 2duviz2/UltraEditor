using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using TMPro;
using UnityEngine;
using UnityEngine.UI;

namespace UltraEditor.Classes
{
    public class ChapterSelectChanger : MonoBehaviour
    {
        public GameObject topLeftCS;
        public GameObject topRightCS;
        public GameObject bottomMiddleCS;

        int frameCount = 0;

        public void Update()
        {
            if (SceneHelper.CurrentScene == "Main Menu" && SceneHelper.CurrentThreadIsMainThread())
            {
                if (topLeftCS != null || topRightCS != null || bottomMiddleCS != null)
                {
                    frameCount = 0;
                    return;
                }
                frameCount++;
                if (frameCount < 60)
                {
                    return;
                }
                ChangeLayout();
            }
        }

        public void ChangeLayout()
        {
            if (topLeftCS != null || topRightCS != null || bottomMiddleCS != null)
            {
                return;
            }
            Destroy(topLeftCS);
            Destroy(topRightCS);
            Destroy(bottomMiddleCS);

            foreach (var chapter in FindObjectsOfType<VerticalLayoutGroup>(true))
            {
                if (chapter.name == "Chapters")
                {
                    // start pos (960 465 0)

                    chapter.gameObject.SetActive(true);

                    GameObject newTopLeft = new GameObject("TopLeftChapterSelect", typeof(RectTransform), typeof(VerticalLayoutGroup), typeof(ObjectActivateInSequence));
                    newTopLeft.transform.SetParent(chapter.transform.parent);
                    topLeftCS = newTopLeft;
                    GameObject newTopRight = new GameObject("TopRightChapterSelect", typeof(RectTransform), typeof(VerticalLayoutGroup));
                    newTopRight.transform.SetParent(chapter.transform.parent);
                    topRightCS = newTopRight;
                    GameObject newBottomMiddle = new GameObject("BottomMiddleChapterSelect", typeof(RectTransform), typeof(VerticalLayoutGroup));
                    newBottomMiddle.transform.SetParent(chapter.transform.parent);
                    bottomMiddleCS = newBottomMiddle;

                    ObjectActivateInSequence objectActivateInSequence = topLeftCS.GetComponent<ObjectActivateInSequence>();
                    objectActivateInSequence.delay = 0.025f;
                    objectActivateInSequence.objectsToActivate = new GameObject[] { };

                    topLeftCS.GetComponent<RectTransform>().localPosition = new Vector2(-200, 200);
                    topRightCS.GetComponent<RectTransform>().localPosition = new Vector2(200, 212);
                    bottomMiddleCS.GetComponent<RectTransform>().localPosition = new Vector2(-200, -50);
                    topLeftCS.GetComponent<RectTransform>().localScale = Vector3.one;
                    topRightCS.GetComponent<RectTransform>().localScale = Vector3.one;
                    bottomMiddleCS.GetComponent<RectTransform>().localScale = Vector3.one;

                    topLeftCS.GetComponent<VerticalLayoutGroup>().spacing = 4;
                    topLeftCS.GetComponent<VerticalLayoutGroup>().childAlignment = TextAnchor.UpperCenter;
                    topLeftCS.GetComponent<VerticalLayoutGroup>().childForceExpandHeight = false;
                    topLeftCS.GetComponent<VerticalLayoutGroup>().childForceExpandWidth = false;
                    topLeftCS.GetComponent<VerticalLayoutGroup>().childControlHeight = false;
                    topLeftCS.GetComponent<VerticalLayoutGroup>().childControlWidth = false;
                    topLeftCS.GetComponent<VerticalLayoutGroup>().childScaleHeight = false;
                    topLeftCS.GetComponent<VerticalLayoutGroup>().childScaleWidth = false;
                    topRightCS.GetComponent<VerticalLayoutGroup>().spacing = 4;
                    topRightCS.GetComponent<VerticalLayoutGroup>().childAlignment = TextAnchor.UpperCenter;
                    topRightCS.GetComponent<VerticalLayoutGroup>().childForceExpandHeight = false;
                    topRightCS.GetComponent<VerticalLayoutGroup>().childForceExpandWidth = false;
                    topRightCS.GetComponent<VerticalLayoutGroup>().childControlHeight = false;
                    topRightCS.GetComponent<VerticalLayoutGroup>().childControlWidth = false;
                    topRightCS.GetComponent<VerticalLayoutGroup>().childScaleHeight = false;
                    topRightCS.GetComponent<VerticalLayoutGroup>().childScaleWidth = false;
                    bottomMiddleCS.GetComponent<VerticalLayoutGroup>().spacing = 4;
                    bottomMiddleCS.GetComponent<VerticalLayoutGroup>().childAlignment = TextAnchor.UpperCenter;
                    bottomMiddleCS.GetComponent<VerticalLayoutGroup>().childForceExpandHeight = false;
                    bottomMiddleCS.GetComponent<VerticalLayoutGroup>().childForceExpandWidth = false;
                    bottomMiddleCS.GetComponent<VerticalLayoutGroup>().childControlHeight = false;
                    bottomMiddleCS.GetComponent<VerticalLayoutGroup>().childControlWidth = false;
                    bottomMiddleCS.GetComponent<VerticalLayoutGroup>().childScaleHeight = false;
                    bottomMiddleCS.GetComponent<VerticalLayoutGroup>().childScaleWidth = false;

                    int phase = 0;
                    GameObject lastCopiedChapter = null;
                    GameObject lastCopiedText = null;
                    for (int i = 0; i < chapter.transform.childCount; i++)
                    {
                        GameObject chapterButton = chapter.transform.GetChild(i).gameObject;

                        if (chapterButton.name == "Secondary")
                        {
                            lastCopiedText = chapterButton;
                            phase = 1;
                        }

                        if (phase == 0)
                        {
                            GameObject copyChapterButton = Instantiate(chapterButton.gameObject, topLeftCS.transform);
                            copyChapterButton.transform.SetParent(topLeftCS.transform);
                            lastCopiedChapter = copyChapterButton;
                            var l = objectActivateInSequence.objectsToActivate.ToList();
                            l.Add(copyChapterButton);
                            objectActivateInSequence.objectsToActivate = l.ToArray();
                        }
                        else if (phase == 1)
                        {
                            GameObject copyChapterButton = null;
                            if (chapterButton.name.Contains("(Clone)"))
                            {
                                copyChapterButton = chapterButton;

                                if (chapterButton.name == "CustomLevels(Clone)") // angry level loader
                                {
                                    chapterButton.GetComponentInChildren<TMP_Text>().text = "ANGRY LEVEL LOADER";
                                }
                            }
                            else
                            {
                                copyChapterButton = Instantiate(chapterButton.gameObject, topRightCS.transform);
                            }
                            copyChapterButton.transform.SetParent(topRightCS.transform);
                            lastCopiedChapter = copyChapterButton;
                            var l = objectActivateInSequence.objectsToActivate.ToList();
                            l.Add(copyChapterButton);
                            objectActivateInSequence.objectsToActivate = l.ToArray();
                        }
                    }

                    GameObject copyChapterButton2 = Instantiate(lastCopiedText.gameObject, bottomMiddleCS.transform);
                    copyChapterButton2.GetComponent<RectTransform>().localScale = Vector3.one;
                    copyChapterButton2.transform.SetParent(bottomMiddleCS.transform);
                    copyChapterButton2.GetComponentInChildren<TMP_Text>().text = "LEVEL EDITOR";

                    var l1 = objectActivateInSequence.objectsToActivate.ToList();
                    l1.Add(copyChapterButton2);
                    objectActivateInSequence.objectsToActivate = l1.ToArray();

                    copyChapterButton2 = Instantiate(lastCopiedChapter.gameObject, bottomMiddleCS.transform);
                    copyChapterButton2.GetComponent<RectTransform>().localScale = Vector3.one;
                    copyChapterButton2.transform.SetParent(bottomMiddleCS.transform);
                    copyChapterButton2.GetComponentInChildren<TMP_Text>().text = "CREATE LEVEL";
                    copyChapterButton2.GetComponent<Button>().onClick = new();
                    copyChapterButton2.GetComponent<Button>().onClick.RemoveAllListeners();
                    copyChapterButton2.GetComponent<Button>().onClick.AddListener(() => {
                        EmptySceneLoader.forceEditor = true;
                        EmptySceneLoader.Instance.LoadLevel();
                    });

                    l1 = objectActivateInSequence.objectsToActivate.ToList();
                    l1.Add(copyChapterButton2);
                    objectActivateInSequence.objectsToActivate = l1.ToArray();

                    copyChapterButton2 = Instantiate(lastCopiedChapter.gameObject, bottomMiddleCS.transform);
                    copyChapterButton2.GetComponent<RectTransform>().localScale = Vector3.one;
                    copyChapterButton2.transform.SetParent(bottomMiddleCS.transform);
                    copyChapterButton2.GetComponentInChildren<TMP_Text>().text = "OPEN LEVEL";
                    copyChapterButton2.GetComponent<Button>().onClick = new();
                    copyChapterButton2.GetComponent<Button>().onClick.AddListener(() => {
                        GameObject loadLevelCanvas = Instantiate(BundlesManager.editorBundle.LoadAsset<GameObject>("OpenLevelCanvas"));
                        EditorManager.StaticLoadPopup(loadLevelCanvas);
                    });

                    l1 = objectActivateInSequence.objectsToActivate.ToList();
                    l1.Add(copyChapterButton2);
                    objectActivateInSequence.objectsToActivate = l1.ToArray();

                    copyChapterButton2 = Instantiate(lastCopiedChapter.gameObject, bottomMiddleCS.transform);
                    copyChapterButton2.GetComponent<RectTransform>().localScale = Vector3.one;
                    copyChapterButton2.transform.SetParent(bottomMiddleCS.transform);
                    copyChapterButton2.GetComponentInChildren<TMP_Text>().text = "EXPLORE LEVELS";
                    /*copyChapterButton2.GetComponent<Button>().interactable = false;
                    copyChapterButton2.GetComponentInChildren<TMP_Text>().color = new Color(1, 1, 1, 0.5f);
                    copyChapterButton2.GetComponentInChildren<Image>().color = new Color(1, 1, 1, 0.5f);*/
                    copyChapterButton2.GetComponent<Button>().onClick = new();
                    copyChapterButton2.GetComponent<Button>().onClick.AddListener(() => {
                        GameObject loadLevelCanvas = Instantiate(BundlesManager.editorBundle.LoadAsset<GameObject>("ExploreLevelsCanvas"));
                    });

                    l1 = objectActivateInSequence.objectsToActivate.ToList();
                    l1.Add(copyChapterButton2);
                    objectActivateInSequence.objectsToActivate = l1.ToArray();

                    chapter.gameObject.SetActive(false);

                    return;
                }
            }
        }
    }
}
