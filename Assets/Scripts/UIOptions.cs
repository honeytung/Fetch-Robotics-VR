/*
 *  
 *  TECHIN 517 Robotics Lab Perception Team
 *  Author: Harry Tung
 *  Date: May 24, 2023
 *  Description: UI Interface Control
 *
 */

using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

public class UIOptions : MonoBehaviour {
    [SerializeField] GameObject canvas;
    [SerializeField] RawImage webStreamImage;

    bool isActive;

    // Start is called before the first frame update
    void Start() {
        isActive = false;
        canvas.SetActive(isActive);
        webStreamImage.enabled = isActive;
    }

    // Update is called once per frame
    void Update() {
        if (OVRInput.GetUp(OVRInput.Button.One)) {
            isActive = !isActive;
            canvas.SetActive(isActive);
            webStreamImage.enabled = isActive;
        }
    }
}
