// PlayerHiderState.cs
public class PlayerHiderState : IPlayerState
{
    public void EnterState(PlayerController player)
    {
        player.hiderControlUI.SetActive(true);
        player.seekerControlUI.SetActive(false);
        var outline = player.GetComponent<Outline>();
        if (outline != null) outline.enabled = true;
    }

    public void UpdateState(PlayerController player)
    {
        float speed = player.movement.GetSpeed();
        player.GetComponent<InvisibleController>()?.UpdateInvisible(speed);
    }

    public void ExitState(PlayerController player)
    {
        player.GetComponent<InvisibleController>()?.ResetInvisible();
        player.hiderControlUI.SetActive(false);

        var outline = player.GetComponent<Outline>();
        if (outline != null) outline.enabled = false;
    }
}