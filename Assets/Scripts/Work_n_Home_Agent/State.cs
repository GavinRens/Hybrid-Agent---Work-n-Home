public class State
{
    public int hunger;          // {0, 1, ..., MAX_HUNGER}
    public int energy;          // {0, 1, ..., MAX_ENERGY}
    public int boredem;         // {0, 1, ..., MAX_BOREDEM}
    public int nuofWidgetsMade; // {0, 1, ..., MAX_NUOF_WIDGETS_MADE}
    public bool atWork;
    public bool atHome;
    public bool traveling;

    public State()
    {
        
    }

    public State(int hu, int en, int bo, int nu, bool aw, bool ah, bool tr)
    {
        hunger = hu;
        energy = en;
        boredem = bo;
        nuofWidgetsMade = nu;
        atWork = aw;
        atHome = ah;
        traveling = tr;
    }

    public string ToString()
    {
        System.Text.StringBuilder sb = new System.Text.StringBuilder();
        sb.Append(hunger + ", " + energy + ", " + boredem + ", " + nuofWidgetsMade + ", " + atWork + ", " + atHome + ", " + traveling);
        return sb.ToString();
    }
}