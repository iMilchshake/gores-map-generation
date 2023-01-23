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
        public UIDocument uiDocument;
        public static UIHandler instance;

        public IBinding testValue;

        private DropdownField configDropdown;
        private Button exportConfigButton;

        private void Awake()
        {
            Debug.Log("UIHandler Awake");
            instance = this;
            var rootElement = uiDocument.rootVisualElement;
            exportConfigButton = rootElement.Q<Button>("exportConfigButton");

            configDropdown = rootElement.Q<DropdownField>("generationConfig");
            UpdateConfigDropdown();
            configDropdown.RegisterValueChangedCallback(evt =>
            {
                MainMapGeneration.instance.generationConfig = ConfigSerializer.ImportConfig(evt.newValue);
            });
            configDropdown.value = DEFAULT_CONFIG;

            exportConfigButton.clicked += () =>
            {
                ConfigSerializer.ExportConfig(MainMapGeneration.instance.generationConfig);
                UpdateConfigDropdown();
                configDropdown.value = MainMapGeneration.instance.generationConfig.configName;
            };
        }

        public void SetEnabled(bool value)
        {
            configDropdown.SetEnabled(value);
            exportConfigButton.SetEnabled(value);
        }

        private void UpdateConfigDropdown()
        {
            configDropdown.choices = ConfigSerializer.GetConfigNames();
        }
    }
}