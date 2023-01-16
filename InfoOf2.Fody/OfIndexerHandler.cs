using System;
using System.Collections.Generic;
using System.Linq;
using Fody;
using Mono.Cecil;
using Mono.Cecil.Cil;

public partial class ModuleWeaver
{
    void HandleOfIndexerGet(Instruction instruction, ILProcessor ilProcessor, Dictionary<Instruction, Instruction> offsetMaps, MethodReference ofIndexerGetReference)
    {
        HandleOfIndexer(instruction, ilProcessor, offsetMaps, ofIndexerGetReference, x => x.GetMethod, (_, p) => p);
    }

    void HandleOfIndexerSet(Instruction instruction, ILProcessor ilProcessor, Dictionary<Instruction, Instruction> offsetMaps, MethodReference ofIndexerSetReference)
    {
        HandleOfIndexer(instruction, ilProcessor, offsetMaps, ofIndexerSetReference, x => x.SetMethod, (d, p) => p.Append(d.PropertyType.Name).ToList());
    }

    void HandleOfIndexer(
        Instruction instruction, ILProcessor ilProcessor,
        Dictionary<Instruction, Instruction> offsetMaps, MethodReference propertyReference,
        Func<PropertyDefinition, MethodDefinition> func, Func<PropertyDefinition, List<string>, List<string>> getParameters)
    {
        var indexerNameInstruction = instruction.Previous;
        var parametersInstruction = indexerNameInstruction.Previous;
        var parameters = GetLdString(parametersInstruction)
            .Split(new[] { "," }, StringSplitOptions.RemoveEmptyEntries)
            .Select(x => x.Trim())
            .ToList();

        var propertyName = indexerNameInstruction.Operand as string ?? "Item";

        var typeReferenceData = LoadTypeReference(propertyReference, ilProcessor, parametersInstruction.Previous);
        var typeDefinition = typeReferenceData.TypeReference.Resolve();

        var property = typeDefinition.Properties.FirstOrDefault(x => x.Name == propertyName &&
            (func(x)?.HasSameParams(getParameters(x, parameters)) ?? false));

        if (property == null)
        {
            throw new WeavingException("Could not find indexer.");
        }

        var methodDefinition = func(property);

        if (methodDefinition == null)
        {
            throw new WeavingException("Could not find indexer.");
        }

        ilProcessor.Remove(parametersInstruction);

        MethodReference methodReference;

        if (typeDefinition.HasGenericParameters)
        {
            var typeReference = typeReferenceData.TypeReference as GenericInstanceType;

            methodReference = ModuleDefinition.ImportGenericMethodInstance(methodDefinition, typeReference.GenericArguments.ToArray());
        }
        else
        {
            methodReference = ModuleDefinition.ImportReference(methodDefinition);
        }

        indexerNameInstruction.OpCode = OpCodes.Ldtoken;
        indexerNameInstruction.Operand = methodReference;

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