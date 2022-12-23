using UnityEngine;
using UnityEngine.SceneManagement;
using UnityEngine.UI;
using System.Collections;
using System.Collections.Generic;

namespace dsps {
	public class PyramidSolitaire : MonoBehaviour
	{

		static public PyramidSolitaire S;

		[Header("Set in Inspector")]
		public TextAsset deckXML;
		public TextAsset layoutXML;
		public float xOffset = 3;
		public float yOffset = -2.5f;
		public Vector3 layoutCenter;
		public Vector2 fsPosMid = new Vector2(0.5f, 0.90f);
		public Vector2 fsPosRun = new Vector2(0.5f, 0.75f);
		public Vector2 fsPosMid2 = new Vector2(0.4f, 1.0f);
		public Vector2 fsPosEnd = new Vector2(0.5f, 0.95f);
		public float reloadDelay = 2f;// 2 sec delay between rounds
									  //public Text gameOverText, roundResultText, highScoreText;


		[Header("Set Dynamically")]
		public Deck deck;
		public Layout layout;
		public List<CardPyramid> drawPile;
		public Transform layoutAnchor;
		public CardPyramid target;
		public List<CardPyramid> tableau;
		public List<CardPyramid> discardPile;
		//public CardPyramid cardOnTheTop;
		//public FloatingScore fsRun;
		CardPyramid firstCardThatIsClicked; // just added these 4 lines
		CardPyramid secondCardThatIsClicked;
		CardPyramid unselectCardThatIsClicked;

		void Awake()
		{
			S = this; // Set up a singleton for prospector
					  //SetUpUITexts();
		}

		void Start()
		{
			//Scoreboard.S.score = ScoreManager.SCORE;
			deck = GetComponent<Deck>(); // Get the deck
			deck.InitDeck(deckXML.text); // Pass DeckXML to it
			Deck.Shuffle(ref deck.cards); // This shuffles the deck by reference 

			// this section can be commented out; we're working on a real layout now
			//card c;
			//for (int cNum = 0; cNum<deck.cards.Count; cNum++)
			//{
			//	c = deck.cards[cNum];
			//	c.transform.localPosition = new Vector3((cNum % 13) * 3, cNum / 13 * 4, 0);
			//}

			layout = GetComponent<Layout>(); // Get the Layout component
			layout.ReadLayout(layoutXML.text); // Pass LayoutXML to it

			drawPile = ConvertListcardsToListCardPyramids(deck.cards);
			LayoutGame();
		}

		void Update() // added new function for visual selection of cards
		{
			if (firstCardThatIsClicked == true)
			{
				SpriteRenderer spriteColor;
				spriteColor = firstCardThatIsClicked.GetComponentInChildren<SpriteRenderer>();
				spriteColor.color = Color.red;
				unselectCardThatIsClicked = firstCardThatIsClicked;
			}
			if (secondCardThatIsClicked == true)
			{
				SpriteRenderer spriteColor;
				spriteColor = secondCardThatIsClicked.GetComponentInChildren<SpriteRenderer>();
				spriteColor.color = Color.blue;
				unselectCardThatIsClicked = secondCardThatIsClicked;
			}
			if (firstCardThatIsClicked == false && unselectCardThatIsClicked == true)
			{
				SpriteRenderer spriteColor;
				spriteColor = unselectCardThatIsClicked.GetComponentInChildren<SpriteRenderer>();
				spriteColor.color = Color.white;
			}
			/*if (secondCardThatIsClicked == false && unselectCardThatIsClicked == true)
			{
				SpriteRenderer spriteColor;
				spriteColor = unselectCardThatIsClicked.GetComponentInChildren<SpriteRenderer>();
				spriteColor.color = Color.white;
			}*/
		}

		List<CardPyramid> ConvertListcardsToListCardPyramids(List<Card> lCD)
		{
			List<CardPyramid> lCP = new List<CardPyramid>();
			CardPyramid tCP;
			foreach (Card tCD in lCD)
			{
				tCP = tCD as CardPyramid; // a
				lCP.Add(tCP);
			}

			return (lCP);
		}

