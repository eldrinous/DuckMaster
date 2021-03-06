﻿using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using System.IO;

public enum tileType
{
	NOTHING,
	Ground,
	Water,
	Tree,
	Geyser,
	GateUp,
	GateDown
}

public enum buttonType
{
	Gate1 = 5,
	Gate2 = 6
}

[CreateAssetMenu(fileName = "traversibleGround", order = 1)]
public class traversibleGround : ScriptableObject
{
	[SerializeField] List<string> traverseNames = new List<string> 
	{
	"NOTHING",
	"Ground",
	"Water",
	"Tree",
	"Geyser",
	"GateUp",
	"GateDown"
	};
	public List<bool> traversePossibilities;
}

public class tile
{
	//tile information
	public int index;
	public Vector2 index2;
	public Vector3 pos;
	public tileType tType;
	public buttonType typeButton;

	//pathfinding information
	public tile prevTile;
	public float costSofar;
	public float costWithHeurestics;
	public bool walkable = true;

	public	float getHeurestic()
	{
		return costWithHeurestics - costSofar;
	}
}

public class obstacleTilingSystem : MonoBehaviour
{
	//tileMapInfo
    float tileSize = 1;
    float mapScaleX; //size of the map(private)
    float mapScaleZ;
    Transform mapTransform;
    int tileCountX; //amount of tiles in across
    int tileCountZ; //amount of tiles in vertical
	public float sizeX; //size of the map (public)
	public float sizeZ;
	int amtOfTiles = 0;

    //min max for x and z
    float minX;
    float maxX;
    float minZ;
    float maxZ;

	List<tile> tileList;
	List<tileBehaviour> tileObjList;
	List<tile> buttonRelatedTiles;

    [SerializeField] Texture2D levelData;
    [SerializeField] TextAsset specialFileData;
	[SerializeField] List<bool> buttonAffectedTilesCheck;
    [SerializeField] List<GameObject> tileTypeObject;
    [SerializeField] List<Color32> colorToTile;
	[SerializeField] List<GameObject> specials;
	// Start is called before the first frame update
	//to create the index is i = (row * Colsize) + column where col is x
	//to find y = index / rowSize
	//to find x = i% rowSize
	void Start()
    {
        tileList = new List<tile>();
		tileObjList = new List<tileBehaviour>();
		buttonRelatedTiles = new List<tile> ();

        mapTransform = gameObject.transform;
		mapScaleX = sizeX;
		mapScaleZ = sizeZ;
        tileCountX = (int)(mapScaleX / tileSize);
        tileCountZ = (int)(mapScaleZ / tileSize);

        //find min max
        minX = -sizeX / 2;
        maxX = sizeX / 2;

        minZ = -sizeZ / 2;
        maxZ = sizeZ / 2;

        for (int row = 0; row < tileCountZ; row++) //z?
        {
			for (int col = 0; col < tileCountX; col++) //x?
			{
				tile newTile = new tile();
				int index = (row * tileCountX) + col;

				//do tile data generate
				newTile.index = index;
				newTile.index2 = new Vector2(col, row);
				float XPos = (col * tileSize) + tileSize / 2 - mapScaleX / 2;
				float ZPos = (row * tileSize) + tileSize / 2 - mapScaleZ / 2;
				newTile.pos = new Vector3(XPos, 0, ZPos);

				Color32 levelColor = levelData.GetPixel(col, row);
				int tileTypeIndex = getTileTypeFromColor(levelColor);
				newTile.tType = (tileType)tileTypeIndex;

				GameObject spawnTile = tileTypeObject[tileTypeIndex];
				GameObject tileObj = Instantiate(spawnTile, newTile.pos, transform.rotation);

				//add tile
				tileList.Add(newTile);
				amtOfTiles++;

				tileBehaviour tileB = tileObj.GetComponent<tileBehaviour>();
				if(tileB != null)
				{
					tileObjList.Add(tileB);
				}
				else
				{
					tileObjList.Add(null);
				}
				

				if (buttonAffectedTilesCheck[tileTypeIndex])
				{
					buttonRelatedTiles.Add(newTile);
					newTile.typeButton = (buttonType)tileTypeIndex;
				}
            }
        }

        //spawn specials. Source: http://saadkhawaja.com/read-text-file-line-line/
        string[] lineInFile = specialFileData.text.Split('\n');
        foreach(string line in lineInFile)
        {
			string[] lineData = line.Split(' ');
			int specialType = int.Parse(lineData[1]);
			int posX = int.Parse(lineData[3]);
			int posZ = int.Parse(lineData[5]);
			string type = lineData[6];

			tile atTile = tileList[(posZ * tileCountX) + posX];
			GameObject specialObj = Instantiate(specials[specialType], atTile.pos + new Vector3(0,1,0), transform.rotation);
		}
    }

