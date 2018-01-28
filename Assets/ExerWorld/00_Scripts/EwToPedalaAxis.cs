using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using ExerWorld;

public class EwToPedalaAxis : ControlInputHelper {

	// Use this for initialization
	void Start () {
		
	}
	
	// Update is called once per frame
	void Update () {
		Vector3 angles = this.transform.localEulerAngles;
		angles.x = GetCurrentControlValueMapped ();
		this.transform.localEulerAngles = angles;
	}
}
