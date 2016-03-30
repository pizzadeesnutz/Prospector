using UnityEngine;
using System.Collections;
using System.Collections.Generic;
using UnityEngine.SceneManagement;

public enum ScoreEvent{
	draw,
	mine,
	mineGold,
	gameWin,
	gameLoss
}


public class Prospector : MonoBehaviour {

	static public Prospector 	S;
	static public int SCORE_FROM_PREV_ROUND = 0;
	static public int HIGH_SCORE = 0;

	public float reloadDelay = 1f;

	public Vector3 fsPosMid = new Vector3 (0.5f, 0.9f, 0);
	public Vector3 fsPosRun = new Vector3 (0.5f, 0.75f, 0);
	public Vector3 fsPosMid2 = new Vector3 (0.5f, 0.5f, 0);
	public Vector3 fsPosEnd = new Vector3 (1.0f, 0.65f, 0);

	public Deck					deck;
	public TextAsset			deckXML;
	public Vector3 layoutCenter;
	public float xOffset = 3;
	public float yOffset = -2.5f;
	public Transform layoutAnchor;

	public CardProspector target;
	public List<CardProspector> tableau;
	public List<CardProspector> discardPile;

	public int chain = 0;
	public int scoreRun = 0;
	public int score = 0;
	public FloatingScore fsRun;

	public GUIText GTGameOver;
	public GUIText GTRoundResult;

	public Layout layout;
	public TextAsset layoutXML;

	void Awake(){
		S = this;
		if (PlayerPrefs.HasKey ("ProspectorHighScore")) {
			HIGH_SCORE = PlayerPrefs.GetInt ("ProspectorHighScore");
		}//end of if

		score += SCORE_FROM_PREV_ROUND;
		SCORE_FROM_PREV_ROUND = 0;

		GameObject go = GameObject.Find ("GameOver");

		if (go != null) {
			GTGameOver = go.GetComponent<GUIText>();
		}//end of if

		go = GameObject.Find ("RoundResult");

		if (go != null) {
			GTRoundResult = go.GetComponent<GUIText> ();
		}//end of if

		ShowResultGTs (false);

		go = GameObject.Find ("HighScore");
		string hScore = "High Score: "+Utils.AddCommasToNumber(HIGH_SCORE);
		go.GetComponent<GUIText>().text = hScore;
	}//end of Awake

	void ShowResultGTs(bool show){
		GTGameOver.gameObject.SetActive (show);
		GTRoundResult.gameObject.SetActive (show);
	}//end of ShowResultsGT

	public List<CardProspector> drawPile;

	void Start() {
		Scoreboard.S.score = score;
		deck = GetComponent<Deck> ();
		deck.InitDeck (deckXML.text);
		Deck.Shuffle (ref deck.cards);

		layout = GetComponent<Layout> ();
		layout.ReadLayout (layoutXML.text);

		drawPile = ConvertListCardsToListCardProspectors (deck.cards);
		LayoutGame ();
	}//end of Start

	List<CardProspector> ConvertListCardsToListCardProspectors(List<Card> lCD){
		List<CardProspector> lCP = new List<CardProspector>();
		CardProspector tCP;
		foreach (Card tCD in lCD){
			tCP = tCD as CardProspector;
			lCP.Add(tCP);
		}//end of foreach
		return (lCP);
	}// end of ConvertListCardsToListCardProspectors

	CardProspector Draw(){
		CardProspector cd = drawPile [0];
		drawPile.RemoveAt (0);
		return (cd);
	}//end of Draw

	CardProspector FindCardByLayoutID(int layoutID){
		foreach (CardProspector tCP in tableau) {
			if (tCP.layoutID == layoutID) return (tCP);
		}//end of foreach
		return (null);
	}//end of FindCardByLayoutID

