using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class CriticZonesController : MonoBehaviour
{
    private HashSet<GameObject> potetionallyCriticElements = new HashSet<GameObject>();
    private HashSet<GameObject> criticElements = new HashSet<GameObject>();
    private SelectionController selectionController;

    private void Start()
    {
        selectionController = GetComponent<SelectionController>();
    }

    public bool wasCriticZonesCalculated = false;
    public void addCriticElements(HashSet<GameObject> elements)
    {
        criticElements.UnionWith(elements);
    }

    public void addPotentionallyCriticElements(HashSet<GameObject> elements)
    {
        potetionallyCriticElements.UnionWith(elements);
        HighlightConntroller.setHighlightOfSetToState(potetionallyCriticElements, true, 2);
    }

    

    public void clearPotentionallyCriticElements()
    {
        potetionallyCriticElements.Clear();
    }
    public void removePotentionallyCriticElement(GameObject go)
    {
        potetionallyCriticElements.Remove(go);

    }
    public HashSet<GameObject> getPotentionallyCriticPoints()
    {
        return potetionallyCriticElements;
    }
    public HashSet<GameObject> getCriticPoints()
    {
        return criticElements;
    }



    //ButtonHandlers
    public void checkSelectedElementsAsPotentionallyCritic()
    {
        addPotentionallyCriticElements(new HashSet<GameObject>(selectionController.getSelectedElements()));
    }
    public void calculateSetOfUnCheckedCriticElements()
    {
        HashSet<GameObject> uncheckedElements = new HashSet<GameObject>(criticElements);
        HashSet<GameObject> incorrectlyCheckedElements = new HashSet<GameObject>(potetionallyCriticElements);
        HashSet<GameObject> correctlyCheckedElements = new HashSet<GameObject>(potetionallyCriticElements);
        //var incorrectlyCheckedElements = potetionallyCriticElements;

        uncheckedElements.ExceptWith(potetionallyCriticElements);
        incorrectlyCheckedElements.ExceptWith(criticElements);
        correctlyCheckedElements.ExceptWith(incorrectlyCheckedElements);
        //uncheckedElements.UnionWith(incorrectlyCheckedElements);

        HighlightConntroller.setHighlightOfSetToState(correctlyCheckedElements, true, 0);
        HighlightConntroller.setHighlightOfSetToState(uncheckedElements, true, 1);
        HighlightConntroller.setHighlightOfSetToState(incorrectlyCheckedElements, true, 2);

        wasCriticZonesCalculated = true;

        HandlerOfCriticZonesDeterming.addResultOfTry(
            new ResultOfTryToDetermineCriticZone(correctlyCheckedElements.Count, criticElements.Count)
            );

        HandlerOfCriticZonesDeterming.showLastResultOn(selectionController.menuObj);
    }

    public void resetCriticZoneInfo()
    {
        
        HighlightConntroller.setHighlightOfSetToState(potetionallyCriticElements, false);
        HighlightConntroller.setHighlightOfSetToState(criticElements, false);
        //selectionController.deselectAll();

        clearPotentionallyCriticElements();
        wasCriticZonesCalculated = false;
        //It needs to close panel
        selectionController.deselectAll();
    }
}
