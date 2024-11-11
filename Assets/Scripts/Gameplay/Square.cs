using System.Collections.Generic;
using UnityEngine;
using UnityEngine.EventSystems;

/// <summary>
/// The base class for all squares.
/// </summary>
public abstract class Square : MonoBehaviour
{
    /// <summary>
    /// The type of tile.
    /// </summary>
    public abstract TileType Type{get;}
    
    /// <summary>
    /// Whether this type of square can have links to other tiles.
    /// </summary>
    public abstract bool IsLinkable{get;}
    
    /// <summary>
    /// The list of all of the squares that this square is linked to, if not implemented (e.g. for floor), it will raise a warning.
    /// </summary>
    public virtual List<Square> Links
    {
        get 
        {
            Debug.LogWarning("Links not implemented for tile type "+Type.ToString());
            return new List<Square>(); 
        }
        set
        {
            Debug.LogWarning("Links not implemented for tile type "+Type.ToString());   
        }
    }

    /// <summary>
    /// Whether this type of square can have multiple states.
    /// </summary>
    public abstract bool IsMultiState{get;}

    /// <summary>
    /// The state of this square, if not implemented (e.g. for floor), it will raise a warning.
    /// </summary>
    public virtual int State
    {
        get
        {
            Debug.LogWarning("Multi-State not implemented for tile type "+Type.ToString());
            return 0; 
        }
        set
        {
            Debug.LogWarning("Multi-State not implemented for tile type "+Type.ToString());
        }
    }

    /// <summary>
    /// The graphics variant to use for this square.
    /// </summary>
    public abstract int GraphicsVariant{get;set;}
    
    /// <summary>
    /// Whether the tile is currently passable.
    /// </summary>
    public abstract bool IsPassable{get; protected set;}
    
    
}
