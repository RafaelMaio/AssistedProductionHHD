// ===============================
// AUTHOR     : Rafael Maio (rafael.maio@ua.pt)
// PURPOSE     : Handle configuration menu.
// SPECIAL NOTES: Communicates with the main script.
// ===============================

using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
using TMPro;

public class ConfigurationMenuScript : MonoBehaviour
{
    /// <summary>
    /// Canvas for scaling purposes.
    /// </summary>
    public Canvas canvas;

    /// <summary>
    /// Button to add a new anchor.
    /// </summary>
    public Button newAnchorButton;

    /// <summary>
    /// Button to place a new anchor in the plane.
    /// </summary>
    public Button placeAnchorButton;

    /// <summary>
    /// Button to add a new box object.
    /// </summary>
    public Button addBoxButton;

    /// <summary>
    /// Button to remove a created box.
    /// </summary>
    public Button removeBoxButton;

    /// <summary>
    /// Button to host an anchor.
    /// </summary>
    public Button hostButton;

    /// <summary>
    /// Button to return 1 menu.
    /// </summary>
    public Button backButton;

    /// <summary>
    /// Auxiliary text
    /// </summary>
    public TMP_Text helpText;

    /// <summary>
    /// Connection to the main script.
    /// </summary>
    public AppMainScript appMainScript;

    /// <summary>
    /// Initial menu GameObject.
    /// </summary>
    public GameObject initialMenu;

    /// <summary>
    /// Button to show/hide the anchor related menu.
    /// </summary>
    public Button SHAnchorMenuButton;

    /// <summary>
    /// Anchor menu.
    /// </summary>
    public GameObject AnchorMenu;

    /// <summary>
    /// Button to show/hide the box transformations related menu.  
    /// </summary>
    public Button SHTransformMenuButton;

    /// <summary>
    /// Transformations menu.
    /// </summary>
    public GameObject TransformMenu;

    /// <summary>
    /// Slider to switch between rotation and scale.
    /// </summary>
    public Slider rotScaleSlider;

    /// <summary>
    /// Slider to switch between rotation: roll, pitch, yaw.
    /// </summary>
    public Slider rotXYZSlider;

    /// <summary>
    /// Texts to change to bold -> rotXYZSlider related.
    /// </summary>
    public TMP_Text RotateText, ScaleText, XText, YText, ZText;

    /// <summary>
    /// Show/hide the pieces related menu.
    /// </summary>
    public Button SHPiecesMenuButton;

    /// <summary>
    /// Pieces information menu.
    /// </summary>
    public GameObject PiecesMenu;

    /// <summary>
    /// Distance from the show/hide buttons to their anchors.
    /// </summary>
    private float distance_from_AnhorButton, distance_from_TransformButton, distance_from_PiecesButton;

    /// <summary>
    /// Pieces menu scroll bar.
    /// </summary>
    public Scrollbar piecesScrollbar;

    /// <summary>
    /// Content from the scroll down menu.
    /// </summary>
    public GameObject scrolldownContent;

    /// <summary>
    /// Content from the scroll down menu.
    /// </summary>
    private List<GameObject> itemsConfigured = new List<GameObject>();

    /// <summary>
    /// Button for blocking the translation in the X axis.
    /// </summary>
    public Button TranslationXButton;

    /// <summary>
    /// Button for blocking the translation in the Y axis.
    /// </summary>
    public Button TranslationYButton;

    /// <summary>
    /// Button for blocking the translation in the Z axis.
    /// </summary>
    public Button TranslationZButton;

    /// <summary>
    /// Image representing that the translation is blocked.
    /// </summary>
    public Sprite blockSprite;

    /// <summary>
    /// Image representing that the translation is allowed.
    /// </summary>
    public Sprite checkmarkSprite;

    /// <summary>
    /// True if the translation is allowed;
    /// False otherwise.
    /// </summary>
    private bool transX = true, transY = true, transZ = true;


