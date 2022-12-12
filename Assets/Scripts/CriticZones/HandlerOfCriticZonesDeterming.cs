using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public struct ResultOfTryToDetermineCriticZone
{
    public int countOfCorrectElements;
    public int totalQuantityOfCriticElements;

    public ResultOfTryToDetermineCriticZone(int c, int a)
    {
        countOfCorrectElements = c;
        totalQuantityOfCriticElements = a;
    }
}

public static class HandlerOfCriticZonesDeterming
{
    static private List<ResultOfTryToDetermineCriticZone> resultsOfTry = new List<ResultOfTryToDetermineCriticZone>();

    public static void addResultOfTry(ResultOfTryToDetermineCriticZone newResult)
    {
        resultsOfTry.Add(newResult);
    }

    public static ResultOfTryToDetermineCriticZone? getResultsOfLastTry()
    {
        if (resultsOfTry.Count > 0)
        {
            return resultsOfTry[resultsOfTry.Count - 1];
        }
        return null;
    }

    public static void showLastResultOn(GameObject menuObj)
    {
        MenuController menu = menuObj.GetComponent<MenuController>();

        if (menu)
        {
            if (getResultsOfLastTry().HasValue)
            {
                float proc = 
                   (float) getResultsOfLastTry().Value.countOfCorrectElements / (float) getResultsOfLastTry().Value.totalQuantityOfCriticElements * 100;
                string main_text = "Вы выбрали правильно " + proc.ToString("0.00") + "%";

                menu.setHeaderInfo("Результат выполнения работы");
                menu.setMainText(main_text);

                menu.setInfoPanelVisibilityToState(true);
            }
        }
    }
}
