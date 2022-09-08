using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class My_Hybrid_Agent : Hybrid_Agent
{
    RM_MCTS RM_Planner;
    HMB_MCTS HMB_Planner;
    public List<Goal> play_n_eat_Goals;
    public List<Goal> make_n_rest_Goals;
    

    public My_Hybrid_Agent() : base()
    {
        DefineGoals();
        CategorizeActions("rm");
        SpecifyGoalWeights();
        SpecifyGoalNoncompatabilities();
        foreach (Goal g in Goals)
        {
            DesireLevel.Add(g, 1f);
            SatisfactionLevels.Add(g, new Queue<float>());
        }
        InitializeAgentState();
        InitializeIntentions(new HashSet<Goal> { Goal.EnoughWidgetsMade });
        RM_Planner = new RM_MCTS(this);
        HMB_Planner = new HMB_MCTS(this);
    }

    protected override void CategorizeActions(string context)
    {
        Actions.Clear();

        switch (context)
        {
            case "rm":
                Actions.Add(Action.GoWork);
                Actions.Add(Action.GoinFactory);
                Actions.Add(Action.Make_n_Rest);
                Actions.Add(Action.GoHome);
                Actions.Add(Action.GoinHouse);
                Actions.Add(Action.Play_n_Eat);
                Actions.Add(Action.GotoBedroom);
                Actions.Add(Action.Sleep);
                Actions.Add(Action.No_Op);
                break;
            case "make_n_rest":
                Actions.Add(Action.AssembleWidget);
                Actions.Add(Action.TakeBreak);
                Actions.Add(Action.No_Op);
                break;
            case "play_n_eat":
                Actions.Add(Action.PlayGame);
                Actions.Add(Action.Eat);
                Actions.Add(Action.No_Op);
                break;
        }
    }

    public override RewardMachine DefineRewardMachine()
    {
        RewardMachine rm = new(Agent.Observations);

        var atHome = new rmNode("atHome");
        rm.AddNode(atHome);
        var play_n_eat = new rmNode("play_n_eat");
        rm.AddNode(play_n_eat);
        var goingBedroom = new rmNode("goingBedroom");
        rm.AddNode(goingBedroom);
        var sleeping = new rmNode("sleeping");
        rm.AddNode(sleeping);
        var goingWork = new rmNode("goingWork");
        rm.AddNode(goingWork);
        var atWork = new rmNode("atWork");
        rm.AddNode(atWork);
        var make_n_rest = new rmNode("make_n_rest");
        rm.AddNode(make_n_rest);
        var goingHome = new rmNode("goingHome");
        rm.AddNode(goingHome);
        
        rm.ActiveNode = sleeping;

        rm.AddEdge(atHome, play_n_eat, Observation.HomeResting, 1f);
        rm.AddEdge(play_n_eat, goingBedroom, Observation.GoingToBR, 1f);
        rm.AddEdge(goingBedroom, sleeping, Observation.InBed, 1f);
        rm.AddEdge(sleeping, goingWork, Observation.Traveling, 1f);
        rm.AddEdge(goingWork, atWork, Observation.Arrived, 1f);
        rm.AddEdge(atWork, make_n_rest, Observation.Working, 1f);
        rm.AddEdge(make_n_rest, goingHome, Observation.Traveling, 1f);
        rm.AddEdge(goingHome, atHome, Observation.Arrived, 1f);
        
        return rm;
    }
    
    
    public override List<State> GenerateStates()
    {
        var states = new List<State>();
        for (int hu = 0; hu <= Parameters.MAX_HUNGER; hu++)// hunger;
            for (int en = 0; en <= Parameters.MAX_ENERGY; en++)// energy;
                for (int bo = 0; bo <= Parameters.MAX_BOREDEM; bo++)// boredem;
                    for (int nu = 0; nu <= Parameters.MAX_NUOF_WIDGETS_MADE; nu++)// nuofWidgetsMade;
                    {
                        states.Add(new State(hu, en, bo, nu, true, false, false));// atWork
                        states.Add(new State(hu, en, bo, nu, false, true, false));//atHome
                        states.Add(new State(hu, en, bo, nu, false, false, true));//traveling
                    }
        return states;
    }


    public override bool HasFinished(State state)
    {
        return false;
    }


    public override void InitializeAgentState()
    {
        foreach(State s in States)
            if(s.hunger == 1 && s.energy == 4 && s.boredem == 1 && s.nuofWidgetsMade == 0 && s.atHome == true && s.atWork == false && s.traveling == false) // for 5s
            // if(s.hunger == 2 && s.energy == 8 && s.boredem == 5 && s.nuofWidgetsMade == 0 && s.atHome == true && s.atWork == false && s.traveling == false) // for 10s
            {
                CurrentState = s;
                return;
            }
    }


    public override void DefineGoals()
    {
        // Goals defined via an Enum in separate file

        foreach (Goal name in Enum.GetValues(typeof(Goal)))
            Goals.Add(name);

        // Also specify which goals are applicable to which context
        play_n_eat_Goals = new List<Goal>();
        play_n_eat_Goals.Add(Goal.PlayedEnough);
        play_n_eat_Goals.Add(Goal.Satiated);
        make_n_rest_Goals = new List<Goal>();
        make_n_rest_Goals.Add(Goal.EnoughWidgetsMade);
        make_n_rest_Goals.Add(Goal.Rested);
    }


    public override void SpecifyGoalWeights()
    {
        GoalWeight.Add(Goal.PlayedEnough, 1f); // play games
        GoalWeight.Add(Goal.Satiated, 1f); // eat
        GoalWeight.Add(Goal.EnoughWidgetsMade, 1f); // make widgets
        GoalWeight.Add(Goal.Rested, 1f); // take a break
    }


    public override void InitializeIntentions(HashSet<Goal> intentions)
    {
        Intentions.Clear();
        foreach(Goal g in intentions)
            Intentions.Add(g);
    }


    public override void SpecifyGoalNoncompatabilities()
    {
        // The following spec is simply for testing purposes
        NonCompatibleGoals.Add(Goal.PlayedEnough, new HashSet<Goal> { Goal.Satiated, Goal.EnoughWidgetsMade, Goal.Rested });
        NonCompatibleGoals.Add(Goal.Satiated, new HashSet<Goal> { Goal.PlayedEnough, Goal.EnoughWidgetsMade, Goal.Rested });
        NonCompatibleGoals.Add(Goal.EnoughWidgetsMade, new HashSet<Goal> { Goal.PlayedEnough, Goal.Satiated, Goal.Rested });
        NonCompatibleGoals.Add(Goal.Rested, new HashSet<Goal> { Goal.PlayedEnough, Goal.Satiated, Goal.EnoughWidgetsMade });
    }


    // For general well-being / desire satisfaction.
    // Should be goal-agnostic (??)
    public override float Preference(Action a, State s)
    //public override float Preference(Goal g, Action a, State s)
    {
        if (a == Action.No_Op)
            return 0;

        return 0.1f;
    }


    public override float Cost(Action a, State s)
    {
        return 0.1f;
    }


    public override float Satisfaction(Goal g, Action a, State s)
    {
        // The action does not influence the satisfaction in this definition
        switch (g)
        {
            case Goal.PlayedEnough:
                return (Parameters.MAX_BOREDEM - s.boredem) / (float)Parameters.MAX_BOREDEM;
            case Goal.Satiated:
                return (Parameters.MAX_HUNGER - s.hunger) / (float)Parameters.MAX_HUNGER;
            case Goal.EnoughWidgetsMade:
                //Debug.Log("In Satisfaction, case Goal.EnoughWidgetsMade: " + s.nuofWidgetsMade + " / " + (float)Parameters.MAX_NUOF_WIDGETS_MADE + " = " + s.nuofWidgetsMade / (float)Parameters.MAX_NUOF_WIDGETS_MADE);
                return s.nuofWidgetsMade / (float)Parameters.MAX_NUOF_WIDGETS_MADE;
            case Goal.Rested:
                return s.energy / (float)Parameters.MAX_ENERGY;
            default:
                return 0;
        }
    }
    

    public override (Action, ValueTuple) GetPlan(HashSet<Goal> intentions, State s)
    {
        // Must still test adding written plans that can be used before plan generation
        // (Hand-written plans might be unnecessary)

        Action action = SelectAction(s);
        return (action, ValueTuple.Create());
    }


    // NOTE: Synchronize this function with GetRealNextState() in the environment model
    public override float TransitionFunction(State stateFrom, Action action, State stateTo) // public for Model Validation
    {
        if (action == Action.GoWork)
        {
            if (stateFrom.atHome)
            {
                if (stateTo.traveling && stateFrom.hunger == stateTo.hunger - 1 && stateFrom.energy == stateTo.energy && 
                    stateFrom.boredem == stateTo.boredem && stateTo.nuofWidgetsMade == 0) // new day, new widgets to make
                    return 1;
                else if (stateFrom.hunger == Parameters.MAX_HUNGER && stateFrom == stateTo)
                    return 1;
            }
            else if(stateFrom == stateTo)
                return 1;
        }

        if (action == Action.GoinFactory)
        {
            if (stateFrom.traveling)
            {
                if (stateTo.atWork && stateFrom.hunger == stateTo.hunger && stateFrom.energy == stateTo.energy && 
                    stateFrom.boredem == stateTo.boredem && stateTo.nuofWidgetsMade == 0) // just confirming that nuofWidgetsMade == 0
                    return 1;
            }
            else if (stateFrom == stateTo)
                return 1;
        }

        if (action == Action.AssembleWidget)
        {
            if (stateFrom.atWork && stateTo.atWork)
            {
                // Simple version:
                //if (stateTo.hunger == stateFrom.hunger + 1 && stateTo.energy == stateFrom.energy - 1 &&
                //    stateTo.boredem == stateFrom.boredem + 1 && stateTo.nuofWidgetsMade == stateFrom.nuofWidgetsMade + 1)
                //    return 1;
                //else if ((stateFrom.hunger == Parameters.MAX_HUNGER || stateFrom.energy == 0 || stateFrom.boredem == Parameters.MAX_BOREDEM || stateFrom.nuofWidgetsMade == Parameters.MAX_NUOF_ACTIONS) && stateFrom == stateTo)
                //    return 1;

                // Complex version
                if (stateTo.hunger == stateFrom.hunger + 1 && stateTo.energy == stateFrom.energy - 1 &&
                    stateTo.boredem == stateFrom.boredem + 1 && stateTo.nuofWidgetsMade == stateFrom.nuofWidgetsMade + 1)
                    return 1;
                else if (stateTo.nuofWidgetsMade != stateFrom.nuofWidgetsMade + 1)
                {
                    if (stateTo.hunger == stateFrom.hunger + 1 && stateTo.energy == stateFrom.energy - 1 &&
                    stateTo.boredem == stateFrom.boredem + 1 && stateFrom.hunger == Parameters.MAX_HUNGER && stateTo.hunger == Parameters.MAX_HUNGER)
                        return 1;
                }
                else
                {
                    if ((stateFrom.hunger == Parameters.MAX_HUNGER || stateFrom.energy == 0 || stateFrom.boredem == Parameters.MAX_BOREDEM) && stateFrom == stateTo)
                        return 1;
                }
                
                if ((stateFrom.hunger == Parameters.MAX_HUNGER || stateFrom.energy == 0 || stateFrom.boredem == Parameters.MAX_BOREDEM || stateFrom.nuofWidgetsMade == Parameters.MAX_NUOF_ACTIONS) && stateFrom == stateTo)
                    return 1;

            }
            else if (stateFrom == stateTo)
                return 1;
        }

        if (action == Action.TakeBreak)
        {
            if (stateFrom.atWork && stateTo.atWork)
            {
                if (stateFrom.hunger == stateTo.hunger && stateFrom.energy == stateTo.energy - 1 && 
                    stateFrom.boredem == stateTo.boredem && stateFrom.nuofWidgetsMade == stateTo.nuofWidgetsMade)
                    return 1;
                else if (stateFrom.energy == Parameters.MAX_ENERGY && stateFrom == stateTo)
                    return 1;
            }
            else if (stateFrom == stateTo)
                return 1;
        }

        if (action == Action.GoHome)
        {
            if (stateFrom.atWork)
            {
                if (stateTo.traveling && stateFrom.hunger == stateTo.hunger - 1 && stateFrom.energy == stateTo.energy &&
                    stateFrom.boredem == stateTo.boredem && stateFrom.nuofWidgetsMade == stateTo.nuofWidgetsMade)
                    return 1;
                else if (stateFrom.hunger == Parameters.MAX_HUNGER && stateFrom == stateTo)
                    return 1;
            }
            else if (stateFrom == stateTo)
                return 1;
        }

        if (action == Action.GoinHouse)
        {
            if (stateFrom.traveling)
            {
                if (stateTo.atHome && stateFrom.hunger == stateTo.hunger && stateFrom.energy == stateTo.energy &&
                    stateFrom.boredem == stateTo.boredem && stateFrom.nuofWidgetsMade == stateTo.nuofWidgetsMade)
                    return 1;
            }
            else if (stateFrom == stateTo)
                return 1;
        }

        if (action == Action.PlayGame)
        {
            if (stateFrom.atHome && stateTo.atHome)
            {
                if (stateFrom.hunger == stateTo.hunger && stateFrom.energy == stateTo.energy &&
                    stateFrom.boredem == stateTo.boredem + 1 && stateFrom.nuofWidgetsMade == stateTo.nuofWidgetsMade)
                    return 1;
                else if ((stateFrom.boredem == 0) && stateFrom == stateTo)
                    return 1;
            }
            else if (stateFrom == stateTo)
                return 1;
        }

        if (action == Action.Eat)
        {
            if (stateFrom.atHome && stateTo.atHome)
            {
                if (stateFrom.hunger == stateTo.hunger + 1 && stateFrom.energy == stateTo.energy && 
                    stateFrom.boredem == stateTo.boredem && stateFrom.nuofWidgetsMade == stateTo.nuofWidgetsMade)
                    return 1;
                else if ((stateFrom.hunger == 0) && stateFrom == stateTo)
                    return 1;
            }
            else if (stateFrom == stateTo)
                return 1;
        }

        if (action == Action.GotoBedroom)
        {
            if (stateFrom.atHome && stateTo.atHome)
            {
                if (stateFrom.hunger == stateTo.hunger && stateFrom.energy == stateTo.energy &&
                    stateFrom.boredem == stateTo.boredem && stateFrom.nuofWidgetsMade == stateTo.nuofWidgetsMade)
                    return 1;
            }
            else if (stateFrom == stateTo)
                return 1;
        }

        if (action == Action.Sleep)
        {
            if (stateFrom.atHome && stateTo.atHome)
            {
                //if (stateFrom.hunger == stateTo.hunger - 2 && stateFrom.energy == stateTo.energy - Parameters.MAX_ENERGY / 4)
                if (stateTo.hunger == stateFrom.hunger + 1 && stateTo.energy == stateFrom.energy + 1)
                {
                    if (stateFrom.boredem == stateTo.boredem && stateFrom.nuofWidgetsMade == stateTo.nuofWidgetsMade)
                        return 1;
                }
                else if (stateTo.hunger == stateFrom.hunger + 1)
                {
                    if (stateFrom.energy == Parameters.MAX_ENERGY && stateTo.energy == Parameters.MAX_ENERGY && 
                        stateFrom.boredem == stateTo.boredem && stateFrom.nuofWidgetsMade == stateTo.nuofWidgetsMade)
                        return 1;
                }    
                else if (stateTo.energy == stateFrom.energy + 1)
                {
                    if (stateFrom.hunger == Parameters.MAX_HUNGER && stateTo.hunger == Parameters.MAX_HUNGER &&
                        stateFrom.boredem == stateTo.boredem && stateFrom.nuofWidgetsMade == stateTo.nuofWidgetsMade)
                        return 1;
                }
                else if (stateFrom.energy == Parameters.MAX_ENERGY && stateFrom.hunger == Parameters.MAX_HUNGER && stateFrom == stateTo)
                    return 1;
            }
            else if (stateFrom == stateTo)
                return 1;
        }

        if (action == Action.No_Op || action == Action.Make_n_Rest || action == Action.Play_n_Eat)
            if (stateFrom == stateTo)
                return 1;

        return 0f;
    }


    // Return the observation perceived in (next state) s after performing a
    public override Observation GetObservation(Action action, State state)
    {
        //Arrived, InBed, Traveling, Working, HomeResting, Null

        if (action == Action.GoWork)
        {
            if (state.traveling)
                return Observation.Traveling;
        }

        if (action == Action.GoinFactory)
        {
            if (state.atWork)
                return Observation.Arrived;
        }

        if (action == Action.Make_n_Rest)
        {
            if (state.atWork)
                return Observation.Working;
        }

        if (action == Action.GoHome)
        {
            if (state.traveling)
                return Observation.Traveling;
        }

        if (action == Action.GoinHouse)
        {
            if (state.atHome)
                return Observation.Arrived;
        }

        if (action == Action.Play_n_Eat)
        {
            if (state.atHome)
                return Observation.HomeResting;
        }

        if (action == Action.GotoBedroom)
        {
            if (state.atHome)
                return Observation.GoingToBR;
        }

        if (action == Action.Sleep)
        {
            if (state.atHome)
                return Observation.InBed;
        }

        if (action == Action.No_Op)
            return Observation.Null;

        return Observation.Null;  // all other possibilities produce the null observation
    }
    
    
    public override bool isNavigationAction(Action a)
    {
        switch (a)
        {
            case Action.GoWork: return true;
            case Action.GoinFactory: return true;
            case Action.GoHome: return true;
            case Action.GoinHouse: return true;
            case Action.GotoBedroom: return true;
            default: return false;
        }
    }


    public override void Focus()
    {
        var intentions = new HashSet<Goal>(Intentions);

        // Remove intentions that have been satisfied or are unsatisfiable at the moment
        foreach (Goal inten in intentions)
            if (ShouldRemove(inten))
            {
                SatisfactionLevels[inten].Clear();  // no record of satisfaction levels required for non-intentions
                Intentions.Remove(inten);
            }

        // Find the goal that currently has most intense desire level
        Goal mostIntense = Goal.Rested; // a temporary value
        float mostIntenseLevel = -float.MaxValue;

        // But first norrow down the goals to those that are applicable to the current contect
        List<Goal> applicableGoals;
        if (RewardMachine.ActiveNode.name == "make_n_rest")
            applicableGoals = make_n_rest_Goals;
        else if (RewardMachine.ActiveNode.name == "play_n_eat")
            applicableGoals = play_n_eat_Goals;
        else
            applicableGoals = Goals; // will not be used

        foreach (Goal goal in applicableGoals)
        {
            if (DesireLevel[goal] > mostIntenseLevel)
            {
                mostIntense = goal;
                mostIntenseLevel = DesireLevel[goal];
            }
        }

        // If the most intense goal is not already an intention and there is not an intention in the set that is incompatible w/ the most intense goal
        intentions = new HashSet<Goal>(Intentions);
        intentions.IntersectWith(NonCompatibleGoals[mostIntense]);
        if (!Intentions.Contains(mostIntense) && intentions.Count == 0)
        {
            Intentions.Add(mostIntense);
            SatisfactionLevels[mostIntense].Clear();  // to double-check to start a fresh record of satisfaction levels for mostIntense
        }
    }
    
    
    float timeRestedAtHome = 0;
    float timeAtWork = 0;
    float startRestTime;
    float startWorkTime;
    bool startWorkTimeSet = false;
    bool startRestTimeSet = false;

    public override Action SelectAction(State currentState)
    {
        if (RewardMachine.ActiveNode.name == "play_n_eat")
        {
            Debug.Log("In play_n_eat node");
            if (!startRestTimeSet)
            {
                Debug.Log("not startRestTimeSet");
                startRestTime = Time.time;
                startRestTimeSet = true;
                CategorizeActions("play_n_eat");
            }
            else
                Debug.Log("startRestTimeSet");
            if (timeRestedAtHome < 20f)
            {
                Debug.Log("timeRestedAtHome < 10f: " + timeRestedAtHome);
                Debug.Log("Available actions: " + string.Join(", ", Actions));
                Action a = HMB_Planner.SelectAction(currentState);
                timeRestedAtHome = Time.time - startRestTime;
                return a;
            }
            else
            {
                Debug.Log("timeRestedAtHome >= 10f: " + timeRestedAtHome);
                timeRestedAtHome = 0;
                startRestTimeSet = false;
                return Action.GotoBedroom;
            }
        }
        else if (RewardMachine.ActiveNode.name == "make_n_rest")
        {
            Debug.Log("In make_n_rest node");
            if (!startWorkTimeSet)
            {
                Debug.Log("not startWorkTimeSet");
                startWorkTime = Time.time;
                startWorkTimeSet = true;
                CategorizeActions("make_n_rest");
            }
            else
                Debug.Log("startWorkTimeSet");

            if (timeAtWork < 20f)
            {
                Debug.Log("timeAtWork < 10f: " + timeAtWork);
                Debug.Log("Available actions: " + string.Join(", ", Actions));
                Action a = HMB_Planner.SelectAction(currentState);
                timeAtWork = Time.time - startWorkTime;
                return a;
            }
            else
            {
                Debug.Log("timeAtWork >= 10f: " + timeAtWork);
                timeAtWork = 0;
                startWorkTimeSet = false;
                return Action.GoHome;
            }
        }
        else
        {
            Debug.Log("In other node");
            CategorizeActions("rm");
            return RM_Planner.SelectAction(currentState);
        }
        //return RM_Planner.SelectAction(currentState);  // Run only this line to check whether the reward-machine-based agent behaves as expected
    }


    public void PrintCurrentIntentions()
    {
        Debug.Log("Intentions: " + string.Join(", ", Intentions));
    }


    public void PrintDesireLevels()
    {
        foreach (Goal g in Goals)
            Debug.Log("DesireLevel of " + g.ToString() + ": " + DesireLevel[g].ToString());
    }


    public void PrintSatLevelsHistory()
    {
        foreach (Goal g in Intentions)
        //foreach (Goal g in Goals)
        {
            Debug.Log("SatLevel memory of " + g.ToString());
            Debug.Log(string.Join(", ", SatisfactionLevels[g]));
        }
    }
}
