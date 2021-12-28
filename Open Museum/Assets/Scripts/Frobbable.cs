using UnityEngine;
using System.Collections;

//OPENSOURCE: remove glow code
public class Frobbable : MonoBehaviour
{
    // Update is called once per frame
    void Update()
    {

    }

    public virtual void OnHighlight()
    {
        //In the museum, this just activates a shader on the associated object
    }

    public virtual void HighlightOff()
    {
        
    }

    public virtual void OnFrob(Player player)
    {
        //Just here to be overridden on subclasses
    }
}
