using UnityEngine;

namespace Forbge;

public class Managers {
    public static T GetResource<T>() where T : UnityEngine.Object {
        T[] objs = Resources.FindObjectsOfTypeAll<T>();
        if (objs.Length > 0)
            return objs[0];
        else
            return null;
    }

    private static Relics.RelicManager _relicManager = null;
    public static Relics.RelicManager relicManager { get {
        // NB: Checking "== null" checks both if it's actually null, _and_ if the
        // object has been deleted, because Unity thought that would be a good idea
        if (_relicManager == null)
            _relicManager = GetResource<Relics.RelicManager>();
        return _relicManager;
    }}

    private static DeckManager _deckManager = null;
    public static DeckManager deckManager { get {
        if (_deckManager == null)
            _deckManager = GetResource<DeckManager>();
        return _deckManager;
    }}

    private static Cruciball.CruciballManager _cruciballManager = null;
    public static Cruciball.CruciballManager cruciballManager { get {
        if (_cruciballManager == null)
            _cruciballManager = GetResource<Cruciball.CruciballManager>();
        return _cruciballManager;
    }}

    private static Battle.BattleController _battleController = null;
    public static Battle.BattleController battleController { get {
        if (_battleController == null)
            _battleController = GetResource<Battle.BattleController>();
        return _battleController;
    }}

    private static Battle.PostBattleController _postBattleController = null;
    public static Battle.PostBattleController postBattleController { get {
        if (_postBattleController == null)
            _postBattleController = GetResource<Battle.PostBattleController>();
        return _postBattleController;
    }}

    private static Battle.Attacks.AttackManager _attackManager = null;
    public static Battle.Attacks.AttackManager attackManager { get {
        if (_attackManager == null)
            _attackManager = GetResource<Battle.Attacks.AttackManager>();
        return _attackManager;
    }}

    private static Battle.TargetingManager _targetingManager = null;
    public static Battle.TargetingManager targetingManager { get {
        if (_targetingManager == null)
            _targetingManager = GetResource<Battle.TargetingManager>();
        return _targetingManager;
    }}

    private static EnemyManager _enemyManager = null;
    public static EnemyManager enemyManager { get {
        if (_enemyManager == null)
            _enemyManager = GetResource<EnemyManager>();
        return _enemyManager;
    }}

    private static PeglinUI.LoadoutManager.LoadoutManager _loadoutManager = null;
    public static PeglinUI.LoadoutManager.LoadoutManager loadoutManager { get {
        if (_loadoutManager == null)
            _loadoutManager = GetResource<PeglinUI.LoadoutManager.LoadoutManager>();
        return _loadoutManager;
    }}

    private static SecretSeedManager _secretSeedManager = null;
    public static SecretSeedManager secretSeedManager { get {
        if (_secretSeedManager == null)
            _secretSeedManager = GetResource<SecretSeedManager>();
        return _secretSeedManager;
    }}

    private static Currency.CurrencyManager _currencyManager = null;
    public static Currency.CurrencyManager currencyManager { get {
        if (_currencyManager == null)
            _currencyManager = GetResource<Currency.CurrencyManager>();
        return _currencyManager;
    }}

    private static Battle.PlayerHealthController _playerHealthController = null;
    public static Battle.PlayerHealthController playerHealthController { get {
        if (_playerHealthController == null)
            _playerHealthController = GetResource<Battle.PlayerHealthController>();
        return _playerHealthController;
    }}

    private static Battle.StatusEffects.PlayerStatusEffectController _playerStatusEffectController = null;
    public static Battle.StatusEffects.PlayerStatusEffectController playerStatusEffectController { get {
        if (_playerStatusEffectController == null)
            _playerStatusEffectController = GameObject.FindWithTag("Player")?.GetComponent<Battle.StatusEffects.PlayerStatusEffectController>();
        return _playerStatusEffectController;
    }}

    public static Challenges.ChallengeManager challengeManager => Challenges.ChallengeManager.Instance;
    public static Map.MapController mapController => Map.MapController.instance;
    public static SaveManager saveManager => SaveManager.Instance;
    public static TooltipManager tooltipManager => TooltipManager.Instance;
    public static Loading.PeglinSceneLoader sceneLoader => Loading.PeglinSceneLoader.Instance;
}
