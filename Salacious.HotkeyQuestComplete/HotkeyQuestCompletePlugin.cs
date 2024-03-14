using BepInEx;
using BepInEx.Configuration;
using BepInEx.Logging;
using EFT.Quests;
using EFT.UI;
using EFT.UI.Screens;
using UnityEngine;

//using QuestClass = GClass1249;
using MainUI = GClass3112;

namespace Salacious.HotkeyQuestComplete;

[BepInPlugin("com.Salacious.HotkeyQuestComplete", "Salacious.HotkeyQuestComplete", "1.0.0")]
public class HotkeyQuestCompletePlugin : BaseUnityPlugin
{
    public static ConfigEntry<KeyboardShortcut> QuestCompleteKey { get; set; }

    internal static ManualLogSource Log;

    private bool isKeyPressed = false;

    private void Awake()
    {
        Log = Logger;
    }

    internal void Start()
    {
        QuestCompleteKey = Config.Bind("Hotkeys", "Quest Complete Key", new KeyboardShortcut(KeyCode.RightControl));
    }

    private void Update()
    {
        if (!QuestCompleteKey.Value.IsPressed() && !isKeyPressed)
            return;

        if (!IsTraderScreen())
            return;

        var questView = GetQuestView();
        var completeButton = questView?._button;
        if (completeButton == null)
            return;

        if (questView._quest?.QuestStatus != EQuestStatus.Started)
            return;

        if (!QuestCompleteKey.Value.IsPressed() && isKeyPressed)
        {
            isKeyPressed = false;
            questView.ShowButtonBlock();
            return;
        }

        isKeyPressed = true;

        completeButton.OnClick.RemoveAllListeners();
        completeButton.OnClick.AddListener(ButtonClicked);
        completeButton.Interactable = true;
        completeButton.SetHeaderText("Complete");
    }

    public void OnDestroy() { }

    private bool IsTraderScreen()
    {
        return MainUI.Instance.CheckCurrentScreen(EEftScreenType.Trader);
    }

    private QuestView GetQuestView()
    {
        return MonoBehaviourSingleton<MenuUI>.Instance?.TradingScreen?.TraderScreensGroup?._questsScreen?._questView;
    }

    public static void ButtonClicked()
    {
        if (!QuestCompleteKey.Value.IsPressed())
            return;

        var questView = MonoBehaviourSingleton<MenuUI>.Instance?.TradingScreen?.TraderScreensGroup?._questsScreen?._questView;
        var quest = questView?._quest;
        if (quest != null && quest.QuestStatus == EQuestStatus.Started)
        {
            quest.SetStatus(EQuestStatus.Success, false, false);
            questView.FinishQuest();
            questView.ShowButtonBlock();
        }
    }
}
