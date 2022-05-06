using System;
using System.Collections.Generic;
using UnityEngine;

namespace Logger {
    public class Log2GUI : MonoBehaviour {
        public LogType LoggerType = LogType.Log;
        public bool GUILogEnable = false;
        public int MaxLineForGUI = 10;
        public Color LogColor = Color.red;

        private List<string> mLinesForGUI = new List<string>();

        void Awake() {
            Application.logMessageReceived += HandleLog;
        }

        private void HandleLog(string logString, string stackTrace, LogType type) {
            if (LoggerType < type) {
                return;
            }

            string logStr = string.Format("{0} {1}: {2}", DateTime.Now.ToString("yy/MM/dd HH:mm:ss:fff"), type, logString);

            if (GUILogEnable) {
                LogGUI(logStr);
                //if (null != stackTrace && stackTrace.Length > 0) {
                //    LogGUI(stackTrace);
                //}
            }
        }

        public void SwitchGUILogStatus() {
            GUILogEnable = !GUILogEnable;
        }

        public void LogGUI(params object[] objs) {
            string text = "";
            for (int i = 0; i < objs.Length; ++i) {
                if (i == 0) {
                    text += objs[i].ToString();
                } else {
                    text += ", " + objs[i].ToString();
                }
            }
            if (Application.isPlaying) {
                while (mLinesForGUI.Count > MaxLineForGUI) {
                    mLinesForGUI.RemoveAt(0);
                }
                mLinesForGUI.Add(text);
            }
        }

        void OnGUI() {
            if (!GUILogEnable) {
                return;
            }
            Color clrBackup = GUI.color;

            GUI.color = LogColor;
            for (int i = 0, imax = mLinesForGUI.Count; i < imax; ++i) {
                GUILayout.Label(mLinesForGUI[i]);
            }

            GUI.color = clrBackup;
        }

    }
}
