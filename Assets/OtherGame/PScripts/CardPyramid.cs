using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace dsps{


// An enum defines a variable type with a few prenamed values // a
public enum ePyramidCardState 
{
    drawpile,
    tableau,
    target,
    discard
}

public class CardPyramid : Card // Make sure CardProspector extends card
{
    [Header("Set Dynamically: CardPyramid")]
    // This is how you use the enum ecardState
    public ecardState state = ecardState.drawpile;
    // The hiddenBy list stores which other cards will keep this one face down
    public List<CardPyramid> hiddenBy = new List<CardPyramid>();
    // The layoutID matches this card to the tableau XML if it's a tableau card
    public int layoutID;
    // The SlotDef class stores information pulled in from the LayoutXML <slot>
    public SlotDef slotDef;

    // This allows the card to react to being clicked
    override public void OnMouseUpAsButton() 
    {
        // Call the cardClicked method on the Prospector singleton
        PyramidSolitaire.S.cardClicked(this);
        // Also call the base class (card.cs) version of this method
        base.OnMouseUpAsButton(); // a
    }
}
}
