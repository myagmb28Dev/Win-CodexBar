using CodexBar.Core.Providers.Codex;

var state = CodexSessionStateReader.ReadLatestState();
Console.WriteLine(state is null ? "state=null" : $"active={state.ActiveModel?.DisplayName ?? "null"} models={state.Models.Count}");
if (state is not null)
{
    foreach (var model in state.Models)
    {
        Console.WriteLine($"{model.ModelName} current={model.Current?.UsedPercent} weekly={model.Weekly?.UsedPercent}");
    }
}
