using System;
using System.Collections.Generic;
using UnityEngine;
using HarmonyLib;

namespace Forbge;

[HarmonyPatch]
public abstract class CustomOrb : MonoBehaviour {
    // Components
    private PachinkoBall _ball;
    private Battle.Attacks.Attack _attack;
    private CircleCollider2D _collider;
    private Rigidbody2D _rigidbody;
    private SpriteRenderer _renderer;
    private Animator _animator;
    public PachinkoBall Ball => _ball;
    public Battle.Attacks.Attack Attack => _attack;
    public CircleCollider2D Collider => _collider;
    public Rigidbody2D Rigidbody => _rigidbody;
    public SpriteRenderer Renderer => _renderer;
    public Animator Animator => _animator;

    // Translation magic
    private string _baseName;
    private string _locKey;
    private Translation _name;
    private List<Translation> _description = new List<Translation>();
    private List<String> _descriptionKeys = new List<String>();
    private Translation _lockedDescription;
    public Translation Name => _name;
    public List<Translation> Description => _description;
    public Translation LockedDescription => _lockedDescription;
    public PeglinUI.Tooltip.LockedDescriptionPresets LockedDescriptionPreset {
        get => Attack.lockedDescriptionPreset;
        set { Attack.lockedDescriptionPreset = value; }
    }

    // Convenience properties
    public int Damage { get => Attack.DamagePerPeg; set { Attack.DamagePerPeg = value; }}
    public int Crit { get => Attack.CritDamagePerPeg; set { Attack.CritDamagePerPeg = value; }}
    public PachinkoBall.OrbRarity Rarity { get => Ball.orbRarity; set { Ball.orbRarity = value; }}
    public Sprite Sprite { get => Ball._renderer.sprite; set { Ball._renderer.sprite = value; }}
    public int Level { get => Attack.Level; set { Attack.Level = value; }}
    public float Radius { get => Collider.radius; set { Collider.radius = value; }}
    public float Mass { get => Rigidbody.mass; set { Rigidbody.mass = value; }}
    public float Bounciness {
        get => Rigidbody.sharedMaterial == null ? 0f : Rigidbody.sharedMaterial.bounciness;
        set {
            if (Rigidbody.sharedMaterial == null)
                Rigidbody.sharedMaterial = new PhysicsMaterial2D();
            Rigidbody.sharedMaterial.bounciness = value;
        }
    }
    public float Friction {
        get => Rigidbody.sharedMaterial == null ? 0.4f : Rigidbody.sharedMaterial.friction;
        set {
            if (Rigidbody.sharedMaterial == null)
                Rigidbody.sharedMaterial = new PhysicsMaterial2D();
            Rigidbody.sharedMaterial.friction = value;
        }
    }
    public float Scale {
        get => Renderer.transform.localScale.x;
        set { Renderer.transform.localScale = new Vector2(value, value); }
    }

    // ProjectileAttack._shotPrefab, _criticalShotPrefab
    // TargetedAttack._thunderPrefab, _criticalThunderPrefab

    // CircleCollider2D.radius = PachinkoBall.DEFAULT_ORB_COLLIDER_RADIUS * (sprite size / 16)
    // Collider2D.bounciness = 0; Rubborb = 0.85
    // Rigidbody2D.mass = 1.2; Bouldorb = 2.0; Infernorb = 0.8


    private static GameObject _baseOrb;
    private static GameObject _custom_parent;
    internal static void Init() {
        foreach (var i in Resources.FindObjectsOfTypeAll<PachinkoBall>()) {
            if (i.name == "BaseOrb") {
                _baseOrb = i.gameObject;
                break;
            }
        }
        if (_baseOrb == null)
            throw new InvalidOperationException("Cannot find BaseOrb");

        // Possible I'm missing something in the Unity API, but it seems that:
        // * We can't create our custom orbs in the same "resource" state as the
        //   actual game orbs, that state is privileged for actual prefabs from
        //   the Unity editor.
        // * We have to instead have to create them in a Scene which will make
        //   them active, and visible on-screen.
        // * We can hide and deactivate them, but then the game instantiates the
        //   orb, it'll still be deactivated and hidden, which it's not expecting.
        // * But we _can_ create a parent object, hide/deactivate that, and then
        //   make all our custom orbs children of that parent. This seems to work
        _custom_parent = new GameObject("Forbge Custom Orbs", typeof(RectTransform));
        GameObject.DontDestroyOnLoad(_custom_parent);
        _custom_parent.SetActive(false);
    }

