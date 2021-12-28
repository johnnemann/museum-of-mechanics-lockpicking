using UnityEngine;
using System.Collections;

//A very simple class just to allow us to catch the trigger collision
public class MassEffectCursor : MonoBehaviour
{
    public MassEffectLockpickGame lockpickGame;

    //When we get a trigger collision, tell the main game about it
    private void OnTriggerEnter2D(Collider2D collision)
    {
        lockpickGame.CursorCollision();
    }
}
