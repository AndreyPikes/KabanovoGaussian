using System;
using UnityEngine;

[Serializable]
public enum ObjectType
{
    NotSet,
    Tree,
    Bush,
    Flower,
    Engeneering,
    Building
}

[Serializable]
public class InteractiveObjectDto
{
    public Vector3 Position;
    public Vector3 Scale;
    public Quaternion Rotation;
    public ObjectType ObjectType;
    public string Name;
    [TextArea]
    public string Description;
    public int Year;
}
