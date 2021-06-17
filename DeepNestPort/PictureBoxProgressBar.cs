namespace DeepNestPort
{
    using System;
    using System.Drawing;
    using System.Windows.Forms;

    public class PictureBoxProgressBar : PictureBox
    {
        public PictureBoxProgressBar()
        {
            this.bmp = new Bitmap(this.Width, this.Height);
            this.gr = Graphics.FromImage(this.bmp);
            this.SizeChanged += this.PictureBoxProgressBar_SizeChanged;
        }

        private void PictureBoxProgressBar_SizeChanged(object sender, EventArgs e)
        {
            this.bmp = new Bitmap(this.Width, this.Height);
            this.gr = Graphics.FromImage(this.bmp);
        }

        private Bitmap bmp;
        private Graphics gr;
        public float Value;

        public void UpdateImg()
        {
            this.gr.FillRectangle(Brushes.White, 0, 0, this.Width, this.Height);
            this.gr.FillRectangle(Brushes.LightGreen, 0, 0, (this.Value / 100.0f) * this.Width, this.Height);
            this.gr.DrawRectangle(Pens.Black, 0, 0, this.Width - 1, this.Height - 1);
            this.Image = this.bmp;

            // gr.DrawString((Value * 100.0f),new Font("Arial",18),);
        }
    }
}
