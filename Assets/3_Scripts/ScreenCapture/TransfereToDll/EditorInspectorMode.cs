using UnityEditor;
using UnityEngine;
using System;
using System.Reflection;

namespace PxP.Tools
{
    public static class EditorInspectorMode
    {
        // Define the delegate for the event
        public delegate void InspectorModeChangedEventHandler(InspectorMode newMode);
        // Define the event
        public static event InspectorModeChangedEventHandler OnInspectorModeChanged;

        private static InspectorMode s_currentInspectorMode = InspectorMode.Normal;
        private static double s_lastCheckTime = 0;
        private const double CHECK_INTERVAL = 0.5; // Check every 0.5 seconds

        // Use InitializeOnLoad to ensure this runs when Unity loads
        [InitializeOnLoadMethod]
        private static void Initialize()
        {
            EditorApplication.update -= CheckInspectorModeAndNotify; // Prevent double subscription
            EditorApplication.update += CheckInspectorModeAndNotify;
            // Perform an initial check
            CheckInspectorModeAndNotify();
        }

        /// <summary>
        /// Periodically checks the Inspector mode and invokes an event if it changes.
        /// This method is subscribed to EditorApplication.update.
        /// </summary>
        private static void CheckInspectorModeAndNotify()
        {
            double currentTime = EditorApplication.timeSinceStartup;
            if (currentTime - s_lastCheckTime < CHECK_INTERVAL)
            {
                return; // Not enough time has passed since the last check
            }

            s_lastCheckTime = currentTime;

            InspectorMode newMode = GetInspectorModeSafe();

            if (newMode != s_currentInspectorMode)
            {
                s_currentInspectorMode = newMode;
                OnInspectorModeChanged?.Invoke(s_currentInspectorMode);
            }
        }

        /// <summary>
        /// Safely gets the current Inspector mode without forcing focus.
        /// </summary>
        public static InspectorMode GetInspectorModeSafe()
        {
            System.Type inspectorWindowType = typeof(EditorWindow).Assembly.GetType("UnityEditor.InspectorWindow");
            if (inspectorWindowType == null)
                return InspectorMode.Normal;

            // Use Resources.FindObjectsOfTypeAll to find existing Inspector windows
            EditorWindow[] inspectorWindows = Resources.FindObjectsOfTypeAll(inspectorWindowType) as EditorWindow[];

            if (inspectorWindows == null || inspectorWindows.Length == 0)
                return InspectorMode.Normal;

            // Get the first Inspector window found
            EditorWindow inspectorWindow = inspectorWindows[0];

            FieldInfo inspectorModeField = inspectorWindowType.GetField("m_InspectorMode", BindingFlags.NonPublic | BindingFlags.Instance);
            if (inspectorModeField == null)
                return InspectorMode.Normal;

            object inspectorMode = inspectorModeField.GetValue(inspectorWindow);

            if ((int)inspectorMode == (int)InspectorMode.Debug)
            {
                return InspectorMode.Debug;
            }
            else
            {
                return InspectorMode.Normal;
            }
        }
    }
}