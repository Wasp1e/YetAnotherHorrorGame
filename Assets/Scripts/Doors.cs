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

        if (dot > 0)
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