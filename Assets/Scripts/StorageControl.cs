// ===============================
// AUTHOR     : Rafael Maio (rafael.maio@ua.pt)
// PURPOSE     : Static functions: Store cloud anchor/scenarios information and mathmatical functions;
// ===============================

using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class StorageControl : MonoBehaviour
{

    /// <summary>
    /// The key name used in PlayerPrefs which stores persistent Cloud Anchors history data.
    /// Expired data will be cleared at runtime.
    /// </summary>
    private const string _persistentCloudAnchorsStorageKey = "PersistentCloudAnchors";

    /// <summary>
    /// The limitation of how many Cloud Anchors can be stored in local storage.
    /// </summary>
    private const int _storageLimit = 40;

    /// <summary>
    /// Save the persistent Cloud Anchors history to local storage,
    /// also remove the oldest data if current storage has met maximal capacity.
    /// </summary>
    /// <param name="data">The Cloud Anchor history data needs to be stored.</param>
    public static void SaveCloudAnchorHistory(MyCloudAnchorHistory data)
    {
        var history = LoadCloudAnchorHistory();

        // Sort the data from latest record to oldest record which affects the option order in
        // multiselection dropdown.
        history.Collection.Add(data);
        history.Collection.Sort((left, right) => right.CreatedTime.CompareTo(left.CreatedTime));

        // Remove the oldest data if the capacity exceeds storage limit.
        if (history.Collection.Count > _storageLimit)
        {
            history.Collection.RemoveRange(
                _storageLimit, history.Collection.Count - _storageLimit);
        }

        PlayerPrefs.SetString(_persistentCloudAnchorsStorageKey, JsonUtility.ToJson(history));
    }

    /// <summary>
    /// Update the persistent Cloud Anchors history to local storage,
    /// also remove the oldest data with the same anchor id.
    /// </summary>
    /// <param name="data">The Cloud Anchor history data needs to be stored.</param>
    public static void UpdateCloudAnchorHistory(MyCloudAnchorHistory data)
    {
        var history = LoadCloudAnchorHistory();
        MyCloudAnchorHistory previousData = new MyCloudAnchorHistory();

        //Removes the oldest version of the data;
        foreach(var h in history.Collection)
        {
            if(h.Id == data.Id)
            {
                previousData = h;
                break;
            }
        }
        history.Collection.Remove(previousData);

        // Sort the data from latest record to oldest record which affects the option order in
        // multiselection dropdown.
        history.Collection.Add(data);
        history.Collection.Sort((left, right) => right.CreatedTime.CompareTo(left.CreatedTime));

        PlayerPrefs.SetString(_persistentCloudAnchorsStorageKey, JsonUtility.ToJson(history));
    }

    /// <summary>
    /// Load the persistent Cloud Anchors history from local storage,
    /// also remove outdated records and update local history data. 
    /// </summary>
    /// <returns>A collection of persistent Cloud Anchors history data.</returns>
    public static MyCloudAnchorHistoryCollection LoadCloudAnchorHistory()
    {
        if (PlayerPrefs.HasKey(_persistentCloudAnchorsStorageKey))
        {
            var history = JsonUtility.FromJson<MyCloudAnchorHistoryCollection>(
                PlayerPrefs.GetString(_persistentCloudAnchorsStorageKey));

            // Remove all records created more than 1 year and update stored history.
            DateTime current = DateTime.Now;
            history.Collection.RemoveAll(
                data => current.Subtract(data.CreatedTime).Days > 365);
            PlayerPrefs.SetString(_persistentCloudAnchorsStorageKey,
                JsonUtility.ToJson(history));
            return history;
        }

        return new MyCloudAnchorHistoryCollection();
    }

    /// <summary>
    /// Load the saved scenarios from local storage.
    /// </summary>
    /// <returns>A list of scenarios for the scenarios dropdown.</returns>
    public static List<string> LoadScenariosDDHistory()
    {
        if (PlayerPrefs.HasKey(_persistentCloudAnchorsStorageKey))
        {
            var history = JsonUtility.FromJson<MyCloudAnchorHistoryCollection>(
                PlayerPrefs.GetString(_persistentCloudAnchorsStorageKey));
            return history.dropdownScenarios;
        }
        return new List<string>();
    }

    /// <summary>
    /// Save a new scenario.
    /// </summary>
    /// <param name="option">The new scenario to be stored.</param>
    public static void SaveDropdownHistory(string option)
    {
        var history = LoadCloudAnchorHistory();
        history.dropdownScenarios.Add(option);
        PlayerPrefs.SetString(_persistentCloudAnchorsStorageKey, JsonUtility.ToJson(history));
    }

    /// <summary>
    /// Remove the selected scenario.
    /// </summary>
    /// <param name="option">The scenario name to be removed.</param>
    public static void RemoveDropdownHistory(string option)
    {
        var history = LoadCloudAnchorHistory();
        history.dropdownScenarios.Remove(option);
        PlayerPrefs.SetString(_persistentCloudAnchorsStorageKey, JsonUtility.ToJson(history));
    }

    /// <summary>
    /// Clear the anchors on the selected scenario storage.
    /// </summary>
    /// <param name="scenarioToHost">Selected scenario.</param>
    public static void ClearAnchors(string scenarioToHost)
    {
        var history = LoadCloudAnchorHistory();
        List<MyCloudAnchorHistory> toRemove = new List<MyCloudAnchorHistory>();
        foreach (var cloud in history.Collection)
        {
            if (cloud.scenarioName == scenarioToHost)
            {
                toRemove.Add(cloud);
            }
        }
        foreach (var cloud in toRemove)
        {
            history.Collection.Remove(cloud);
        }
        PlayerPrefs.SetString(_persistentCloudAnchorsStorageKey, JsonUtility.ToJson(history));
    }

    /// <summary>
    /// Convert from degreens to radians.
    /// </summary>
    /// <param name="angle">Angle in degrees.</param>
    /// <returns>The angle in radians.</returns>
    public static double ConvertToRadians(double angle)
    {
        return (Math.PI / 180) * angle;
    }

    /// <summary>
    /// Convert from radians to degrees
    /// </summary>
    /// <param name="angle">Angle in radians.</param>
    /// <returns>The angle in degrees.</returns>
    public static float ConvertToDegrees(double angle)
    {
        return (float)(angle * (180 / Math.PI));
    }

    /// <summary>
    /// Computes the rotation matrix around the X axis.
    /// </summary>
    /// <param name="aAngleRad">X angle in radians.</param>
    /// <returns>The rotation matrix around the X axis.</returns>
    public static Matrix4x4 RotateX(float aAngleRad)
    {
        Matrix4x4 m = Matrix4x4.identity;     //  1   0   0   0 
        m.m11 = m.m22 = Mathf.Cos(aAngleRad); //  0  cos -sin 0
        m.m21 = Mathf.Sin(aAngleRad);         //  0  sin  cos 0
        m.m12 = -m.m21;                       //  0   0   0   1
        return m;
    }

    /// <summary>
    /// Computes the rotation matrix around the Y axis.
    /// </summary>
    /// <param name="aAngleRad">Y angle in radians.</param>
    /// <returns>The rotation matrix around the Y axis.</returns>
    public static Matrix4x4 RotateY(float aAngleRad)
    {
        Matrix4x4 m = Matrix4x4.identity;     // cos  0  sin  0
        m.m00 = m.m22 = Mathf.Cos(aAngleRad); //  0   1   0   0
        m.m02 = Mathf.Sin(aAngleRad);         //-sin  0  cos  0
        m.m20 = -m.m02;                       //  0   0   0   1
        return m;
    }

    /// <summary>
    /// Computes the rotation matrix around the Z axis.
    /// </summary>
    /// <param name="aAngleRad">Z angle in radians.</param>
    /// <returns>The rotation matrix around the Z axis.</returns>
    public static Matrix4x4 RotateZ(float aAngleRad)
    {
        Matrix4x4 m = Matrix4x4.identity;     // cos -sin 0   0
        m.m00 = m.m11 = Mathf.Cos(aAngleRad); // sin  cos 0   0
        m.m10 = Mathf.Sin(aAngleRad);         //  0   0   1   0
        m.m01 = -m.m10;                       //  0   0   0   1
        return m;
    }

    /// <summary>
    /// Computes the rotation matrix around all axis.
    /// </summary>
    /// <param name="aEulerAngles">X, Y and Z angles in degrees.</param>
    /// <returns>The rotation matrix around all axis.</returns>
    public static Matrix4x4 Rotate(Vector3 aEulerAngles)
    {
        var rad = aEulerAngles * Mathf.Deg2Rad;
        return RotateZ(rad.z) * RotateX(rad.x) * RotateY(rad.y);
    }

    /// <summary>
    /// Mod operator calculation.
    /// </summary>
    /// <param name="x">Right operand.</param>
    /// <param name="m">Left operand.</param>
    /// <returns>Mod operand result.</returns>
    public static float mod(float x, float m)
    {
        float r = x % m;
        return r < 0 ? r + m : r;
    }

    /// <summary>
    /// Calculate the straight forward 3d distance between two vectors.
    /// </summary>
    /// <param name="g">First vector.</param>
    /// <param name="h">Second vector.</param>
    /// <returns>The straight forward 3d distance between the vectors.</returns>
    public static double calculate3dDistance(Vector3 g, Vector3 h)
    {
        return Mathf.Sqrt(Mathf.Pow(g.x - h.x, 2) + Mathf.Pow(g.z - h.z, 2) + Mathf.Pow(g.y - h.y, 2));
    }

    /// <summary>
    /// Calculate the closest object in a list to a specif gmaeobject (probabily the camera).
    /// </summary>
    /// <param name="cam">Specif gameObject to calculate the distance from.</param>
    /// <param name="gos">List of gameobjects to verify which is closest to "cam".</param>
    /// <returns></returns>
    public static GameObject closestGameobjectToCamera(Vector3 cam, List<GameObject> gos)
    {
        GameObject closestGo = gos[0];
        double closestDistance = Double.MaxValue;
        foreach(var go in gos)
        {
            double dist = calculate3dDistance(cam, go.transform.position);
            if(dist < closestDistance)
            {
                closestDistance = dist;
                closestGo = go;
            }
        }
        return closestGo;
    }
}