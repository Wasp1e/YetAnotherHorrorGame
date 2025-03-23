using UnityEngine;

public class Doors : MonoBehaviour
{
    public Animator door;
    private AudioSource openSound;
    private AudioSource closeSound;
    private bool inReach;
    private bool doorisOpen;
    private Transform player; // Получаем игрока

    void Awake()
    {
        openSound = GameObject.Find("Player/Sounds/DoorOpen").GetComponent<AudioSource>();
        closeSound = GameObject.Find("Player/Sounds/DoorClose").GetComponent<AudioSource>();
        player = GameObject.FindGameObjectWithTag("Player").transform;
    }

    void Start()
    {
        inReach = false;
        doorisOpen = false;
    }

    void OnTriggerEnter(Collider other)
    {
        if (other.CompareTag("Reach"))
        {
            inReach = true;
        }
    }

    void OnTriggerExit(Collider other)
    {
        if (other.CompareTag("Reach"))
        {
            inReach = false;
        }
    }

    void Update()
    {
        if (inReach && Input.GetButtonDown("Interact"))
        {
            if (!doorisOpen)
            {
                OpenDoor();
            }
            else
            {
                CloseDoor();
            }
        }
    }

    void OpenDoor()
    {
        Vector3 toPlayer = player.position - transform.position;
        float dot = Vector3.Dot(transform.forward, toPlayer.normalized);

<<<<<<< Updated upstream
        if (dot > 0)
=======
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

        // Определяем, в какую сторону открывать дверь
        if (Mathf.Abs(angle) > 90)
>>>>>>> Stashed changes
        {
            door.SetBool("OpenForward", true);
        }
        else
        {
            door.SetBool("OpenBackward", true);
        }

        openSound.Play();
        doorisOpen = true;
    }

    void CloseDoor()
    {
        door.SetBool("OpenForward", false);
        door.SetBool("OpenBackward", false);
        closeSound.Play();
        doorisOpen = false;
    }
}