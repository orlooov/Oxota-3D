using System.Collections;
using UnityEngine;

// Убедитесь, что имя класса ТОЧНО совпадает с именем файла: AmbientAudioManager
public class AmbientAudioManager : MonoBehaviour
{
    [Header("Managed Audio Source")]
    [Tooltip("Перетащите сюда компонент AudioSource с этого же объекта")]
    public AudioSource managedAudioSource;

    [Header("Bird Sounds")]
    [Tooltip("Аудиоклипы ТОЛЬКО для пения птиц")]
    public AudioClip[] birdClips;
    [Tooltip("Минимальная пауза между попытками проиграть звук птиц (секунды)")]
    public float birdMinDelay = 4.0f;
    [Tooltip("Максимальная пауза между попытками проиграть звук птиц (секунды)")]
    public float birdMaxDelay = 12.0f;
    [Tooltip("Шанс (0.0 до 1.0), что птицы промолчат, когда придет их время")]
    [Range(0.0f, 1.0f)]
    public float birdSilenceChance = 0.15f;

    [Header("Accent Sounds")]
    [Tooltip("Аудиоклипы ТОЛЬКО для редких акцентов (ветки, дятел и т.д.)")]
    public AudioClip[] accentClips;
    [Tooltip("Минимальная пауза между акцентными звуками (секунды)")]
    public float accentMinDelay = 20.0f;
    [Tooltip("Максимальная пауза между акцентными звуками (секунды)")]
    public float accentMaxDelay = 60.0f;

    [Header("Optional: Fill Silence")]
    [Tooltip("Если птицы молчат (из-за шанса тишины), попробовать ли вместо них сыграть акцентный звук?")]
    public bool tryPlayAccentOnBirdSilence = true;
    [Tooltip("Шанс (0.0 до 1.0) сыграть акцент, ЕСЛИ птицы молчат И опция выше включена")]
    [Range(0.0f, 1.0f)]
    public float accentFillChance = 0.5f; // 50% шанс

    // Внутренние таймеры для планирования
    private float nextBirdTime = 0f;
    private float nextAccentTime = 0f;

    void Start()
    {
        // Пытаемся найти AudioSource, если он не назначен в инспекторе
        if (managedAudioSource == null)
        {
            managedAudioSource = GetComponent<AudioSource>();
        }

        // Критическая проверка: без AudioSource работать не можем
        if (managedAudioSource == null)
        {
            Debug.LogError("AmbientAudioManager: Компонент AudioSource НЕ НАЙДЕН или НЕ НАЗНАЧЕН в инспекторе! Скрипт не может работать.", this.gameObject);
            enabled = false; // Выключаем скрипт
            return;
        }

        // Проверка, что AudioSource подключен к микшеру (важно!)
        if (managedAudioSource.outputAudioMixerGroup == null)
        {
            Debug.LogWarning("AmbientAudioManager: У назначенного 'Managed Audio Source' не настроен 'Output' (AudioMixerGroup)! Звуки пойдут мимо микшера.", this.gameObject);
        }

        // Убедимся, что основной AudioSource не играет сам по себе
        managedAudioSource.playOnAwake = false;
        managedAudioSource.loop = false;

        // Планируем первое срабатывание для каждого типа звуков
        ScheduleNextBird(true); // true = первый запуск, добавит доп. вариативности
        ScheduleNextAccent(true);

        // Запускаем основную корутину, которая будет следить за временем
        StartCoroutine(ManageAmbientSounds());
        Debug.Log("AmbientAudioManager: Запущен.", this.gameObject);
    }

