// Copyright (c) Microsoft Corporation. All rights reserved.
// Portions derived from React Native:
// Copyright (c) 2015-present, Facebook, Inc.
// Licensed under the MIT License.
using Windows.Networking.Connectivity;
using Microsoft.ReactNative.Managed;
using System.Collections.Generic;
using System;

namespace RNCNetInfo
{
    /// <summary>
    /// Module that monitors and provides information about the connectivity
    /// state of the device.
    /// </summary>
    [ReactModule]
    internal sealed class RNCNetInfo
    {
        // These constants need to match the strings defined in types.ts
        private const string CONNECTION_TYPE_CELLULAR = "cellular";
        private const string CONNECTION_TYPE_ETHERNET = "ethernet";
        private const string CONNECTION_TYPE_NONE = "none";
        private const string CONNECTION_TYPE_UNKNOWN = "unknown";
        private const string CONNECTION_TYPE_WIFI = "wifi";
        private const string CONNECTION_TYPE_OTHER = "other";

        private const string CELLULAR_GENERATION_2G = "2g";
        private const string CELLULAR_GENERATION_3G = "3g";
        private const string CELLULAR_GENERATION_4G = "4g";
        private const string CELLULAR_GENERATION_NONE = null;
        private const string CELLULAR_GENERATION_UNKNOWN = null;

        private readonly INetworkInformation _networkInfo;

        /// <summary>
        /// Instantiates the <see cref="RNCNetInfoModule"/>.
        /// </summary>
        /// <param name="reactContext">The React context.</param>
        public RNCNetInfo()
            : this(new DefaultNetworkInformation())
        {
        }

        /// <summary>
        /// Instantiates the <see cref="RNCNetInfoModule"/>.
        /// </summary>
        /// <param name="networkInfo">The network information.</param>
        /// <param name="reactContext">The React context.</param>
        public RNCNetInfo(INetworkInformation networkInfo)
            : base()
        {
            _networkInfo = networkInfo;
        }



        /// <summary>
        /// Gets the current connectivity state of the app.
        /// </summary>
        /// <param name="promise">A promise to resolve the request.</param>
        [ReactMethod]
        public void getCurrentState(ReactPromise<JSValue> promise)
        {
            promise.Resolve(CreateConnectivityEventMap());
        }

        /// <summary>
        /// Called when the application host is destroyed.
        /// </summary>
        public void OnDestroy()
        {
        }

        /// <summary>
        /// Called when the application host is resumed.
        /// </summary>
        public void OnResume()
        {
            _networkInfo.Start();
            _networkInfo.NetworkStatusChanged += OnStatusChanged;
        }

        /// <summary>
        /// Called when the application host is suspended.
        /// </summary>
        public void OnSuspend()
        {
            _networkInfo.NetworkStatusChanged -= OnStatusChanged;
            _networkInfo.Stop();
        }

        private JSValue CreateConnectivityEventMap()
        {
            var eventMap = new Dictionary<string, JSValue>();

            // Add the connection type information
            var type = GetConnectivityType();
            eventMap.Add("type", new JSValue(type));

            // Add the connection state information
            var isConnected = GetIsConnected();
            eventMap.Add("isConnected", new JSValue(isConnected));

            // Add the details, if there are any
            Dictionary<string, JSValue> details = null;
            if (isConnected)
            {
                details = new Dictionary<string, JSValue>();

                var isConnectionExpensive = GetIsConnectionExpensive();
                details.Add("isConnectionExpensive", new JSValue(isConnectionExpensive));

                if (type == CONNECTION_TYPE_CELLULAR)
                {
                    var cellularGeneration = GetCellularGeneration();
                    details.Add("cellularGeneration", new JSValue(cellularGeneration));
                }
            }
            eventMap.Add("details", new JSValue(details));

            return new JSValue(eventMap);
        }

        private string GetConnectivityType()
        {
            var profile = _networkInfo.GetInternetConnectionProfile();
            if (profile == null)
            {
                return CONNECTION_TYPE_NONE;
            }

            switch (profile.ConnectionType)
            {
                case NetworkConnectionType.Unknown:
                    return CONNECTION_TYPE_UNKNOWN;
                case NetworkConnectionType.None:
                    return CONNECTION_TYPE_NONE;
                case NetworkConnectionType.Cellular:
                    return CONNECTION_TYPE_CELLULAR;
                case NetworkConnectionType.Ethernet:
                    return CONNECTION_TYPE_ETHERNET;
                case NetworkConnectionType.Wifi:
                    return CONNECTION_TYPE_WIFI;
                case NetworkConnectionType.Other:
                    return CONNECTION_TYPE_OTHER;
                default:
                    return CONNECTION_TYPE_UNKNOWN;
            }
        }

        private string GetCellularGeneration()
        {
            var profile = _networkInfo.GetInternetConnectionProfile();
            if (profile == null)
            {
                return CELLULAR_GENERATION_NONE;
            }

            switch (profile.CellularGeneration)
            {
                case CellularGeneration.Unknown:
                    return CELLULAR_GENERATION_UNKNOWN;
                case CellularGeneration.None:
                    return CELLULAR_GENERATION_NONE;
                case CellularGeneration.Generation2:
                    return CELLULAR_GENERATION_2G;
                case CellularGeneration.Generation3:
                    return CELLULAR_GENERATION_3G;
                case CellularGeneration.Generation4:
                    return CELLULAR_GENERATION_4G;
                default:
                    return CELLULAR_GENERATION_UNKNOWN;
            }
        }

        private bool GetIsConnected()
        {
            var profile = _networkInfo.GetInternetConnectionProfile();
            return profile != null && profile.ConnectivityLevel != NetworkConnectivityLevel.None;
        }

        private bool GetIsConnectionExpensive()
        {
            var profile = _networkInfo.GetInternetConnectionProfile();
            if (profile == null)
            {
                return false;
            }

            var connectionCost = profile.ConnectionCost;
            return connectionCost == NetworkCostType.Fixed || connectionCost == NetworkCostType.Variable;
        }

        [ReactEvent]
        public Action<object> NetworkStatusDidChange { get; set; }

        private void OnStatusChanged(object ignored)
        {
            var connectivity = CreateConnectivityEventMap();
            NetworkStatusDidChange(CreateConnectivityEventMap());
        }
    }
}
