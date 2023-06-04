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

    bool UIisActive;
    bool CamisActive;

    // Start is called before the first frame update
    void Start() {
        UIisActive = false;
        CamisActive = false;
        canvas.SetActive(UIisActive);
        webStreamImage.enabled = CamisActive;
    }

    // Update is called once per frame
    void Update() {
        if (OVRInput.GetUp(OVRInput.Button.One)) {
            UIisActive = !UIisActive;
            canvas.SetActive(UIisActive);
        }
        if (OVRInput.GetUp(OVRInput.Button.Two))
        {
            CamisActive = !CamisActive;
            webStreamImage.enabled = CamisActive;
        }
    }
}
