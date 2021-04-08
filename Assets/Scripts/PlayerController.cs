using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.AI;

[RequireComponent(typeof(NavMeshAgent), typeof(Animator))]
public class PlayerController : MonoBehaviour
{
    [SerializeField, Range(0f, 10f)] private float _speed = 5f;
    public bool isDead = false;

    public enum PowerAttack { Low, Middle, Hight }

    [Header ("Attack")]
    public PowerAttack powerAttack;
    [SerializeField] private GameObject _bulletParticlePrefab;
    [SerializeField] private float _timeoutShot = 0.2f;
    private float _curTimeout;

    private RaycastHit _hit;
    private Vector3 _pointHit;

    private bool _isAttack = false;
    private bool _onTheTower = false;

    private NavMeshAgent _navMeshAgent;
    private Animator _animator;
    private UIController _uiController;

    private void Awake()
    {
        _navMeshAgent = GetComponent<NavMeshAgent>();
        _animator = GetComponent<Animator>();
        _uiController = FindObjectOfType<UIController>();
    }

    private void Update()
    {
        Ray rayDown = new Ray(transform.position, -transform.up);
        RaycastHit hitGround;
        _onTheTower = Physics.Raycast(rayDown, out hitGround, 1f,LayerMask.GetMask("Tower"));

        Moving();
        Attacking();

        if(isDead)
        {
            _uiController.losePanel.SetActive(true);
            Time.timeScale = 0f;
        }
    }

    /// <summary>
    /// Реализует передвижение игрока по нажатию ЛКМ в нажатую позицию
    /// </summary>
    private void Moving()
    {
        Ray rayMove = Camera.main.ScreenPointToRay(Input.mousePosition);

        if (Physics.Raycast(rayMove, out _hit, 100))
        {
            if (Input.GetMouseButtonDown(0))
            {
                _pointHit = _hit.point;

                float rangeOfMovement = Vector3.Distance(transform.position, _pointHit);

                if (rangeOfMovement >= 0.5f && !_hit.transform.gameObject.TryGetComponent(out PlayerController player))
                {
                    _navMeshAgent.enabled = true;
                    _animator.SetBool("IsMoving", true);
                    
                    _navMeshAgent.speed = _speed;
                    _navMeshAgent.SetDestination(_pointHit);

                    _animator.SetFloat("Speed", _speed);
                }              
            }
        }

        float distance = Vector3.Distance(transform.position, _pointHit);

        if (distance <= 0.5f)
        {
            _navMeshAgent.enabled = false;
            _animator.SetBool("IsMoving", false);
        }

        Debug.DrawLine(rayMove.origin, _pointHit, Color.yellow);
    }

    /// <summary>
    /// Реализует выстрел игроком в нажатую позицию ПКМ
    /// </summary>
    private void Attacking()
    {
        if (Input.GetMouseButtonDown(1) && !_isAttack && _curTimeout >= _timeoutShot && _onTheTower)
        {
            _animator.SetBool("IsShooting", true);
            _isAttack = true;
            _curTimeout = 0f;
            StartCoroutine(Shot());
        }
        else if (_curTimeout < _timeoutShot)
        {
            _curTimeout += Time.deltaTime;
        }
        else if (Input.GetMouseButtonDown(1) && !_onTheTower)
        {
            _uiController.textMessage.text = "Поднимитесь на башню, чтобы открыть огонь!";
            _uiController.textMessage.gameObject.SetActive(true);
        }

        if (_isAttack)
        {
            if (Input.GetMouseButtonUp(1))
            {
                _isAttack = false;
                _animator.SetBool("IsShooting", false);
            }
        }
    }

    /// <summary>
    /// Реализует стрельбу с указанной переодичностью в _timeoutShot
    /// </summary>
    /// <returns></returns>
    private IEnumerator Shot()
    {
        while (_isAttack && _onTheTower)
        {
            GetComponentInChildren<ParticleSystem>().Play();

            Transform bulletHole = Instantiate(_bulletParticlePrefab.transform, _hit.point, Quaternion.identity);
            bulletHole.parent = _hit.transform;

            if (_hit.transform.gameObject.TryGetComponent(out EnemyController enemyController))
            {
                enemyController.isDead = true;
            }

            yield return new WaitForSecondsRealtime(0.3f);
            GetComponentInChildren<ParticleSystem>().Stop();
            yield return new WaitForSecondsRealtime(_timeoutShot);
        }
        _animator.SetBool("IsShooting", false);
        StopCoroutine(Shot());
    }
}
