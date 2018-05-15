using System;
using UnityEngine;
using Sirenix.OdinInspector;

public class SMBEventInfo
{
    [Tooltip("Function name to trigger.")]
    [SerializeField]
    private string _function;
    [Tooltip("Parameter to be sent when function is triggered.")]
    [SerializeField, ShowInInspector]
    private IEventParameter _parameter;

    public string Function => _function;
    public IEventParameter Parameter => _parameter;
}