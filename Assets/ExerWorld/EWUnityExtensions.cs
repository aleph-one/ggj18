using System.Collections;
using System.Collections.Generic;
using UnityEngine;


// extension class
namespace ExerWorld {
	public static class EWUnityExtensions
	{

		private static int sometimesCounter = 0;
		private static int sometimesMod = 100;



		public static void Log (string debugString) {
			if (ExerWorldPlugin.it != null && ExerWorldPlugin.it.enableLog == false)
				return;
			Debug.Log (debugString);
		}

		public static void LogWarning (string debugString) {
			if (ExerWorldPlugin.it != null && ExerWorldPlugin.it.enableLog == false)
				return;
			Debug.LogWarning (debugString);
		}

		public static void LogError (string debugString) {
			if (ExerWorldPlugin.it != null && ExerWorldPlugin.it.enableLog == false)
				return;
			Debug.LogError (debugString);
		}

		public static void LogSometimes (string debugString) {
			if (ExerWorldPlugin.it != null && ExerWorldPlugin.it.enableLogSometimes == false)
				return;
			
			sometimesCounter += 1;

			//Debug.Log ("sometimesCounter is now:" + sometimesCounter);
			if (sometimesCounter % sometimesMod == 0) {
				Debug.Log (debugString);
			}

			if (sometimesCounter > sometimesMod) {
				sometimesCounter = Random.Range(0,sometimesMod/4);
			}
		}


	}
}