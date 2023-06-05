using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

public class PointerCoordinates : MonoBehaviour {
    [SerializeField] OVRRaycaster rayCast;

    // Start is called before the first frame update
    void Start() {

    }

    // Update is called once per frame
    void Update() {

        Vector2 position = rayCast.GetScreenPosition();

        Debug.Log("Hit point: " + position);
    }
}