	void LayoutGame(){
		if (layoutAnchor == null) {
			GameObject tGO = new GameObject ("_LayoutAnchor");
			layoutAnchor = tGO.transform;
			layoutAnchor.transform.position = layoutCenter;
		}//end of if

		CardProspector cp;

		foreach (SlotDef tSD in layout.slotDefs) {
			cp = Draw ();
			cp.faceUP = tSD.faceUp;
			cp.transform.parent = layoutAnchor;
			cp.transform.localPosition = new Vector3 (layout.multiplier.x * tSD.x, layout.multiplier.y * tSD.y, -tSD.layerID);
			cp.layoutID = tSD.id;
			cp.slotDef = tSD;
			cp.state = CardState.tableau;
			cp.SetSortingLayerName (tSD.layerName);
			tableau.Add (cp);
		}//end of foreach

		foreach (CardProspector tCP in tableau) {
			foreach (int hid in tCP.slotDef.hiddenBy) {
				cp = FindCardByLayoutID (hid);
				tCP.hiddenBy.Add (cp);
			}//end of nested foreach
		}//end of foreach

		MoveToTarget (Draw ());

		UpdateDrawPile ();
	}//end of LayoutGame

	public void CardClicked(CardProspector cd) {
		switch (cd.state) {
		case CardState.target:
			break;
		case CardState.drawpile:
			MoveToDiscard (target);
			MoveToTarget (Draw ());
			UpdateDrawPile ();
			ScoreManager(ScoreEvent.draw);
			break;
		case CardState.tableau:
			bool validMatch = true;
			if (!cd.faceUP) validMatch = false;
			if (!AdjacentRank(cd,target)) validMatch = false;
			if (!validMatch) return;
			tableau.Remove(cd);
			MoveToTarget(cd);
			SetTableauFaces();
			ScoreManager(ScoreEvent.mine);
			break;
		}//end of switch
		CheckForGameOver ();
	}//end of CardClicked

	void MoveToDiscard (CardProspector cd){
		cd.state = CardState.discard;
		discardPile.Add (cd);
		cd.transform.parent = layoutAnchor;
		cd.transform.localPosition = new Vector3 (layout.multiplier.x * layout.discardPile.x, layout.multiplier.y * layout.discardPile.y, -layout.discardPile.layerID + 0.5f);
		cd.faceUP = true;
		cd.SetSortingLayerName (layout.discardPile.layerName);
		cd.SetSortOrder (-100 + discardPile.Count);
	}//end of MoveToDiscard

	void MoveToTarget(CardProspector cd){
		if (target != null) MoveToDiscard (target);
		target = cd;
		cd.state = CardState.target;
		cd.transform.parent = layoutAnchor;
		cd.transform.localPosition = new Vector3 (layout.multiplier.x * layout.discardPile.x, layout.multiplier.y * layout.discardPile.y, -layout.discardPile.layerID);
		cd.faceUP = true;
		cd.SetSortingLayerName (layout.discardPile.layerName);
		cd.SetSortOrder (0);
	}//end of MoveToTarget

	void UpdateDrawPile() {
		CardProspector cd;
		for (int i=0; i<drawPile.Count; i++){
			cd = drawPile[i];
			cd.transform.parent = layoutAnchor;
			Vector2 dpStagger = layout.drawPile.stagger;
			cd.transform.localPosition = new Vector3(layout.multiplier.x * (layout.drawPile.x + i*dpStagger.x), layout.multiplier.y * (layout.drawPile.y + i*dpStagger.y), -layout.drawPile.layerID+0.1f*i);
			cd.faceUP = false;
			cd.state = CardState.drawpile;
			cd.SetSortingLayerName(layout.drawPile.layerName);
			cd.SetSortOrder(-10*i);
		}//end of for loop
	}//end of UpdateDrawPile

	public bool AdjacentRank(CardProspector c0, CardProspector c1){
		if (!c0.faceUP || !c1.faceUP) return(false);
		if (Mathf.Abs (c0.rank - c1.rank) == 1) return(true);
		if (c0.rank == 1 && c1.rank == 13) return (true);
		if (c0.rank == 13 && c1.rank == 1) return (true);
		return (false);
	}//end of AdjacentRank

