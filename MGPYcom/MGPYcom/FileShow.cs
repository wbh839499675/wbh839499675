using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Data;
using System.Drawing;
using System.Linq;
using System.Text;
using System.Windows.Forms;

namespace MGPYcom
{
    public partial class FileShow : Form
    {
        public FileShow(String file)
        {
            
            InitializeComponent();
            richTextBox1.Text = file;
            
        }
    }
}
