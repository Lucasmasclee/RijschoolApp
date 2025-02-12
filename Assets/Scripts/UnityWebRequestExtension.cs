using UnityEngine.Networking;
using System.Runtime.CompilerServices;
using System.Threading.Tasks;

public static class UnityWebRequestExtension
{
    public static TaskAwaiter<UnityWebRequest> GetAwaiter(this UnityWebRequestAsyncOperation operation)
    {
        var tcs = new TaskCompletionSource<UnityWebRequest>();
        operation.completed += asyncOp => tcs.SetResult(((UnityWebRequestAsyncOperation)asyncOp).webRequest);
        return tcs.Task.GetAwaiter();
    }
} 