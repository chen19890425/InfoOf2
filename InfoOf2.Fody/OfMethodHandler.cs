using System;
using System.Collections.Generic;
using System.Linq;
using Mono.Cecil;
using Mono.Cecil.Cil;

public partial class ModuleWeaver
{
    void HandleOfMethod(Instruction instruction, ILProcessor ilProcessor, Dictionary<Instruction, Instruction> offsetMaps, MethodReference ofMethodReference)
    {
        //Info.OfMethod("AssemblyToProcess","MethodClass","InstanceMethod");

        Instruction methodNameInstruction;
        List<string> parameters;
        Instruction parametersInstruction = null;
        if (ofMethodReference.Parameters.Count is 2 or 4)
        {
            parametersInstruction = instruction.Previous;
            parameters = GetLdString(parametersInstruction)
                .Split(new[] { "," }, StringSplitOptions.RemoveEmptyEntries)
                .Select(x => x.Trim())
                .ToList();
            methodNameInstruction = parametersInstruction.Previous;
        }
        else
        {
            methodNameInstruction = instruction.Previous;
            parameters = new List<string>();
        }

        var methodName = GetLdString(methodNameInstruction);

        var typeReferenceData = LoadTypeReference(ofMethodReference, ilProcessor, methodNameInstruction.Previous);
        var typeDefinition = typeReferenceData.TypeReference.Resolve();

        var method = typeDefinition.FindMethodDefinitions(methodName, parameters);

        MethodReference methodReference;

        if (typeDefinition.HasGenericParameters)
        {
            var typeReference = typeReferenceData.TypeReference as GenericInstanceType;

            methodReference = ModuleDefinition.ImportGenericMethodInstance(method, typeReference.GenericArguments.ToArray());
        }
        else
        {
            methodReference = ModuleDefinition.ImportReference(method);
        }

        if (parametersInstruction != null)
        {
            ilProcessor.Remove(parametersInstruction);
        }

        methodNameInstruction.OpCode = OpCodes.Ldtoken;
        methodNameInstruction.Operand = methodReference;

        if (typeDefinition.HasGenericParameters)
        {
            ilProcessor.InsertBefore(instruction, Instruction.Create(OpCodes.Ldtoken, typeReferenceData.TypeReference));
            instruction.Operand = getMethodFromHandleGeneric;

            offsetMaps[instruction] = instruction.Previous.Previous;
        }
        else
        {
            instruction.Operand = getMethodFromHandle;

            offsetMaps[instruction] = instruction.Previous;
        }

        ilProcessor.InsertAfter(instruction, Instruction.Create(OpCodes.Castclass, methodInfoType));
    }
}