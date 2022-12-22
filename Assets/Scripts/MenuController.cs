using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

public enum AdditionalDataType
{
    Cargo
}

public class MenuController : MonoBehaviour
{
    public GameObject header;
    public GameObject mainText;

    public GameObject additionalDataHeader;
    public GameObject additionalData;

    public GameObject buildingInfoPanel;
    public GameObject criticZoneInfoPanel;

    private Dictionary<AdditionalDataType, string> additionaDataHeaders = new Dictionary<AdditionalDataType, string>();

    private void Start()
    {
        fillAdditionalDataHeaders();
        setInfoPanelVisibilityToState(false);
        setCriticZoneControllerPanelVisibility(false);
    }

    private void fillAdditionalDataHeaders()
    {
        additionaDataHeaders.Add(AdditionalDataType.Cargo, "Тип груза");
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

    public void setCargoInfoVisibilityToState(bool state)
    {
        additionalDataHeader.SetActive(state);
        additionalData.SetActive(state);
    }

    public void setAdditionalData(AdditionalDataType dataType, string dataText)
    {
        if (additionaDataHeaders.ContainsKey(dataType))
        {
            additionalDataHeader.GetComponent<Text>().text = additionaDataHeaders[dataType];
            additionalData.GetComponent<Text>().text = dataText;
        }
       
    }
}
