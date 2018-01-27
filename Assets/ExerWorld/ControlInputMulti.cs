using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace ExerWorld {
	public class ControlInputMulti : ControlInput {

		public string multiId = null;

		// static vars
		public static Dictionary<string, ControlInputMulti> allMultiControls = new Dictionary<string, ControlInputMulti> ();
		public static List<ControlInputMulti> allMultiControlsList = new List<ControlInputMulti> ();

		// Use this for initialization
		void Start () {
			
		}

		public void RegisterMulti (string _controlName) {
			this.controlName = _controlName;

			// register in Dictionary
			if (allMultiControls.ContainsKey (_controlName))
				allMultiControls.Remove (_controlName);
			allMultiControls.Add (_controlName, this);

			if (allMultiControlsList.Contains (this) == false)
				allMultiControlsList.Add (this);
		}

		
		// Update is called once per frame
		void Update () {
			
		}
	}
}
