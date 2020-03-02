using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace MKDD.Patcher.GUI
{
    public class ModEditVm
    {
        public ModInfo ModInfo { get; }

        public string Title
        {
            get => ModInfo.Title;
            set => ModInfo.Title = value;
        }

        public string Version
        {
            get => ModInfo.Version;
            set => ModInfo.Version = value;
        }

        public string Authors
        {
            get => ModInfo.Authors;
            set => ModInfo.Authors = value;
        }

        public string Description
        {
            get => ModInfo.Description;
            set => ModInfo.Description = value;
        }

        public ModEditVm( ModInfo info )
        {
            ModInfo = info;
        }
    }
}
