using UnityEngine;
using UnityEngine.Rendering.PostProcessing;

public class WeaponZoom : MonoBehaviour
{
    [Header("Main Settings")]
    [SerializeField] private KeyCode zoomKey = KeyCode.Mouse1;
    [SerializeField] private float zoomSpeed = 8f;
    private bool isZoomed = false;

    [Header("Camera Settings")]
    [SerializeField] private Camera mainCamera;
    [SerializeField] private float normalFOV = 60f;
    [SerializeField] private float zoomedFOV = 30f;

    [Header("Weapon Adjustments")]
    [SerializeField] private Transform weaponTransform;
    [SerializeField] private Vector3 zoomPosition = new Vector3(0f, 0.03f, 0.1f);
    [SerializeField] private Vector3 zoomRotation = new Vector3(0f, 0f, 0f);
    private Vector3 normalPosition;
    private Quaternion normalRotation;

    [Header("Post Processing")]
    [SerializeField] private PostProcessVolume postProcessVolume;
    private DepthOfField depthOfField;

    [Header("Audio Settings")]
    [SerializeField] private AudioSource audioSource;
    [SerializeField] private AudioClip zoomInSound;
    [SerializeField] private AudioClip zoomOutSound;
    [Range(0.1f, 1f)][SerializeField] private float volume = 0.7f;
    [Range(0.8f, 1.2f)][SerializeField] private float pitchVariation = 0.1f;

    void Start()
    {
        // Сохраняем исходную позицию и вращение
        normalPosition = weaponTransform.localPosition;
        normalRotation = weaponTransform.localRotation;

        // Получаем компоненты постобработки
        postProcessVolume.profile.TryGetSettings(out depthOfField);

        // Проверяем AudioSource
        if (audioSource == null)
        {
            audioSource = gameObject.AddComponent<AudioSource>();
            audioSource.spatialBlend = 0f; // 2D звук
            audioSource.playOnAwake = false;
        }
    }

    void Update()
    {
        if (Input.GetKeyDown(zoomKey))
        {
            ToggleZoom();
        }

        ApplyZoomEffects();
    }

    void ToggleZoom()
    {
        isZoomed = !isZoomed;
        PlayZoomSound();
    }

    void PlayZoomSound()
    {
        if (audioSource == null || (isZoomed && zoomInSound == null) || (!isZoomed && zoomOutSound == null))
            return;

        // Настраиваем параметры звука
        audioSource.volume = volume;
        audioSource.pitch = 1f + Random.Range(-pitchVariation, pitchVariation);

        // Проигрываем соответствующий звук
        audioSource.PlayOneShot(isZoomed ? zoomInSound : zoomOutSound);
    }

    void ApplyZoomEffects()
    {
        float delta = zoomSpeed * Time.deltaTime;

        // Плавное изменение FOV
        mainCamera.fieldOfView = Mathf.Lerp(
            mainCamera.fieldOfView,
            isZoomed ? zoomedFOV : normalFOV,
            delta
        );

        // Плавное перемещение оружия
        weaponTransform.localPosition = Vector3.Lerp(
            weaponTransform.localPosition,
            isZoomed ? zoomPosition : normalPosition,
            delta
        );

        // Плавное вращение оружия
        weaponTransform.localRotation = Quaternion.Lerp(
            weaponTransform.localRotation,
            isZoomed ? Quaternion.Euler(zoomRotation) : normalRotation,
            delta
        );

        // Настройка размытия
        if (depthOfField != null)
        {
            depthOfField.active = isZoomed;
            depthOfField.focusDistance.value = Mathf.Lerp(
                depthOfField.focusDistance.value,
                isZoomed ? 2f : 10f,
                delta
            );
        }
    }
}