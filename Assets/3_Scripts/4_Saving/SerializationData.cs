// --- Data Transfer Objects (DTOs) for Unity Types ---

[System.Serializable]
public class Vector3Data
{
    public float x, y, z;

    // Use a different name for the constructor parameter to avoid ambiguity.
    // An underscore prefix is a common convention.
    public Vector3Data(UnityEngine.Vector3 _vector)
    {
        x = _vector.x;
        y = _vector.y;
        z = _vector.z;
    }

    // A parameterless constructor is good practice for serialization.
    public Vector3Data() { }

    public UnityEngine.Vector3 ToVector3()
    {
        return new UnityEngine.Vector3(x, y, z);
    }
}

[System.Serializable]
public class QuaternionData
{
    public float x, y, z, w;

    // Use a different name for the constructor parameter.
    public QuaternionData(UnityEngine.Quaternion _quaternion)
    {
        x = _quaternion.x;
        y = _quaternion.y;
        z = _quaternion.z;
        w = _quaternion.w;
    }

    // A parameterless constructor is good practice for serialization.
    public QuaternionData() { }

    public UnityEngine.Quaternion ToQuaternion()
    {
        return new UnityEngine.Quaternion(x, y, z, w);
    }
}