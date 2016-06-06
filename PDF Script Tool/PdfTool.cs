﻿//-----------------------------------------------------------------------------------------------------------
// <copyright file="PdfScriptTool.cs" company="Michael Brandon Morris">
//     Copyright © Michael Brandon Morris 2016
// </copyright>
//-----------------------------------------------------------------------------------------------------------

namespace PdfTool
{
    using System.Linq;
    using Action = System.Action;
    using DialogResult = System.Windows.Forms.DialogResult;
    using EventArgs = System.EventArgs;
    using Exception = System.Exception;
    using Form = System.Windows.Forms.Form;
    using Func = System.Func<System.Threading.Tasks.Task>;
    using IProgress = System.IProgress<ProgressReport>;
    using ListString = System.Collections.Generic.List<string>;
    using MessageBox = System.Windows.Forms.MessageBox;
    using Path = System.IO.Path;
    using Resources = Properties.Resources;
    using StringComparison = System.StringComparison;
    using Task = System.Threading.Tasks.Task;

    internal partial class PdfTool : Form, IProgress
    {
        private const bool FileViewFileIsChecked = true;

        private const bool OpenFileDialogAllowMultiple = true;

        private PdfProcessor pdfProcessor;

        internal PdfTool()
        {
            InitializeComponent();
            InitializeOpenFileDialog();
            pdfProcessor = new PdfProcessor();
        }

        public void Report(ProgressReport progressReport)
        {
            if (InvokeRequired)
            {
                Invoke((Action)(() => Report(progressReport)));
            }
            else
            {
                progressBar.Value = progressReport.Percent;
            }
        }

        internal async Task PerformTask(Func function)
        {
            if (fileView.CheckedItems.Count > 0)
            {
                Enabled = false;
                try
                {
                    pdfProcessor.Files =
                        fileView.CheckedItems.OfType<string>().ToList();
                    await function();
                }
                catch (Exception e)
                {
                    ShowException(e);
                }

                ShowMessage(Resources.FilesSavedInMessage +
                    PdfProcessor.OutputPath);
                PdfProcessor.ClearProcessing();
                fileView.Items.Clear();
                progressBar.Value = 0;
                Enabled = true;
            }
            else
            {
                ShowMessage(Resources.NoFilesSelectedErrorMessage);
            }
        }

        private static void ShowException(Exception e)
        {
            ShowMessage(e.Message);
        }

        private static void ShowMessage(string message)
        {
            MessageBox.Show(message);
        }

        private async void ConvertOnly_Click(object sender, EventArgs e)
        {
            await PerformTask(() => pdfProcessor.ProcessFiles(this));
        }

        private bool FileIsAlreadySelected(
                    string filename,
                    ListString selectedFilenames,
                    out string filenameWithoutExtensionReturn)
        {
            filenameWithoutExtensionReturn = null;
            foreach (var selectedFilename in selectedFilenames)
            {
                var filenameWithoutExtension =
                    Path.GetFileNameWithoutExtension(filename);
                var selectedFilenameWithoutExtension =
                    Path.GetFileNameWithoutExtension(selectedFilename);
                if (string.Equals(
                    filenameWithoutExtension,
                    selectedFilenameWithoutExtension,
                    StringComparison.InvariantCultureIgnoreCase))
                {
                    filenameWithoutExtensionReturn = filenameWithoutExtension;
                    return true;
                }
            }

            return false;
        }

        private void InitializeOpenFileDialog()
        {
            openFileDialog.Filter = Resources.OpenFileDialogFilter;
            openFileDialog.Multiselect = OpenFileDialogAllowMultiple;
            openFileDialog.Title = Resources.OpenFileDialogTitle;
        }

        private void SelectFiles_Click(object sender, EventArgs e)
        {
            var dialogResult = openFileDialog.ShowDialog();
            if (dialogResult == DialogResult.OK)
            {
                foreach (var filename in openFileDialog.FileNames)
                {
                    string filenameWithoutExtension;
                    if (FileIsAlreadySelected(
                        filename,
                        fileView.CheckedItems.OfType<string>().ToList(),
                        out filenameWithoutExtension))
                    {
                        ShowMessage("A file with the name \"" +
                                filenameWithoutExtension +
                                "\" is already selected.");
                    }
                    else
                    {
                        fileView.Items.Add(
                            PdfProcessor.CopyFileToProcessing(filename),
                            FileViewFileIsChecked);
                    }
                }
            }
        }

        private async void TimeStampDefaultDay_Click(
            object sender, EventArgs e)
        {
            await PerformTask(() => pdfProcessor.ProcessFiles(
                this,
                Field.DefaultTimeStampField,
                Script.TimeStampOnPrintDefaultDayScript));
        }

        private async void TimeStampDefaultMonth_Click(
            object sender, EventArgs e)
        {
            await PerformTask(() => pdfProcessor.ProcessFiles(
                this,
                Field.DefaultTimeStampField,
                Script.TimeStampOnPrintDefaultMonthScript));
        }
    }
}