// ===============================
// AUTHOR     : Rafael Maio (rafael.maio@ua.pt)
// PURPOSE     : Contains the JSON structure for deserialization - Desktop configuration module.
// ===============================

using System;

/// <summary>
/// A wrapper class for serializing a collection of <see cref="boxJson"/>.
/// </summary>
[Serializable]
public struct boxesJsonStruct
{
    /// <summary>
    /// A list of boxes.
    /// </summary>
    public boxJson[] objects;
}

/// <summary>
/// A serializable struct that stores the basic information of a box.
/// </summary>
[Serializable]
public struct boxJson
{
    /// <summary>
    /// An informative name given to the box.
    /// </summary>
    public string box_name;

    /// <summary>
    /// Box position.
    /// </summary>
    public boxJsonPostionAndScale box_position;

    /// <summary>
    /// Box rotation.
    /// </summary>
    public boxJsonRotation box_rotation;

    /// <summary>
    /// Box scaling.
    /// </summary>
    public boxJsonPostionAndScale box_scale;

    /// <summary>
    /// The industrial information of the piece corresponding to this virtual object.
    /// </summary>
    public boxJsonIndustrialInfo tag;
}

/// <summary>
/// A serializable struct that stores the box postion and scaling axis.
/// </summary>
[Serializable]
public struct boxJsonPostionAndScale
{
    /// <summary>
    /// Box x postion/scale.
    /// </summary>
    public string x;

    /// <summary>
    /// Box y postion/scale.
    /// </summary>
    public string y;

    /// <summary>
    /// Box z postion/scale.
    /// </summary>
    public string z;
}

/// <summary>
/// A serializable struct that stores the box orientation.
/// </summary>
[Serializable]
public struct boxJsonRotation
{
    /// <summary>
    /// Box x rotation.
    /// </summary>
    public string x;

    /// <summary>
    /// Box y rotation;
    /// </summary>
    public string y;

    /// <summary>
    /// Box z rotation;
    /// </summary>
    public string z;

    /// <summary>
    /// Box w rotation;
    /// </summary>
    public string w;
}


/// <summary>
/// A serializable struct the industrial information of the piece corresponding to the virtual object.
/// </summary>
[Serializable]
public struct boxJsonIndustrialInfo
{
    /// <summary>
    /// Piece name.
    /// </summary>
    public string part_name;

    /// <summary>
    /// Piece reference.
    /// </summary>
    public string part_reference;

    /// <summary>
    /// Piece localization code.
    /// </summary>
    public string part_location;
}