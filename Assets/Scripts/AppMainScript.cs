// ===============================
// AUTHOR     : Rafael Maio (rafael.maio@ua.pt)
// PURPOSE     : Main script - Configuration and visualization logics.
// SPECIAL NOTES: Communicates with every menu script.
// ===============================

using Google.XR.ARCoreExtensions;
using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.EventSystems;
using UnityEngine.UI;
using UnityEngine.XR.ARFoundation;
using UnityEngine.XR.ARSubsystems;
using Lean.Common;
using Lean.Touch;
using TMPro;

public class AppMainScript : MonoBehaviour
{
    /// <summary>
    /// Application states.
    /// </summary>
    public enum ApplicationStates {None, Configuring, Visualizing, Scanning};

    /// <summary>
    /// Application current state.
    /// </summary>
    public ApplicationStates applicationState = ApplicationStates.None;

    /// <summary>
    /// Configuration menu scripts.
    /// </summary>
    public ConfigurationMenuScript confMenuScr;

    /// <summary>
    /// Visualization menu scripts.
    /// </summary>
    public VisualizationMenuScript visMenuScr;

    /// <summary>
    /// ARSession origin raycast manager.
    /// </summary>
    private ARRaycastManager raycastManager;

    /// <summary>
    /// ARSession origin anchor manager.
    /// </summary>
    private ARAnchorManager anchorManager;

    /// <summary>
    /// ARSession origin plane manager.
    /// </summary>
    private ARPlaneManager planeManager;

    /// <summary>
    /// Orange circle where the object will be placed.
    /// </summary>
    private Pose placementPose;

    /// <summary>
    /// Hit tracklabe id.
    /// </summary>
    private TrackableId placementId;

    /// <summary>
    /// The placememnt is valid. A plane is being recognized.
    /// </summary>
    private bool placementIsValid = false;

    /// <summary>
    /// Object placement indication GameObject.
    /// </summary>
    public GameObject placementIndicatorGo;

    /// <summary>
    /// Anchor prefab GameObject.
    /// </summary>
    public GameObject anchorPrefab;

    /// <summary>
    /// GameObject prefab representing an anchor with configured info.
    /// </summary>
    public GameObject anchorResPrefab;

    /// <summary>
    /// Anchor GameObject.
    /// </summary>
    private GameObject anchorGo;

    /// <summary>
    /// Anchor GameObject.
    /// </summary>
    private GameObject anchorResGo;

    /// <summary>
    /// ARAnchor to be hosted.
    /// </summary>
    private ARAnchor anchor;

    /// <summary>
    /// ARAnchor to be hosted.
    /// </summary>
    private ARCloudAnchor arCloudAnchor;

    /// <summary>
    /// ARCloudAnchors in the current scenario.
    /// </summary>
    private List<GameObject> configurationCloudAnchorGos = new List<GameObject>();

    /// <summary>
    /// List to store the walls (from the anchor) where the good quality was detected. 
    /// </summary>
    private List<String> anchorWallsGoodQuality = new List<String>();

    /// <summary>
    /// List to store the walls (from the anchor) where sufficient quality was detected. 
    /// </summary>
    private List<String> anchorWallsSufficientQuality = new List<String>();

    /// <summary>
    /// List with the resolved anchors in the current scenario.
    /// </summary>
    private List<ARCloudAnchor> resolvedAnchors = new List<ARCloudAnchor>();

    /// <summary>
    /// A list of Cloud Anchors that have been created but are not yet ready to use -> Hosting
    /// </summary>
    private List<ARCloudAnchor> _pendingCloudAnchorsHos = new List<ARCloudAnchor>();

    /// <summary>
    /// A list of created Cloud Anchors that are ready to be updated.
    /// </summary>
    private List<ARCloudAnchor> _pendingExistCloudAnchorsHos = new List<ARCloudAnchor>();

    /// <summary>
    /// A list of Cloud Anchors that have been created but are not yet ready to use -> Resolving
    /// </summary>
    private Dictionary<ARCloudAnchor, MyCloudAnchorHistory> _pendingCloudAnchorsRes = new Dictionary<ARCloudAnchor, MyCloudAnchorHistory>();

    /// <summary>
    /// A list of Cloud Anchors that have been created but are not yet ready to use -> Resolving for Configuration
    /// </summary>
    private Dictionary<ARCloudAnchor, MyCloudAnchorHistory> _pendingCloudAnchorsResConf = new Dictionary<ARCloudAnchor, MyCloudAnchorHistory>();

    /// <summary>
    /// A list of Cloud Anchors that have been created but are not yet ready to use -> Resolving for Configuration
    /// </summary>
    private Dictionary<ARCloudAnchor, MyCloudAnchorHistory> _resolvedCloudAnchorsConf = new Dictionary<ARCloudAnchor, MyCloudAnchorHistory>();

    /// <summary>
    /// Current scenario.
    /// </summary>
    private string scenarioName = "";

    /// <summary>
    /// Cube (box) prefab gameobject.
    /// </summary>
    public GameObject boxPrefab;

    /// <summary>
    /// List of the boxes associated to the anchor that is being hosted.
    /// </summary>
    private List<GameObject> listOfBoxesToHost = new List<GameObject>();

    /// <summary>
    /// List of the boxes associated to the resolved anchors.
    /// </summary>
    private List<GameObject> listOfResolvedBoxes = new List<GameObject>();

    /// <summary>
    /// True if add new anchor button clicked and user can add a new anchor to the plane; otherwise, no anchor will be added.
    /// </summary>
    private bool addNewAnchorEnabled = false;

    /// <summary>
    /// Device camera.
    /// </summary>
    private GameObject cam;

    /// <summary>
    /// Dictionary for saving the applied piece information related to the virtual object.
    /// </summary>
    private Dictionary<GameObject, List<String>> boxPieceInformationHos = new Dictionary<GameObject, List<String>>();

    /// <summary>
    /// Dictionary for associating the piece reference to the correponding box. -> For visualization
    /// </summary>
    private Dictionary<GameObject, String> boxPieceInformationRes = new Dictionary<GameObject, String>();

    /// <summary>
    /// Materials when box selected, already configured or (deselected and not configured).
    /// </summary>
    public Material selectedMaterial, noneMaterial, configuredMaterial;

    /// <summary>
    /// Last selected box. Null if all deselected.
    /// </summary>
    private GameObject previousSelected;

    /// <summary>
    /// List of pieces to search during visualization.
    /// </summary>
    private Dictionary<string, int> listOfPiecesToFetch = new Dictionary<string, int>();

    /// <summary>
    /// List of boxes already reached during visualization.
    /// </summary>
    private List<GameObject> listOfFetchedBoxes = new List<GameObject>();

    /// <summary>
    /// Next piece (identified by the reference) to fetch.
    /// </summary>
    private string nextRefToFectch = "";

    /// <summary>
    /// List of performed rotations on the corresponding box.
    /// </summary>
    private Dictionary<GameObject, List<Vector3>> objectRotations = new Dictionary<GameObject, List<Vector3>>();

    /// <summary>
    /// Connection to the usability test script.
    /// </summary>
    public UsabilityTestScript usabilityTestScript;

    /// <summary>
    /// Lines to support the translation direction.
    /// </summary>
    public LineRenderer[] translationLines;

    /// <summary>
    /// Interface texts for debug purposes.
    /// </summary>
    public Text debugText1, debugText2, debugText3, debugText4, debugText1Vis;

    /// <summary>
    /// Game object to substitute the box - Green light with a tag above.
    /// </summary>
    public GameObject boxSubstitutePrefab;

    /// <summary>
    /// The last box caught;
    /// </summary>
    private GameObject lastFetchedBox = null;

    /// <summary>
    /// The last box caught - prefab;
    /// </summary>
    public GameObject lastFetchedBoxPrefab;

    /// <summary>
    /// Arrow game object for giving the orientation of the next/closest box
    /// </summary>
    public GameObject animatedArrow;

    /// <summary>
    /// Connection to the usability test script - Bosch.
    /// </summary>
    public UserTestsBoschScript userTestsBoschScript;

    /// <summary>
    /// Unity Start function.
    /// </summary>
    void Start()
    {
        raycastManager = FindObjectOfType<ARRaycastManager>();
        anchorManager = FindObjectOfType<ARAnchorManager>();
        planeManager = FindObjectOfType<ARPlaneManager>();
        cam = GameObject.Find("AR Camera");

        translationLines[0].sortingOrder = 10;
        translationLines[1].sortingOrder = 10;
        translationLines[2].sortingOrder = 10;
    }

    /// <summary>
    /// Unity Update function.
    /// </summary>
    void Update()
    {
        if (applicationState == ApplicationStates.Configuring) // Configuration state behaviour
        {
            UpdatePlacementPose();
            UpdatePlacementIndicator();
            VerifyClickToPlaceObj();
            UpdateCloudAnchorQuality();
            UpdateCloudAnchorHostingState();
            UpdateCloudAnchorResolvingStateForConfiguration();
            UpdateCurrentHostingAnchor();
            UpdateCloudAnchorObjects();
            UpdateSelectedMaterial();
            UpdateSelectedBoxTextOrientation();
            UpdateTranslationLines();
        }
        else if (applicationState == ApplicationStates.Visualizing) // Visualization state behavior
        {
            UpdateCloudAnchorResolvingState();
            UpdatePieceFetched();
            UpdateTagDirection();
            UpdateAnimatedArrow();
        }
    }

