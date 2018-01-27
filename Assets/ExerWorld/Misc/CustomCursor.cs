using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using ExerWorld;
using System;

namespace ExerWorld {
	public class CustomCursor : MonoBehaviour {

		public float z;

	    public bool IsController { private set; get; }
	    public int ControllerId { private set; get; }
		public bool useExerWorld = false;
		public string exerWorldId = "player1";
		public float relMul = 0.1f;

		public float timeSinceLastBurgerSpawned = -1000;
		public float timeSinceLastAppleSpawned = -1000;

		//private bool lastBtn1State = false;
		//private bool lastBtn2State = false;

		public static List<CustomCursor> allCursors = new List<CustomCursor>();
		public static Dictionary<string, CustomCursor> allCursorsDict = new Dictionary<string, CustomCursor>();

		public Color color = Color.white;

	    public bool IsBtn2Down = false;
	    public bool IsBtn1Down = false;

		public delegate void ClickFunc(Vector3 pos);
		public event ClickFunc OnClickBtn1;
		public event ClickFunc OnClickBtn2;

	    public bool IsAnyDown {
	        get {
	            return IsBtn1Down || IsBtn2Down;
	        }
	    }

	    public Vector2 ScreenPosition {
	        get {
	            //TODO: Camera.main should not be used like this, potential performance drain
	            //Please chache this somehow
	            return Camera.main.WorldToScreenPoint(transform.position);
	        }
	    }

		// ref
		public SpriteRenderer cursorSprite;
		public TrailRenderer cursorTrail;

		// Use this for initialization
		void Start () {
	        Cursor.visible = false;
		}


	    public void SetAsController(int id)
	    {
	        IsController = true;
	        ControllerId = id;
	    }

	    private void OnDisable()
	    {
	        //TODO: Possibly Unregister here as well
	        //UnregisterCursor(this.exerWorldId);
	    }

	    private void OnDestroy()
	    {
	        UnregisterCursor();
	    }

	    /// <summary>
	    /// Unregister Cursor from Exerworld, called automatically on destroy
	    /// </summary>
	    public void UnregisterCursor()
	    {
	        if (string.IsNullOrEmpty(this.exerWorldId))
	        {
	            Debug.LogError("Must register cursor first before unregistering", this.gameObject);
	            return;
	        }
	        allCursors.Remove(this);
	        allCursorsDict.Remove(this.exerWorldId);
	    }

	    public void RegisterCursor(string _exerWorldId) {
			this.exerWorldId = _exerWorldId;
			allCursors.Add (this);
			allCursorsDict.Add (this.exerWorldId, this);

			this.name = "CustomCursor_" + this.exerWorldId;
		}
		
		// Update is called once per frame
		void Update () {
	        if (IsController)
	        {
	            //TODO: This should not be hardcoded like this
	            //There is no way to check if the input names exist
	            //So this was the easiest to avoid error log spamming

	            //Check for max controllers
	            if (ControllerId < 4)
	            {
	                float xRel = Input.GetAxis("Joystick" + (ControllerId + 1) + " X");
	                float yRel = Input.GetAxis("Joystick" + (ControllerId + 1) + " Y");
	                xRel = Mathf.Pow(xRel,2) * Mathf.Sign(xRel);
	                xRel = Mathf.Sin(xRel);
	                
	                yRel = Mathf.Pow(yRel,2) * Mathf.Sign(yRel);
	                yRel = Mathf.Sin(yRel);


	                //This registers every button of every controller, but would be much nicer
	                //bool curBtn1State = Input.GetButton("Joystick" + (ControllerId + 1) + " Button1");
	                //bool curBtn2State = Input.GetButton("Joystick" + (ControllerId + 1) + " Button2");
	                
	                //HACK: TODO: This is just so that every button of every controller is registered
	                bool curBtn1State = Input.GetKey((KeyCode)Enum.Parse(typeof(KeyCode), "Joystick" + (ControllerId + 1) + "Button0"));
	                bool curBtn2State = Input.GetKey((KeyCode)Enum.Parse(typeof(KeyCode), "Joystick" + (ControllerId + 1) + "Button1"));
	                
	                
	                ProcessRawDeltaInput(xRel, yRel, curBtn1State, curBtn2State);
	            }
	        }
			else if (useExerWorld)
	        {
	            float xRel = ControlInput.GetCurrentValueMulti("playerMulti_xRel", exerWorldId) - 0.5f;
	            float yRel = ControlInput.GetCurrentValueMulti("playerMulti_yRel", exerWorldId) - 0.5f;
	            bool curBtn1State = ControlInput.GetCurrentValueMulti("playerMulti_btn1", exerWorldId) > 0.5f ? true : false;
	            bool curBtn2State = ControlInput.GetCurrentValueMulti("playerMulti_btn2", exerWorldId) > 0.5f ? true : false;
	            
	            ProcessRawDeltaInput(xRel, yRel, curBtn1State, curBtn2State);
	        }
	        else
	        {
	            ProcessUnityInput();
	        }
	    }

