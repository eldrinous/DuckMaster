﻿public class Connection
{
	public const byte MAX_COST = 255;

	public Connection()
	{
		mFromTile = null;
		mToTile = null;
        mDuckCost = 255;
        mMasterCost = 255;

    }

	public Connection(DuckTile fromTile, DuckTile toTile, byte duckCost = MAX_COST, byte masterCost = MAX_COST)
	{
		mFromTile = fromTile;
		mToTile = toTile;
        mDuckCost = duckCost;
        mMasterCost = masterCost;
	}

    public DuckTile mFromTile { get; set; }
    public DuckTile mToTile { get; set; }
    public byte mDuckCost { get; set; }
    public byte mMasterCost { get; set; }
}