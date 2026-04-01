using UnityEngine;

public class GameInitializer : MonoBehaviour
{
    [Header("References")]
    [SerializeField] private GameObject ammoUI;
    [SerializeField] private GameObject player;

    void Start()
    {
        // Активируем игрока и UI
        if (ammoUI != null) ammoUI.SetActive(true);
        if (player != null) player.SetActive(true);

        // Гарантируем правильное состояние курсора
        Cursor.lockState = CursorLockMode.Locked;
        Cursor.visible = false;
    }
}