using System.Collections;
using System.Collections.Generic;
using UnityEngine;

using cakeslice;

public enum CargoType
{
    None, Gases, Fluid, Sand, Tare, Containers 
}
public enum CargoDangerType
{
    None, HighDanger, Danger, NotDanger
}

public class BuildingInfo : MonoBehaviour
{
    public string main_title;
    public string main_info;
    public CargoType cargoType = CargoType.None;
    public CargoDangerType cargoDangerType = CargoDangerType.None;
    public bool isCritic;

    public static Dictionary<CargoType, string> cargoTypeTitles  = new Dictionary<CargoType, string>();
    public static Dictionary<CargoDangerType, string> cargoTypeDangers = new Dictionary<CargoDangerType, string>();

    private void Start()
    {
        fillCargoTypeTitlesDictionary();
        fillCargoDangerLevelsTitlesDictionary();
    }
    private void fillCargoTypeTitlesDictionary()
    {
        if (!cargoTypeTitles.ContainsKey(CargoType.Gases))       cargoTypeTitles.Add(CargoType.Gases, "Газы");
        if (!cargoTypeTitles.ContainsKey(CargoType.Fluid))       cargoTypeTitles.Add(CargoType.Fluid, "Наливные грузы");
        if (!cargoTypeTitles.ContainsKey(CargoType.Sand))        cargoTypeTitles.Add(CargoType.Sand, "Насыпные (навалочные) грузы");
        if (!cargoTypeTitles.ContainsKey(CargoType.Tare))        cargoTypeTitles.Add(CargoType.Tare, "Тарно-штучные грузы");
        if (!cargoTypeTitles.ContainsKey(CargoType.Containers))  cargoTypeTitles.Add(CargoType.Containers, "Контейнеры");
    }
    private void fillCargoDangerLevelsTitlesDictionary()
    {
        if (!cargoTypeDangers.ContainsKey(CargoDangerType.HighDanger))  cargoTypeDangers.Add(CargoDangerType.HighDanger, "Груз повышенной опасности");
        if (!cargoTypeDangers.ContainsKey(CargoDangerType.Danger))  cargoTypeDangers.Add(CargoDangerType.Danger, "Опасный груз");
        if (!cargoTypeDangers.ContainsKey(CargoDangerType.NotDanger))  cargoTypeDangers.Add(CargoDangerType.NotDanger, "Неопасный груз");
    }
}

