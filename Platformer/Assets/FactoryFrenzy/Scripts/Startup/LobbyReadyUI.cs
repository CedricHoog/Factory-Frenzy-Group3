using FactoryFrenzy.Gameplay;
using TMPro;
using UnityEngine;

public class LobbyReadyUI : MonoBehaviour
{
    [SerializeField] private TMP_Text readyText;

    private void OnEnable()
    {
        PlayerData.OnPlayersChanged += Refresh;
        Refresh();
    }

    private void OnDisable() => PlayerData.OnPlayersChanged -= Refresh;

    private void Refresh()
    {
        int ready = 0;
        foreach (var p in PlayerData.All)
            if (p.IsReady.Value) ready++;

        readyText.text = $"Ready Players : {ready} / {PlayerData.All.Count}";
    }
}