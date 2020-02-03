using Microsoft.Extensions.Configuration;
using Serilog.Core;
using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Data;
using System.Drawing;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Forms;

namespace MKDD.Patcher.GUI
{
    public partial class MainForm : Form
    {
        private Logger mLogger;
        private IConfiguration mConfiguration;
        private Patcher mPatcher;
        private MergeOrder mMergeOrder;

        public MainForm()
        {
            InitializeComponent();
        }

        public MainForm(Logger logger, IConfiguration configuration)
            : this()
        {
            mLogger = logger;
            mConfiguration = configuration;
            mPatcher = new Patcher(logger, configuration);
            mMergeOrder = MergeOrder.LastToFirst;

            PopulateGrid();
        }

        private void PopulateGrid()
        {
            foreach (DataGridViewRow row in dgvMods.Rows)
                dgvMods.Rows.Remove(row);

            // Iterate over mods
            foreach (var modDir in Directory.EnumerateDirectories(mConfiguration["ModsDir"]))
            {
                var modDirName = Path.GetFileName(modDir);
                if (modDirName.StartsWith("."))
                    continue;

                dgvMods.Rows.Add(new object[] { true, modDirName });
            }
        }

        private void btnSave_Click(object sender, EventArgs e)
        {
            var modFilter = new List<string>();
            foreach (DataGridViewRow row in dgvMods.Rows)
            {
                var enabled = (bool)row.Cells[clmEnabled.Index].Value;
                var title = (string)row.Cells[clmTitle.Index].Value;
                if (enabled)
                    modFilter.Add(title);
            }

            var patchTask = Task.Run(() => mPatcher.Patch(mMergeOrder, modFilter))
                .ContinueWith(task => MessageBox.Show("Patching done!"));
        }

        private void tsmiAbout_Click(object sender, EventArgs e)
        {
            var text = @"
MKDD Patcher GUI made by TGE.
Special thanks to:
- Lunaboy (LunaboyRarcTools)
- arookas (mareep)";

            MessageBox.Show(text, "About", MessageBoxButtons.OK, MessageBoxIcon.Information);
        }

        private void btnUp_Click(object sender, EventArgs e)
        {
            MoveSelectedRows(-1);
        }

        private void btnDown_Click(object sender, EventArgs e)
        {
            MoveSelectedRows(1);
        }

        private void MoveSelectedRows(int offset)
        {
            var changedSelRowIndices = new List<int>();
            foreach (DataGridViewRow row in dgvMods.SelectedRows)
            {
                var index = row.Index;
                var newIndex = index + offset;
                if (newIndex >= 0 && newIndex < dgvMods.Rows.Count)
                {
                    dgvMods.Rows.RemoveAt(index);
                    dgvMods.Rows.Insert(newIndex, row);
                    changedSelRowIndices.Add(row.Index);
                }
            }

            dgvMods.ClearSelection();
            foreach (var item in changedSelRowIndices)
                dgvMods.Rows[item].Cells[0].Selected = true;
        }

        private void tsmiSettings_Click(object sender, EventArgs e)
        {
            using (var dialog = new ConfigurationForm())
                dialog.ShowDialog();
        }
    }
}
