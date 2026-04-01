using System.Collections; // Необходимо для корутин
using UnityEngine;

public class AmbientSoundPlayer : MonoBehaviour // Убедитесь, что имя класса совпадает с именем файла!
{
    [Header("Audio Setup")]
    public AudioClip[] ambientClips; // Массив для хранения всех ваших звуков окружения
    // Убрал ссылку на AudioMixerGroup здесь, так как мы настраиваем ее через AudioSource
    // public AudioMixerGroup sfxGroup; 

    [Header("Timing (Seconds)")]
    public float minDelay = 8.0f;  // Минимальная пауза между звуками (пример)
    public float maxDelay = 20.0f; // Максимальная пауза между звуками (пример)

    private AudioSource audioSource;

    void Start()
    {
        // Получаем компонент AudioSource с этого же объекта
        audioSource = GetComponent<AudioSource>();

        // Проверка, есть ли AudioSource
        if (audioSource == null)
        {
            Debug.LogError("AmbientSoundPlayer: Компонент AudioSource не найден на этом GameObject! Пожалуйста, добавьте AudioSource.", this.gameObject);
            return; // Останавливаем выполнение, если нет AudioSource
        }

        // Проверка, что AudioSource настроен на нужную группу микшера (ВАЖНО!)
        if (audioSource.outputAudioMixerGroup == null)
        {
            Debug.LogWarning("AmbientSoundPlayer: У AudioSource не назначена группа Output (AudioMixerGroup). Звуки будут идти мимо микшера!", this.gameObject);
            // Рекомендуется назначить группу SFX вручную в инспекторе на компоненте AudioSource!
        }
        else
        {
            Debug.Log($"AmbientSoundPlayer: AudioSource настроен на группу '{audioSource.outputAudioMixerGroup.name}'.", this.gameObject);
        }

        // Убедимся, что AudioSource не играет сам по себе
        audioSource.playOnAwake = false;
        audioSource.loop = false;


        // Проверяем, есть ли звуки для воспроизведения
        if (ambientClips != null && ambientClips.Length > 0)
        {
            // Запускаем корутину, которая будет проигрывать звуки
            StartCoroutine(PlayAmbientSoundsPeriodically());
            Debug.Log("AmbientSoundPlayer: Запуск корутины для проигрывания звуков.", this.gameObject);
        }
        else
        {
            Debug.LogWarning("AmbientSoundPlayer: Массив ambientClips пуст или не назначен. Звуки окружения не будут воспроизводиться.", this.gameObject);
        }
    }

    IEnumerator PlayAmbientSoundsPeriodically()
    {
        // Небольшая начальная задержка перед первым звуком (опционально)
        yield return new WaitForSeconds(Random.Range(1.0f, minDelay));

        // Бесконечный цикл, пока объект активен
        while (true)
        {
            // 1. Выбираем случайный звук из массива
            // Проверяем, что массив все еще валиден и не пуст
            if (ambientClips == null || ambientClips.Length == 0)
            {
                Debug.LogWarning("AmbientSoundPlayer: Массив ambientClips стал пустым или null во время работы.", this.gameObject);
                yield break; // Выходим из корутины, если нет клипов
            }

            int randomIndex = Random.Range(0, ambientClips.Length);
            AudioClip clipToPlay = ambientClips[randomIndex];

            // 2. Проигрываем выбранный звук через AudioSource
            if (clipToPlay != null && audioSource != null)
            {
                // Используем PlayOneShot, чтобы звуки могли немного накладываться
                audioSource.PlayOneShot(clipToPlay);
                // Debug.Log($"Playing ambient sound: {clipToPlay.name}"); // Для отладки
            }
            else if (clipToPlay == null)
            {
                Debug.LogWarning($"AmbientSoundPlayer: AudioClip в массиве ambientClips по индексу {randomIndex} равен null.", this.gameObject);
            }

            // 3. Выбираем случайную паузу
            float delay = Random.Range(minDelay, maxDelay);
            // Debug.Log($"AmbientSoundPlayer: Следующий звук через {delay:F2} секунд."); // Для отладки

            // 4. Ждем эту паузу
            yield return new WaitForSeconds(delay);
        }
    }
}