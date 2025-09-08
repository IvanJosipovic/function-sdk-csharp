using Apiextensions.Fn.Proto.V1;
using Google.Protobuf.WellKnownTypes;

namespace Function.SDK.CSharp;

public static class ResourceExtensions
{
    /// <summary>
    /// Gets the Resource condition by name
    /// </summary>
    /// <param name="resource"></param>
    /// <param name="conditionType"></param>
    /// <returns></returns>
    public static Struct? GetCondition(this Resource resource, string conditionType)
    {
        if (resource.Resource_.Fields.TryGetValue("status", out Value? status))
        {
            if (status.StructValue.Fields.TryGetValue("conditions", out Value? conditions))
            {
                var conditionValues = conditions.ListValue.Values;

                foreach (var condition in conditionValues)
                {
                    if (condition.StructValue.Fields["type"].StringValue == conditionType)
                    {
                        return condition.StructValue;
                    }
                }
            }
        }

        return null;
    }
}
