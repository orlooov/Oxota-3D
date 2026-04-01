using System.Collections.Generic;
using System.Linq; // Потребуется для работы с разрешениями
using UnityEngine;
using UnityEngine.Audio;
using UnityEngine.UI;

public class SettingsManager : MonoBehaviour
{
    [Header("Audio Settings")]
    public AudioMixer mainMixer; // Перетащите сюда ваш MainMixer из Project
    public Slider musicVolumeSlider;
    public Slider sfxVolumeSlider; // Слайдер для "Громкость в игре"

    [Header("Graphics Settings")]
    public TMPro.TMP_Dropdown resolutionDropdown; // Используем TextMeshPro Dropdown
    public Toggle vSyncToggle;

    private Resolution[] resolutions; // Массив доступных разрешений экрана
    private List<Resolution> filteredResolutions; // Отфильтрованные разрешения (по частоте)

    // Ключи для сохранения настроек
    private const string MusicVolKey = "MusicVolume";
    private const string SFXVolKey = "SFXVolume";
    private const string ResolutionWidthKey = "ResolutionWidth";
    private const string ResolutionHeightKey = "ResolutionHeight";
    private const string VSyncKey = "VSyncEnabled";

    void Start()
    {
        // --- Настройка Разрешений ---
        SetupResolutions();

        // --- Загрузка Настроек ---
        LoadSettings();

        // --- Добавление Слушателей событий для UI ---
        // Убедитесь, что слушатели добавляются только один раз
        musicVolumeSlider.onValueChanged.RemoveAllListeners(); // Очистка на всякий случай
        musicVolumeSlider.onValueChanged.AddListener(SetMusicVolume);

        sfxVolumeSlider.onValueChanged.RemoveAllListeners();
        sfxVolumeSlider.onValueChanged.AddListener(SetSFXVolume);

        resolutionDropdown.onValueChanged.RemoveAllListeners();
        resolutionDropdown.onValueChanged.AddListener(SetResolution);

        vSyncToggle.onValueChanged.RemoveAllListeners();
        vSyncToggle.onValueChanged.AddListener(SetVSync);
    }

    void SetupResolutions()
    {
        resolutions = Screen.resolutions;
        filteredResolutions = new List<Resolution>();

        resolutionDropdown.ClearOptions();
        int currentRefreshRate = (int)Screen.currentResolution.refreshRateRatio.value; // Исправленная строка

        // Фильтруем разрешения, оставляя только уникальные и с текущей частотой обновления
        for (int i = 0; i < resolutions.Length; i++)
        {
            if ((int)resolutions[i].refreshRateRatio.value == currentRefreshRate) // Исправленная строка
            {
                // Проверка на уникальность разрешения (ширина x высота)
                bool alreadyExists = filteredResolutions.Any(res => res.width == resolutions[i].width && res.height == resolutions[i].height);
                if (!alreadyExists)
                {
                    filteredResolutions.Add(resolutions[i]);
                }
            }
        }

        List<string> options = new List<string>();
        foreach (Resolution res in filteredResolutions)
        {
            options.Add($"{res.width} x {res.height} @ {res.refreshRateRatio.value:0}Hz"); // Обновленный формат
        }

        resolutionDropdown.AddOptions(options);
        resolutionDropdown.RefreshShownValue();
    }


    public void SetMusicVolume(float volume)
    {
        // Audio Mixer использует децибелы (логарифмическая шкала)
        // Конвертируем линейное значение слайдера (0.0001 до 1) в дБ (-80 до 0)
        mainMixer.SetFloat("MusicVolume", Mathf.Log10(volume) * 20);
        PlayerPrefs.SetFloat(MusicVolKey, volume);
        // PlayerPrefs.Save(); // Можно сохранять сразу или по кнопке "Применить"
    }

