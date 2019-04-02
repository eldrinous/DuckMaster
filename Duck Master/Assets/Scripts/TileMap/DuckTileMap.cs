﻿using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class DuckTileGrid
{
    List<List<DuckTile>> mGrid;

    public DuckTileGrid()
    {
        mGrid = new List<List<DuckTile>>();
    }

    public DuckTileGrid(int x, int y)
    {
        mGrid = new List<List<DuckTile>>();
        List<DuckTile> tempList;
        for (int j = 0; j < x; ++j)
        {
            tempList = new List<DuckTile>();
            for (int k = 0; k < x; ++k)
            {
                tempList.Add(new DuckTile());
                Debug.Log("X: " + k + " Y: " + j);
            }
            mGrid.Add(tempList);
        }
    }

    // Height change is for the fact that a tile might be walkable above but not walkable from below
    public DuckTileGrid(List<List<DuckTile.TileType>> typeGrid, List<List<bool>> baitableGrid, List<List<bool>> heightChangeGrid, int height)
    {
        mGrid = new List<List<DuckTile>>();
        List<DuckTile> tempList;
        for(int j = 0; j < typeGrid.Count; ++j)
        {
            tempList = new List<DuckTile>();
            for (int k = 0; k < typeGrid[j].Count; ++k)
            {
                // j is y, k is x
                tempList.Add(new DuckTile(typeGrid[j][k], baitableGrid[j][k], heightChangeGrid[j][k], height));
                //Debug.Log("X: " + k + " Y: " + j + "\nType: " + typeGrid[j][k] + "\nBaitable: " + baitableGrid[j][k] + "\nHeight Change: " + heightChangeGrid[j][k]);
            }
            mGrid.Add(tempList);
        }
    }

	//public List<DuckTile> GetRow(int y)
	//{
	//	if (y > -1 && y < mGrid.Count)
	//		return mGrid[y];
	//	return null;
	//}

    public DuckTile GetTile(int x, int y)
    {
		if ((mGrid.Count < y && mGrid[y].Count < x && x > -1 && y > -1) || (mGrid[y][x].mType != DuckTile.TileType.INVALID_TYPE))
			return mGrid[y][x];
        return null;
    }

	public List<List<DuckTile>> GetGrid()
	{
		return mGrid;
	}

	public int GetLength()
	{
		return mGrid.Count;
	}

	public int GetRowLength(int y)
	{
		if(y > -1 && y < mGrid.Count)
			return mGrid[y].Count;
		return -1;
	}

	public void AddTile(int y, DuckTile tile)
	{
		if(y > -1 && y < mGrid.Count)
		{
			mGrid[y].Add(tile);
		}
	}

	public void AddRow(List<DuckTile> duckTiles)
	{
		mGrid.Add(duckTiles);
	}
}

public class DuckTileMap
{
    public List<DuckTileGrid> mGridMap { get; set; }
	public DuckTileGrid mHeightMap { get; set; }

    public DuckTileMap()
    {
        mGridMap = new List<DuckTileGrid>();
		mHeightMap = new DuckTileGrid();
    }

    public DuckTileMap(int x, int y, int height)
    {
        mGridMap = new List<DuckTileGrid>();
        for(int i = 0; i < height; ++i)
        {
            mGridMap.Add(new DuckTileGrid(x, y));
        }
		mHeightMap = new DuckTileGrid();
		CreateHeightMap();
		CreateConnections();
    }

    public DuckTileMap(List<DuckTileGrid> gridMap)
    {
        mGridMap = gridMap;
		mHeightMap = new DuckTileGrid();
		CreateHeightMap();
		CreateConnections();
    }

    public DuckTile GetTile(int x, int y, int height)
    {
        return mGridMap[height].GetTile(x, y);
    }

    void CreateHeightMap()
    {
		DuckTile tempTile;
		for(int i = 0; i < mGridMap.Count; ++i)
		{
			DuckTileGrid grid = mGridMap[i];
			for (int j = 0; j < grid.GetLength(); ++j)
			{
				for(int k = 0; k < grid.GetRowLength(j); ++k)
				{
					if(j < mHeightMap.GetLength() && k < mHeightMap.GetRowLength(j) && (tempTile = grid.GetTile(k, j)) != null)
					{
						mHeightMap.GetGrid()[j][k] = tempTile;
					}
					else
					{
						if(j >= mHeightMap.GetLength())
						{
							mHeightMap.AddRow(new List<DuckTile>());
						}
						if(k >= mHeightMap.GetRowLength(j) && (tempTile = grid.GetTile(k, j)) != null)
						{
							mHeightMap.AddTile(j, tempTile);
						}
					}
				}
			}
		}

		Debug.Log(mHeightMap);
    }

	void CreateConnections()
	{
		DuckTile currentTile, rightTile, bottomTile;
		Connection rightConnection, bottomConnection;
		//bool rightHeightPassable = true, bottomHeightPassable;
		for (int j = 0; j < mHeightMap.GetLength() - 1; ++j)
		{
			for (int k = 0; k < mHeightMap.GetRowLength(j) - 1; ++k)
			{
				currentTile = mHeightMap.GetTile(k, j);
				rightTile = mHeightMap.GetTile(k + 1, j);
				bottomTile = mHeightMap.GetTile(k, j + 1);
				rightConnection = new Connection(currentTile, rightTile);
				bottomConnection = new Connection(currentTile, bottomTile);

				if (currentTile.mHeight == rightTile.mHeight || (currentTile.mHeight != rightTile.mHeight && currentTile.mHeightChange && rightTile.mHeightChange))
				{
					if (currentTile.GetDuckPassable() && rightTile.GetDuckPassable())
					{
						// right connection is duck passable
						rightConnection.mDuckCost = 1;
					}
					if (currentTile.GetMasterPassable() && rightTile.GetMasterPassable())
					{
						// right connection is master passable
						rightConnection.mMasterCost = 1;
					}
				}
				if (currentTile.mHeight == bottomTile.mHeight || (currentTile.mHeight != bottomTile.mHeight && currentTile.mHeightChange && bottomTile.mHeightChange))
				{
					if (currentTile.GetDuckPassable() && bottomTile.GetDuckPassable())
					{
						// bottom connection duck passable
						bottomConnection.mDuckCost = 1;
					}
					if (currentTile.GetMasterPassable() && bottomTile.GetMasterPassable())
					{
						// bottom connection master passable
						bottomConnection.mMasterCost = 1;
					}
				}
				currentTile.SetConnectionDirection(DuckTile.ConnectionDirection.RIGHT, rightConnection);
				currentTile.SetConnectionDirection(DuckTile.ConnectionDirection.DOWN, bottomConnection);
				rightTile.SetConnectionDirection(DuckTile.ConnectionDirection.LEFT, new Connection(rightConnection.mToTile, rightConnection.mFromTile, rightConnection.mDuckCost, rightConnection.mMasterCost));
				rightTile.SetConnectionDirection(DuckTile.ConnectionDirection.DOWN, new Connection(bottomConnection.mToTile, bottomConnection.mFromTile, bottomConnection.mDuckCost, bottomConnection.mMasterCost));
			}
		}
		Debug.Log("Connection");
	}
}