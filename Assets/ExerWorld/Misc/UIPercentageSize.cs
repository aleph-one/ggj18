using System.Collections;
using System.Collections.Generic;
using UnityEngine;


namespace ExerWorld {

	[ExecuteInEditMode]
	public class UIPercentageSize : MonoBehaviour {


	    public bool useForWidth = true;
	    public bool useForHeight = false;

		public bool useScale = false;

	    [Range(0,400)]
	    public float percWidth = 100;
	    [Range(0, 400)]
	    public float percHeight = 100;

	    private RectTransform rectTransform;
	    private RectTransform parent;

		// Use this for initialization
		void Start () {
	        rectTransform = this.GetComponent<RectTransform>();
	        parent = this.transform.parent.GetComponent<RectTransform>();
		}
		
		// Update is called once per frame
		void Update () {
			if (useScale == false) {
				if (useForWidth) {
					rectTransform.SetSizeWithCurrentAnchors (RectTransform.Axis.Horizontal, parent.rect.width * (percWidth / 100f));
				}
				if (useForHeight) {
					rectTransform.SetSizeWithCurrentAnchors (RectTransform.Axis.Vertical, parent.rect.height * (percHeight / 100f));
				}
			} else { // use scale
				float pHeight = parent.rect.height;
				float originalWidth = rectTransform.sizeDelta.x;
				float originalRatio = originalWidth / pHeight;

				if (useForHeight) {
					
					float perc = (percHeight / 100f);

					float scale = perc * (1f/originalRatio);
					rectTransform.localScale = new Vector3 (scale, scale, scale);

				}

				if (useForWidth) {
					float perc = (percWidth / 100f);

					float scale = perc * (1f/originalRatio);
					rectTransform.localScale = new Vector3 (scale, scale, scale);

				}

			}
		}
	}
}