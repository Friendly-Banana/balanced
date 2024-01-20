using System;
using System.Net;
using Mirror;
using Mirror.Discovery;
using UnityEngine;

public struct ServerRequest : NetworkMessage
{
}

public class ServerResponse : NetworkMessage
{
    // The server that sent this
    // this is a property so that it is not serialized, but the
    // client fills this up after we receive it
    public IPEndPoint EndPoint { get; set; }

    public Uri uri;

    // Prevent duplicate server appearance when a connection can be made via LAN on multiple NICs
    public long serverId;
    public string serverName;
}

[DisallowMultipleComponent]
[AddComponentMenu("Network/NetworkDiscovery")]
public class MyND : NetworkDiscoveryBase<ServerRequest, ServerResponse>
{
    #region Server

    /// <summary>
    /// Process the request from a client
    /// </summary>
    /// <remarks>
    /// Override if you wish to provide more information to the clients
    /// such as the name of the host player
    /// </remarks>
    /// <param name="request">Request coming from client</param>
    /// <param name="endpoint">Address of the client that sent the request</param>
    /// <returns>The message to be sent back to the client or null</returns>
    protected override ServerResponse ProcessRequest(ServerRequest request, IPEndPoint endpoint)
    {
        try
        {
            return new ServerResponse
            {
                serverId = ServerId,
                uri = transport.ServerUri(),
                serverName = Utils.PlayerName,
            };
        }
        catch (NotImplementedException)
        {
            Debug.LogError($"Transport {transport} does not support network discovery");
            throw;
        }
    }

    #endregion

    #region Client

    /// <summary>
    /// Process the answer from a server
    /// </summary>
    /// <remarks>
    /// A client receives a reply from a server, this method processes the
    /// reply and raises an event
    /// </remarks>
    /// <param name="response">Response that came from the server</param>
    /// <param name="endpoint">Address of the server that replied</param>
    protected override void ProcessResponse(ServerResponse response, IPEndPoint endpoint)
    {
        // we received a message from the remote endpoint
        response.EndPoint = endpoint;

        // although we got a supposedly valid url, we may not be able to resolve
        // the provided host
        // However we know the real ip address of the server because we just
        // received a packet from it, so use that as host.
        UriBuilder realUri = new UriBuilder(response.uri)
        {
            Host = response.EndPoint.Address.ToString()
        };
        response.uri = realUri.Uri;

        OnServerFound.Invoke(response);
    }

    #endregion
}