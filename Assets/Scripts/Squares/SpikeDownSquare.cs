using UnityEngine;
using System.Collections.Generic;
using Unity.VisualScripting;

/// <summary>
/// A Spike square that is impassable when active. It is on a cycle,
/// decided by the state for the frequency, poking every
/// {state} turns. Starts Down.
/// </summary>
public class SpikeDownSquare : Square
{
    public override TileType Type =>  TileType.SpikeDown;

    /// <summary>
    /// The animator on the graphics gameObject representing the retracting spikes for this tile
    /// </summary>
    [SerializeField]
    private AnimationController SpikeGraphics;

    /// <summary>
    /// A transform indicating the position the spikes should protrude to
    /// </summary>
    [SerializeField]
    private Transform activePosition;

    /// <summary>
    /// A transform indicating the position the spikes should retract to
    /// </summary>
    [SerializeField]
    private Transform retractedPosition;


    // Is always passable but spikes will kill you.
    public override bool IsPassable
    {
        get
        {
            return true;
        }
        protected set { }
    }

    // Sets up the property for graphics variant
    public override int GraphicsVariant { get; set; }

    /// <summary>
    /// Keeps track of what turn it is on.
    /// </summary>
    private int _turnCounter { get; set; }



    /// <summary>
    /// Changes the spike up and down depending on the frquency.
    /// </summary>
    public override void OnPlayerMove()
    {
        _turnCounter++;
        if (_turnCounter % State == 0)
        {
            ApplyVisuals();

            // Play a sound effect
            AudioManager.Play(AudioManager.SoundEffects.metalSwoosh);
        }
        else if (_turnCounter % State == 1)
        {
            ApplyVisuals();
        }
    }

    /// <summary>
    /// Just to check whether the player has landed on a spike
    /// tile and deserves to die.
    /// </summary>
    public override void OnPlayerLand()
    {
        // Play a sound effect
        AudioManager.Play(AudioManager.SoundEffects.thud);
        // Check for death
        if (_turnCounter % State == 0)
        {
            Debug.Log("Player Dies");
            AudioManager.Play(AudioManager.SoundEffects.ouch);
        }
        else
        {
            Debug.Log("You have survived... for now");
        }

    }


    /// <summary>
    /// Setting up the platforms for the start of the level.
    /// </summary>
    public override void OnLevelStart()
    {
        _turnCounter = 1;
        SpikeGraphics.transform.position = retractedPosition.transform.position;
        ApplyVisuals();
    }

    /// <summary>
    /// Increases the turn counter by one.
    /// </summary>
    public override void OnLevelTurn()
    {
        //
    }

    /// <summary>
    /// Apply the visual positioning of the spike graphics
    /// </summary>
    public void ApplyVisuals()
    {
        Transform targetTransform = _turnCounter % State == 0 ? activePosition : retractedPosition;
        SpikeGraphics.SlideTo(targetTransform.position, -1, false);
    }
}
