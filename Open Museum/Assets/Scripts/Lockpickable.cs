using UnityEngine;
using System.Collections;

public class Lockpickable : Frobbable
{
    public LockpickGame Game;

    public override void OnFrob(Player player)
    {
        Game.BeginLockpicking(player);
    }
}
