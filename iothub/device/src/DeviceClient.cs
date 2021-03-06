// Copyright (c) Microsoft. All rights reserved.
// Licensed under the MIT license. See LICENSE file in the project root for full license information.

using System;
using System.Collections.Generic;
using System.IO;
using System.Threading;
using System.Threading.Tasks;
using Microsoft.Azure.Devices.Shared;

namespace Microsoft.Azure.Devices.Client
{
    /// <summary>
    /// Contains methods that a device can use to send messages to and receive from the service.
    /// </summary>
    public sealed class DeviceClient : IDisposable
    {
        /// <summary>
        /// Default operation timeout.
        /// </summary>
        public const uint DefaultOperationTimeoutInMilliseconds = 4 * 60 * 1000;

        private DeviceClient(InternalClient internalClient)
        {
            InternalClient = internalClient ?? throw new ArgumentNullException(nameof(internalClient));

            if (InternalClient.IotHubConnectionString?.ModuleId != null)
            {
                throw new ArgumentException("A module ID was specified in the connection string - please use ModuleClient for modules.");
            }

            if (Logging.IsEnabled) Logging.Associate(this, this, internalClient, nameof(DeviceClient));
        }

        /// <summary>
        /// Creates a disposable, Amqp DeviceClient from the specified parameters
        /// </summary>
        /// <param name="hostname">The fully-qualified DNS hostname of IoT Hub</param>
        /// <param name="authenticationMethod">The authentication method that is used</param>
        /// <returns>A disposable DeviceClient instance</returns>
        public static DeviceClient Create(string hostname, IAuthenticationMethod authenticationMethod)
        {
            return Create(() => ClientFactory.Create(hostname, authenticationMethod));
        }

        /// <summary>
        /// Create a disposable, Amqp DeviceClient from the specified parameters
        /// </summary>
        /// <param name="hostname">The fully-qualified DNS hostname of IoT Hub</param>
        /// <param name="authenticationMethod">The authentication method that is used</param>
        /// <param name="gatewayHostname">The fully-qualified DNS hostname of Gateway</param>
        /// <returns>A disposable DeviceClient instance</returns>
        public static DeviceClient Create(string hostname, string gatewayHostname, IAuthenticationMethod authenticationMethod)
        {
            return Create(() => ClientFactory.Create(hostname, gatewayHostname, authenticationMethod));
        }

        /// <summary>
        /// Creates a disposable DeviceClient from the specified parameters
        /// </summary>
        /// <param name="hostname">The fully-qualified DNS hostname of IoT Hub</param>
        /// <param name="authenticationMethod">The authentication method that is used</param>
        /// <param name="transportType">The transportType used (Http1 or Amqp)</param>
        /// <returns>A disposable DeviceClient instance</returns>
        public static DeviceClient Create(string hostname, IAuthenticationMethod authenticationMethod, TransportType transportType)
        {
            return Create(() => ClientFactory.Create(hostname, authenticationMethod, transportType));
        }

        /// <summary>
        /// Create a disposable DeviceClient from the specified parameters
        /// </summary>
        /// <param name="hostname">The fully-qualified DNS hostname of IoT Hub</param>
        /// <param name="gatewayHostname">The fully-qualified DNS hostname of Gateway</param>
        /// <param name="authenticationMethod">The authentication method that is used</param>
        /// <param name="transportType">The transportType used (Http1 or Amqp)</param>
        /// <returns>A disposable DeviceClient instance</returns>
        public static DeviceClient Create(string hostname, string gatewayHostname, IAuthenticationMethod authenticationMethod, TransportType transportType)
        {
            return Create(() => ClientFactory.Create(hostname, gatewayHostname, authenticationMethod, transportType));
        }

        /// <summary>
        /// Creates a disposable DeviceClient from the specified parameters
        /// </summary>
        /// <param name="hostname">The fully-qualified DNS hostname of IoT Hub</param>
        /// <param name="authenticationMethod">The authentication method that is used</param>
        /// <param name="transportSettings">Prioritized list of transportTypes and their settings</param>
        /// <returns>A disposable DeviceClient instance</returns>
        public static DeviceClient Create(string hostname, IAuthenticationMethod authenticationMethod,
            ITransportSettings[] transportSettings)
        {
            return Create(() => ClientFactory.Create(hostname, authenticationMethod, transportSettings));
        }

