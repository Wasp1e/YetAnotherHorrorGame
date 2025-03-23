using UnityEngine;
using System.Collections;
using System.Collections.Generic;
public class TurnOffLightsOnTrigger : MonoBehaviour
{
    public AudioSource audiosource;
    public AudioSource audioSource2;
    public FlashlightController flashlight;
    public AudioClip audioclip;
    private bool hasTriggered = false;
    public AudioClip drone;
    private Hint hintScript;
    public float fadeInDuration = 5f; // Длительность плавного появления звука

    void Start()
    {
        GameObject hintObject = GameObject.FindGameObjectWithTag("Hint");
        if (hintObject != null)
        {
            hintScript = hintObject.GetComponent<Hint>();
        }
    }
    // Метод, который вызывается при входе другого коллайдера в триггер
    private void OnTriggerEnter(Collider other)
    {
        // Проверяем, если объект, вошедший в триггер, имеет тег "Player"
        if (other.CompareTag("Player") && !hasTriggered)
        {
            hasTriggered = true;
            audioSource2.PlayOneShot(audioclip, 0.85f);
            PlaySoundWithDelay();
            StartCoroutine(FadeIn());
            // Выключаем все источники света с тегом "Light"
            TurnOffLightsWithTag();

            // Отключаем или уничтожаем триггер
            flashlight.IsEnabled = true;

            hintScript.ShowHint("Press F to use Flashlight", 2f);
        }

    }
    // Метод для выключения всех источников света с тегом "Light"
    private void TurnOffLightsWithTag()
    {
        // Находим все объекты с тегом "Light"
        GameObject[] lightObjects = GameObject.FindGameObjectsWithTag("Light");
        GameObject[] cylinders = GameObject.FindGameObjectsWithTag("Cylinder");

        // Если объекты найдены
        if (lightObjects.Length > 0)
        {
            // Проходим по каждому объекту
            foreach (GameObject lightObject in lightObjects)
            {
                LightsFlicker.isOn = false;
                AudioSource audioComponent = lightObject.GetComponent<AudioSource>();
                // Получаем компонент Light
                Light lightComponent = lightObject.GetComponent<Light>();

                // Если компонент Light существует, выключаем его
                if (lightComponent != null)
                {
                    lightComponent.enabled = false;
                }
                if (audioComponent != null)
                {
                    audioComponent.enabled = false;
                }
            }

            // Отключаем все объекты с тегом "Cylinder"
            foreach (GameObject cylinder in cylinders)
            {
                cylinder.SetActive(false);
            }
        }
        else
        {
            Debug.LogWarning("Объекты с тегом 'Light' не найдены.");
        }
    }

    // Метод для отключения или уничтожения триггера
    // Метод для воспроизведения звука с задержкой
    private void PlaySoundWithDelay()
    {
        if (audiosource != null && drone != null)
        {
            audiosource.clip = drone;
            audiosource.PlayDelayed(0f);
            audiosource.loop = true;
        }
        else
        {
            Debug.LogWarning("AudioSource или AudioClip не назначены.");
        }
    }

    IEnumerator FadeIn()
    {
        float timer = 0f;

        while (timer < fadeInDuration)
        {
            timer += Time.deltaTime;
            // Плавно увеличиваем громкость и ограничиваем её диапазоном [0, 1]
            audiosource.volume = Mathf.Clamp01(Mathf.Lerp(0f, 0.1f, timer / fadeInDuration));
            yield return null; // Ждем следующий кадр
        }

    }
    private void DisableTrigger()
    {
        // Отключаем объект, на котором находится этот скрипт
        gameObject.SetActive(false);

        // Или уничтожаем объект (если он больше не нужен в сцене)
        // Destroy(gameObject);
    }
}