﻿using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class WaterWheel : MonoBehaviour, LogicInput
{
    bool active;

    // Start is called before the first frame update
    void Start()
    {
        active = false;
        GetComponent<Rigidbody>().constraints = RigidbodyConstraints.FreezePosition;
    }

    // Update is called once per frame
    void Update()
    {
        
    }

    private void OnTriggerEnter(Collider other)
    {
        if (other.gameObject.name == "water(Clone)")
        {
            Water water = other.gameObject.GetComponent<Water>();

            if (water.GetWaterDirection() != WaterDirections.NONE)
            {
                active = true;
                print("Water wheel activated");
            }
        }
    }

    private void OnTriggerExit(Collider other)
    {
        if (other.gameObject.name == "water(Clone)")
        {
            print("water wheel deactivated");
            active = false;
        }
    }

    public bool IsActive()
    {
        return active;
    }
}