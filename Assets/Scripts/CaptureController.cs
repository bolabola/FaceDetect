using UnityEngine;
using System.Collections;
using OpenCvSharp.MachineLearning;
using OpenCvSharp;

public class CaptureController : MonoBehaviour {
	
	const int CAPTURE_WIDTH = 320;
    const int CAPTURE_HEIGHT = 240;
	
	public Texture2D maskTextureBlack;
	public Texture2D maskTextureGreen;
	public GameObject frontMaskObj;
	public GameObject charactorHeadObj;
   	public Texture2D orginalHeadTexture;
	public FaceDetectController faceDetectController;
	
    private CvCapture capture;
	private bool isCaptureDisplayEnabled = false;
	private Texture2D captureTexture;
	
	// Use this for initialization
	void Start () {
		capture = Cv.CreateCameraCapture(0);
        Cv.SetCaptureProperty(capture, CaptureProperty.FrameWidth, CAPTURE_WIDTH);
        Cv.SetCaptureProperty(capture, CaptureProperty.FrameHeight, CAPTURE_HEIGHT);
		captureTexture = new Texture2D(CAPTURE_WIDTH, CAPTURE_HEIGHT, TextureFormat.RGBA32, false);
		isCaptureDisplayEnabled = true;
	}
	
	// Update is called once per frame
	void Update () {
		
		FaceInCaptureDetection();
		if(isCaptureDisplayEnabled)
		{
			RenderImagePlane();
		}
		
		if(Input.GetKeyUp(KeyCode.F))
		{
			if(isCaptureDisplayEnabled)
			{
				if(faceDetectController.isFaceInCapture)
				{
					this.renderer.enabled = false;
					transform.FindChild("FrontMask").renderer.enabled = false;
					CutOutFaceTexture();
				}
			}
			else
			{
				this.renderer.enabled = true;
				transform.FindChild("FrontMask").renderer.enabled = true;
				isCaptureDisplayEnabled = true;
				charactorHeadObj.renderer.material.mainTexture = orginalHeadTexture;
				charactorHeadObj.renderer.material.mainTextureScale = new Vector2(1, 1);
				charactorHeadObj.transform.parent.animation.Play("idle");
			}
		}
		
		
	}
	
	void RenderImagePlane()
	{
		IplImage frame = Cv.QueryFrame(capture);
        Color[] pixelArray = new Color[captureTexture.width*captureTexture.height];

        for (int y = 0; y < captureTexture.height; y++) {
            for (int x = 0; x < captureTexture.width; x++) {
                CvColor tempColor = frame.Get2D(y, x);
                pixelArray[y * captureTexture.width + x] = new Color(tempColor.R / 255.0f, tempColor.G / 255.0f, tempColor.B / 255.0f, 1.0f);
            }
        }
        captureTexture.SetPixels(pixelArray);
        captureTexture.Apply();
		renderer.material.mainTexture = captureTexture;
	}
	
	void CutOutFaceTexture()
	{
		Color[] capturedPixelArr = captureTexture.GetPixels();
		Color[] maskPixelArr = maskTextureBlack.GetPixels();		
		maskPixelArr = maskTextureBlack.GetPixels();
		// check all the pixel by alpha image then cut out
		for(int y = 0; y < CAPTURE_HEIGHT; y++)
		{
			for(int x = 0; x < CAPTURE_WIDTH; x++)
			{
				int index = y * CAPTURE_WIDTH + x;
				if (maskPixelArr[index] == Color.black)
				{
					capturedPixelArr[index] = Color.clear;
				}
			}
		}
		
		captureTexture.SetPixels (capturedPixelArr,0);
   		captureTexture.Apply();
		isCaptureDisplayEnabled = false;
		
		charactorHeadObj.renderer.material.mainTexture = captureTexture;
		charactorHeadObj.renderer.material.mainTextureScale = new Vector2(-1, -1);
		
		charactorHeadObj.transform.parent.animation.Play("jump");
	}
	
	void FaceInCaptureDetection()
	{
		if(faceDetectController.isFaceInCapture)
		{
			frontMaskObj.renderer.material.mainTexture = maskTextureGreen;
		}
		else
		{
			frontMaskObj.renderer.material.mainTexture = maskTextureBlack;
		}
	}
}
