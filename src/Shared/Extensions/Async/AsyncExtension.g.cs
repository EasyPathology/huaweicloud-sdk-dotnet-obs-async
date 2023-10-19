using OBS;
using OBS.Model;

namespace OBS.Extensions;

public static partial class AsyncExtension
{
	public static Task<AppendObjectResponse> AppendObjectAsync(this ObsClient client, 
        AppendObjectRequest request, 
        object state, 
        CancellationToken? token = null)
	{
        var source = new TaskCompletionSource<AppendObjectResponse>();
        var ar     = client.BeginAppendObject(request, ar => { source.SetResult(client.EndAppendObject(ar)); }, state);
        token?.Register(() =>
        {
            client.EndAppendObject(ar);
            source.SetCanceled();
        });

        return source.Task;
	}

	public static Task<AbortMultipartUploadResponse> AbortMultipartUploadAsync(this ObsClient client, 
        AbortMultipartUploadRequest request, 
        object state, 
        CancellationToken? token = null)
	{
        var source = new TaskCompletionSource<AbortMultipartUploadResponse>();
        var ar     = client.BeginAbortMultipartUpload(request, ar => { source.SetResult(client.EndAbortMultipartUpload(ar)); }, state);
        token?.Register(() =>
        {
            client.EndAbortMultipartUpload(ar);
            source.SetCanceled();
        });

        return source.Task;
	}

	public static Task<CopyObjectResponse> CopyObjectAsync(this ObsClient client, 
        CopyObjectRequest request, 
        object state, 
        CancellationToken? token = null)
	{
        var source = new TaskCompletionSource<CopyObjectResponse>();
        var ar     = client.BeginCopyObject(request, ar => { source.SetResult(client.EndCopyObject(ar)); }, state);
        token?.Register(() =>
        {
            client.EndCopyObject(ar);
            source.SetCanceled();
        });

        return source.Task;
	}

	public static Task<CopyPartResponse> CopyPartAsync(this ObsClient client, 
        CopyPartRequest request, 
        object state, 
        CancellationToken? token = null)
	{
        var source = new TaskCompletionSource<CopyPartResponse>();
        var ar     = client.BeginCopyPart(request, ar => { source.SetResult(client.EndCopyPart(ar)); }, state);
        token?.Register(() =>
        {
            client.EndCopyPart(ar);
            source.SetCanceled();
        });

        return source.Task;
	}

	public static Task<CompleteMultipartUploadResponse> CompleteMultipartUploadAsync(this ObsClient client, 
        CompleteMultipartUploadRequest request, 
        object state, 
        CancellationToken? token = null)
	{
        var source = new TaskCompletionSource<CompleteMultipartUploadResponse>();
        var ar     = client.BeginCompleteMultipartUpload(request, ar => { source.SetResult(client.EndCompleteMultipartUpload(ar)); }, state);
        token?.Register(() =>
        {
            client.EndCompleteMultipartUpload(ar);
            source.SetCanceled();
        });

        return source.Task;
	}

	public static Task<DeleteObjectResponse> DeleteObjectAsync(this ObsClient client, 
        DeleteObjectRequest request, 
        object state, 
        CancellationToken? token = null)
	{
        var source = new TaskCompletionSource<DeleteObjectResponse>();
        var ar     = client.BeginDeleteObject(request, ar => { source.SetResult(client.EndDeleteObject(ar)); }, state);
        token?.Register(() =>
        {
            client.EndDeleteObject(ar);
            source.SetCanceled();
        });

        return source.Task;
	}

	public static Task<DeleteObjectsResponse> DeleteObjectsAsync(this ObsClient client, 
        DeleteObjectsRequest request, 
        object state, 
        CancellationToken? token = null)
	{
        var source = new TaskCompletionSource<DeleteObjectsResponse>();
        var ar     = client.BeginDeleteObjects(request, ar => { source.SetResult(client.EndDeleteObjects(ar)); }, state);
        token?.Register(() =>
        {
            client.EndDeleteObjects(ar);
            source.SetCanceled();
        });

        return source.Task;
	}

	public static Task<GetObjectResponse> GetObjectAsync(this ObsClient client, 
        GetObjectRequest request, 
        object state, 
        CancellationToken? token = null)
	{
        var source = new TaskCompletionSource<GetObjectResponse>();
        var ar     = client.BeginGetObject(request, ar => { source.SetResult(client.EndGetObject(ar)); }, state);
        token?.Register(() =>
        {
            client.EndGetObject(ar);
            source.SetCanceled();
        });

        return source.Task;
	}

