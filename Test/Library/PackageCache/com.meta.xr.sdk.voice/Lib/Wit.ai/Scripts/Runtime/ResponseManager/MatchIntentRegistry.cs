﻿/*
 * Copyright (c) Meta Platforms, Inc. and affiliates.
 * All rights reserved.
 *
 * This source code is licensed under the license found in the
 * LICENSE file in the root directory of this source tree.
 */

using System;
using System.Reflection;
using System.Threading;
using Meta.Voice.Logging;
using Meta.WitAi.Utilities;

namespace Meta.WitAi
{
    internal class RegisteredMatchIntent
    {
        public Type type;
        public MethodInfo method;
        public MatchIntent matchIntent;
    }


    [LogCategory("MatchIntent")]
    internal static class MatchIntentRegistry
    {
        /// <inheritdoc/>
        public static IVLogger Logger { get; } = LoggerRegistry.Instance.GetLogger("MatchIntent");
        private static DictionaryList<string, RegisteredMatchIntent> registeredMethods;

        public static DictionaryList<string, RegisteredMatchIntent> RegisteredMethods
        {
            get
            {
                if (null == registeredMethods)
                {
                    // Note, first run this will not return any values. Initialize
                    // scans assemblies on a different thread. This is ok for voice
                    // commands since they are generally executed in realtime after
                    // initialization is complete. This is a perf optimization.
                    //
                    // Best practice is to call Initialize in Awake of any method
                    // that will be using the resulting data.
                    Initialize();
                }

                return registeredMethods;
            }
        }

        internal static void Initialize()
        {
            if (null != registeredMethods) return;
            registeredMethods = new DictionaryList<string, RegisteredMatchIntent>();
            _ = ThreadUtility.Background(Logger, RefreshAssemblies);
        }

        internal static void RefreshAssemblies()
        {
            if (Thread.CurrentThread.ThreadState == ThreadState.Aborted)
            {
                return;
            }
            // TODO: We could potentially build this list at compile time and cache it
            // Work on a local dictionary to avoid thread complications
            var dictionary = new DictionaryList<string, RegisteredMatchIntent>();
            foreach (var assembly in AppDomain.CurrentDomain.GetAssemblies())
            {
                try {
                    foreach (Type t in assembly.GetTypes()) {
                        try {
                            foreach (var method in t.GetMethods()) {
                                try {
                                    foreach (var attribute in method.GetCustomAttributes(typeof(MatchIntent))) {
                                        try {
                                            var mi = (MatchIntent)attribute;
                                            dictionary[mi.Intent].Add(new RegisteredMatchIntent() {
                                                type = t,
                                                method = method,
                                                matchIntent = mi
                                            });
                                        } catch (Exception e) {
                                            Logger.Debug(e.Message);
                                        }
                                    }
                                } catch (Exception e) {
                                    Logger.Debug(e.Message);
                                }
                            }
                        } catch (Exception e) {
                            Logger.Debug(e.Message);
                        }
                    }
                } catch (Exception e) {
                    Logger.Debug(e.Message);
                }
            }

            registeredMethods = dictionary;
        }
    }
}
