using UnityEngine;
using System.Collections;
using System.Collections.Generic;
using System.Text;
using System.IO;

public class SaveParameters : MonoBehaviour
{
    [SerializeField] bool angleTrain;
    [SerializeField] Texture2D[] trackerImages;
    [SerializeField] Texture2D[] sampleImages;
    [SerializeField] Renderer backGroudRend;
    [SerializeField] GameObject dLight;
    Camera camera;
    Renderer renderer;
    Light dLightLight;
    private List<string[]> trainRowData = new List<string[]>();
    private List<string[]> testRowData = new List<string[]>();
    int trainImageCount = 30000;
    int testImageCount = 30000;
    int imageCount = 30000;
    int resWidth = 800;
    int resHeight = 600;

    void Start()
    {
        dLightLight = dLight.GetComponent<Light>();
        camera = FindObjectOfType<Camera>();
        renderer = GetComponent<Renderer>();
        string[] rowDataTemp;
        if (angleTrain)
        {
            rowDataTemp = new string[3];
            rowDataTemp[0] = "xRot";
            rowDataTemp[1] = "yRot";
            rowDataTemp[2] = "zRot";
        }
        else
        {
            rowDataTemp = new string[11];
            rowDataTemp[0] = "filename";
            rowDataTemp[1] = "width";
            rowDataTemp[2] = "height";
            rowDataTemp[3] = "class";
            rowDataTemp[4] = "xmin";
            rowDataTemp[5] = "ymin";
            rowDataTemp[6] = "xmax";
            rowDataTemp[7] = "ymax";
            rowDataTemp[8] = "xrot";
            rowDataTemp[9] = "yrot";
            rowDataTemp[10] = "zrot";
        }
        trainRowData.Add(rowDataTemp);
        testRowData.Add(rowDataTemp);
        StartCoroutine(PositionPlane());
    }

    IEnumerator PositionPlane()
    {
        yield return new WaitForEndOfFrame();
        for (int i = 0; i < 10000; i++)
        {
            NewPos();
            yield return new WaitForEndOfFrame();
        }
        SaveCsv();
    }

    void NewPos()
    {
        //renderer.material.SetTexture("_MainTex", trackerImages[Random.Range(0, trackerImages.Length)]);
        backGroudRend.material.SetTexture("_MainTex", sampleImages[Random.Range(0, sampleImages.Length)]);
        dLightLight.intensity = Random.Range(0.5f, 2);
        float newSize = Random.Range(0.5f, 1f);
        transform.localScale = new Vector3(newSize, newSize, newSize);
        if(!angleTrain) { transform.parent.transform.position = new Vector3(Random.Range(0, resWidth), Random.Range(0, resHeight), -500); }
        transform.localRotation = Quaternion.Euler(Random.Range(-75, 75), Random.Range(0, 360), Random.Range(-75, 75));
        dLight.transform.rotation = Quaternion.Euler(Random.Range(115, 165), Random.Range(-50, 50), 0);
        string[] rowDataTemp;
        if (angleTrain)
        {
            rowDataTemp = new string[3];
            rowDataTemp[0] = Mathf.Round(transform.localRotation.eulerAngles.x + (transform.localRotation.eulerAngles.x > 180 ?
                -285 : 75)).ToString();
            rowDataTemp[1] = Mathf.Round(transform.localRotation.eulerAngles.z + (transform.localRotation.eulerAngles.z > 180 ?
                -285 : 75)).ToString();
            rowDataTemp[2] = Mathf.Round(transform.localRotation.eulerAngles.y).ToString();
        }
        else
        {
            rowDataTemp = new string[11];
            rowDataTemp[0] = ((imageCount % 5 == 0) ? ("test_image_" + testImageCount) : ("train_image_" + trainImageCount)) + ".jpg";
            rowDataTemp[1] = resWidth.ToString();
            rowDataTemp[2] = resHeight.ToString();
            rowDataTemp[3] = "Ticket";
            rowDataTemp[4] = Mathf.Round(Mathf.Max(0, (transform.position.x - renderer.bounds.extents.x))).ToString();
            rowDataTemp[5] = Mathf.Round(Mathf.Max(0, (transform.position.y - renderer.bounds.extents.y))).ToString();
            rowDataTemp[6] = Mathf.Round(Mathf.Min(resWidth, (transform.position.x + renderer.bounds.extents.x))).ToString();
            rowDataTemp[7] = Mathf.Round(Mathf.Min(resHeight, (transform.position.y + renderer.bounds.extents.y))).ToString();
            rowDataTemp[8] = Mathf.Round(transform.rotation.eulerAngles.x).ToString();
            rowDataTemp[9] = Mathf.Round(transform.rotation.eulerAngles.y).ToString();
            rowDataTemp[10] = Mathf.Round(transform.rotation.eulerAngles.z).ToString();
        }
        if (imageCount % 5 == 0 && !angleTrain)
        {
            testRowData.Add(rowDataTemp);
        }
        else
        {
            trainRowData.Add(rowDataTemp);
        }
        StartCoroutine(CaptureImage());
    }

