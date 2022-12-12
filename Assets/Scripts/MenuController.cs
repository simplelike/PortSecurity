using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

public class MenuController : MonoBehaviour
{
    public GameObject header;
    public GameObject mainText;

    public GameObject buildingInfoPanel;
    public GameObject criticZoneInfoPanel;

    private void Start()
    {
        setInfoPanelVisibilityToState(false);
        setCriticZoneControllerPanelVisibility(false);
    }
    public void setHeaderInfo(string text)
    {
        header.GetComponentInChildren<Text>().text = text;
    }
    public void setMainText(string text)
    {
        mainText.GetComponentInChildren<Text>().text = text;
    }

    public void closeMenu()
    {
        setInfoPanelVisibilityToState(false);
    }

    public void setInfoPanelVisibilityToState(bool state)
    {
        buildingInfoPanel.SetActive(state);
    }
    public void setCriticZoneControllerPanelVisibility(bool state)
    {
        criticZoneInfoPanel.SetActive(state);
    }
}
