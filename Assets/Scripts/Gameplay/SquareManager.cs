using System.Collections.Generic;
using System.Linq;
using System;
using Unity.VisualScripting;
using UnityEngine;
using System.Xml.Serialization;
using UnityEngine.UIElements;

/// <summary>
/// A manager to handle the top level interaction with all of the levels squares.
/// </summary>
public class SquareManager : MonoBehaviour
{
    /// <summary>
    /// The player object's player controller.
    /// </summary>
    public PlayerController player;

    /// <summary>
    /// The level object to build and manage.
    /// </summary>
    public Level level;

    /// <summary>
    /// The scriptable object containing most of the game's prefabs
    /// </summary>
    public Prefabs prefabs;

    /// <summary>
    /// Tells the square manager how to convert the level's data into gameObjects
    /// </summary>
    public TilePrefabManager tilePrefabManager;

    /// <summary>
    /// A dictionary to find the square object at any given position
    /// </summary>
    Dictionary<Vector2Int, Square> squares;

    /// <summary>
    /// The player's position
    /// </summary>
    public Vector2Int PlayerPos;

    bool isPlayerTurn;

    /// <summary>
    /// The valid moves a knight can make
    /// </summary>
    Vector2Int[] KnightMoves =
    {
        new Vector2Int(1,2),
        new Vector2Int(2,1),
        new Vector2Int(1,-2),
        new Vector2Int(2,-1),
        new Vector2Int(-1,2),
        new Vector2Int(-2,1),
        new Vector2Int(-1,-2),
        new Vector2Int(-2,-1)
    };

    // Start is called once before the first execution of Update after the MonoBehaviour is created
    void Start()
    {
        squares = new Dictionary<Vector2Int, Square>();

        PlayerPos = level.startPos;

        player.transform.position = GridToWorldPos(PlayerPos);

        BuildLevel();

        foreach (Square square in squares.Values)
        {
            square.OnLevelStart();
        }

        OnPlayerTurnStart();
    }

    void Update()
    {
        // Handles player movement
        if (Input.GetMouseButtonDown(0) && isPlayerTurn)
        {
            Vector2Int mousePos = GetMouseGridPos();
            if (GetValidMoves().Contains(mousePos))
            {
                // Perform actions related to leaving the current square
                squares[PlayerPos].OnPlayerLeave();

                // Moves the player
                player.MoveTo(mousePos, AnimationController.MovementType.Jump);

                // Informs the squares that the player has moved
                OnPlayerMove();

                // Handles player land once game-blocking animations have finished
                ActionQueue.QueueAction(OnPlayerLand);
            }
        }

        // Process the action queue
        ActionQueue.Update();
    }


    /// <summary>
    /// The actions to be performed at the start of the player's turn.
    /// </summary>
    void OnPlayerTurnStart()
    {
        isPlayerTurn = true;

        foreach (Square square in squares.Values)
        {
            square.OnPlayerTurnStart();
        }

        // Add square highlights to valid moves
        HighlightSquares(GetValidMoves());
    }

    /// <summary>
    /// The actions to be performed once the player has input their move
    /// </summary>
    void OnPlayerMove()
    {
        // Remove all square highlights
        HighlightSquares(new());

        isPlayerTurn = false;
        foreach (Square square in squares.Values)
        {
            square.OnPlayerMove();
        }
    }

    /// <summary>
    /// The actions to be performed once the player lands on its new tile.
    /// </summary>
    public void OnPlayerLand()
    {
        squares[PlayerPos].OnPlayerLand();

        // Once the horse has landed and game-blocking animations have finished, initiate the level's turn.
        ActionQueue.QueueAction(OnLevelTurn);
    }

    /// <summary>
    /// The actions to be performed on the level's turn.
    /// </summary>
    void OnLevelTurn()
    {
        // Does all of the square's turns.
        foreach (Square square in squares.Values)
        {
            square.OnLevelTurn();
        }

        // Triggers the start of the players turn once all game-blocking animations have finished
        ActionQueue.QueueAction(OnPlayerTurnStart);
    }


