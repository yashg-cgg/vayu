using System;
using System.ServiceModel;
using System.ServiceModel.Description;

namespace Vayu.CRRPNLDetails.Model
{
    public class WcfClientWrapper<T> where T : class
    {
        private ChannelFactory<T> _factory;
        private T _channel;
        private readonly object _lock = new object();
        private readonly string _endpoint;

        public WcfClientWrapper(string endpoint)
        {
            _endpoint = endpoint;
            _factory = CreateFactory();
        }

        private ChannelFactory<T> CreateFactory()
        {
            var binding = new NetTcpBinding
            {
                CloseTimeout = TimeSpan.FromMinutes(20),
                OpenTimeout = TimeSpan.FromMinutes(20),
                SendTimeout = TimeSpan.FromMinutes(20),
                ReceiveTimeout = TimeSpan.FromMinutes(20),

                MaxReceivedMessageSize = 300 * 1024 * 1024,
                MaxBufferSize = 300 * 1024 * 1024,
                MaxBufferPoolSize = 300 * 1024 * 1024,

                Security = { Mode = SecurityMode.None },
                TransferMode = TransferMode.Buffered
            };

            binding.ReaderQuotas.MaxArrayLength = 300 * 1024 * 1024;

            //binding.ReliableSession.Enabled = true;
            //binding.ReliableSession.InactivityTimeout = TimeSpan.FromMinutes(30);

            var factory = new ChannelFactory<T>(binding, new EndpointAddress(_endpoint));

            foreach (var op in factory.Endpoint.Contract.Operations)
            {
                var behavior = op.Behaviors.Find<DataContractSerializerOperationBehavior>();
                if (behavior != null)
                {
                    behavior.MaxItemsInObjectGraph = int.MaxValue;
                }
            }

            return factory;
        }

        private T CreateChannel()
        {
            var channel = _factory.CreateChannel();
            ((ICommunicationObject)channel).Open();
            return channel;
        }

        private void EnsureChannel()
        {
            if (_channel == null)
            {
                _channel = CreateChannel();
                return;
            }

            var state = ((ICommunicationObject)_channel).State;

            if (state == CommunicationState.Faulted ||
                state == CommunicationState.Closed ||
                state == CommunicationState.Closing)
            {
                SafeAbort(_channel);
                _channel = CreateChannel();
            }
        }

        public TResult Execute<TResult>(Func<T, TResult> action)
        {
            lock (_lock)
            {
                try
                {
                    EnsureChannel();
                    return action(_channel);
                }
                catch (CommunicationException)
                {
                    ResetChannel();
                    throw;
                }
                catch (TimeoutException)
                {
                    ResetChannel();
                    throw;
                }
                catch (Exception)
                {
                    ResetChannel();
                    throw;
                }
            }
        }

        public void Execute(Action<T> action)
        {
            Execute<bool>(client =>
            {
                action(client);
                return true;
            });
        }

        private void ResetChannel()
        {
            SafeAbort(_channel);
            _channel = null;
        }

        private void SafeAbort(T channel)
        {
            if (channel == null) return;

            try
            {
                ((ICommunicationObject)channel).Abort();
            }
            catch { }
        }

        public void Close()
        {
            if (_channel == null) return;

            var commObj = (ICommunicationObject)_channel;

            try
            {
                if (commObj.State != CommunicationState.Faulted)
                    commObj.Close();
                else
                    commObj.Abort();
            }
            catch
            {
                commObj.Abort();
            }

            _channel = null;
        }
    }
}
