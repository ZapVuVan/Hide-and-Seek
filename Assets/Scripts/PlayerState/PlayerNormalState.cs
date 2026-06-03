// PlayerNormalState.cs
public class PlayerNormalState : IPlayerState
{
    public void EnterState(PlayerController player) {
        player.seekerCamera.gameObject.SetActive(false);
        player.seekerControlUI.SetActive(false);
        player.hiderCamera.gameObject.SetActive(true);
        player.hiderControlUI.SetActive(false);
        //var outline = player.GetComponent<Outline>();
        //if (outline != null) outline.enabled = false;
    }
    public void UpdateState(PlayerController player) { }
    public void ExitState(PlayerController player) { }
}