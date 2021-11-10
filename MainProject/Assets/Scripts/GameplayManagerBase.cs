using System.Collections;
using System.Collections.Generic;
using UnityEngine;


//this abstract class serves as baseline for singleton gameplay managers that controll the flow of their gameplay scene
//all of these should be singletons, because you dont want 2 managers managing the same thing
public abstract class GameplayManagerBase : MonoBehaviour, IGameplayManager
{
    public GameObject PlayerPrefab;
    protected static GameplayManagerBase instance;
    public static GameplayManagerBase Instance { get { return instance; } }

    protected virtual void Awake()
    {
        if (instance != null && instance != this)
        {
            Destroy(this.gameObject);
        }
        else
        {
            instance = this;
        }
    }

    protected virtual void Update()
    {
        //TODO propper event system
        if (GameManager.Instance.SceneLoaded && GameManager.Instance.CurrentSceneIndex == 1)
        {
            GameManager.Instance.SceneLoaded = false;
            OnSceneLoaded();
        }
        else if (GameManager.Instance.SceneCompletelyReady && GameManager.Instance.CurrentSceneIndex == 1)
        {
            GameManager.Instance.SceneCompletelyReady = false;
            OnSceneCompletelyReady();
        }
    }

    public virtual PlayerController CreatePlayer(Vector3 pos, Vector3 rot)
    {
        return Instantiate(PlayerPrefab, pos, Quaternion.Euler(rot)).GetComponent<PlayerController>();
    }


    public abstract void SetupStage();

    public virtual void OnSceneLoaded()
    {
        CreatePlayer(Vector3.zero, Vector3.zero);
    }

    public virtual void OnSceneCompletelyReady()
    {
    }
}
