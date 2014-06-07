using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.IO;

namespace mplistDumpCSV
{
	class Program
	{
		private const string JSDATADIR = "\\JSDATA";
		private const string JSMUSICDIR = "\\JSMUSIC";
		private const string JSMPLISTFILE = "mplist";
		private const int RECORDSIZE = 320;	// 0x140
		private const int FILENAMEOFFSET = 0x0;
		private const int FILENAMELENGTH = 256;
		private const int TITLEOFFSET = 0x100;
		private const int TITLELENGTH = 24;
		private const int ARTISTNAMEOFFSET = 0x11A;
		private const int ARTISTNAMELENGTH = 24;
		private const int SONGNUMBEROFFSET = 0x13E;
		private const int SONGNUMBERLENGTH = 1;

		static void Main(string[] args)
		{
			if (1 != args.Length)
			{
				printUsage();
				return;
			}

			//string driveLetter = args[0];
			//string filePath = driveLetter + JSDATADIR + "\\" + JSMPLISTFILE;
			string filePath = args[0];
			byte[] buffer = new byte[RECORDSIZE];

			try
			{
				if (!File.Exists(filePath))
				{
					Console.WriteLine("ファイルが存在しません。");
					return;
				}

				using (FileStream fso = File.Open(filePath, FileMode.Open))
				{
					long length = fso.Length;
					int seekPosition = 0;

					if (0 != (length % RECORDSIZE))
					{
						Console.WriteLine("ファイルサイズがおかしいです。");
						return;
					}

					printHeader();

					while (seekPosition < length)
					{
						fso.Read(buffer, 0, RECORDSIZE);
						printSongInfo(buffer);
						seekPosition += RECORDSIZE;
					}
				}
			}
			catch (Exception)
			{

				throw;
			}
		}

		private static void printHeader()
		{
			Console.WriteLine("NO,タイトル,アーティスト,ファイル名");
		}

		private static void printSongInfo(byte[] buffer)
		{
			Encoding cp932Encoding = Encoding.GetEncoding(932);
			byte[] fileNameBytes = new byte[FILENAMELENGTH];
			byte[] titleBytes = new byte[TITLELENGTH];
			byte[] artistNameBytes = new byte[ARTISTNAMELENGTH];
			byte[] songNumberBytes = new byte[SONGNUMBERLENGTH];

			Array.Copy(buffer, FILENAMEOFFSET, fileNameBytes, 0, FILENAMELENGTH);
			Array.Copy(buffer, TITLEOFFSET, titleBytes, 0, TITLELENGTH);
			Array.Copy(buffer, ARTISTNAMEOFFSET, artistNameBytes, 0, ARTISTNAMELENGTH);
			Array.Copy(buffer, SONGNUMBEROFFSET, songNumberBytes, 0, SONGNUMBERLENGTH);

			fillNullValueToSpace(fileNameBytes);
			fillNullValueToSpace(titleBytes);
			fillNullValueToSpace(artistNameBytes);

			string fileName = cp932Encoding.GetString(fileNameBytes);
			string title = cp932Encoding.GetString(titleBytes);
			string artistName = cp932Encoding.GetString(artistNameBytes);
			int songNumber = (int)songNumberBytes[0];

			Console.WriteLine(@"{0},""{1}"",""{2}"",""{3}""", songNumber, title.TrimEnd(), artistName.TrimEnd(), fileName.TrimEnd());
		}

		private static void fillNullValueToSpace(byte[] buffer)
		{
			for (int i = 0; i < buffer.Length; i++)
			{
				if (0x00 == buffer[i])
				{
					buffer[i] = 0x20;	// ASCII SPACE(0x20)
				}
			}
		}

		private static void printUsage()
		{
			Console.WriteLine("Usage Example: mplistDumpCSV <<Path to mplist>>");
		}
	}
}
