using System.Collections;
using System.Collections.Generic;
using System.IO;
using UnityEngine;
using UnityEngine.UI;

public class ScreenshotAPI
{
    private int captureWidth = 1920;
    private int captureHeight = 1080;
    public enum Format { RAW, JPG, PNG, PPM };
    public Format format = Format.JPG;
    private string outputFolder;
    private Rect rect;
    private RenderTexture renderTexture;
    private Texture2D screenShot;
    public bool isProcessing;
    private string _fileName;

    private static ScreenshotAPI _instance;

    private ScreenshotAPI() { }

    public static ScreenshotAPI GetInstance()
    {
        if(_instance == null)
        {
            _instance = new ScreenshotAPI();
        }

        return _instance;
    }

    public void Initialize()
    {
        outputFolder = Application.persistentDataPath + "/Screenshots/";
        if (!Directory.Exists(outputFolder))
        {
            Directory.CreateDirectory(outputFolder);
            Debug.Log("Save Path will be : " + outputFolder);
        }
    }

    private string CreateFileName(int width, int height)
    {
        string timestamp = System.DateTime.Now.ToString("yyyyMMddTHHmmss");
        _fileName = string.Format("{0}/screen_{1}x{2}_{3}.{4}", outputFolder, width, height, timestamp, format.ToString().ToLower());
        return _fileName;
    }

    private void CaptureScreenshot()
    {
        isProcessing = true;

        if (renderTexture == null)
        {
            rect = new Rect(0, 0, captureWidth, captureHeight);
            renderTexture = new RenderTexture(captureWidth, captureHeight, 24);
            screenShot = new Texture2D(captureWidth, captureHeight, TextureFormat.RGB24, false);
            //Sprite NewSprite = Sprite.Create(screenShot, new Rect(0, 0, captureWidth, captureHeight), new Vector2(0, 0), 100.0f, 0, SpriteMeshType.Tight);
            //_image.sprite = NewSprite;
        }

        Camera camera = Camera.main;
        camera.targetTexture = renderTexture;
        camera.Render();

        RenderTexture.active = renderTexture;
        screenShot.ReadPixels(rect, 0, 0);

        //Sprite NewSprite = Sprite.Create(screenShot, new Rect(0, 0, captureWidth, captureHeight), new Vector2(0, 0), 100.0f, 0, SpriteMeshType.Tight);
        //_image.sprite = NewSprite;

        camera.targetTexture = null;
        RenderTexture.active = null;

        string filename = CreateFileName((int)rect.width, (int)rect.height);

        byte[] fileHeader = null;
        byte[] fileData = null;

        if (format == Format.RAW)
        {
            fileData = screenShot.GetRawTextureData();
        }
        else if (format == Format.PNG)
        {
            fileData = screenShot.EncodeToPNG();
        }
        else if (format == Format.JPG)
        {
            fileData = screenShot.EncodeToJPG();
        }
        else 
        {
            string headerStr = string.Format("P6\n{0} {1}\n255\n", rect.width, rect.height);
            fileHeader = System.Text.Encoding.ASCII.GetBytes(headerStr);
            fileData = screenShot.GetRawTextureData();
        }

        // create new thread to offload the saving from the main thread
        new System.Threading.Thread(() =>
        {
            var file = System.IO.File.Create(filename);
            if (fileHeader != null)
            {
                file.Write(fileHeader, 0, fileHeader.Length);
            }
            file.Write(fileData, 0, fileData.Length);
            file.Close();
            Debug.Log(string.Format("Screenshot Saved {0}, size {1}", filename, fileData.Length));
            isProcessing = false;
        }).Start();

        //Cleanup
        //Destroy(renderTexture);
        renderTexture = null;
        screenShot = null;

        //StartCoroutine(LoadSprite(filename));
       // _currentSprite = LoadNewSprite();
    }

    //public Sprite GetSpriteAsync()
    //{
    //    Sprite sprite = null;
    //
    //    if (!isProcessing)
    //    {
    //        sprite = LoadNewSprite(_fileName);
    //    }
    //
    //    return sprite;
    //}

    public Sprite LoadSprite()
    {
        return LoadNewSprite(_fileName);
    }

    public bool TakeScreenShot()
    {
        bool screenshotTaken = false;

        if (!isProcessing)
        {
            CaptureScreenshot();
            screenshotTaken = true;
        }
        else
        {
            Debug.Log("Currently Processing");
        }

        return screenshotTaken;
    }

    private Sprite LoadNewSprite(string FilePath, float PixelsPerUnit = 100.0f, SpriteMeshType spriteType = SpriteMeshType.Tight)
    {
        Texture2D SpriteTexture = LoadTexture(FilePath);
        Sprite NewSprite = Sprite.Create(SpriteTexture, new Rect(0, 0, SpriteTexture.width, SpriteTexture.height), new Vector2(0, 0), PixelsPerUnit, 0, spriteType);

        return NewSprite;
    }

    private Texture2D LoadTexture(string FilePath)
    {
        Texture2D Tex2D;
        byte[] FileData;

        if (File.Exists(FilePath))
        {
            FileData = File.ReadAllBytes(FilePath);
            Tex2D = new Texture2D(2, 2);     
            if (Tex2D.LoadImage(FileData))   
                return Tex2D;                
        }
        return null;
    }
}