	void SetTableauFaces(){
		foreach (CardProspector cd in tableau) {
			bool fup = true;
			foreach (CardProspector cover in cd.hiddenBy) {
				if (cover.state == CardState.tableau) fup = false;
			}//end of nested foreach
			cd.faceUP = fup;
		}//end of foreach
	}//end of SetTableauFaces

	void CheckForGameOver() {
		if (tableau.Count == 0) {
			GameOver (true);
			return;
		}//end of if
		if (drawPile.Count > 0) return;
		foreach (CardProspector cd in tableau) {
			if (AdjacentRank (cd, target)) {
				return;
			}//end of if
		}//end of foreach
		GameOver (false);
	}//end of CheckForGameOver

	void GameOver(bool won) {
		if (won) ScoreManager(ScoreEvent.gameWin);
		else ScoreManager(ScoreEvent.gameLoss);
		Invoke ("ReloadLevel", reloadDelay);
	}//end of GameOver

	void ReloadLevel(){
		SceneManager.LoadScene("__Prospector_Scene_0");
	}//end of ReloadLevel

	void ScoreManager (ScoreEvent sEvt) {
		List<Vector3> fsPts;

		switch (sEvt) {
		case ScoreEvent.draw:
		case ScoreEvent.gameWin:
		case ScoreEvent.gameLoss:
			chain = 0;
			score += scoreRun;
			scoreRun = 0;
			if (fsRun != null){
				fsPts = new List<Vector3>();
				fsPts.Add (fsPosRun);
				fsPts.Add (fsPosMid2);
				fsPts.Add (fsPosEnd);
				fsRun.reportFinishTo = Scoreboard.S.gameObject;
				fsRun.Init (fsPts, 0, 1);
				fsRun.fontSizes = new List<float>(new float[] {14,18,2});
				fsRun = null;
			}//end of if
			break;
		case ScoreEvent.mine:
			chain++;
			scoreRun += chain;
			FloatingScore fs;
			Vector3 p0 = Input.mousePosition;
			p0.x /= Screen.width;
			p0.y /= Screen.height;
			fsPts = new List<Vector3>();
			fsPts.Add (p0);
			fsPts.Add (fsPosMid);
			fsPts.Add (fsPosRun);
			fs = Scoreboard.S.CreateFloatingScore (chain,fsPts);
			fs.fontSizes = new List<float>(new float[] {4, 50, 28});
			if (fsRun == null){
				fsRun = fs;
				fsRun.reportFinishTo = null;
			}//end of if
			else fs.reportFinishTo = fsRun.gameObject;
			break;
		}//end of switch

		switch (sEvt) {
		case ScoreEvent.gameWin:
			GTGameOver.text = "Round Over";
			Prospector.SCORE_FROM_PREV_ROUND = score;
			print ("You won this round! Round score: " + score);
			GTRoundResult.text = "You won this round! \nRound score: "+score;
			ShowResultGTs (true);
			break;
		case ScoreEvent.gameLoss:
			GTGameOver.text = "Game Over";
			if (Prospector.HIGH_SCORE <= score) {
				print ("You got the high score! High score: " + score);
				string sRR = "You got the high score!\nHigh score: "+score;
				GTRoundResult.text = sRR;
				Prospector.HIGH_SCORE = score;
				PlayerPrefs.SetInt ("ProspectorHighScore", score);
			}//end of if
			else {
				print ("Your final score for the game was : " + score);
				GTRoundResult.text = "Your final score was: "+score;
			}//end of else

			ShowResultGTs (true);
			break;
		default:
			print ("score: " + score + "   scoreRun:" + scoreRun + "    chain:" + chain);
			break;
		}//end of switch
	}//end of ScoreManager
}//end of Prospector