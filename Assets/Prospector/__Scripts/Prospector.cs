using UnityEngine;
using UnityEngine.SceneManagement;
using UnityEngine.UI;
using System.Collections;
using System.Collections.Generic;


public class Prospector : MonoBehaviour {

	static public Prospector 	S;

	[Header("Set in Inspector")]
	public TextAsset			deckXML;
	public TextAsset			layoutXML;



	[Header("Set Dynamically")]
	public Deck					deck;
	public Layout               layout;

	void Awake(){
		S = this; // Set up a singleton for prospector
	}

	void Start() {
		deck = GetComponent<Deck> (); // Get the deck
		deck.InitDeck (deckXML.text); // Pass DeckXML to it
		Deck.Shuffle(ref deck.cards); // This shuffles the deck by reference 

		// this section can be commented out; we're working on a real layout now
		//Card c;
		//for (int cNum = 0; cNum<deck.cards.Count; cNum++)
        //{
		//	c = deck.cards[cNum];
		//	c.transform.localPosition = new Vector3((cNum % 13) * 3, cNum / 13 * 4, 0);
        //}

		layout = GetComponent<Layout>(); // Get the Layout component
		layout.ReadLayout(layoutXML.text); // Pass LayoutXML to it
	}

}