        /// <summary>
        /// Creates a disposable DeviceClient from the specified parameters
        /// </summary>
        /// <param name="hostname">The fully-qualified DNS hostname of IoT Hub</param>
        /// <param name="gatewayHostname">The fully-qualified DNS hostname of Gateway</param>
        /// <param name="authenticationMethod">The authentication method that is used</param>
        /// <param name="transportSettings">Prioritized list of transportTypes and their settings</param>
        /// <returns>A disposable DeviceClient instance</returns>
        public static DeviceClient Create(string hostname, string gatewayHostname, IAuthenticationMethod authenticationMethod,
            ITransportSettings[] transportSettings)
        {
            return Create(() => ClientFactory.Create(hostname, gatewayHostname, authenticationMethod, transportSettings));
        }

        /// <summary>
        /// Creates a disposable DeviceClient using Amqp transport from the specified connection string
        /// </summary>
        /// <param name="connectionString">Connection string for the IoT hub (including DeviceId)</param>
        /// <returns>A disposable DeviceClient instance</returns>
        public static DeviceClient CreateFromConnectionString(string connectionString)
        {
            return Create(() => ClientFactory.CreateFromConnectionString(connectionString));
        }

        /// <summary>
        /// Creates a disposable DeviceClient using Amqp transport from the specified connection string
        /// </summary>
        /// <param name="connectionString">IoT Hub-Scope Connection string for the IoT hub (without DeviceId)</param>
        /// <param name="deviceId">Id of the Device</param>
        /// <returns>A disposable DeviceClient instance</returns>
        public static DeviceClient CreateFromConnectionString(string connectionString, string deviceId)
        {
            return Create(() => ClientFactory.CreateFromConnectionString(connectionString, deviceId));
        }

        /// <summary>
        /// Creates a disposable DeviceClient from the specified connection string using the specified transport type
        /// </summary>
        /// <param name="connectionString">Connection string for the IoT hub (including DeviceId)</param>
        /// <param name="transportType">Specifies whether Amqp or Http transport is used</param>
        /// <returns>A disposable DeviceClient instance</returns>
        public static DeviceClient CreateFromConnectionString(string connectionString, TransportType transportType)
        {
            return Create(() => ClientFactory.CreateFromConnectionString(connectionString, transportType));
        }

        /// <summary>
        /// Creates a disposable DeviceClient from the specified connection string using the specified transport type
        /// </summary>
        /// <param name="connectionString">IoT Hub-Scope Connection string for the IoT hub (without DeviceId)</param>
        /// <param name="deviceId">Id of the device</param>
        /// <param name="transportType">The transportType used (Http1 or Amqp)</param>
        /// <returns>A disposable DeviceClient instance</returns>
        public static DeviceClient CreateFromConnectionString(string connectionString, string deviceId, TransportType transportType)
        {
            return Create(() => ClientFactory.CreateFromConnectionString(connectionString, deviceId, transportType));
        }

        /// <summary>
        /// Create a disposable DeviceClient from the specified connection string using a prioritized list of transports
        /// </summary>
        /// <param name="connectionString">Connection string for the IoT hub (with DeviceId)</param>
        /// <param name="transportSettings">Prioritized list of transports and their settings</param>
        /// <returns>A disposable DeviceClient instance</returns>
        public static DeviceClient CreateFromConnectionString(string connectionString,
            ITransportSettings[] transportSettings)
        {
            return Create(() => ClientFactory.CreateFromConnectionString(connectionString, transportSettings));
        }

        /// <summary>
        /// Creates a disposable DeviceClient from the specified connection string using the prioritized list of transports
        /// </summary>
        /// <param name="connectionString">Connection string for the IoT hub (without DeviceId)</param>
        /// <param name="deviceId">Id of the device</param>
        /// <param name="transportSettings">Prioritized list of transportTypes and their settings</param>
        /// <returns>A disposable DeviceClient instance</returns>
        public static DeviceClient CreateFromConnectionString(string connectionString, string deviceId,
            ITransportSettings[] transportSettings)
        {
            return Create(() => ClientFactory.CreateFromConnectionString(connectionString, deviceId, transportSettings));
        }