    /// <summary>
    /// Update the pose of the placement indicator.
    /// Verifies if the raycast hits a plane.
    /// </summary>
    private void UpdatePlacementPose()
    {
        if (Camera.current != null)
        {
            var screenCenter = Camera.current.ViewportToScreenPoint(new Vector3(0.5f, 0.5f));
            var hits = new List<ARRaycastHit>();

            raycastManager.Raycast(screenCenter, hits, TrackableType.PlaneWithinPolygon);
            placementIsValid = hits.Count > 0 && addNewAnchorEnabled;
            if (placementIsValid)
            {
                placementPose = hits[0].pose;
                placementId = hits[0].trackableId;
            }
        }
    }

    /// <summary>
    /// Update the placement indicator gameobject pose (if valid).
    /// </summary>
    private void UpdatePlacementIndicator()
    {
        if (placementIsValid)
        {
            if (!placementIndicatorGo.activeSelf)
            {
                placementIndicatorGo.SetActive(true);
                confMenuScr.changePlaceButtonInterac(true);
                confMenuScr.changeHelpText("Place the anchor on the intended surface by clicking the button or the screen.");
            }
            placementIndicatorGo.transform.SetPositionAndRotation(placementPose.position, placementPose.rotation);
        }
        else
        {
            if (placementIndicatorGo.activeSelf)
            {
                placementIndicatorGo.SetActive(false);
                confMenuScr.changePlaceButtonInterac(false);
                confMenuScr.changeHelpText("Move around until the application recognizes the intended surface.");
            }
        }
    }

    /// <summary>
    /// Enable the placement of a new anchor.
    /// </summary>
    public void enableNewAnchor()
    {
        addNewAnchorEnabled = true;
    }

    /// <summary>
    /// Creates the anchor in the placement_indicator gameobject pose.
    /// </summary>
    public void InstantiateAnchorObjByButton()
    {
        InstantiateAnchorObj(placementId, new Pose(placementIndicatorGo.transform.position, placementIndicatorGo.transform.rotation));
    }

    /// <summary>
    /// Verify if the click intersected a plane.
    /// </summary>
    private void VerifyClickToPlaceObj()
    {
        if (addNewAnchorEnabled)
        {
            if (Input.touchCount == 0)
                return;

            var hits = new List<ARRaycastHit>();
            raycastManager.Raycast(Input.GetTouch(0).position, hits, TrackableType.PlaneWithinPolygon);
            if (hits.Count > 0)
            {
                if (!VerifyClickOnUI(Input.GetTouch(0).position))
                {
                    InstantiateAnchorObj(hits[0].trackableId, hits[0].pose);
                }
            }
        }
    }

    /// <summary>
    /// Verify if the user clicked a UI button.
    /// Change the GameObject button tag to "UI" in Editor
    /// </summary>
    /// <param name="pos">Click position.</param>
    /// <returns>If the user clicked a UI button.</returns>
    private bool VerifyClickOnUI(Vector3 pos)
    {
        PointerEventData pointer = new PointerEventData(EventSystem.current);
        List<RaycastResult> raycastResult = new List<RaycastResult>();
        pointer.position = new Vector2(pos.x, pos.y);
        EventSystem.current.RaycastAll(pointer, raycastResult);
        foreach (RaycastResult result in raycastResult)
        {
            if (result.gameObject.tag == "UI")
            {
                return true;
            }
        }
        raycastResult.Clear();
        return false;
    }

    /// <summary>
    /// Instatiates the anchor and the anchor object.
    /// </summary>
    /// <param name="objPose">Pose to place the anchor.</param>
    /// <param name="planeId">Plane associated with the anchor.</param>
    private void InstantiateAnchorObj(TrackableId planeId, Pose objPose)
    {
        //Clear current anchor
        if (anchorGo != null)
        {
            Destroy(anchorGo);
            anchorWallsGoodQuality.Clear();
        }
        boxPieceInformationHos.Clear();
        objectRotations.Clear();
        foreach (var box in listOfBoxesToHost)
        {
            Destroy(box);
        }
        listOfBoxesToHost.Clear();
        foreach (var line in translationLines)
        {
            line.gameObject.SetActive(false);
        }
        if (arCloudAnchor != null)
        {
            GameObject arCloudAnchorGo = Instantiate(anchorResPrefab, arCloudAnchor.pose.position, arCloudAnchor.pose.rotation, arCloudAnchor.transform);
            configurationCloudAnchorGos.Add(arCloudAnchorGo);
        }
        arCloudAnchor = null;

        ARPlane plane = planeManager.GetPlane(planeId);
        anchor = anchorManager.AttachAnchor(plane, objPose);
        if (plane.alignment == PlaneAlignment.HorizontalUp)
        {
            anchorGo = Instantiate(anchorPrefab, objPose.position, Quaternion.Euler(0.0f, objPose.rotation.eulerAngles.y, objPose.rotation.eulerAngles.z), anchor.transform);
        }
        else if(plane.alignment == PlaneAlignment.Vertical)
        {
            anchorGo = Instantiate(anchorPrefab, objPose.position, Quaternion.Euler(0.0f, closestToCamera(objPose.rotation.eulerAngles.y, cam.GetComponent<Camera>().transform.rotation.eulerAngles.y), 0.0f), anchor.transform);
        }
        confMenuScr.changeHostButtonInterac(false);
        if (addNewAnchorEnabled)
        {
            addNewAnchorEnabled = false;
            confMenuScr.changeNewAnchorButtonInterac(true);
        }
    }

    /// <summary>
    /// Function to verify the rotation wich will get the vertical anchor rotation turned to camera.
    /// </summary>
    /// <param name="placement_y">Vertical anchor y rotation.</param>
    /// <param name="camera_y">Camera y rotation.</param>
    /// <returns>Returns the sum or the subtraction of 90 to the anchor y rotation.</returns>
    private float closestToCamera(float placement_y, float camera_y)
    {
        float auxSum, auxSub;
        if (camera_y > 180)
        {
            auxSum = placement_y + 90;
        }
        else
        {
            if (placement_y + 90 > 360)
            {
                auxSum = StorageControl.mod(placement_y + 90, 60);
            }
            else
            {
                auxSum = placement_y + 90;
            }
        }
        if(camera_y < 180)
        {
            auxSub = placement_y - 90;
        }
        else
        {
            if (placement_y - 90 < 0)
            {
                auxSub = StorageControl.mod(placement_y - 90, 360);
            }
            else
            {
                auxSub = (placement_y - 90);
            }
        }
        if (Math.Abs(camera_y - auxSum) < Math.Abs(camera_y - auxSub))
        {
            return placement_y + 90;
        }
        else
        {
            return placement_y - 90;
        }
    }

    /// <summary>
    /// Verify the cloud anchor feature points quality. Paint the gameobject walls if good or sufficient.
    /// </summary>
    private void UpdateCloudAnchorQuality()
    {
        if(anchor != null && anchorGo != null)
        {
            foreach (Transform child in anchor.transform.GetChild(0))
            {
                if (!child.gameObject.name.Contains("anchor") && !anchorWallsGoodQuality.Contains(child.gameObject.name))
                {
                    if (IsLookingAtSide(child))
                    {
                        FeatureMapQuality quality = anchorManager.EstimateFeatureMapQualityForHosting
                                                    (new Pose(Camera.current.transform.position, Camera.current.transform.rotation));
                        if (quality == FeatureMapQuality.Good)
                        {
                            child.gameObject.GetComponent<Renderer>().material.SetColor("_Color", new Color32(0, 255, 0, 185));
                            anchorWallsGoodQuality.Add(child.gameObject.name);
                            if (anchorWallsSufficientQuality.Contains(child.gameObject.name))
                            {
                                anchorWallsSufficientQuality.RemoveAll(x => ((string)x).Equals(child.gameObject.name)); // LAMBDA query to remove by a condition
                            }
                        }
                        else if(quality == FeatureMapQuality.Sufficient)
                        {
                            child.gameObject.GetComponent<Renderer>().material.SetColor("_Color", new Color32(255, 255, 0, 185));
                            anchorWallsSufficientQuality.Add(child.gameObject.name);
                        }
                    }
                }
            }
            if ((anchorWallsGoodQuality.Count >= 4) || (anchorWallsGoodQuality.Count >= 3 && anchorWallsSufficientQuality.Count >= 1))
            {
                Invoke("invokedInstantiation", 1.5f);
                confMenuScr.changeHelpText("The quality of the anchor is good.");
                foreach (Transform child in anchor.transform.GetChild(0))
                {
                    if (!child.gameObject.name.Contains("anchor")){
                        Destroy(child.gameObject);
                    }
                }
                anchorWallsGoodQuality.Clear();
                anchorWallsSufficientQuality.Clear();
            }
        }
    }

