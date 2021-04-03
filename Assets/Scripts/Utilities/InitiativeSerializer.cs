using Mirror;

public static class InitiativeSerializer
{
    public static void WriteInitiative(this NetworkWriter writer, Initiative initiative)
    {
        writer.WriteGameObject(initiative.obj);
        writer.WriteInt32(initiative.initiative);
    }

    public static Initiative ReadInitiative(this NetworkReader reader)
    {
        return new Initiative(reader.ReadGameObject(), reader.ReadInt32());
    }
}
