/// <summary>Toolset interface. Use with ToolsetProvider attribute to create custom toolsets.</summary>
public interface IToolset
{
    void Setup();
    void Teardown();
    void Draw();
}
