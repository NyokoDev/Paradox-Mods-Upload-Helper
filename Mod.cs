using Colossal.IO.AssetDatabase;
using Colossal.Logging;
using Game;
using Game.Modding;
using Game.SceneFlow;
using Paradox_Mods_Upload_Helper.Systems;

namespace Paradox_Mods_Upload_Helper
{
    public class Mod : IMod
    {
        public static ILog log = LogManager.GetLogger($"{nameof(Paradox_Mods_Upload_Helper)}.{nameof(Mod)}").SetShowsErrorsInUI(false);
        private Setting m_Setting;
        public const string MODUI = "ParadoxModsUploadAssistant";


        public void OnLoad(UpdateSystem updateSystem)
        {
            log.Info(nameof(OnLoad));

            if (GameManager.instance.modManager.TryGetExecutableAsset(this, out var asset))
                log.Info($"Current mod asset at {asset.path}");

            m_Setting = new Setting(this);
            m_Setting.RegisterInOptionsUI();
            GameManager.instance.localizationManager.AddSource("en-US", new LocaleEN(m_Setting));


            AssetDatabase.global.LoadSettings(nameof(Paradox_Mods_Upload_Helper), m_Setting, new Setting(this));

            updateSystem.UpdateAt<UISystem>(SystemUpdatePhase.UIUpdate);
        }

        public void OnDispose()
        {
            log.Info(nameof(OnDispose));
            if (m_Setting != null)
            {
                m_Setting.UnregisterInOptionsUI();
                m_Setting = null;
            }
        }
    }
}
