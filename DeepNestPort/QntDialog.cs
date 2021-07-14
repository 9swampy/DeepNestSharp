﻿namespace DeepNestSharp
{
    using System;
    using System.Drawing;
    using System.Windows.Forms;

    public partial class QntDialog : Form
    {
        public QntDialog()
        {
            InitializeComponent();
            StartPosition = FormStartPosition.CenterParent;
        }

        public int Qnt=0;

        public void Quit()
        {
            DialogResult = DialogResult.None;
            try
            {
                Qnt = int.Parse(textBox1.Text);
                DialogResult = DialogResult.OK;
                Close();
            }
            catch 
            {
                textBox1.BackColor = Color.Red;
                textBox1.ForeColor = Color.White;
            }
        }
        private void button1_Click(object sender, EventArgs e)
        {
            Quit();
        }

        private void textBox1_KeyDown(object sender, KeyEventArgs e)
        {
            if (e.KeyCode == Keys.Enter)
            {
                Quit();
            }
        }

        private void textBox1_TextChanged(object sender, EventArgs e)
        {
            textBox1.BackColor = Color.White;
            textBox1.ForeColor = Color.Black;
        }
    }
}