		// The Draw function will pull a single card from the drawPile andreturn it
		CardPyramid Draw()
		{

			CardPyramid cd = drawPile[0]; // Pull the 0th CardPyramid
			drawPile.RemoveAt(0); // Then remove it from List<> drawPile
			return (cd); // And return it
		}
		// LayoutGame() positions the initial tableau of cards, a.k.a. the "mine"
		void LayoutGame()
		{
			// Create an empty GameObject to serve as an anchor for the tableau // a
			if (layoutAnchor == null)
			{
				GameObject tGO = new GameObject("_LayoutAnchor");
				// ^ Create an empty GameObject named _LayoutAnchor in the Hierarchy
				layoutAnchor = tGO.transform; // Grab its Transform
				layoutAnchor.transform.position = layoutCenter; // Position it
			}
			CardPyramid cp;
			// Follow the layout
			foreach (SlotDef tSD in layout.slotDefs)
			{
				// ^ Iterate through all the SlotDefs in the layout.slotDefs as tSD
				cp = Draw(); // Pull a card from the top (beginning) of the draw Pile
				cp.faceUp = tSD.faceUp; // Set its faceUp to the value in SlotDef
				cp.transform.parent = layoutAnchor; // Make its parent layoutAnchor
													// This replaces the previous parent: deck.deckAnchor, which
													// appears as _Deck in the Hierarchy when the scene is playing.
				cp.transform.localPosition = new Vector3(
				layout.multiplier.x * tSD.x,
				layout.multiplier.y * tSD.y,
				-tSD.layerID);
				// ^ Set the localPosition of the card based on slotDef
				cp.layoutID = tSD.id;
				cp.slotDef = tSD;
				// CardProspectors in the tableau have the state cardState.tableau
				cp.state = ecardState.tableau;
				// CardProspectors in the tableau have the state cardState.tableau
				cp.SetSortingLayerName(tSD.layerName); // Set the sorting layers 
				tableau.Add(cp); // Add this CardProspector to the List<> tableau
			}

			// Set which cards are hiding others
			foreach (CardPyramid tCP in tableau) {
				foreach (int hid in tCP.slotDef.hiddenBy) {
					cp = FindcardByLayoutID(hid);
					tCP.hiddenBy.Add(cp);
				}
			}
			// Set up the initial target card
			MoveToTarget(Draw());
			// Set up the Draw pile
			UpdateDrawPile();
		}

		// Convert from the layoutID int to the CardProspector with that ID
		CardPyramid FindcardByLayoutID(int layoutID) {
			foreach (CardPyramid tCP in tableau) {
				// Search through all cards in the tableau List<>
				if (tCP.layoutID == layoutID) {
					// If the card has the same ID, return it
					return (tCP);
				}
			}
			// If it's not found, return null
			return (null);
		}
		// This turns cards in the Mine face-up or face-down
		/*void SetTableauFaces() {
			foreach (CardPyramid cd in tableau) {
				bool faceUp = true; // Assume the card will be face-up
				foreach (CardPyramid cover in cd.hiddenBy) {
					// If either of the covering cards are in the tableau
					if (cover.state == ecardState.tableau) {
						faceUp = false; // then this card is face-down
					}
				}
				cd.faceUp = faceUp; // Set the value on the card
			}
		}*/

		// Moves the current target to the discardPile
		void MoveToDiscard(CardPyramid cd)
		{
			// Set the state of the card to discard
			cd.state = ecardState.discard;
			discardPile.Add(cd); // Add it to the discardPile List<>
			cd.transform.parent = layoutAnchor; // Update its transform parent
												// Position this card on the discardPile
			cd.transform.localPosition = new Vector3(
			layout.multiplier.x * layout.discardPile.x,
			layout.multiplier.y * layout.discardPile.y,
			-layout.discardPile.layerID + 0.5f);
			cd.faceUp = true;
			// Place it on top of the pile for depth sorting
			cd.SetSortingLayerName(layout.discardPile.layerName);
			cd.SetSortOrder(-100 + discardPile.Count);
		}

		// Make cd the new target card
		void MoveToTarget(CardPyramid cd)
		{
			// If there is currently a target card, move it to discardPile
			if (target != null) MoveToDiscard(target);
			target = cd; // cd is the new target
			cd.state = ecardState.target;
			cd.transform.parent = layoutAnchor;
			// Move to the target position
			cd.transform.localPosition = new Vector3(
			layout.multiplier.x * layout.discardPile.x,
			layout.multiplier.y * layout.discardPile.y,
			-layout.discardPile.layerID);
			cd.faceUp = true; // Make it face-up
							  // Set the depth sorting
			cd.SetSortingLayerName(layout.discardPile.layerName);
			cd.SetSortOrder(0);
		}
		// Arranges all the cards of the drawPile to show how many are left
		void UpdateDrawPile()
		{
			CardPyramid cd;
			// Go through all the cards of the drawPile
			for (int i = 0; i < drawPile.Count; i++)
			{
				cd = drawPile[i];
				cd.transform.parent = layoutAnchor;
				// Position it correctly with the layout.drawPile.stagger
				//Vector2 dpStagger = layout.drawPile.stagger;
				cd.transform.localPosition = new Vector3(
				layout.multiplier.x * layout.drawPile.x,
				layout.multiplier.y * layout.drawPile.y,
				-layout.drawPile.layerID + 0.1f);
				cd.faceUp = false; // Make them all face-down
				//cd.state = ecardState.drawpile; - commented out line
				// Set depth sorting
				cd.SetSortingLayerName(layout.drawPile.layerName);
				cd.SetSortOrder(-10 * i);
			}
		}

