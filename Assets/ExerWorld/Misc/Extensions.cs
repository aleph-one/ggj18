using UnityEngine;
using UnityEngine.UI;
using System.Collections;
using System.Collections.Generic;

namespace ExerWorld {
	public class Utils {

		// Calculates x in modulo m // TODO: move to UTIL class
		public static float Mod(float x, float m)
		{
			if (m < 0) m = -m;
			float r = x % m;
			return r < 0 ? r + m : r;
		}
		
		// Calculates the distance from a to b in modulo m
		public static float ModularDistance(float a, float b, float m)
		{
			return Mathf.Min(Mod(a - b, m), Mod(b - a, m));
		}

	}




    // extension class
    static class ExerWorldExtensions
    {
	    // NOTE: this methos will extend IList classes. usage: alist.Shuffle()
	    public static void Shuffle<T>(this IList<T> list) {
		    int n = list.Count;
		    while (n > 1) {
			    int k = (Random.Range(0, n) % n);
			    n--;
			    T value = list[k];
			    list[k] = list[n];
			    list[n] = value;
		    }
	    }

	    // NOTE: this extends the GameObject class, and adds this instance method 
	    // NOTE: if you use this without lerping, just use 0 as lerp_alpa
	    public static void ChangeTransparency(this GameObject gameObject, float to) {
		    ChangeTransparency (gameObject, to, 0);
	    }
	    public static void ChangeTransparency(this GameObject gameObject, float to, float lerp_alpha) {
		    ChangeTransparency(gameObject,to,lerp_alpha,false);
	    }
	    public static void ChangeTransparency(this GameObject gameObject, float to, float lerp_alpha_or_step, bool doSteps) {
		    Renderer[] tempMaterials = gameObject.GetComponentsInChildren<Renderer>(true);
		    foreach(Renderer tempRenderer in tempMaterials)
		    {
			    // tempRenderer.enabled = false;
			    Material material = tempRenderer.material;
			    Color newColor = material.color;

			    float newAlpha;

			    newAlpha = newColor.a*lerp_alpha_or_step + (to*(1f-lerp_alpha_or_step));

			    if (doSteps) {
				    if (Mathf.Abs(newAlpha - to) > lerp_alpha_or_step) {
					    if (newAlpha < to)
						    newAlpha = newColor.a + lerp_alpha_or_step;
					    else
						    newAlpha = newColor.a - lerp_alpha_or_step;
				    } else {
					    // nothing to do
					    // return;
				    }
			    } else { // lerp
				    float lerpedValue = newColor.a*lerp_alpha_or_step + (to*(1f-lerp_alpha_or_step));
				    newAlpha = lerpedValue;
				    // Debug.Log ("lerping happens lerpedValue, old alpha, to " + lerpedValue + " " + newAlpha + " " + to);
			    }

			    // TODO / NOTE: this is hacky. model faces should be changed to allow smooth transparent rendering
			    float quasiMax = 0.6f;
			    newAlpha = Mathf.Clamp(newAlpha,0f,quasiMax);

			    // TODO: this is a bit hacky: switching between blend modes, 
			    // see: http://forum.unity3d.com/threads/access-rendering-mode-var-on-standard-shader-via-scripting.287002/
			    // switch rendering mode
			    if (newAlpha >= quasiMax) {
				    // go to opaque render mode 
				    material.SetFloat("_Mode", 0); 
				    material.SetInt("_SrcBlend", 	1);
				    material.SetInt("_DstBlend", 	0);
				    material.SetInt("_ZWrite", 		1);
				    /*
				    material.DisableKeyword("_ALPHATEST_OFF");
				    material.EnableKeyword("_ALPHABLEND_OFF");
				    material.DisableKeyword("_ALPHAPREMULTIPLY_OFF");
				    //*/
				    //*
				    material.EnableKeyword("_ALPHATEST_ON");
				    material.DisableKeyword("_ALPHABLEND_ON");
				    material.EnableKeyword("_ALPHAPREMULTIPLY_ON");
				    //*/
				    material.renderQueue = 3000;
			    } else { // fade rendermode
				    // go to fade mode
				    //if (material.GetFloat ("_Mode") != (int) BlendMode.Transparent) {
					    material.SetFloat("_Mode", 2);
					    material.SetInt("_SrcBlend", (int)UnityEngine.Rendering.BlendMode.SrcAlpha);
					    material.SetInt("_DstBlend", (int)UnityEngine.Rendering.BlendMode.OneMinusSrcAlpha);
					    material.SetInt("_ZWrite", 0);
					    material.DisableKeyword("_ALPHATEST_ON");
					    material.EnableKeyword("_ALPHABLEND_ON");
					    material.DisableKeyword("_ALPHAPREMULTIPLY_ON");
					    material.renderQueue = 3000;
				    //} else {
				    //	Debug.Log ("already in transparent mode");
				    //}
			    }

			    // now really set alpha
			    newColor.a = newAlpha;
			    material.color = newColor;


		    }
        }

        // NOTE: maybe fuse with method above ?
        public static void SetUIAlpha(this GameObject go_, float newAlpha, float alphaLerp=1)
        {
            foreach (CanvasRenderer r in go_.GetComponentsInChildren<CanvasRenderer>())
            {
                r.SetAlpha(r.GetAlpha() * (1f - alphaLerp) + (newAlpha * alphaLerp));
            }
        }
	
	    public static GameObject FindChildObjectByName(this GameObject parent, string name)
	    {
		    Transform[] trs= parent.GetComponentsInChildren<Transform>(true);
		    foreach(Transform t in trs){
			    if(t.name == name){
				    return t.gameObject;
			    }
		    }
		    return null;
	    }

	    public static GameObject FindChildObjectByTag(this GameObject parent, string tag)
	    {
		    Transform[] trs= parent.GetComponentsInChildren<Transform>(true);
		    foreach(Transform t in trs){
			    if(t.gameObject.tag == tag){
				    return t.gameObject;
			    }
		    }
		    return null;
	    }

	    public static List<GameObject> FindChildObjectsByTag(this GameObject parent, string tag)
	    {
		    List<GameObject> gos = new List<GameObject>();
		    Transform[] trs= parent.GetComponentsInChildren<Transform>(true);
		    foreach(Transform t in trs){
			    if(t.gameObject.tag == tag){
				    gos.Add(t.gameObject);
			    }
		    }

		    if (gos.Count > 0) {
			    return gos;
		    }

		    return null;
	    }

	    public static List<GameObject> FindChildObjectsByName(this GameObject parent, string name)
	    {
		    List<GameObject> gos = new List<GameObject>();
		    Transform[] trs= parent.GetComponentsInChildren<Transform>(true);
		    foreach(Transform t in trs){
			    if(t.name == name){
				    gos.Add(t.gameObject);
			    }
		    }

		    if (gos.Count > 0) {
			    return gos;
		    }

		    return null;
	    }
    }
}