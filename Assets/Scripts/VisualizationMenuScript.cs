// ===============================
// AUTHOR     : Rafael Maio (rafael.maio@ua.pt)
// PURPOSE     : Handle visualization menu.
// SPECIAL NOTES: Communicates with the main script.
// ===============================

using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
using TMPro;
using System;

public class VisualizationMenuScript : MonoBehaviour
{
    /// <summary>
    /// Canvas for scaling purposes.
    /// </summary>
    public Canvas canvas;

    /// <summary>
    /// Auxiliary text
    /// </summary>
    public TMP_Text helpText;

    /// <summary>
    /// Button to return 1 menu.
    /// </summary>
    public Button backButton;

    /// <summary>
    /// Connection to the main script.
    /// </summary>
    public AppMainScript appMainScript;

    /// <summary>
    /// Initial menu GameObject.
    /// </summary>
    public GameObject initialMenu;

    /// <summary>
    /// Button to list the pieces in a unordered/ordered way.
    /// </summary>
    public Button orderButton;

    /// <summary>
    /// Content from the scroll down menu.
    /// </summary>
    public GameObject scrolldownContent;

    /// <summary>
    /// Pieces already fetched.
    /// </summary>
    public List<GameObject> itemsFetched = new List<GameObject>();

    /// <summary>
    /// Message when the kit is completed.
    /// </summary>
    public TMP_Text endText;

    /// <summary>
    /// Scrollbar of the list of pieces to fetch.
    /// </summary>
    public Scrollbar piecesListScrollbar;

    public Text debugText1;

    /// <summary>
    /// Scanner menu GameObject.
    /// </summary>
    public GameObject scannerMenu;

    /// <summary>
    /// Connection to the scanner menu script.
    /// </summary>
    public ScannerMenuScript scannerMenuScript;

    /// <summary>
    /// Progress bar to represent the pieces fetched and how many remainig.
    /// </summary>
    public GameObject progressBar;

    /// <summary>
    /// Theoretical time for fetching an entire kit.
    /// </summary>
    private const int timePerKit = 60;

    /// <summary>
    /// Initial date
    /// </summary>
    private DateTime initialKitDate;

    /// <summary>
    /// Time bar to represent the time remaining to complete the kit.
    /// </summary>
    public GameObject timeBar;

    /// <summary>
    /// Unity Start function.
    /// </summary>
    void Start()
    {
        this.GetComponent<RectTransform>().sizeDelta = canvas.GetComponent<RectTransform>().sizeDelta; // Adjust menu size

        //Button listeners
        backButton.onClick.AddListener(Back);
        orderButton.onClick.AddListener(changeOrder);
        initialKitDate = DateTime.Now;
    }

    /// <summary>
    /// Unity Update function.
    /// </summary>
    private void Update()
    {
        TimeSpan time = DateTime.Now - initialKitDate;
        if(time.Minutes < 1)
        {
            int remainingTime = timePerKit - time.Seconds;
            timeBar.transform.GetChild(0).localScale = new Vector3((float)time.Seconds / (float)timePerKit, timeBar.transform.GetChild(0).localScale.y, timeBar.transform.GetChild(0).localScale.z);
            timeBar.transform.GetChild(1).GetComponent<TMP_Text>().text = remainingTime.ToString() + "s";
        }
        else
        {
            timeBar.transform.GetChild(0).localScale = new Vector3(1, timeBar.transform.GetChild(0).localScale.y, timeBar.transform.GetChild(0).localScale.z);
            timeBar.transform.GetChild(1).GetComponent<TMP_Text>().text = "0s";
        }
    }

    /// <summary>
    /// Change the auxiliary text.
    /// </summary>
    /// <param name="text">New text.</param>
    public void changeHelpText(string text)
    {
        helpText.text = text;
    }

    /// <summary>
    /// Return a menu.
    /// </summary>
    private void Back()
    {
        this.gameObject.SetActive(false);
        initialMenu.SetActive(true);
        appMainScript.Clear();
        appMainScript.applicationState = AppMainScript.ApplicationStates.None;
        foreach (Transform child in scrolldownContent.transform)
        {
            Destroy(child.gameObject);
        }
    }

    /// <summary>
    /// Change to unordered/ordered list.
    /// </summary>
    private void changeOrder()
    {
        if (orderButton.GetComponentInChildren<TMP_Text>().text == "Unordered")
        {
            orderButton.GetComponentInChildren<TMP_Text>().text = "Ordered";
            for (int i = 0; i < scrolldownContent.transform.childCount; i++)
            {
                Transform item = scrolldownContent.transform.GetChild(i);
                if (!itemsFetched.Contains(item.gameObject))
                {
                    appMainScript.changeToOrdered(true, scrolldownContent.transform.GetChild(i).GetChild(1).GetComponent<TMP_Text>().text);
                    changeScrolldownTransparency(true);
                    break;
                }
            }
        }
        else
        {
            orderButton.GetComponentInChildren<TMP_Text>().text = "Unordered";
            appMainScript.changeToOrdered(false);
            changeScrolldownTransparency(false);
        }
    }

