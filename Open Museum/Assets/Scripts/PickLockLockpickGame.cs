using UnityEngine;
using UnityEngine.Video;


//PickLock is not an actual game in the museum - it shows a video of a thesis project from NYU with a physical lockpick controller.
//So all this "game" does is play that video. Since I have permission to include the video in the museum, but not permission to distribute it,
//the video is not here and this game is pretty pointless. I have included it for completion's sake
public class PickLockLockpickGame : LockpickGame
{
    Player thePlayer;


    //UI Stuff
    public GameObject Panel;
    //A reference to the video player, already created with the correct file
    public VideoPlayer videoPlayer;

    public override void BeginLockpicking(Player player)
    {
        Panel.SetActive(true);
        thePlayer = player;
        thePlayer.FreezePlayer();

        //Just play the video
        videoPlayer.Play();
    }

    //When the player exits, open the lock, stop the video, and leave the game
    public override void EndLockpicking()
    {
        OpenLock();
        videoPlayer.Stop();
        Panel.SetActive(false);
        thePlayer.UnfreezePlayer();
    }

    //No failure...
    public override void OnFailure()
    {
        throw new System.NotImplementedException();
    }
    //Or success
    public override void OnSuccess()
    {
        throw new System.NotImplementedException();
    }

    void Update()
    {
        if (Input.GetKeyUp(KeyCode.Escape))
        {
            EndLockpicking();
        }

    }

    public override string GetFriendlyName()
    {
        return "Picklock (Not Available)";
    }
}
