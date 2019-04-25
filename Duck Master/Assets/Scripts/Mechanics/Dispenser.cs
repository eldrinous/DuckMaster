﻿using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class Dispenser : LogicOutput
{
    [SerializeField] BaitTypes baitType;
    GameObject spawnedBait;

    public override void Activate(bool active)
    {
        if (active && spawnedBait == null)
            SpawnBait();
    }

    public void SpawnBait()
    {
        Vector3 position = transform.position;
        position = new Vector3(position.x, 0, position.z) + (transform.forward);
        spawnedBait = GameManager.Instance.GetBait().spawnDispenserBait(position, baitType);
    }
}