public enum StatusType
{
    Bleed
}

public class StatusEffect
{
    public StatusType type;
    public int potency;
    public int count;

    public StatusEffect(StatusType type, int potency, int count)
    {
        this.type = type;
        this.potency = potency;
        this.count = count;
    }
}
