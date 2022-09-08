
public interface Planner_Interface
{
    /// <summary>
    /// Interface to any planner
    /// </summary>
    /// <param name="currentState">The agent's current state</param>
    /// <returns>The action to be executed</returns>
    public Action SelectAction(State currentState);
}