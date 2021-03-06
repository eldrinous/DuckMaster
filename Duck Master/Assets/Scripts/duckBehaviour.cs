﻿using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class duckBehaviour : MonoBehaviour
{
	public traversibleGround traverseData;

	//all conditions
	public bool canFollow;
	public bool isBeingHeld;
	bool returnMoving;
	bool run;
	public bool inAir;

	//follow data
	public bool startFollowing; //check to start following
	public float followThreshold; //the range to start following
	public float followVelocity; // velocity to follow
	public float targetRadius; //circle size of the target point, so not a direct movement to target
	public float toPointDistance; // the distance to then update new target
	Queue<Vector3> positionListData; //list of the targets
	public float updatePositionTime; //timer to create new target point path
	float updateTimeCount = 0;
	int positionCount = 0;
	Vector3 targetPoint;

	//pathfindin data
	public List<Vector3> tilePath;
	int tilePathIndex;
	[SerializeField] float pathApproachValue;
	[SerializeField] float pathVelocity;

	//hold data
	public float duckHeightAtHold;

	//throw data
	Vector3 startingPos;
	public Vector3 targetPos;
	[SerializeField] float startingVelocity;
	[SerializeField] float gravity;
	float maxAirTime;
	float currentAirTime;
	Vector3 initialVelocity;

	//run data
	[SerializeField] float fleeRange;
	int runCheckPerFrame = 10;
	int frameCount = 0;
	Vector3 runTar;
	float runToApproach = .3f;
	[SerializeField]float runVelocity;

	//Transform
	Transform duckTransform;
    public Transform playerTransform;
    // Start is called before the first frame update
    void Start()
    {
		inAir = false;
		returnMoving = false;
		tilePath = new List<Vector3>();
		returnMoving = false;
		canFollow = true;
		run = false;
        positionListData = new Queue<Vector3>();
        duckTransform = gameObject.transform;
    }

	private void Update()
	{
		//every or so frame check if duck is near unfreindlies
		if(frameCount > runCheckPerFrame && !run)
		{
			runTar = GameManager.Instance.checkToRun(fleeRange);
			frameCount = 0;

			if (runTar != Vector3.zero)
			{
				Debug.Log("RUN");
				run = true;
				canFollow = false;
				positionListData.Clear(); //clear all follow positions
				returnMoving = false;
				isBeingHeld = false;

				if(inAir)
				{
					inAir = false;
					runTar = playerTransform.position;
				}

			}
			else
			{
				run = false;
			}
		}
		frameCount++;

	}

	void FixedUpdate()
	{
		
		if (run) //run away ducko! The unfriendlies
		{
			Vector3 dir = (runTar - duckTransform.position);
			if (dir.magnitude < runToApproach)
			{
				run = false;
			}
			else
			{
				duckTransform.position += dir.normalized * runVelocity;
			}
		}
		else if (inAir)
		{
			currentAirTime += Time.deltaTime;
			if (currentAirTime < maxAirTime)
			{
				float xPos = startingPos.x + (initialVelocity.x * currentAirTime);
				float yPos = startingPos.y + (initialVelocity.y * currentAirTime) - ((gravity * currentAirTime * currentAirTime) / 2);
				float zPos = startingPos.z + (initialVelocity.z * currentAirTime);
				duckTransform.position = new Vector3(xPos, yPos, zPos);
			}
			else
			{
				inAir = false;
				//check if landed on geyser
				Vector3 target = GameManager.Instance.checkGeyser(targetPos, startingPos);
				if (target != Vector3.zero)
				{
					throwDuck(target);
				}
			}
		}
		else
		{
			int tilePathCount = tilePath.Count;
			if (canFollow) //follow
			{
				returnMoving = false;
				if (positionListData.Count == 0)
				{
					addnewPos();
				}
				followPlayer();
			}
			else if (returnMoving && tilePathCount != 0) //recall
			{
				movePaths();
			}

			if (isBeingHeld)
			{
				duckTransform.position = playerTransform.position + new Vector3(0, duckHeightAtHold, 0);
			}
		}
	}

	//move through the given path
	void movePaths()
	{
		Vector3 direction = (tilePath[tilePathIndex] - duckTransform.position);
		
		duckTransform.position += direction.normalized * pathVelocity;

		//approaches the next tile, update new target tile to move to
		if (direction.magnitude < pathApproachValue)
		{
			tilePathIndex--;
		}

		//updateTimer for follow, this way it will move towards player from pathfinding
		//then will have an already follow path to follow once follow takes over
		updateTimer();

		if (tilePathIndex < 0)
		{
			tilePath.Clear();
			canFollow = true;
			returnMoving = false;

			//begin following
			targetPoint = positionListData.Dequeue();
		}
	}

	void followPlayer()
    {
        //check if out of threshhold
        if((duckTransform.position - playerTransform.position).magnitude > followThreshold && startFollowing == false)
        {
            startFollowing = true;
            addnewPos();
   
        }
        else if((duckTransform.position - playerTransform.position).magnitude < followThreshold)
        {
            //reset data
            targetPoint = Vector3.zero;
            startFollowing = false;
            positionListData.Clear();
            updateTimeCount = 0;
            positionCount = 0;
        }

        if(startFollowing)
        {
            //check if the target is null, add new target
            if(targetPoint == Vector3.zero)
            {
                //if there are none in the list, create new one
               if(positionCount == 0)
               {
                    addnewPos();
               }
               targetPoint = positionListData.Dequeue();
               positionCount--;
            }

            //find direction and follow
            Vector3 dir = targetPoint - duckTransform.position;
            duckTransform.position += dir.normalized * followVelocity;        

            //check if approaching distance
            if(dir.magnitude < toPointDistance)
            {
                targetPoint = Vector3.zero;
                positionCount--;
            }

			updateTimer();
        }
    }


	//update timer to create a follow path
	void updateTimer()
	{
		updateTimeCount += Time.deltaTime;
		if (updateTimeCount > updatePositionTime)
		{
			addnewPos();
			updateTimeCount = 0;
		}
	}

	//find new target position in the follow path
    void addnewPos()
    {
        Vector3 newPos = new Vector3(playerTransform.position.x, 0, playerTransform.position.z);
        newPos += new Vector3(Random.Range(-targetRadius * 100, targetRadius * 100) / 100, 1, Random.Range(-targetRadius * 100, targetRadius * 100) / 100);
        positionListData.Enqueue(newPos);
        positionCount++;
    }

	//send in the new path to be read and activate return
	public void applyNewPath(List<Vector3> newPath)
	{
		returnMoving = true;
		tilePath = newPath;
		tilePathIndex = tilePath.Count - 1;
	}

	public bool isRecallable()
	{
		if(!canFollow && !returnMoving && !isBeingHeld && !inAir)
		{
			return true;
		}
		return false;
	}

	public void pickUpDuck()
	{
		canFollow = false;
		isBeingHeld = true;
		positionListData.Clear();
		//place duck ontop of player 
	}

	public void throwDuck(Vector3 target)
	{
		isBeingHeld = false;
		canFollow = false;
		inAir = true;

		startingPos = duckTransform.position;
		targetPos = target;

		Vector3 dir = new Vector3(targetPos.x - startingPos.x, 0, targetPos.z - startingPos.z);

		float distance = dir.magnitude;
		float heightDiff = targetPos.y - startingPos.y; //difference in height between two points
		float theta = Mathf.Atan((Mathf.Pow(startingVelocity, 2) + Mathf.Sqrt(Mathf.Pow(startingVelocity, 4) + gravity*(gravity*distance*distance + (2*heightDiff*Mathf.Pow(startingVelocity,2))))) / (gravity * distance));

		Debug.Log(theta * Mathf.Rad2Deg);
		heightDiff = startingPos.y - targetPos.y - 1; //initial height compared to the ground 0, which is tile position + 1
		maxAirTime = (startingVelocity * Mathf.Sin(theta) + Mathf.Sqrt(Mathf.Pow(startingVelocity*Mathf.Sin(theta),2) + 2*gravity*heightDiff))/gravity;
		currentAirTime = 0;

		dir = dir.normalized * Mathf.Cos(theta);
		initialVelocity = new Vector3(dir.x, Mathf.Sin(theta), dir.z) * startingVelocity;
		
	}

}
