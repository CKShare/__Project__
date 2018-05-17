using System.Collections.Generic;
using UnityEngine;
using Sirenix.OdinInspector;

[RequireComponent(typeof(SphereCollider))]
public class ConcentrateSphere : MonoBehaviour
{
    [SerializeField, InlineEditor, Required]
    private SphereCollider _collider;

    private List<Material[][]> _materialsList = new List<Material[][]>();

    private void OnEnable()
    {
        _collider.enabled = true;
    }

    private void OnDisable()
    {
        foreach (var materialss in _materialsList)
            foreach (var materials in materialss)
                foreach (var material in materials)
                    material.renderQueue = 2000;
        _materialsList.Clear();

        _collider.enabled = false;
    }

    private void OnTriggerEnter(Collider collider)
    {
        var controller = collider.GetComponent<CharacterControllerBase>();
        if (controller == null)
            return;
        
        var materialss = controller.Materials;
        foreach (var materials in materialss)
            foreach (var material in materials)
                material.renderQueue = 3000;
        _materialsList.Add(materialss);
    }

    private void OnTriggerExit(Collider collider)
    {
        var controller = collider.GetComponent<CharacterControllerBase>();
        if (controller == null)
            return;

        var materialss = controller.Materials;
        foreach (var materials in materialss)
            foreach (var material in materials)
                material.renderQueue = 2000;
        _materialsList.Remove(materialss);
    }
}