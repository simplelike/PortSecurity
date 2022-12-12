using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using cakeslice;


public static class HighlightConntroller
{
    public static void setHighlightOfSetToState(HashSet<GameObject> set, bool state, int color = 0)
    {
        foreach (var el in set)
        {
            if (el.GetComponent<Outline>())
            {
                el.GetComponentInChildren<Outline>().enabled = state;
                el.GetComponentInChildren<Outline>().color = color;
            }
        }
    }

    public static void setHighlightOfElemToState(GameObject go, bool state, int color = 0)
    {
        go.GetComponentInChildren<Outline>(true).enabled = state;
        go.GetComponentInChildren<Outline>(true).color = color;
    }

}
