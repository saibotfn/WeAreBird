
using UnityEngine;

public enum QuestStage
{
    Introduction,
    Warmup,
    ReturnToUncle,
    CollectPackage,
    MeetCousin,
    DeliverPackage,
    ReturnMoney,
    Completed
}

public class QuestManager : MonoBehaviour
{
    public static QuestManager Instance { get; private set; }

    public QuestStage currentStage = QuestStage.Introduction;

    void Awake()
    {
        Instance = this;
    }

    public void SetStage(QuestStage newStage)
    {
        currentStage = newStage;

        Debug.Log("New quest stage: " + currentStage);
    }
}
