using UnityEngine;
using System.Collections;
using System.Collections.Generic;
public class TurnOffLightsOnTrigger : MonoBehaviour
{
    public AudioSource audiosource;
    public FlashlightController flashlight;
    public AudioClip audioclip;
    public AudioClip drone;

    // Метод, который вызывается при входе другого коллайдера в триггер
    private void OnTriggerEnter(Collider other)
    {
        // Проверяем, если объект, вошедший в триггер, имеет тег "Player"
        if (other.CompareTag("Player"))
        {
            // Выключаем все источники света с тегом "Light"
            TurnOffLightsWithTag();

            // Отключаем или уничтожаем триггер
            DisableTrigger();
            flashlight.IsEnabled = true;
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
            audiosource.PlayOneShot(audioclip, 0.85f);
            PlaySoundWithDelay();

            // Проходим по каждому объекту
            foreach (GameObject lightObject in lightObjects)
            {
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
            audiosource.PlayDelayed(3f);
            audiosource.volume = 0.1f;
            audiosource.loop = true;
        }
        else
        {
            Debug.LogWarning("AudioSource или AudioClip не назначены.");
        }
    }
    private void DisableTrigger()
    {
        // Отключаем объект, на котором находится этот скрипт
        // gameObject.SetActive(false);

        // Или уничтожаем объект (если он больше не нужен в сцене)
        Destroy(gameObject);
    }
}