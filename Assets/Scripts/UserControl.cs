using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.EventSystems;
using UnityEngine.AI;
using UnityEngine.UI;

public class UserControl : MonoBehaviour
{
    [SerializeField]
    private LayerMask groundLayer;
    [SerializeField]
    private Image healthBar = null;
    private NavMeshAgent agent;

    [SerializeField]
    private int maxHealth = 100;

    private int _hp = 100;

    [SerializeField]
    private Canvas attackRangeCanvas = null;

    private bool isAiming = false;

    public int HP
    {
        get => _hp;
        set
        {
            _hp += value;
            if (_hp > maxHealth)
            {
                _hp = maxHealth;
            }
            else if (_hp < 0)
            {
                _hp = 0;
            }
            healthBar.fillAmount = (float)_hp / maxHealth;
        }
    }

    private void Start()
    {
        agent = GetComponent<NavMeshAgent>();
        StartCoroutine(TakeDamage());
    }

    private void Update()
    {
        if (Input.GetMouseButtonDown(1) && !EventSystem.current.IsPointerOverGameObject())
        {
            Ray ray = Camera.main.ScreenPointToRay(Input.mousePosition);
            RaycastHit hit;
            if (Physics.Raycast(ray, out hit, Mathf.Infinity, groundLayer))
            {
                Move(hit.point);
                GameManager.Instance.SendCommand("#Move#" + hit.point.x + "," + hit.point.z);
            }
        }

        if (Input.GetKeyDown(KeyCode.A))
        {
            isAiming = true;
        }

        if (Input.GetKeyUp(KeyCode.A))
        {
            isAiming = false;
        }

        if (Input.GetMouseButtonDown(0) && isAiming)
        {
            GameManager.Instance.SendCommand("#Attack#");
        }
    }

    public void Move(Vector3 pos)
    {
        agent.SetDestination(pos);
    }

    private IEnumerator TakeDamage()
    {
        while (true)
        {
            HP = -1;
            yield return new WaitForSeconds(1);
        }
    }

    public void Revive()
    {
        HP = maxHealth;
    }
}
