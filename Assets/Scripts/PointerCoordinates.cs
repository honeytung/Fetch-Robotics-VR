using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
using UnityEngine.EventSystems;
using TMPro;
using Oculus.Interaction;

public class PointerCoordinates : MonoBehaviour {
    [SerializeField] TextMeshProUGUI textElement;
    [SerializeField] OVRRaycaster rayCaster;
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
            Vector3 spherePosition = sphere.transform.localPosition;
            coordinates = spherePosition.ToString();
            textElement.text = coordinates;
        }
        /*
        PointerEventData pointerData = new PointerEventData(EventSystem.current);
        List<RaycastResult> results = new List<RaycastResult>();
        Vector2 position;

        rayCaster.RaycastPointer(pointerData, results);

        foreach (RaycastResult result in results) {
            position = rayCaster.GetScreenPosition(result);
            coordinates = position.ToString();

            UnityEngine.Debug.Log(coordinates);
            textElement.text = coordinates;
        }
        */
    }
}
