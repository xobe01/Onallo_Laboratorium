using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class TrackerController : MonoBehaviour
{
    [SerializeField] Vector2 resolution;
    Vector3 rendererBounds;
    GameObject projection;
    MeshRenderer tempRend;

    List<Vector3> rotations;
    List<Vector3> sizes;
    List<Vector3> positions;

    private void Start()
    {
        rotations = new List<Vector3>();
        sizes = new List<Vector3>();
        positions = new List<Vector3>();
        tempRend = GetComponent<MeshRenderer>();
        projection = transform.Find("Projection").gameObject;
        rendererBounds = GetComponent<Renderer>().bounds.extents;
        PlaceProjection("86;128;107;176;42;-1;244;");
    }

    public void PlaceProjection(string result)
    {
        if(!projection.activeSelf) SetProjectionEnabled(true);
        List<int> returnValues = new List<int>();
        string helper = "";
        foreach (char c in result)
        {
            if (c == ';')
            {
                returnValues.Add(int.Parse(helper));
                helper = "";
            }
            else
            {
                helper += c;
            }
        }
        if (returnValues.Count != 7) { return; }
        int xMin = returnValues[0];
        int xMax = returnValues[1];
        int yMin = returnValues[2];
        int yMax = returnValues[3];
        int xRot = returnValues[4];
        int yRot = returnValues[6];
        int zRot = returnValues[5];
        float centerX = (xMin + xMax) / 2 * (1600 / resolution.x);
        float centerY = (yMin + yMax) / 2 * (900 / resolution.y);
        Vector3 recvPos = new Vector3(centerX, centerY, transform.parent.position.z);
        Vector3 recvSize = new Vector3((xMax - xMin) / (rendererBounds.x * 2) * (1600 / resolution.x), 1,
            (yMax - yMin) / (rendererBounds.y * 2) * (900 / resolution.y));
        Vector3 recvRot = new Vector3(xRot, yRot, zRot);
        transform.parent.position = getAvg(positions, recvPos);
        transform.localScale = getAvg(sizes, recvSize);
        Vector3 rotHelper = getAvg(rotations, recvRot);
        float diff = rotHelper.y - transform.localRotation.eulerAngles.y;
        if (Mathf.Abs(diff) > 300)
        {
            rotHelper += new Vector3(0, -diff/diff * 360, 0);
        }
        transform.localRotation = Quaternion.Euler(rotHelper);
    }

    Vector3 getAvg(List<Vector3> list, Vector3 recvVector)
    {
        list.Add(recvVector);
        if (list.Count > 5) list.RemoveAt(0);
        Vector3 returnVector = Vector3.zero;
        foreach (Vector3 v in list) returnVector += v;
        returnVector /= list.Count;
        return returnVector;
    }

    public void SetProjectionEnabled(bool value)
    {
        projection.SetActive(value);
        //tempRend.enabled = value;
    }
}
