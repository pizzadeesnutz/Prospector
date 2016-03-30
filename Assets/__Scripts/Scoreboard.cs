using UnityEngine;
using System.Collections;
using System.Collections.Generic;

public class Scoreboard : MonoBehaviour {
	public static Scoreboard S;

	public GameObject prefabFloatingScore;

	public bool ________________________;
	[SerializeField]
	private int _score = 0;
	public string _scoreString;

	public int score {
		get {
			return (_score);
		}//end of get
		set {
			_score = value;
			scoreString = Utils.AddCommasToNumber (_score);
		}//end of set
	}//end of score

	public string scoreString {
		get {
			return(_scoreString);
		}//end of get
		set {
			_scoreString = value;
			GetComponent<GUIText> ().text = _scoreString;
		}//end of set
	}//end of scoreString

	void Awake(){
		S = this;
	}//end of Awake

	public void FSCallback(FloatingScore fs) {
		score += fs.score;
	}//end of FSCallback

	public FloatingScore CreateFloatingScore(int amt, List<Vector3> pts){
		GameObject go = Instantiate (prefabFloatingScore) as GameObject;
		FloatingScore fs = go.GetComponent<FloatingScore> ();
		fs.score = amt;
		fs.reportFinishTo = this.gameObject;
		fs.Init (pts);
		return(fs);
	}//end of CreateFloatingScore
}//end of Scoreboard