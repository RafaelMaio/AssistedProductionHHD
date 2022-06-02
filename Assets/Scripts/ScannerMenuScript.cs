// ===============================
// AUTHOR     : Rafael Maio (rafael.maio@ua.pt)
// PURPOSE     : Handles the scanner menu.
// ===============================

using System.Collections;
using UnityEngine;
using UnityEngine.UI;
using ZXing;
using UnityEngine.XR.ARFoundation;
using UnityEngine.XR.ARSubsystems;

public class ScannerMenuScript : MonoBehaviour
{
    /// <summary>
    /// QRCode reading library.
    /// </summary>
    IBarcodeReader reader;

    /// <summary>
    /// Current AR Camera.
    /// </summary>
    ARCameraManager aRCamera;

    /// <summary>
    /// AR Foundation raycast manager.
    /// </summary>
    ARRaycastManager arRaycastManager;

    /// <summary>
    /// Texture to hold the processed AR Camera frame
    /// </summary>
    private Texture2D arCameraTexture;

    /// <summary>
    /// Flag to scan only once the QRCode/datamatrix/....
    /// </summary>
    private bool onlyonce;

    /// <summary>
    /// Text for debugging purposes.
    /// </summary>
    public Text debugText1;

    /// <summary>
    /// Button to return 1 menu.
    /// </summary>
    public Button backButton;

    /// <summary>
    /// Initial menu GameObject.
    /// </summary>
    public GameObject initialMenu;

    /// <summary>
    /// Visualization menu GameObject.
    /// </summary>
    public GameObject visualizationMenu;

    /// <summary>
    /// Connection to the main script.
    /// </summary>
    public AppMainScript appMainScript;

    /// <summary>
    /// Visualization pieces dropdown script.
    /// </summary>
    public ScrollViewAdapter scrollViewAdapterVis;

    /// <summary>
    /// AR Foundation Tracked Image Manager.
    /// </summary>
    private ARTrackedImageManager aRTrackedImageManager;

    /// <summary>
    /// Connection to the usability test script - Bosch.
    /// </summary>
    public UserTestsBoschScript userTestsBoschScript;

    private int fifthImageUpdate = 0;

    /// <summary>
    /// Unity Start function.
    /// </summary>
    void Start()
    {
        backButton.onClick.AddListener(back);
        aRCamera = FindObjectOfType<ARCameraManager>(); //Load the ARCamera
        arRaycastManager = FindObjectOfType<ARRaycastManager>(); //Load the Raycast Manager
                                                                 //Get the ZXing Barcode/QRCode reader

        aRTrackedImageManager = FindObjectOfType<ARTrackedImageManager>(); 

        reader = new BarcodeReader();
        //Subscribe to read AR camera frames: Make sure this statement runs only once
        aRCamera.frameReceived += OnCameraFrameReceived;
    }

    /// <summary>
    /// Return a menu.
    /// </summary>
    private void back()
    {
        this.gameObject.SetActive(false);
        initialMenu.SetActive(true);
    }

    /// <summary>
    /// Adquires a camera image and starts the coroutine.
    /// </summary>
    /// <param name="eventArgs">Arguments of the AR Camera Frame Event.</param>
    private unsafe void OnCameraFrameReceived(ARCameraFrameEventArgs eventArgs)
    {
        if ((Time.frameCount % 15) == 0)
        { //You can set this number based on the frequency to scan the QRCode
            XRCpuImage image;
            if (aRCamera.TryAcquireLatestCpuImage(out image))
            {
                if (this.gameObject.activeSelf)
                {
                    StartCoroutine(ProcessQRCode(image));
                }
                image.Dispose();
            }
        }
    }

