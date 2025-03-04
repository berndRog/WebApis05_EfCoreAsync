namespace WebApi.Core;

public interface IDataContext {
   Task<bool> SaveAllChangesAsync();
   void ClearChangeTracker();
   void LogChangeTracker(string text);
}