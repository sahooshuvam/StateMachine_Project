using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.AI;

public class GameManager : MonoBehaviour
{
    NavMeshAgent agent;
    Animator animator;
    public Transform player;
    State currentState;

    // Start is called before the first frame update
    void Start()
    {
        agent = GetComponent<NavMeshAgent>();
        animator = GetComponent<Animator>();

    }

    // Update is called once per frame
    void Update()
    {
        
    }
}

public class State
{
    public enum STATE
    {
        IDLE,PATROL,RUN,ATTACK,DEATH
    }

    public enum EVENTS
    {
        ENTRY,UPDATE,EXIT
    }

    public STATE stateName;
    public EVENTS eventStage;

    public GameObject npc;
    public Transform playerPosition;
    public NavMeshAgent navMeshAgent;
    public Animator anim;


    public State nextState;

    float visualDistance;//if my player in the distance from the npc 
    float visualAngle;
    float shootingDistance;

    public  State(GameObject _npc,NavMeshAgent _agent,Animator _anim,Transform _playerPosition)
    {
        this.npc = _npc;
        this.playerPosition = _playerPosition;
        this.anim = _anim;
        this.navMeshAgent = _agent;
        eventStage = EVENTS.ENTRY;
    }

    public virtual void Enter()
    {
        eventStage = EVENTS.UPDATE;
    }
    public virtual void Update()
    {
        eventStage = EVENTS.UPDATE;
    }
    public virtual void Exit()
    {
        eventStage = EVENTS.EXIT;
    }

    public State Process()// keep on the running in the outside and keep on checking the process
    {
        if (eventStage == EVENTS.ENTRY)
        {
            Enter();
        } 
        if (eventStage == EVENTS.UPDATE)
        {
            Update();
        }
        if (eventStage == EVENTS.EXIT)
        {
            Exit();
            return nextState;
        }
        return this;
    }

    public bool CanSeePlayer()
    {
        Vector3 direction = playerPosition.position - npc.transform.position;
        float angle = Vector3.Angle(direction, npc.transform.forward);
        if (direction.magnitude < visualDistance&& angle < visualAngle)
        {
            return true;
        }
        return false;
    }
    public bool EnemyCanAttackPlayer()
    {
        Vector3 direction = playerPosition.position - npc.transform.position;
        if (direction.magnitude <shootingDistance)
        {
            return true;
        }
        return false;
    }
}


public class Idle : State
{ 
    public Idle(GameObject _npc, NavMeshAgent _agent, Animator _anim, Transform _playerPosition) : base(_npc,_agent,_anim,_playerPosition)
    {
        stateName = STATE.IDLE;        
    }

    public override void Enter()
    {
        anim.SetTrigger("isIdle");
        base.Enter();
    } 
    public override void Update()
    {
        if (Random.Range(0,100) <5)
        {
            nextState = new Patrol(npc, navMeshAgent, anim, playerPosition);
            eventStage = EVENTS.EXIT;
        }
        base.Update();
    }
    public override void Exit()
    {
        anim.ResetTrigger("isIdle");
        base.Exit();
    }
}
public class Patrol : State
{
    int currentIndex = -1;
    public Patrol(GameObject _npc, NavMeshAgent _agent, Animator _anim, Transform _playerPosition) : base(_npc,_agent,_anim,_playerPosition)
    {
        stateName = STATE.PATROL;
        this.navMeshAgent.speed = 2;
        this.navMeshAgent.isStopped = false;
    }

    public override void Enter()
    {
        anim.SetTrigger("isWalking");
        currentIndex = 0;
        base.Enter();
    } 
    public override void Update()
    {
        if (navMeshAgent.remainingDistance < 1f)
        {
            if (currentIndex >= GameController.Instance.CheckPoints.Count) 
            {
                currentIndex = 0;
            }
            else
            {
                currentIndex++;
            }
            navMeshAgent.SetDestination(GameController.Instance.CheckPoints[currentIndex].transform.position);
            nextState = new Patrol(npc, navMeshAgent, anim, playerPosition);
            eventStage = EVENTS.EXIT;
        }
        base.Update();
    }
    public override void Exit()
    {
        anim.ResetTrigger("isIdle");
        base.Exit();
    }
}

public class Chase : State
{
    int currentIndex = -1;
    public Chase(GameObject _npc, NavMeshAgent _agent, Animator _anim, Transform _playerPosition) : base(_npc, _agent, _anim, _playerPosition)
    {
        stateName = STATE.RUN;
        this.navMeshAgent.speed = 5f;
        this.navMeshAgent.isStopped = false;
    }

    public override void Enter()
    {
        anim.SetTrigger("isRunning");
        base.Enter();
    }
    public override void Update()
    {
        navMeshAgent.SetDestination(playerPosition.position);
        if (navMeshAgent.hasPath < )
        {
            if (EnemyCanAttackPlayer())
            {
                nextState = STATE.ATTACK;
            }
            else
            {
                currentIndex++;
            }
            navMeshAgent.SetDestination(GameController.Instance.CheckPoints[currentIndex].transform.position);
            nextState = new Patrol(npc, navMeshAgent, anim, playerPosition);
            eventStage = EVENTS.EXIT;
        }
        base.Update();
    }
    public override void Exit()
    {
        anim.ResetTrigger("isIdle");
        base.Exit();
    }
}