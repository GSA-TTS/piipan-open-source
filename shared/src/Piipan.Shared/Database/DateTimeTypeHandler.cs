using NodaTime;
using System;
using System.Data;

namespace Piipan.Shared.Database
{
    /// <summary>
    /// This class provides a Dapper TypeHandler for DateTime. The standard Handler doesn't support NodaTime types
    /// and the TypeHandlers provided by the NodaTime library do not provide an alternative
    /// https://github.com/mattjohnsonpint/Dapper-NodaTime/tree/master/src/Dapper.NodaTime
    /// </summary>
    public class DateTimeTypeHandler : Dapper.SqlMapper.TypeHandler<DateTime>
    {
        // <summary>
        // Implement custom parsing to handle different types that convert to DateTime
        // </summary>
        public override DateTime Parse(object value)
        {
            return value switch
            {
                LocalDate v => v.ToDateTimeUnspecified(),
                string v => DateTime.Parse(v),
                Instant v => v.ToDateTimeUtc(),
                DateTime v => v,
                _ => throw new DataException($"Unable to convert {value} to DateTime")
            };
        }

        // <summary>
        // Format value DateTime into required NodaTime input format
        // from a DateTime.
        // </summary>
        public override void SetValue(IDbDataParameter parameter, DateTime value)
        {
            if (value.Kind == DateTimeKind.Utc)
            {
                parameter.Value = Instant.FromDateTimeUtc((DateTime)value);
            }
            else
            {
                parameter.Value = LocalDateTime.FromDateTime((DateTime)value);
            }
        }
    }
}
