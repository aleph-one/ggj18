using UnityEngine;
using UnityEngine.UI;
using System.Collections;

namespace ExerWorld {
	public class SpriteShiny : MonoBehaviour {

		public Color[] colorss = new Color[5]{
			new Color(160f/255f,160f/255f,160f/255f),
			new Color(255f/255f,144f/255f,105f/255f),
			new Color(255f/255f,0f/255f,0f/255f),
			new Color(105f/255f,0f/255f,0f/255f),
			new Color(0f/255f,0f/255f,0f/255f),
		};

		public float timeMul = 1;
		public float lerp = 0.05f;

		private SpriteRenderer rend;
		// Use this for initialization
		void Start () {
	        rend = GetComponent<SpriteRenderer> ();
		}
		
		// Update is called once per frame
		void Update () {
			float time = Time.timeSinceLevelLoad * timeMul;
			float timeMod = time % 1f;

			Color oldCol = rend.color;
	        Color col1 = oldCol;
	        Color col2 = oldCol;


	        int idx = (int)(Mathf.Floor(time) % colorss.Length);
			int idx2 = (idx + 1) % colorss.Length;

			col1 = colorss [idx];
			col2 = colorss [idx2];

			Color newColorTarget = Color.Lerp(col1, col2, timeMod);

			rend.color = (Color.Lerp(oldCol, newColorTarget, lerp));
		}
	}
}