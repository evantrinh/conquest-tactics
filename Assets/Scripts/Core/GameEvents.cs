using System;
using UnityEngine;

// Systems talk through these events, so no script needs a reference to another system.
// New events are only ever added here. Existing ones keep their signatures.
public static class GameEvents
{
    public static Action<Bounds> OnLevelBuilt;           // GridManager, after it builds the board
    public static Action<Unit> OnUnitSpawned;            // whoever creates a unit announces it here
    public static Action<Unit> OnUnitSelected;
    public static Action OnSelectionCleared;
    public static Action<Unit, Vector2Int> OnUnitMoved;
    public static Action<Unit> OnUnitDamaged;
    public static Action<Team> OnTurnChanged;
    public static Action<bool> OnLevelEnded;            // true = player won
    public static Action<Unit, Unit, int> OnUnitAttacked;   // attacker, target, damage
    public static Action<Unit> OnUnitKilled;
    public static Action<Unit> OnUnitWaited;
    public static Action<int, int> OnActionPointsChanged;   // remaining, max
}
