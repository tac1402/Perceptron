// Author: Sergej Jakovlev <tac1402@gmail.com>
// Copyright (C) 2025 Sergej Jakovlev

using System;
using System.Collections.Generic;
using System.IO;
using System.Text;

namespace Tac.Perceptron
{

	/// <summary>
	/// Блочное обучение
	/// </summary>
	public class NeironNetBT : NeironNetTree
	{
		private int BCount; // Количество блоков обучения
		private int BIteration; // Количество итераций на один блок обучения
		private int BLenght { get { return HCount / BCount; } }


		public NeironNetBT(int argSCount, int argACount, int argRCount, int argHCount, int argBCount, int argBIteration) :
			base(argSCount, argACount, argRCount, argHCount)
		{
			BCount = argBCount;
			BIteration = argBIteration;

		}


		/// <summary>
		/// Активация S-A слоя
		/// </summary>
		/// <param name="argStimulNumber">Номер примера в выборке</param>
		/// <param name="argMode">0 - обучение, 1 - экзамен</param>
		private void SActivationLB(int argStimulNumber, int argSaveStimulNumber)
		{
			for (int i = 0; i < ACount; i++)
			{
				AssociationsField[i] = 0;
			}

			// Кинем на сенсоры обучающий пример
			SensorsField = LearnedStimuls[argStimulNumber];

			DateTime begin = DateTime.Now;

			for (int i = 0; i < SCount; i++)
			{
				if (SensorsField[i] == true)
				{
					for (int j = 0; j < ACount; j++)
					{
						AssociationsField[j] += WeightSA[i][j];
					}
				}
			}
			double t = (DateTime.Now - begin).TotalMilliseconds;
			aTime += t;

			// Запомним как на этот пример реагировали A - элементы
			for (int j = 0; j < ACount; j++)
			{
				if (AssociationsField[j] > 0)
				{
					AHConnections[argSaveStimulNumber].Add(j);
				}
			}

			// Check
			if (AHConnections[argSaveStimulNumber].Count < AHMinimum)
			{
				AHMinimum = AHConnections[argSaveStimulNumber].Count;
			}
		}

		private void ClearAH()
		{
			for (int j = 0; j < BLenght; j++)
			{
				AHConnections[j].Clear();
			}
		}

		private void SaveAH(string argFileName)
		{
			Console.Write(".");
			FileStream file = new FileStream(argFileName, FileMode.Create);
			BinaryWriter writer = new BinaryWriter(file);

			for (int i = 0; i < BLenght; i++)
			{
				StringBuilder line = new StringBuilder();
				for (int j = 0; j < AHConnections[i].Count; j++)
				{
					writer.Write(AHConnections[i][j]);
				}
				writer.Write(-1);
			}
			writer.Close();
			file.Close();
			Console.WriteLine("SaveAH");
		}

		private void LoadAH(string argFileName)
		{
			Console.Write(".");
			FileStream file = new FileStream(argFileName, FileMode.Open);
			BinaryReader reader = new BinaryReader(file);

			int i = 0;
			while (reader.BaseStream.Position != reader.BaseStream.Length)
			{
				int v = reader.ReadInt32();
				if (v != -1)
				{
					AHConnections[i].Add(v);
				}
				else
				{
					i++;
				}
			}
			reader.Close();
			file.Close();
			Console.WriteLine("LoadAH");
		}



		public override void Learned()
		{
			AHMinimum = ACount;
			for (int i = 0; i < ACount; i++)
			{
				InitSA(i);
			}

			// Делаем очень много итераций
			for (int n = 0; n < 100000; n++)
			{
				int bError = 0;

				for (int b = 0; b < BCount; b++)
				{
					bool saActivate = false;
					ClearAH();

					if (n > 0)
					{
						LoadAH("AH_" + b.ToString() + ".bin");
						saActivate = true;
					}

					for (int bn = 0; bn < BIteration; bn++)
					{
						int Error = 0;

						DateTime begin = DateTime.Now;
						aTime = 0;
						// За каждую итерацию прокручиваем все примеры из блока обучающей выборки
						for (int i = 0; i < BLenght; i++)
						{
							int stimulNumber = b * BLenght + i;

							// Активируем S-элементы, т.е. подаем входы и рассчитываем средний слой A-элементы
							if (saActivate == false) 
							{ 
								SActivationLB(stimulNumber, i);
								if (i % 10000 == 0)
									Console.WriteLine("AHMinimum = " + AHMinimum.ToString());
							}
							// Активируем R-элементы, т.е. рассчитываем выходы
							RActivation(i);
							// Узнаем ошибся перцептрон или нет, если ошибся отправляем на обучение
							bool e = GetError(stimulNumber);
							if (e == true)
							{
								LearnedStimul(i);
								Error++; // Число ошибок, если в конце итерации =0, то выскакиваем из обучения.
							}
						}
						saActivate = true;
						double t = (DateTime.Now - begin).TotalMilliseconds;
						Console.WriteLine(n.ToString() + "." + bn.ToString() + " - " + "(" + b.ToString() + ")" + Error.ToString() + " - " + t.ToString() + " ms");
						Console.WriteLine("\t" + aTime.ToString() + " ms");
						if (Error == 0) { break; }
						bError += Error;
					}

					if (n == 0)
					{
						SaveAH("AH_" + b.ToString() + ".bin");
					}

				}
				if (bError == 0) { break; }
			}
		}

	}
}
