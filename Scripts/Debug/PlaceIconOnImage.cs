using UnityEngine;
using UnityEngine.UI;

public class PlaceIconOnImage : MonoBehaviour
{
    public Image backgroundImage;
    public Image iconImage;

    private void Start()
    {
        // Example: Place the icon at pixel coordinate (100, 100) on the background image
        PlaceIconAtPixelCoordinate(new Vector2(330, 220));
    }

    public void PlaceIconAtPixelCoordinate(Vector2 pixelCoordinate)
    {
        if (backgroundImage == null || iconImage == null)
        {
            Debug.LogError("Background Image and/or Icon Image is not set.");
            return;
        }

        // Set iconImage as a child of backgroundImage
        iconImage.transform.SetParent(backgroundImage.transform, false);

        // Calculate the relative position (0-1 range) from the pixel coordinate
        RectTransform bgRectTransform = backgroundImage.rectTransform;
        Vector2 relativePosition = new Vector2(pixelCoordinate.x / bgRectTransform.rect.width, 1 - (pixelCoordinate.y / bgRectTransform.rect.height));

        // Set iconImage anchor and pivot to the desired position
        iconImage.rectTransform.anchorMin = relativePosition;
        iconImage.rectTransform.anchorMax = relativePosition;
        iconImage.rectTransform.pivot = new Vector2(0.5f, 0.5f);

        // Set iconImage position to zero, since it's already placed correctly by its anchors
        iconImage.rectTransform.anchoredPosition = Vector2.zero;
    }

}