    public enum OrbType {
        PROJECTILE, TARGETED, SIMPLE, HEAL, EMPTY,
    }
    public static CustomOrb Create(string name, OrbType type=OrbType.PROJECTILE, Type customType=null) {
        Registry.AssertInRegistration();

        // Create the orb object
        GameObject orb = GameObject.Instantiate(_baseOrb);
        GameObject.DontDestroyOnLoad(orb);
        orb.transform.SetParent(_custom_parent.transform);

        // Create the CustomOrb component, and replace the Attack component if needed
        CustomOrb custom = null;
        if (customType != null) {
            // ...
        } else switch (type) {
            case OrbType.PROJECTILE:
                custom = orb.AddComponent<CustomProjectileOrb>();
                break;
        }
        custom._attack = custom.InitAttack();

        // Grab all the interesting components for the convenience properties
        custom._ball = custom.GetComponent<PachinkoBall>();
        custom._collider = custom.GetComponent<CircleCollider2D>();
        custom._rigidbody = custom.GetComponent<Rigidbody2D>();
        custom._renderer = custom.GetComponentInChildren<SpriteRenderer>();
        custom._animator = custom._renderer.GetComponent<Animator>();

        // Set up defaults
        custom._baseName = name;
        orb.name = custom.Attack.locNameString = custom._locKey = $"_forbge_custom_orb__{Registry.currentRegistrar.GetName().Name}__{name}";
        custom._name = new Translation(name);
        custom._name.Register($"Orbs/{custom._locKey}_name");
        custom._lockedDescription = new Translation();
        custom._lockedDescription.Register($"Orbs/{custom._locKey}_desc_locked");

        custom._animator.runtimeAnimatorController = null;
        custom.Damage = 2;
        custom.Crit = 4;
        custom.Sprite = AssetHelper.QUESTIONMARK;
        custom.Rarity = PachinkoBall.OrbRarity.COMMON;
        custom.Level = -1;
        custom.Scale = 0.6f;

        // OrbPool stores the list of orbs twice, once as an array (set up at build time)
        // and then again as a list (initialised at runtime)... potentially need to add
        // our new orb to both
        var pool = Managers.deckManager.OrbPool;

        var lst = new List<GameObject>(pool.AvailableOrbs);
        lst.Add(orb);
        pool.AvailableOrbs = lst.ToArray();

        if (pool._availableOrbs.Count > 0)
            pool._availableOrbs.Add(orb);

        Loading.AssetLoading.Instance.OrbPrefabs[orb.name] = orb;

        return custom;
    }

    private static int _description_counter = 0;
    public int DescriptionCount => _description.Count;
    public Translation AddDescription() {
        string locKey = $"_forbge_custom_orb_desc__{_description_counter}";
        _description_counter++;

        var t = new Translation();
        _description.Add(t);
        _descriptionKeys.Add(locKey);
        t.Register($"Orbs/{locKey}");

        _attack.locDescStrings = _descriptionKeys.ToArray();

        return t;
    }
    public Translation GetDescription(int ix) => _description[ix];
    public void RemoveDescription(int ix) {
        _description.RemoveAt(ix);
        _descriptionKeys.RemoveAt(ix);
        _attack.locDescStrings = _descriptionKeys.ToArray();
    }

    public CustomOrb CreateLevel() {
        if (Level < 0)
            Level = 1;

        GameObject nextLevel = GameObject.Instantiate(gameObject);
        GameObject.DontDestroyOnLoad(nextLevel);
        nextLevel.transform.SetParent(_custom_parent.transform);

        var nextCustom = nextLevel.GetComponent<CustomOrb>();
        // Fix up all our shortcut properties
        nextCustom._attack = nextCustom.GetComponent<Battle.Attacks.Attack>();
        nextCustom._ball = nextCustom.GetComponent<PachinkoBall>();
        nextCustom._collider = nextCustom.GetComponent<CircleCollider2D>();
        nextCustom._rigidbody = nextCustom.GetComponent<Rigidbody2D>();
        nextCustom._renderer = nextCustom.GetComponentInChildren<SpriteRenderer>();
        nextCustom._animator = nextCustom._renderer.GetComponent<Animator>();

        nextCustom._description = new List<Translation>();
        nextCustom._descriptionKeys = new List<string>();
        for (int i = 0; i < _description.Count; i++) {
            nextCustom.AddDescription().Clone(_description[i]);
        }

        Attack.NextLevelPrefab = nextLevel;
        nextCustom.Attack.PreviousLevelPrefab = gameObject;
        nextCustom.Level = Level + 1;
        nextLevel.name = gameObject.name + "_lvl";
        Loading.AssetLoading.Instance.OrbPrefabs[nextLevel.name] = nextLevel;

        return nextCustom;
    }

    internal abstract Battle.Attacks.Attack InitAttack();
}

public class CustomProjectileOrb : CustomOrb {
    public Battle.Attacks.ProjectileAttack projectileAttack => (Battle.Attacks.ProjectileAttack)Attack;

    internal override Battle.Attacks.Attack InitAttack() {
        return GetComponent<Battle.Attacks.ProjectileAttack>();
    }
}