    /// <summary>
    /// Unity Start function.
    /// </summary>
    void Start()
    {
        this.GetComponent<RectTransform>().sizeDelta = canvas.GetComponent<RectTransform>().sizeDelta; // Adjust menu size

        //Button listeners
        newAnchorButton.onClick.AddListener(enableNewAnchor);
        placeAnchorButton.onClick.AddListener(PlaceAnchorFunc);
        addBoxButton.onClick.AddListener(addNewObject);
        removeBoxButton.onClick.AddListener(removeSelectedObject);
        hostButton.onClick.AddListener(HostFunc);
        backButton.onClick.AddListener(Back);
        SHAnchorMenuButton.onClick.AddListener(ShowHideAnchorMenu);
        SHTransformMenuButton.onClick.AddListener(ShowHideTransformMenu);
        rotScaleSlider.onValueChanged.AddListener(delegate { changeRotScale(); });
        rotXYZSlider.onValueChanged.AddListener(delegate { changeRotXYZ(); });
        SHPiecesMenuButton.onClick.AddListener(ShowHidePiecesMenu);
        TranslationXButton.onClick.AddListener(changeButtonImageX);
        TranslationYButton.onClick.AddListener(changeButtonImageY);
        TranslationZButton.onClick.AddListener(changeButtonImageZ);

        //Button initial interactability
        changeHostButtonInterac(false);
        changeAddBoxButtonInterac(false);
        changeRemoveBoxButtonInterac(false);
        changePlaceButtonInterac(false);

        //Get initial show/hide button positions
        distance_from_AnhorButton = SHAnchorMenuButton.transform.position.x - SHAnchorMenuButton.GetComponent<RectTransform>().rect.width / 2;
        distance_from_TransformButton = SHTransformMenuButton.transform.position.y - SHTransformMenuButton.GetComponent<RectTransform>().rect.width / 2;
        distance_from_PiecesButton = piecesScrollbar.transform.position.x - SHPiecesMenuButton.transform.position.x;
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
    /// Enable the placement of a new anchor.
    /// </summary>
    private void enableNewAnchor()
    {
        changeNewAnchorButtonInterac(false);
        changeHostButtonInterac(false);
        changeRemoveBoxButtonInterac(false);
        changeAddBoxButtonInterac(false);
        appMainScript.enableNewAnchor();
        changeHelpText("Move around until the application recognizes the intended surface.");
    }

    /// <summary>
    /// Add a new box to the anchor.
    /// </summary>
    private void addNewObject()
    {
        appMainScript.addNewObject();
    }

    /// <summary>
    /// Remove the selected box from the scene.
    /// </summary>
    private void removeSelectedObject()
    {
        appMainScript.removeSelectedObject();
    }

    /// <summary>
    /// Place a new anchor on the plane.
    /// </summary>
    private void PlaceAnchorFunc()
    {
        appMainScript.InstantiateAnchorObjByButton();
    }

    /// <summary>
    /// Host the cloud anchor.
    /// </summary>
    private void HostFunc()
    {
        appMainScript.HostAnchor();
        appMainScript.StoreCloudAnchorInformation();
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
    /// Change the add a new anchor button interactability.
    /// </summary>
    /// <param name="interactable">Button interactability</param>
    public void changeNewAnchorButtonInterac(bool interactable)
    {
        if (newAnchorButton.interactable != interactable)
            newAnchorButton.interactable = interactable;
    }

    /// <summary>
    /// Change the place anchor button interactability.
    /// </summary>
    /// <param name="interactable">Button interactability</param>
    public void changePlaceButtonInterac(bool interactable)
    {
        if (placeAnchorButton.interactable != interactable)
            placeAnchorButton.interactable = interactable;
    }

    /// <summary>
    /// Change the add a new box button interactability.
    /// </summary>
    /// <param name="interactable">Button interactability</param>
    public void changeAddBoxButtonInterac(bool interactable)
    {
        if (addBoxButton.interactable != interactable)
            addBoxButton.interactable = interactable;
    }

    /// <summary>
    /// Change the remove created box button interactability.
    /// </summary>
    /// <param name="interactable">Button interactability</param>
    public void changeRemoveBoxButtonInterac(bool interactable)
    {
        if (removeBoxButton.interactable != interactable)
            removeBoxButton.interactable = interactable;
    }

    /// <summary>
    /// Change the host button interactability.
    /// </summary>
    /// <param name="interactable">Button interactability</param>
    public void changeHostButtonInterac(bool interactable)
    {
        if (hostButton.interactable != interactable)
            hostButton.interactable = interactable;
    }

    /// <summary>
    /// Show/Hide the anchor related menu.
    /// </summary>
    private void ShowHideAnchorMenu()
    {
        SHAnchorMenuButton.transform.Rotate(0, 0, -180);
        SHAnchorMenuButton.transform.Translate(-distance_from_AnhorButton, 0, 0);
        AnchorMenu.SetActive(!AnchorMenu.activeSelf);
    }

    /// <summary>
    /// Show/Hide the box transformations related menu.
    /// </summary>
    private void ShowHideTransformMenu()
    {
        SHTransformMenuButton.transform.Rotate(0, 0, -180);
        SHTransformMenuButton.transform.Translate(-distance_from_TransformButton, 0, 0);
        TransformMenu.SetActive(!TransformMenu.activeSelf);
    }

    /// <summary>
    /// Change between rotation and scaling.
    /// </summary>
    private void changeRotScale()
    {
        if(rotScaleSlider.value == 0)
        {
            RotateText.fontStyle = FontStyles.Bold;
            ScaleText.fontStyle = FontStyles.Normal;
        }
        else
        {
            RotateText.fontStyle = FontStyles.Normal;
            ScaleText.fontStyle = FontStyles.Bold;
        }
        appMainScript.changeBetweenSlideRot((int)rotScaleSlider.value);
    }

    /// <summary>
    /// Change the axis of rotation.
    /// </summary>
    private void changeRotXYZ()
    {
        if (rotXYZSlider.value == 0)
        {
            XText.fontStyle = FontStyles.Bold;
            YText.fontStyle = FontStyles.Normal;
            ZText.fontStyle = FontStyles.Normal;
        }
        else if (rotXYZSlider.value == 1)
        {
            XText.fontStyle = FontStyles.Normal;
            YText.fontStyle = FontStyles.Bold;
            ZText.fontStyle = FontStyles.Normal;
        }
        else
        {
            XText.fontStyle = FontStyles.Normal;
            YText.fontStyle = FontStyles.Normal;
            ZText.fontStyle = FontStyles.Bold;
        }
        appMainScript.changeBetweenRotationXYZ((int)rotXYZSlider.value);
    }

    /// <summary>
    /// Show/Hide the pieces related menu.
    /// </summary>
    private void ShowHidePiecesMenu()
    {
        SHPiecesMenuButton.transform.Rotate(0, 0, -180);
        SHPiecesMenuButton.transform.Translate(-distance_from_PiecesButton, 0, 0);
        PiecesMenu.SetActive(!PiecesMenu.activeSelf);
    }

    /// <summary>
    /// Enable/Disable the piece buttons inside the pieces menu list.
    /// </summary>
    /// <param name="enable">Enable (true) or disable (false) the buttons.</param>
    public void enableListButtons(bool enable)
    {
        for (int i = 0; i < scrolldownContent.transform.childCount; i++)
        {
            if (!itemsConfigured.Contains(scrolldownContent.transform.GetChild(i).gameObject))
            {
                scrolldownContent.transform.GetChild(i).GetComponent<Button>().interactable = enable;
            }
        }
    }

    /// <summary>
    /// Change the "add a new box" text to support copy.
    /// </summary>
    /// <param name="selected">If any selected box, copy it</param>
    public void changeAddNewBoxButtonText(bool selected)
    {
        if (selected)
        {
            addBoxButton.GetComponentInChildren<TMP_Text>().text = "Copy" + '\n' + "Box";
        }
        else
        {
            addBoxButton.GetComponentInChildren<TMP_Text>().text = "Add New" + '\n' + "Box";
        }
    }

    /// <summary>
    /// Indicates that a piece from the list is already selected.
    /// </summary>
    /// <param name="ref_green">Piece selected.</param>
    /// <param name="previous_ref">Previous piece selected.</param>
    public void changeDropdownButtonColors(string ref_green, string previous_ref)
    {
        for (int i = 0; i < scrolldownContent.transform.childCount; i++)
        {
            Transform item = scrolldownContent.transform.GetChild(i);
            if (item.GetChild(1).GetComponent<TMP_Text>().text.Equals(ref_green))
            {
                item.GetComponent<Image>().color = new Color(0.27f,
                                                             0.75f,
                                                             0.25f,
                                                             0.58f);
                item.GetComponent<Button>().interactable = false;
                itemsConfigured.Add(item.gameObject);
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
            }
            else if (item.GetChild(1).GetComponent<TMP_Text>().text.Equals(previous_ref))
            {
                item.GetComponent<Image>().color = new Color(0.11f,
                                                             0.27f,
                                                             0.557f,
                                                             1f);
                item.GetComponent<Button>().interactable = true;
                itemsConfigured.Remove(item.gameObject);
                for (int j = 0; j < item.childCount; j++)
                {
                    if (item.GetChild(j).GetComponent<TMP_Text>() != null)
                    {
                        item.GetChild(j).GetComponent<TMP_Text>().color = new Color(1f,
                                                                                    1f,
                                                                                    1f,
                                                                                    1f);
                    }
                    else
                    {
                        item.GetChild(j).GetComponent<Image>().color = new Color(1f,
                                                                                 1f,
                                                                                 1f,
                                                                                 1f);
                    }
                }
            }
        }
    }

    /// <summary>
    /// Change the image of X translation button: representing blocked/allowed
    /// </summary>
    private void changeButtonImageX()
    {
        transX = !transX;
        if (!transX)
        {
            TranslationXButton.transform.GetChild(0).GetComponent<Image>().sprite = blockSprite;
        }
        else
        {
            TranslationXButton.transform.GetChild(0).GetComponent<Image>().sprite = checkmarkSprite;
        }
    }

    /// <summary>
    /// Change the image of Y translation button: representing blocked/allowed
    /// </summary>
    private void changeButtonImageY()
    {
        transY = !transY;
        if (!transY)
        {
            TranslationYButton.transform.GetChild(0).GetComponent<Image>().sprite = blockSprite;
        }
        else
        {
            TranslationYButton.transform.GetChild(0).GetComponent<Image>().sprite = checkmarkSprite;
        }
    }

    /// <summary>
    /// Change the image of Z translation button: representing blocked/allowed.
    /// </summary>
    private void changeButtonImageZ()
    {
        transZ = !transZ;
        if (!transZ)
        {
            TranslationZButton.transform.GetChild(0).GetComponent<Image>().sprite = blockSprite;
        }
        else
        {
            TranslationZButton.transform.GetChild(0).GetComponent<Image>().sprite = checkmarkSprite;
        }
    }

    /// <summary>
    /// Obtain the possible translations.
    /// </summary>
    /// <returns>An array with size 3, representing if each axis is blocked or allowed.</returns>
    public bool[] getTranslationAxis()
    {
        return new bool[] { transX, transY, transZ };
    }

    /// <summary>
    /// Start the scrolldown from the beggining.
    /// </summary>
    public void scrollbarUp()
    {
        piecesScrollbar.value = 1;
    }
}