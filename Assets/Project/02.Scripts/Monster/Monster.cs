using System;
using UnityEngine;

public enum MonsterType
{
    Normal,
    Boss,
}

public abstract class Monster : MonoBehaviour
{
    [SerializeField] private MonsterType type;
    [SerializeField] private int hp;
    [SerializeField] private float moveSpeed;

    public bool IsMoveStop { get; set; }

    protected void Update()
    {
        if (IsMoveStop) return;
        transform.Translate(Vector2.left * (moveSpeed * Time.deltaTime));
    }
}
