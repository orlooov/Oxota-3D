using TMPro;
using UnityEngine;

public class PlayerMoney : MonoBehaviour
{
    public static PlayerMoney Instance { get; private set; }

    [SerializeField] private int startingMoney = 100; // Стартовые деньги
    private int currentMoney;

    [Header("UI References")]
    public TextMeshProUGUI moneyText; // Ссылка на текстовый элемент UI

    private void Awake()
    {
        // Реализация синглтона
        if (Instance == null)
        {
            Instance = this;
            DontDestroyOnLoad(gameObject);
            currentMoney = startingMoney;
            UpdateMoneyUI();
        }
        else
        {
            Destroy(gameObject);
        }
    }

    // Добавление денег
    public void AddMoney(int amount)
    {
        currentMoney += amount;
        UpdateMoneyUI();
        Debug.Log($"Получено {amount} монет. Всего: {currentMoney}");

        // Здесь можно добавить эффекты: звук, анимацию и т.д.
    }

    // Проверка, хватает ли денег
    public bool CanAfford(int amount)
    {
        return currentMoney >= amount;
    }

    // Списание денег
    public bool SpendMoney(int amount)
    {
        if (CanAfford(amount))
        {
            currentMoney -= amount;
            UpdateMoneyUI();
            return true;
        }
        return false;
    }

    // Обновление UI
    private void UpdateMoneyUI()
    {
        if (moneyText != null)
        {
            moneyText.text = $"{currentMoney}";
        }
    }

    // Получение текущего количества денег
    public int GetCurrentMoney()
    {
        return currentMoney;
    }
}