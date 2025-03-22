using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class Doors : MonoBehaviour
{

    public Animator door;
    // public GameObject lockOB;
    // public GameObject keyOB;


    public AudioSource openSound;
    public AudioSource closeSound;
    // public AudioSource lockedSound;
    // public AudioSource unlockedSound;

    private bool inReach;
    private bool doorisOpen;
    private bool doorisClosed;
    public bool locked;
    public bool unlocked;





    void OnTriggerEnter(Collider other)
    {
        if (other.gameObject.tag == "Reach" && doorisClosed)
        {
            inReach = true;
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

    void Start()
    {
        inReach = false;
        doorisClosed = true;
        doorisOpen = false;
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

        if (inReach && Input.GetButtonDown("Interact"))
        {
            locked = false;
            StartCoroutine(unlockDoor());
        }

        if (inReach && doorisClosed && unlocked && Input.GetButtonDown("Interact"))
        {
            door.SetBool("Open", true);
            door.SetBool("Closed", false);
            openSound.Play();
            doorisOpen = true;
            doorisClosed = false;
        }

        else if (inReach && doorisOpen && unlocked && Input.GetButtonDown("Interact"))
        {
            door.SetBool("Open", false);
            door.SetBool("Closed", true);
            closeSound.Play();
            doorisClosed = true;
            doorisOpen = false;
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