        private static DeviceClient Create(Func<InternalClient> internalClientCreator)
        {
            return new DeviceClient(internalClientCreator());
        }

        internal IDelegatingHandler InnerHandler
        {
            get => InternalClient.InnerHandler;
            set => InternalClient.InnerHandler = value;
        }

        internal InternalClient InternalClient { get; private set; }

        /// <summary>
        /// Diagnostic sampling percentage value, [0-100];
        /// 0 means no message will carry on diag info
        /// </summary>
        public int DiagnosticSamplingPercentage
        {
            get => InternalClient.DiagnosticSamplingPercentage;
            set => InternalClient.DiagnosticSamplingPercentage = value;
        }

        /// <summary>
        /// Stores the timeout used in the operation retries. Note that this value is ignored for operations
        /// where a cancellation token is provided. For example, SendEventAsync(Message) will use this timeout, but
        /// SendEventAsync(Message, CancellationToken) will not. The latter operation will only be canceled by the
        /// provided cancellation token.
        /// </summary>
        // Codes_SRS_DEVICECLIENT_28_002: [This property shall be defaulted to 240000 (4 minutes).]
        public uint OperationTimeoutInMilliseconds
        {
            get => InternalClient.OperationTimeoutInMilliseconds;
            set => InternalClient.OperationTimeoutInMilliseconds = value;
        }

        /// <summary>
        /// Stores custom product information that will be appended to the user agent string that is sent to IoT Hub.
        /// </summary>
        public string ProductInfo
        {
            get => InternalClient.ProductInfo;
            set => InternalClient.ProductInfo = value;
        }

        /// <summary>
        /// Stores the retry strategy used in the operation retries.
        /// </summary>
        // Codes_SRS_DEVICECLIENT_28_001: [This property shall be defaulted to the exponential retry strategy with backoff
        // parameters for calculating delay in between retries.]
        [Obsolete("This method has been deprecated.  Please use Microsoft.Azure.Devices.Client.SetRetryPolicy(IRetryPolicy retryPolicy) instead.")]
        public RetryPolicyType RetryPolicy
        {
            get => InternalClient.RetryPolicy;
            set => InternalClient.RetryPolicy = value;
        }

        /// <summary>
        /// Sets the retry policy used in the operation retries.
        /// The change will take effect after any in-progress operations.
        /// </summary>
        /// <param name="retryPolicy">The retry policy. The default is new ExponentialBackoff(int.MaxValue, TimeSpan.FromMilliseconds(100), TimeSpan.FromSeconds(10), TimeSpan.FromMilliseconds(100));</param>
        // Codes_SRS_DEVICECLIENT_28_001: [This property shall be defaulted to the exponential retry strategy with backoff
        // parameters for calculating delay in between retries.]
        public void SetRetryPolicy(IRetryPolicy retryPolicy)
        {
            InternalClient.SetRetryPolicy(retryPolicy);
        }

        /// <summary>
        /// Explicitly open the DeviceClient instance.
        /// </summary>
        public Task OpenAsync() => InternalClient.OpenAsync();

        /// <summary>
        /// Explicitly open the DeviceClient instance.
        /// <param name="cancellationToken">A cancellation token to cancel the operation.</param>
        /// <exception cref="OperationCanceledException">Thrown when the operation has been canceled.</exception>
        /// </summary>
        public Task OpenAsync(CancellationToken cancellationToken) => InternalClient.OpenAsync(cancellationToken);

        /// <summary>
        /// Close the DeviceClient instance
        /// </summary>
        public Task CloseAsync() => InternalClient.CloseAsync();

        /// <summary>
        /// Close the DeviceClient instance
        /// </summary>
        /// <param name="cancellationToken">A cancellation token to cancel the operation.</param>
        /// <exception cref="OperationCanceledException">Thrown when the operation has been canceled.</exception>
        /// <returns></returns>
        public Task CloseAsync(CancellationToken cancellationToken) => InternalClient.CloseAsync(cancellationToken);