    // Основной цикл управления звуками
    IEnumerator ManageAmbientSounds()
    {
        while (true)
        {
            // 1. Определяем, сколько ждать до БЛИЖАЙШЕГО следующего события (птицы или акцента)
            float timeToNextBird = nextBirdTime - Time.time;
            float timeToNextAccent = nextAccentTime - Time.time;
            // Используем Max(0, ...) чтобы не получить отрицательное время ожидания, если событие уже просрочено
            float timeToWait = Mathf.Min(Mathf.Max(0, timeToNextBird), Mathf.Max(0, timeToNextAccent));

            // 2. Ждем это время
            if (timeToWait > 0)
            {
                yield return new WaitForSeconds(timeToWait);
            }

            // 3. Ждем, пока AudioSource освободится (если он еще играет предыдущий звук)
            // Добавляем таймаут, чтобы не зависнуть навсегда
            float waitStartTime = Time.time;
            while (managedAudioSource.isPlaying)
            {
                if (Time.time - waitStartTime > 15f) // Таймаут 15 секунд
                {
                    Debug.LogWarning("AmbientAudioManager: AudioSource занят более 15 секунд! Пропускаем и перепланируем.", this.gameObject);
                    managedAudioSource.Stop(); // Останавливаем на всякий случай
                    ScheduleNextBird(false); // Перепланируем оба
                    ScheduleNextAccent(false);
                    yield return new WaitForSeconds(1.0f); // Короткая пауза
                    continue; // Начинаем цикл while заново
                }
                yield return null; // Ждем следующий кадр
            }

            // 4. Определяем, какой тип звука должен сейчас прозвучать
            bool isAccentTime = Time.time >= nextAccentTime;
            bool isBirdTime = Time.time >= nextBirdTime;

            // --- Логика выбора и воспроизведения ---

            if (isAccentTime && accentClips != null && accentClips.Length > 0)
            {
                // Время акцента! Играем его.
                PlayRandomClip(accentClips, "Accent");
                ScheduleNextAccent(false); // Планируем следующий акцент

                // Если время птиц тоже подошло, просто перепланируем птиц (акцент важнее)
                if (isBirdTime)
                {
                    ScheduleNextBird(false, 0.5f); // Планируем птиц чуть позже
                }
            }
            else if (isBirdTime && birdClips != null && birdClips.Length > 0)
            {
                // Время птиц! Проверяем шанс тишины.
                if (Random.value > birdSilenceChance)
                {
                    // Шанс тишины не сработал - играем птиц.
                    PlayRandomClip(birdClips, "Bird");
                    ScheduleNextBird(false); // Планируем следующих птиц
                }
                else
                {
                    // Птицы должны молчать. Проверяем опцию "Заполнить тишину".
                    if (tryPlayAccentOnBirdSilence && accentClips != null && accentClips.Length > 0 && Random.value < accentFillChance)
                    {
                        // Играем акцентный звук ВМЕСТО тишины птиц
                        PlayRandomClip(accentClips, "Accent (Fill Silence)");
                        ScheduleNextAccent(false); // Перепланируем акцент, т.к. только что сыграли
                    }
                    else
                    {
                        // Просто тишина от птиц
                        // Debug.Log("Birds silent this time.");
                    }
                    // В любом случае планируем следующую попытку для птиц
                    ScheduleNextBird(false);
                }
            }
            else
            {
                // Ничье время не подошло (или массивы пусты). Просто ждем.
                yield return null;
            }
        }
    }

    // Вспомогательная функция для воспроизведения случайного клипа
    void PlayRandomClip(AudioClip[] clips, string clipType)
    {
        if (clips.Length == 0) return; // Нечего играть

        int randomIndex = Random.Range(0, clips.Length);
        AudioClip clipToPlay = clips[randomIndex];

        if (clipToPlay != null)
        {
            managedAudioSource.PlayOneShot(clipToPlay);
            // Debug.Log($"Playing {clipType}: {clipToPlay.name}");
        }
        else
        {
            Debug.LogWarning($"AmbientAudioManager: Попытка воспроизвести NULL клип из массива '{clipType}' по индексу {randomIndex}.", this.gameObject);
        }
    }

    // Вспомогательная функция для планирования следующей птицы
    void ScheduleNextBird(bool firstRun, float additionalDelay = 0f)
    {
        float delay = Random.Range(birdMinDelay, birdMaxDelay) + additionalDelay;
        // Добавляем небольшую вариативность к первому запуску
        nextBirdTime = Time.time + delay + (firstRun ? Random.Range(0f, birdMinDelay * 0.5f) : 0f);
        // Debug.Log($"Next bird scheduled at: {nextBirdTime:F2}");
    }

    // Вспомогательная функция для планирования следующего акцента
    void ScheduleNextAccent(bool firstRun, float additionalDelay = 0f)
    {
        float delay = Random.Range(accentMinDelay, accentMaxDelay) + additionalDelay;
        nextAccentTime = Time.time + delay + (firstRun ? Random.Range(0f, accentMinDelay * 0.5f) : 0f);
        // Debug.Log($"Next accent scheduled at: {nextAccentTime:F2}");
    }
}