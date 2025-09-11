using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Data;
using System.Drawing;
using System.Drawing.Imaging;
using System.Text;
using System.Windows.Forms;
using System.IO;

namespace ViewMNIST
{
	public partial class PictureView : Form
	{

		Bitmap tmpBitmap;

		public PictureView()
		{
			InitializeComponent();
			ReadPicture();
			ReadLabels();
		}

		byte[,] tmpPicture = new byte[60000,28 * 28];
		string[] tmpLabels = new string [60000];
		int PictureNumber=0;

		public void ReadPicture()
		{
			//FileStream fs = new FileStream("train-images.idx3-ubyte", FileMode.Open, FileAccess.Read);
			FileStream fs = new FileStream("t10k-images.idx3-ubyte", FileMode.Open, FileAccess.Read);
			BinaryReader r = new BinaryReader(fs);
			for (int i = 0; i < 4; i++)
			{
				r.ReadInt32();
			}
			for (int n = 0; n < 10000-1 ; n++)
			{
				for (int i = 0; i < 28 * 28; i++)
				{
					tmpPicture[n,i] = r.ReadByte();
				}
			}
		}

		public void ReadLabels()
		{
			//FileStream fs = new FileStream("train-labels.idx1-ubyte", FileMode.Open, FileAccess.Read);
			FileStream fs = new FileStream("t10k-labels.idx1-ubyte", FileMode.Open, FileAccess.Read);
			BinaryReader r = new BinaryReader(fs);
			for (int i = 0; i < 2; i++)
			{
				r.ReadInt32();
			}
			for (int n = 0; n < 10000 - 1; n++)
			{
				tmpLabels[n] = r.ReadByte().ToString();
			}
		}

		public void ImageDraw()
		{
			pictureBox.Image = Image.FromFile("map1.jpg");
			tmpBitmap = new Bitmap(pictureBox.Image);

			Bitmap newBitmap = new Bitmap(21, 21);

			string tmp="";

			int g = 0, x = -1, y = -1;
			for (int i = 0; i < 28; i++)
			{
				if (i <= 3 || i >= 25)
				{
				}
				else
				{
					y++;
				}
				x = -1;
				for (int j = 0; j < 28; j++)
				{
					if (i <= 3 || i >= 25 || j <= 3 || j >= 25)
					{
						tmpBitmap.SetPixel(j, i, Color.FromArgb(255, Color.Red));
						g++;
						continue;
					}

					if (j <= 3 || j >= 25)
					{ }
					else
					{
						x++;
					}
					byte tmpValue=0;
					if (tmpPicture[PictureNumber, g] > 100) tmpValue = 255;
					tmpBitmap.SetPixel(j, i, Color.FromArgb(tmpValue, Color.Black));

					if (tmpValue == 0)
					{ 
						newBitmap.SetPixel(x, y, Color.White);
						tmp += "0";
					}
					else
					{ 
						newBitmap.SetPixel(x, y, Color.Black);
						tmp += "1";
					}

					g++;
				}
			}
			// Обновление карты на форме
			pictureBox.Image = tmpBitmap;

			if (PictureNumber <= 10000)
			{
				newBitmap.Save("e" + PictureNumber.ToString() + ".png", ImageFormat.Png);
				//xFile.WriteFile("ExaminationSet.txt", tmpLabels[PictureNumber] + ":" + tmp);
				PictureNumber++;
			}
			PictureNumberTxt.Text = PictureNumber.ToString();
		}


		private void button1_Click(object sender, EventArgs e)
		{
			PictureNumber++;
			ImageDraw();
		}

	}
}