    /// <summary>
    /// Change the transparency of the list content (except the next piece to fetch).
    /// </summary>
    /// <param name="ordered">True=The list is ordered. False=The list is unordered.</param>
    private void changeScrolldownTransparency(bool ordered)
    {
        bool first = true;
        for (int i = 0; i < scrolldownContent.transform.childCount; i++)
        {
            Transform item = scrolldownContent.transform.GetChild(i);
            if (!itemsFetched.Contains(item.gameObject))
            {
                if (!first)
                {
                    item.GetComponent<Image>().color = new Color(item.GetComponent<Image>().color.r,
                                                                item.GetComponent<Image>().color.g,
                                                                item.GetComponent<Image>().color.b,
                                                                ordered ? 0.4f : 1f);
                    for (int j = 0; j < item.childCount; j++)
                    {
                        if (item.GetChild(j).GetComponent<TMP_Text>() != null)
                        {
                            item.GetChild(j).GetComponent<TMP_Text>().color = new Color(item.GetChild(j).GetComponent<TMP_Text>().color.r,
                                                                                        item.GetChild(j).GetComponent<TMP_Text>().color.g,
                                                                                        item.GetChild(j).GetComponent<TMP_Text>().color.b,
                                                                                        ordered ? 0.4f : 1f);
                        }
                        else
                        {
                            item.GetChild(j).GetComponent<Image>().color = new Color(item.GetChild(j).GetComponent<Image>().color.r,
                                                                                        item.GetChild(j).GetComponent<Image>().color.g,
                                                                                        item.GetChild(j).GetComponent<Image>().color.b,
                                                                                        ordered ? 0.4f : 1f);
                        }
                    }
                }
                first = false;
            }
        }
    }

    /// <summary>
    /// Removes the item corresponding to the caught piece from the scrolldown list.
    /// </summary>
    /// <param name="item_ref">Reference of the item to remove.</param>
    /// <param name="usability_enabled">Is the IEETA usability tests enabled?</param>
    public bool destroyContentItem(string item_ref, bool usability_enabled)
    {
        for (int i = 0; i < scrolldownContent.transform.childCount; i++)
        {
            Transform item = scrolldownContent.transform.GetChild(i);
            if (item.GetChild(1).GetComponent<TMP_Text>().text.Equals(item_ref))
            {
                if (!item.GetChild(4).GetComponent<TMP_Text>().text.Contains("1") && usability_enabled)
                {
                    item.GetChild(4).GetComponent<TMP_Text>().text = "Qtd. " + (Int32.Parse(item.GetChild(4).GetComponent<TMP_Text>().text.Split(' ')[1]) - 1).ToString();
                    item.GetChild(5).gameObject.SetActive(true);
                    Invoke("feedbackdissapear", 1f);
                    return false;
                }
                else
                {
                    item.GetComponent<Image>().color = new Color(0.27f,
                                                                 0.75f,
                                                                 0.25f,
                                                                 0.58f);
                    itemsFetched.Add(item.gameObject);
                    for (int j = 0; j < item.childCount; j++)
                    {
                        if (item.GetChild(j).GetComponent<TMP_Text>() != null)
                        {
                            item.GetChild(j).GetComponent<TMP_Text>().color = new Color(1f,
                                                                                        1f,
                                                                                        1f,
                                                                                        0.58f);
                        }
                        else
                        {
                            item.GetChild(j).GetComponent<Image>().color = new Color(1f,
                                                                                     1f,
                                                                                     1f,
                                                                                     0.58f);
                        }
                    }
                    if (usability_enabled)
                    {
                        item.GetChild(4).GetComponent<TMP_Text>().text = "Qtd. 0";
                    }
                    for(int k = 1; k + i < scrolldownContent.transform.childCount; k++) {
                        Transform nextItem = scrolldownContent.transform.GetChild(i + k);
                        if (!itemsFetched.Contains(nextItem.gameObject))
                        {
                            nextItem.GetComponent<Image>().color = new Color(nextItem.GetComponent<Image>().color.r,
                                                                        nextItem.GetComponent<Image>().color.g,
                                                                        nextItem.GetComponent<Image>().color.b,
                                                                        1);
                            for (int j = 0; j < nextItem.childCount; j++)
                            {
                                if (nextItem.GetChild(j).GetComponent<TMP_Text>() != null)
                                {
                                    nextItem.GetChild(j).GetComponent<TMP_Text>().color = new Color(1f,
                                                                                                1f,
                                                                                                1f,
                                                                                                1f);
                                }
                                else
                                {
                                    nextItem.GetChild(j).GetComponent<Image>().color = new Color(1f,
                                                                                                1f,
                                                                                                1f,
                                                                                                1f);
                                }
                            }
                            appMainScript.setNextRef(nextItem.GetChild(1).GetComponent<TMP_Text>().text);
                            break;
                        }
                    }   
                    return true;
                }
            } 
        }
        return true;
    }

