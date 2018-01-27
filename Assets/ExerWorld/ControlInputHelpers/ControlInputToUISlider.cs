using UnityEngine;
using UnityEngine.UI;
using System.Collections;

namespace ExerWorld {
	public class ControlInputToUISlider : ControlInputHelper
	{

	    public Slider targetSlider;

		// Use this for initialization
		void Start () {
		
		}
		
		// Update is called once per frame
		void Update () {

	        float inputValue = GetCurrentControlValue();
	        // TODO: thing about easing. And provide functions in ControlInput ?
	        float mappedValue = GetCurrentControlValueMapped();

	        if (targetSlider)
	        {
	            targetSlider.normalizedValue = mappedValue;
	        }
	    }
	}

}