    /// <summary>
    /// Asynchronously Convert to Grayscale and Color : https://docs.unity3d.com/Packages/com.unity.xr.arfoundation@1.0/manual/cpu-camera-image.html
    /// </summary>
    /// <param name="image">Latest Camera image to very the QRCode/data matrix value</param>
    /// <returns></returns>
    IEnumerator ProcessQRCode(XRCpuImage image)
    {
        // Create the async conversion request
        var request = image.ConvertAsync(new XRCpuImage.ConversionParams
        {
            inputRect = new RectInt(0, 0, image.width, image.height),
            outputDimensions = new Vector2Int(image.width / 2, image.height / 2),
            // Color image format
            outputFormat = TextureFormat.RGB24,
            // Flip across the Y axis
            //  transformation = CameraImageTransformation.MirrorY
        });
        while (!request.status.IsDone())
            yield return null;
        // Check status to see if it completed successfully.
        if (request.status != XRCpuImage.AsyncConversionStatus.Ready)
        {
            // Something went wrong
            Debug.LogErrorFormat("Request failed with status {0}", request.status);
            // Dispose even if there is an error.
            request.Dispose();
            yield break;
        }
        // Image data is ready. Let's apply it to a Texture2D.
        var rawData = request.GetData<byte>();
        // Create a texture if necessary
        if (arCameraTexture == null)
        {
            arCameraTexture = new Texture2D(
            request.conversionParams.outputDimensions.x,
            request.conversionParams.outputDimensions.y,
            request.conversionParams.outputFormat,
            false);
        }
        // Copy the image data into the texture
        arCameraTexture.LoadRawTextureData(rawData);
        arCameraTexture.Apply();
        byte[] barcodeBitmap = arCameraTexture.GetRawTextureData();
        LuminanceSource source = new RGBLuminanceSource(barcodeBitmap, arCameraTexture.width, arCameraTexture.height);
        //Send the source to decode the QRCode using ZXing
        //Check if a frame is already being decoded for QRCode. If not, get inside the block.
        var result = reader.Decode(source);
        if (result != null && result.Text != "")
        {
            debugText1.text = result.Text;
            if (result.Text.Equals("COMPUTER VISION")) // For configuration test
            {
                aRTrackedImageManager.enabled = true;
                aRTrackedImageManager.trackedImagesChanged += OnImageChanged;
            }
            else
            {
                //If QRCode found inside the frame
                appMainScript.applicationState = AppMainScript.ApplicationStates.Visualizing;
                this.gameObject.SetActive(false);
                visualizationMenu.SetActive(true);
                string fileName = "8-738-722-409 - Acessório";
                if (result.Text.Equals("R8738720154   1    16000854       0034816903000034816903854KITS              0000000000000")) // Mais frequente 16
                {
                    fileName = "long_frequent_kit_16";
                    userTestsBoschScript.setKitId(1);
                }
                else if (result.Text.Equals("R8738722409   1    28000854       0034824330000034824330854KITS              0000000000000")) // Mais frequente 28
                {
                    fileName = "long_frequent_kit_28";
                    userTestsBoschScript.setKitId(1);
                }
                else if (result.Text.Equals("R8738721819   1    16000854       0034819243000034819243854KITS              0000000000000")) // Menos frequente
                {
                    fileName = "not_frequent_kit";
                    userTestsBoschScript.setKitId(2);
                }
                else if (result.Text.Equals("R8738703126   1    16000854       0034824472000034824472854KITS              0000000000000")) // Frequente pequeno
                {
                    fileName = "short_frequent_kit";
                    userTestsBoschScript.setKitId(3);
                }
                else if (result.Text.Equals("Minikit")) // Mini-kit
                {
                    fileName = "mini_kit";
                    userTestsBoschScript.setKitId(0);

                }
                userTestsBoschScript.increaseKitCount();
                scrollViewAdapterVis.setFileName(fileName);
                scrollViewAdapterVis.FillList();
            }
        }
        else
        {
            //debugText1.text = "Searching for a valid data matrix.";
        }
    }

    /// <summary>
    /// Obtain the QRCode image pose for the (desktop) configuration module.
    /// </summary>
    /// <param name="args">ARCore Tracking image fields.</param>
    public void OnImageChanged(ARTrackedImagesChangedEventArgs args)
    {
        foreach (var trackedImage in args.updated)
        {
            if(fifthImageUpdate < 5)
            {
                fifthImageUpdate += 1;
            }
            else if(fifthImageUpdate == 5)
            {
                //appMainScript.placeByQRCode(trackedImage.transform.position, trackedImage.transform.rotation);
                fifthImageUpdate += 1;
                appMainScript.placeByQRCode(trackedImage.gameObject);
            }
        }
    }
}