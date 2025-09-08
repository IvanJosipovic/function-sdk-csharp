using Apiextensions.Fn.Proto.V1;
using Google.Protobuf.WellKnownTypes;
using k8s;

namespace Function.SDK.CSharp;

public static class StateExtensions
{
    /// <summary>
    /// Adds Resource or merges with an exiting one
    /// </summary>
    /// <param name="state"></param>
    /// <param name="key"></param>
    /// <param name="obj"></param>
    public static void AddOrUpdate(this State state, string key, IKubernetesObject obj)
    {
        var kubeObj = Struct.Parser.ParseJson(KubernetesJson.Serialize(obj));

        if (state.Resources.TryGetValue(key, out Resource? value))
        {
            value.Resource_.MergeFrom(kubeObj);
        }
        else
        {
            state.Resources[key] = new()
            {
                Resource_ = kubeObj
            };
        }
    }
}