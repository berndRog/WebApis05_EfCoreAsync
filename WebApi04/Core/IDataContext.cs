namespace WebApi.Core;

public interface IDataContext {
   Task<bool> SaveAllChangesAsync(
      string? text = null, CancellationToken ctToken = default);
   void ClearChangeTracker();
   void LogChangeTracker(string text);
}