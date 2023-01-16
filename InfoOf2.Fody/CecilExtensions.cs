using System.Collections.Generic;
using Mono.Cecil;
using Mono.Cecil.Cil;

namespace InfoOf2.Fody
{
    internal static class CecilExtensions
    {
        public static void UpdateDebugInfo(this MethodDefinition method, List<SequencePoint> sequencePoints)
        {
            var debugInfo = method.DebugInformation;

            debugInfo.SequencePoints.Clear();

            foreach (var sequencePoint in sequencePoints)
            {
                debugInfo.SequencePoints.Add(sequencePoint);
            }
        }
    }
}