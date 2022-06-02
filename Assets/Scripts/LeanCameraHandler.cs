// ===============================
// AUTHOR     : Rafael Maio (rafael.maio@ua.pt)
// PURPOSE     : Associates the camera to lean scripts.
//               Other brief associations are done.
// ===============================

using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using Lean.Touch;
using System;

public class LeanCameraHandler : MonoBehaviour
{
    /// <summary>
    /// Device camera.
    /// </summary>
    private GameObject cam;

    /// <summary>
    /// Device camera.
    /// </summary>
    private GameObject arsession;

    /// <summary>
    /// Configure Menu Script
    /// To acess toggle, rotation axis.
    /// </summary>
    private ConfigurationMenuScript configureMenuScript;

    /// <summary>
    /// Unity Start function.
    /// </summary>
    void Start()
    {
        try
        {
            cam = GameObject.Find("AR Camera");
            arsession = GameObject.Find("AR Session");
            configureMenuScript = GameObject.Find("ConfigurationMenu").GetComponent<ConfigurationMenuScript>();
        }
        catch { }
        try
        {
            if (this.GetComponent<LeanTwistRotateAxisAux>() != null)
            {
                this.GetComponent<LeanTwistRotateAxisAux>().enabled = configureMenuScript.rotScaleSlider.value == 0;
                this.GetComponent<LeanTwistRotateAxisAux>().Axis = new Vector3(-Convert.ToInt32(configureMenuScript.rotXYZSlider.value == 0),
                                                                        -Convert.ToInt32(configureMenuScript.rotXYZSlider.value == 1),
                                                                        -Convert.ToInt32(configureMenuScript.rotXYZSlider.value == 2));
            }
            this.GetComponent<LeanTwistRotateAxisAux>().appMainScript = arsession.GetComponent<AppMainScript>();
        }
        catch { }
        try
        {
            if (this.GetComponent<LeanPinchScale>() != null)
            {
                this.GetComponent<LeanPinchScale>().Camera = cam.GetComponent<Camera>();
                this.GetComponent<LeanPinchScale>().enabled = configureMenuScript.rotScaleSlider.value == 1;
            }
        }
        catch { }
        try
        {
            if (this.GetComponent<LeanDragTranslateBlock>() != null)
            {
                this.GetComponent<LeanDragTranslateBlock>().Camera = cam.GetComponent<Camera>();
                this.GetComponent<LeanDragTranslateBlock>().confMenuScript = configureMenuScript;
            }
        }
        catch { }
        try
        {
            if (this.GetComponent<LeanDragTranslate>() != null)
            {
                this.GetComponent<LeanDragTranslate>().Camera = cam.GetComponent<Camera>();
            }
        }
        catch { }
    }
}