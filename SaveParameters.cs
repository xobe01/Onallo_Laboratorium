using UnityEngine;
using System.Collections;
using System.Collections.Generic;
using System.Text;
using System.IO;

public class SaveParameters : MonoBehaviour
{
    [SerializeField] bool angleTrain;
    [SerializeField] Vector2 resolution;
    [SerializeField] int generateImageCount;
    [SerializeField] Texture2D[] sampleImages;
    [SerializeField] Renderer backGroudRend;
    [SerializeField] GameObject dLight;
    Camera camera;
    Renderer renderer;
    Light dLightLight;
    private List<string[]> trainRowData = new List<string[]>();
    private List<string[]> testRowData = new List<string[]>();
    int trainImageCount = 0;
    int testImageCount = 0;
    int imageCount = 0;

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
            rowDataTemp = new string[8];
            rowDataTemp[0] = "filename";
            rowDataTemp[1] = "width";
            rowDataTemp[2] = "height";
            rowDataTemp[3] = "class";
            rowDataTemp[4] = "xmin";
            rowDataTemp[5] = "ymin";
            rowDataTemp[6] = "xmax";
            rowDataTemp[7] = "ymax";
        }
        trainRowData.Add(rowDataTemp);
        testRowData.Add(rowDataTemp);
        StartCoroutine(PositionPlane());        
    }

    IEnumerator PositionPlane()
    {
        yield return new WaitForEndOfFrame();
        for (int i = 0; i < generateImageCount; i++)
        {
            NewPos();
            yield return new WaitForEndOfFrame();
        }
        SaveCsv();
    }

    void NewPos()
    {
        backGroudRend.transform.rotation = Quaternion.Euler(Random.Range(0, 360),90,90);
        backGroudRend.transform.position = new Vector3(480+Random.Range(-200,200), 270+Random.Range(-200,200), -1000);
        backGroudRend.material.SetTexture("_MainTex", sampleImages[Random.Range(0, sampleImages.Length)]);
        dLightLight.intensity = Random.Range(0.75f, 2);
        float newSize = Random.Range(1, 3f);
        transform.localScale = new Vector3(newSize, newSize, newSize);
        if(!angleTrain) { transform.parent.position = new Vector3(Random.Range(newSize*70, 960- newSize * 70),
            Random.Range(newSize * 70, 540- newSize * 70), -468); } //a resolution 1920*1080, ennek a felét mozoghatja +- valamennyi, hogy kb ne lógjon ki
        transform.localRotation = Quaternion.Euler(Random.Range(-45, 45), Random.Range(0, 360), Random.Range(-45, 45));
        dLight.transform.rotation = Quaternion.Euler(Random.Range(115, 165), Random.Range(-50, 50), 0);
        string[] rowDataTemp;
        if (angleTrain)
        {
            rowDataTemp = new string[3];
            rowDataTemp[0] = Mathf.Round(transform.localRotation.eulerAngles.x + (transform.localRotation.eulerAngles.x > 180 ?
                -315 : 45)).ToString();
            rowDataTemp[1] = Mathf.Round(transform.localRotation.eulerAngles.z + (transform.localRotation.eulerAngles.z > 180 ?
                -315 : 45)).ToString();
            rowDataTemp[2] = Mathf.Round(transform.localRotation.eulerAngles.y).ToString();
        }
        else
        {
            rowDataTemp = new string[8];
            rowDataTemp[0] = ((imageCount % 5 == 0) ? ("test_image_" + testImageCount) : ("train_image_" + trainImageCount)) + ".jpg";
            rowDataTemp[1] = resolution.x.ToString();
            rowDataTemp[2] = resolution.y.ToString();
            rowDataTemp[3] = "tracker";
            rowDataTemp[4] = Mathf.Round((int)Mathf.Max(0, (transform.position.x - renderer.bounds.extents.x) * resolution.x / 960)).ToString();
            rowDataTemp[5] = Mathf.Round((int)Mathf.Max(0, (transform.position.y - renderer.bounds.extents.y) * resolution.y / 540)).ToString();
            rowDataTemp[6] = Mathf.Round((int)Mathf.Min(resolution.x, (transform.position.x + renderer.bounds.extents.x) * resolution.x / 960)).ToString();
            rowDataTemp[7] = Mathf.Round((int)Mathf.Min(resolution.y, (transform.position.y + renderer.bounds.extents.y) * resolution.y / 540)).ToString();
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

        string trainFilePath = "C:/Users/ungbo/neural_network" + (angleTrain? "/angle/data/label.csv" :
            "/models/research/object_detection/images/train_labels.csv");
        string testFilePath = "C:/Users/ungbo/neural_network/models/research/object_detection/images/test_labels.csv";

        StreamWriter trainOutStream = System.IO.File.CreateText(trainFilePath);
        StreamWriter testOutStream = System.IO.File.CreateText(testFilePath);

        trainOutStream.WriteLine(trainSb);
        trainOutStream.Close();

        if (!angleTrain) { testOutStream.WriteLine(testSb); }
        testOutStream.Close();
    }

    string ScreenShotName(bool isTest)
    {
        return string.Format("C:/Users/ungbo/neural_network" + (angleTrain? "/angle/data/images/train_image_" + trainImageCount +
                            ".jpg": "/models/research/object_detection/images" + (isTest?("/test/test_image_" + testImageCount +
                            ".jpg"): ("/train/train_image_" + trainImageCount + ".jpg"))),
                             Application.dataPath,
                             1920, 1080,
                             System.DateTime.Now.ToString("yyyy-MM-dd_HH-mm-ss"));
    }

    IEnumerator CaptureImage()
    {
        yield return new WaitForEndOfFrame();
        bool isTest = imageCount % 5 == 0;
        if (angleTrain)
        {
            float plusFactor = (float)Random.Range(0, 10) / 10;
            int width = (int)(renderer.bounds.extents.x * (4 + plusFactor));
            int height = (int)(renderer.bounds.extents.y * (4 + plusFactor));
            int x = Mathf.Min(1920 - width, (int)(1920 / 2 - renderer.bounds.extents.x * (2 + plusFactor * 0.5f)));
            int y = Mathf.Min(1080 - height, (int)(1080 / 2 - renderer.bounds.extents.y * (2 + plusFactor * 0.5f)));
            Texture2D screencap = new Texture2D(width, height, TextureFormat.RGB24, false);
            screencap.ReadPixels(new Rect(x, y, width, height), 0, 0);
            screencap.Apply();

            byte[] bytes = screencap.EncodeToJPG();
            string filename = ScreenShotName(isTest);
            System.IO.File.WriteAllBytes(filename, bytes);
            Destroy(screencap);
        }
        else
        {
            RenderTexture rt = new RenderTexture(1920, 1080, 24);
            camera.targetTexture = rt;
            Texture2D screenShot = new Texture2D(1920, 1080, TextureFormat.RGB24, false);
            camera.Render();
            RenderTexture.active = rt;
            screenShot.ReadPixels(new Rect(0, 0, 1920, 1080), 0, 0);
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