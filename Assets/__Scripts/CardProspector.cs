using UnityEngine;
using System.Collections;
using System.Collections.Generic;

public enum CardState{
	drawpile,
	tableau,
	target,
	discard
}//end of enum CardState

public class CardProspector : Card {
	public CardState state = CardState.drawpile;
	public List<CardProspector> hiddenBy = new List<CardProspector>();
	public int layoutID;
	public SlotDef slotDef;

	void Start () {
	
	}//end of Start

	void Update () {
	
	}//end of Update

	override public void OnMouseUpAsButton(){
		Prospector.S.CardClicked (this);
		base.OnMouseUpAsButton ();
	}// end of OnMouseUpAsButton()
}//end of CardProspector