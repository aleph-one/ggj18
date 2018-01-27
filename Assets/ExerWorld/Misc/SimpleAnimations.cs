using UnityEngine;
using UnityEngine.UI;
using System.Collections;

namespace ExerWorld {
	public class SimpleAnimations : MonoBehaviour {

	    public enum SimpleAnimType
	    {
	        Whobble,
	        Float,
	        AlternateEnableDisable,
	        Rotation,
			AlternateList
	    }

		public SimpleAnimType animType = SimpleAnimType.Whobble;


	    // general properties
	    public float animSpeed = 1;
	    public float animTimeOffset = 0;
	    public Vector3 animStrength = new Vector3(0.05f, 0.05f, 0.05f);
	    public float animModTime = 0.3f;
		public bool continuesMode = false;
		public bool overrideMode = false;

	    // general memory
	    private Vector3 origLocalScale;
	    private Vector3 origLocalPos;
	    private Vector3 origLocalRot;
	    private float   animTime = 0;
	    private CanvasRenderer canvasRenderer;


	    // Anim: Whobble


	    // Anim: Float


	    // Anim: AlternateEnableDisable
	    public GameObject[] alternateFirstOn;
	    public GameObject[] alternateFirstOff;

		[System.Serializable]
		public class AlternateList
		{
			public GameObject[] list;
		}
		public AlternateList[] alternateLists;
		private int alternateListIdx = -1;
		private bool oldAlternateListToggleValue = true;

	    public float alphaLerp = 0.05f;
	    // public bool doAlternateContinously = true;

		[Header("Misc Configs")]
		public bool useRoundedValues = false;
		public float roundValuesWith = 1;

	    // Use this for initialization
	    void Start () {
	        origLocalScale = this.transform.localScale;
	        origLocalPos = this.transform.localPosition;
	        origLocalRot = this.transform.localEulerAngles;

	        canvasRenderer = this.GetComponent<CanvasRenderer>();
	    }
		
		// Update is called once per frame
		void Update () {
	        animTime += (Time.deltaTime * animSpeed);

			Vector3 processedValues = ProcessValues ();

			if (animType == SimpleAnimType.Whobble) {
				if (overrideMode) {
					transform.localScale = processedValues;
				} else {
					transform.localScale = origLocalScale + processedValues;
				}

			} else if (animType == SimpleAnimType.Float) {
				if (overrideMode) {
					transform.localPosition = processedValues;
				} else {
					transform.localPosition = origLocalPos + processedValues;
				}

			} else if (animType == SimpleAnimType.AlternateEnableDisable) {
				bool isInOnState = ((int)((animTime / animModTime) % 2)) == 0 ? true : false;

				// TODO: option for smooth transparency fade in & out
				foreach (GameObject go in alternateFirstOn) {
					float newAlpha = 1;
					if (isInOnState == false)
						newAlpha = 0;

					// go.SetUIAlpha(newAlpha, alphaLerp); // TODO: reuse
					if (newAlpha == 1) {
						go.SetActive (true);
					} else {
						go.SetActive (false);
					}
				}
				foreach (GameObject go in alternateFirstOff) {
					float newAlpha = 0;
					if (isInOnState == false)
						newAlpha = 1;

					// go.SetUIAlpha(newAlpha, alphaLerp); // TODO: reuse
					if (newAlpha == 1) { 
						go.SetActive (true);
					} else {
						go.SetActive (false);
					}
				}

			} else if (animType == SimpleAnimType.AlternateList) {
				bool alternateListToggleValue = ((int)((animTime / animModTime) % 2)) == 0 ? true : false;

				if (alternateListToggleValue != oldAlternateListToggleValue) {
					oldAlternateListToggleValue = alternateListToggleValue;

					GameObject[] alternateList;

					if (alternateListIdx >= 0) {
						alternateList = alternateLists [alternateListIdx].list;
						foreach (GameObject go in alternateList) {
							float newAlpha = 0;

							// go.SetUIAlpha(newAlpha, alphaLerp); // TODO: reuse
							if (newAlpha == 1) { 
								go.SetActive (true);
							} else {
								go.SetActive (false);
							}
						}
					}

					// go on and enable
					alternateListIdx = alternateListIdx + 1;
					if (alternateListIdx >= alternateLists.Length) {
						alternateListIdx = 0;
					}

					alternateList = alternateLists [alternateListIdx].list;
					foreach (GameObject go in alternateList) {
						float newAlpha = 1;

						// go.SetUIAlpha(newAlpha, alphaLerp); // TODO: reuse
						if (newAlpha == 1) { 
							go.SetActive (true);
						} else {
							go.SetActive (false);
						}
					}

				}



			}
			else if (animType == SimpleAnimType.Rotation)
	        {
				if (overrideMode) {
					transform.localEulerAngles = processedValues;
				} else {
					transform.localEulerAngles = origLocalRot + processedValues;
				}

	        }
	        else
	        {
	            // feel the void
	        }
	    }

		public Vector3 ProcessValues() {
			Vector3 newVec;
			if (continuesMode) {
				newVec = animStrength*animTime* animModTime;
			} else { 
				float sinVal = Mathf.Sin (animTime * animModTime);
				newVec = (Vector3.Scale (new Vector3 (sinVal, sinVal, sinVal), animStrength));
			}

			if (useRoundedValues) {
				newVec.x = RoundValue (newVec.x, roundValuesWith);
				newVec.y = RoundValue (newVec.y, roundValuesWith);
				newVec.z = RoundValue (newVec.z, roundValuesWith);
			}

			return newVec;
		}

		// TODO: move to math extension ?
		public static float RoundValue(float val) {
			return RoundValue (val, 1f); // standard rounding woth 1
		}
		public static float RoundValue(float val, float roundWith) {
			float rest = Utils.Mod (val, roundWith);
			if (rest == 0) {
				return val;
			} else if (rest < (roundWith / 2f)) {
				return val - rest;
			} else {
				return val + (roundWith - rest);
			}
		}
	}
}