using UnityEngine;

public class BotController : MonoBehaviour
{
    private IBotRole currentRole;

    public void ChangeRole(IBotRole newRole)
    {
        currentRole?.Exit();
        currentRole = newRole;
        currentRole?.Enter();
    }

    private void Update()
    {
        currentRole?.Update();
    }
}
