using System;
using UnityEngine;

public abstract class LockpickGame : MonoBehaviour, IComparable
{

    public Animator LockAnimator;

    public abstract void BeginLockpicking(Player player);

    public abstract void EndLockpicking();

    public abstract void OnSuccess();

    public abstract void OnFailure();

    public void OpenLock()
    {
        if (LockAnimator != null)
        {
            LockAnimator?.SetTrigger("Open");
        }
    }
    
    //This does not exist in the 3d museum but here it makes it easier to dynamically populate the menu
    public virtual string GetFriendlyName()
    {
        Debug.LogWarning("No friendlyname set for " + this.ToString());
        return this.ToString();
    }

    //When we're sorted, sort alphabetically by friendlyname
    public int CompareTo(object obj)
    {
        if (obj == null) return 1;

        LockpickGame otherGame = obj as LockpickGame;
        if (otherGame != null)
        {
            return this.GetFriendlyName().CompareTo(otherGame.GetFriendlyName());
        }
        else
        {
            throw new ArgumentException("Object is not a LockpickGame");
        }
    }

}