    public void SetSFXVolume(float volume)
    {
        mainMixer.SetFloat("SFXVolume", Mathf.Log10(volume) * 20);
        PlayerPrefs.SetFloat(SFXVolKey, volume);
        // PlayerPrefs.Save();
    }

    public void SetResolution(int resolutionIndex)
    {
        if (resolutionIndex < 0 || resolutionIndex >= filteredResolutions.Count) return;

        Resolution selectedResolution = filteredResolutions[resolutionIndex];
        Screen.SetResolution(selectedResolution.width, selectedResolution.height, Screen.fullScreen); // Используем текущий режим экрана

        // Сохраняем выбранное разрешение
        PlayerPrefs.SetInt(ResolutionWidthKey, selectedResolution.width);
        PlayerPrefs.SetInt(ResolutionHeightKey, selectedResolution.height);
        // PlayerPrefs.Save();
    }

    public void SetVSync(bool isVSyncOn)
    {
        QualitySettings.vSyncCount = isVSyncOn ? 1 : 0; // 1 - вкл, 0 - выкл
        PlayerPrefs.SetInt(VSyncKey, isVSyncOn ? 1 : 0);
        // PlayerPrefs.Save();
    }

    public void SaveAllSettings()
    {
        // Этот метод можно вызвать по нажатию кнопки "Применить" или "Назад"
        PlayerPrefs.Save();
        Debug.Log("Settings Saved!");
    }

    void LoadSettings()
    {
        // --- Загрузка Громкости ---
        float musicVol = PlayerPrefs.GetFloat(MusicVolKey, 0.75f); // 0.75f - значение по умолчанию
        musicVolumeSlider.value = musicVol;
        SetMusicVolume(musicVol); // Применяем загруженное значение

        float sfxVol = PlayerPrefs.GetFloat(SFXVolKey, 0.75f);
        sfxVolumeSlider.value = sfxVol;
        SetSFXVolume(sfxVol);

        // --- Загрузка V-Sync ---
        int vSyncPref = PlayerPrefs.GetInt(VSyncKey, 1); // По умолчанию VSync включен (1)
        bool isVSyncOn = vSyncPref == 1;
        vSyncToggle.isOn = isVSyncOn;
        SetVSync(isVSyncOn); // Применяем

        // --- Загрузка и установка Разрешения ---
        int savedWidth = PlayerPrefs.GetInt(ResolutionWidthKey, Screen.currentResolution.width);
        int savedHeight = PlayerPrefs.GetInt(ResolutionHeightKey, Screen.currentResolution.height);

        int currentResolutionIndex = -1;
        for (int i = 0; i < filteredResolutions.Count; i++)
        {
            if (filteredResolutions[i].width == savedWidth && filteredResolutions[i].height == savedHeight)
            {
                currentResolutionIndex = i;
                break;
            }
        }

        // Если сохраненное разрешение найдено в списке, устанавливаем его в Dropdown
        if (currentResolutionIndex != -1)
        {
            resolutionDropdown.value = currentResolutionIndex;
            // Применять разрешение при загрузке необязательно, если оно уже установлено
            // Screen.SetResolution(savedWidth, savedHeight, Screen.fullScreen);
        }
        else
        {
            // Если сохраненное не найдено (например, сменился монитор), ищем текущее
            currentResolutionIndex = -1;
            for (int i = 0; i < filteredResolutions.Count; i++)
            {
                if (filteredResolutions[i].width == Screen.currentResolution.width && filteredResolutions[i].height == Screen.currentResolution.height)
                {
                    currentResolutionIndex = i;
                    break;
                }
            }
            if (currentResolutionIndex != -1)
            {
                resolutionDropdown.value = currentResolutionIndex;
            }
            else
            {
                // Если и текущее не найдено, ставим первое попавшееся
                resolutionDropdown.value = 0;
                SetResolution(0); // Применяем первое из списка
            }
        }
        resolutionDropdown.RefreshShownValue();

        Debug.Log("Settings Loaded!");
    }
}