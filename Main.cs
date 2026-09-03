using System;
using System.Drawing;
using System.IO;
using System.Runtime.InteropServices;
using System.Threading.Tasks;
using System.Windows.Forms;
using ICSharpCode.Decompiler;
using ICSharpCode.Decompiler.CSharp;
using ICSharpCode.Decompiler.Metadata;
using ICSharpCode.Decompiler.TypeSystem;

namespace TazikDecompiler
{
    public class Main : Form
    {

        // forgot what it does but ok
        private TextBox txtDllPath = null!;
        private TextBox txtOutputDir = null!;
        private Button btnBrowseDll = null!;
        private Button btnBrowseOutput = null!;
        private Button btnReverse = null!;
        private CheckBox chkLogging = null!;
        private ProgressBar progressBar = null!;
        private Label lblStatus = null!;
        private TextBox txtLog = null!;
        private bool loggingEnabled;

        public Main()
        {
            InitializeComponent();
            ApplyDarkTheme();
        }

        private void InitializeComponent()
        {
            Text = "TazikDecompiler";
            Size = new Size(720, 600);
            MinimumSize = new Size(620, 500);
            Font = new Font("Segoe UI", 9F);

            Label lblDll = new Label { Text = "DLL file:", AutoSize = true, Location = new Point(12, 15) };
            txtDllPath = new TextBox { Location = new Point(110, 12), Width = 440, Anchor = AnchorStyles.Top | AnchorStyles.Left | AnchorStyles.Right };
            btnBrowseDll = new Button { Text = "Browse", Location = new Point(560, 10), Width = 90 };

            Label lblOut = new Label { Text = "Output folder:", AutoSize = true, Location = new Point(12, 55) };
            txtOutputDir = new TextBox { Location = new Point(110, 52), Width = 440, Anchor = AnchorStyles.Top | AnchorStyles.Left | AnchorStyles.Right };
            btnBrowseOutput = new Button { Text = "Browse", Location = new Point(560, 50), Width = 90 };

            chkLogging = new CheckBox { Text = "Enable logging", AutoSize = true, Location = new Point(12, 90) };
            chkLogging.CheckedChanged += (s, e) => loggingEnabled = chkLogging.Checked;

            btnReverse = new Button
            {
                Text = "Reverse",
                Location = new Point(12, 120),
                Size = new Size(638, 40),
                Font = new Font("Segoe UI", 12F, FontStyle.Bold),
                Anchor = AnchorStyles.Top | AnchorStyles.Left | AnchorStyles.Right
            };

            progressBar = new ProgressBar
            {
                Location = new Point(12, 170),
                Size = new Size(638, 20),
                Style = ProgressBarStyle.Marquee,
                MarqueeAnimationSpeed = 30,
                Anchor = AnchorStyles.Top | AnchorStyles.Left | AnchorStyles.Right
            };
            progressBar.Visible = false;

            lblStatus = new Label
            {
                Text = "Ready",
                Location = new Point(12, 195),
                AutoSize = true,
                Anchor = AnchorStyles.Top | AnchorStyles.Left
            };

            Label lblLog = new Label { Text = "Output:", AutoSize = true, Location = new Point(12, 225) };
            txtLog = new TextBox
            {
                Location = new Point(12, 245),
                Size = new Size(638, 300),
                Multiline = true,
                ScrollBars = ScrollBars.Vertical,
                ReadOnly = true,
                Anchor = AnchorStyles.Top | AnchorStyles.Bottom | AnchorStyles.Left | AnchorStyles.Right
            };

            Controls.AddRange(new Control[] {
                lblDll, txtDllPath, btnBrowseDll,
                lblOut, txtOutputDir, btnBrowseOutput,
                chkLogging,
                btnReverse,
                progressBar, lblStatus,
                lblLog, txtLog
            });

            btnBrowseDll.Click += BtnBrowseDll_Click;
            btnBrowseOutput.Click += BtnBrowseOutput_Click;
            btnReverse.Click += BtnReverse_Click;
        }

        private void ApplyDarkTheme()
        {
            Color back = Color.FromArgb(30, 30, 30);
            Color fore = Color.FromArgb(220, 220, 220);
            Color controlBack = Color.FromArgb(45, 45, 48);
            Color buttonBack = Color.FromArgb(63, 63, 70);
            Color accent = Color.FromArgb(0, 122, 204);

            BackColor = back;
            ForeColor = fore;

            foreach (Control c in Controls)
            {
                if (c is TextBox tb)
                {
                    tb.BackColor = controlBack;
                    tb.ForeColor = fore;
                    tb.BorderStyle = BorderStyle.FixedSingle;
                }
                else if (c is Button btn)
                {
                    btn.BackColor = buttonBack;
                    btn.ForeColor = fore;
                    btn.FlatStyle = FlatStyle.Flat;
                    btn.FlatAppearance.BorderColor = Color.FromArgb(80, 80, 80);
                }
                else if (c is Label || c is CheckBox)
                {
                    c.ForeColor = fore;
                }
            }
            btnReverse.BackColor = accent;
            btnReverse.ForeColor = Color.White;
            progressBar.BackColor = controlBack;
            progressBar.ForeColor = accent;
        }