	public static Task<GetObjectAclResponse> GetObjectAclAsync(this ObsClient client, 
        GetObjectAclRequest request, 
        object state, 
        CancellationToken? token = null)
	{
        var source = new TaskCompletionSource<GetObjectAclResponse>();
        var ar     = client.BeginGetObjectAcl(request, ar => { source.SetResult(client.EndGetObjectAcl(ar)); }, state);
        token?.Register(() =>
        {
            client.EndGetObjectAcl(ar);
            source.SetCanceled();
        });

        return source.Task;
	}

	public static Task<GetObjectMetadataResponse> GetObjectMetadataAsync(this ObsClient client, 
        GetObjectMetadataRequest request, 
        object state, 
        CancellationToken? token = null)
	{
        var source = new TaskCompletionSource<GetObjectMetadataResponse>();
        var ar     = client.BeginGetObjectMetadata(request, ar => { source.SetResult(client.EndGetObjectMetadata(ar)); }, state);
        token?.Register(() =>
        {
            client.EndGetObjectMetadata(ar);
            source.SetCanceled();
        });

        return source.Task;
	}

	public static Task<bool> HeadObjectAsync(this ObsClient client, 
        HeadObjectRequest request, 
        object state, 
        CancellationToken? token = null)
	{
        var source = new TaskCompletionSource<bool>();
        var ar     = client.BeginHeadObject(request, ar => { source.SetResult(client.EndHeadObject(ar)); }, state);
        token?.Register(() =>
        {
            client.EndHeadObject(ar);
            source.SetCanceled();
        });

        return source.Task;
	}

	public static Task<InitiateMultipartUploadResponse> InitiateMultipartUploadAsync(this ObsClient client, 
        InitiateMultipartUploadRequest request, 
        object state, 
        CancellationToken? token = null)
	{
        var source = new TaskCompletionSource<InitiateMultipartUploadResponse>();
        var ar     = client.BeginInitiateMultipartUpload(request, ar => { source.SetResult(client.EndInitiateMultipartUpload(ar)); }, state);
        token?.Register(() =>
        {
            client.EndInitiateMultipartUpload(ar);
            source.SetCanceled();
        });

        return source.Task;
	}

	public static Task<ListPartsResponse> ListPartsAsync(this ObsClient client, 
        ListPartsRequest request, 
        object state, 
        CancellationToken? token = null)
	{
        var source = new TaskCompletionSource<ListPartsResponse>();
        var ar     = client.BeginListParts(request, ar => { source.SetResult(client.EndListParts(ar)); }, state);
        token?.Register(() =>
        {
            client.EndListParts(ar);
            source.SetCanceled();
        });

        return source.Task;
	}

	public static Task<PutObjectResponse> PutObjectAsync(this ObsClient client, 
        PutObjectRequest request, 
        object state, 
        CancellationToken? token = null)
	{
        var source = new TaskCompletionSource<PutObjectResponse>();
        var ar     = client.BeginPutObject(request, ar => { source.SetResult(client.EndPutObject(ar)); }, state);
        token?.Register(() =>
        {
            client.EndPutObject(ar);
            source.SetCanceled();
        });

        return source.Task;
	}

	public static Task<RestoreObjectResponse> RestoreObjectAsync(this ObsClient client, 
        RestoreObjectRequest request, 
        object state, 
        CancellationToken? token = null)
	{
        var source = new TaskCompletionSource<RestoreObjectResponse>();
        var ar     = client.BeginRestoreObject(request, ar => { source.SetResult(client.EndRestoreObject(ar)); }, state);
        token?.Register(() =>
        {
            client.EndRestoreObject(ar);
            source.SetCanceled();
        });

        return source.Task;
	}

	public static Task<SetObjectAclResponse> SetObjectAclAsync(this ObsClient client, 
        SetObjectAclRequest request, 
        object state, 
        CancellationToken? token = null)
	{
        var source = new TaskCompletionSource<SetObjectAclResponse>();
        var ar     = client.BeginSetObjectAcl(request, ar => { source.SetResult(client.EndSetObjectAcl(ar)); }, state);
        token?.Register(() =>
        {
            client.EndSetObjectAcl(ar);
            source.SetCanceled();
        });

        return source.Task;
	}

	public static Task<UploadPartResponse> UploadPartAsync(this ObsClient client, 
        UploadPartRequest request, 
        object state, 
        CancellationToken? token = null)
	{
        var source = new TaskCompletionSource<UploadPartResponse>();
        var ar     = client.BeginUploadPart(request, ar => { source.SetResult(client.EndUploadPart(ar)); }, state);
        token?.Register(() =>
        {
            client.EndUploadPart(ar);
            source.SetCanceled();
        });

        return source.Task;
	}

}