using System.Data;
using Microsoft.Extensions.Configuration;
using Npgsql;

namespace shared.Factories
{
    public interface IConnectionFactory
    {
        public IDbConnection GetConnection();
    }
    public class ConnectionFactory : IConnectionFactory
    {
        private readonly IConfiguration _configuration;

        public ConnectionFactory(IConfiguration configuration)
        {
            _configuration = configuration;
        }


        public IDbConnection GetConnection()
        {
            var connectionString = _configuration.GetConnectionString("postgres");
            return new NpgsqlConnection(connectionString);
        }
    }
}