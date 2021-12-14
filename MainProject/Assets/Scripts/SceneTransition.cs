using System.Threading.Tasks;
using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.SceneManagement;

public static class SceneTransition
{
    public const float FADETIME = 2f;
    // public static UIController UIController;

    public static event Action OnAnyTransitionBegin = delegate { };
    public static event Action OnMenuTransitionBegin = delegate { };
    public static event Action OnHubTransitionBegin = delegate { };
    public static event Action OnGameplayTransitionBegin = delegate { };

    public static event Action OnAnyTransitionComplete = delegate { };
    public static event Action OnMenuTransitionComplete = delegate { };
    public static event Action OnHubTransitionComplete = delegate { };
    public static event Action OnGameplayTransitionComplete = delegate { };

    public async static void TransitionToArena()
    {
        OnAnyTransitionBegin?.Invoke();
        await TransitionInitialize();
        UIController.Instance.ShowLoadingScreen("Preparing Arena");
        await FadeSceneToTransparent(FADETIME);
        GameManager.Instance.UpdateState(GameState.Transition);
        GameManager.Instance.DestroyCurrentManager();
        AsyncOperation load = SceneManager.LoadSceneAsync(GameConstants.ARENASCENE);
        load.completed += delegate
        {
            UIController.Instance.SetLoadingScreenText("Fight!");
            UIController.Instance.EnableUIInteraction();
            OnAnyTransitionComplete?.Invoke();
        };
    }

    public async static void TransitionToMenu()
    {
        OnMenuTransitionBegin?.Invoke();
        OnAnyTransitionBegin?.Invoke();
        await TransitionInitialize(FADETIME);
        GameManager.Instance.UpdateState(GameState.Transition);
        GameManager.Instance.DestroyCurrentManager();
        SceneManager.LoadScene(GameConstants.MAINMENUSCENE);
        await Task.Delay(2000);
        UIController.Instance.EnableUIInteraction();
        OnMenuTransitionComplete?.Invoke();
        OnAnyTransitionComplete?.Invoke();
        AudioManager.Instance.PlayMenuMusic();
        AudioManager.Instance.FadeInAllSound();
        await FadeSceneToTransparent(FADETIME + 1f);
    }

    public async static void TransitionToHub()
    {
        OnHubTransitionBegin?.Invoke();
        OnAnyTransitionBegin?.Invoke();
        await TransitionInitialize();
        UIController.Instance.DisableUIInteraction();
        UIController.Instance.ShowLoadingScreen("...LIGHTING CAMPFIRE...");
        GameManager.Instance.DestroyCurrentManager();
        await FadeSceneToTransparent(FADETIME);
        GameManager.Instance.UpdateState(GameState.Transition);
        AsyncOperation load = SceneManager.LoadSceneAsync(GameConstants.HUBSCENE);
        load.completed += delegate
        {
            HubManager.OnSceneReady += delegate
            {
                UIController.Instance.SetLoadingScreenText("...HOME SWEET HOME...");
                UIController.Instance.EnableUIInteraction();
                OnHubTransitionComplete?.Invoke();
                OnAnyTransitionComplete?.Invoke();
            };
        };
    }

    public async static void TransitionToGameplay()
    {
        OnGameplayTransitionBegin?.Invoke();
        OnAnyTransitionBegin?.Invoke();
        await TransitionInitialize();
        UIController.Instance.DisableUIInteraction();
        UIController.Instance.ShowLoadingScreen("...GENERATING NIGHTMARE...");
        GameManager.Instance.DestroyCurrentManager();
        await FadeSceneToTransparent(FADETIME);
        GameManager.Instance.UpdateState(GameState.Transition);
        AsyncOperation load = SceneManager.LoadSceneAsync(GameConstants.GAMEPLAYSCENE);
        load.completed += delegate
        {
            StageManager.OnSceneReady += delegate
            {
                UIController.Instance.SetLoadingScreenText("...START DEADLY ADVENTURE...");
                UIController.Instance.EnableUIInteraction();
                OnGameplayTransitionComplete?.Invoke();
                OnAnyTransitionComplete?.Invoke();
            };
        };
    }

    private async static Task TransitionInitialize(float fadetime = FADETIME)
    {
        GameManager.Instance.UpdateState(GameState.TransitionBegin);
        await FadeSceneToBlack(fadetime);
        UIController.Instance.HideAll();
    }

    public async static void FadeAwayLoadingScreen()
    {
        GameManager.Instance.UpdateState(GameState.TransitionEnding);
        UIController.Instance.DisableUIInteraction();
        await FadeSceneToBlack(FADETIME);
        UIController.Instance.LoadingScreen.SetActive(false);
        await FadeSceneToTransparent(FADETIME);
        GameManager.Instance.CurrentManager.GiveControl();
    }


    public static async Task FadeSceneToBlack(float fadeOutDuration)
    {
        UIController.Instance.FadePannel.alpha = 0;

        for (float i = 0; i <= 1; i += Time.unscaledDeltaTime / fadeOutDuration)
        {
            UIController.Instance.FadePannel.alpha = i;
            await Task.Yield();
        }
        UIController.Instance.FadePannel.alpha = 1;
    }

    public async static Task FadeSceneToTransparent(float fadeInDuration)
    {
        UIController.Instance.FadePannel.alpha = 1;
        for (float i = 1; i >= 0; i -= Time.unscaledDeltaTime / fadeInDuration)
        {
            UIController.Instance.FadePannel.alpha = i;
            await Task.Yield();
        }
        UIController.Instance.FadePannel.alpha = 0;
    }





}
