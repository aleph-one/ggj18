using UnityEditor;
using UnityEngine;
using System.Collections;
using System.Collections.Generic;

namespace ExerWorld {
	public class ExerWorldEditorToolbox : EditorWindow
	{
		string myString = "Hello World";
		bool groupEnabled;
		bool myBool = true;
		float myFloat = 1.23f;

		// Add menu item named "My Window" to the Window menu
		[MenuItem("Window/Exer.World/Toolbox")]
		public static void ShowWindow()
		{
			//Show existing window instance. If one doesn't exist, make one.
			EditorWindow.GetWindow(typeof(ExerWorldEditorToolbox));
		}

		void OnGUI()
		{
			GUILayout.Label ("Base Settings", EditorStyles.boldLabel);
			myString = EditorGUILayout.TextField ("Text Field", myString);

			groupEnabled = EditorGUILayout.BeginToggleGroup ("Optional Settings", groupEnabled);
			myBool = EditorGUILayout.Toggle ("Toggle", myBool);
			myFloat = EditorGUILayout.Slider ("Slider", myFloat, -3, 3);
			EditorGUILayout.EndToggleGroup ();
		}

		// TODO: auto generate app_spec.json
	}
}