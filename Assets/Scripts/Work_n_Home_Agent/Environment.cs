
public class Environment
{
    /// <summary>
    /// The state the agent will end up in if it executes the action in the current state
    /// This is the `ground truth', not a model of what is expected <see cref="Agent.GetNextState(Action, State)"/>
    /// </summary>
    /// <param name="action">An action</param>
    /// <param name="currentState">An environment state</param>
    /// <returns>A successor state</returns>
    public static State GetRealNextState(State currentState, Action action)
    {        
        if (action == Action.GoWork)
        {
            foreach (State s in Agent.States)
                if (currentState.atHome)
                {
                    if (s.traveling && currentState.hunger == s.hunger - 1 &&
                    currentState.energy == s.energy && currentState.boredem == s.boredem && s.nuofWidgetsMade == 0) // new day, new widgets to make
                        return s;
                    else if (currentState.hunger == Parameters.MAX_HUNGER && currentState == s)
                        return s;
                }
        }

        if (action == Action.GoinFactory)
        {
            foreach (State s in Agent.States)
                if (currentState.traveling)
                {
                    if (s.atWork && currentState.hunger == s.hunger &&
                    currentState.energy == s.energy && currentState.boredem == s.boredem && s.nuofWidgetsMade == 0) // new day, new widgets to make
                        return s;
                }
        }

        if (action == Action.AssembleWidget)
        {
            foreach (State s in Agent.States)
            {
                // Simple version:
                //if (currentState.atWork)
                //{
                //    if (s.atWork && currentState.hunger == s.hunger - 1 && currentState.energy == s.energy + 1 &&
                //        currentState.boredem == s.boredem - 1 && currentState.nuofWidgetsMade == s.nuofWidgetsMade - 1)
                //        return s;
                //    else if ((currentState.hunger == Parameters.MAX_HUNGER || currentState.energy == 0 || currentState.boredem == Parameters.MAX_BOREDEM || currentState.nuofWidgetsMade == Parameters.MAX_NUOF_WIDGETS_MADE) && currentState == s)
                //        return s;
                //}

                // Complex version
                if (currentState.atWork && s.atWork)
                {
                    if (s.hunger == currentState.hunger + 1 && s.energy == currentState.energy - 1 &&
                    s.boredem == currentState.boredem + 1 && s.nuofWidgetsMade == currentState.nuofWidgetsMade + 1)
                        return s;
                    else if (s.nuofWidgetsMade != currentState.nuofWidgetsMade + 1)
                    {
                        if (s.hunger == currentState.hunger + 1 && s.energy == currentState.energy - 1 &&
                        s.boredem == currentState.boredem + 1 && currentState.hunger == Parameters.MAX_HUNGER && s.hunger == Parameters.MAX_HUNGER)
                            return s;
                    }
                    else
                    {
                        if ((currentState.hunger == Parameters.MAX_HUNGER || currentState.energy == 0 || currentState.boredem == Parameters.MAX_BOREDEM) && currentState == s)
                            return s;
                    }

                    if ((currentState.hunger == Parameters.MAX_HUNGER || currentState.energy == 0 || currentState.boredem == Parameters.MAX_BOREDEM || currentState.nuofWidgetsMade == Parameters.MAX_NUOF_ACTIONS) && currentState == s)
                        return s;
                }
            }
        }

        if (action == Action.TakeBreak)
        {
            foreach (State s in Agent.States)
                if (currentState.atWork)
                {
                    if (s.atWork && currentState.hunger == s.hunger &&
                    currentState.energy == s.energy - 1 && currentState.boredem == s.boredem && currentState.nuofWidgetsMade == s.nuofWidgetsMade)
                        return s;
                    else if ((currentState.energy == Parameters.MAX_ENERGY) && currentState == s)
                        return s;
                }
        }

        if (action == Action.GoHome)
        {
            foreach (State s in Agent.States)
                if (currentState.atWork)
                {
                    if (s.traveling && currentState.hunger == s.hunger - 1 &&
                    currentState.energy == s.energy && currentState.boredem == s.boredem && currentState.nuofWidgetsMade == s.nuofWidgetsMade)
                        return s;
                    else if (currentState.hunger == Parameters.MAX_HUNGER && currentState == s)
                        return s;
                }
        }

        if (action == Action.GoinHouse)
        {
            foreach (State s in Agent.States)
                if (currentState.traveling)
                {
                    if (s.atHome && currentState.hunger == s.hunger &&
                    currentState.energy == s.energy && currentState.boredem == s.boredem && currentState.nuofWidgetsMade == s.nuofWidgetsMade)
                        return s;
                }
        }

        if (action == Action.PlayGame)
        {
            foreach (State s in Agent.States)
                if (currentState.atHome)
                {
                    if (s.atHome && currentState.hunger == s.hunger &&
                    currentState.energy == s.energy && currentState.boredem == s.boredem + 1 && currentState.nuofWidgetsMade == s.nuofWidgetsMade)
                        return s;
                    else if ((currentState.boredem == 0) && currentState == s)
                        return s;
                }
        }

        if (action == Action.Eat)
        {
            foreach (State s in Agent.States)
                if (currentState.atHome)
                {
                    if (s.atHome && currentState.hunger == s.hunger + 1 &&
                    currentState.energy == s.energy && currentState.boredem == s.boredem && currentState.nuofWidgetsMade == s.nuofWidgetsMade)
                        return s;
                    else if ((currentState.hunger == 0) && currentState == s)
                        return s;
                }
        }

        if (action == Action.GotoBedroom)
        {
            foreach (State s in Agent.States)
                if (currentState.atHome)
                {
                    if (s.atHome && currentState.hunger == s.hunger &&
                    currentState.energy == s.energy && currentState.boredem == s.boredem && currentState.nuofWidgetsMade == s.nuofWidgetsMade)
                        return s;
                }
        }

        if (action == Action.Sleep)
        {
            foreach (State s in Agent.States)
            {
                // Simple version
                //if (currentState.atHome)
                //{
                //    if (s.atHome && currentState.hunger == s.hunger + 1 &&
                //currentState.energy == s.energy - Parameters.MAX_ENERGY / 4 && currentState.boredem == s.boredem && currentState.nuofWidgetsMade == s.nuofWidgetsMade)
                //        return s;
                //    else if ((currentState.energy == Parameters.MAX_ENERGY || currentState.hunger == 0) && currentState == s)
                //        return s;
                //}

                // Complex version
                if (currentState.atHome && s.atHome)
                {
                    //if (currentState.hunger == s.hunger - 2 && currentState.energy == s.energy - Parameters.MAX_ENERGY / 4)
                    if (s.hunger == currentState.hunger + 1 && s.energy == currentState.energy + 1)
                    {
                        if (currentState.boredem == s.boredem && currentState.nuofWidgetsMade == s.nuofWidgetsMade)
                            return s;
                    }
                    else if (s.hunger == currentState.hunger + 1)
                    {
                        if (currentState.energy == Parameters.MAX_ENERGY && s.energy == Parameters.MAX_ENERGY &&
                            currentState.boredem == s.boredem && currentState.nuofWidgetsMade == s.nuofWidgetsMade)
                            return s;
                    }
                    else if (s.energy == currentState.energy + 1)
                    {
                        if (currentState.hunger == Parameters.MAX_HUNGER && s.hunger == Parameters.MAX_HUNGER &&
                            currentState.boredem == s.boredem && currentState.nuofWidgetsMade == s.nuofWidgetsMade)
                            return s;
                    }
                    else if (currentState.energy == Parameters.MAX_ENERGY && currentState.hunger == Parameters.MAX_HUNGER && currentState == s)
                        return s;
                }
                else if (currentState == s)
                    return s;
            }
        }

        return currentState;
    }
}