    /// <summary>
    /// Verify if the camera is looking to a specif angle of the object.
    /// </summary>
    /// <param name="side">Object transform component.</param>
    /// <returns>If the camera is looking to a specif angle of the object.</returns>
    private bool IsLookingAtSide(Transform side)
    {
        // Check whether the bar is inside camera's view:
        var screenPoint = Camera.current.WorldToViewportPoint(side.position);
        if (screenPoint.z <= 0 || screenPoint.x <= 0 || screenPoint.x >= 1 ||
            screenPoint.y <= 0 || screenPoint.y >= 1)
        {
            confMenuScr.changeHelpText("Point to the virtual object.");
            return false;
        }

        // Check the distance between the indicator and the camera.
        float distance = (side.position - Camera.current.transform.position).magnitude;
        if (distance >= 10.0f)
        {
            confMenuScr.changeHelpText("Get closer, you are too far away from the virtual object.");
            return false;
        }
        else if(distance <= 1.5f * 0.1f)
        {
            confMenuScr.changeHelpText("Move backward, you are too close to the virtual object.");
            return false;
        }

        //Check if camera reached top view
        var cameraDir = Camera.current.transform.position - transform.position;
        var topviewThreshold = 15.0f;
        if (Vector3.Angle(cameraDir, Vector3.up) < topviewThreshold)
        {
            confMenuScr.changeHelpText("You are looking from the top view, move around from all sides.");
            return false;
        }

        // Check the angle between the camera and the object.
        if (side.rotation.eulerAngles.y - Camera.current.transform.rotation.eulerAngles.y >= -15 && side.rotation.eulerAngles.y - Camera.current.transform.rotation.eulerAngles.y <= 15)
        {
            confMenuScr.changeHelpText("Move the device around the virtual object until its walls turn green.");
            return true;
        }
        else
        {
            confMenuScr.changeHelpText("Move the device around the virtual object until its walls turn green.");
            return false;
        }
    }

    /// <summary>
    /// Invoked after 1,5s.
    /// Destroys the anchors and instantiates a box in its place.
    /// </summary>
    void invokedInstantiation()
    {
        GameObject box = Instantiate(boxPrefab, anchor.transform.position, anchor.transform.rotation, anchor.transform);
        listOfBoxesToHost.Add(box);
        objectRotations.Add(box, new List<Vector3> { anchor.transform.rotation.eulerAngles, new Vector3() });
        Destroy(anchorGo);
        anchorWallsGoodQuality.Clear();
        confMenuScr.changeHostButtonInterac(true);
        confMenuScr.changeAddBoxButtonInterac(true);
        confMenuScr.changeRemoveBoxButtonInterac(true);
        confMenuScr.changeHelpText("Select or add a new box. Host the anchor and its associated boxes if the setup is finished.");
    }

    /// <summary>
    ///  Creates and associates a new box to the anchor.
    /// </summary>
    public void addNewObject()
    {
        bool anySelected = false;
        GameObject selectedBox = null;
        foreach(var box in listOfBoxesToHost)
        {
            if (box.GetComponent<LeanSelectable>().IsSelected)
            {
                anySelected = true;
                selectedBox = box;
            }
        }
        if (anySelected)
        {
            GameObject box;
            if (anchor != null)
            {
                box = Instantiate(boxPrefab, selectedBox.transform.position, selectedBox.transform.rotation, anchor.transform);
            }
            else
            {
                box = Instantiate(boxPrefab, selectedBox.transform.position, selectedBox.transform.rotation, arCloudAnchor.transform);
            }
            box.transform.localScale = selectedBox.transform.localScale;
            listOfBoxesToHost.Add(box);
            List<Vector3> copyList = new List<Vector3>();
            copyList.AddRange(objectRotations[selectedBox]);
            copyList.Add(new Vector3());
            objectRotations.Add(box, copyList);
        }
        else
        {
            GameObject box;
            if (anchor != null)
            {
                box = Instantiate(boxPrefab, anchor.transform.position, anchor.transform.rotation, anchor.transform);
                objectRotations.Add(box, new List<Vector3> { anchor.transform.rotation.eulerAngles, new Vector3() });
            }
            else
            {
                box = Instantiate(boxPrefab, arCloudAnchor.transform.position, arCloudAnchor.transform.rotation, arCloudAnchor.transform);
                objectRotations.Add(box, new List<Vector3> { arCloudAnchor.transform.rotation.eulerAngles, new Vector3() });
            }
            listOfBoxesToHost.Add(box);
        }
    }

    /// <summary>
    /// Deletes the selected box.
    /// </summary>
    public void removeSelectedObject()
    {
        List<GameObject> boxesToRemove = new List<GameObject>();
        foreach (var box in listOfBoxesToHost)
        {
            if (box.GetComponent<LeanSelectable>() != null)
            {
                if (box.GetComponent<LeanSelectable>().IsSelected)
                {
                    boxesToRemove.Add(box);
                }
            }
        }
        foreach(var box in boxesToRemove)
        {
            box.GetComponent<LeanSelectable>().Deselect();
            listOfBoxesToHost.Remove(box);
            if (objectRotations.ContainsKey(box))
            {
                objectRotations.Remove(box);
            }
            if (boxPieceInformationHos.ContainsKey(box))
            {
                confMenuScr.changeDropdownButtonColors("", boxPieceInformationHos[box][0]);
                boxPieceInformationHos.Remove(box);
            }
            Destroy(box);
        }
    }

    /// <summary>
    /// Update the box material (green, black or blue) -> (selected, not selected, not selected but configured)
    /// </summary>
    private void UpdateSelectedMaterial()
    {
        bool anySelected = false;
        foreach(var box in listOfBoxesToHost)
        {
            if (box.GetComponent<LeanSelectable>().IsSelected)
            {
                anySelected = true;
                if (box != previousSelected)
                {
                    {
                        if (previousSelected != null)
                        {
                            if (boxPieceInformationHos.ContainsKey(previousSelected))
                            {
                                previousSelected.GetComponent<MeshRenderer>().material = configuredMaterial;
                            }
                            else
                            {
                                previousSelected.GetComponent<MeshRenderer>().material = noneMaterial;
                            }
                            previousSelected.transform.GetChild(0).gameObject.SetActive(false);
                            previousSelected.transform.GetChild(1).gameObject.SetActive(false);
                            confMenuScr.enableListButtons(false);
                        }
                        box.GetComponent<MeshRenderer>().material = selectedMaterial;
                        box.transform.GetChild(0).gameObject.SetActive(true);
                        box.transform.GetChild(1).gameObject.SetActive(true);
                        confMenuScr.enableListButtons(true);
                        previousSelected = box;
                        confMenuScr.changeAddNewBoxButtonText(true);
                        confMenuScr.changeHelpText("Move, rotate and scale the box to the intended position. Associate the corresponding piece.");
                    }
                }
            }
        }
        if (!anySelected)
        {
            if (previousSelected != null)
            {
                if (boxPieceInformationHos.ContainsKey(previousSelected))
                {
                    previousSelected.GetComponent<MeshRenderer>().material = configuredMaterial;
                }
                else
                {
                    previousSelected.GetComponent<MeshRenderer>().material = noneMaterial;

                }
                previousSelected.transform.GetChild(0).gameObject.SetActive(false);
                previousSelected.transform.GetChild(1).gameObject.SetActive(false);
                confMenuScr.enableListButtons(false);
                previousSelected = null;
                confMenuScr.changeAddNewBoxButtonText(false);
                confMenuScr.changeHelpText("Select or add a new box. Host the anchor and its associated boxes if the setup is finished.");
            }
        }
    }

    /// <summary>
    /// Turn the piece information text inside the box to the camera.
    /// </summary>
    private void UpdateSelectedBoxTextOrientation()
    {
        foreach (var box in listOfBoxesToHost)
        {
            if (box.GetComponent<LeanSelectable>().IsSelected)
            {
                box.transform.GetChild(0).rotation = Quaternion.Euler(cam.transform.rotation.eulerAngles.x,
                                                                                  cam.transform.rotation.eulerAngles.y,
                                                                                  cam.transform.rotation.eulerAngles.z);
            }
        }
    }

