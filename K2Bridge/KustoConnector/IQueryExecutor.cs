namespace K2Bridge.KustoConnector
{
    using System.Data;

    public interface IQueryExecutor
    {
        ElasticResponse ExecuteQuery(string query);

        IDataReader ExecuteControlCommand(string query);
    }
}