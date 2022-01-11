using System.Collections;
using System.Collections.Generic;
using System.Runtime.CompilerServices;
using UnityEngine;
using UnityEngine.UI;

public class HPBar : MonoBehaviour
{
    [SerializeField] private Slider hpSlider;
    [SerializeField] private Image fill;
    private Quaternion initialRotation;
    private Renderer renderer;

    void Awake()
    {
        initialRotation = transform.rotation;
    }

    public void UpdateHPBar(ActiveEntity entity, bool friendly)
    {
        if (renderer == null)
        {
            renderer = entity.GetComponentInChildren<Renderer>();
        }

        transform.rotation = initialRotation;
        transform.position = new Vector3(entity.capsuleCollider.bounds.center.x, entity.capsuleCollider.bounds.max.y + 1, entity.capsuleCollider.bounds.center.z);
        hpSlider.maxValue = entity.maxHP;
        hpSlider.value = entity.currentHP;
        hpSlider.gameObject.SetActive(!entity.isDead);
        if (friendly)
        {
            fill.color = Color.green;
        }
    }
}