    /// <summary>
    /// Update the position and orientation of the translation lines.
    /// </summary>
    private void UpdateTranslationLines()
    {
        bool noneSelected = true;
        foreach (var box in listOfBoxesToHost)
        {
            if (box.GetComponent<LeanSelectable>().IsSelected)
            {
                noneSelected = false;

                //LineRenderer X
                if (confMenuScr.getTranslationAxis()[0])
                {
                    if (!translationLines[0].gameObject.activeSelf)
                    {
                        translationLines[0].gameObject.SetActive(true);
                    }
                    translationLines[0].SetPositions(new Vector3[] { new Vector3(box.transform.position.x + 0.1f, box.transform.position.y - 0.15f, box.transform.position.z),
                                                                    new Vector3(box.transform.position.x + 0.1f, box.transform.position.y - 0.15f, box.transform.position.z) + Vector3.right / 2.05f,
                                                                    new Vector3(box.transform.position.x + 0.1f, box.transform.position.y - 0.15f, box.transform.position.z) + Vector3.right / 2f,
                                                                    new Vector3(box.transform.position.x + 0.1f, box.transform.position.y - 0.15f, box.transform.position.z) + Vector3.right / 1.75f});
                }
                else
                {
                    if (translationLines[0].gameObject.activeSelf)
                    {
                        translationLines[0].gameObject.SetActive(false);
                    }
                }

                //LineRenderer Y
                if (confMenuScr.getTranslationAxis()[1])
                {
                    if (!translationLines[1].gameObject.activeSelf)
                    {
                        translationLines[1].gameObject.SetActive(true);
                    }
                    translationLines[1].SetPositions(new Vector3[] { new Vector3(box.transform.position.x, box.transform.position.y + 0.1f, box.transform.position.z),
                                                                    new Vector3(box.transform.position.x, box.transform.position.y + 0.1f, box.transform.position.z) + Vector3.up / 2.05f,
                                                                    new Vector3(box.transform.position.x, box.transform.position.y + 0.1f, box.transform.position.z) + Vector3.up / 2f,
                                                                    new Vector3(box.transform.position.x, box.transform.position.y + 0.1f, box.transform.position.z) + Vector3.up / 1.75f});
                }
                else
                {
                    if (translationLines[1].gameObject.activeSelf)
                    {
                        translationLines[1].gameObject.SetActive(false);
                    }
                }

                //LineRenderer Z
                if (confMenuScr.getTranslationAxis()[2])
                {
                    if (!translationLines[2].gameObject.activeSelf)
                    {
                        translationLines[2].gameObject.SetActive(true);
                    }
                    translationLines[2].SetPositions(new Vector3[] { new Vector3(box.transform.position.x, box.transform.position.y - 0.15f, box.transform.position.z + 0.1f),
                                                                    new Vector3(box.transform.position.x, box.transform.position.y - 0.15f, box.transform.position.z + 0.1f) + Vector3.forward / 2.05f,
                                                                    new Vector3(box.transform.position.x, box.transform.position.y - 0.15f, box.transform.position.z + 0.1f) + Vector3.forward / 2f,
                                                                    new Vector3(box.transform.position.x, box.transform.position.y - 0.15f, box.transform.position.z + 0.1f) + Vector3.forward / 1.75f});
                }
                else
                {
                    if (translationLines[2].gameObject.activeSelf)
                    {
                        translationLines[2].gameObject.SetActive(false);
                    }
                }
            }
        }
        if (noneSelected)
        {
            if (translationLines[0].gameObject.activeSelf)
            {
                translationLines[0].gameObject.SetActive(false);
            }
            if (translationLines[1].gameObject.activeSelf)
            {
                translationLines[1].gameObject.SetActive(false);
            }
            if (translationLines[2].gameObject.activeSelf)
            {
                translationLines[2].gameObject.SetActive(false);
            }
        }
    }
    
    /// <summary>
    /// Associate a piece to the selected box.
    /// </summary>
    /// <param name="button">Button clicked from the pieces list.</param>
    public void selectedPieceFromList(GameObject button)
    {
        string previousRef = "";
        foreach (var box in listOfBoxesToHost)
        {
            if (box.GetComponent<LeanSelectable>().IsSelected)
            {
                if (boxPieceInformationHos.ContainsKey(box))
                {
                    previousRef = boxPieceInformationHos[box][0];
                    boxPieceInformationHos[box] = new List<string> { button.transform.Find("ReferenceText").GetComponent<TMP_Text>().text,
                                                                button.transform.Find("NameText").GetComponent<TMP_Text>().text,
                                                                button.transform.Find("LocalizationText").GetComponent<TMP_Text>().text};
                    box.transform.GetChild(0).GetComponent<TMP_Text>().text = boxPieceInformationHos[box][0] + '\n' + boxPieceInformationHos[box][1] + '\n' + boxPieceInformationHos[box][2];
                }
                else
                {
                    boxPieceInformationHos.Add(box, new List<string> { button.transform.Find("ReferenceText").GetComponent<TMP_Text>().text,
                                                                button.transform.Find("NameText").GetComponent<TMP_Text>().text,
                                                                button.transform.Find("LocalizationText").GetComponent<TMP_Text>().text});
                    box.transform.GetChild(0).GetComponent<TMP_Text>().text = boxPieceInformationHos[box][0] + '\n' + boxPieceInformationHos[box][1] + '\n' + boxPieceInformationHos[box][2];
                }
            }
            confMenuScr.changeDropdownButtonColors(button.transform.Find("ReferenceText").GetComponent<TMP_Text>().text, previousRef);
        }
    }

    /// <summary>
    /// Host the anchor for a year in the cloud.
    /// </summary>
    public void HostAnchor()
    {
        if (anchor == null)
            return;

        ARCloudAnchor cloudAnchor = anchorManager.HostCloudAnchor(anchor, 364);
        _pendingCloudAnchorsHos.Add(cloudAnchor);
        confMenuScr.changeHostButtonInterac(false);
        confMenuScr.changePlaceButtonInterac(false);
        confMenuScr.changeRemoveBoxButtonInterac(false);
        confMenuScr.changeAddBoxButtonInterac(false);
    }

    /// <summary>
    /// Update the cloud anchor hosting state.
    /// Saves the stored anchor information in the local storage if host succeeded. 
    /// </summary>
    private void UpdateCloudAnchorHostingState()
    {
        foreach (var cloudAnchor in _pendingCloudAnchorsHos)
        {
            if (cloudAnchor.cloudAnchorState == CloudAnchorState.Success)
            {
                confMenuScr.changeHelpText("Host succedded. Add the next anchor.");
                int count = StorageControl.LoadCloudAnchorHistory().Collection.Count;
                List<AnchoredObject> auxAnchoredObjectList = new List<AnchoredObject>();
                Matrix4x4 m = StorageControl.Rotate(new Vector3(-cloudAnchor.transform.rotation.eulerAngles.x, -cloudAnchor.transform.rotation.eulerAngles.y, -cloudAnchor.transform.rotation.eulerAngles.z));

                foreach (var box in listOfBoxesToHost)
                {
                    Vector3 auxPos = box.transform.position - cloudAnchor.transform.position;
                    Vector3 pos = new Vector3(
                        auxPos.x * m.m00 + auxPos.y * m.m01 + auxPos.z * m.m02,
                        auxPos.x * m.m10 + auxPos.y * m.m11 + auxPos.z * m.m12,
                        auxPos.x * m.m20 + auxPos.y * m.m21 + auxPos.z * m.m22);
                    float rot_x = box.transform.eulerAngles.x - cloudAnchor.transform.rotation.eulerAngles.x;
                    float rot_y = box.transform.eulerAngles.y - cloudAnchor.transform.rotation.eulerAngles.y;
                    float rot_z = box.transform.eulerAngles.z - cloudAnchor.transform.rotation.eulerAngles.z;
                    IndustrialInfo pieceInfo;
                    if (boxPieceInformationHos.ContainsKey(box))
                    {
                        pieceInfo = new IndustrialInfo(boxPieceInformationHos[box][0], boxPieceInformationHos[box][1], boxPieceInformationHos[box][2]);
                    }
                    else
                    {
                        pieceInfo = new IndustrialInfo("", "", "");
                    }
                    AnchoredObject auxAnchoredObject = new AnchoredObject("Box", pos.x, pos.y, pos.z, objectRotations[box],
                        box.transform.localScale.x, box.transform.localScale.y, box.transform.localScale.z, pieceInfo);
                    auxAnchoredObjectList.Add(auxAnchoredObject);
                }
                MyCloudAnchorHistory newCloudAnchor = new MyCloudAnchorHistory("CloudAnchor" + count, cloudAnchor.cloudAnchorId, scenarioName, auxAnchoredObjectList);
                StorageControl.SaveCloudAnchorHistory(newCloudAnchor);

                //Clear after host completed
                boxPieceInformationHos.Clear();
                objectRotations.Clear();
                foreach(var box in listOfBoxesToHost)
                {
                    Destroy(box);
                }
                listOfBoxesToHost.Clear();
                _resolvedCloudAnchorsConf.Add(cloudAnchor, newCloudAnchor);
                GameObject arCloudAnchorGo = Instantiate(anchorResPrefab, cloudAnchor.pose.position, cloudAnchor.pose.rotation, cloudAnchor.transform);
                configurationCloudAnchorGos.Add(arCloudAnchorGo);

            }
            else if (cloudAnchor.cloudAnchorState != CloudAnchorState.TaskInProgress)
            {
                confMenuScr.changeHelpText("Host failed - " + cloudAnchor.cloudAnchorState.ToString());
            }
        }
        _pendingCloudAnchorsHos.RemoveAll(
                x => x.cloudAnchorState != CloudAnchorState.TaskInProgress);
    }

