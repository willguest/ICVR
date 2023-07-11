using System.Collections;
using System.Collections.Generic;
using UnityEngine;


public class VolumeSolver
{
    private Vector3 ObjScale { get; set; }
    private bool IsBusy = false;

    public VolumeSolver(Vector3 scale)
    {
        ObjScale = scale;
    }

    private double SignedVolumeOfTriangle(Vector3 p1, Vector3 p2, Vector3 p3)
    {
        p1 *= ObjScale.x;
        p2 *= ObjScale.y;
        p3 *= ObjScale.z;

        var v321 = p3.x * p2.y * p1.z;
        var v231 = p2.x * p3.y * p1.z;
        var v312 = p3.x * p1.y * p2.z;
        var v132 = p1.x * p3.y * p2.z;
        var v213 = p2.x * p1.y * p3.z;
        var v123 = p1.x * p2.y * p3.z;
        return (1.0f / 6.0f) * (-v321 + v231 + v312 - v132 - v213 + v123);
    }


    public double GetMeshVolume(Mesh mesh)
    {
        IsBusy = true;
        Vector3[] vertices = mesh.vertices;
        int[] triangles = mesh.triangles;

        double volume = 0.0;

        for (int i = 0; i < mesh.triangles.Length; i += 3)
        {
            Vector3 p1 = vertices[triangles[i + 0]];
            Vector3 p2 = vertices[triangles[i + 1]];
            Vector3 p3 = vertices[triangles[i + 2]];

            volume += SignedVolumeOfTriangle(p1, p2, p3);
        }
        IsBusy = false;

        return Mathf.Abs((float)volume);
    }



    private GameObject GetActiveObject(GameObject currentObject)
    {
        // priority: Mesh of self, parent, child, null
        if (currentObject.GetComponent<Mesh>())
        {
            return currentObject;
        }
        else if (currentObject.transform.parent.gameObject)
        {
            return currentObject.transform.parent.gameObject;
        }
        else if (currentObject.GetComponentInChildren<Mesh>())
        {
            return currentObject.GetComponentInChildren<MeshFilter>().gameObject;
        }
        else
        {
            return null;
        }
    }


    public IEnumerator<double> GetMeshVolume(GameObject g)
    {
        GameObject activeG = GetActiveObject(g);
        Vector3 ps = activeG.transform.lossyScale;
        ObjScale = new Vector3(1 / ps.x, 1 / ps.y, 1 / ps.z);

        double vol = GetMeshVolume(activeG.GetComponent<Mesh>());
        new WaitWhile(() => IsBusy);
        yield return vol;
    }

}






