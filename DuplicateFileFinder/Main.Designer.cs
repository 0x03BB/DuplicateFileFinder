namespace DuplicateFileFinder
{
    partial class Main
    {
        /// <summary>
        ///  Required designer variable.
        /// </summary>
        private System.ComponentModel.IContainer components = null;

        /// <summary>
        ///  Clean up any resources being used.
        /// </summary>
        /// <param name="disposing">true if managed resources should be disposed; otherwise, false.</param>
        protected override void Dispose(bool disposing)
        {
            if (disposing && (components != null))
            {
                components.Dispose();
            }
            base.Dispose(disposing);
        }

        #region Windows Form Designer generated code

        /// <summary>
        ///  Required method for Designer support - do not modify
        ///  the contents of this method with the code editor.
        /// </summary>
        private void InitializeComponent()
        {
            selectFolder = new Button();
            selectedFolder = new Label();
            progressBar = new ProgressBar();
            status = new Label();
            action = new Button();
            databaseDialog = new SaveFileDialog();
            copyException = new Button();
            duplicateList = new CheckedListBox();
            deleteSelected = new Button();
            SuspendLayout();
            // 
            // selectFolder
            // 
            selectFolder.Location = new Point(12, 12);
            selectFolder.Name = "selectFolder";
            selectFolder.Size = new Size(88, 23);
            selectFolder.TabIndex = 0;
            selectFolder.Text = "Select Folder";
            selectFolder.UseVisualStyleBackColor = true;
            selectFolder.Click += SelectFolder_Click;
            // 
            // selectedFolder
            // 
            selectedFolder.AutoEllipsis = true;
            selectedFolder.AutoSize = true;
            selectedFolder.Location = new Point(106, 16);
            selectedFolder.Name = "selectedFolder";
            selectedFolder.Size = new Size(0, 15);
            selectedFolder.TabIndex = 1;
            // 
            // progressBar
            // 
            progressBar.Anchor = AnchorStyles.Top | AnchorStyles.Left | AnchorStyles.Right;
            progressBar.Location = new Point(12, 41);
            progressBar.Name = "progressBar";
            progressBar.Size = new Size(776, 23);
            progressBar.TabIndex = 3;
            // 
            // status
            // 
            status.AutoSize = true;
            status.Location = new Point(12, 74);
            status.Name = "status";
            status.Size = new Size(0, 15);
            status.TabIndex = 4;
            // 
            // action
            // 
            action.Anchor = AnchorStyles.Top | AnchorStyles.Right;
            action.Enabled = false;
            action.Location = new Point(713, 12);
            action.Name = "action";
            action.Size = new Size(75, 23);
            action.TabIndex = 2;
            action.Text = "Refresh";
            action.UseVisualStyleBackColor = true;
            action.Click += Action_Click;
            // 
            // databaseDialog
            // 
            databaseDialog.Filter = "Database files|*.sqlite";
            databaseDialog.OverwritePrompt = false;
            databaseDialog.Title = "Select Database File";
            // 
            // copyException
            // 
            copyException.Anchor = AnchorStyles.Top | AnchorStyles.Right;
            copyException.Location = new Point(683, 70);
            copyException.Name = "copyException";
            copyException.Size = new Size(105, 23);
            copyException.TabIndex = 5;
            copyException.Text = "Copy Exception";
            copyException.UseVisualStyleBackColor = true;
            copyException.Visible = false;
            copyException.Click += CopyException_Click;
            // 
            // duplicateList
            // 
            duplicateList.Anchor = AnchorStyles.Top | AnchorStyles.Bottom | AnchorStyles.Left | AnchorStyles.Right;
            duplicateList.CheckOnClick = true;
            duplicateList.FormattingEnabled = true;
            duplicateList.Location = new Point(12, 99);
            duplicateList.Name = "duplicateList";
            duplicateList.Size = new Size(776, 292);
            duplicateList.TabIndex = 6;
            duplicateList.ItemCheck += DuplicateList_ItemCheck;
            duplicateList.DoubleClick += DuplicateList_DoubleClick;
            // 
            // deleteSelected
            // 
            deleteSelected.Anchor = AnchorStyles.Bottom | AnchorStyles.Left | AnchorStyles.Right;
            deleteSelected.Location = new Point(347, 404);
            deleteSelected.Name = "deleteSelected";
            deleteSelected.Size = new Size(106, 23);
            deleteSelected.TabIndex = 7;
            deleteSelected.Text = "Delete Selected";
            deleteSelected.UseVisualStyleBackColor = true;
            // 
            // Main
            // 
            AutoScaleDimensions = new SizeF(7F, 15F);
            AutoScaleMode = AutoScaleMode.Font;
            ClientSize = new Size(800, 439);
            Controls.Add(deleteSelected);
            Controls.Add(duplicateList);
            Controls.Add(copyException);
            Controls.Add(action);
            Controls.Add(status);
            Controls.Add(progressBar);
            Controls.Add(selectedFolder);
            Controls.Add(selectFolder);
            Name = "Main";
            Text = "Duplicate File Finder";
            FormClosing += Main_FormClosing;
            ResumeLayout(false);
            PerformLayout();
        }

        #endregion

        private Button selectFolder;
        private Label selectedFolder;
        private ProgressBar progressBar;
        private CheckedListBox duplicateList;
        private Label status;
        private Button action;
        private SaveFileDialog databaseDialog;
        private Button copyException;
        private Button deleteSelected;
    }
}