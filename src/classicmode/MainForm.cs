#if !MONOGAME
using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Diagnostics;
using System.Drawing;
using System.Globalization;
using System.IO;
using System.Runtime.InteropServices;
using System.Windows.Forms;

namespace NonsensicalVideoGenerator
{
    public partial class MainForm : Form
    {
        public MainForm()
        {
            InitializeComponent();
        }
        private void YTPPlusPlus_FormClosing(object sender, FormClosingEventArgs e)
        {
            SaveData.Save();
        }
        private void PausePlay_Click(object sender, EventArgs e)
        {
        }
        private void Start_Click(object sender, EventArgs e)
        {
        }
        private void End_Click(object sender, EventArgs e)
        {
        }
        private void AddMaterial_Click(object sender, EventArgs e)
        {
        }
        private void ClearMaterial_Click(object sender, EventArgs e)
        {
        }
        private void Render_Click(object sender, EventArgs e)
        {
        }
        private void SaveAs_Click(object sender, EventArgs e)
        {
        }
        private void YTPPlusPlus_Load(object sender, EventArgs e)
        {
            this.FormClosing += YTPPlusPlus_FormClosing;
        }
    }
}
#endif
