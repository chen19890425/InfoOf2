using System.Collections.Generic;
using System.Linq;
using Mono.Cecil;

public partial class ModuleWeaver
{
    public void ProcessMethods(List<TypeDefinition> allTypes)
    {
        foreach (var type in allTypes)
        {
            foreach (var method in type.Methods.Where(x => x.HasBody))
            {
                ProcessMethod(method);
            }
        }
    }
}