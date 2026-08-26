using System.Data;
using Dapper;

namespace JobTracker.App.Infrastructure.Data
{
    public class SqliteGuidTypeHandler : SqlMapper.TypeHandler<Guid>
    {
        // Cuando Dapper guarda en BD, lo pasa a string
        public override void SetValue(IDbDataParameter parameter, Guid value)
        {
            parameter.Value = value.ToString();
        }

        // Cuando Dapper lee de la BD, lo convierte de string a Guid
        public override Guid Parse(object value)
        {
            if (value is string strGuid)
            {
                return Guid.Parse(strGuid);
            }
            else if (value is byte[] bytes) // Por si SQLite lo guarda como binario
            {
                return new Guid(bytes);
            }

            return Guid.Empty;
        }
    }
}
