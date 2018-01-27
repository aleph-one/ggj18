using UnityEngine;
using System.Collections;

namespace ExerWorld {
	public class ControlInputToAngle : ControlInputHelper {

	   
	    public  Transform   targetTransform;
	    private bool        origAnglesAlreadySet = false;
	    private Vector3     origAngles;

	    // config
	    public AngleControlMode mode = AngleControlMode.X;

	    public bool doLocally = true;


	    public enum AngleControlMode
	    {
	        X,
	        Y,
	        Z
	    }

	    // Use this for initialization
	    void Start () {
		
		}
		
		// Update is called once per frame
		void Update () {
	        if (targetTransform == null)
	            targetTransform = this.transform;

	        if (origAnglesAlreadySet == false) {
	            if (doLocally) { 
	                origAngles = targetTransform.localEulerAngles;
	            } else
	            {
	                origAngles = targetTransform.eulerAngles;
	            }
	            origAnglesAlreadySet = true;
	        }
	      
	        Vector3 newAngs = new Vector3();
	        newAngs.x = origAngles.x;
	        newAngs.y = origAngles.y;
	        newAngs.z = origAngles.z;

	        if (doLocally)
	        {
	            targetTransform.localEulerAngles = newAngs;
	        } else
	        {
	            targetTransform.eulerAngles = newAngs;
	        }

	    }

	    // Update is called once per frame
	    void LateUpdate()
	    {

	        float inputValue = GetCurrentControlValue();


	        // TODO: thing about easing. And provide functions in ControlInput ?
	        float mappedValue = GetCurrentControlValueMapped();

	        Vector3 newAngs = new Vector3();
	        if (doLocally)
	        {
	            newAngs.x = targetTransform.localEulerAngles.x;
	            newAngs.y = targetTransform.localEulerAngles.y;
	            newAngs.z = targetTransform.localEulerAngles.z;
	        }
	        else
	        {
	            newAngs.x = targetTransform.eulerAngles.x;
	            newAngs.y = targetTransform.eulerAngles.y;
	            newAngs.z = targetTransform.eulerAngles.z;
	        }
	        

	        if (mode == AngleControlMode.X)
	            newAngs.x += mappedValue;
	        if (mode == AngleControlMode.Y)
	            newAngs.y += mappedValue;
	        if (mode == AngleControlMode.Z)
	            newAngs.z += mappedValue;

	        if (doLocally)
	        {
	            targetTransform.localEulerAngles = newAngs;
	        }
	        else
	        {
	            targetTransform.eulerAngles = newAngs;
	        }

	    }
	}
}