	public List<Vector3> getTilePath(Vector3 from, Vector3 to, List<bool> travableTile, bool ignoreWalkable)
	{
		//Create list data to keep track of
		List<tile> openList = new List<tile>();
		HashSet<tile> closedList = new HashSet<tile>();
		List<Vector3> path = new List<Vector3>();
		int openListCount = 0;
		int nodesProcessed = 0; //processed is when it is thrown into the closed list

		//Allocate positions
		Vector3 startingPos = from;
		Vector3 targetPos = to;

		//Get the first node
		tile firstNode = tileList[getIndexOfTile(startingPos)];
		firstNode.costSofar = 0;
		firstNode.costWithHeurestics = (targetPos - startingPos).magnitude;

		//Add Node
		openList.Add(firstNode);
		openListCount++;

		//getTargetNode
		tile targetNode = tileList[getIndexOfTile(targetPos)];
		if (!travableTile[(int)targetNode.tType] || targetNode == firstNode || (!targetNode.walkable && !ignoreWalkable))
		{
			Debug.Log("same Node");
			return path;
		}

		//conditions for other stuff like limiting the range etc.
		bool loop = true;

		//condition when tile is found
		bool foundToTile = false;
		while(loop && openListCount > 0 && !foundToTile) //&& if node is not found, count should be fine but in case
		{
			tile curNode = openList[0];
			Vector3 curPos = curNode.pos;

			//to create the index is i = (row * Colsize) + column where col is x
			//maybe a check if the nodes processed is too much then stop or something
			if(targetNode == curNode)
			{
				Debug.Log("Found Tile");
				foundToTile = true;
			}
			else
			{
				if (!closedList.Contains(curNode))
				{
					//iterate through all adjacents
					for (int row = -1; row < 2; row++)
					{
						for (int col = -1; col < 2; col++)
						{
							if (col == 0 || row == 0)
							{
								//check tile information
								int adjIndex = ((row + (int)curNode.index2.y) * tileCountX) + (col + (int)curNode.index2.x);
                                
                                //making sure that the index is within bounds of the "2d" array 
                                if(adjIndex < tileCountX * tileCountZ && curNode.index2.y + row < tileCountZ && curNode.index2.y + row >= 0 
                                    && curNode.index2.x < tileCountX && curNode.index2.x >= 0)
                                {
                                    tile adjTile = tileList[adjIndex];
                                    tileType adjType = adjTile.tType;

									//if it cannot ignore walkable and the tile is not walkable, then it cannot travel to adj tile
                                    if (travableTile[(int)adjType] && !closedList.Contains(adjTile) && curNode != adjTile && !(!adjTile.walkable && !ignoreWalkable))
                                    {
                                        adjTile.costSofar = curNode.costSofar + (adjTile.pos - curPos).magnitude;
                                        adjTile.costWithHeurestics = adjTile.costSofar + (targetPos - adjTile.pos).magnitude;
                                        adjTile.prevTile = curNode;

                                        //place the node into a queue 
                                        bool placed = false;
                                        for (int i = 0; i < openListCount; i++)
                                        {
                                            if (openList[i].costWithHeurestics > adjTile.costWithHeurestics)
                                            {
                                                placed = true;
                                                openList.Insert(i, adjTile);
                                                openListCount++;
                                                i = openListCount;
                                            }
                                        }

                                        if (placed == false)
                                        {
                                            openList.Insert(openListCount, adjTile);
                                            openListCount++;
                                        }
                                    }
                                }
							}
						}
					}
					nodesProcessed++;
					closedList.Add(curNode);
				}
			}
            openList.RemoveAt(0);
			openListCount--;
		}

        if(foundToTile)
        {
            path.Add(targetNode.pos + new Vector3(0, 1, 0));
            bool finishPath = false;
            tile curTile = targetNode;

            while (!finishPath)
            {
                curTile = curTile.prevTile;
                path.Add(curTile.pos + new Vector3(0,1,0));
                if (curTile == firstNode)
                {
                    finishPath = true;
                }
            }
        }

		return path;
	}
	
