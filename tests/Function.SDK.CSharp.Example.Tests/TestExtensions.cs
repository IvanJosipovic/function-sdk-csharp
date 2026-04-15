using Apiextensions.Fn.Proto.V1;
using Google.Protobuf;
using Google.Protobuf.WellKnownTypes;
using Grpc.Core;
using Grpc.Core.Testing;
using Grpc.Core.Utils;
using k8s;
using Microsoft.Extensions.Logging;

namespace Function.SDK.CSharp.Example.Tests;

public static class TestExtensions
{
    public static RunFunctionRequest GetFunctionRequest()
    {
        var request = new RunFunctionRequest
        {
            Observed = new()
            {
                Composite = new()
            },
            Desired = new(),
            Context = new(),
        };

        return request;
    }

    public static RunFunctionResponse GetTestResponse(this RunFunctionRequest request)
    {
        var svc = new RunFunctionService(new LoggerFactory().CreateLogger<RunFunctionService>());
        var fakeServerCallContext = TestServerCallContext.Create("/apiextensions.fn.proto.v1.FunctionRunnerService/RunFunction", null, DateTime.UtcNow.AddHours(1), [], CancellationToken.None, "127.0.0.1", null, null, (metadata) => TaskUtils.CompletedTask, () => new WriteOptions(), (writeOptions) => { });

        return svc.RunFunction(request, fakeServerCallContext)
            .GetAwaiter()
            .GetResult();
    }

    public static void SetCompositeResource(this RunFunctionRequest request, IKubernetesObject obj)
    {
        var kubeObj = Struct.Parser.ParseJson(KubernetesJson.Serialize(obj));
        request.Observed.Composite.Resource_ = kubeObj;
    }

    public static T GetResource<T>(this State state, string key)
    {
        var json = JsonFormatter.Default.Format(state.Resources[key].Resource_);

        return KubernetesJson.Deserialize<T>(json);
    }
}
