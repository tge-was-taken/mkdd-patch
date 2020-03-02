namespace MKDD.Patcher.GUI
{
    public class ModListVm
    {
        private readonly GuiModConfig mGuiInfo;

        internal ModInfo ModInfo { get; }

        public bool Enabled
        {
            get => mGuiInfo.Enabled;
            set => mGuiInfo.Enabled = value;
        }

        public string Title => ModInfo.Title;
        public string Version => ModInfo.Version;
        public string Authors => ModInfo.Authors;
        public string Description => ModInfo.Description;

        public ModListVm( ModInfo dbInfo, GuiModConfig guiInfo)
        {
            ModInfo = dbInfo;
            mGuiInfo = guiInfo;
        }
    }
}
