using System.Collections.Generic;

namespace Abandoned.Core
{
    /// <summary>
    /// Implemented by ScriptableObject configs and definitions so the Editor content validator
    /// can catch impossible values (crouch taller than standing, negative capacity…) before Play.
    /// </summary>
    public interface IValidatable
    {
        void Validate(List<string> errors);
    }
}