	int getTileTypeFromColor(Color32 type)
	{
		bool foundTileType = false;
		int colorIndex = 0;
		while(!foundTileType)
		{
			if(colorToTile[colorIndex].r == type.r && colorToTile[colorIndex].g == type.g && colorToTile[colorIndex].b == type.b)
			{
				return colorIndex;
			}
			colorIndex++;
		}

		return -1; //color failed
	}

   
	public bool checkToTile(RaycastHit hit, GameObject highlighter)
	{
		float Xpoint = hit.point.x - transform.position.x;
		float Zpoint = hit.point.z - transform.position.z;

		Xpoint = Mathf.Floor((Xpoint + (mapScaleX / 2)) / tileSize);
		Zpoint = Mathf.Floor((Zpoint + (mapScaleZ / 2)) / tileSize);
		int index = (int)(Xpoint + (Zpoint * tileCountX));

		if (index < amtOfTiles)
		{
			highlighter.transform.position = tileList[index].pos + transform.position + new Vector3(0,.3f,0);
			return true;
		}

        return false;
	}

    //returns the tile being hit
    public tile getToTile(RaycastHit hit)
    {
        float Xpoint = hit.point.x - transform.position.x;
        float Zpoint = hit.point.z - transform.position.z;

        Xpoint = Mathf.Floor((Xpoint + (mapScaleX / 2)) / tileSize);
        Zpoint = Mathf.Floor((Zpoint + (mapScaleZ / 2)) / tileSize);
        int index = (int)(Xpoint + (Zpoint * tileCountX));

        if (index < amtOfTiles)
        {			
            return tileList[index];
        }

        return null;
    }


	//returns the tile being hit
	public tile getToTileByPosition(Vector3 pos)
	{
		float Xpoint = pos.x - transform.position.x;
		float Zpoint = pos.z - transform.position.z;

		Xpoint = Mathf.Floor((Xpoint + (mapScaleX / 2)) / tileSize);
		Zpoint = Mathf.Floor((Zpoint + (mapScaleZ / 2)) / tileSize);
		int index = (int)(Xpoint + (Zpoint * tileCountX));

		if (index < amtOfTiles)
		{
			return tileList[index];
		}

		return null;
	}

	//get the index based on position
	public int getIndexOfTile(Vector3 position)
	{
		float Xpoint = Mathf.Floor((position.x + (mapScaleX / 2)) / tileSize);
		float Zpoint = Mathf.Floor((position.z + (mapScaleZ / 2)) / tileSize);
		return (int)(Xpoint + (Zpoint * tileCountX));
	}

	public tile getTilebyIndex(int x, int z)
	{
		return tileList[x + (z * tileCountX)];
	}

	public bool isInBoundsByAxis(ref Vector3 position, List<bool> travableTiles)
	{
        if(position.x < minX)
        {
            return false;
        }
        if(position.x > maxX)
        {
            return false;
        }
        if(position.z < minZ)
        {
            return false;
        }
        if(position.z > maxZ)
        {
            return false;
        }

        //checks if the type of the tile at index is true from the object's travableTile list
        //it finds the tile at the position, gets the tType and cast it as int to see if true in list
        tile atTile = getToTileByPosition(position);
        if (travableTiles[(int)atTile.tType])
        {
            position = atTile.pos;
            return true;
        }
        else
        {
            return false;
        }
	}

	public void changeAllFromButtons()
	{
		foreach(tile node in buttonRelatedTiles)
		{
			//hard coded
			if(node.typeButton == buttonType.Gate1)
			{
				node.typeButton = buttonType.Gate2;
			}
			else if(node.typeButton == buttonType.Gate2)
			{
				node.typeButton = buttonType.Gate1;
			}

			node.tType = (tileType)node.typeButton;

			tileObjList[node.index].changedByButton(node.tType);
		}
	}

	//is called whenever the unfriendlies moves positions
	public List<tile> turnWalkable(List<tile> originalTiles, Vector3 newPos, int range = 0)
	{
		foreach(tile node in originalTiles)
		{
			node.walkable = true;
		}

		List<tile> unWalkables = new List<tile>();

		tile atTile = getToTileByPosition(newPos);

		for(int row = -range; row < range+1; row++)
		{
			for(int col = -range; col < range+1; col++)
			{
				int adjIndex = ((row + (int)atTile.index2.y)* tileCountX) + (col + (int)atTile.index2.x);
				tile adjTile = tileList[adjIndex];
				adjTile.walkable = false;
				unWalkables.Add(adjTile);
			}
		}
		return unWalkables;
	}
}
