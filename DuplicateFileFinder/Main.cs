using System.Diagnostics;
using DuplicateFileFinder.Repository;

namespace DuplicateFileFinder;

public partial class Main : Form
{
    private readonly Progress<(ProgressPhase progressPhase, int total, int current)> _progress;

    private List<List<FileInfoSlim>>? _duplicates;
    private CancellationTokenSource? _cancellationTokenSource;
    private bool closeWhenComplete = false;
    private string? _lastException;

    public Main()
    {
        InitializeComponent();
        _progress = new(tuple =>
        {
            if (_cancellationTokenSource is not null && !_cancellationTokenSource.IsCancellationRequested)
            {
                if (tuple.progressPhase == ProgressPhase.Finding)
                {
                    status.Text = "Finding files... " + tuple.total.ToString();
                }
                else
                {
                    progressBar.Style = ProgressBarStyle.Continuous;
                    progressBar.Maximum = tuple.total;
                    progressBar.Value = tuple.current;
                    status.Text = "Hashing file " + tuple.current.ToString() + " of " + tuple.total.ToString();
                }
            }
        });
    }

    private async void SelectFolder_Click(object sender, EventArgs e)
    {
        if (databaseDialog.ShowDialog() == DialogResult.OK)
        {
            selectedFolder.Text = databaseDialog.FileName;
            await Start();
        }
    }

    private async void Action_Click(object sender, EventArgs e)
    {
        if (_cancellationTokenSource is not null)
        {
            Debug.Assert(action.Text == "Cancel");
            _cancellationTokenSource.Cancel();
        }
        else
        {
            Debug.Assert(action.Text == "Refresh");
            await Start();
        }
    }

    private async Task Start()
    {
        _duplicates = null;
        _lastException = null;
        _cancellationTokenSource = new CancellationTokenSource();

        progressBar.Value = 0;
        progressBar.Style = ProgressBarStyle.Marquee;

        status.Text = "Finding files...";

        action.Text = "Cancel";
        action.Enabled = true;

        selectFolder.Enabled = false;

        copyException.Visible = false;

        try
        {
            var duplicateFileFinder = new DuplicateFileFinder(_progress);
            var result = await Task.Run(async () => await duplicateFileFinder.FindDuplicatesAsync(databaseDialog.FileName, _cancellationTokenSource.Token));
            result.Switch(
            duplicates =>
            {
                _duplicates = duplicates.Collection;

                status.Text = "Complete. " + duplicates.Total.ToString() + " duplicate file(s) found.";

                foreach (var fiList in _duplicates)
                {
                    foreach (var fi in fiList)
                    {
                        duplicateList.Items.Add(fi);
                    }
                    duplicateList.Items.Add("—");
                }
            },
            error =>
            {
                status.Text = error.Value;
            });
        }
        catch (OperationCanceledException)
        {
            status.Text = "Canceled.";
        }
        catch (Exception ex)
        {
            status.Text = "Error: Unhandled exception.";
            _lastException = ex.Message + Environment.NewLine + Environment.NewLine + ex.StackTrace;
            copyException.Visible = true;
        }

        _cancellationTokenSource = null;

        progressBar.Value = 0;
        progressBar.Style = ProgressBarStyle.Continuous;

        action.Text = "Refresh";

        selectFolder.Enabled = true;

        if (closeWhenComplete)
        {
            Close();
        }
    }

    private void CopyException_Click(object sender, EventArgs e)
    {
        Clipboard.SetText(_lastException ?? string.Empty);
        MessageBox.Show("Copied to clipboard");
    }

    private void Main_FormClosing(object sender, FormClosingEventArgs e)
    {
        if (_cancellationTokenSource is not null)
        {
            _cancellationTokenSource.Cancel();
            closeWhenComplete = true;
            e.Cancel = true;
        }
    }

    private void DuplicateList_DoubleClick(object sender, EventArgs e)
    {
        if (duplicateList.SelectedItem is FileInfoSlim fi)
        {
            Process.Start(new ProcessStartInfo(Path.Combine(Path.GetDirectoryName(selectedFolder.Text), fi.Path)) { UseShellExecute = true });
        }
    }
}
