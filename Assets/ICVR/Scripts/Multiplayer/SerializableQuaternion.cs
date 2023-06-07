using UnityEngine;
using System;

/// <summary> Serializable version of UnityEngine.Quaternion. </summary>
[Serializable]
public struct SQuaternion
{
    public float x;
    public float y;
    public float z;
    public float w;

    public SQuaternion(float x, float y, float z, float w)
    {
        this.x = x;
        this.y = y;
        this.z = z;
        this.w = w;
    }

    public override string ToString()
        => $"[{x}, {y}, {z}, {w}]";

    public static implicit operator Quaternion(SQuaternion s)
        => new Quaternion(s.x, s.y, s.z, s.w);

    public static implicit operator SQuaternion(Quaternion q)
        => new SQuaternion(q.x, q.y, q.z, q.w);
}

/// <summary> Serializable version of UnityEngine.Color32 without transparency. </summary>
[Serializable]
public struct SColor32
{
    public byte r;
    public byte g;
    public byte b;

    public SColor32(byte r, byte g, byte b)
    {
        this.r = r;
        this.g = g;
        this.b = b;
    }

    public SColor32(Color32 c)
    {
        r = c.r;
        g = c.g;
        b = c.b;
    }

    public override string ToString()
        => $"[{r}, {g}, {b}]";

    public static implicit operator Color32(SColor32 rValue)
        => new Color32(rValue.r, rValue.g, rValue.b, a: byte.MaxValue);

    public static implicit operator SColor32(Color32 rValue)
        => new SColor32(rValue.r, rValue.g, rValue.b);
}