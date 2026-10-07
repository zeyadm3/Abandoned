namespace Abandoned.Extraction
{
    public enum RunPhase : byte
    {
        /// <summary>Looting; the truck waits.</summary>
        Running,
        /// <summary>Someone started the truck: it honks, then leaves with whoever is inside.</summary>
        Honking,
        /// <summary>The truck left; the appraisal takes over.</summary>
        Departed,
    }
}
