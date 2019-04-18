﻿using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

public class UIManager : MonoBehaviour
{

    [SerializeField]
    private GameObject primaryButton;
    
    [SerializeField]
    private GameObject throwButton;
    [SerializeField]
    private GameObject primaryBaitButton;
    [SerializeField]
    private GameObject attractButton;
    [SerializeField]
    private GameObject repelButton;
    [SerializeField]
    private GameObject pepperButton;

    //private Text primaryButtonText;
    //private Text throwButtonText;

    [SerializeField]
    Texture2D PickUpUnPushTex;
    [SerializeField]
    Texture2D PickUpPushTex;
    [SerializeField]
    Texture2D throwUnPushTex;
    [SerializeField]
    Texture2D throwPushTex;
    [SerializeField]
    Texture2D whistleUnPushTex;
    [SerializeField]
    Texture2D whistlePushTex;
   
    private Text attractText;
    private Text repelText;
    private Text pepperText;

    BaitSystem bait;
    BaitTypes currentType;

    [SerializeField]
    private double magnitude = 1.5;
    private bool throwToggle;
    private bool baitToggle;
    private const string WHISTLE = "WHISTLE";
    private const string PICKUP = "PICK UP";
    private const string NONE = "   ";
    private string currentPrimaryState = "WHISTLE";
    private bool duckRecalled;

    // Start is called before the first frame update
    void Start()
    {
        duckRecalled = false;
        currentType = BaitTypes.INVALID;
        //primaryButtonText = primaryButton.GetComponentInChildren<Text>();
        //throwButtonText = throwButton.GetComponentInChildren<Text>();
        attractText = attractButton.GetComponentInChildren<Text>();
        repelText = repelButton.GetComponentInChildren<Text>();
        pepperText = pepperButton.GetComponentInChildren<Text>();

        
        //pickUpUnpushed.texture = PickUpUnPushTex;
        //pickUpPushed.texture = PickUpPushTex;
        //throwUnPushed.texture = throwUnPushTex;
        //throwPushed.texture = throwPushTex;
        //whistleUnPushed.texture = whistleUnPushTex;
        //whistlePushed.texture = whistlePushTex;

        throwToggle = false;
        baitToggle = false;
        bait = GameManager.Instance.getPlayerTrans().GetComponentInChildren<BaitSystem>();
    }

