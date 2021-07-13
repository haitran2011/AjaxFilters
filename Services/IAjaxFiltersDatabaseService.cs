namespace Spikes.Nop.Plugins.AjaxFilters.Services
{
	public interface IAjaxFiltersDatabaseService
	{
		void CreateDatabaseScripts();

		void UpdateDatabaseScripts();

		void RemoveDatabaseScripts();
	}
}
