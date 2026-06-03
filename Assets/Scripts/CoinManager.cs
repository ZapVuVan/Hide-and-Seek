using System;
using UnityEngine;

public class CoinManager : MonoBehaviour
{
    public static CoinManager Instance { get; private set; }

    public int coin;

    public event Action<int> OnCoinChanged;

    private const string COIN_KEY = "PLAYER_COIN";

    private void Awake()
    {
        if (Instance == null) Instance = this;
        else Destroy(gameObject);

        LoadCoin();
    }

    public void AddCoin(int amount)
    {
        coin += amount;
        OnCoinChanged?.Invoke(coin);
        SaveCoin();
    }

    public bool SpendCoin(int amount)
    {
        if (coin < amount) return false;

        coin -= amount;
        OnCoinChanged?.Invoke(coin);
        SaveCoin();
        return true;
    }

    public int GetCoin() => coin;

    // ================= SAVE / LOAD =================

    public void SaveCoin()
    {
        PlayerPrefs.SetInt(COIN_KEY, coin);
        PlayerPrefs.Save();
    }

    public void LoadCoin()
    {
        coin = PlayerPrefs.GetInt(COIN_KEY, 0);
        OnCoinChanged?.Invoke(coin);
    }

    public void ResetCoin()
    {
        coin = 0;
        PlayerPrefs.DeleteKey(COIN_KEY);
        OnCoinChanged?.Invoke(coin);
    }
}