    /// <summary>
    /// Host the anchor for a year in the cloud.
    /// </summary>
    public void StoreCloudAnchorInformation()
    {
        if (arCloudAnchor == null)
            return;

        _pendingExistCloudAnchorsHos.Add(arCloudAnchor);
        confMenuScr.changeHostButtonInterac(false);
        confMenuScr.changePlaceButtonInterac(false);
        confMenuScr.changeRemoveBoxButtonInterac(false);
        confMenuScr.changeAddBoxButtonInterac(false);
    }

    /// <summary>
    /// Update the cloud anchor hosting state.
    /// Saves the stored anchor information relative to the anchor.
    /// </summary>
    private void UpdateCloudAnchorObjects()
    {
        foreach (var cloudAnchor in _pendingExistCloudAnchorsHos)
        {
            int count = StorageControl.LoadCloudAnchorHistory().Collection.Count;
            List<AnchoredObject> auxAnchoredObjectList = new List<AnchoredObject>();
            Matrix4x4 m = StorageControl.Rotate(new Vector3(-cloudAnchor.transform.rotation.eulerAngles.x, -cloudAnchor.transform.rotation.eulerAngles.y, -cloudAnchor.transform.rotation.eulerAngles.z));

            foreach (var box in listOfBoxesToHost)
            {
                Vector3 auxPos = box.transform.position - cloudAnchor.transform.position;
                Vector3 pos = new Vector3(
                    auxPos.x * m.m00 + auxPos.y * m.m01 + auxPos.z * m.m02,
                    auxPos.x * m.m10 + auxPos.y * m.m11 + auxPos.z * m.m12,
                    auxPos.x * m.m20 + auxPos.y * m.m21 + auxPos.z * m.m22);
                float rot_x = box.transform.eulerAngles.x - cloudAnchor.transform.rotation.eulerAngles.x;
                float rot_y = box.transform.eulerAngles.y - cloudAnchor.transform.rotation.eulerAngles.y;
                float rot_z = box.transform.eulerAngles.z - cloudAnchor.transform.rotation.eulerAngles.z;
                IndustrialInfo pieceInfo;
                if (boxPieceInformationHos.ContainsKey(box))
                {
                    pieceInfo = new IndustrialInfo(boxPieceInformationHos[box][0], boxPieceInformationHos[box][1], boxPieceInformationHos[box][2]);
                }
                else
                {
                    pieceInfo = new IndustrialInfo("", "", "");
                }
                AnchoredObject auxAnchoredObject = new AnchoredObject("Box", pos.x, pos.y, pos.z, objectRotations[box],
                    box.transform.localScale.x, box.transform.localScale.y, box.transform.localScale.z, pieceInfo);
                auxAnchoredObjectList.Add(auxAnchoredObject);
            }
            MyCloudAnchorHistory newCloudAnchor = new MyCloudAnchorHistory("CloudAnchor" + count, cloudAnchor.cloudAnchorId, scenarioName, auxAnchoredObjectList);
            StorageControl.UpdateCloudAnchorHistory(newCloudAnchor);
            _resolvedCloudAnchorsConf[arCloudAnchor] = newCloudAnchor;
            confMenuScr.changeHelpText("Host succedded. Add the next anchor.");
        }
        _pendingExistCloudAnchorsHos.Clear();
    }

    /// <summary>
    /// Resolve the cloud anchors for the current scenario.
    /// </summary>
    public void ResolveAnchor()
    {
        if (placementIndicatorGo.activeSelf)
        {
            placementIndicatorGo.SetActive(false);
        }
        MyCloudAnchorHistoryCollection anchorsColl = StorageControl.LoadCloudAnchorHistory();
        foreach(var storedAnchor in anchorsColl.Collection)
        {
            if (storedAnchor.scenarioName == scenarioName)
            {
                ARCloudAnchor cloudAnchor = anchorManager.ResolveCloudAnchorId(storedAnchor.Id);
                _pendingCloudAnchorsRes.Add(cloudAnchor, storedAnchor);
            }
        }
    }

    /// <summary>
    /// Deprecated
    /// Update the resolving state of the cloud anchors for the current scenario.
    /// Instatiates the related gameobjects.
    /// </summary>
    private void UpdateCloudAnchorResolvingStateDep()
    {
        List<ARCloudAnchor> resolvedCloudAnchors = new List<ARCloudAnchor>();
        foreach (var cloudAnchor in _pendingCloudAnchorsRes)
        {
            visMenuScr.changeHelpText("Searching for the cloud anchors in the scenario.");
            if (cloudAnchor.Key.cloudAnchorState == CloudAnchorState.Success)
            {
                foreach (var box in cloudAnchor.Value.listOfAnchoredObjects)
                {
                    GameObject boxGo = Instantiate(boxPrefab, cloudAnchor.Key.transform.position, cloudAnchor.Key.transform.rotation, cloudAnchor.Key.transform);
                    boxGo.transform.Translate(box.X, box.Y, box.Z);
                    for (int i = 1; i < box.Rotations.Count; i++)
                    {
                        boxGo.transform.Rotate(box.Rotations[i]);

                    }
                    boxGo.transform.localScale = new Vector3(box.Scale_X, box.Scale_Y, box.Scale_Z);
                    listOfResolvedBoxes.Add(boxGo);
                    removeLeanFunctionalities(boxGo);
                    if (listOfPiecesToFetch.ContainsKey(box.PieceInfo.Reference))
                    {
                        if (nextRefToFectch.Equals(""))
                        {
                            boxGo.GetComponent<MeshRenderer>().material = selectedMaterial;
                            boxGo.transform.GetChild(0).gameObject.SetActive(true);
                            boxGo.transform.GetChild(0).gameObject.GetComponent<TMP_Text>().text = box.PieceInfo.Reference + '\n' + box.PieceInfo.Name + '\n' + box.PieceInfo.Localization;
                        }
                        else
                        {
                            if (nextRefToFectch.Equals(box.PieceInfo.Reference))
                            {
                                boxGo.GetComponent<MeshRenderer>().material = selectedMaterial;
                                boxGo.transform.GetChild(0).gameObject.SetActive(true);
                                boxGo.transform.GetChild(0).gameObject.GetComponent<TMP_Text>().text = box.PieceInfo.Reference + '\n' + box.PieceInfo.Name + '\n' + box.PieceInfo.Localization;
                            }
                            else
                            {
                                boxGo.GetComponent<MeshRenderer>().material = noneMaterial;
                                boxGo.transform.GetChild(0).gameObject.SetActive(false);
                                boxGo.transform.GetChild(0).gameObject.GetComponent<TMP_Text>().text = box.PieceInfo.Reference + '\n' + box.PieceInfo.Name + '\n' + box.PieceInfo.Localization;
                            }
                        }
                    }
                    else
                    {
                        boxGo.GetComponent<MeshRenderer>().material = noneMaterial;
                        boxGo.transform.GetChild(0).gameObject.SetActive(false);
                        boxGo.transform.GetChild(0).gameObject.GetComponent<TMP_Text>().text = box.PieceInfo.Reference + '\n' + box.PieceInfo.Name + '\n' + box.PieceInfo.Localization;
                    }
                    boxPieceInformationRes.Add(boxGo, box.PieceInfo.Reference);
                }
                visMenuScr.changeHelpText("Fetch the piece inside the green box. Select it when fetched.");
                resolvedCloudAnchors.Add(cloudAnchor.Key);
            }
            else if (cloudAnchor.Key.cloudAnchorState != CloudAnchorState.TaskInProgress)
            {
                visMenuScr.changeHelpText("Failed in resolving cloud anchor - " + cloudAnchor.Key.cloudAnchorState.ToString());
            }
        }
        foreach (var item in resolvedCloudAnchors)
        {
            _pendingCloudAnchorsRes.Remove(item);
        }
    }

