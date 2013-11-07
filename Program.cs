using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.IO;

namespace DSXCompare {
	class Program {
		static List<string> lineDifferences = new List<string>();
		static List<string> exportDifferences = new List<string>();
		static List<string> newFiles = new List<string>();
		static List<string> matchedFileNames = new List<string>();
		static List<string> deletedFiles = new List<string>();

		static void Main(string[] args) {

			if (args.Length != 2) {
				Console.WriteLine("Please pass 2 directories to compare:");
				Console.WriteLine("   eg. DSXComparer.exe <dir1> <dir2>  ");

				return;
			}

			var directoryOneFiles = new DirectoryInfo(args[0]).GetFiles("*.dsx").ToList();
			var directoryTwoFiles = new DirectoryInfo(args[1]).GetFiles("*.dsx").ToList();

			foreach (var file in directoryOneFiles) {
				FileInfo matchedFile = null;
				foreach (var f in directoryTwoFiles) {
					if (f.Name == file.Name) {
						matchedFile = f;
						matchedFileNames.Add(f.Name);
						break;
					}
				}

				if (matchedFile == null) {
					newFiles.Add(file.Name);
					continue;
				}

				var lines1 = File.ReadAllLines(file.FullName).ToList();
				var lines2 = File.ReadAllLines(matchedFile.FullName).ToList();

				for (var i = 0; i < lines1.Count; i++) {
					if (lines1[i] != lines2[i]) {
						if (IsStartOfBinary(lines1[i])) {
							break;
						}
						if (IsExportDifference(lines1[i], lines2[i])) {
							exportDifferences.Add(file.Name);
							break;
						}
						if (IsNotTimeDifference(lines1[i])) {
							var items = lines1[i].Split('"');
							bool wasDate = false;
							foreach (var item in items) {
								DateTime foo;

								if (DateTime.TryParse(item, out foo)) {
									wasDate = true;
								}
							}
							if (!wasDate) {
								lineDifferences.Add(file.Name);
								break;
							}
						}
					}
				}
			}

			foreach (var file in directoryTwoFiles) {
				bool wasDeleted = true;
				foreach (string name in matchedFileNames) {
					if (name == file.Name) {
						wasDeleted = false;
						break;
					}
				}
				if (wasDeleted) {
					deletedFiles.Add(file.Name);
				}
			}

			PrintResults();
		}

		private static bool IsStartOfBinary(string line) {
			return line.Trim().StartsWith("BEGIN DSBPBINARY");
		}

		private static bool IsExportDifference(string line1, string line2) {
			return line1.Trim().StartsWith("COMMENT") && 
						 line2.Trim().StartsWith("BEGIN");
		}

		private static bool IsNotTimeDifference(string line) {
			return !line.Trim().StartsWith("DateModified \"") &&
						 !line.Trim().StartsWith("TimeModified \"") &&
						 !line.Trim().StartsWith("Time \"") &&
						 !line.Trim().StartsWith("Date \"") &&
						 !line.Trim().StartsWith("COMMENT ") &&
						 !line.Trim().StartsWith("*** [Generated at ");
		}

		private static void PrintResults() {
			var allErrors = new List<string>();
			allErrors.Add(lineDifferences.Count + " different lines");
			allErrors.AddRange(lineDifferences);
			allErrors.Add("");
			allErrors.Add(exportDifferences.Count + " exported differently");
			allErrors.AddRange(exportDifferences);
			allErrors.Add("");
			allErrors.Add(newFiles.Count + " new files");
			allErrors.AddRange(newFiles);
			allErrors.Add("");
			allErrors.Add(deletedFiles.Count + " deleted files");
			allErrors.AddRange(deletedFiles);

			allErrors.ForEach(a => Console.WriteLine(a));

			Console.WriteLine("Done!");
			Console.ReadLine();
		}
	}
}