    /// <summary>
    /// Deprecated: Not being used.
    /// </summary>
    private void feedbackdissapear()
    {
        for (int i = 0; i < scrolldownContent.transform.childCount; i++)
        {
            Transform item = scrolldownContent.transform.GetChild(i);
            if (item.GetChild(5).gameObject.activeSelf)
            {
                item.GetChild(5).gameObject.SetActive(false);
            }
        }
    }

    /// <summary>
    /// If usability tests enabled, change to obligatory ordered fetching.
    /// </summary>
    public void callChangeOrder(bool testsEnabled)
    {
        if (testsEnabled)
        {
            changeOrder();
            orderButton.interactable = false;
        }
        else
        {
            if (orderButton.GetComponentInChildren<TMP_Text>().text == "Ordered")
            {
                for (int i = 0; i < scrolldownContent.transform.childCount; i++)
                {
                    Transform item = scrolldownContent.transform.GetChild(i);
                    if (!itemsFetched.Contains(item.gameObject))
                    {
                        appMainScript.changeToOrdered(true, scrolldownContent.transform.GetChild(i).GetChild(1).GetComponent<TMP_Text>().text);
                        changeScrolldownTransparency(true);
                        break;
                    }
                }
            }
            else
            {
                appMainScript.changeToOrdered(false);
                changeScrolldownTransparency(false);
            }
        }
    }

    /// <summary>
    /// Change if the end text is active.
    /// </summary>
    /// <param name="end">Operation concluded.</param>
    public void changeEndText(bool end)
    {
        endText.gameObject.SetActive(end);
    }

    /// <summary>
    /// Place the scrollbar at the beggining.
    /// </summary>
    public void scrollbarUp()
    {
        piecesListScrollbar.value = 1;
    }

    /// <summary>
    /// Place the scrollbar at a specific refence.
    /// </summary>
    /// <param name="reference">The specific reference.</param>
    public void scrollBarAtRef(string reference)
    {
        int count = 0;
        foreach (Transform child in scrolldownContent.transform)
        {
            count += 1;
            if (child.GetChild(1).GetComponent<TMP_Text>().text.Equals(reference)){
                break;
            }
        }
        piecesListScrollbar.value = 1 - (float)count / (float)scrolldownContent.transform.childCount;
    }

    /// <summary>
    /// Change the scroll bar to the position of the first piece that has to be fetched.
    /// </summary>
    public void changeScrollBarToFirstPieceToFetch()
    {
        for (int i = 0; i < scrolldownContent.transform.childCount; i++)
        {
            Transform item = scrolldownContent.transform.GetChild(i);
            if (!itemsFetched.Contains(item.gameObject))
            {
                piecesListScrollbar.value = 1 - (float)i / (float)scrolldownContent.transform.childCount;
                break;
            }
        }
    }

    /// <summary>
    /// Change to scanner menu to scan a new datamatrix kit.
    /// </summary>
    public void changeToScanner()
    {
        appMainScript.applicationState = AppMainScript.ApplicationStates.Scanning;
        this.gameObject.SetActive(false);
        scannerMenu.SetActive(true);
        foreach(Transform child in scrolldownContent.transform)
        {
            Destroy(child.gameObject);
        }
    }

    /// <summary>
    /// Progress bar behaviour.
    /// </summary>
    /// <param name="num_pieces_fetched">Number of pieces already fetched.</param>
    /// <param name="num_total_pieces">Total number of pieces to fetch.</param>
    public void changeProgressBar(int num_pieces_fetched, int num_total_pieces)
    { 
        progressBar.transform.GetChild(0).localScale = new Vector3((float)num_pieces_fetched / (float)num_total_pieces, progressBar.transform.GetChild(0).localScale.y, progressBar.transform.GetChild(0).localScale.z);
        progressBar.transform.GetChild(1).GetComponent<TMP_Text>().text = num_pieces_fetched.ToString() + "/" + num_total_pieces;
    }

    /// <summary>
    /// Time at the beggining of the task.
    /// </summary>
    public void startTimer()
    {
        initialKitDate = DateTime.Now;
    }

    /// <summary>
    /// Hide time bar for usability tests enabled.
    /// </summary>
    /// <param name="show">Show/Hide time bar.</param>
    public void SWTimeBar(bool show)
    {
        //timeBar.gameObject.SetActive(show);
    }
}