        /// <summary>
        /// Receive a message from the device queue using the default timeout. After handling a received message, a client should call Complete, Abandon, or Reject, and then dispose the message.
        /// </summary>
        /// <returns>The receive message or null if there was no message until the default timeout</returns>
        public Task<Message> ReceiveAsync() => InternalClient.ReceiveAsync();

        /// <summary>
        /// Receive a message from the device queue using the cancellation token. After handling a received message, a client should call Complete, Abandon, or Reject, and then dispose the message.
        /// </summary>
        /// <param name="cancellationToken">A cancellation token to cancel the operation.</param>
        /// <exception cref="OperationCanceledException">Thrown when the operation has been canceled.</exception>
        /// <returns>The receive message or null if there was no message until CancellationToken Expired</returns>
        public Task<Message> ReceiveAsync(CancellationToken cancellationToken) => InternalClient.ReceiveAsync(cancellationToken);

        /// <summary>
        /// Receive a message from the device queue with the specified timeout. After handling a received message, a client should call Complete, Abandon, or Reject, and then dispose the message.
        /// </summary>
        /// <returns>The receive message or null if there was no message until the specified time has elapsed</returns>
        public Task<Message> ReceiveAsync(TimeSpan timeout) => InternalClient.ReceiveAsync(timeout);

        /// <summary>
        /// Deletes a received message from the device queue
        /// </summary>
        /// <param name="lockToken">The message lockToken.</param>
        /// <returns>The lock identifier for the previously received message</returns>
        public Task CompleteAsync(string lockToken) => InternalClient.CompleteAsync(lockToken);

        /// <summary>
        /// Deletes a received message from the device queue
        /// </summary>
        /// <param name="lockToken">The message lockToken.</param>
        /// <param name="cancellationToken">A cancellation token to cancel the operation.</param>
        /// <returns>The lock identifier for the previously received message</returns>
        /// <exception cref="OperationCanceledException">Thrown when the operation has been canceled.</exception>
        public Task CompleteAsync(string lockToken, CancellationToken cancellationToken) => InternalClient.CompleteAsync(lockToken, cancellationToken);

        /// <summary>
        /// Deletes a received message from the device queue
        /// </summary>
        /// <param name="message">The message.</param>
        /// <returns>The previously received message</returns>
        public Task CompleteAsync(Message message) => InternalClient.CompleteAsync(message);

        /// <summary>
        /// Deletes a received message from the device queue
        /// </summary>
        /// <param name="message">The message.</param>
        /// <param name="cancellationToken">A cancellation token to cancel the operation.</param>
        /// <exception cref="OperationCanceledException">Thrown when the operation has been canceled.</exception>
        /// <returns>The previously received message</returns>
        public Task CompleteAsync(Message message, CancellationToken cancellationToken) => InternalClient.CompleteAsync(message, cancellationToken);

        /// <summary>
        /// Puts a received message back onto the device queue
        /// </summary>
        /// <param name="lockToken">The message lockToken.</param>
        /// <returns>The previously received message</returns>
        public Task AbandonAsync(string lockToken) => InternalClient.AbandonAsync(lockToken);

        /// <summary>
        /// Puts a received message back onto the device queue
        /// </summary>
        /// <param name="lockToken">The message lockToken.</param>
        /// <param name="cancellationToken">A cancellation token to cancel the operation.</param>
        /// <exception cref="OperationCanceledException">Thrown when the operation has been canceled.</exception>
        /// <returns>The previously received message</returns>
        public Task AbandonAsync(string lockToken, CancellationToken cancellationToken) => InternalClient.AbandonAsync(lockToken, cancellationToken);

        /// <summary>
        /// Puts a received message back onto the device queue
        /// </summary>
        /// <param name="message">The message.</param>
        /// <returns>The lock identifier for the previously received message</returns>
        public Task AbandonAsync(Message message) => InternalClient.AbandonAsync(message);

        /// <summary>
        /// Puts a received message back onto the device queue
        /// </summary>
        /// <param name="message">The message.</param>
        /// <param name="cancellationToken">A cancellation token to cancel the operation.</param>
        /// <exception cref="OperationCanceledException">Thrown when the operation has been canceled.</exception>
        /// <returns>The lock identifier for the previously received message</returns>
        public Task AbandonAsync(Message message, CancellationToken cancellationToken) => InternalClient.AbandonAsync(message, cancellationToken);

