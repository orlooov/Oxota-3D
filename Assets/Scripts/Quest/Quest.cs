using UnityEngine;

[CreateAssetMenu(fileName = "New Quest", menuName = "Quests/Quest")]
public class Quest : ScriptableObject
{
    [Header("Основное")]
    public string questID;
    public string title = "Охота на волков";
    [TextArea(3, 5)] public string description = "Убейте 1 волка в лесу.";

    [Header("Цели")]
    public string targetAnimalType = "Wolf";
    public int targetAmount = 1;

    [Header("Награды")]
    public int expReward = 100;
    public int moneyReward = 50;
    // Можно добавить другие награды (предметы и т.д.)
}