namespace DeepNestPort
{
    using System;
    using System.Drawing;
    using System.Windows.Forms;

    public partial class QntDialog : Form
    {
        public QntDialog()
        {
            this.InitializeComponent();
            this.StartPosition = FormStartPosition.CenterParent;
        }

        public int Qnt = 0;

        public void Quit()
        {
            this.DialogResult = DialogResult.None;
            try
            {
                this.Qnt = int.Parse(this.textBox1.Text);
                this.DialogResult = DialogResult.OK;
                this.Close();
            }
            catch (Exception ex)
            {
                this.textBox1.BackColor = Color.Red;
                this.textBox1.ForeColor = Color.White;
            }
        }

        private void button1_Click(object sender, EventArgs e)
        {
            this.Quit();
        }

        private void textBox1_KeyDown(object sender, KeyEventArgs e)
        {
            if (e.KeyCode == Keys.Enter)
            {
                this.Quit();
            }
        }

        private void textBox1_TextChanged(object sender, EventArgs e)
        {
            this.textBox1.BackColor = Color.White;
            this.textBox1.ForeColor = Color.Black;
        }
    }
}
