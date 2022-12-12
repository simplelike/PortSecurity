using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using cakeslice;

public class SelectionController : MonoBehaviour
{
    public Dictionary<int, GameObject> selectedTable = new Dictionary<int, GameObject>();
    public Dictionary<int, GameObject> buffer;

    public GameObject menuObj;
    private MenuController menu;
    private CriticZonesController cr_zone_controller;

    
    private void Start()
    {
        menu = menuObj.GetComponent<MenuController>();

        cr_zone_controller = GameObject.FindGameObjectWithTag("EventSystem").GetComponentInChildren<CriticZonesController>();
    }
    void Update()
    {
       
    }


    public void addSelected(GameObject go)
    {
        buffer = new Dictionary<int, GameObject>(selectedTable);
        if (!cr_zone_controller.wasCriticZonesCalculated)
        {
            int id = go.GetInstanceID();

            if (!(selectedTable.ContainsKey(id)))
            {
                selectedTable.Add(id, go);

                if (go.GetComponentInChildren<Outline>(true))
                {
                    HighlightConntroller.setHighlightOfElemToState(go, true);

                    if (go.GetComponentInChildren<BuildingInfo>())
                    {
                        fillMenu(go.GetComponent<BuildingInfo>());
                        menu.setInfoPanelVisibilityToState(selectedTable.Count == 1);
                    }
                    menu.setCriticZoneControllerPanelVisibility(true);
                }
            }
            else
            {
                deselect(id);
            }

            
        }
    }

    public void deselect(int id)
    {
        HighlightConntroller.setHighlightOfElemToState(selectedTable[id], false);

        cr_zone_controller.removePotentionallyCriticElement(selectedTable[id]);
        selectedTable.Remove(id);
        if (selectedTable.Count == 0)
        {
            menu.setCriticZoneControllerPanelVisibility(false);
        }
    }

    public void deselectAll()
    {
        HashSet<GameObject> potentionallyCriticPoints = new HashSet<GameObject>(cr_zone_controller.getPotentionallyCriticPoints());
        HashSet<GameObject> pointsToDisable = new HashSet<GameObject>(selectedTable.Values);
        pointsToDisable.ExceptWith(potentionallyCriticPoints);

        foreach (var point in pointsToDisable)
        {
            if(point != null)
            {
                HighlightConntroller.setHighlightOfElemToState(point, false);
            }
        }
        selectedTable.Clear();
        menu.setInfoPanelVisibilityToState(false);
        if (cr_zone_controller.getPotentionallyCriticPoints().Count == 0)
        {
            menu.setCriticZoneControllerPanelVisibility(false);
        }
       
    }
    private void fillMenu(BuildingInfo info)
    {
        menu.setHeaderInfo(info.main_title);
        menu.setMainText(info.main_info);
    }

    public HashSet<GameObject> getSelectedElements()
    {
        return new HashSet<GameObject>(selectedTable.Values);
    }

    public void Undo()
    {
        deselectAll();
        Debug.Log(buffer.Values.Count);
        foreach (var go in buffer)
        {
            addSelected(go.Value);
        }
    }
}
