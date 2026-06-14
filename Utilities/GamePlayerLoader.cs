using Memori.SaveData;
using Memori.Utilities;
using UnityEngine;
using System.Threading.Tasks;
using MagicaCloth2;
using Memori.Scenes;

namespace TJ
{
public class GamePlayerLoader : Singleton<GamePlayerLoader>
{
    [Header("Player Hero")]
    [SerializeField] private GameObject playerHeroGameObject;
    [SerializeField] private Transform playerBattlePosition, playerMapPosition, playerGamesPosition;

    [Header("Enemy Hero")]
    [SerializeField] private GameObject enemyHeroGameObject;
    [SerializeField] private Transform enemyBattlePosition, enemyMapPosition, enemyGamesPosition;

    private int _loadGeneration;

    private void Start()
    {
        LoadGamePlayer();
        SceneHandler.Instance.OnGameStateChanged += OnGameStateChanged;
    }

    private void OnDestroy()
    {
        if (SceneHandler.HasInstance)
            SceneHandler.Instance.OnGameStateChanged -= OnGameStateChanged;
    }

    public async void LoadGamePlayer()
    {
        int gen = ++_loadGeneration;
        if (playerHeroGameObject != null) Destroy(playerHeroGameObject);
        if (enemyHeroGameObject != null) Destroy(enemyHeroGameObject);

        GameObject playerPrefab = await SaveDataHandler.GetPlayerHeroPrefabAsync();
        if (gen != _loadGeneration) return;
        playerHeroGameObject = Instantiate(playerPrefab, playerBattlePosition);
        playerHeroGameObject.SwapLayer("TavernGOs");

        bool hasCampaign = SaveDataHandler.CampaignSaveExists();
        CampaignSaveData saveData = hasCampaign ? SaveDataHandler.Load() : null;
        Hero enemyHero = TabletopTavernData.Instance.GetEnemyHeroForCampaign(
            hasCampaign ? saveData.heroID : 0,
            hasCampaign ? saveData.bookNumber : 0,
            hasCampaign ? saveData.seed : 0,
            justGetRandomHero: !hasCampaign
        );
        GameObject enemyPrefab = await TabletopTavernData.Instance.LoadHeroPrefabAsync(enemyHero.HeroID);
        if (gen != _loadGeneration) return;
        enemyHeroGameObject = Instantiate(enemyPrefab, enemyBattlePosition);
        enemyHeroGameObject.SwapLayer("TavernGOs");

        MagicaClothHandler[] clothHandlers = GetComponentsInChildren<MagicaClothHandler>(true);
        foreach (MagicaClothHandler clothHandler in clothHandlers)
            clothHandler.DisableClothSimulation();

        PositionPlayer(SceneHandler.Instance.CurrentGameState);
    }

    public void MoveToGames()
    {
        MoveHero(playerHeroGameObject, playerGamesPosition);
        MoveHero(enemyHeroGameObject, enemyGamesPosition);
    }

    public void MoveToMap()
    {
        MoveHero(playerHeroGameObject, playerMapPosition);
        MoveHero(enemyHeroGameObject, enemyMapPosition);
    }

    private void PositionPlayer(GameStateEnum currentState)
    {
        if (currentState == GameStateEnum.Battle)
        {
            MoveHero(playerHeroGameObject, playerBattlePosition);
            MoveHero(enemyHeroGameObject, enemyBattlePosition);
        }
        else if (currentState == GameStateEnum.Map || currentState == GameStateEnum.MainMenu)
        {
            MoveToMap();
        }
    }

    public void PlayPlayerAnimation(string animationName)
    {
        if (playerHeroGameObject == null) return;
        Animator animator = playerHeroGameObject.GetComponentInChildren<Animator>();
        if (animator != null) animator.CrossFade(animationName, 0.2f);
    }

    public void PlayEnemyAnimation(string animationName)
    {
        if (enemyHeroGameObject == null) return;
        Animator animator = enemyHeroGameObject.GetComponentInChildren<Animator>();
        if (animator != null) animator.CrossFade(animationName, 0.2f);
    }

    private void MoveHero(GameObject hero, Transform target)
    {
        if (hero == null || target == null) return;
        hero.transform.SetParent(target);
        hero.transform.SetLocalPositionAndRotation(Vector3.zero, Quaternion.identity);
    }

    private GameStateEnum _previousState;

    private void OnGameStateChanged(GameStateEnum newState)
    {
        if (newState == GameStateEnum.Map && _previousState == GameStateEnum.MainMenu)
            LoadGamePlayer();
        else
            PositionPlayer(newState);

        _previousState = newState;
    }
}
}
