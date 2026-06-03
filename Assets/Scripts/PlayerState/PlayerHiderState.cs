// PlayerHiderState.cs
public class PlayerHiderState : IPlayerState
{
    private InvisibleController invisibleController;
    public void EnterState(PlayerController player)
    {
        invisibleController = player.GetComponent<InvisibleController>();
        player.hiderControlUI.SetActive(true);
        player.seekerControlUI.SetActive(false);                                                                              
        //var outline = player.GetComponent<Outline>();
        //if (outline != null) outline.enabled = true;
    }

    public void UpdateState(PlayerController player)
    {
        float speed = player.movement.GetSpeed();
        invisibleController?.UpdateInvisible(speed);
    }

    public void ExitState(PlayerController player)
    {
        player.GetComponent<InvisibleController>()?.ResetInvisible();
        player.hiderControlUI.SetActive(false);

        //var outline = player.GetComponent<Outline>();
        //if (outline != null) outline.enabled = false;
    }
}