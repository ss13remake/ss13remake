﻿using System;
using System.ServiceModel;
using System.Timers;
using SS13.IoC;
using SS13_Shared;
using SS13_Shared.GO;
using ServerInterfaces;
using ServerInterfaces.Configuration;
using ServerInterfaces.MessageLogging;

namespace ServerServices.MessageLogging
{
    public class MessageLogger : IMessageLogger, IService
    {
        private static Timer _pingTimer;
        private readonly MessageLoggerServiceClient _loggerServiceClient;
        private bool _logging;

        public MessageLogger(IConfigurationManager _configurationManager)
        {
            _logging = _configurationManager.MessageLogging;
            _loggerServiceClient = new MessageLoggerServiceClient("NetNamedPipeBinding_IMessageLoggerService");
            if (_logging)
            {
                Ping();
                _pingTimer = new Timer(5000);
                _pingTimer.Elapsed += CheckServer;
                _pingTimer.Enabled = true;
            }
        }

        #region IMessageLogger Members

        /// <summary>
        /// Check to see if the server is still running 
        /// </summary>
        public void Ping()
        {
            bool failed = false;
            try
            {
                bool up = _loggerServiceClient.ServiceStatus();
            }
            catch (CommunicationException e)
            {
                failed = true;
            }
            finally
            {
                if (failed)
                    _logging = false;
            }
        }

        public void LogOutgoingComponentNetMessage(long clientUID, int uid, ComponentFamily family, object[] parameters)
        {
            if (!_logging)
                return;

            for (int i = 0; i < parameters.Length; i++)
            {
                if (parameters[i] is Enum)
                    parameters[i] = (int) parameters[i];
            }
            try
            {
                _loggerServiceClient.LogServerOutgoingNetMessage(clientUID, uid, (int) family, parameters);
            }
            catch (CommunicationException e)
            {
            }
        }

        public void LogIncomingComponentNetMessage(long clientUID, int uid, EntityMessage entityMessage,
                                                   ComponentFamily componentFamily, object[] parameters)
        {
            if (!_logging)
                return;

            for (int i = 0; i < parameters.Length; i++)
            {
                if (parameters[i] is Enum)
                    parameters[i] = (int) parameters[i];
            }
            try
            {
                _loggerServiceClient.LogServerIncomingNetMessage(clientUID, uid, (int) entityMessage,
                                                                 (int) componentFamily, parameters);
            }
            catch (CommunicationException e)
            {
            }
        }

        public void LogComponentMessage(int uid, ComponentFamily senderfamily, string sendertype,
                                        ComponentMessageType type)
        {
            if (!_logging)
                return;

            try
            {
                _loggerServiceClient.LogServerComponentMessage(uid, (int) senderfamily, sendertype, (int) type);
            }
            catch (CommunicationException e)
            {
            }
        }

        #endregion

        #region IService Members

        public ServerServiceType ServiceType
        {
            get { return ServerServiceType.MessageLogger; }
        }

        #endregion

        public static void CheckServer(object source, ElapsedEventArgs e)
        {
            IoCManager.Resolve<IMessageLogger>().Ping();
        }
    }
}