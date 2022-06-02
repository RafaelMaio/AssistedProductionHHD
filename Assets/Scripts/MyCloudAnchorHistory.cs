// ===============================
// AUTHOR     : Rafael Maio (rafael.maio@ua.pt)
// PURPOSE     : Structures about the cloud anchors and associated game objects to be stored.
// ===============================

using System;
using System.Collections.Generic;
using UnityEngine;

/// <summary>
/// A serializable struct the industrial information of the piece corresponding to the virtual object.
/// </summary>
[Serializable]
public struct IndustrialInfo
{
    /// <summary>
    /// Piece reference.
    /// </summary>
    public string Reference;

    /// <summary>
    /// Piece name.
    /// </summary>
    public string Name;

    /// <summary>
    /// Piece localization code.
    /// </summary>
    public string Localization;

    /// <summary>
    /// Struct Constructor
    /// </summary>
    /// <param name="reference">Piece refence.</param>
    /// <param name="name">Piece name.</param>
    /// <param name="localization">Piece localization code.</param>
    public IndustrialInfo(string reference, string name, string localization)
    {
        Reference = reference;
        Name = name;
        Localization = localization;
    }
}

/// <summary>
/// A serializable struct the basic information of an object attached to the anchor.
/// </summary>
[Serializable]
public struct AnchoredObject
{
    /// <summary>
    /// Object prefab type.
    /// </summary>
    public string Prefab_name;

    /// <summary>
    /// Object x position.
    /// </summary>
    public float X;

    /// <summary>
    /// Object y position.
    /// </summary>
    public float Y;

    /// <summary>
    /// Object z position.
    /// </summary>
    public float Z;

    /// <summary>
    /// List of ocurred rotations in the virtual object.
    /// </summary>
    [SerializeField] public List<Vector3> Rotations;

    /// <summary>
    /// Object x scale.
    /// </summary>
    public float Scale_X;

    /// <summary>
    /// Object y scale.
    /// </summary>
    public float Scale_Y;

    /// <summary>
    /// Object z scale.
    /// </summary>
    public float Scale_Z;

    /// <summary>
    /// The industrial information of the piece corresponding to this virtual object.
    /// </summary>
    public IndustrialInfo PieceInfo;

    /// <summary>
    /// Struct constructor.
    /// </summary>
    /// <param name="prefab_name">Object prefab type.</param>
    /// <param name="x">Object x position.</param>
    /// <param name="y">Object y position.</param>
    /// <param name="z">Object z position.</param>
    /// <param name="rots">List of ocurred rotations..</param>
    /// <param name="scale_x">Object x scale.</param>
    /// <param name="scale_y">Object y scale.</param>
    /// <param name="scale_z">Object z scale.</param>
    /// <param name="industrialInfo">The industrial information of the piece corresponding to this virtual object.</param>
    public AnchoredObject(string prefab_name, float x, float y, float z, List<Vector3> rots, float scale_x, float scale_y, float scale_z, IndustrialInfo industrialInfo)
    {
        Prefab_name = prefab_name;
        X = x;
        Y = y;
        Z = z;
        Rotations = rots;
        Scale_X = scale_x;
        Scale_Y = scale_y;
        Scale_Z = scale_z;
        PieceInfo = industrialInfo;
    }
}


/// <summary>
/// A serializable struct that stores the basic information of a persistent cloud anchor.
/// Adapted from ARFoundation -> CloudAnchorHistory.
/// </summary>
[Serializable]
public struct MyCloudAnchorHistory
{
    /// <summary>
    /// An informative name given by the user.
    /// </summary>
    public string Name;

    /// <summary>
    /// The Cloud Anchor Id which is used for resolving.
    /// </summary>
    public string Id;

    /// <summary>
    /// The created time of this Cloud Anchor.
    /// </summary>
    public string SerializedTime;

    /// <summary>
    /// The scenario where Cloud Anchor is hosted.
    /// </summary>
    public string scenarioName;

    /// <summary>
    /// The scenario where Cloud Anchor is hosted.
    /// </summary>
    public List<AnchoredObject> listOfAnchoredObjects;

    /// <summary>
    /// Construct a Cloud Anchor history.
    /// </summary>
    /// <param name="name">An informative name given by the user.</param>
    /// <param name="id">The Cloud Anchor Id which is used for resolving.</param>
    /// <param name="time">The time this Cloud Anchor was created.</param>
    public MyCloudAnchorHistory(string name, string id, DateTime time, string scenario_name, List<AnchoredObject> anchoredObjects)
    {
        Name = name;
        Id = id;
        SerializedTime = time.ToString();
        scenarioName = scenario_name;
        listOfAnchoredObjects = anchoredObjects;
    }

    /// <summary>
    /// Construct a Cloud Anchor history.
    /// </summary>
    /// <param name="name">An informative name given by the user.</param>
    /// <param name="id">The Cloud Anchor Id which is used for resolving.</param>
    public MyCloudAnchorHistory(string name, string id, string scenario_name, List<AnchoredObject> anchoredObjects) : this(name, id, DateTime.Now, scenario_name, anchoredObjects)
    {
    }

    /// <summary>
    /// Gets created time in DataTime format.
    /// </summary>
    public DateTime CreatedTime
    {
        get
        {
            return Convert.ToDateTime(SerializedTime);
        }
    }

    /// <summary>
    /// Overrides ToString() method.
    /// </summary>
    /// <returns>Return the json string of this object.</returns>
    public override string ToString()
    {
        return JsonUtility.ToJson(this);
    }
}

/// <summary>
/// A wrapper class for serializing a collection of <see cref="MyCloudAnchorHistory"/>.
/// </summary>
[Serializable]
public class MyCloudAnchorHistoryCollection
{
    /// <summary>
    /// A list of Cloud Anchor History Data.
    /// </summary>
    public List<MyCloudAnchorHistory> Collection = new List<MyCloudAnchorHistory>();

    /// <summary>
    /// List of scenarios for filling the dropdown.
    /// </summary>
    public List<string> dropdownScenarios = new List<string>();
}