using System.Collections.Generic;
using System.Reflection;
using Mono.Cecil;
using Mono.Cecil.Cil;

namespace InfoOf2.Fody
{
    internal static class CecilExtensions
    {
        private static readonly FieldInfo SequencePointOffsetFieldInfo = typeof(SequencePoint).GetField("offset", BindingFlags.Instance | BindingFlags.NonPublic);
        private static readonly FieldInfo InstructionOffsetInstructionFieldInfo = typeof(InstructionOffset).GetField("instruction", BindingFlags.Instance | BindingFlags.NonPublic);

        public static void UpdateDebugInfo(this MethodDefinition method, List<SequencePoint> sequencePoints, Dictionary<Instruction, Instruction> offsetMaps)
        {
            var debugInfo = method.DebugInformation;

            debugInfo.SequencePoints.Clear();

            foreach (var sequencePoint in sequencePoints)
            {
                var instructionOffset = (InstructionOffset)SequencePointOffsetFieldInfo.GetValue(sequencePoint);
                var offsetInstruction = (Instruction)InstructionOffsetInstructionFieldInfo.GetValue(instructionOffset);

                if (offsetMaps.TryGetValue(offsetInstruction, out var newOffsetInstruction))
                {
                    var newSequencePoint = new SequencePoint(newOffsetInstruction, sequencePoint.Document)
                    {
                        StartLine = sequencePoint.StartLine,
                        StartColumn = sequencePoint.StartColumn,
                        EndLine = sequencePoint.EndLine,
                        EndColumn = sequencePoint.EndColumn
                    };

                    debugInfo.SequencePoints.Add(newSequencePoint);
                }
                else
                {
                    debugInfo.SequencePoints.Add(sequencePoint);
                }
            }
        }
    }
}