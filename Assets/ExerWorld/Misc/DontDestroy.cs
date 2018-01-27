using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace ExerWorld {
	public class DontDestroy : MonoBehaviour {

		public bool killSelfWhenThereIsOtherOwn = false;
		// Use this for initialization
		void Start () {
			if (killSelfWhenThereIsOtherOwn)
			{
				// TODO:
				/*
				Destroy(this.gameObject); // kill yourself!
				Debug.Log("kill myself");
				return; 
				*/

			} 
			DontDestroyOnLoad(transform.gameObject);	
		}
		
		// Update is called once per frame
		void Update () {
			
		}
	}
}
