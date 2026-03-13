using System;
using UnityEngine;
using UnityEngine.Serialization;
using UnityEngine.UI;

public class PlayerButtonController : MonoBehaviour
{
    [SerializeField] private Button[] playerButtons;

    private void Awake()
    {
        Attack();
        Guard();
        Dash();
    }

    private void Attack()
    {
        playerButtons[0].onClick.AddListener(() =>
        {
            
        });
    }

    private void Guard()
    {
        playerButtons[1].onClick.AddListener(() =>
        {
            
        });
    }

    private void Dash()
    {
        playerButtons[2].onClick.AddListener(() =>
        {
            
        });
    }
}
