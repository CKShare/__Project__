using System;
using System.Collections.Generic;
using UnityEngine;

public class ComboWeapon : Weapon
{
    [SerializeField]
    private ComboPattern[] _patterns = new ComboPattern[0];

    protected override void Start()
    {
        base.Start();

        Array.Sort(_patterns, (x, y) => x.Priority < y.Priority ? -1 : 1);
    }

    public IReadOnlyList<ComboPattern> Patterns => _patterns;
}