        /// <summary>
        /// Deletes a received message from the device queue and indicates to the server that the message could not be processed.
        /// </summary>
        /// <param name="lockToken">The message lockToken.</param>
        /// <returns>The previously received message</returns>
        public Task RejectAsync(string lockToken) => InternalClient.RejectAsync(lockToken);

        /// <summary>
        /// Deletes a received message from the device queue and indicates to the server that the message could not be processed.
        /// </summary>
        /// <param name="cancellationToken">A cancellation token to cancel the operation.</param>
        /// <param name="lockToken">The message lockToken.</param>
        /// <exception cref="OperationCanceledException">Thrown when the operation has been canceled.</exception>
        /// <returns>The previously received message</returns>
        public Task RejectAsync(string lockToken, CancellationToken cancellationToken) => InternalClient.RejectAsync(lockToken, cancellationToken);

        /// <summary>
        /// Deletes a received message from the device queue and indicates to the server that the message could not be processed.
        /// </summary>
        /// <param name="message">The message.</param>
        /// <returns>The lock identifier for the previously received message</returns>
        public Task RejectAsync(Message message) => InternalClient.RejectAsync(message);

        /// <summary>
        /// Deletes a received message from the device queue and indicates to the server that the message could not be processed.
        /// </summary>
        /// <param name="message">The message.</param>
        /// <param name="cancellationToken">A cancellation token to cancel the operation.</param>
        /// <exception cref="OperationCanceledException">Thrown when the operation has been canceled.</exception>
        /// <returns>The lock identifier for the previously received message</returns>
        public Task RejectAsync(Message message, CancellationToken cancellationToken) => InternalClient.RejectAsync(message, cancellationToken);

        /// <summary>
        /// Sends an event to a hub
        /// </summary>
        /// <param name="message">The message to send. Should be disposed after sending.</param>
        /// <returns>The task to await</returns>
        public Task SendEventAsync(Message message) => InternalClient.SendEventAsync(message);

        /// <summary>
        /// Sends an event to a hub
        /// </summary>
        /// <param name="message">The message to send. Should be disposed after sending.</param>
        /// <param name="cancellationToken">A cancellation token to cancel the operation.</param>
        /// <exception cref="OperationCanceledException">Thrown when the operation has been canceled.</exception>
        /// <returns>The task to await</returns>
        public Task SendEventAsync(Message message, CancellationToken cancellationToken) => InternalClient.SendEventAsync(message, cancellationToken);

        /// <summary>
        /// Sends a batch of events to a hub. Requires AMQP or AMQP over WebSockets.
        /// </summary>
        /// <param name="messages">A list of one or more messages to send. The messages should be disposed after sending.</param>
        /// <returns>The task to await</returns>
        public Task SendEventBatchAsync(IEnumerable<Message> messages) => InternalClient.SendEventBatchAsync(messages);

        /// <summary>
        /// Sends a batch of events to device hub. Requires AMQP or AMQP over WebSockets.
        /// </summary>
        /// <param name="messages">An IEnumerable set of Message objects.</param>
        /// <param name="cancellationToken">A cancellation token to cancel the operation.</param>
        /// <exception cref="OperationCanceledException">Thrown when the operation has been canceled.</exception>
        /// <returns>The task to await</returns>
        public Task SendEventBatchAsync(IEnumerable<Message> messages, CancellationToken cancellationToken) => InternalClient.SendEventBatchAsync(messages, cancellationToken);

        /// <summary>
        /// Uploads a stream to a block blob in a storage account associated with the IoTHub for that device.
        /// If the blob already exists, it will be overwritten. If a proxy is set in the first transport settings of this
        /// client, then this blob upload will use that same proxy
        /// </summary>
        /// <param name="blobName">The name of the blob to upload.</param>
        /// <param name="source">A stream with blob contents. Should be disposed after upload completes.</param>
        /// <returns>AsncTask</returns>
        public Task UploadToBlobAsync(string blobName, Stream source) => InternalClient.UploadToBlobAsync(blobName, source);

