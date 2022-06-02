// ===============================
// AUTHOR     : Rafael Maio (rafael.maio@ua.pt)
// PURPOSE     : Game Object rotation.
// SPECIAL NOTES: Adapted from LeanTwistRotateAxusAux.cs Lean Script -> saves the occurred rotation values.
// ===============================

using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using Lean.Touch;

public class LeanTwistRotateAxisAux : MonoBehaviour
{

    /// <summary>The method used to find fingers to use with this component. See LeanFingerFilter documentation for more information.</summary>
    public LeanFingerFilter Use = new LeanFingerFilter(true);

    /// <summary>The axis of rotation.</summary>
    public Vector3 Axis { set { axis = value; } get { return axis; } }
    [SerializeField] private Vector3 axis = Vector3.down;

    /// <summary>Rotate locally or globally?</summary>
    public Space Space { set { space = value; } get { return space; } }
    [SerializeField] private Space space = Space.Self;

    /// <summary>The sensitivity of the rotation.
    /// 1 = Default.
    /// 2 = Double.</summary>
    public float Sensitivity { set { sensitivity = value; } get { return sensitivity; } }
    [SerializeField] private float sensitivity = 1.0f;

    public AppMainScript appMainScript;

    /// <summary>If you've set Use to ManuallyAddedFingers, then you can call this method to manually add a finger.</summary>
    public void AddFinger(LeanFinger finger)
    {
        Use.AddFinger(finger);
    }

    /// <summary>If you've set Use to ManuallyAddedFingers, then you can call this method to manually remove a finger.</summary>
    public void RemoveFinger(LeanFinger finger)
    {
        Use.RemoveFinger(finger);
    }

    /// <summary>If you've set Use to ManuallyAddedFingers, then you can call this method to manually remove all fingers.</summary>
    public void RemoveAllFingers()
    {
        Use.RemoveAllFingers();
    }

#if UNITY_EDITOR
    protected virtual void Reset()
    {
        Use.UpdateRequiredSelectable(gameObject);
    }
#endif

    protected virtual void Awake()
    {
        Use.UpdateRequiredSelectable(gameObject);
    }

    protected virtual void Update()
    {
        // Get the fingers we want to use
        var fingers = Use.UpdateAndGetFingers();

        // Calculate the rotation values based on these fingers
        var twistDegrees = LeanGesture.GetTwistDegrees(fingers) * sensitivity;

        // Perform rotation
        transform.Rotate(axis, twistDegrees, space);

        // Informs the main script that a rotation occurred
        if (twistDegrees != 0)
        {
            appMainScript.rotationOcurred(twistDegrees);
        }
    }
}