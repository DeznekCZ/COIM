// The source-linked parser files carry a few stray/unused `using` directives that point at
// namespaces which do not exist on a modern .NET target (UnityEngine, Mafi.Core.Factory.Lifts,
// and the .NET-Framework-only System.Runtime.Remoting.Contexts). Nothing is actually *used* from
// them. Declaring each namespace with a dummy internal type makes the `using` resolve so the
// unchanged source compiles. Editing the originals would defeat the "single source of truth" goal.

namespace UnityEngine
{
    internal sealed class _RuntimeShim { }

    // InExpression.cs has a stray `using static UnityEngine.GraphicsBuffer;`, so this must be a TYPE.
    internal static class GraphicsBuffer { }
}

namespace Mafi.Core.Factory.Lifts
{
    internal sealed class _RuntimeShim { }
}

namespace System.Runtime.Remoting.Contexts
{
    internal sealed class _RuntimeShim { }
}
