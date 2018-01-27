using UnityEngine;
using System.Collections;
using System.Collections.Generic;

namespace ExerWorld {
	public class ControlInput : MonoBehaviour
	{
		public string controlName;	   // e.g.	= planeUp / shoot
		public string inputValuesFrom; // e.g.  = "leftGloveIndex"; // TODO: not really needed. the game can be agnostic about where the input is coming from... but it could be used as preset

		public ControlType controlType = ControlType.Range; 	// default is range
		public MappingType mapType = MappingType.Linear; 	// default is Linear
		
		public float thresholdOnOff = 0; 	// ... TODO: only important for OnOff type

		public float currentValue = 0;					// should be updated by ExerWorldPlugin
	    public float currentValueCalibrated = 0;		// should be updated by ExerWorldPlugin

		public string debugCurrentValue;

		// NOTE: used for arbitary number of devices, e.g. in multi mice setup or when a lot of phones can connect
		// TODO: really implement
		public bool isMultiInput = false; 
		public ControlInput multiParent = null; // not null if a child controlinput

		[System.Serializable]
		public class MultiInputEntry
		{
			public string multiId;
			public string name;
			public ControlInput controlInput;
		}

		public List<MultiInputEntry> multiInputs; // needed to bookkeep multi things incoming data 


		// accumulator. neat for multi mouse
		public float valueAccumulated = 0;
		public float valueAccuMin = 0;
		public float valueAccuMax = 1000;



		// public ClampType clampType

		public enum ControlType
		{
			Range,  // e.g. for angle of car or whatever
			OnOff 	// e.g. for shooting	// TODO: implement when useful
		}

		public enum MappingType
		{
			Linear,
			Log,							// TODO: implement when useful
			Exponential,					// TODO: implement when useful
			Custom							// TODO: implement when useful
		}


		// static vars
		public static Dictionary<string, ControlInput> allControls = new Dictionary<string, ControlInput> ();
		public static List<ControlInput> allControlsList = new List<ControlInput> ();


		// Use this for initialization
		void Start ()
		{
			// register in Dictionary
			if (allControls.ContainsKey (controlName))
				allControls.Remove (controlName);
			allControls.Add (controlName, this);

			if (allControlsList.Contains (this) == false)
				allControlsList.Add (this);
		}
		
		// Update is called once per frame
		void Update ()
		{
		
		}

		public float GetCurrentValue() {
			return this.currentValue;
		}


		public void SetCurrentValue(float newValue) {
			this.currentValue = newValue;

			// TODO: delta correct somehow ? who is responsible here ? config ?
			float newVal = this.valueAccumulated + this.currentValue;
			this.valueAccumulated = Mathf.Clamp (newVal, this.valueAccuMin, this.valueAccuMax);

			// TODO: trigger events !

		}



		// static methods

		public static float GetCurrentValue(string _controlName) {
			try {
				return allControls[_controlName].currentValue;
			} catch (UnityException e ) {
				EWUnityExtensions.LogWarning("Could not find value for control named '"+_controlName+"', Exception: " + e);
				return 0;
			}
		}

		public static void SetCurrentValue(string _controlName, float value) {
			try {
				allControls[_controlName].SetCurrentValue(value);
			} catch (UnityException e ) {
				EWUnityExtensions.LogWarning("Could not find value for control named '"+_controlName+"', Exception: " + e);
			}
		}

		public static float GetCurrentValueMulti(string _controlNameMulti, string _exerworldId) {
			string wholeName = MakeMultiValueName (_controlNameMulti, _exerworldId);

			// TODO not dry bääh
			if (ControlInputMulti.allMultiControls.ContainsKey(wholeName)) {
				return ControlInputMulti.allMultiControls[wholeName].currentValue;
			} else {
				EWUnityExtensions.LogWarning("Could not find value for control named '" + wholeName + "'");
				return 0;
			}
		}

		public static string MakeMultiValueName(string _controlNameMulti, string _exerworldId) {
			return _exerworldId + "_" + _controlNameMulti;
		}

		// NOTE: only for multi value fields
		public static Dictionary<string, float> GetCurrentMultiValues(string _controlName) {
			Dictionary<string, float> multiValDict = new Dictionary<string, float> ();
			try {
				ControlInput ci =  allControls[_controlName];
				if (ci.isMultiInput) {
					foreach(MultiInputEntry entry in ci.multiInputs) {
						multiValDict[entry.multiId] = entry.controlInput.GetCurrentValue();
					}
				}
			} catch (UnityException e ) {
				EWUnityExtensions.LogWarning("Could not find value for control named '"+_controlName+"', Exception: " + e);
			}

			return multiValDict;
		}
	}
}