    /// <summary>
    /// Update the resolving state of the cloud anchors for the current scenario.
    /// Instatiates the related gameobjects.
    /// </summary>
    private void UpdateCloudAnchorResolvingState()
    {
        List<ARCloudAnchor> resolvedCloudAnchors = new List<ARCloudAnchor>();
        foreach (var cloudAnchor in _pendingCloudAnchorsRes)
        {
            visMenuScr.changeHelpText("Searching for the cloud anchors in the scenario.");
            if (cloudAnchor.Key.cloudAnchorState == CloudAnchorState.Success)
            {
                foreach (var box in cloudAnchor.Value.listOfAnchoredObjects)
                {
                    GameObject boxGo = Instantiate(boxSubstitutePrefab, cloudAnchor.Key.transform.position, cloudAnchor.Key.transform.rotation, cloudAnchor.Key.transform);
                    boxGo.transform.Translate(box.X, box.Y, box.Z);
                    listOfResolvedBoxes.Add(boxGo);

                    //Write information
                    Transform texts = boxGo.transform.GetChild(1);
                    texts.GetChild(0).GetComponent<TMP_Text>().text = box.PieceInfo.Name;
                    texts.GetChild(1).GetComponent<TMP_Text>().text = box.PieceInfo.Reference;
                    texts.GetChild(2).GetComponent<TMP_Text>().text = box.PieceInfo.Localization;
                    if (listOfPiecesToFetch.ContainsKey(box.PieceInfo.Reference)) // If the box has to be fetched
                    {
                        texts.GetChild(3).GetComponent<TMP_Text>().text = listOfPiecesToFetch[box.PieceInfo.Reference].ToString();
                        if (nextRefToFectch.Equals("")) { }//If not sequencial
                        else //If sequential
                        {
                            if (nextRefToFectch.Equals(box.PieceInfo.Reference)) { } //If is next piece
                            else //Hide if not next piece
                            {
                                boxGo.SetActive(false);
                            }
                        }
                    }
                    else
                    {
                        boxGo.SetActive(false);
                    }
                    boxPieceInformationRes.Add(boxGo, box.PieceInfo.Reference);
                }
                visMenuScr.changeHelpText("Fetch the piece inside the green box. Select it when fetched.");
                resolvedCloudAnchors.Add(cloudAnchor.Key);
            }
            else if (cloudAnchor.Key.cloudAnchorState != CloudAnchorState.TaskInProgress)
            {
                visMenuScr.changeHelpText("Failed in resolving cloud anchor - " + cloudAnchor.Key.cloudAnchorState.ToString());
            }
        }
        foreach (var item in resolvedCloudAnchors)
        {
            _pendingCloudAnchorsRes.Remove(item);
        }
    }

    /// <summary>
    /// Resolve the cloud anchors for the current scenario -> configuration purposes.
    /// </summary>
    public void ResolveAnchorForConfiguration()
    {
        MyCloudAnchorHistoryCollection anchorsColl = StorageControl.LoadCloudAnchorHistory();
        foreach (var storedAnchor in anchorsColl.Collection)
        {
            if (storedAnchor.scenarioName == scenarioName)
            {
                ARCloudAnchor cloudAnchor = anchorManager.ResolveCloudAnchorId(storedAnchor.Id);
                _pendingCloudAnchorsResConf.Add(cloudAnchor, storedAnchor);
            }
        }
    }

    /// <summary>
    /// Update the resolving state of the cloud anchors for the current scenario in configuration.
    /// Creates an object representing the anchor location.
    /// </summary>
    private void UpdateCloudAnchorResolvingStateForConfiguration()
    {
        foreach (var cloudAnchor in _pendingCloudAnchorsResConf)
        {
            if (cloudAnchor.Key.cloudAnchorState == CloudAnchorState.Success)
            {
                anchorResGo = Instantiate(anchorResPrefab, cloudAnchor.Key.pose.position, cloudAnchor.Key.pose.rotation, cloudAnchor.Key.transform);
                _resolvedCloudAnchorsConf.Add(cloudAnchor.Key, cloudAnchor.Value);
                configurationCloudAnchorGos.Add(anchorResGo);
                foreach(var box in cloudAnchor.Value.listOfAnchoredObjects)
                {
                    confMenuScr.changeDropdownButtonColors(box.PieceInfo.Reference, "");
                }
            }
            else if (cloudAnchor.Key.cloudAnchorState != CloudAnchorState.TaskInProgress)
            {
                confMenuScr.changeHelpText("Failed in resolving cloud anchor - " + cloudAnchor.Key.cloudAnchorState.ToString());
            }

        }
        foreach (var item in _resolvedCloudAnchorsConf)
        {
            _pendingCloudAnchorsResConf.Remove(item.Key);
        }
    }

    /// <summary>
    /// Change the anchor being hosted.
    /// </summary>
    private void UpdateCurrentHostingAnchor()
    {
        GameObject toDestroy = null;
        ARCloudAnchor previousAnchor = null;
        foreach(var cloudAnchorGo in configurationCloudAnchorGos)
        {
            if (cloudAnchorGo.GetComponent<LeanSelectable>().IsSelected)
            {
                confMenuScr.changeHostButtonInterac(true);
                confMenuScr.changeAddBoxButtonInterac(true);
                confMenuScr.changeRemoveBoxButtonInterac(true);
                //Clear current anchor
                boxPieceInformationHos.Clear();
                objectRotations.Clear();
                foreach (var box in listOfBoxesToHost)
                {
                    Destroy(box);
                }
                listOfBoxesToHost.Clear();
                foreach(var line in translationLines)
                {
                    line.gameObject.SetActive(false);
                }
                anchor = null;
                if(anchorGo != null)
                {
                    Destroy(anchorGo);
                    anchorWallsSufficientQuality.Clear();
                    anchorWallsGoodQuality.Clear();
                }

                previousAnchor = arCloudAnchor;

                cloudAnchorGo.GetComponent<LeanSelectable>().Deselect();
                toDestroy = cloudAnchorGo;

                //Expand new one
                arCloudAnchor = cloudAnchorGo.transform.parent.gameObject.GetComponent<ARCloudAnchor>();
                foreach (var box in _resolvedCloudAnchorsConf[arCloudAnchor].listOfAnchoredObjects)
                {
                    GameObject boxGo = Instantiate(boxPrefab, arCloudAnchor.transform.position, arCloudAnchor.transform.rotation, arCloudAnchor.transform);
                    boxGo.transform.Translate(box.X, box.Y, box.Z);
                    for (int i = 1; i < box.Rotations.Count; i++)
                    {
                        boxGo.transform.Rotate(box.Rotations[i]);

                    }
                    boxGo.transform.localScale = new Vector3(box.Scale_X, box.Scale_Y, box.Scale_Z);
                    listOfBoxesToHost.Add(boxGo);
                    List<Vector3> copyList = new List<Vector3>();
                    copyList.AddRange(box.Rotations);
                    copyList.Add(new Vector3());
                    objectRotations.Add(boxGo, copyList);
                    if (!box.PieceInfo.Reference.Equals(""))
                    {
                        boxGo.GetComponent<MeshRenderer>().material = configuredMaterial;
                        boxPieceInformationHos[boxGo] = new List<string> { box.PieceInfo.Reference,
                                                                box.PieceInfo.Name,
                                                                box.PieceInfo.Localization};
                        boxGo.transform.GetChild(0).GetComponent<TMP_Text>().text = box.PieceInfo.Reference + '\n' + box.PieceInfo.Name + '\n' + box.PieceInfo.Localization;
                        boxGo.transform.GetChild(0).gameObject.SetActive(false);

                    }
                    else
                    {
                        boxGo.GetComponent<MeshRenderer>().material = noneMaterial;
                    }
                }
            }
        }
        if (previousAnchor != null)
        {
            GameObject arCloudAnchorGo = Instantiate(anchorResPrefab, previousAnchor.pose.position, previousAnchor.pose.rotation, previousAnchor.transform);
            configurationCloudAnchorGos.Add(arCloudAnchorGo);
        }
        if (toDestroy != null)
        {
            configurationCloudAnchorGos.Remove(toDestroy);
            Destroy(toDestroy);
        }
    }

    /// <summary>
    /// Disables the lean functionalities from the boxes in the visualization mode.
    /// </summary>
    /// <param name="box">Box gameobject to disable lean.</param>
    private void removeLeanFunctionalities(GameObject box)
    {
        if (box.GetComponent<LeanCameraHandler>() != null && box.GetComponent<LeanCameraHandler>().enabled != false)
        {
            box.GetComponent<LeanCameraHandler>().enabled = false;
        }
        if (box.GetComponent<LeanCameraHandler>() != null && box.GetComponent<LeanCameraHandler>().enabled != false)
        {
            box.transform.GetComponent<LeanCameraHandler>().enabled = false;
        }
        if (box.GetComponent<LeanDragTranslate>() != null && box.GetComponent<LeanDragTranslate>().enabled != false)
        {
            box.GetComponent<LeanDragTranslate>().enabled = false;
        }
        if (box.GetComponent<LeanTwistRotateAxisAux>() != null && box.GetComponent<LeanTwistRotateAxisAux>().enabled != false)
        {
            box.GetComponent<LeanTwistRotateAxisAux>().enabled = false;
        }
        if (box.GetComponent<LeanPinchScale>() != null && box.GetComponent<LeanPinchScale>().enabled != false)
        {
            box.GetComponent<LeanPinchScale>().enabled = false;
        }
    }

    /// <summary>
    /// Set the selected scenario.
    /// </summary>
    /// <param name="scenario_name">Selected scenario.</param>
    public void setScenarioName(string scenario_name)
    {
        scenarioName = scenario_name;
    }

    /// <summary>
    /// Add a piece reference to the pieces to fetch list.
    /// </summary>
    /// <param name="pieceRef">Piece reference.</param>
    public void addToListOfPiecesToFetch(string pieceRef, int qtd)
    {
        if (applicationState == ApplicationStates.Visualizing)
        {
            listOfPiecesToFetch.Add(pieceRef, qtd);
        }
    }

