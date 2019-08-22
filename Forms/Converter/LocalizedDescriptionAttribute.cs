﻿/*
DeepDungeon HOH Party is licensed under a
Creative Commons Attribution-NonCommercial-ShareAlike 4.0 International License.

You should have received a copy of the license along with this
work. If not, see <http://creativecommons.org/licenses/by-nc-sa/4.0/>.

Orginal work done by zzi, contibutions by Omninewb, Freiheit, and mastahg
                                                                                 */
using DeepHoh.Properties;
using System.ComponentModel;

namespace DeepHoh.Forms.Converter
{
    public class LocalizedDescriptionAttribute : DescriptionAttribute
    {
        #region Fields

        #endregion

        #region Constructors

        public LocalizedDescriptionAttribute(string resourceName)
        {
            ResourceName = resourceName;
        }

        #endregion

        #region DescriptionAttribute Members

        public override string Description => Resources.ResourceManager.GetString(ResourceName);

        #endregion

        #region Properties

        public string ResourceName { get; }

        #endregion
    }
}