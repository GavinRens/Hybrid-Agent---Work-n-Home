using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public interface PO_Agent_Interface
{
    /// <summary>
    /// Keeps track of the agent's current state
    /// </summary>
    public Dictionary<State, float> CurrentBelief { get; set; }// { get { return currentBelief; } set { currentBelief = value; } }

    /// <summary>
    /// A function defining the observation model
    /// </summary>
    /// <param name="z">Perceived observation</param>
    /// <param name="a">Action associated with the observation</param>
    /// <param name="to">State reached due to the action</param>
    /// <returns>The probability that an observation perceived in 'to', given the action that caused the agent to be there </returns>
    public float ObservationFunction(Observation z, Action a, State to);

    /// <summary>
    /// Defines how a belief should be updated
    /// </summary>
    /// <param name="z">Perceived observation</param>
    /// <param name="a">Action associated with the observation</param>
    /// <param name="b">The current belief - a probability distribution over states</param>
    /// <returns></returns>
    public Dictionary<State, float> GetNextBelief(Observation z, Action a, Dictionary<State, float> b);

    /// <summary>
    /// Sets the belief that the agent will start in
    /// </summary>
    public void InitializeAgentBelief();
}