    /// <summary>
    /// Instantiates all the square prefabs in the level and sets up their initial conditions
    /// </summary>
    void BuildLevel()
    {
        level.ValidateLevel();

        GameObject prefab;
        Vector2Int pos;
        int initialState;
        int graphicsVariant;

        GameObject currentSquareObject;
        Square currentSquare;

        // Loops over all the tiles in the level object received
        foreach (TilePositionPair tilePosPair in level.tiles)
        {
            //Gets the prefab for the tile's type from the prefab manager
            prefab = tilePrefabManager.GetPrefab(tilePosPair.tile.type);
            pos = tilePosPair.position;

            //Reads the initial state and graphics variant for the tile
            initialState = tilePosPair.tile.initialState;
            graphicsVariant = tilePosPair.tile.graphicsVariant;

            // Creates an instance of the prefab
            currentSquareObject = Instantiate(prefab, GridToWorldPos(pos), Quaternion.identity);

            // Names the square object
            currentSquareObject.gameObject.name = $"Square ({pos[0]}, {pos[1]})";

            // Gets the square component from the prefab and adds it to the list of all squares
            currentSquare = currentSquareObject.GetComponent<Square>();
            
            // Sets properties of the square
            currentSquare.PlayerController = player;
            currentSquare.Position = tilePosPair.position;
            currentSquare.validMoveIndicator = Instantiate(prefabs.validMoveIndicator, currentSquareObject.transform);
            currentSquare.validMoveIndicator.gameObject.SetActive(false);

            // Adds the square to the squares array
            squares.Add(pos, currentSquare);

            // Sets up the square's initial state
            if (currentSquare.IsMultiState)
            {
                currentSquare.State = initialState;
            }
            // Sets up the square's graphics variant
            currentSquare.GraphicsVariant = graphicsVariant;
        }

        // Does some custom error handling to make sure all the links are set up properly
        ValidateLinks();

        // loops over all the tiles in the level
        foreach (TilePositionPair tilePositionPair in level.tiles)
        {
            // Finds the square corresponding to that tile
            currentSquare = squares[tilePositionPair.position];

            // If it's linkable loop over all its links
            if (currentSquare.IsLinkable)
            {
                // Creates a list for stuff to be added to. 
                currentSquare.Links = new List<Square>();
                foreach (Vector2Int link in tilePositionPair.tile.links)
                {
                    // Adds the square corresponding to the linked position to the square's list of links.
                    currentSquare.Links.Add(squares[link]);
                }
            }
        }
    }

    /// <summary>
    /// Checks that all of the links are set up properly
    /// </summary>
    /// <exception cref="Exception"></exception>
    public void ValidateLinks()
    {
        Vector2Int currentPos;

        // Loops over all of the tiles
        foreach (TilePositionPair tilePosPair in level.tiles)
        {
            currentPos = tilePosPair.position;
            // Checks if the tile is trying to link to itself (bad)
            if (tilePosPair.tile.links.Contains(currentPos))
            {
                Debug.LogWarning("Trying to link a tile to itself at position " + currentPos.ToString());
            }
            // Checks if there are links registered to an unlinkable tile
            if (tilePosPair.tile.links.Count != 0 && !squares[currentPos].IsLinkable)
            {
                Debug.LogWarning("Trying to link an unlinkable tile at position " + currentPos.ToString());
            }
            // Checks if the link goes to a location with no tile created
            foreach (Vector2Int link in tilePosPair.tile.links)
            {
                if (!squares.Keys.Contains(link))
                {
                    throw new Exception("Trying to create a link to a tile that does not exist from " + currentPos.ToString() + " to " + link.ToString());
                }
            }
        }
    }

    /// <summary>
    /// Converts the grid position to a world position
    /// </summary>
    /// <param name="gridPos">The position in grid coordinates</param>
    /// <returns>The position in world coordinates</returns>
    public static Vector3 GridToWorldPos(Vector2Int gridPos)
    {
        // To do: do something more clever and flexible here
        return new Vector3(gridPos.x, 0, gridPos.y);
    }

    /// <summary>
    /// Converts the world position to a grid position
    /// </summary>
    /// <param name="worldPos">The position in global world coordinates</param>
    /// <returns>The position in grid coordinates</returns>
    public static Vector2Int WorldToGridPos(Vector3 worldPos)
    {
        // To do: see GridToWorldPos()...
        return new Vector2Int(Mathf.RoundToInt(worldPos.x), Mathf.RoundToInt(worldPos.z));
    }

    /// <summary>
    /// Gets the grid position the mouse is currently hovering over.
    /// </summary>
    /// <returns> The world position of the mouse. </returns>
    public Vector3 GetMouseWorldPos()
    {
        // Casts a ray from the camera to mouse position
        Ray ray = Camera.main.ScreenPointToRay(Input.mousePosition);

        // create a plane at 0,0,0 whose normal points to +Y:
        // It is currently assumed that all tiles will exist on this plane
        Plane hPlane = new Plane(Vector3.up, Vector3.zero);

        // Plane.Raycast stores the distance from ray.origin to the hit point in this variable:
        float distance = 0;

        // if the ray hits the plane...
        if (hPlane.Raycast(ray, out distance))
        {
            // get the hit point:
            return ray.GetPoint(distance);
        }
        else
        {
            Debug.LogError("No world position found for mouse position.");
            return Vector3.zero;
        }
    }

    /// <summary>
    /// Gets the world position the mouse is hovering over.
    /// </summary>
    /// <returns>The grid position of the mouse</returns>
    public Vector2Int GetMouseGridPos()
    {
        return WorldToGridPos(GetMouseWorldPos());
    }


    public List<Vector2Int> GetValidMoves()
    {
        List<Vector2Int> moves = new List<Vector2Int>();

        foreach (Vector2Int position in squares.Keys)
        {
            if (KnightMoves.Contains(position - PlayerPos) && squares[position].IsPassable)
            {
                moves.Add(position);
            }
        }

        return moves;
    }

    /// <summary>
    /// Highlight squares at a given set of Vector2Int coordinates
    /// </summary>
    /// <param name="coordinatesToHighlight">The set of coordinates to highlight</param>
    public void HighlightSquares(List<Vector2Int> coordinatesToHighlight)
    {

        foreach (var (coordinate, square) in squares)
        {
            square.IndicateMoveValidity(coordinatesToHighlight.Contains(coordinate));
        }
    }
}
