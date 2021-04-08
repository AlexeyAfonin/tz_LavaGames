using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.AI;

[RequireComponent(typeof(NavMeshAgent), typeof(Animator), typeof(CapsuleCollider))]
public class EnemyController : MonoBehaviour
{
    [SerializeField, Range(0f, 10f)] private float _speed = 1.5f;
    public bool isDead = false;

    [Header("Vision")]
    [SerializeField] private int _numberOfRays = 40;
    [SerializeField, Range(0f, 360f)] private float _angleVision = 40;
    [SerializeField] private Vector3 _offset;
    [SerializeField, Range(5f, 30f)] private float _distanceVision = 5f;

    [Header("Patrolling")]
    [SerializeField] private Transform[] _territoryInspectionPositions;
    private Transform _randomPosition;
    [SerializeField] private float _timeWaiting = 1f;

    private bool _isMoveInRandomTarget = true;
    private bool _isKinematic = true;

    private Rigidbody[] _rigidbodies;
    private NavMeshAgent _navMeshAgent;
    private PlayerController _player;
    private Animator _animator;

    private void Awake()
    {
        _rigidbodies = GetComponentsInChildren<Rigidbody>();
        _navMeshAgent = GetComponent<NavMeshAgent>();
        _animator = GetComponent<Animator>();
    }

    private void Start()
    {
        foreach (Rigidbody rigidbody in _rigidbodies)
            rigidbody.isKinematic = true;

        _player = FindObjectOfType<PlayerController>();
    }

    private void Update()
    {
        if(!isDead)
        {
            if (Vision())
            {
                if (!_navMeshAgent.enabled) _navMeshAgent.enabled = true;
                if (!_animator.enabled) _animator.enabled = true;

                if (_isMoveInRandomTarget)
                {
                    _isMoveInRandomTarget = false;
                    StopCoroutine(Waiting());
                }

                Moving(_player.gameObject.transform);
                Rotating(_player.gameObject.transform.position);
            }
            else
            {
                if (_territoryInspectionPositions.Length != 0)
                {
                    _isMoveInRandomTarget = true;
                    if (_randomPosition == null)
                    {
                        _randomPosition = _territoryInspectionPositions[Random.Range(0, _territoryInspectionPositions.Length)];
                    }

                    if(_navMeshAgent.enabled)
                    {
                        Rotating(_randomPosition.position);
                        Moving(_randomPosition);
                    }
                }
            }    
        }
        else
        {
            Dead();
        }
    }

    /// <summary>
    /// Реализует зрение данного персонажа
    /// </summary>
    private bool Vision()
    {
        float angleRay = 0;

        for (int i = 0; i < _numberOfRays; i++)
        {
            float x = Mathf.Sin(angleRay);
            float y = Mathf.Cos(angleRay);

            angleRay += _angleVision * Mathf.Deg2Rad / _numberOfRays;

            if (CreateRay(transform.TransformDirection(new Vector3(x, 0, y))) || CreateRay(transform.TransformDirection(new Vector3(-x, 0, y)))) 
                return true;
        }

        return false;
    }

    /// <summary>
    /// Выпускает лучи из заданной точки, реализуя функцию видимости объектов 
    /// и окрашивает их (Ничего не пересекающий луч - красный, пересекающий игрока - зеленый, пересекающий другие объекты - синий)
    /// </summary>
    /// <param name="rayDirection">Направление луча</param>
    /// <returns></returns>
    private bool CreateRay(Vector3 rayDirection)
    {
        RaycastHit hit;
        Vector3 startRayPosition = transform.position + _offset;

        if (Physics.Raycast(startRayPosition, rayDirection, out hit, _distanceVision))
        {
            if (hit.transform.gameObject.TryGetComponent(out PlayerController playerController))
            {
                Debug.DrawLine(startRayPosition, hit.point, Color.green);
                return true;
            }
            else Debug.DrawLine(startRayPosition, hit.point, Color.blue);
        }
        else Debug.DrawRay(startRayPosition, rayDirection * _distanceVision, Color.red);

        return false;
    }

    /// <summary>
    /// Осуществяет передвижение персонажа
    /// </summary>
    /// <param name="target">Цель (объект) к которому движется данный персонаж</param>
    private void Moving(Transform target)
    {
        _navMeshAgent.speed = _speed;
        _navMeshAgent.SetDestination(target.position);
        _animator.SetBool("IsMoving", true);
        _animator.SetFloat("Speed", _speed);

        float distance = Vector3.Distance(transform.position, target.position);

        if (_isMoveInRandomTarget)
        {
            if (distance <= 0.5f)
            {
                _randomPosition = null;
                StartCoroutine(Waiting());
            }
        }
        else
        {
            if (distance >= 2f)
            {
                _animator.SetBool("IsMoving", true);
                _animator.SetBool("IsAttack", false);
            }
            else
            {
                _animator.SetBool("IsMoving", false);

                if (target.gameObject.TryGetComponent(out PlayerController playerController))
                {
                    Attack();
                }
            }
        }
    }

    /// <summary>
    /// Осуществяет поворот персожана в сторону цели (объекта)
    /// </summary>
    /// <param name="target">Цель (объект) за которым следит данный персонаж</param>
    private void Rotating(Vector3 target)
    {
        transform.LookAt(target);
        transform.rotation = new Quaternion(0f, transform.rotation.y, 0f, transform.rotation.w);
    }

    /// <summary>
    /// Атакует игрока
    /// </summary>
    private void Attack()
    {
        _animator.SetBool("IsAttack", true);
        _player.isDead = true;
    }

    /// <summary>
    /// Реализует "смерть" персонажа, включая ригдолл и одтлакивая тело исходя из силы импульса
    /// </summary>
    private void Dead()
    {
        if (_isKinematic)
        {
            foreach (Rigidbody rigidbody in _rigidbodies)
            {
                rigidbody.isKinematic = false;

                switch (FindObjectOfType<PlayerController>().powerAttack)
                {
                    case PlayerController.PowerAttack.Low:
                        rigidbody.AddForce(_player.transform.forward * 10, ForceMode.Impulse);
                        break;
                    case PlayerController.PowerAttack.Middle:
                        rigidbody.AddForce(_player.transform.forward * 50, ForceMode.Impulse);
                        break;
                    case PlayerController.PowerAttack.Hight:
                        rigidbody.AddForce(_player.transform.forward * 100, ForceMode.Impulse);
                        break;
                }

            }
            _isKinematic = false;
        }

        _navMeshAgent.enabled = false;
        _animator.enabled = false;
    }

    /// <summary>
    /// Осуществляет "ожидание" указанное количество времени
    /// </summary>
    /// <returns></returns>
    private IEnumerator Waiting()
    {
        _navMeshAgent.enabled = false;
        _animator.SetBool("IsMoving", false);
        
        yield return new WaitForSecondsRealtime(_timeWaiting);

        _navMeshAgent.enabled = true;
        StopCoroutine(Waiting());
    }
}
