using UnityEngine;
using System.Collections;
using System.Collections.Generic;


public class Prospector : MonoBehaviour {

	static public Prospector 	S;
	public Deck					deck;
	public TextAsset			deckXML;
	public TextAsset layoutXML;
	public Layout layout;
	public Vector3 layoutCenter;
	public float xOffset = 3;
	public float yOffset = -2.5f;
	public Transform layoutAnchor;

	public CardProspector target;
	public List<CardProspector> tableau;
	public List<CardProspector> discardPile;
	public List<CardProspector> drawPile;

	void Awake(){
		S = this;
	}//end of Awake

	void Start() {  
		deck = GetComponent<Deck> ();
		deck.InitDeck (deckXML.text);
		Deck.Shuffle (ref deck.cards);
		layout = GetComponent<Layout> ();
		layout.ReadLayout (layoutXML.text);
		drawPile = ConvertListCardsToListCardProspectors (deck.cards);
		LayoutGame ();
	}//end of Start

	CardProspector Draw(){
		CardProspector cd = drawPile [0];
		drawPile.RemoveAt (0);
		return(cd);
	}//end of Draw

	void LayoutGame(){
		if (layoutAnchor == null) {
			GameObject tGO = new GameObject ("_LayoutAnchor");
			layoutAnchor = tGO.transform;
			layoutAnchor.transform.position = layoutCenter;
		}//end of if

		CardProspector cp;
		foreach(SlotDef tSD in layout.slotDefs){
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
	}//end of LayoutGame

	List<CardProspector> ConvertListCardsToListCardProspectors (List<Card> lCD){
		List<CardProspector> lCP = new List<CardProspector>();
		CardProspector tCP;
		foreach (Card tCD in lCD){
			tCP = tCD as CardProspector;
			lCP.Add(tCP);
		}//end of for each
		return(lCP);
	}//end of ConvertListCardsToListCardProspectors
}