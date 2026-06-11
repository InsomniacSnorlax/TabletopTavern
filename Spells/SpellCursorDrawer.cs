using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using Shapes;

namespace TJ.Spells
{
public class SpellCursorDrawer : MonoBehaviour
{
    [SerializeField] private float spellCursorRadius = 0.5f, spellCursorHitPointSphereRadius = 0.05f, spellCursorRotationSpeed = 0.5f;
    public float SpellCursorRadius => spellCursorRadius;
    public float SpellCursorHitPointSphereRadius => spellCursorHitPointSphereRadius;
    public float SpellCursorRotationSpeed => spellCursorRotationSpeed;
}
}
