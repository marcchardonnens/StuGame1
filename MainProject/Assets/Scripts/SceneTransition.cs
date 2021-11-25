using System.Threading.Tasks;
using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.SceneManagement;

public static class SceneTransition
{
    public const float FADETIME = 0f;
    public static UIController UIController;

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
        await TransitionInitialize();
        UIController.Instance.ShowLoadingScreen("Preparing Arena");
        await FadeSceneToTransparent(FADETIME);
        GameManager.Instance.UpdateState(GameState.Transition);
        GameManager.Instance.DestroyCurrentManager();
        AsyncOperation load = SceneManager.LoadSceneAsync(GameConstants.ARENASCENE);
        load.completed += delegate
        {
            UIController.SetLoadingScreenText("Fight!");
            UIController.EnableUIInteraction();
        };
    }

    public async static void TransitionToMenu()
    {
        OnMenuTransitionBegin?.Invoke();
        await TransitionInitialize();
        GameManager.Instance.UpdateState(GameState.Transition);
        GameManager.Instance.DestroyCurrentManager();
        SceneManager.LoadScene(GameConstants.MAINMENUSCENE);
    }

    public async static void TransitionToHub()
    {
        OnHubTransitionBegin?.Invoke();
        await TransitionInitialize();
        UIController.Instance.ShowLoadingScreen("Lighting Fire");
        await FadeSceneToTransparent(FADETIME);
        GameManager.Instance.UpdateState(GameState.Transition);
        GameManager.Instance.DestroyCurrentManager();
        AsyncOperation load = SceneManager.LoadSceneAsync(GameConstants.HUBSCENE);
        load.completed += delegate
        {
            UIController.SetLoadingScreenText("Start Journey");
            UIController.EnableUIInteraction();
        };
    }

    public async static void TransitionToGameplay()
    {
        OnGameplayTransitionBegin?.Invoke();
        await TransitionInitialize();
        UIController.Instance.ShowLoadingScreen("Generating Level");
        await FadeSceneToTransparent(FADETIME);
        GameManager.Instance.UpdateState(GameState.Transition);
        GameManager.Instance.DestroyCurrentManager();
        AsyncOperation load = SceneManager.LoadSceneAsync(GameConstants.GAMEPLAYSCENE);
        load.completed += delegate
        {
            UIController.SetLoadingScreenText("Save the Survivor!");
            UIController.EnableUIInteraction();
        };
    }

    private async static Task TransitionInitialize()
    {
        OnAnyTransitionBegin?.Invoke();
        GameManager.Instance.UpdateState(GameState.TransitionBegin);
        await FadeSceneToBlack(FADETIME);
        UIController.Instance.HideAll();
    }

    public async static void FadeAwayLoadingScreen()
    {
        GameManager.Instance.UpdateState(GameState.TransitionEnding);
        UIController.DisableUIInteraction();
        await FadeSceneToBlack(FADETIME);
        UIController.LoadingScreen.SetActive(false);
        await FadeSceneToTransparent(FADETIME);
        GameManager.Instance.CurrentManager.GiveControl();


        // OnAnyTransitionComplete?.Invoke();
        // if (GameManager.Instance.CurrentSceneIndex == GameConstants.MAINMENUSCENE)
        // {
        //     OnMenuTransitionComplete?.Invoke();
        // }
        // else if (GameManager.Instance.CurrentSceneIndex == GameConstants.HUBSCENE)
        // {
        //     OnHubTransitionComplete?.Invoke();
        // }
        // else if (GameManager.Instance.CurrentSceneIndex == GameConstants.GAMEPLAYSCENE)
        // {
        //     OnGameplayTransitionComplete?.Invoke();
        // }
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
