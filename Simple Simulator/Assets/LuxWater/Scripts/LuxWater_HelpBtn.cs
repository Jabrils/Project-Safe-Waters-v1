using UnityEngine;
using System.Collections;
#if UNITY_EDITOR
    using UnityEditor;
#endif

namespace LuxWater {
    public class LuxWater_HelpBtn : PropertyAttribute
    {
        public string URL;
        public LuxWater_HelpBtn(string URL) {
            this.URL = URL;
        }
    }

#if UNITY_EDITOR
    [CustomPropertyDrawer(typeof(LuxWater_HelpBtn))]
    public class LuxWater_HelpBtnDrawer : DecoratorDrawer {
        private static string baseURL = "https://docs.google.com/document/d/1NDtUpVBgd3UYEI8LRNI4teFF3qm38h-u3Az-ozaaOX0/view#heading=";

        LuxWater_HelpBtn help {
            get { return ((LuxWater_HelpBtn)attribute); }
        }

        public override float GetHeight() {
            return base.GetHeight() + 2;
        }

        override public void OnGUI(Rect position) {
            Color helpCol = new Color(0.30f,0.47f,1.0f,1.0f); // matches highlight blue //new Color(1.0f,0.3f,0.0f,1.0f); // Orange
            if (!EditorGUIUtility.isProSkin) {
                helpCol = Color.blue;
            }
            GUIStyle myMiniHelpBtn = new GUIStyle(EditorStyles.miniButton);
            myMiniHelpBtn.padding = new RectOffset(0, 0, 2, 2);
            myMiniHelpBtn.normal.background = null;
            myMiniHelpBtn.normal.textColor = helpCol;
            myMiniHelpBtn.onNormal.textColor = helpCol;
            myMiniHelpBtn.active.textColor = helpCol;
            myMiniHelpBtn.onActive.textColor = helpCol;
            myMiniHelpBtn.focused.textColor = helpCol;
            myMiniHelpBtn.onFocused.textColor = helpCol;

            position.x = position.x + position.width - 30;
            position.width = 30;

            if (GUI.Button(position, "Help", myMiniHelpBtn)) {
                Application.OpenURL(baseURL + help.URL);
            }
        }
    }
#endif
}