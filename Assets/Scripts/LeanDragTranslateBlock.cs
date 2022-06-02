// ===============================
// AUTHOR     : Rafael Maio (rafael.maio@ua.pt)
// PURPOSE     : Game Object translation
// SPECIAL NOTES: Adapted from LeanDragTranslate.cs Lean Script -> allow to block/unblock 1 or more translation axis.
// ===============================

using UnityEngine;
using CW.Common;
using Lean.Touch;

public class LeanDragTranslateBlock : MonoBehaviour
{
    /// <summary>The method used to find fingers to use with this component. See LeanFingerFilter documentation for more information.</summary>
    public LeanFingerFilter Use = new LeanFingerFilter(true);

    /// <summary>The camera the translation will be calculated using.
    /// None/null = MainCamera.</summary>
    public Camera Camera { set { _camera = value; } get { return _camera; } }
    [SerializeField] private Camera _camera;

    /// <summary>The movement speed will be multiplied by this.
    /// -1 = Inverted Controls.</summary>
    public float Sensitivity { set { sensitivity = value; } get { return sensitivity; } }
    [SerializeField] private float sensitivity = 1.0f;

    /// <summary>If you want this component to change smoothly over time, then this allows you to control how quick the changes reach their target value.
    /// -1 = Instantly change.
    /// 1 = Slowly change.
    /// 10 = Quickly change.</summary>
    public float Damping { set { damping = value; } get { return damping; } }
    [SerializeField] private float damping = -1.0f;

    /// <summary>This allows you to control how much momentum is retained when the dragging fingers are all released.
    /// NOTE: This requires <b>Dampening</b> to be above 0.</summary>
    public float Inertia { set { inertia = value; } get { return inertia; } }
    [SerializeField] [Range(0.0f, 1.0f)] private float inertia;

    [SerializeField]
    private Vector3 remainingTranslation;

    public ConfigurationMenuScript confMenuScript;

    /// <summary>If you've set Use to ManuallyAddedFingers, then you can call this method to manually add a finger.</summary>
    public void AddFinger(LeanFinger finger)
    {
        Use.AddFinger(finger);
    }

    /// <summary>If you've set Use to ManuallyAddedFingers, then you can call this method to manually remove a finger.</summary>
    public void RemoveFinger(LeanFinger finger)
    {
        Use.RemoveFinger(finger);
    }

    /// <summary>If you've set Use to ManuallyAddedFingers, then you can call this method to manually remove all fingers.</summary>
    public void RemoveAllFingers()
    {
        Use.RemoveAllFingers();
    }

#if UNITY_EDITOR
    protected virtual void Reset()
    {
        Use.UpdateRequiredSelectable(gameObject);
    }
#endif

    protected virtual void Awake()
    {
        Use.UpdateRequiredSelectable(gameObject);
    }

    protected virtual void Update()
    {
        // Store
        var oldPosition = transform.localPosition;

        // Get the fingers we want to use
        var fingers = Use.UpdateAndGetFingers();

        // Calculate the screenDelta value based on these fingers
        var screenDelta = LeanGesture.GetScreenDelta(fingers);

        if (screenDelta != Vector2.zero)
        {
            // Perform the translation
            if (transform is RectTransform)
            {
                TranslateUI(screenDelta);
            }
            else
            {
                Translate(screenDelta);
            }
        }

        // Increment
        remainingTranslation += transform.localPosition - oldPosition;

        // Get t value
        var factor = CwHelper.DampenFactor(Damping, Time.deltaTime);

        // Dampen remainingDelta
        var newRemainingTranslation = Vector3.Lerp(remainingTranslation, Vector3.zero, factor);

        // Shift this transform by the change in delta
        transform.localPosition = oldPosition + remainingTranslation - newRemainingTranslation;

        if (fingers.Count == 0 && Inertia > 0.0f && Damping > 0.0f)
        {
            newRemainingTranslation = Vector3.Lerp(newRemainingTranslation, remainingTranslation, Inertia);
        }

        // Update remainingDelta with the dampened value
        remainingTranslation = newRemainingTranslation;
    }

    private void TranslateUI(Vector2 screenDelta)
    {
        var camera = this._camera;

        if (camera == null)
        {
            var canvas = transform.GetComponentInParent<Canvas>();

            if (canvas != null && canvas.renderMode != RenderMode.ScreenSpaceOverlay)
            {
                camera = canvas.worldCamera;
            }
        }

        // Screen position of the transform
        var screenPoint = RectTransformUtility.WorldToScreenPoint(camera, transform.position);

        // Add the deltaPosition
        screenPoint += screenDelta * Sensitivity;

        // Convert back to world space
        var worldPoint = default(Vector3);

        if (RectTransformUtility.ScreenPointToWorldPointInRectangle(transform.parent as RectTransform, screenPoint, camera, out worldPoint) == true)
        {
            transform.position = worldPoint;
        }
    }

    private void Translate(Vector2 screenDelta)
    {
        // Make sure the camera exists
        var camera = CwHelper.GetCamera(this._camera, gameObject);

        if (camera != null)
        {
            // Screen position of the transform
            var screenPoint = camera.WorldToScreenPoint(transform.position);

            // Add the deltaPosition
            screenPoint += (Vector3)screenDelta * Sensitivity;

            // Convert back to world space
            // Block if needed 
            float x = transform.position.x;
            float y = transform.position.y;
            float z = transform.position.z;
            if (confMenuScript.getTranslationAxis()[0])
            {
                x = camera.ScreenToWorldPoint(screenPoint).x;
            }
            if (confMenuScript.getTranslationAxis()[1])
            {
                y = camera.ScreenToWorldPoint(screenPoint).y;
            }
            if (confMenuScript.getTranslationAxis()[2])
            {
                z = camera.ScreenToWorldPoint(screenPoint).z;
            }
            transform.position = new Vector3(x, y, z);
        }
        else
        {
            Debug.LogError("Failed to find camera. Either tag your camera as MainCamera, or set one in this component.", this);
        }
    }
}