    // Update is called once per frame
    void Update()
    { 
        Transform duck = GameManager.Instance.getduckTrans();
        Transform player = GameManager.Instance.getPlayerTrans();

        //Graphical stuff
        if (throwToggle)
        {
            RawImage tex = throwButton.GetComponent<RawImage>();
            //tex = throwPushed;
            tex.texture = throwPushTex;
            //throwButtonText.color = Color.black;   
        }

        else
        {
            RawImage tex = throwButton.GetComponent<RawImage>();
            //tex = throwUnPushed;
            tex.texture = throwUnPushTex;
            //throwButton.GetComponent<RawImage>().color = Color.black;
            //throwButtonText.color = Color.white;
        }

        if (baitToggle)
        {
            primaryBaitButton.GetComponent<RawImage>().color = Color.white;
            attractButton.SetActive(true);

            int num = bait.GetBaitAmount(BaitTypes.ATTRACT);

            attractText.text = num.ToString();

            num = bait.GetBaitAmount(BaitTypes.REPEL);

            repelButton.SetActive(true);
            repelText.text = num.ToString();

            num = bait.GetBaitAmount(BaitTypes.PEPPER);

            pepperButton.SetActive(true);
            pepperText.text = num.ToString();
        }

        else
        {
            //primaryBaitButton.GetComponent<RawImage>().color = Color.black;
            attractButton.SetActive(false);
            repelButton.SetActive(false);
            pepperButton.SetActive(false);
        }

        if (currentType == BaitTypes.ATTRACT)
        {
            attractButton.GetComponent<RawImage>().color = Color.green;
            
            repelButton.GetComponent<RawImage>().color = Color.white;
            
            pepperButton.GetComponent<RawImage>().color = Color.white;
        }

        if (currentType == BaitTypes.REPEL)
        {
            attractButton.GetComponent<RawImage>().color = Color.white;

            repelButton.GetComponent<RawImage>().color = Color.green;

            pepperButton.GetComponent<RawImage>().color = Color.white;
        }

        if (currentType == BaitTypes.PEPPER)
        {
            attractButton.GetComponent<RawImage>().color = Color.white;

            repelButton.GetComponent<RawImage>().color = Color.white;

            pepperButton.GetComponent<RawImage>().color = Color.green;
        }

        if (currentType == BaitTypes.INVALID)
        {
            attractButton.GetComponent<RawImage>().color = Color.white;
            attractText.color = Color.white;

            repelButton.GetComponent<RawImage>().color = Color.white;
            repelText.color = Color.white;

            pepperButton.GetComponent<RawImage>().color = Color.white;
            pepperText.color = Color.white;
        }


        //Determine whether to whistle or hold - Do the calculation here to avoid merge conflict
        //Also check for throw
        if (!GameManager.Instance.checkIsHoldingDuck())
        {
            if ((duck.position - player.position).magnitude < magnitude && !GameManager.Instance.checkIsHoldingDuck())
            {
                //reset
                duckRecalled = false;
                RawImage tex = primaryButton.GetComponent<RawImage>();
                tex.texture = PickUpUnPushTex;
                currentPrimaryState = PICKUP;
            }

            else
            {
                if (!duckRecalled)
                {
                    RawImage tex = primaryButton.GetComponent<RawImage>();
                    tex.texture = whistleUnPushTex;
                    currentPrimaryState = WHISTLE;
                }
            }

            throwButton.SetActive(false);
        }

        else
            throwButton.SetActive(true);
       

        InputManager manager = GameManager.Instance.GetComponent<InputManager>();

        //If throw enabled tap on tile to throw
        if (throwToggle)
        {
            if (manager != null)
            {
                List<RaycastHit> hitList = manager.GetTapHits();

                foreach (RaycastHit hit in hitList)
                {
                    if (hit.collider != null)
                    {
                        if (hit.collider.gameObject.name == "ground(Clone)" || hit.collider.gameObject.name == "water(Clone)")
                        {
                            //print("valid throw target");
                            GameManager.Instance.enableThrowDuck(hit);
                            throwToggle = false;
                            RawImage tex = primaryButton.GetComponent<RawImage>();
                            //tex = whistleUnPushed;
                            tex.texture = whistleUnPushTex;
                        }
                    }

                }
            }
            return;
        }
        //Whistle Control
        if (!throwToggle && !baitToggle)
        {
            //path find call
            if (manager != null)
            {
                List<RaycastHit> hitList = manager.GetTapHits();

                foreach (RaycastHit hit in hitList)
                {
                    if (hit.collider != null)
                    {
                        if (hit.collider.gameObject.name == "ground(Clone)")
                        {
                            Vector3 pos = hit.collider.gameObject.transform.position;
                            GameManager.Instance.movePlayerTo(pos);
                            RawImage tex = primaryButton.GetComponent<RawImage>();
                            //tex = whistlePushed;
                            tex.texture = whistlePushTex;
                        }
                    }
                }
            }
            return;
        }

        //Bait Logic Input
        if (baitToggle)
        {
            if (manager != null)
            {
                List<RaycastHit> hitList = manager.GetTapHits();

                foreach (RaycastHit hit in hitList)
                {
                    if (hit.collider != null)
                    {
                        if (hit.collider.gameObject.name == "ground(Clone)" || hit.collider.gameObject.name == "water(clone)")
                        {
                            Vector3 pos = hit.collider.gameObject.transform.position;
                            
                            if (currentType == BaitTypes.ATTRACT)
                            {
                                bait.spawnBait(pos, BaitTypes.ATTRACT);
                            }

                            if (currentType == BaitTypes.REPEL)
                            {
                                bait.spawnBait(pos, BaitTypes.REPEL);
                            }

                            if (currentType == BaitTypes.PEPPER)
                            {
                                bait.spawnBait(pos, BaitTypes.PEPPER);
                            }

                        }
                    }

                }
            }
            return;
        }



    }

    public void ToggleThrow()
    {
        throwToggle = !throwToggle;
    }

    //To Pick up or Whistle for Duck, this will deduce that
    public void PrimaryButtonCall()
    {
        if (currentPrimaryState == PICKUP)
        {
            GameManager.Instance.pickUpDuck();
            RawImage tex = primaryButton.GetComponent<RawImage>();
            //tex = pickUpPushed;
            tex.texture = PickUpPushTex;
        }

        if (currentPrimaryState == WHISTLE)
        {
            RawImage tex = primaryButton.GetComponent<RawImage>();
            duckRecalled = true;
            tex.texture = whistlePushTex;
            GameManager.Instance.duckRecall();
        }

    }

    public void ToggleBait()
    {
        baitToggle = !baitToggle;
        currentType = BaitTypes.INVALID;
    }

    public void SetBaitType(string type)
    {
        if (type == "Attract")
            currentType = BaitTypes.ATTRACT;

        if (type == "Repel")
            currentType = BaitTypes.REPEL;

        if (type == "Pepper")
            currentType = BaitTypes.PEPPER;

        if (type == "Invalid")
            currentType = BaitTypes.INVALID;
    }

}