    /// <summary>
    /// Change between rotation and scaling for interacting with the virtual objects in the configuration mode.
    /// </summary>
    /// <param name="value">Slider value - Rotation(0)/Scale(1)</param>
    public void changeBetweenSlideRot(int value)
    {
        foreach (var box in listOfBoxesToHost)
        {
            box.GetComponent<LeanPinchScale>().enabled = (value == 1);
            box.GetComponent<LeanTwistRotateAxisAux>().enabled = (value == 0);
        }
    }

    /// <summary>
    /// Change between the axis of rotation of the virtual objects in the configuration mode.
    /// </summary>
    /// <param name="value">Slider value - X(0)/Y(1)/Z(2)</param>
    public void changeBetweenRotationXYZ(int value)
    {
        foreach (var box in listOfBoxesToHost)
        {
            List<Vector3> currentList = objectRotations[box];
            currentList.Add(new Vector3());
            objectRotations[box] = currentList;
            box.GetComponent<LeanTwistRotateAxisAux>().Axis = new Vector3(-Convert.ToInt32(value == 0),
                                                                                             -Convert.ToInt32(value == 1),
                                                                                             -Convert.ToInt32(value == 2));
        }
    }

    /// <summary>
    /// Verifies if the object was selected, indicating that the piece was caught.
    /// </summary>
    private void UpdatePieceFetched()
    {
        foreach(var box in listOfResolvedBoxes)
        {
            bool destroyed = false;
            if (box.GetComponent<LeanSelectable>().IsSelected)
            {
                box.GetComponent<LeanSelectable>().Deselect();
                if (!listOfFetchedBoxes.Contains(box))
                {
                    //if (box.GetComponent<MeshRenderer>().material.name.Split(' ')[0].Equals(selectedMaterial.name))
                    //{
                        if (visMenuScr.destroyContentItem(boxPieceInformationRes[box], usabilityTestScript.getUsabilityEnabled()))
                        {
                            destroyed = true;
                            if(lastFetchedBox != null)
                            {
                                Destroy(lastFetchedBox);
                            }
                            lastFetchedBox = Instantiate(lastFetchedBoxPrefab, box.transform.position, box.transform.rotation);
                            lastFetchedBox.transform.GetChild(1).GetChild(0).GetComponent<TMP_Text>().text = box.transform.GetChild(1).GetChild(0).GetComponent<TMP_Text>().text;
                            lastFetchedBox.transform.GetChild(1).GetChild(1).GetComponent<TMP_Text>().text = box.transform.GetChild(1).GetChild(1).GetComponent<TMP_Text>().text;
                            lastFetchedBox.transform.GetChild(1).GetChild(2).GetComponent<TMP_Text>().text = box.transform.GetChild(1).GetChild(2).GetComponent<TMP_Text>().text;
                            lastFetchedBox.transform.GetChild(1).GetChild(3).GetComponent<TMP_Text>().text = box.transform.GetChild(1).GetChild(3).GetComponent<TMP_Text>().text;
                            box.SetActive(false);
                            //box.GetComponent<MeshRenderer>().material = noneMaterial;
                            //box.transform.GetChild(0).gameObject.SetActive(false);
                            listOfFetchedBoxes.Add(box);
                        }
                        visMenuScr.scrollBarAtRef(boxPieceInformationRes[box]);
                        if (IsInvoking())
                        {
                            CancelInvoke();
                        }
                        Invoke("callVisMenuBluePiece", 2f);
                        visMenuScr.changeProgressBar(listOfFetchedBoxes.Count, listOfPiecesToFetch.Count);
                        //For usability tests purposes - IEETA
                        if (usabilityTestScript.getUsabilityEnabled())
                        {
                            if (listOfFetchedBoxes.Contains(box))
                            {
                                usabilityTestScript.breakTime(listOfFetchedBoxes.Count, destroyed);
                            }
                            else
                            {
                                usabilityTestScript.breakTime(listOfFetchedBoxes.Count + 1, destroyed);
                            }
                        }
                        //Verify if every piece was caught
                        if (listOfFetchedBoxes.Count == listOfPiecesToFetch.Count)
                        {
                            visMenuScr.changeEndText(true);
                            if (usabilityTestScript.getUsabilityEnabled())
                            {
                                usabilityTestScript.stopTime();
                            }
                            if (userTestsBoschScript.getUsabilityEnabled())
                            {
                                userTestsBoschScript.breakTime(listOfFetchedBoxes.Count);
                            }
                            Invoke("scanAgain", 1f);
                        }
                    //}
                }
            }
        }
    }

    /// <summary>
    /// Call the visualization menu script function to change the scroll bar position.
    /// </summary>
    private void callVisMenuBluePiece()
    {
        visMenuScr.changeScrollBarToFirstPieceToFetch();
    }

    /// <summary>
    /// Change to the scanner manu to fetch a new kit.
    /// </summary>
    private void scanAgain()
    {
        visMenuScr.changeEndText(false);
        visMenuScr.changeToScanner();
        listOfFetchedBoxes.Clear();
        listOfPiecesToFetch.Clear();
        if(lastFetchedBox != null)
        {
            Destroy(lastFetchedBox);
        }
    }

    /// <summary>
    /// Change to unordered/ordered list.
    /// </summary>
    /// <param name="ordered">True/False=The list is ordered/unordered.</param>
    public void changeToOrdered(bool ordered, string firstBoxRef = "")
    {
        nextRefToFectch = firstBoxRef;
        if (ordered)
        {
            foreach(var box in listOfResolvedBoxes)
            {
                if (boxPieceInformationRes[box].Equals(firstBoxRef))
                {
                    box.SetActive(true);
                    //box.GetComponent<MeshRenderer>().material = selectedMaterial;
                    //box.transform.GetChild(0).gameObject.SetActive(true);
                }
                else
                {
                    box.SetActive(false);
                    //box.GetComponent<MeshRenderer>().material = noneMaterial;
                    //box.transform.GetChild(0).gameObject.SetActive(false);
                }
            }
        }
        else
        {
            foreach(var box in listOfResolvedBoxes)
            {
                if (!listOfFetchedBoxes.Contains(box))
                {
                    if (listOfPiecesToFetch.ContainsKey(boxPieceInformationRes[box]))
                    {
                        box.SetActive(true);
                        //box.GetComponent<MeshRenderer>().material = selectedMaterial;
                        //box.transform.GetChild(0).gameObject.SetActive(true);
                    }
                    else
                    {
                        box.SetActive(false);
                        //box.GetComponent<MeshRenderer>().material = noneMaterial;
                        //box.transform.GetChild(0).gameObject.SetActive(false);
                    }
                }
            }
        }
    }

    /// <summary>
    /// Set the next piece to fetch, identified by the piece reference
    /// </summary>
    /// <param name="reference">Next piece reference.</param>
    public void setNextRef(string reference)
    {
        if (!nextRefToFectch.Equals(""))
        {
            nextRefToFectch = reference;
        }
        foreach (var box in listOfResolvedBoxes)
        {
            if (boxPieceInformationRes[box].Equals(nextRefToFectch))
            {
                box.SetActive(true);
                //box.GetComponent<MeshRenderer>().material = selectedMaterial;
                //box.transform.GetChild(0).gameObject.SetActive(true);
            }
        }
    }

    /// <summary>
    /// Rotation ocurred. 
    /// Change the rotation to store.
    /// </summary>
    /// <param name="twitstedAngle">Angle of the rotation occurred.</param>
    public void rotationOcurred(float twitstedAngle)
    {
        foreach (var box in listOfBoxesToHost)
        {
            if (box.GetComponent<LeanSelectable>().IsSelected)
            {
                Vector3 newRot = new Vector3(-Convert.ToInt32(confMenuScr.rotXYZSlider.value == 0), 
                                             -Convert.ToInt32(confMenuScr.rotXYZSlider.value == 1), 
                                             -Convert.ToInt32(confMenuScr.rotXYZSlider.value == 2)) * twitstedAngle;
                objectRotations[box][objectRotations[box].Count - 1] = newRot + objectRotations[box][objectRotations[box].Count - 1];
            }
        }
    }

    /// <summary>
    /// Notifies that the pieces scrolldown is already fulfilled.
    /// </summary>
    public void getEndOfScrolldown()
    {
        if(applicationState == ApplicationStates.Visualizing)
        {
            visMenuScr.scrollbarUp();
            if (usabilityTestScript.getUsabilityEnabled())
            {
                visMenuScr.callChangeOrder(true);
            }
            else if (userTestsBoschScript.getUsabilityEnabled() && ((userTestsBoschScript.getKitCount() % 2) != 0))
            {
                visMenuScr.callChangeOrder(true);
            }
            else
            {
                visMenuScr.callChangeOrder(false);
            }
            visMenuScr.changeProgressBar(listOfFetchedBoxes.Count, listOfPiecesToFetch.Count);
            visMenuScr.startTimer();
            userTestsBoschScript.setTime();
        }
        else if(applicationState == ApplicationStates.Configuring)
        {

            confMenuScr.scrollbarUp();
        }
    }

