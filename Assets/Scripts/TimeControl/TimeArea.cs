using System.Collections;
using System.Collections.Generic;
using UnityEngine;

[RequireComponent(typeof(Rigidbody))]
[RequireComponent(typeof(SphereCollider))]
public class TimeArea : MonoBehaviour
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
    private AnimationCurve _scaleCurve;
    [SerializeField]
    private float _scaleDelay = 0.1F;
    [SerializeField]
    private float _duration = 4F;

    private Rigidbody _rigidbody;
    private SphereCollider _collider;
    private List<ScaleTarget> _targets = new List<ScaleTarget>();
    private Coroutine _scaleCoroutine;
    private WaitForSeconds _scaleDelayRef;
    private WaitForSeconds _durationRef;

    private void Awake()
    {
        _collider = GetComponent<SphereCollider>();
        _rigidbody = GetComponent<Rigidbody>();
        _rigidbody.isKinematic = true;
        _collider.isTrigger = true;
        _scaleDelayRef = new WaitForSeconds(_scaleDelay);
        _durationRef = new WaitForSeconds(_duration);
    }

    private void OnEnable()
    {
        StartCoroutine(Deactivate());
        _scaleCoroutine = StartCoroutine(Scale());
    }

    private void OnDisable()
    {
        foreach (var target in _targets)
            target.TimeController.RemoveEffector(this);
        
        _targets.Clear();
        StopCoroutine(_scaleCoroutine);
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

            timeController.RemoveEffector(this);
        }
    }

    private IEnumerator Deactivate()
    {
        yield return _durationRef;
        gameObject.SetActive(false);
    }

    private IEnumerator Scale()
    {
        Bounds bounds = _collider.bounds;
        Vector3 center = bounds.center;
        float radius = bounds.extents.x;

        while (true)
        {
            foreach (var target in _targets)
            {
                float distance = (target.Transform.position - center).magnitude;
                float ratio = Mathf.Clamp(distance, 0F, radius) / radius;
                float timeScale = _scaleCurve.Evaluate(ratio);

                target.TimeController.AddOrSetEffector(this, timeScale);
            }

            yield return _scaleDelayRef;
        }
    }
}