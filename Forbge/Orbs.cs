using System;
using System.Collections.Generic;
using UnityEngine;

namespace Forbge;

public abstract class CustomOrb : MonoBehaviour {
    // Components
    private PachinkoBall _ball;
    private Battle.Attacks.Attack _attack;
    private CircleCollider2D _collider;
    private Rigidbody2D _rigidbody;
    public PachinkoBall Ball => _ball;
    public Battle.Attacks.Attack Attack => _attack;
    public CircleCollider2D Collider => _collider;
    public Rigidbody2D Rigidbody => _rigidbody;

    // Translation magic
    private string _baseName;
    private string _locKey;
    private Translation _name;
    private List<Translation> _description;
    private Translation _lockedDescription;
    public Translation Name => _name;
    public List<Translation> Description => _description;
    public Translation LockedDescription => _lockedDescription;

    // Convenience properties
    public Sprite Sprite { get => Ball._renderer.sprite; set { Ball._renderer.sprite = value; } }
    public int Level {
        get => Attack.Level;
        set {
            Attack.Level = value;
            // Todo: update the translations
        }
    }
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
    public int Damage { get => Attack.DamagePerPeg; set { Attack.DamagePerPeg = value; }}
    public int Crit { get => Attack.CritDamagePerPeg; set { Attack.CritDamagePerPeg = value; }}

    // Attack.Level
    // Attack.NextLevelPrefab
    // Attack.PreviousLevelPrefab

    // ProjectileAttack._shotPrefab, _criticalShotPrefab
    // TargetedAttack._thunderPrefab, _criticalThunderPrefab

    // CircleCollider2D.radius = PachinkoBall.DEFAULT_ORB_COLLIDER_RADIUS * (sprite size / 16)
    // Collider2D.bounciness = 0; Rubborb = 0.85
    // Rigidbody2D.mass = 1.2; Bouldorb = 2.0; Infernorb = 0.8

    //  xlats: Orbs/{locNameString}_name, Orbs/locNameString}_desc_locked
    // Attack.locDescStrings = [Orbs/{i}], joined as bullet points


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

        return custom;
    }

    internal abstract Battle.Attacks.Attack InitAttack();
}

public class CustomProjectileOrb : CustomOrb {
    public Battle.Attacks.ProjectileAttack projectileAttack => (Battle.Attacks.ProjectileAttack)Attack;

    internal override Battle.Attacks.Attack InitAttack() {
        return GetComponent<Battle.Attacks.ProjectileAttack>();
    }
}
