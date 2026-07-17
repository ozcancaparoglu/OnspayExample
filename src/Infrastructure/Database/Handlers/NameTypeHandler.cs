using System.Data;
using Dapper;
using Domain.Orders.ValueObjects;

namespace Infrastructure.Database.Handlers;

internal class NameTypeHandler : SqlMapper.TypeHandler<Name>
{
    public override Name Parse(object value)
    {
        if (value is string strValue)
            return Name.From(strValue);

        throw new ArgumentException("Invalid value for Name type handler.");
    }

    public override void SetValue(IDbDataParameter parameter, Name? value)
    {
        parameter.DbType = DbType.String;
        parameter.Value = value?.Value;
    }
}