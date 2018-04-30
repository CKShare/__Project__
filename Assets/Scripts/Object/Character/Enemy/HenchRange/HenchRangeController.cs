using System;
using UnityEngine;

[RequireComponent(typeof(RangeWeapon))]
public class HenchRangeController : EnemyController<HenchRangeState>
{
    private RangeWeapon _rangeWeapon;

    protected override void Awake()
    {
        base.Awake();

        _rangeWeapon = GetComponent<RangeWeapon>();
    }

    protected override void OnStateEnter(HenchRangeState state)
    {
        switch (state)
        {
            case HenchRangeState.Idle:
                {
                    if (PatrolOnAwake)
                        ChangeState(HenchRangeState.Patrol);
                }
                break;
        }
    }

    protected override void OnStateUpdate(HenchRangeState state)
    {
        switch (state)
        {
            case HenchRangeState.Patrol:
                {

                }
                break;
        }
    }

    protected override void OnStateExit(HenchRangeState state)
    {
        switch (state)
        {

        }
    }
}