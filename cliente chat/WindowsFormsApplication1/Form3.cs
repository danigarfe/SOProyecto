using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Data;
using System.Drawing;
using System.Linq;
using System.Text;
using System.Windows.Forms;

namespace WindowsFormsApplication1
{
    public partial class Form3 : Form
    {
        public int resultadoinvitacion=0;
        string inv;
        public Form3(string invitador)
        {
            inv = invitador;
            InitializeComponent();
        }

        private void Form3_Load(object sender, EventArgs e)
        {
            label1.Text = inv + " busca rival digno. ¿Serás tú?";
        }

        private void button2_Click(object sender, EventArgs e)
        {
            resultadoinvitacion = 2;
            this.Close();
        }

        private void button1_Click(object sender, EventArgs e)
        {
            resultadoinvitacion = 1;
            this.Close();
        }
    }
}
