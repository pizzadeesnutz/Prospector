using UnityEngine;
using System.Collections;
using System.Collections.Generic;

public enum FSState{
	idle,
	pre,
	active,
	post
}//end of FSState

public class FloatingScore : MonoBehaviour {
	public FSState state = FSState.idle;
	[SerializeField]
	private int _score = 0;
	public string scoreString;

	public int score{
		get{
			return(_score);
		}//end of get
		set{
			_score = value;
			scoreString = Utils.AddCommasToNumber (_score);
			GetComponent<GUIText> ().text = scoreString;
		}//end of set
	}//end of score

	public List<Vector3> bezierPts;
	public List<float> fontSizes;
	public float timeStart = -1f;
	public float timeDuration = 1f;
	public string easingCurve = Easing.InOut;
	public GameObject reportFinishTo = null;

	public void Init(List<Vector3> ePts, float eTimeS = 0, float eTimeD = 1){
		bezierPts = new List<Vector3> (ePts);
		if (ePts.Count == 1) {
			transform.position = ePts [0];
			return;
		}//end of if

		if (eTimeS == 0) eTimeS = Time.time;
		timeStart = eTimeS;
		timeDuration = eTimeD;

		state = FSState.pre;
	}//end of Init

	public void FSCallback(FloatingScore fs){
		score += fs.score;
	}//end of FSCallBack

	void Update () {
		if(state == FSState.idle) return;

		float u = (Time.time - timeStart);
		float uC = Easing.Ease (u, easingCurve);
		if (u < 0) {
			state = FSState.pre;
			transform.position = bezierPts [0];
		}//end of if
		else {
			if (u >= 1) {
				uC = 1;
				state = FSState.post;
				if (reportFinishTo != null) {
					reportFinishTo.SendMessage ("FSCallback", this);
					Destroy (gameObject);
				}//end of second nested if
				else {
					state = FSState.idle;
				}//end of second nested else
			}//end of nested if
			else {
				state = FSState.active;
			}//end of nested else
		}//end of else
		Vector3 pos = Utils.Bezier(uC, bezierPts);
		transform.position = pos;
		if (fontSizes != null && fontSizes.Count > 0) {
			int size = Mathf.RoundToInt (Utils.Bezier (uC, fontSizes));
			GetComponent<GUIText> ().fontSize = size;
		}//end of if
	}//end of Update
}//end of FloatingScore