namespace MKDD.Patcher.GUI
{
    public class ModVm
    {
        private readonly ModInfo mDbInfo;
        private readonly GuiModConfig mGuiInfo;

        public bool Enabled
        {
            get => mGuiInfo.Enabled;
            set => mGuiInfo.Enabled = value;
        }

        public string Title => mDbInfo.Title;
        public string Version => mDbInfo.Version;
        public string Authors => mDbInfo.Author;
        public string Description => mDbInfo.Description;

        public ModVm( ModInfo dbInfo, GuiModConfig guiInfo)
        {
            mDbInfo = dbInfo;
            mGuiInfo = guiInfo;
        }
    }
}
