// ===============================
// AUTHOR     : Rafael Maio (rafael.maio@ua.pt)
// PURPOSE     : Handles the initial menu.
// SPECIAL NOTES: Swap to configuration and visualization menu.
// ===============================

using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
using TMPro;

public class InitialMenuScript : MonoBehaviour
{
    /// <summary>
    /// Button to change menus to configuration.
    /// </summary>
    public Button configurationButton;

    /// <summary>
    /// Button to change menus to visualization.
    /// </summary>
    public Button visualizationButton;

    /// <summary>
    /// Configuration menu GameObject.
    /// </summary>
    public GameObject configurationMenu;

    /// <summary>
    /// Visualization menu GameObject.
    /// </summary>
    public GameObject visualizationMenu;

    /// <summary>
    /// Communication with the visualization menu script.
    /// </summary>
    public VisualizationMenuScript visualizationMenuScript;

    /// <summary>
    /// Canvas for scaling purposes.
    /// </summary>
    public Canvas canvas;

    /// <summary>
    /// New scenario Input Field.
    /// </summary>
    public TMP_InputField addScenarioIF;

    /// <summary>
    /// Button to add a new scenario.
    /// </summary>
    public Button addScenarioButton;

    /// <summary>
    /// Dropdown with every scenario.
    /// </summary>
    public TMP_Dropdown scenariosDD;

    /// <summary>
    /// Button to remove a created scenario.
    /// </summary>
    public Button removeScenarioButton;

    /// <summary>
    /// Button to clear the cloud anchors hosted in the selected scenario.
    /// </summary>
    public Button clearScenarioButton;

    /// <summary>
    /// "Select Scenario" text. -> Show/hide.
    /// </summary>
    public TMP_Text selectText;

    /// <summary>
    /// Connection to the main script.
    /// </summary>
    public AppMainScript appMainScript;

    /// <summary>
    /// Toggle to enable/disable usability test - IEETA.
    /// </summary>
    public Toggle usabilityTestsToggle;

    /// <summary>
    /// Dropdown with the lists for the usability tests.
    /// </summary>
    public TMP_Dropdown usabilityTestsPiecesListDD;

    /// <summary>
    /// Visualization pieces dropdown script.
    /// </summary>
    public ScrollViewAdapter scrollViewAdapterConf;

    /// <summary>
    /// Visualization pieces dropdown script.
    /// </summary>
    public ScrollViewAdapter scrollViewAdapterVis;

    /// <summary>
    /// Connection to the usability test script.
    /// </summary>
    public UsabilityTestScript usabilityTestScript;

    /// <summary>
    /// Connection to the usability test script.
    /// </summary>
    public UserTestsBoschScript userTestsBoschScript;

    /// <summary>
    /// Scanner menu GameObject.
    /// </summary>
    public GameObject scannerMenu;

    /// <summary>
    /// Connection to the scanner menu script.
    /// </summary>
    public ScannerMenuScript scannerMenuScript;

    /// <summary>
    /// Toggle to enable/disable usability test - Bosch.
    /// </summary>
    public Toggle usabilityTestsToggleBosch;

    /// <summary>
    /// Unity Start function.
    /// </summary>
    void Start()
    {
        this.GetComponent<RectTransform>().sizeDelta = canvas.GetComponent<RectTransform>().sizeDelta; // Adjust menu size

        //Button listeners
        configurationButton.onClick.AddListener(ChangeToConfiguration); 
        visualizationButton.onClick.AddListener(ChangeToVisualization);
        addScenarioButton.onClick.AddListener(addScenario);
        removeScenarioButton.onClick.AddListener(removeScenario);
        clearScenarioButton.onClick.AddListener(clearScenario);
        usabilityTestsToggle.onValueChanged.AddListener(delegate {toggleChanged(); });
        usabilityTestsToggleBosch.onValueChanged.AddListener(delegate { toggleBoschChanged(); });

        updateScenarios(); //Update dropdown with created scenarios
    }

