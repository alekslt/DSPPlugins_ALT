using UnityEngine;

namespace DSPPlugins_ALT.GUI
{
    public class UITheme
    {
        public static GUILayoutOption PlanetColWidth;
        public static GUILayoutOption LocationColWidth;
        public static GUILayoutOption AlarmColWidth;
        public static GUILayoutOption[] VeinIconLayoutOptions;
        public static GUILayoutOption[] VeinIconLayoutSmallOptions;
        public static GUILayoutOption[] MenuButtonLayoutOptions;

        public static GUILayoutOption VeinTypeColWidth;
        public static GUILayoutOption VeinAmountColWidth;
        public static GUILayoutOption VeinRateColWidth;
        public static GUILayoutOption VeinETAColWidth;

        public static GUIStyle TextAlignStyle;

        public static GUIStyle MenuButtonStyle;
        public static GUIStyle MenuButtonHighlightedStyle;

        public static GUIStyle TabMenuButtonStyle;
        public static GUIStyle TabMenuButtonSelectedStyle;

        public static GUIStyle DemandStyle;
        public static GUIStyle SupplyStyle;

        public static Texture2D[] Sign_state;
        public static Texture2D Menu_button_texture;
        public static Texture2D Tab_menu_button_texture;
        public static Texture2D Tab_menu_button_texture_selected;

        public static void Init()
        {
            Sign_state = new Texture2D[10];

            for (int i = 0; i <= 8; i++)
            {
                //Debug.Log("Loading sign-state-" + i + " - res :" + sign_state[i]);
                Sign_state[i] = Resources.Load<Texture2D>("ui/textures/sprites/icons/sign-state-" + i);
                if (Sign_state[i] == null)
                {
                    Debug.LogWarning("Failed Loading sign-state-" + i);
                    Sign_state[i] = Texture2D.blackTexture;
                }
            }

            //"ui/textures/sprites/round-64px-border-slice"
            // ui/textures/sprites/sci-fi/crystal-btn-trans
            // "auxes/rts/circle-tex-1"
            Menu_button_texture = Resources.Load<Texture2D>("auxes/rts/circle-tex-1");
            if (Menu_button_texture == null)
            {
                Debug.LogWarning("Failed Loading menu_button_texture");
                Menu_button_texture = Texture2D.blackTexture;
            }

            Tab_menu_button_texture = Resources.Load<Texture2D>("ui/textures/sprites/sci-fi/panel-4");
            if (Tab_menu_button_texture == null)
            {
                Debug.LogWarning("Failed Loading menu_button_texture");
                Tab_menu_button_texture = Texture2D.blackTexture;
            }

            Tab_menu_button_texture_selected = Resources.Load<Texture2D>("ui/textures/sprites/sci-fi/panel-3");
            if (Tab_menu_button_texture_selected == null)
            {
                Debug.LogWarning("Failed Loading menu_button_texture");
                Tab_menu_button_texture_selected = Texture2D.blackTexture;
            }



            TextAlignStyle = new GUIStyle(UnityEngine.GUI.skin.label);
            TextAlignStyle.alignment = TextAnchor.MiddleLeft;

            MenuButtonStyle = new GUIStyle(UnityEngine.GUI.skin.button);
            MenuButtonStyle.padding.left = 10;
            MenuButtonStyle.normal.background = MenuButtonStyle.hover.background = MenuButtonStyle.active.background = Menu_button_texture;

            MenuButtonStyle.normal.textColor = Color.white;
            MenuButtonStyle.fontSize = 14;

            MenuButtonHighlightedStyle = new GUIStyle(MenuButtonStyle);
            MenuButtonHighlightedStyle.normal.textColor = Color.red;

            PlanetColWidth = GUILayout.Width(160);
            LocationColWidth = GUILayout.Width(80);
            AlarmColWidth = GUILayout.Width(140);
            //VeinIconColWidth = GUILayout.Width(45);
            VeinTypeColWidth = GUILayout.Width(135);
            VeinAmountColWidth = GUILayout.Width(80);
            VeinRateColWidth = GUILayout.Width(80);
            VeinETAColWidth = GUILayout.Width(95);

            VeinIconLayoutOptions = new GUILayoutOption[] { GUILayout.Height(35), GUILayout.MaxWidth(35) };
            VeinIconLayoutSmallOptions = new GUILayoutOption[] { GUILayout.Height(30), GUILayout.MaxWidth(30) };

            MenuButtonLayoutOptions = new GUILayoutOption[] { GUILayout.Height(42), GUILayout.MaxWidth(42) };

            TabMenuButtonStyle = new GUIStyle(UnityEngine.GUI.skin.button);
            TabMenuButtonStyle.normal.background = TabMenuButtonStyle.hover.background = TabMenuButtonStyle.active.background = Tab_menu_button_texture;
            TabMenuButtonSelectedStyle = new GUIStyle(TabMenuButtonStyle);
            TabMenuButtonSelectedStyle.normal.background = TabMenuButtonSelectedStyle.hover.background = TabMenuButtonSelectedStyle.active.background = Tab_menu_button_texture_selected;
            TabMenuButtonSelectedStyle.normal.textColor = TabMenuButtonSelectedStyle.hover.textColor = TabMenuButtonSelectedStyle.active.textColor = Color.white;

            DemandStyle = new GUIStyle(TextAlignStyle);
            DemandStyle.normal.textColor = new Color(0.8784f, 0.5450f, 0.3647f);  // 224, 139, 93
            SupplyStyle = new GUIStyle(TextAlignStyle);
            SupplyStyle.normal.textColor = new Color(0.2392f, 0.5450f, 0.6549f); // 61, 139, 167

            // slider = MineralExhaustionNotifier.instance.gameObject.AddComponent<Slider>();
            //sourceCombo = MineralExhaustionNotifier.instance.gameObject.AddComponent<UIComboBox>();
            //sourceCombo.Items = new List<string>() {"A", "B" };
        }
    }
}
