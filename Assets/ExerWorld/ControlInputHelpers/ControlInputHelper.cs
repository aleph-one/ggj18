using UnityEngine;
using System.Collections;

namespace ExerWorld {
	public class ControlInputHelper : MonoBehaviour {

	    public string controlName = "";
	    public float mapToMin = 0;  // incoming '0' mapps to this value
	    public float mapToMax = 90; // incoming '1' mapps to this value
	                                
	    void Start () {
		
		}
		
		// Update is called once per frame
		void Update () {
		
		}

	    public float GetCurrentControlValue()
	    {
	        return ControlInput.GetCurrentValue(controlName);
	    }

	    public float GetCurrentControlValueMapped()
	    {
	        return (GetCurrentControlValue() * (mapToMax - mapToMin)) + mapToMin;
	    }
	}
}