    /// <summary>
    /// Change to the configuration menu.
    /// </summary>
    private void ChangeToConfiguration()
    {
        appMainScript.applicationState = AppMainScript.ApplicationStates.Configuring;
        this.gameObject.SetActive(false);
        configurationMenu.SetActive(true);
        appMainScript.setScenarioName(scenariosDD.options[scenariosDD.value].text);
        appMainScript.ResolveAnchorForConfiguration();
        if (usabilityTestsToggle.isOn)
        {
            scrollViewAdapterConf.setFileName("usability_tests_pieces_list");
        }
        else
        {
            scrollViewAdapterConf.setFileName("lista_pecas_bosch_tests");
        }
        scrollViewAdapterConf.FillList();
    }

    /// <summary>
    /// Change to the visualization menu.
    /// </summary>
    private void ChangeToVisualization()
    {
        this.gameObject.SetActive(false);
        appMainScript.setScenarioName(scenariosDD.options[scenariosDD.value].text);
        appMainScript.ResolveAnchor();
        if (usabilityTestsToggle.isOn)
        {
            appMainScript.applicationState = AppMainScript.ApplicationStates.Visualizing;
            visualizationMenu.SetActive(true);
            scrollViewAdapterVis.setFileName("usability_tests_fetch_pieces_" + (usabilityTestsPiecesListDD.value + 1).ToString());
            usabilityTestScript.enableTest();
            scrollViewAdapterVis.FillList();
            visualizationMenuScript.SWTimeBar(false);
        }
        else if (usabilityTestsToggleBosch.isOn)
        {
            appMainScript.applicationState = AppMainScript.ApplicationStates.Scanning;
            scannerMenu.SetActive(true);
            userTestsBoschScript.enableTest();
        }
        else
        {
            appMainScript.applicationState = AppMainScript.ApplicationStates.Scanning;
            scannerMenu.SetActive(true);
            visualizationMenuScript.SWTimeBar(true);
        }
    }

    /// <summary>
    /// Create a new scenario with the text in the inputfield.
    /// </summary>
    private void addScenario()
    {
        if(addScenarioIF.text != "")
            StorageControl.SaveDropdownHistory(addScenarioIF.text);
        updateScenarios(StorageControl.LoadScenariosDDHistory().Count - 1);
    }

    /// <summary>
    /// Remove the selected scenario.
    /// </summary>
    private void removeScenario()
    {
        StorageControl.RemoveDropdownHistory(scenariosDD.options[scenariosDD.value].text);
        updateScenarios();
    }

    /// <summary>
    /// Update the scenarios dropdown.
    /// </summary>
    private void updateScenarios(int value = 0)
    {
        scenariosDD.ClearOptions(); //Clear dropdown

        //Fill dropdown with the stored scenarios.
        foreach (var scenario in StorageControl.LoadScenariosDDHistory())
        {
            scenariosDD.options.Add(new TMP_Dropdown.OptionData() { text = scenario });
        }

        //Hide scenarios dropdown if there are no created scenarios.
        if (scenariosDD.options.Count == 0)
        {
            selectText.gameObject.SetActive(false);
            scenariosDD.gameObject.SetActive(false);
            removeScenarioButton.gameObject.SetActive(false);
        }
        else //Show scenarios if any exists.
        {
            selectText.gameObject.SetActive(true);
            scenariosDD.gameObject.SetActive(true);
            removeScenarioButton.gameObject.SetActive(true);
            scenariosDD.value = value;
            scenariosDD.Select();
            scenariosDD.RefreshShownValue();
        }
    }

    /// <summary>
    /// Clears the cloud anchors hosted in the selected scenario.
    /// </summary>
    private void clearScenario()
    {
        StorageControl.ClearAnchors(scenariosDD.options[scenariosDD.value].text);
    }

    /// <summary>
    /// Enable/disable usability tests - IEETA.
    /// Show/Hide pieces lists dropdown. 
    /// </summary>
    private void toggleChanged()
    {
        usabilityTestsPiecesListDD.gameObject.SetActive(usabilityTestsToggle.isOn);
    }

    /// <summary>
    /// Enable/disable usability tests - Bosch.
    /// </summary>
    private void toggleBoschChanged()
    {
        usabilityTestsToggle.gameObject.SetActive(!usabilityTestsToggleBosch.isOn);
        usabilityTestsPiecesListDD.gameObject.SetActive(!usabilityTestsToggleBosch.isOn);
    }
}