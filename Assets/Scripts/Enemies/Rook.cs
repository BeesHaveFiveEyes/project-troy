using UnityEngine;

public class Rook : Enemy
{
    public override EntityType Type => EntityType.Rook;

    /// <summary>
    /// The move of the pawn.
    /// </summary>
    public override Vector2Int[] EnemyMove
    {
        get
        {
            return new Vector2Int[] { new Vector2Int(0, 1), new Vector2Int(1, 0), new Vector2Int(0, -1), new Vector2Int(-1, 0), };
        }
    }
    public override int GraphicsVariant { get; set; }

    public override void OnEnemyTurn()
    {
        CalculateNextSquare();
    }

    public void CalculateNextSquare()
    {
        int oppositeDirection;
        bool shouldCapture = false;

        if (Direction + 2 < 4)
        {
            oppositeDirection = Direction + 2;
        }
        else
        {
            oppositeDirection = Direction - 2;
        }

        for (int i = 0; i < 4; i++)
        {
            shouldCapture = CheckNextSquareCapture(Position, i);
            if (shouldCapture)
            {
                break;
            }
        }

        if (shouldCapture)
        {
            NextSquare = PlayerController.position;
            PlayerController.Die();
        }
        else if (SquareManager.squares.ContainsKey(Position + EnemyMove[Direction])
            && SquareManager.squares[Position + EnemyMove[Direction]].IsPassable
            && !CheckSquareForEnemy(Position + EnemyMove[Direction]))
        {
            NextSquare = Position + EnemyMove[Direction];
        }
        else if (SquareManager.squares.ContainsKey(Position + EnemyMove[oppositeDirection])
            && SquareManager.squares[Position + EnemyMove[oppositeDirection]].IsPassable
            && !CheckSquareForEnemy(Position + EnemyMove[oppositeDirection]))
        {
            Direction = oppositeDirection;
            NextSquare = Position + EnemyMove[Direction];
        }
        else
        {
            NextSquare = Position;
        }

    }

    public bool CheckNextSquareCapture(Vector2Int startSquare, int captureDirection)
    {
        if (SquareManager.squares.ContainsKey(startSquare + EnemyMove[captureDirection])
            && SquareManager.squares[startSquare + EnemyMove[captureDirection]].IsPassable
            && !CheckSquareForEnemy(startSquare + EnemyMove[captureDirection]))
        {
            if (startSquare + EnemyMove[captureDirection] == PlayerController.position)
            {
                return true;
            }
            else
            {
                return CheckNextSquareCapture(startSquare + EnemyMove[captureDirection], captureDirection);
            }
        }
        return false;
    }
}