		// cardClicked is called any time a card in the game is clicked
		public void cardClicked(CardPyramid cd)
		{
			// The reaction is determined by the state of the clicked card
			switch (cd.state)
			{
				case ecardState.target:
					if(cd.rank == 13)
					{
						MoveToDiscard(target);
						MoveToTarget(Draw());
						UpdateDrawPile();
					}
					if(firstCardThatIsClicked == true)
					{
						if(cd == firstCardThatIsClicked)
						{
							firstCardThatIsClicked = null;
							return;
						}
						secondCardThatIsClicked = cd;
						if(AreCardsEqualToThirteen(firstCardThatIsClicked, secondCardThatIsClicked) == true)
						{
							MoveToDiscard(firstCardThatIsClicked);
							firstCardThatIsClicked = null;
							MoveToDiscard(secondCardThatIsClicked);
							secondCardThatIsClicked = null;
							MoveToTarget(Draw());
							UpdateDrawPile();
						}
						else
						{
							firstCardThatIsClicked = null;
							secondCardThatIsClicked = null;
						}
					}
					// Clicking the target card does nothing
					break;
				case ecardState.drawpile:
					// Clicking any card in the drawPile will draw the next card
					MoveToDiscard(target); // Moves the target to the discardPile
					MoveToTarget(Draw()); // Moves the next drawn card to the target
					UpdateDrawPile(); // Restacks the drawPile
									  //ScoreManager.EVENT(eScoreEvent.draw);
					//FloatingScoreHandler(eScoreEvent.mine);
					break;

				case ecardState.tableau:
					// Clicking a card in the tableau will check if it's a valid play
					bool validMatch = true;
					/*if (!cd.faceUp)
					{
						// If the card is face-down, it's not valid
						validMatch = false;
					}*/
					foreach (CardPyramid cover in cd.hiddenBy)
                	{
                    	if (cover.state == ecardState.tableau)
                    	{
                        	validMatch = false;
                    	}
                	}

                	if (validMatch == false)  
                	{
                    	return;
                	}

					if(cd.rank == 13)
					{
						MoveToDiscard(cd);
						if (target == null)
						{
							MoveToDiscard(Draw());
						}
					}

					if(firstCardThatIsClicked == true)
					{
						if(cd == firstCardThatIsClicked)
						{
							firstCardThatIsClicked = null;
							return;
						}
						secondCardThatIsClicked = cd;
						if(AreCardsEqualToThirteen(firstCardThatIsClicked,secondCardThatIsClicked) == true)
						{
							MoveToDiscard(firstCardThatIsClicked);
							if(tableau.Contains(firstCardThatIsClicked) == true)
							{
								tableau.Remove(firstCardThatIsClicked);
							}
								firstCardThatIsClicked = null;
								//validMatch = true;
							MoveToDiscard(secondCardThatIsClicked);
							if(tableau.Contains(secondCardThatIsClicked) == true)
							{
								tableau.Remove(secondCardThatIsClicked);
							}
								secondCardThatIsClicked = null;
							if(target == null)
							{
								MoveToTarget(Draw());
							}
					} else{
						firstCardThatIsClicked = null;
						secondCardThatIsClicked = null;
					}
					} else{
						firstCardThatIsClicked = cd;
					}
					break;
					/*if (!AreCardsEqualToThirteen(cd, target))
					{
						// If it's not an adjacent rank, it's not valid
						validMatch = false;
					}
					if (!validMatch) return; // return if not valid
											 // If we got here, then: Yay! It's a valid card.
					tableau.Remove(cd); // Remove it from the tableau List
					MoveToTarget(cd); // Make it the target card
					SetTableauFaces(); // Update tableau card face-ups
									   //ScoreManager.EVENT(eScoreEvent.mine);
									   // Clicking a card in the tableau will check if it's a valid play
					break; */
			}
			// Check to see whether the game is over or not
			//CheckForGameOver();
			//}

			// Test whether the game is over
			//void CheckForGameOver() {
			// If the tableau is empty, the game is over
			if (tableau.Count == 0) 
			{
				// Call GameOver() with a win
				//GameOver(true);
				return;
			}
			// If there are still cards in the draw pile, the game's not over
			if (drawPile.Count > 0)
			{
				return;
			}

			// Check for remaining valid plays
			/*foreach (CardPyramid cd in tableau)
			{
				if (AreCardsEqualToThirteen(cd, target))
				{
					// If there is a valid play, the game's not over
					return;
				}
			}*/

			// Since there are no valid plays, the game is over
			// Call GameOver with a loss

			//GameOver (false);
			//}
			/*
			// Called when the game is over. Simple for now, but expandable
			void GameOver(bool won) {
				int score = ScoreManager.SCORE;
				if (fsRun != null) score += fsRun.score;
			if (won) {
				gameOverText.text = "Round Over";
				roundResultText.text ="You won this round!\nRound Score: "+score;
				ShowResultsUI( true );
			//print ("Game Over. You won! :)");
			ScoreManager.EVENT(eScoreEvent.gameWin);
			FloatingScoreHandler(eScoreEvent.gameWin);
			} else {
				gameOverText.text = "Game Over";
		if (ScoreManager.HIGH_SCORE <= score) {
		string str = "You got the high score!\nHigh score: "+score;
		roundResultText.text = str;
		} else {
		roundResultText.text = "Your final score was: "+score;
		}
		ShowResultsUI( true );
			//print ("Game Over. You Lost. :(");
			ScoreManager.EVENT(eScoreEvent.gameLoss);
			FloatingScoreHandler(eScoreEvent.gameLoss);
			}
			// Reload the scene, resetting the game
			// SceneManager.LoadScene("__Prospector_Scene_0");
			// Reload the scene in reloadDelay seconds
		// This will give the score a moment to travel
		Invoke ("ReloadLevel", reloadDelay); // a
		}
				void ReloadLevel()
				{
					// Reload the scene, resetting the game
					SceneManager.LoadScene("GameScene");
				}*/

		}

