using Dapper;
using System.Data;

namespace Playground.VerticalSlice.Application.Shared.Helpers
{
    public class DateOnlyTypeHandler : SqlMapper.TypeHandler<DateOnly>
    {
        public override void SetValue(IDbDataParameter parameter, DateOnly value)
        {
            parameter.DbType = DbType.Date;
            parameter.Value = value.ToDateTime(TimeOnly.MinValue);
        }

        public override DateOnly Parse(object value)
            => DateOnly.FromDateTime((DateTime)value);
    }
}
