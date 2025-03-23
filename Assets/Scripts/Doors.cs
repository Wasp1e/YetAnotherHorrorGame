using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class Doors : MonoBehaviour
{
    public Animator door;
    // public GameObject lockOB;
    // public GameObject keyOB;

    private AudioSource openSound;
    private AudioSource closeSound;
    private AudioSource lockedSound;
    // public AudioSource unlockedSound;

    private bool inReach;
    private bool doorisOpen;
    private Hint hintScript;
    private bool doorisClosed;
    public bool locked;
    public bool lockedlocked;
    public bool unlocked;
    public static bool interactMessageWasShown = false;

    public Transform player; // Добавляем ссылку на трансформ игрока

    void Awake()
    {
        GameObject doorOpenObject = GameObject.Find("Player/Sounds/DoorOpen");
        openSound = doorOpenObject.GetComponent<AudioSource>();
        GameObject doorCloseObject = GameObject.Find("Player/Sounds/DoorClose");
        closeSound = doorCloseObject.GetComponent<AudioSource>();
        GameObject doorLockedObject = GameObject.Find("Player/Sounds/DoorLocked");
        lockedSound = doorLockedObject.GetComponent<AudioSource>();
    }

    void Start()
    {
        GameObject hintObject = GameObject.FindGameObjectWithTag("Hint");
        if (hintObject != null)
        {
            hintScript = hintObject.GetComponent<Hint>();
        }
        inReach = false;
        doorisClosed = true;
        doorisOpen = false;

        // Находим игрока по тегу
        player = GameObject.FindGameObjectWithTag("MainCamera").transform;
    }

    void OnTriggerEnter(Collider other)
    {
        if (other.gameObject.tag == "Reach" && doorisClosed)
        {
            inReach = true;
            if (!interactMessageWasShown && !hintScript.hintIsActive)
            {
                Debug.Log("Message shown");
                hintScript.ShowHint("E to interact", 0f);
                interactMessageWasShown = true;
            }
        }

        if (other.gameObject.tag == "Reach" && doorisOpen)
        {
            inReach = true;
        }
    }

    void OnTriggerExit(Collider other)
    {
        if (other.gameObject.tag == "Reach")
        {
            inReach = false;
        }
    }

    void Update()
    {
        // if (lockOB.activeInHierarchy)
        // {
        //     locked = true;
        //     unlocked = false;
        // }
        // else
        // {
        //     unlocked = true;
        //     locked = false;
        // }

        if (inReach && !lockedlocked && Input.GetButtonDown("Interact"))
        {
            locked = false;
            StartCoroutine(unlockDoor());
        }

        if (inReach && !lockedlocked && doorisClosed && unlocked && Input.GetButtonDown("Interact"))
        {
            OpenDoor();
        }
        else if (inReach && !lockedlocked && doorisOpen && unlocked && Input.GetButtonDown("Interact"))
        {
            CloseDoor();
        }
        else if (inReach && lockedlocked && Input.GetButtonDown("Interact"))
        {
            lockedSound.Play();
        }
    }

    IEnumerator unlockDoor()
    {
        yield return new WaitForSeconds(.05f);
        {
            unlocked = true;
        }
    }

    private void OpenDoor()
    {
        // Направление от двери к игроку
        Vector3 doorToPlayer = player.position - transform.position;
        doorToPlayer.y = 0; // Игнорируем разницу по высоте
        doorToPlayer.Normalize(); // Нормализуем вектор

        // Направление двери (предполагаем, что дверь изначально ориентирована правильно)
        Vector3 doorForward = transform.forward;
        doorForward.y = 0; // Игнорируем разницу по высоте
        doorForward.Normalize(); // Нормализуем вектор

        // Угол между направлением двери и направлением к игроку
        float angle = Vector3.SignedAngle(doorForward, doorToPlayer, Vector3.up);

        Debug.Log("Door Forward: " + doorForward);
        Debug.Log("Door to Player: " + doorToPlayer);
        Debug.Log("Angle: " + angle);

        // Определяем, в какую сторону открывать дверь
        if (Mathf.Abs(angle) > 90)
        {
            door.SetBool("OpenForward", true);
            door.SetBool("OpenBackward", false);
        }
        else
        {
            door.SetBool("OpenForward", false);
            door.SetBool("OpenBackward", true);
        }

        door.SetBool("Closed", false);
        openSound.Play();
        doorisOpen = true;
        doorisClosed = false;
    }
    private void CloseDoor()
    {
        door.SetBool("OpenForward", false);
        door.SetBool("OpenBackward", false);
        door.SetBool("Closed", true);
        closeSound.Play();
        doorisClosed = true;
        doorisOpen = false;
    }
}