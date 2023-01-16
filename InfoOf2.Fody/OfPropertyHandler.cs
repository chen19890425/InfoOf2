using System;
using System.Collections.Generic;
using System.Linq;
using Fody;
using Mono.Cecil;
using Mono.Cecil.Cil;

public partial class ModuleWeaver
{
    void HandleOfPropertyGet(Instruction instruction, ILProcessor ilProcessor, Dictionary<Instruction, Instruction> offsetMaps, MethodReference ofPropertyGetReference)
    {
        HandleOfProperty(instruction, ilProcessor, offsetMaps, ofPropertyGetReference, x => x.GetMethod);
    }

    void HandleOfPropertySet(Instruction instruction, ILProcessor ilProcessor, Dictionary<Instruction, Instruction> offsetMaps, MethodReference ofPropertySetReference)
    {
        HandleOfProperty(instruction, ilProcessor, offsetMaps, ofPropertySetReference, x => x.SetMethod);
    }

    void HandleOfProperty(Instruction instruction, ILProcessor ilProcessor, Dictionary<Instruction, Instruction> offsetMaps, MethodReference propertyReference, Func<PropertyDefinition, MethodDefinition> func)
    {
        var propertyNameInstruction = instruction.Previous;
        var propertyName = GetLdString(propertyNameInstruction);

        var typeReferenceData = LoadTypeReference(propertyReference, ilProcessor, propertyNameInstruction.Previous);
        var typeDefinition = typeReferenceData.TypeReference.Resolve();

        var property = typeDefinition.Properties.FirstOrDefault(x => x.Name == propertyName);

        if (property == null)
        {
            throw new WeavingException($"Could not find property named '{propertyName}'.");
        }

        var methodDefinition = func(property);

        if (methodDefinition == null)
        {
            throw new WeavingException($"Could not find property named '{propertyName}'.");
        }

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

        propertyNameInstruction.OpCode = OpCodes.Ldtoken;
        propertyNameInstruction.Operand = methodReference;

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