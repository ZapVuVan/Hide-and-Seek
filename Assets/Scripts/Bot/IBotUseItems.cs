using UnityEngine;

public interface IBotItem
{
    string ItemName { get; }
    void Use(BotController bot);
}