using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.SceneManagement;

namespace ExerWorld {
	public class MgrMultiCursor : MonoBehaviour {

	    [System.Serializable]
	    public class CustomCursorEntry
	    {
	        public string id;
	        public CustomCursor cursor;
	        public float lastUpdated;
	    }

	    public static MgrMultiCursor it;

	    public GameObject prefabCursor;

	    public bool activateExerWorldSoOrSo = false;
	    public bool alwaysColorTrailTwoPartyBased = false;


	    public List<CustomCursorEntry> cursors;

	    // refs
	    public GameObject mainCursorArrow;
	    public GameObject mainCursorSteeringWheel;
	    public GameObject mainCursorTrail;

	    // misc 
	    public bool isExerWorldActiv = false;
	    public List<Color> cursorColors = new List<Color>() {
	        new Color(255,0,0),
	        new Color(0,0,255),
	        new Color(0,255,0),
	        new Color(255,255,0),
	        new Color(0,255,255),
	        new Color(255,0,255),
	        new Color(255,255,255),
	        new Color(0,0,0)
	    };

	    public bool showSteeringWheelCursorAndTrail = true;
	    private List<int> GameControllersConnected = new List<int>();


	    // Use this for initialization
	    void Start () {
	        it = this;

	        SceneManager.sceneLoaded += OnLevelSwitch;
	    }
	    
	    // Update is called once per frame
	    void Update () {

	        CheckAndCreateCursors ();

	    }

	    /// <summary>
	    /// Adds controller if missing, updates values if existing
	    /// </summary>
	    /// <param name="id">Controller id</param>
	    /// <param name="connected">Is controller connected</param>
	    /// <returns>Returns CustomController if one was created, else null</returns>
	    private CustomCursor UpdateControllerListing(int id, bool connected)
	    {
	        //Get Joystick Names
	        string[] joyStickNames = Input.GetJoystickNames();

	        CustomCursor newCustomCursor = null;

	        //New controller added
	        if (!GameControllersConnected.Contains(id) && connected)
	        {
	            if(!CustomCursor.allCursorsDict.ContainsKey("Gamepad" + id.ToString()))
	            {
	                GameControllersConnected.Add(id);
	                //This needs mapping to "connected" controller id
	                int controllerID = GameControllersConnected.IndexOf(id);
	                Debug.Log("New controller connected: " + joyStickNames[id]);
	                RepopulateControllers();
	            }
	        }
	        //Controller removed
	        else if (GameControllersConnected.Contains(id) && !connected)
	        {
	            GameControllersConnected.Remove(id);
	            Debug.Log("Controller disconnected: " + joyStickNames[id]);
	            RepopulateControllers();
	        }
	        return newCustomCursor;
	    }

	    private void RepopulateControllers()
	    {
	        foreach(var cursor in CustomCursor.allCursors)
	        {
	            if (cursor.IsController)
	            {
	                DestroyImmediate(cursor.gameObject);
	            }
	        }

	        foreach(int controllerID in GameControllersConnected)
	        {
	            var customCursor = CreateCustomCursor("Gamepad" + controllerID.ToString());
	            //Register as gamepad/controller
	            customCursor.SetAsController(controllerID);
	        }
	    }

