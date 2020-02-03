namespace MKDD.Patcher.GUI
{
    public class ModVm
    {
        private readonly ModInfo mDbInfo;
        private readonly GuiModInfo mGuiInfo;

        public bool Enabled
        {
            get => mGuiInfo.Enabled;
            set => mGuiInfo.Enabled = value;
        }

        public string Title => mDbInfo.Title;
        public string Version => mDbInfo.Version;
        public string Authors => mDbInfo.Author;
        public string Description => mDbInfo.Description;

        public ModVm( ModInfo dbInfo, GuiModInfo guiInfo)
        {
            mDbInfo = dbInfo;
            mGuiInfo = guiInfo;
        }
    }
}