	    /// <summary>
	    /// Process normal single mouse input from Unity (used if Exerworld is disabled)
	    /// </summary>
	    private void ProcessUnityInput()
	    {
	        //TODO: Camera.main is a huge performance drain here in the Update loop
	        //Please chache this somehow
	        if (Camera.main != null)
	        {
	            Vector3 v3 = Input.mousePosition;
	            v3.z = this.z;
	            //TODO: Camera.main is a huge performance drain here in the Update loop
	            //Please chache this somehow
	            v3 = Camera.main.ScreenToWorldPoint(v3);

	            this.transform.position = v3;
	        }
	    }

	    private void ProcessRawDeltaInput(float xRel, float yRel, bool btn1State, bool btn2State)
	    {
	        

	        // TODO: also get when it was last updated and kill itself and remove it from mgrmulticursor.it.cursors when too old

	        yRel *= -1;

	        Vector3 v3 = this.transform.position;
	        v3.x += xRel * relMul;
	        v3.y += yRel * relMul;
	        v3.z = this.z;


	        // NOTE: maybe smooth controls ? control lost feel ?  => v3 = Vector3.Lerp(this.transform.position,v3,0.05f);
	        this.transform.position = v3;

	        float curTime = Time.time;

	        Vector3 attrV3 = v3;

	        if (btn1State)
	        {
	            IsBtn1Down = true;
				// we have a btn 1 'click'

				if (OnClickBtn1 != null) {
					OnClickBtn1.Invoke (attrV3);
				}
	        }
	        else
	        {
	            IsBtn1Down = false;
	        }

	        if (btn2State)
	        {
	            IsBtn2Down = true;
	            // we have a btn 2 'click'
				if (OnClickBtn1 != null) {
					OnClickBtn1.Invoke (attrV3);
				}
	            
	        }
	        else
	        {
	            IsBtn2Down = false;
	        }

	        //lastBtn1State = curBtn1State;
	        //lastBtn2State = curBtn2State;

	        // push back when over border
	        Vector3 curPos = this.transform.position;
	        Vector3 camPos = Camera.main.transform.position; //Camera.main.WorldToScreenPoint(Camera.main.transform.position);

	        Vector2 topRightCorner = new Vector2(1, 1);
	        //TODO: Camera.main is a huge performance drain here in the Update loop
	        //Please chache this somehow
	        Vector2 edgeVector = Camera.main.ViewportToWorldPoint(topRightCorner);

	        //TODO: Camera.main is a huge performance drain here in the Update loop
	        //Please chache this somehow
	        float camHeight = Camera.main.orthographicSize * 2.0f;
	        float camWidth = camHeight * Screen.width / Screen.height;

	        float borderFac = 1.00f;
			float offsetX = -camWidth / 100;
	        float offsetY = camHeight / 70;

	        float rightBorder = camPos.x + (camWidth * borderFac / 2f) + offsetX;
	        float leftBorder = camPos.x - (camWidth * borderFac / 2f) + offsetX;
	        float topBorder = camPos.y + (camHeight * borderFac / 2f) + offsetY;
	        float bottomBorder = camPos.y - (camHeight * borderFac / 2f) + offsetY;

	        if (curPos.x > rightBorder)
	        {
	            curPos.x = rightBorder;
	        }
	        if (curPos.x < leftBorder)
	        {
	            curPos.x = leftBorder;
	        }

	        if (curPos.y > topBorder)
	        {
	            curPos.y = topBorder;
	        }
	        if (curPos.y < bottomBorder)
	        {
	            curPos.y = bottomBorder;
	        }

	        this.transform.position = curPos;
	    }

	    public void SetColor(Color _color) {

			Debug.Log ("<color=pink>Set color! to " + _color);
			this.color = _color;
			cursorSprite.color = this.color;

			SpriteShiny ss = cursorSprite.GetComponent<SpriteShiny> ();
			if (ss != null) {
				ss.enabled = false;
			}

			/* NOTE: this approach does not seem to work TODO: would be nice like this
			GradientColorKey[] cks = cursorTrail.colorGradient.colorKeys;
			for (int i=0; i < cks.Length; i++) {
				GradientColorKey ck = cks [i];
				ck.color = this.color;
			}

			cursorTrail.colorGradient.colorKeys = cks;
			*/

			SetTrailColor (this.color);


		}

		public void SetTrailColor(Color _color) {
			cursorTrail.startColor = _color;
			Color endColor = _color;
			endColor.a = 0;
			cursorTrail.endColor = endColor;
		}
	}
}

