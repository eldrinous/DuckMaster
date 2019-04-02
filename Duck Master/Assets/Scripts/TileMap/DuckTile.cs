﻿public class DuckTile
{
    public enum ConnectionDirection
    {
        INVALID_DIRECTION = -1,
        UP,
        RIGHT,
        DOWN,
        LEFT,
    }
    private const byte MAX_CONNECTIONS = 4;
    Connection[] mConnections;

	public enum TileType
	{
		INVALID_TYPE = -1,
		UnpassableBoth,
		UnpasssableDuck,
		UnpassableMaster,
		PassableBoth,
	}
	public TileType mType { get; set; }

	public bool mBaitable { get; set; }
	public bool mHeightChange { get; set; }

	public int mHeight { get; set; }

	// Default Constructor
	public DuckTile()
	{
		mConnections = new Connection[MAX_CONNECTIONS];
		mType = TileType.INVALID_TYPE;
		mBaitable = false;
		mHeightChange = false;
		mHeight = -1;
	}

	// Sets to connections and type and baitable
	// If connections is invalid it is set to a new instead
	public DuckTile(Connection[] connections, TileType type, bool baitable, bool heightChange, int height)
	{
		if(connections.Length <= MAX_CONNECTIONS)
		{
			mConnections = connections;
		}
		else
		{
			mConnections = new Connection[MAX_CONNECTIONS];
		}
		mType = type;
		mBaitable = baitable;
		mHeight = height;
	}

    public DuckTile(TileType type, bool baitable, bool heightChange, int height)
    {
		mConnections = new Connection[MAX_CONNECTIONS];
		mType = type;
        mBaitable = baitable;
        mHeightChange = heightChange;
		mHeight = height;
    }

	// Set the specific direction connection
	// Returns false if failed
    public bool SetConnectionDirection(ConnectionDirection direction, Connection connection)
    {
		if(ConnectionDirection.INVALID_DIRECTION == direction)
		{
			return false;
		}

		mConnections[(int)direction] = connection;
		return true;
    }

	// Set the index connection
	// Returns if false if failed
    public bool SetConnectionIndex(int index, Connection connection)
    {
		if(index < -1 || index > 4)
		{
			return false;
		}

		mConnections[index] = connection;
		return true;
    }

	// Get the direction connection
	// Returns null if fails
    public Connection GetConnectionDirection(ConnectionDirection direction)
    {
		if (ConnectionDirection.INVALID_DIRECTION == direction)
		{
			return null;
		}

		return mConnections[(int)direction];
	}

	// Get index connection
	// Returns null if fails
    public Connection GetConnectionIndex(int index)
    {
		if (index < -1 || index > 4)
		{
			return null;
		}

		return mConnections[index];
	}

	// Get direction duck connection cost
	// Returns MAX_COST if fails
    public byte GetDuckCostDirection(ConnectionDirection direction)
    {
		if (ConnectionDirection.INVALID_DIRECTION == direction)
		{
			return Connection.MAX_COST;
		}

		return mConnections[(int)direction].mDuckCost;
	}

	// Get index duck connection cost
	// Returns MAX_COST if fails
	public byte GetDuckCostIndex(int index)
	{
		if (index < -1 || index > 4)
		{
			return Connection.MAX_COST;
		}

		return mConnections[index].mDuckCost;
	}

    // Get direction master connection cost
	// Returns MAX_COST if fails
    public byte GetMasterCostDirection(ConnectionDirection direction)
    {
		if (ConnectionDirection.INVALID_DIRECTION == direction)
		{
			return Connection.MAX_COST;
		}

		return mConnections[(int)direction].mMasterCost;
	}

	// Get index master connection cost
	// Returns MAX_COST if fails
	public byte GetMasterCostIndex(int index)
	{
		if (index < -1 || index > 4)
		{
			return Connection.MAX_COST;
		}

		return mConnections[index].mMasterCost;
	}

	public bool GetDuckPassable()
	{
		return mType != TileType.UnpasssableDuck || mType != TileType.UnpassableBoth;
	}

	public bool GetMasterPassable()
	{
		return mType != TileType.UnpassableMaster || mType != TileType.UnpassableBoth;
	}
}