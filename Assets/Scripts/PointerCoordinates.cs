using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
using UnityEngine.EventSystems;
using TMPro;
using Oculus.Interaction;

public class PointerCoordinates : MonoBehaviour {
    [SerializeField] TextMeshProUGUI textElement;
    [SerializeField] GameObject canvas;
    [SerializeField] RawImage rawImage;
    [SerializeField] GameObject sphere;

    // Start is called before the first frame update
    void Start() {
        string coordinates = "Coordinates";
        textElement.text = coordinates;
    }

    // Update is called once per frame
    void Update() {
        string coordinates = "Coordinates";

        if (sphere.activeSelf) {
            Vector3 spherePosition = sphere.transform.position;
            Vector2 canvasPosition = canvas.transform.InverseTransformPoint(spherePosition);
            Vector2 rawImagePosInCanvas = rawImage.rectTransform.anchoredPosition;
            Vector2 rawImageSize = rawImage.rectTransform.rect.size;
            Vector2 rawImageNormalizedPos = new Vector2(
                ((canvasPosition.x - rawImagePosInCanvas.x) / rawImageSize.x + 0.5f) * rawImageSize.x, 
                ((canvasPosition.y - rawImagePosInCanvas.y) / rawImageSize.y + 0.5f) * rawImageSize.y);

            coordinates = rawImageNormalizedPos.ToString();
        }

        textElement.text = coordinates;
    }
}
