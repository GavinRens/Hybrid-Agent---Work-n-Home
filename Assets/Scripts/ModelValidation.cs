using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class ModelValidation : MonoBehaviour
{
    Hybrid_Agent agent;  // Change agent type as applicable
    HashSet<Action> actions;

    void Start()
    {
        agent = new My_Hybrid_Agent();  // Change agent type as applicable
        actions = new HashSet<Action>();
        foreach (Action a in System.Enum.GetValues(typeof(Action)))
            actions.Add(a);

        Debug.Log("-------- START TRANS FUNC VALIDATION --------");

        foreach (State s in Agent.States)
            foreach (Action a in actions)
            {
                float mass = 0;
                foreach (State ss in Agent.States)
                {
                    mass += agent.TransitionFunction(s, a, ss);
                }
                if (mass != 1f)
                    Debug.Log(s.ToString() + ", " + a + ", " + mass);
            }

        Debug.Log("-------- END TRANS FUNC VALIDATION --------");

    }
}