        private void BtnBrowseDll_Click(object sender, EventArgs e)
        {
            using (OpenFileDialog ofd = new OpenFileDialog())
            {
                ofd.Filter = "DLL files (*.dll)|*.dll|All files (*.*)|*.*";
                if (ofd.ShowDialog() == DialogResult.OK)
                {
                    txtDllPath.Text = ofd.FileName;
                    if (string.IsNullOrEmpty(txtOutputDir.Text))
                    {
                        txtOutputDir.Text = Path.Combine(Path.GetDirectoryName(ofd.FileName), "decompiled");
                    }
                }
            }
        }

        private void BtnBrowseOutput_Click(object sender, EventArgs e)
        {
            using (FolderBrowserDialog fbd = new FolderBrowserDialog())
            {
                if (fbd.ShowDialog() == DialogResult.OK)
                {
                    txtOutputDir.Text = fbd.SelectedPath;
                }
            }
        }

        private async void BtnReverse_Click(object sender, EventArgs e)
        {
            string dll = txtDllPath.Text.Trim();
            if (string.IsNullOrEmpty(dll))
            {
                MessageBox.Show("Select a DLL file", "Error", MessageBoxButtons.OK, MessageBoxIcon.Error);
                return;
            }
            if (!File.Exists(dll))
            {
                MessageBox.Show("DLL file not found", "Error", MessageBoxButtons.OK, MessageBoxIcon.Error);
                return;
            }

            string output = txtOutputDir.Text.Trim();
            if (string.IsNullOrEmpty(output))
            {
                output = Path.Combine(Path.GetDirectoryName(dll), "decompiled");
                txtOutputDir.Text = output;
            }
            Directory.CreateDirectory(output);

            btnReverse.Enabled = false;
            progressBar.Visible = true;
            lblStatus.Text = "Running...";
            txtLog.Clear();
            LogMessage($"Starting: {dll} -> {output}");

            try
            {
                await Task.Run(() => Decompile(dll, output));
                progressBar.Visible = false;
                btnReverse.Enabled = true;
                lblStatus.Text = "Completed";
                LogMessage("Process completed.");
            }
            catch (Exception ex)
            {
                progressBar.Visible = false;
                btnReverse.Enabled = true;
                lblStatus.Text = $"Error: {ex.Message}";
                LogMessage($"Error: {ex.Message}");
            }
        }

        private void Decompile(string inputDll, string outputDir)
        {
            var resolver = new UniversalAssemblyResolver(inputDll, false, null);
            resolver.AddSearchDirectory(Path.GetDirectoryName(inputDll));
            resolver.AddSearchDirectory(RuntimeEnvironment.GetRuntimeDirectory());

            var settings = new DecompilerSettings(LanguageVersion.CSharp7_3);
            settings.LoadInMemory = true;
            settings.ThrowOnAssemblyResolveErrors = false;

            var decompiler = new CSharpDecompiler(inputDll, resolver, settings);

            int successCount = 0;
            int failCount = 0;

            void ProcessType(ITypeDefinition type)
            {
                if (type.FullName.Contains("<") || type.FullName.Contains(">"))
                    return;

                try
                {
                    var fullTypeName = new FullTypeName(type.ReflectionName);
                    string code = decompiler.DecompileTypeAsString(fullTypeName);

                    // deleting ILL comments if there will be some in decompile
                    code = RemoveIlComments(code);

                    string ns = type.Namespace ?? string.Empty;
                    string nsFolder = ns.Replace('.', Path.DirectorySeparatorChar);
                    string typeFolder = Path.Combine(outputDir, nsFolder);
                    Directory.CreateDirectory(typeFolder);

                    string fileName = type.ReflectionName.Replace('+', '_') + ".cs";
                    string filePath = Path.Combine(typeFolder, fileName);
                    File.WriteAllText(filePath, code);
                    successCount++;
                    LogMessage($"Decompiled {type.FullName} -> {filePath}");
                }
                catch (Exception ex)
                {
                    failCount++;
                    LogMessage($"Failed to decompile {type.FullName}: {ex.Message}");
                }

                foreach (var nested in type.NestedTypes)
                {
                    ProcessType(nested);
                }
            }

            foreach (var type in decompiler.TypeSystem.MainModule.TypeDefinitions)
            {
                ProcessType(type);
            }

            LogMessage($"Finished. Types: {successCount} success, {failCount} failed.");
        }

        private string RemoveIlComments(string code)
        {
            var lines = code.Split(new[] { '\n' }, StringSplitOptions.None);
            var result = new System.Collections.Generic.List<string>();
            foreach (var line in lines)
            {
                if (!line.TrimStart().StartsWith("//IL_"))
                {
                    result.Add(line);
                }
            }
            return string.Join("\n", result);
        }

        private void LogMessage(string message)
        {
            string timestamp = DateTime.Now.ToString("HH:mm:ss");
            string line = $"[{timestamp}] {message}";
            if (txtLog.InvokeRequired)
            {
                txtLog.Invoke(new Action(() => AppendLog(line)));
            }
            else
            {
                AppendLog(line);
            }

            if (loggingEnabled && !string.IsNullOrEmpty(txtDllPath.Text))
            {
                string logFile = Path.Combine(Path.GetDirectoryName(txtDllPath.Text), "tazik_decompiler_log.txt");
                File.AppendAllText(logFile, line + Environment.NewLine);
            }
        }

        private void AppendLog(string line)
        {
            txtLog.AppendText(line + Environment.NewLine);
        }
    }
}
