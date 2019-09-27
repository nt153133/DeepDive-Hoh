/*
DeepDungeon HOH Party is licensed under a
Creative Commons Attribution-NonCommercial-ShareAlike 4.0 International License.

You should have received a copy of the license along with this
work. If not, see <http://creativecommons.org/licenses/by-nc-sa/4.0/>.

Orginal work done by zzi, contibutions by Omninewb, Freiheit, and mastahg
                                                                                 */

using System;
using System.Windows.Forms;
using DeepHoh.Logging;
using DeepHoh.Structure;

namespace DeepHoh.Forms
{
    public partial class SettingsForm : Form
    {
        public SettingsForm()
        {
            InitializeComponent();
        }

        private void SettingsForm_Load(object sender, EventArgs e)
        {
            settings.SelectedObject = Settings.Instance;
            Settings.Instance.EnsureFloorSettings();
            Levels.DataSource = Settings.Instance.FloorSettings;
            Levels.SelectedIndexChanged += changelevel;
            Levels.SelectedItem = Settings.Instance.SelectedLevel;
            Levels.Refresh();
            solostop.Checked = !Settings.Instance.SoloStop;
            solostop.CheckedChanged += solostop_CheckStateChanged;
        }

        private void changelevel(object sender, EventArgs e)
        {
            Logger.Verbose("Changing the selected floor to run");
            Settings.Instance.SelectedLevel = (FloorSetting) Levels.SelectedItem;
        }

        private void Levels_SelectedIndexChanged(object sender, EventArgs e)
        {
            Settings.Instance.SelectedLevel = (FloorSetting) Levels.SelectedItem;
           
        }

        private void solostop_CheckStateChanged(object sender, EventArgs e)
        {
            Logger.Verbose("Changing stop state");
            Settings.Instance.SoloStop = !Settings.Instance.SoloStop;
        }
    }
}