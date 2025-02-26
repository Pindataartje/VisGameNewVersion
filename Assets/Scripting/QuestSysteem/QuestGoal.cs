using System.Collections;
using System.Collections.Generic;
using UnityEngine;

[System.Serializable]
public class QuestGoal
{
    public QuestType GoalType;

    public int requiredAmount;
    public int currentAmount;


    public bool isReached()
    {
        return (currentAmount >= requiredAmount);
    }

    public void EnemyKilled()  // als we een bepaalde enemy willen die de speler moet doden kunnen we een tag in de argument zetten
    {
        if (GoalType == QuestType.Kill)
        {
            currentAmount++;
        }
    }
    public void Gathered()
    {
        if (GoalType == QuestType.Gather)
        {
            currentAmount++;
        }
    }
    public void AreaTook()
    {
        if (GoalType == QuestType.TakeArea)
        {
            currentAmount++;
        }
    }
    public void Found()
    {
        if (GoalType == QuestType.Find)
        {
            currentAmount++;
        }
    }

}

public enum QuestType
{
    Kill,
    Gather,
    TakeArea,
    Find
}