        /// <summary>
        /// Uploads a stream to a block blob in a storage account associated with the IoTHub for that device.
        /// If the blob already exists, it will be overwritten. If a proxy is set in the first transport settings of this
        /// client, then this blob upload will use that same proxy
        /// </summary>
        /// <param name="blobName">The name of the blob to upload</param>
        /// <param name="source">A stream with blob contents.. Should be disposed after upload completes.</param>
        /// <param name="cancellationToken">A cancellation token to cancel the operation.</param>
        /// <exception cref="OperationCanceledException">Thrown when the operation has been canceled.</exception>
        /// <returns>The task to await</returns>
        public Task UploadToBlobAsync(string blobName, Stream source, CancellationToken cancellationToken) =>
            InternalClient.UploadToBlobAsync(blobName, source, cancellationToken);

        /// <summary>
        /// Registers a new delegate for the named method. If a delegate is already associated with
        /// the named method, it will be replaced with the new delegate.
        /// <param name="methodName">The name of the method to associate with the delegate.</param>
        /// <param name="methodHandler">The delegate to be used when a method with the given name is called by the cloud service.</param>
        /// <param name="userContext">generic parameter to be interpreted by the client code.</param>
        /// </summary>
        public Task SetMethodHandlerAsync(string methodName, MethodCallback methodHandler, object userContext) =>
            InternalClient.SetMethodHandlerAsync(methodName, methodHandler, userContext);

        /// <summary>
        /// Registers a new delegate for the named method. If a delegate is already associated with
        /// the named method, it will be replaced with the new delegate.
        /// <param name="methodName">The name of the method to associate with the delegate.</param>
        /// <param name="methodHandler">The delegate to be used when a method with the given name is called by the cloud service.</param>
        /// <param name="userContext">generic parameter to be interpreted by the client code.</param>
        /// <param name="cancellationToken">A cancellation token to cancel the operation.</param>
        /// <exception cref="OperationCanceledException">Thrown when the operation has been canceled.</exception>
        /// </summary>
        public Task SetMethodHandlerAsync(string methodName, MethodCallback methodHandler, object userContext, CancellationToken cancellationToken) =>
            InternalClient.SetMethodHandlerAsync(methodName, methodHandler, userContext, cancellationToken);

        /// <summary>
        /// Registers a new delegate that is called for a method that doesn't have a delegate registered for its name.
        /// If a default delegate is already registered it will replace with the new delegate.
        /// </summary>
        /// <param name="methodHandler">The delegate to be used when a method is called by the cloud service and there is no delegate registered for that method name.</param>
        /// <param name="userContext">Generic parameter to be interpreted by the client code.</param>
        public Task SetMethodDefaultHandlerAsync(MethodCallback methodHandler, object userContext) =>
            InternalClient.SetMethodDefaultHandlerAsync(methodHandler, userContext);

        /// <summary>
        /// Registers a new delegate that is called for a method that doesn't have a delegate registered for its name.
        /// If a default delegate is already registered it will replace with the new delegate.
        /// </summary>
        /// <param name="methodHandler">The delegate to be used when a method is called by the cloud service and there is no delegate registered for that method name.</param>
        /// <param name="userContext">Generic parameter to be interpreted by the client code.</param>
        /// <param name="cancellationToken">A cancellation token to cancel the operation.</param>
        /// <exception cref="OperationCanceledException">Thrown when the operation has been canceled.</exception>
        public Task SetMethodDefaultHandlerAsync(MethodCallback methodHandler, object userContext, CancellationToken cancellationToken) =>
            InternalClient.SetMethodDefaultHandlerAsync(methodHandler, userContext, cancellationToken);

        /// <summary>
        /// Registers a new delegate for the named method. If a delegate is already associated with
        /// the named method, it will be replaced with the new delegate.
        /// <param name="methodName">The name of the method to associate with the delegate.</param>
        /// <param name="methodHandler">The delegate to be used when a method with the given name is called by the cloud service.</param>
        /// <param name="userContext">generic parameter to be interpreted by the client code.</param>
        /// </summary>

        [Obsolete("Please use SetMethodHandlerAsync.")]
        public void SetMethodHandler(string methodName, MethodCallback methodHandler, object userContext) =>
            InternalClient.SetMethodHandler(methodName, methodHandler, userContext);

