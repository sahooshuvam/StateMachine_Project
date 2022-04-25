using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.AI;

public class GameManager : MonoBehaviour
{
    // Start is called before the first frame update
    NavMeshAgent agent;
    Animator animator;
    public Transform player;
    State currentState;
    void Start()
    {
        agent = GetComponent<NavMeshAgent>();
        animator = GetComponent<Animator>();
        currentState = new Idle(this.gameObject, agent, animator, player);
    }

    // Update is called once per frame
    void Update()
    {
        currentState = currentState.Process();
    }
}
public class State
{
    public enum STATE { IDLE, ATTACK, PATROL, CHASE, DEATH }
    public enum EVENTS { ENTER, UPDATE, EXIT }
    public STATE stateName;
    public EVENTS eventStage;
    public GameObject npc;
    public NavMeshAgent agent;
    public Animator animator;
    public Transform playerPosition;
    public State nextState;

    public float visualDistance = 10f, visualAngle = 30f, shootingDistance = 5f;
    public State(GameObject _npc, NavMeshAgent _agent, Animator _animator, Transform _playerPosition)
    {
        this.npc = _npc;
        this.agent = _agent;
        this.animator = _animator;
        this.playerPosition = _playerPosition;
        eventStage = EVENTS.ENTER;

    }
    public virtual void EnterMethod()
    {
        eventStage = EVENTS.UPDATE;
    }
    public virtual void UpdateMethod()
    {
        eventStage = EVENTS.UPDATE;
    }
    public virtual void ExitMethod()
    {
        eventStage = EVENTS.EXIT;
    }
    public State Process()
    {
        if (eventStage == EVENTS.ENTER)
        {
            EnterMethod();
        }
        if (eventStage == EVENTS.UPDATE)
        {
            UpdateMethod();
        }
        if (eventStage == EVENTS.EXIT)
        {
            ExitMethod();
            return nextState;
        }
        return this;
    }
    public bool CanSeePlayer()
    {
        Vector3 direction = playerPosition.position - npc.transform.position;
        float angle = Vector3.Angle(direction, npc.transform.forward);
        if (direction.magnitude < visualDistance && angle < visualAngle)
        {
            return true;
        }
        return false;
    }
    public bool EnemyCanAttackPlayer()
    {
        Vector3 direction = playerPosition.position - npc.transform.position;
        if (direction.magnitude < shootingDistance)
        {
            return true;
        }
        return false;
    }
}
public class Idle : State
{
    public Idle(GameObject _npc, NavMeshAgent _agent, Animator _animator, Transform _playerPosition) : base(_npc, _agent, _animator, _playerPosition)
    {
        stateName = STATE.IDLE;

    }
    public override void EnterMethod()
    {
        animator.SetTrigger("isIdle");
        base.EnterMethod();

    }
    public override void UpdateMethod()
    {
        if (CanSeePlayer())
        {
            nextState = new Chase(npc, animator, agent, playerPosition);
            eventStage = EVENTS.EXIT;
        }
        else if (Random.Range(0, 100) < 10)
        {
            nextState = new Patrol(npc, animator, agent, playerPosition);
            eventStage = EVENTS.EXIT;
        }
        //base.UpdateMethod();

    }
    public override void ExitMethod()
    {
        animator.ResetTrigger("isIdle");
        base.ExitMethod();
    }
}
public class Patrol : State
{
    int currentIndex = -1;
    public Patrol(GameObject _npc, Animator _animator, NavMeshAgent _agent, Transform _playerPosition) : base(_npc, _agent, _animator, _playerPosition)
    {
        stateName = STATE.PATROL;
        agent.speed = 2;
        agent.isStopped = false;
    }
    public override void EnterMethod()
    {
        animator.SetTrigger("isWalking");
        currentIndex = 0;
        base.EnterMethod();

    }
    public override void UpdateMethod()
    {

        if (CanSeePlayer())
        {
            nextState = new Chase(npc, animator, agent, playerPosition);
            eventStage = EVENTS.EXIT;
        }

        if (agent.remainingDistance < 1)
        {
            if (currentIndex >= GameController.Instance.Checkpoints.Count - 1)
            {
                currentIndex = 0;
            }
            else
            {
                currentIndex++;
            }
            agent.SetDestination(GameController.Instance.Checkpoints[currentIndex].transform.position);

        }
        //base.UpdateMethod();

    }
    public override void ExitMethod()
    {
        animator.ResetTrigger("isWalking");
        base.ExitMethod();
    }

}
public class Chase : State
{
    public Chase(GameObject _npc, Animator _animator, NavMeshAgent _agent, Transform _playerPosition) : base(_npc, _agent, _animator, _playerPosition)
    {
        stateName = STATE.CHASE;
        agent.speed = 5f;
        agent.isStopped = false;
    }
    public override void EnterMethod()
    {
        animator.SetTrigger("isRunning");
        base.EnterMethod();

    }
    public override void UpdateMethod()
    {
        agent.SetDestination(playerPosition.position);
        if (agent.hasPath)
        {
            if (EnemyCanAttackPlayer())
            {
                nextState = new Attack(npc, animator, agent, playerPosition);
                eventStage = EVENTS.EXIT;
            }
            else if (!CanSeePlayer())
            {
                nextState = new Patrol(npc, animator, agent, playerPosition);
                eventStage = EVENTS.EXIT;
            }
        }
    }
    public override void ExitMethod()
    {
        animator.ResetTrigger("isRunning");
        base.ExitMethod();
    }
}
public class Attack : State
{
    float rotationSpeed = 5;
    public Attack(GameObject _npc, Animator _animator, NavMeshAgent _agent, Transform _playerPosition) : base(_npc, _agent, _animator, _playerPosition)
    {
        stateName = STATE.ATTACK;
    }
    public override void EnterMethod()
    {
        animator.SetTrigger("isShooting");
        agent.isStopped = true;
        base.EnterMethod();

    }
    public override void UpdateMethod()
    {
        Vector3 direction = playerPosition.position - npc.transform.position;
        float angle = Vector3.Angle(direction, npc.transform.forward);
        direction.y = 0;
        npc.transform.rotation = Quaternion.Slerp(npc.transform.rotation, Quaternion.LookRotation(direction), rotationSpeed * Time.deltaTime);
        if (EnemyCanAttackPlayer())
        {
            nextState = new Dead(npc, animator, agent, playerPosition);
            eventStage = EVENTS.EXIT;

        }
        /* else if(!CanSeePlayer())
         {
             nextState=new Dead(npc, animator, agent,  playerPosition);
             eventStage = EVENTS.EXIT;
         }
         else if(EnemyCanAttackPlayer())
         {
             nextState = new Dead(npc, animator, agent, playerPosition);
             eventStage = EVENTS.EXIT;
         }*/


    }
    public override void ExitMethod()
    {
        animator.ResetTrigger("isShooting");
        //nextState = new Dead(npc, animator, agent, playerPosition);
        base.ExitMethod();
    }
}
public class Dead : State
{
    public Dead(GameObject _npc, Animator _animator, NavMeshAgent _agent, Transform _playerPosition) : base(_npc, _agent, _animator, _playerPosition)
    {
        stateName = STATE.DEATH;
    }

    public override void EnterMethod()
    {
        Debug.Log("Death enter method");
        animator.SetTrigger("isSleeping");
        // agent.isStopped = true;
        base.EnterMethod();

    }
    public override void UpdateMethod()
    {
        Debug.Log("Death Update method");
        //animator.SetTrigger("isSleeping");
        //Future Update
    }
    public override void ExitMethod()
    {
        Debug.Log("Death exit method");
        animator.ResetTrigger("isSleeping");
        // nextState = new Dead(npc, animator, agent, playerPosition);
        base.ExitMethod();
    }
}