    /// <summary>
    /// Clear the virtual objects and associated variables.
    /// Used when returning a menu.
    /// </summary>
    public void Clear()
    {
        if(anchorGo != null)
            Destroy(anchorGo);
        if (anchor != null)
            Destroy(anchor);
        anchor = null;
        _pendingCloudAnchorsRes.Clear();
        _pendingCloudAnchorsHos.Clear();
        if (placementIndicatorGo != null)
            placementIndicatorGo.SetActive(false);
        foreach(var linerend in translationLines)
        {
            if (linerend.gameObject.activeSelf)
            {
                linerend.gameObject.SetActive(false);
            }
        }
        foreach(var box in listOfBoxesToHost)
        {
            Destroy(box);
        }
        listOfBoxesToHost.Clear();
        foreach(var box in listOfResolvedBoxes)
        {
            Destroy(box);
        }
        listOfResolvedBoxes.Clear();
        boxPieceInformationHos.Clear();
        objectRotations.Clear();
        visMenuScr.changeEndText(false);
        if (arCloudAnchor != null)
            Destroy(arCloudAnchor);
        arCloudAnchor = null;
        foreach(var a in configurationCloudAnchorGos)
        {
            Destroy(a);
        }
        configurationCloudAnchorGos.Clear();
        listOfPiecesToFetch.Clear();
        listOfFetchedBoxes.Clear();
    }

    /// <summary>
    /// Reads the configuration JSON file (created from a module).
    /// Creates the AR scenario based on the json.
    /// </summary>
    /// <param name="pos">QRCode position.</param>
    /// <param name="rot">QRCode orientation.</param>
    // public void placeByQRCode(Vector3 pos, Quaternion rot)
    public void placeByQRCode(GameObject QRCodeImage)
    {
        /*
        QRCodeEmptyGO = new GameObject("EmptyQRCodeGO");
        QRCodeEmptyGO.transform.position = pos;
        QRCodeEmptyGO.transform.rotation = rot;
        */
        //QRCodeEmptyGO = Instantiate(emptyGOPrefab, pos, rot);
        
        TextAsset jsonFile = Resources.Load<TextAsset>("BoxesData");

        boxesJsonStruct boxesInJson = JsonUtility.FromJson<boxesJsonStruct>(jsonFile.text);
        foreach (var box in boxesInJson.objects)
        {
            //GameObject boxGo = Instantiate(boxPrefabJSON, QRCodeEmptyGO.transform.position, QRCodeEmptyGO.transform.rotation, QRCodeEmptyGO.transform);
            GameObject boxGo = Instantiate(boxPrefab, QRCodeImage.transform.position, QRCodeImage.transform.rotation, QRCodeImage.transform);
            //Debug.Log(float.Parse(box.box_position.x).ToString() + "," + float.Parse(box.box_position.y).ToString() + "," + float.Parse(box.box_position.z).ToString());
            boxGo.transform.Translate(float.Parse(box.box_position.x), float.Parse(box.box_position.y), float.Parse(box.box_position.z));
            //TESTAR ROTATÇÃO:
                Quaternion auxQuaternion = new Quaternion(float.Parse(box.box_rotation.x), float.Parse(box.box_rotation.y), float.Parse(box.box_rotation.z), float.Parse(box.box_rotation.w));
                boxGo.transform.Rotate(auxQuaternion.eulerAngles);
            boxGo.transform.localScale = new Vector3(float.Parse(box.box_scale.x), float.Parse(box.box_scale.x), float.Parse(box.box_scale.x));
            boxGo.transform.SetParent(null);
            listOfResolvedBoxes.Add(boxGo);
            removeLeanFunctionalities(boxGo);
            if (listOfPiecesToFetch.ContainsKey(box.tag.part_reference))
            {
                if (nextRefToFectch.Equals(""))
                {
                    boxGo.GetComponent<MeshRenderer>().material = selectedMaterial;
                    boxGo.transform.GetChild(0).gameObject.SetActive(true);
                    boxGo.transform.GetChild(0).gameObject.GetComponent<TMP_Text>().text = box.tag.part_reference + '\n' + box.tag.part_name + '\n' + box.tag.part_location;
                }
                else
                {
                    if (nextRefToFectch.Equals(box.tag.part_reference))
                    {
                        boxGo.GetComponent<MeshRenderer>().material = selectedMaterial;
                        boxGo.transform.GetChild(0).gameObject.SetActive(true);
                        boxGo.transform.GetChild(0).gameObject.GetComponent<TMP_Text>().text = box.tag.part_reference + '\n' + box.tag.part_name + '\n' + box.tag.part_location;
                    }
                    else
                    {
                        boxGo.GetComponent<MeshRenderer>().material = noneMaterial;
                        boxGo.transform.GetChild(0).gameObject.SetActive(false);
                        boxGo.transform.GetChild(0).gameObject.GetComponent<TMP_Text>().text = box.tag.part_reference + '\n' + box.tag.part_name + '\n' + box.tag.part_location;
                    }
                }
            }
            else
            {
                boxGo.GetComponent<MeshRenderer>().material = noneMaterial;
                boxGo.transform.GetChild(0).gameObject.SetActive(false);
                boxGo.transform.GetChild(0).gameObject.GetComponent<TMP_Text>().text = box.tag.part_reference + '\n' + box.tag.part_name + '\n' + box.tag.part_location;
            }
            boxPieceInformationRes.Add(boxGo, box.tag.part_reference);
        }
    }

    /// <summary>
    /// Update the tag orientation during visualization.
    /// </summary>
    private void UpdateTagDirection()
    {
        foreach(var box_tag in listOfResolvedBoxes)
        {
            if (box_tag.activeSelf)
            {
                box_tag.transform.rotation = Quaternion.Euler(cam.transform.rotation.eulerAngles.x,
                                                                                  cam.transform.rotation.eulerAngles.y,
                                                                                  cam.transform.rotation.eulerAngles.z);
                box_tag.transform.rotation = Quaternion.Euler(cam.transform.rotation.eulerAngles.x,
                                                                                  cam.transform.rotation.eulerAngles.y,
                                                                                  cam.transform.rotation.eulerAngles.z);
            }
        }
        if(lastFetchedBox != null)
        {
            lastFetchedBox.transform.rotation = Quaternion.Euler(cam.transform.rotation.eulerAngles.x,
                                                                                  cam.transform.rotation.eulerAngles.y,
                                                                                  cam.transform.rotation.eulerAngles.z);
        }
    }

    /// <summary>
    /// Verify the next box to look at.
    /// </summary>
    private void UpdateAnimatedArrow()
    {
        if (nextRefToFectch.Equals(""))//if not sequential
        {
            List<GameObject> objectsToFetch = new List<GameObject>();
            foreach(var box in listOfResolvedBoxes)
            {
                if (listOfPiecesToFetch.ContainsKey(boxPieceInformationRes[box]) && !listOfFetchedBoxes.Contains(box))
                {
                    objectsToFetch.Add(box);
                }
            }
            reuseAnimatedArrowCode(StorageControl.closestGameobjectToCamera(cam.transform.position, objectsToFetch));
        }
        else //if sequential
        {
            GameObject nextBox = null;
            foreach(var box in listOfResolvedBoxes)
            {
                if (nextRefToFectch.Equals(boxPieceInformationRes[box]))
                {
                    nextBox = box;
                }
            }
            if(nextBox != null)
            {
                reuseAnimatedArrowCode(nextBox);
            }
            else
            {
                if (animatedArrow.activeSelf)
                    animatedArrow.SetActive(false);
            }
        }
    }

    /// <summary>
    /// Update the animated arrow position and orientation.
    /// </summary>
    /// <param name="nextBox">The box to look at.</param>
    private void reuseAnimatedArrowCode(GameObject nextBox)
    {
        // Check whether the bar is inside camera's view:
        var screenPoint = Camera.current.WorldToViewportPoint(nextBox.transform.position);
        if (screenPoint.z <= 0 || screenPoint.x <= 0 || screenPoint.x >= 1 || screenPoint.y <= 0 || screenPoint.y >= 1)
        {
            if (!animatedArrow.activeSelf)
                animatedArrow.SetActive(true);
            animatedArrow.transform.position = cam.transform.position + cam.transform.forward * 2f;
            Quaternion auxQuat = Quaternion.LookRotation(
                /*new Vector3(nextBox.transform.rotation.x - Camera.current.transform.rotation.eulerAngles.x,
                            nextBox.transform.rotation.y - Camera.current.transform.rotation.eulerAngles.y,
                            nextBox.transform.rotation.z - Camera.current.transform.rotation.eulerAngles.z)*/
                nextBox.transform.position - animatedArrow.transform.position
                );

            float time = 0;

            while (time < 1)
            {
                animatedArrow.transform.rotation = Quaternion.Slerp(animatedArrow.transform.rotation, auxQuat, time);
                time += Time.deltaTime * 1;
            }
        }
        else
        {
            if (animatedArrow.activeSelf)
                animatedArrow.SetActive(false);
        }
    }
}