using System;
using System.ComponentModel;
using System.Diagnostics;
using System.IO;
using System.Linq;
using System.Reflection;
using System.Runtime.CompilerServices;
using System.Windows;
using System.Windows.Forms;
using System.Windows.Input;
using System.Windows.Threading;
using System.Xml.Serialization;
using MessageBox = System.Windows.Forms.MessageBox;

namespace HuntHighLightBackUper
{
	/// <summary>
	/// Interaction logic for MainWindow.xaml
	/// </summary>
	public partial class MainWindow : Window, INotifyPropertyChanged
	{
		private DispatcherTimer scanner;
		private XmlParser parser;
		private int processedCount;
		private int totalCount;

		public event PropertyChangedEventHandler PropertyChanged;
		public string TempFolderPath { get => parser.Settings.TempFolderPath; set { parser.Settings.TempFolderPath = value; RaisePropertyChanged(); } }
		public string SaveFolderPath { get => parser.Settings.SaveFolderPath; set { parser.Settings.SaveFolderPath = value; RaisePropertyChanged(); } }
		public int ProcessedCount { get => processedCount; set { processedCount = value; RaisePropertyChanged(); } }
		public int TotalCount { get => totalCount; set { totalCount = value; RaisePropertyChanged(); } }

		public MainWindow()
		{
			InitializeComponent();
			DataContext = this;
			ProcessedCount = 0;

			parser = new XmlParser();
			parser.LoadSettings();
			this.Top = parser.Settings.Top;
			this.Left = parser.Settings.Left;

			scanner = new DispatcherTimer();
			scanner.Tick += Scanner_Tick;
			scanner.Interval = TimeSpan.FromMilliseconds(2000);
			scanner.Start();
		}

		private void Scanner_Tick(object sender, EventArgs e)
		{
			try
			{
				TotalCount = Directory.GetFiles(SaveFolderPath).Length;

				if (Directory.Exists(TempFolderPath))
				{
					string[] paths = Directory.GetFiles(TempFolderPath);

					if (paths.Length == 0) return;
					else
					{
						foreach (string p in paths)
						{
							string filename = p.Split('\\').Last();
							if (File.Exists(SaveFolderPath + '\\' + filename))
							{
								if (new FileInfo(SaveFolderPath + '\\' + filename).Length == new FileInfo(p).Length)
								{
									continue;
								}
								else
								{
									File.Copy(p, SaveFolderPath + '\\' + filename, true);
								}
							}
							else
							{
								File.Copy(p, SaveFolderPath + '\\' + filename);
								ProcessedCount++;
							}
						}
					}
				}
			}
			catch (Exception ex)
			{

			}
		}

		private void Window_Closing(object sender, System.ComponentModel.CancelEventArgs e)
		{
			scanner.Stop();
			parser.Settings.Top = this.Top;
			parser.Settings.Left = this.Left;
			parser.SaveSettings();
		}

		public void RaisePropertyChanged([CallerMemberName] string name = null)
		{
			PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(name));
		}

		private void TempFolder_textBox_PreviewMouseDown(object sender, MouseButtonEventArgs e)
		{
			FolderBrowserDialog dialog = new FolderBrowserDialog();
			if (File.Exists(TempFolderPath))
			{
				dialog.SelectedPath = TempFolderPath;
			}

			if (dialog.ShowDialog() == System.Windows.Forms.DialogResult.OK)
			{
				TempFolderPath = dialog.SelectedPath;
			}
		}

		private void SaveFolder_textBox_PreviewMouseDown(object sender, MouseButtonEventArgs e)
		{
			FolderBrowserDialog dialog = new FolderBrowserDialog();
			if (File.Exists(SaveFolderPath))
			{
				dialog.SelectedPath = SaveFolderPath;
			}

			if (dialog.ShowDialog() == System.Windows.Forms.DialogResult.OK)
			{
				SaveFolderPath = dialog.SelectedPath;
			}
		}

		private void Close_button_Click(object sender, RoutedEventArgs e)
		{
			this.Close();
		}

		private void Grid_MouseDown(object sender, MouseButtonEventArgs e)
		{
			this.DragMove();
		}

		private void TempFolderOpen_button_Click(object sender, RoutedEventArgs e)
		{
			try
			{
				if (Directory.Exists(TempFolderPath))
					Process.Start(TempFolderPath);
				else
					MessageBox.Show(TempFolderPath + " does not exist.");
			}
			catch (Exception ex)
			{
				MessageBox.Show(ex.Message);
			}
		}

		private void SaveFolderOpen_button_Click(object sender, RoutedEventArgs e)
		{
			try
			{
				if (Directory.Exists(SaveFolderPath))
					Process.Start(SaveFolderPath);
				else
					MessageBox.Show(SaveFolderPath + " does not exist.");
			}
			catch (Exception ex)
			{
				MessageBox.Show(ex.Message);
			}
		}
	}

	public class XmlParser
	{
		public XmlSerializer Serializer;
		public Settings Settings;

		public XmlParser()
		{
			Serializer = new XmlSerializer(typeof(Settings));
		}

		public void SaveSettings()
		{
			try
			{
				using (StreamWriter writer = new StreamWriter(Path.GetDirectoryName(Assembly.GetExecutingAssembly().Location) + @"\Settings.xml"))
				{
					Serializer.Serialize(writer, Settings);
				}
			}
			catch (Exception ex)
			{
				MessageBox.Show(ex.Message);
			}
		}

		public void LoadSettings()
		{
			try
			{
				if (File.Exists(Path.GetDirectoryName(Assembly.GetExecutingAssembly().Location) + @"\Settings.xml"))
				{
					using (StreamReader reader = new StreamReader(Path.GetDirectoryName(Assembly.GetExecutingAssembly().Location) + @"\Settings.xml"))
					{
						Settings = Serializer.Deserialize(reader) as Settings;
					}
				}
				else
				{
					Settings = new Settings();
				}
			}
			catch (Exception ex)
			{
				MessageBox.Show(ex.Message);
				Settings = new Settings();
			}
		}
	}

	public class Settings
	{
		public string TempFolderPath { get; set; }
		public string SaveFolderPath { get; set; }
		public double Top { get; set; }
		public double Left { get; set; }

		public Settings()
		{
			TempFolderPath = Environment.GetFolderPath(Environment.SpecialFolder.UserProfile) + @"\AppData\Local\Temp\Highlights\Hunt  Showdown";

			DriveInfo[] drives = DriveInfo.GetDrives();
			Debug.Assert(drives.Length < 1);
			if (drives.Length == 1)
			{
				SaveFolderPath = Path.Combine(Environment.GetFolderPath(Environment.SpecialFolder.Desktop), "HuntShowdown_Highlights");
			}
			else
			{
				SaveFolderPath = Path.Combine(drives[1].Name, "HuntShowdown_Highlights");
			}

			Top = 100;
			Left = 100;
		}
	}
}
