namespace Kinetix.Monitoring
{
    public interface IMonitoringStore
    {
        /// <summary>
        /// Enregistre la fin d'un process
        /// </summary>
        /// <param name="process"></param>
        void AddProcess(Process process);

        /// <summary>
        /// Enregistre le démarrage d'un process
        /// </summary>
        /// <param name="process"></param>
        void StartProcess(Process process);
    }
}
