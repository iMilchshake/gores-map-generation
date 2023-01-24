using IO;
using MonoBehaviour;
using Unity.VisualScripting;
using UnityEngine;
using UnityEngine.UIElements;

namespace UI
{
    public class UIHandler : UnityEngine.MonoBehaviour
    {
        public const string DEFAULT_CONFIG = "main"; // TODO: default is hardcoded here
        public const string DEFAULT_LAYOUT = "spiral";

        public UIDocument uiDocument;
        public static UIHandler instance;

        private DropdownField configDropdown;
        private DropdownField layoutDropdown;
        private Button exportConfigButton;
        private Button exportLayoutButton;

        private void Awake()
        {
            instance = this;

            var rootElement = uiDocument.rootVisualElement;
            exportLayoutButton = rootElement.Q<Button>("exportLayoutButton");
            exportConfigButton = rootElement.Q<Button>("exportConfigButton");
            layoutDropdown = rootElement.Q<DropdownField>("layoutConfig");
            configDropdown = rootElement.Q<DropdownField>("generationConfig");

            UpdateConfigDropdown();
            configDropdown.RegisterValueChangedCallback(evt =>
            {
                MainMapGeneration.instance.generationConfig =
                    ConfigSerializer.ImportMapGenerationConfig(evt.newValue);
            });
            if (configDropdown.choices.Contains(DEFAULT_CONFIG))
            {
                configDropdown.value = DEFAULT_CONFIG;
            }

            UpdateLayoutDropdown();
            layoutDropdown.RegisterValueChangedCallback(evt =>
            {
                MainMapGeneration.instance.layoutConfig = ConfigSerializer.ImportLayoutConfig(evt.newValue);
            });
            if (layoutDropdown.choices.Contains(DEFAULT_LAYOUT))
            {
                layoutDropdown.value = DEFAULT_LAYOUT;
            }

            exportConfigButton.clicked += () =>
            {
                ConfigSerializer.ExportConfig(MainMapGeneration.instance.generationConfig);
                UpdateConfigDropdown();
                configDropdown.value = MainMapGeneration.instance.generationConfig.configName;
            };

            exportLayoutButton.clicked += () =>
            {
                ConfigSerializer.ExportConfig(MainMapGeneration.instance.layoutConfig);
                UpdateLayoutDropdown();
                layoutDropdown.value = MainMapGeneration.instance.layoutConfig.layoutName;
            };
        }

        public void SetEnabled(bool value)
        {
            configDropdown.SetEnabled(value);
            layoutDropdown.SetEnabled(value);
            exportConfigButton.SetEnabled(value);
        }

        private void UpdateConfigDropdown()
        {
            configDropdown.choices = ConfigSerializer.GetMapGenerationConfigs();
        }

        private void UpdateLayoutDropdown()
        {
            layoutDropdown.choices = ConfigSerializer.GetLayoutConfigs();
        }
    }
}