        /// <summary>
        /// Registers a new delegate for the connection status changed callback. If a delegate is already associated,
        /// it will be replaced with the new delegate. Note that this callback will never be called if the client is configured to use HTTP as that protocol is stateless
        /// <param name="statusChangesHandler">The name of the method to associate with the delegate.</param>
        /// </summary>
        public void SetConnectionStatusChangesHandler(ConnectionStatusChangesHandler statusChangesHandler) =>
            InternalClient.SetConnectionStatusChangesHandler(statusChangesHandler);

        /// <summary>
        /// Releases the unmanaged resources used by the DeviceClient and optionally disposes of the managed resources.
        /// </summary>
        public void Dispose() => InternalClient?.Dispose();

        /// <summary>
        /// Set a callback that will be called whenever the client receives a state update
        /// (desired or reported) from the service.  This has the side-effect of subscribing
        /// to the PATCH topic on the service.
        /// </summary>
        /// <param name="callback">Callback to call after the state update has been received and applied</param>
        /// <param name="userContext">Context object that will be passed into callback</param>
        [Obsolete("Please use SetDesiredPropertyUpdateCallbackAsync.")]
        public Task SetDesiredPropertyUpdateCallback(DesiredPropertyUpdateCallback callback, object userContext) =>
            InternalClient.SetDesiredPropertyUpdateCallback(callback, userContext);

        /// <summary>
        /// Set a callback that will be called whenever the client receives a state update
        /// (desired or reported) from the service.  This has the side-effect of subscribing
        /// to the PATCH topic on the service.
        /// </summary>
        /// <param name="callback">Callback to call after the state update has been received and applied</param>
        /// <param name="userContext">Context object that will be passed into callback</param>
        public Task SetDesiredPropertyUpdateCallbackAsync(DesiredPropertyUpdateCallback callback, object userContext) =>
            InternalClient.SetDesiredPropertyUpdateCallbackAsync(callback, userContext);

        /// <summary>
        /// Set a callback that will be called whenever the client receives a state update
        /// (desired or reported) from the service.  This has the side-effect of subscribing
        /// to the PATCH topic on the service.
        /// </summary>
        /// <param name="callback">Callback to call after the state update has been received and applied</param>
        /// <param name="userContext">Context object that will be passed into callback</param>
        /// <param name="cancellationToken">A cancellation token to cancel the operation.</param>
        /// <exception cref="OperationCanceledException">Thrown when the operation has been canceled.</exception>
        public Task SetDesiredPropertyUpdateCallbackAsync(DesiredPropertyUpdateCallback callback, object userContext, CancellationToken cancellationToken) =>
            InternalClient.SetDesiredPropertyUpdateCallbackAsync(callback, userContext, cancellationToken);

        /// <summary>
        /// Retrieve a device twin object for the current device.
        /// </summary>
        /// <returns>The device twin object for the current device</returns>
        public Task<Twin> GetTwinAsync() => InternalClient.GetTwinAsync();

        /// <summary>
        /// Retrieve a device twin object for the current device.
        /// </summary>
        /// <param name="cancellationToken">A cancellation token to cancel the operation.</param>
        /// <exception cref="OperationCanceledException">Thrown when the operation has been canceled.</exception>
        /// <returns>The device twin object for the current device</returns>
        public Task<Twin> GetTwinAsync(CancellationToken cancellationToken) => InternalClient.GetTwinAsync(cancellationToken);

        /// <summary>
        /// Push reported property changes up to the service.
        /// </summary>
        /// <param name="reportedProperties">Reported properties to push</param>
        public Task UpdateReportedPropertiesAsync(TwinCollection reportedProperties) =>
            InternalClient.UpdateReportedPropertiesAsync(reportedProperties);

        /// <summary>
        /// Push reported property changes up to the service.
        /// </summary>
        /// <param name="reportedProperties">Reported properties to push</param>
        /// <param name="cancellationToken">A cancellation token to cancel the operation.</param>
        /// <exception cref="OperationCanceledException">Thrown when the operation has been canceled.</exception>
        public Task UpdateReportedPropertiesAsync(TwinCollection reportedProperties, CancellationToken cancellationToken) =>
            InternalClient.UpdateReportedPropertiesAsync(reportedProperties, cancellationToken);
    }
}
