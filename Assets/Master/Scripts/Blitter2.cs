﻿using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class Blitter2 : MonoBehaviour {

	public CustomRenderTexture A;
	public RenderTexture B;
	public RenderTexture C;
	public Material mat;
	public Material lineMat;
	public Vector4 pos;
	bool f;
	ON_MouseInteraction mouse;
	Vector2 prevCoord;
	Vector2 prevCoord2;
	public int steps = 1;

	List<Vector2> controls;
	// Use this for initialization
	void Start () {
		controls = new List<Vector2> ();
		for (int i = 0; i < 4; i++) {
			AddToControls (Vector2.zero);
		}
		mouse = FindObjectOfType<ON_MouseInteraction> ();
		A.DiscardContents (true,true);
		B.DiscardContents (true,true);
		UnityEngine.RenderTexture.active = B;
		GL.Clear (true, true, Color.black);
		UnityEngine.RenderTexture.active = C;
		GL.Clear (true, true, Color.black);
	}
	
	// Update is called once per frame
	void Update () {
		if (Input.GetKeyUp (KeyCode.X)) {
			mat.SetFloat ("_Mult", 1);
		}
		if (Input.GetKeyUp (KeyCode.Z)) {
			mat.SetFloat ("_Mult", 0);
		}
		if (mouse.beenHit && mouse.hitCoord!=prevCoord) {
//			A.Update ();
			Vector2 anchor1 = (prevCoord-prevCoord2) + prevCoord;
			Vector2 next = (mouse.hitCoord-prevCoord) + mouse.hitCoord;
			Vector2 lerped = Vector2.Lerp (prevCoord, mouse.hitCoord, .5f);
			Vector2 lerped2 = Vector2.Lerp (anchor1, lerped, .5f);

//			AddToControls (prevCoord2);
			AddToControls (prevCoord2);
			AddToControls (prevCoord);
			AddToControls (mouse.hitCoord);
			AddToControls (next);
//			AddToControls (next);


			for (int i = 0; i < steps-1; i++) {

				Vector2 aa = bezier (prevCoord, lerped2, mouse.hitCoord, (float)i / (float)(steps+1));
				Vector2 bb = bezier (prevCoord, lerped2, mouse.hitCoord, (float)(i+1) / (float)(1+steps));

//				Vector2 aa = GetSplinePos ((float)i / (float)(steps));
//				Vector2 bb = GetSplinePos ((float)(i+1) / (float)(steps));
//				Debug.Log ((float)(i+1) / (float)(steps) + " : bb");
				pos = new Vector4(aa.x,aa.y,bb.x,bb.y);
				lineMat.SetVector ("_Pos", pos);

				Graphics.Blit (C, B, lineMat);
				Graphics.Blit (B, C, mat);
			}

			prevCoord2 = prevCoord;
			prevCoord  = mouse.hitCoord;

		}
	
	}

	void AddToControls(Vector2 control){
		controls.Add (control);
		if (controls.Count > 4)
			controls.RemoveAt (0);
	}

	Vector2 bezier(Vector2 A, Vector2 B, Vector2 C, float t){
		Vector2 a = Vector2.Lerp (A, B, t);
		Vector2 b = Vector2.Lerp (B, C, t);
		return Vector2.Lerp (a, b, t);

	}

	Vector3 GetSplinePos(float t)
	{
		int pos = (int)Mathf.Floor(t * (controls.Count-1));

//		Debug.Log (t);
		//The 4 points we need to form a spline between p1 and p2
		Vector2 p0 = controls[ClampListPos(pos - 1)];
		Vector2 p1 = controls[pos];
		Vector2 p2 = controls[ClampListPos(pos + 1)];
		Vector2 p3 = controls[ClampListPos(pos + 2)];

		//			Debug.Log ((t * (controlPointsList.Length-1))-pos);
		return GetCatmullRomPosition(((t * (controls.Count-1))-pos), p0, p1, p2, p3);

	}

	//Clamp the list positions to allow looping
	int ClampListPos(int pos)
	{
		if (pos < 0)
		{
			pos = controls.Count - 1;
		}

		if (pos > controls.Count)
		{
			pos = 1;
		}
		else if (pos > controls.Count - 1)
		{
			pos = 0;
		}

		return pos;
	}

	//Returns a position between 4 Vector3 with Catmull-Rom spline algorithm
	//http://www.iquilezles.org/www/articles/minispline/minispline.htm
	Vector2 GetCatmullRomPosition(float t, Vector2 p0, Vector2 p1, Vector2 p2, Vector2 p3)
	{
		//The coefficients of the cubic polynomial (except the 0.5f * which I added later for performance)
		Vector2 a = 2f * p1;
		Vector2 b = p2 - p0;
		Vector2 c = 2f * p0 - 5f * p1 + 4f * p2 - p3;
		Vector2 d = -p0 + 3f * p1 - 3f * p2 + p3;

		//The cubic polynomial: a + b * t + c * t^2 + d * t^3
		Vector2 pos = 0.5f * (a + (b * t) + (c * t * t) + (d * t * t * t));

		return pos;
	}
}
