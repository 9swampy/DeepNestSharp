namespace DeepNestPort
{
  using System;
  using System.Drawing;
  using System.Windows.Forms;

  public class PictureBoxProgressBar : PictureBox
  {
    public PictureBoxProgressBar()
    {
      bmp = new Bitmap(Width, Height);
      gr = Graphics.FromImage(bmp);
      SizeChanged += PictureBoxProgressBar_SizeChanged;
    }

    private void PictureBoxProgressBar_SizeChanged(object sender, EventArgs e)
    {
      bmp = new Bitmap(Width, Height);
      gr = Graphics.FromImage(bmp);
    }

    private Bitmap bmp;
    private Graphics gr;
    public double Value;

    public void UpdateImg()
    {
      gr.FillRectangle(Brushes.White, 0, 0, Width, Height);
      gr.FillRectangle(Brushes.LightGreen, 0, 0, (float)(Value / 100.0) * Width, Height);
      gr.DrawRectangle(Pens.Black, 0, 0, Width - 1, Height - 1);
      Image = bmp;

      //gr.DrawString((Value * 100.0f),new Font("Arial",18),);
    }
  }
}