    void SaveCsv()
	{
        string[][] trainOutput = new string[trainRowData.Count][];
        string[][] testOutput = new string[testRowData.Count][];

        for (int i = 0; i < trainOutput.Length; i++)
        {
            trainOutput[i] = trainRowData[i];
        }

        for (int i = 0; i < testOutput.Length; i++)
        {
            testOutput[i] = testRowData[i];
        }

        int trainLength = trainOutput.GetLength(0);
        int testLength = testOutput.GetLength(0);

        string delimiter = ",";

        StringBuilder trainSb = new StringBuilder();
        StringBuilder testSb = new StringBuilder();

        for (int index = 0; index < trainLength; index++)
            trainSb.AppendLine(string.Join(delimiter, trainOutput[index]));

        for (int index = 0; index < testLength; index++)
            testSb.AppendLine(string.Join(delimiter, testOutput[index]));

        string trainFilePath = "/Users/reimholz/neural_network"+(angleTrain? "/angle/data/label.csv" : "/models/research/object_detection/images/train_labels.csv");
        string testFilePath = "/Users/reimholz/neural_network/models/research/object_detection/images/test_labels.csv";

        StreamWriter trainOutStream = System.IO.File.CreateText(trainFilePath);
        StreamWriter testOutStream = System.IO.File.CreateText(testFilePath);

        trainOutStream.WriteLine(trainSb);
        trainOutStream.Close();

        if (!angleTrain) { testOutStream.WriteLine(testSb); }
        testOutStream.Close();
    }

    string ScreenShotName(bool isTest)
    {
        return string.Format("/Users/reimholz/neural_network"+(angleTrain? "/angle/data/images/train_image_" + trainImageCount +
                            ".jpg": "/models/research/object_detection/images" + (isTest?("/test/test_image_" + testImageCount +
                            ".jpg"): ("/train/train_image_" + trainImageCount + ".jpg"))),
                             Application.dataPath,
                             resWidth, resHeight,
                             System.DateTime.Now.ToString("yyyy-MM-dd_HH-mm-ss"));
    }

    IEnumerator CaptureImage()
    {
        yield return new WaitForEndOfFrame();
        bool isTest = imageCount % 5 == 0;
        if (angleTrain)
		{
            Texture2D screencap;
            screencap = new Texture2D((int)(renderer.bounds.extents.x*2.4f), (int)(renderer.bounds.extents.y*2.4f), TextureFormat.RGB24, false);
            screencap.ReadPixels(new Rect(400 - renderer.bounds.extents.x - (int)renderer.bounds.extents.x * 0.2f,
                300 - renderer.bounds.extents.y - (int)renderer.bounds.extents.y * 0.2f,
                400 + renderer.bounds.extents.x + (int)renderer.bounds.extents.x * 0.2f,
                300 + renderer.bounds.extents.y + (int)renderer.bounds.extents.y * 0.2f), 0, 0) ;
            screencap.Apply();

            byte[] bytes = screencap.EncodeToJPG();
            string filename = ScreenShotName(isTest);
            System.IO.File.WriteAllBytes(filename, bytes);
        }
		else
		{
            RenderTexture rt = new RenderTexture(resWidth, resHeight, 24);
            camera.targetTexture = rt;
            Texture2D screenShot = new Texture2D(resWidth, resHeight, TextureFormat.RGB24, false);
            camera.Render();
            RenderTexture.active = rt;
            screenShot.ReadPixels(new Rect(0, 0, resWidth, resHeight), 0, 0);
            camera.targetTexture = null;
            RenderTexture.active = null;
            Destroy(rt);
            byte[] bytes = screenShot.EncodeToJPG();
            string filename = ScreenShotName(isTest);
            System.IO.File.WriteAllBytes(filename, bytes);
        }
        if (imageCount % 5 == 0 && !angleTrain)
        {
            testImageCount++;
        }
        else
        {
            trainImageCount++;
        }
        imageCount++;
        print(imageCount);
    }
}