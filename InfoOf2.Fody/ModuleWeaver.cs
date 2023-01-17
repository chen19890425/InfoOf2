using System.Linq;
using Fody;
using Mono.Cecil;

public partial class ModuleWeaver : BaseModuleWeaver
{
    public override bool ShouldCleanReference => true;

    public ModuleWeaver()
    {
        
    }

    public override void Execute()
    {
        var allTypes = ModuleDefinition.GetTypes().ToList();

        FindReferences();
        ProcessMethods(allTypes);
        CleanReferences();
    }
}