	    public void CheckAndCreateCursors() {

	        //Get Joystick Names
	        string[] joyStickNames = Input.GetJoystickNames();
	        //Iterate over every element
	        for (int i = 0; i < joyStickNames.Length; i++)
	        {
	            //Unity does not delete entries but clears strings so check if empty
	            bool isConnected = !string.IsNullOrEmpty(joyStickNames[i]);
	            var customCursor = UpdateControllerListing(i, isConnected);
	        }

	        Dictionary<string, float> mValBtn1 = ControlInput.GetCurrentMultiValues ("playerMulti_btn1");
	        Dictionary<string, float> mValBtn2 = ControlInput.GetCurrentMultiValues ("playerMulti_btn2");
	        Dictionary<string, float> mValXRel = ControlInput.GetCurrentMultiValues ("playerMulti_xRel");
	        Dictionary<string, float> mValYRel = ControlInput.GetCurrentMultiValues ("playerMulti_yRel");

	        // NOTE: do nothing if there is only one
	        if (mValBtn1.Count <= 1  || activateExerWorldSoOrSo) {
	            return;
	        }

	        // create new cursor if needed
	        foreach (string multiId in mValBtn1.Keys) {
	            if ( CustomCursor.allCursorsDict.ContainsKey(multiId) == false) {
	                CreateCustomCursor (multiId);
	            }
	        }
	        foreach (string multiId in mValBtn2.Keys) {
	            if ( CustomCursor.allCursorsDict.ContainsKey(multiId) == false) {
	                CreateCustomCursor (multiId);
	            }
	        }
	        foreach (string multiId in mValXRel.Keys) {
	            if ( CustomCursor.allCursorsDict.ContainsKey(multiId) == false) {
	                CreateCustomCursor (multiId);
	            }
	        }
	        foreach (string multiId in mValYRel.Keys) {
	            if ( CustomCursor.allCursorsDict.ContainsKey(multiId) == false) {
	                CreateCustomCursor (multiId);
	            }
	        }

	        // update values
	        // NOTE: will be done by cursor themself

	        if (cursors.Count > 1 || activateExerWorldSoOrSo) {
	            ActivateExerWorldMode ();
	        } else {
	            DeactivateExerWorldMode ();
	        }

	        // TODO: also deactivate exerworld if the other players leave. check for inactivity
	    }
	    public void ActivateExerWorldMode() {
	        isExerWorldActiv = true;
			if (mainCursorArrow != null) {
				mainCursorArrow.SetActive (false);
			}
			if (mainCursorSteeringWheel != null) {
				mainCursorSteeringWheel.SetActive (showSteeringWheelCursorAndTrail);
			}
			if (mainCursorTrail != null) {
				mainCursorTrail.SetActive (showSteeringWheelCursorAndTrail);
			}
	    }

	    public void DeactivateExerWorldMode() {
	        isExerWorldActiv = false;
			if (mainCursorSteeringWheel != null) {
				mainCursorSteeringWheel.SetActive (false);
			}
			if (mainCursorArrow != null) {
				mainCursorArrow.SetActive (true);
			}
			if (mainCursorTrail != null) {
				mainCursorTrail.SetActive (true);
			}
	    }


	    CustomCursor CreateCustomCursor(string multiId) {
	        GameObject goCC = Instantiate (prefabCursor);
	        CustomCursor cc = goCC.GetComponent<CustomCursor> ();
	        //Register in exerworld
	        cc.RegisterCursor(multiId);
	        cc.transform.SetParent (this.transform);
	        CustomCursorEntry ccEntry = new CustomCursorEntry ();
	        ccEntry.id = multiId;
	        ccEntry.cursor = cc;
	        ccEntry.lastUpdated = Time.realtimeSinceStartup;
	        cursors.Add (ccEntry);

	        // set custom color

	        Color col = cursorColors [(cursors.Count - 1) % cursorColors.Count];
	        cc.SetColor (col);

	        bool isTwoPartyMod = false;


	        // NOTE: only really needed for switzerlan version
	        if (alwaysColorTrailTwoPartyBased || isTwoPartyMod) {

	            int idxOfCursor = CustomCursor.allCursors.IndexOf (cc);
	            if (idxOfCursor % 2 == 1) {
	                cc.SetTrailColor (new Color (0, 0, 255));
	            } else {
	                cc.SetTrailColor (new Color (255, 0, 0));
	            }
	        }
	        return cc;
	    }

	    public void OnLevelSwitch(Scene scene, LoadSceneMode mode) {
	        string activeSceneName = scene.name;
	        Debug.Log ("Active scene is now <color=green>" + activeSceneName + "</color>");
	        if (isExerWorldActiv ) {
	            //TODO: This can have consequences if any scene is added that contains "menu" in it's name
	            if (activeSceneName.ToLower().Contains ("menu")) {
	                showSteeringWheelCursorAndTrail = true;
	            } else {
	                showSteeringWheelCursorAndTrail = false;
	            }

	        }

	        // reset trail colors
	        if (alwaysColorTrailTwoPartyBased) {
	            for (int idxOfCursor = 0; idxOfCursor < CustomCursor.allCursors.Count; idxOfCursor++) {
	                CustomCursor cc = CustomCursor.allCursors [idxOfCursor];
	                if (idxOfCursor % 2 == 1) {
	                    cc.SetTrailColor (new Color (0, 0, 255));
	                } else {
	                    cc.SetTrailColor (new Color (255, 0, 0));
	                }
	            }
	        } else {
	            
	            for (int idxOfCursor = 0; idxOfCursor < CustomCursor.allCursors.Count; idxOfCursor++) {
	                CustomCursor cc = CustomCursor.allCursors [idxOfCursor];
	                cc.SetTrailColor (cc.color);
	            }
	        }
	    }
	}
}