using MudBlazor;

namespace DexRobotPDA.Utilities;

public class InputField
{
    public int Index { get; set; }
    public string Label { get; set; } = string.Empty;
    public string Id { get; set; } = string.Empty;
    
    public MudTextField<string>? FieldRef { get; set; }
}