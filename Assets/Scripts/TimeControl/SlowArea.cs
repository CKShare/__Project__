using System.Collections;
using System.Collections.Generic;
using UnityEngine;

[RequireComponent(typeof(Rigidbody))]
[RequireComponent(typeof(SphereCollider))]
public class SlowArea : MonoBehaviour
{
    private struct ScaleTarget
    {
        public ScaleTarget(Transform transform, TimeController timeController)
        {
            Transform = transform;
            TimeController = timeController;
        }

        public Transform Transform { get; private set; }
        public TimeController TimeController { get; private set; }
    }

    [SerializeField]
    private AnimationCurve _slowCurve;
    [SerializeField]
    private float _applyDelay;
    [SerializeField]
    private float _duration;

    private Rigidbody _rigidbody = null;
    private SphereCollider _collider = null;
    private List<ScaleTarget> _targets = new List<ScaleTarget>();
    private float _durationElapsed = 0F;
    private float _scaleElapsed = 0F;

    private void Awake()
    {
        _collider = GetComponent<SphereCollider>();
        _rigidbody = GetComponent<Rigidbody>();
        _rigidbody.isKinematic = true;
        _collider.isTrigger = true;
    }

    private void OnEnable()
    {
        _durationElapsed = _scaleElapsed = 0F;
    }

    private void OnDisable()
    {
        foreach (var target in _targets)
            target.TimeController.TimeScale = 1F;
        
        _targets.Clear();
    }

    private void OnTriggerEnter(Collider collider)
    {
        var timeController = collider.GetComponent<TimeController>();
        if (timeController != null)
        {
            _targets.Add(new ScaleTarget(collider.transform, timeController));
        }
    }

    private void OnTriggerExit(Collider collider)
    {
        var timeController = collider.GetComponent<TimeController>();
        if (timeController != null)
        {
            int index = _targets.FindIndex(x => x.Transform == collider.transform);
            _targets.RemoveAt(index);

            timeController.TimeScale = 1F;
        }
    }

    private void Update()
    {
        float deltaTime = Time.deltaTime;
        _durationElapsed += deltaTime;
        _scaleElapsed += deltaTime;

        if (_durationElapsed > _duration)
        {
            gameObject.SetActive(false);
        }
        else if (_scaleElapsed > _applyDelay)
        {
            Bounds bounds = _collider.bounds;
            Vector3 center = bounds.center;
            float radius = bounds.extents.x;

            foreach (var target in _targets)
            {
                float distance = (target.Transform.position - center).magnitude;
                float ratio = Mathf.Clamp(distance, 0F, radius) / radius;
                float timeScale = _slowCurve.Evaluate(ratio);

                target.TimeController.TimeScale = timeScale;
            }

            _scaleElapsed -= _applyDelay;
        }
    }
}