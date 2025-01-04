using UnityEngine;
using System.Collections.Generic;
using Unity.VisualScripting;

/// <summary>
/// A square containing a button that can trigger events via links
/// </summary>
public class ButtonSquare : Square
{
    public override TileType Type =>  TileType.Button;

    // Will always report as passable, if you try to change that you get a warning.
    public override bool IsPassable
    {
        get
        {
            return true;
        }
        protected set
        {
            Debug.LogWarning("Trying to change if button is passable!");
        }
    }

    // Sets up the property for graphics variant
    public override int GraphicsVariant { get; set; }

    // Is the button currently pressed? This will be true when the player or an enemy is on the button
    private bool _isPressed;
    public bool IsPressed
    {
        get
        {
            return _isPressed;
        }
        set
        {
            _isPressed = value;
            UpdateOutgoingCharge();
        }
    }

    // To calculate this buttons charge, we just need to know if it is pressed
    protected override bool RecalculateCharge()
    {
        return IsPressed;
    }

    // TODO: Add support for enemies landing
    public override void OnPlayerLand()
    {
        IsPressed = true;

        // Play a click sound effect
        AudioManager.Play(AudioManager.SoundEffects.click);
    }

    // TODO: Add support for enemies leaving
    public override void OnPlayerLeave()
    {
        IsPressed = false;

        // Play a click sound effect
        AudioManager.Play(AudioManager.SoundEffects.click);
    }
}
