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
			//ReadE();
			ReadL();
		}

		public void ReadL()
		{
			//FashionMNIST
			ReadPicture("train-images-idx3-ubyte", 60000);
			ReadLabels("train-labels-idx1-ubyte", 60000);
			//MNIST
			//ReadPicture("train-images.idx3-ubyte", 60000);
			//ReadLabels("train-labels.idx1-ubyte", 60000);
		}

		public void ReadE()
		{
			//FashionMNIST
			ReadPicture("t10k-images-idx3-ubyte", 10000);
			ReadLabels("t10k-labels-idx1-ubyte", 10000);
			//MNIST
			//ReadPicture("t10k-images.idx3-ubyte", 10000);
			//ReadLabels("t10k-labels.idx1-ubyte", 10000);
		}

		byte[,] tmpPicture = new byte[60000,28 * 28];
		string[] tmpLabels = new string [60000];
		int PictureNumber=0;

		public void ReadPicture(string argFileName, int argCount)
		{
			FileStream fs = new FileStream(argFileName, FileMode.Open, FileAccess.Read);
			BinaryReader r = new BinaryReader(fs);
			for (int i = 0; i < 4; i++)
			{
				r.ReadInt32();
			}
			for (int n = 0; n < argCount; n++)
			{
				for (int i = 0; i < 28 * 28; i++)
				{
					tmpPicture[n,i] = r.ReadByte();
				}
			}
		}

		public void ReadLabels(string argFileName, int argCount)
		{
			FileStream fs = new FileStream(argFileName, FileMode.Open, FileAccess.Read);

			BinaryReader r = new BinaryReader(fs);
			for (int i = 0; i < 2; i++)
			{
				r.ReadInt32();
			}
			for (int n = 0; n < argCount; n++)
			{
				tmpLabels[n] = r.ReadByte().ToString();
			}
		}

		public void ImageDraw()
		{

			int sizeX = 28; // 21
			int sizeY = 28; // 21

			Bitmap newBitmap = new Bitmap(sizeX, sizeY);

			string tmp="";

			int frameX = -1; // 3
			int frameY = 28; // 25


			int g = 0, x = -1, y = -1;
			for (int i = 0; i < 28; i++)
			{
				if (i <= frameX || i >= frameY)
				{
				}
				else
				{
					y++;
				}
				x = -1;
				for (int j = 0; j < 28; j++)
				{
					if (i <= frameX || i >= frameY || j <= frameX || j >= frameY)
					{
						tmpBitmap.SetPixel(j, i, Color.FromArgb(255, Color.Red));
						g++;
						continue;
					}

					if (j <= frameX || j >= frameY)
					{ }
					else
					{
						x++;
					}
					byte tmpValue=0;
					if (tmpPicture[PictureNumber, g] > 128) tmpValue = 255;
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

			// Обновление на форме
			pictureBox.Image = tmpBitmap;

			if (PictureNumber <= 10000)
			{
				//newBitmap.Save("e" + PictureNumber.ToString() + ".png", ImageFormat.Png);
				//File.AppendAllText("ExaminationSet.txt", tmpLabels[PictureNumber] + ":" + tmp + "\n");
				PictureNumber++;
			}
			PictureNumberTxt.Text = PictureNumber.ToString();
		}


		private void button1_Click(object sender, EventArgs e)
		{
			ImageDraw();
		}

		private void SaveNext(string argFileName, int argCount)
		{
			string tmp = "";

			int frameX = -1; // 3
			int frameY = 28; // 25

			int g = 0;
			for (int i = 0; i < 28; i++)
			{
				for (int j = 0; j < 28; j++)
				{
					if (i <= frameX || i >= frameY || j <= frameX || j >= frameY)
					{
						g++;
						continue;
					}

					byte tmpValue = 0;
					if (tmpPicture[PictureNumber, g] > 128) tmpValue = 255;

					if (tmpValue == 0)
					{
						tmp += "0";
					}
					else
					{
						tmp += "1";
					}

					g++;
				}
			}

			if (PictureNumber <= argCount)
			{
				File.AppendAllText(argFileName, tmpLabels[PictureNumber] + ":" + tmp + "\n");
				PictureNumber++;
			}
		}

		private void SaveAllData_Click(object sender, EventArgs e)
		{
			PictureNumber = 0;

			int count = 60000;
			for (int i = 0; i < count; i++)
			{
				//SaveNext("ExaminationSet.txt", count);
				SaveNext("LearningSet.txt", count);
			}
		}
	}
}