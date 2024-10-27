namespace ExSyncPlugin
{
  partial class OptionsPanel
  {
    /// <summary> 
    /// Required designer variable.
    /// </summary>
    private System.ComponentModel.IContainer components = null;

    /// <summary> 
    /// Clean up any resources being used.
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

    #region Component Designer generated code

    /// <summary> 
    /// Required method for Designer support - do not modify 
    /// the contents of this method with the code editor.
    /// </summary>
    private void InitializeComponent()
    {
      this.components = new System.ComponentModel.Container();
      this.label1 = new System.Windows.Forms.Label();
      this.textBox_Executable = new System.Windows.Forms.TextBox();
      this.label2 = new System.Windows.Forms.Label();
      this.textBox_WorkingDirectory = new System.Windows.Forms.TextBox();
      this.label3 = new System.Windows.Forms.Label();
      this.textBox_Args = new System.Windows.Forms.TextBox();
      this.contextMenuStrip1 = new System.Windows.Forms.ContextMenuStrip(this.components);
      this.listView_Logs = new System.Windows.Forms.ListView();
      this.button_Restart = new System.Windows.Forms.Button();
      this.button_Stop = new System.Windows.Forms.Button();
      this.button_ClearLogs = new System.Windows.Forms.Button();
      this.label_Status = new System.Windows.Forms.Label();
      this.SuspendLayout();
      // 
      // label1
      // 
      this.label1.AutoSize = true;
      this.label1.Location = new System.Drawing.Point(3, 9);
      this.label1.Name = "label1";
      this.label1.Size = new System.Drawing.Size(63, 13);
      this.label1.TabIndex = 0;
      this.label1.Text = "Executable:";
      // 
      // textBox_Executable
      // 
      this.textBox_Executable.Anchor = ((System.Windows.Forms.AnchorStyles)(((System.Windows.Forms.AnchorStyles.Top | System.Windows.Forms.AnchorStyles.Left) 
            | System.Windows.Forms.AnchorStyles.Right)));
      this.textBox_Executable.Location = new System.Drawing.Point(72, 6);
      this.textBox_Executable.Name = "textBox_Executable";
      this.textBox_Executable.Size = new System.Drawing.Size(479, 20);
      this.textBox_Executable.TabIndex = 1;
      // 
      // label2
      // 
      this.label2.AutoSize = true;
      this.label2.Location = new System.Drawing.Point(3, 61);
      this.label2.Name = "label2";
      this.label2.Size = new System.Drawing.Size(93, 13);
      this.label2.TabIndex = 0;
      this.label2.Text = "Working directory:";
      // 
      // textBox_WorkingDirectory
      // 
      this.textBox_WorkingDirectory.Anchor = ((System.Windows.Forms.AnchorStyles)(((System.Windows.Forms.AnchorStyles.Top | System.Windows.Forms.AnchorStyles.Left) 
            | System.Windows.Forms.AnchorStyles.Right)));
      this.textBox_WorkingDirectory.Location = new System.Drawing.Point(102, 58);
      this.textBox_WorkingDirectory.Name = "textBox_WorkingDirectory";
      this.textBox_WorkingDirectory.Size = new System.Drawing.Size(449, 20);
      this.textBox_WorkingDirectory.TabIndex = 3;
      // 
      // label3
      // 
      this.label3.AutoSize = true;
      this.label3.Location = new System.Drawing.Point(3, 35);
      this.label3.Name = "label3";
      this.label3.Size = new System.Drawing.Size(28, 13);
      this.label3.TabIndex = 0;
      this.label3.Text = "Args";
      // 
      // textBox_Args
      // 
      this.textBox_Args.Anchor = ((System.Windows.Forms.AnchorStyles)(((System.Windows.Forms.AnchorStyles.Top | System.Windows.Forms.AnchorStyles.Left) 
            | System.Windows.Forms.AnchorStyles.Right)));
      this.textBox_Args.Location = new System.Drawing.Point(37, 32);
      this.textBox_Args.Name = "textBox_Args";
      this.textBox_Args.Size = new System.Drawing.Size(514, 20);
      this.textBox_Args.TabIndex = 2;
      // 
      // contextMenuStrip1
      // 
      this.contextMenuStrip1.Name = "contextMenuStrip1";
      this.contextMenuStrip1.Size = new System.Drawing.Size(61, 4);
      // 
      // listView_Logs
      // 
      this.listView_Logs.Anchor = ((System.Windows.Forms.AnchorStyles)((((System.Windows.Forms.AnchorStyles.Top | System.Windows.Forms.AnchorStyles.Bottom) 
            | System.Windows.Forms.AnchorStyles.Left) 
            | System.Windows.Forms.AnchorStyles.Right)));
      this.listView_Logs.HideSelection = false;
      this.listView_Logs.Location = new System.Drawing.Point(3, 113);
      this.listView_Logs.Name = "listView_Logs";
      this.listView_Logs.Size = new System.Drawing.Size(548, 222);
      this.listView_Logs.TabIndex = 0;
      this.listView_Logs.UseCompatibleStateImageBehavior = false;
      this.listView_Logs.View = System.Windows.Forms.View.List;
      // 
      // button_Restart
      // 
      this.button_Restart.Location = new System.Drawing.Point(3, 84);
      this.button_Restart.Name = "button_Restart";
      this.button_Restart.Size = new System.Drawing.Size(75, 23);
      this.button_Restart.TabIndex = 4;
      this.button_Restart.Text = "Restart";
      this.button_Restart.UseVisualStyleBackColor = true;
      this.button_Restart.Click += new System.EventHandler(this.Button_Restart_Click);
      // 
      // button_Stop
      // 
      this.button_Stop.Location = new System.Drawing.Point(84, 84);
      this.button_Stop.Name = "button_Stop";
      this.button_Stop.Size = new System.Drawing.Size(75, 23);
      this.button_Stop.TabIndex = 5;
      this.button_Stop.Text = "Stop";
      this.button_Stop.UseVisualStyleBackColor = true;
      this.button_Stop.Click += new System.EventHandler(this.Button_Stop_Click);
      // 
      // button_ClearLogs
      // 
      this.button_ClearLogs.Anchor = ((System.Windows.Forms.AnchorStyles)((System.Windows.Forms.AnchorStyles.Top | System.Windows.Forms.AnchorStyles.Right)));
      this.button_ClearLogs.Location = new System.Drawing.Point(476, 84);
      this.button_ClearLogs.Name = "button_ClearLogs";
      this.button_ClearLogs.Size = new System.Drawing.Size(75, 23);
      this.button_ClearLogs.TabIndex = 6;
      this.button_ClearLogs.Text = "Clear Logs";
      this.button_ClearLogs.UseVisualStyleBackColor = true;
      this.button_ClearLogs.Click += new System.EventHandler(this.Button_ClearLogs_Click);
      // 
      // label_Status
      // 
      this.label_Status.AutoSize = true;
      this.label_Status.Location = new System.Drawing.Point(165, 89);
      this.label_Status.Name = "label_Status";
      this.label_Status.Size = new System.Drawing.Size(47, 13);
      this.label_Status.TabIndex = 7;
      this.label_Status.Text = "Stopped";
      // 
      // OptionsPanel
      // 
      this.AutoScaleDimensions = new System.Drawing.SizeF(6F, 13F);
      this.AutoScaleMode = System.Windows.Forms.AutoScaleMode.Font;
      this.BackColor = System.Drawing.Color.White;
      this.Controls.Add(this.label_Status);
      this.Controls.Add(this.button_ClearLogs);
      this.Controls.Add(this.button_Stop);
      this.Controls.Add(this.button_Restart);
      this.Controls.Add(this.listView_Logs);
      this.Controls.Add(this.textBox_Args);
      this.Controls.Add(this.label3);
      this.Controls.Add(this.textBox_WorkingDirectory);
      this.Controls.Add(this.label2);
      this.Controls.Add(this.textBox_Executable);
      this.Controls.Add(this.label1);
      this.Location = new System.Drawing.Point(12, 66);
      this.Name = "OptionsPanel";
      this.Size = new System.Drawing.Size(554, 338);
      this.ResumeLayout(false);
      this.PerformLayout();

    }

    #endregion

    private System.Windows.Forms.Label label1;
    private System.Windows.Forms.TextBox textBox_Executable;
    private System.Windows.Forms.Label label2;
    private System.Windows.Forms.TextBox textBox_WorkingDirectory;
    private System.Windows.Forms.Label label3;
    private System.Windows.Forms.TextBox textBox_Args;
    private System.Windows.Forms.ContextMenuStrip contextMenuStrip1;
    private System.Windows.Forms.ListView listView_Logs;
    private System.Windows.Forms.Button button_Restart;
    private System.Windows.Forms.Button button_Stop;
    private System.Windows.Forms.Button button_ClearLogs;
    private System.Windows.Forms.Label label_Status;
  }
}
