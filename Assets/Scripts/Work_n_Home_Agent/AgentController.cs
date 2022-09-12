using TMPro;
using UnityEngine;
using UnityEngine.AI;


public class AgentController : MonoBehaviour
{
    public GameObject factoryEntrance;
    public GameObject factoryFloor;
    public GameObject houseEntrance;
    public GameObject sittingRoom;
    public GameObject bedroom;
    public GameObject actionStatus;
    public My_Hybrid_Agent hybridAgent;

    TextMeshPro actionStatusText;
    enum Phase { Planning, Execution, Updating }
    Phase phase;
    NavMeshAgent navMeshAgent;
    bool alreadyPlanning;
    bool alreadyExecuting;
    bool waitingToGetPath;

    void Start()
    {
        navMeshAgent = GetComponent<NavMeshAgent>();

        navMeshAgent.stoppingDistance = 1.9f;

        hybridAgent = new My_Hybrid_Agent();

        phase = Phase.Planning;

        alreadyPlanning = false;
        alreadyExecuting = false;
        waitingToGetPath = false;

        actionStatusText = actionStatus.GetComponent<TextMeshPro>();

        Time.timeScale = 3f;  // used for testing onlyw
    }


    void LateUpdate()
    {
        if (phase == Phase.Planning)
        {
            Debug.Log("----------------------------------");
            Debug.Log("Entered Planning Phase");
            //Debug.Log("CurrentState (before action): " + hybridAgent.CurrentState.ToString());
            //Debug.Log("waitingToGetPath: " + waitingToGetPath);
            //Debug.Log("alreadyPlanning: " + alreadyPlanning);

            if (!waitingToGetPath && !alreadyPlanning)
            {
                alreadyPlanning = true;
                hybridAgent.CurrentAction = hybridAgent.SelectAction(hybridAgent.CurrentState);
                //if(hybridAgent.CurrentAction != null)
                actionStatusText.text = hybridAgent.CurrentAction.ToString();
                //Debug.Log("CurrentAction: " + hybridAgent.CurrentAction);

                // Only navigation actions allowed here
                switch (hybridAgent.CurrentAction)
                {
                    case Action.GoWork:
                        navMeshAgent.SetDestination(factoryEntrance.transform.position);
                        waitingToGetPath = true;  // computation of the path might take longer than one frame
                        break;
                    case Action.GoinFactory:
                        navMeshAgent.SetDestination(factoryFloor.transform.position);
                        waitingToGetPath = true;  // computation of the path might take longer than one frame
                        break;
                    case Action.GoHome:
                        navMeshAgent.SetDestination(houseEntrance.transform.position);
                        waitingToGetPath = true;  // computation of the path might take longer than one frame
                        break;
                    case Action.GoinHouse:
                        navMeshAgent.SetDestination(sittingRoom.transform.position);
                        waitingToGetPath = true;  // computation of the path might take longer than one frame
                        break;
                    case Action.GotoBedroom:
                        navMeshAgent.SetDestination(bedroom.transform.position);
                        waitingToGetPath = true;  // computation of the path might take longer than one frame
                        break;
                }
                alreadyPlanning = false;
            }

            if (hybridAgent.isNavigationAction(hybridAgent.CurrentAction))
            {
                if (navMeshAgent.hasPath)
                {
                    waitingToGetPath = false;
                    phase = Phase.Execution;
                    //Debug.Log("----------------------------------");
                    //Debug.Log("Entered Execution Phase");
                }
            }
            else
            {
                phase = Phase.Execution;
                //Debug.Log("----------------------------------");
                //Debug.Log("Entered Execution Phase");
            }
        }

        if (phase == Phase.Execution)
        {
            if (hybridAgent.isNavigationAction(hybridAgent.CurrentAction))
            {
                //Debug.Log("remainingDistance: " + navMeshAgent.remainingDistance);
                //Debug.Log("hasPath: " + navMeshAgent.hasPath);

                if (navMeshAgent.remainingDistance < Parameters.AT_TARGET_DISTANCE)
                {
                    navMeshAgent.ResetPath();
                    phase = Phase.Updating;
                }
            }
            else if (!alreadyExecuting)
            {
                alreadyExecuting = true;
                // Only non-navigation actions allowed here
                switch (hybridAgent.CurrentAction)
                {
                    case Action.PlayGame:
                        Debug.Log("Playing game");
                        break;
                    case Action.Eat:
                        Debug.Log("Eating");
                        break;
                    case Action.TakeBreak:
                        Debug.Log("Taking break");
                        break;
                    case Action.AssembleWidget:
                        Debug.Log("Assembling widget");
                        break;
                    case Action.Sleep:
                        Debug.Log("Sleeping");
                        break;
                    case Action.No_Op:
                        Debug.Log("Doing nothing");
                        break;
                }
                switch (hybridAgent.CurrentAction)
                {
                    case Action.PlayGame:
                        Invoke("ChangePhaseToUpdateAfterSeconds", 2f);
                        break;
                    case Action.Eat:
                        Invoke("ChangePhaseToUpdateAfterSeconds", 2f);
                        break;
                    case Action.TakeBreak:
                        Invoke("ChangePhaseToUpdateAfterSeconds", 2f);
                        break;
                    case Action.AssembleWidget:
                        Invoke("ChangePhaseToUpdateAfterSeconds", 2f);
                        break;
                    case Action.Sleep:
                        Invoke("ChangePhaseToUpdateAfterSeconds", 10f);
                        break;
                    case Action.No_Op:
                        Invoke("ChangePhaseToUpdateAfterSeconds", 2f);
                        break;
                    default:
                        Invoke("ChangePhaseToUpdateAfterSeconds", 2f);
                        break;
                }
            }
        }

        if (phase == Phase.Updating)
        {
            Debug.Log("----------------------------------");
            Debug.Log("Entered Updating Phase");

            hybridAgent.UpdateDesires(hybridAgent.CurrentAction, hybridAgent.CurrentState);
            hybridAgent.MaintainSatisfactionLevels(hybridAgent.CurrentAction, hybridAgent.CurrentState);
            //hybridAgent.PrintSatLevelsHistory();

            Debug.Log("CurrentState: " + hybridAgent.CurrentState.ToString());
            Debug.Log("CurrentAction: " + hybridAgent.CurrentAction);
            State nextState = Environment.GetRealNextState(hybridAgent.CurrentState, hybridAgent.CurrentAction);
            Observation obs = hybridAgent.GetObservation(hybridAgent.CurrentAction, nextState);
            Debug.Log("Observation: " + obs);
            Debug.Log("NextState: " + nextState.ToString());
            //Observation obs = Environment.GetRealObservation(hybridAgent.CurrentAction, nextState); // applicable when in partially observable domain
            string rmNodeBefore = hybridAgent.RewardMachine.ActiveNode.name;
            Debug.Log("RewardMachine.ActiveNode (before): " + hybridAgent.RewardMachine.ActiveNode.name);
            hybridAgent.RewardMachine.AdvanceActiveNode(obs);
            string rmNodeAfter = hybridAgent.RewardMachine.ActiveNode.name;
            Debug.Log("RewardMachine.ActiveNode (after): " + hybridAgent.RewardMachine.ActiveNode.name);

            if (rmNodeBefore != rmNodeAfter)  // the active node has changed
            {
                if (rmNodeAfter == "make_n_rest")
                    hybridAgent.InitializeIntentions(new System.Collections.Generic.HashSet<Goal> { Goal.EnoughWidgetsMade });
                if (rmNodeAfter == "play_n_eat")
                    hybridAgent.InitializeIntentions(new System.Collections.Generic.HashSet<Goal> { Goal.Satiated });
            }

            hybridAgent.PrintCurrentIntentions();
            hybridAgent.PrintSatLevelsHistory();
            
            if (rmNodeAfter == "make_n_rest" || rmNodeAfter == "play_n_eat")
                hybridAgent.Focus();

            //hybridAgent.PrintCurrentIntentions();
            hybridAgent.CurrentState = nextState;
            phase = Phase.Planning;
        }
    }

    // Used to let non-nav actions last a few seconds
    void ChangePhaseToUpdateAfterSeconds()
    {
        phase = Phase.Updating;
        alreadyExecuting = false;
    }
}