			// Return true if the two cards are adjacent in rank (A & K wrap around)
			public bool AreCardsEqualToThirteen(CardPyramid c0, CardPyramid c1) {
				if (Mathf.Abs(c0.rank + c1.rank) == 13) {
					return (true);
				}

				if (c0.rank == 1 && c1.rank == 12) return (true);
				if (c0.rank == 12 && c1.rank == 1) return (true);
				if (c0.rank == 2 && c1.rank == 11) return (true);
				if (c0.rank == 11 && c1.rank == 2) return (true);
				if (c0.rank == 3 && c1.rank == 10) return (true);
				if (c0.rank == 10 && c1.rank == 3) return (true);
				if (c0.rank == 4 && c1.rank == 9) return (true);
				if (c0.rank == 9 && c1.rank == 4) return (true);
				if (c0.rank == 5 && c1.rank == 8) return (true);
				if (c0.rank == 8 && c1.rank == 5) return (true);
				if (c0.rank == 7 && c1.rank == 6) return (true);
				if (c0.rank == 6 && c1.rank == 7) return (true);
				if (c0.rank == 13) return (true);
				// Otherwise, return false
				return (false);
			}

			// Handle FloatingScore movement
			/*void FloatingScoreHandler(eScoreEvent evt) {
				List<Vector2> fsPts;
				switch (evt) {
					// Same things need to happen whether it's a draw, a win, or a loss
					case eScoreEvent.draw: // Drawing a card
					case eScoreEvent.gameWin: // Won the round
					case eScoreEvent.gameLoss: // Lost the round
											   // Add fsRun to the Scoreboard score
						if (fsRun != null) {
							// Create points for the Bézier curve1
							fsPts = new List<Vector2>();
							fsPts.Add(fsPosRun);
							fsPts.Add(fsPosMid2);
							fsPts.Add(fsPosEnd);
							fsRun.reportFinishTo = Scoreboard.S.gameObject;
							fsRun.Init(fsPts, 0, 1);
							// Also adjust the fontSize
							fsRun.fontSizes = new List<float>(new float[] { 28, 36, 4 });
							fsRun = null; // Clear fsRun so it's created again
						}
						break;
					case eScoreEvent.mine: // Remove a mine card

						// Create a FloatingScore for this score
						FloatingScore fs;
						// Move it from the mousePosition to fsPosRun
						Vector2 p0 = Input.mousePosition;
						p0.x /= Screen.width;
						p0.y /= Screen.height;
						fsPts = new List<Vector2>();
						fsPts.Add(p0);
						fsPts.Add(fsPosMid);
						fsPts.Add(fsPosRun);
						fs = Scoreboard.S.CreateFloatingScore(ScoreManager.CHAIN, fsPts);
						fs.fontSizes = new List<float>(new float[] { 4, 50, 28 });
						if (fsRun == null) {
							fsRun = fs;
							fsRun.reportFinishTo = null;
						} else {
							fs.reportFinishTo = fsRun.gameObject;
						}
						break;*/
				}
			}

