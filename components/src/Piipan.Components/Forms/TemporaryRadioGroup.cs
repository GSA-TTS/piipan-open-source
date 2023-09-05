using Microsoft.AspNetCore.Components.Forms;

namespace Piipan.Components.Forms
{
    /*
     * This is a temporary workaround for a .NET bug. This class can be removed after updating to .NET 7 where the bug will be fixed.
     * https://github.com/dotnet/aspnetcore/pull/40148
     * There is a bug in .NET currently that when any parameters are set on the Radio Group it re-renders the whole component. 
     * Most often this should not re-render the whole component, but if the Name of the component is changing it warrants a re-render
     */
    public class TemporaryRadioGroup<TValue> : InputRadioGroup<TValue>
    {
        private string? _name;

        protected override void OnParametersSet()
        {
            if (Name != _name)
            {
                _name = Name;
                base.OnParametersSet();
            }
        }
    }
}
