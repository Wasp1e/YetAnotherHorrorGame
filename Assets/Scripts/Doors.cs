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
            door.SetBool("Open", true);
            door.SetBool("Closed", false);
            openSound.Play();
            doorisOpen = true;
            doorisClosed = false;
        }

        else if (inReach && !lockedlocked && doorisOpen && unlocked && Input.GetButtonDown("Interact"))
        {
            door.SetBool("Open", false);
            door.SetBool("Closed", true);
            closeSound.Play();
            doorisClosed = true;
            doorisOpen = false;
        }
        else if (inReach && lockedlocked && Input.GetButtonDown("Interact")) {
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




}