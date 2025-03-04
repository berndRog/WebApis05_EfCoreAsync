namespace WebApi.Core;

public interface IDataContext {
   Task<bool> SaveAllChangesAsync(string? text = null);
   void ClearChangeTracker();
   void LogChangeTracker(string text);
}