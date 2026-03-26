namespace COMP_3951_BlockForge_TechPro
{
    public sealed class VariableDefinitionDialog : Form
    {
        private readonly TextBox _nameTextBox;
        private readonly ComboBox _typeComboBox;

        public VariableBlock? Result { get; private set; }

        public VariableDefinitionDialog()
        {
            Text = "Add Variable Block";
            FormBorderStyle = FormBorderStyle.FixedDialog;
            StartPosition = FormStartPosition.CenterParent;
            MaximizeBox = false;
            MinimizeBox = false;
            ClientSize = new Size(320, 150);

            Label nameLabel = new()
            {
                Text = "Variable Name",
                AutoSize = true,
                Location = new Point(12, 15)
            };

            _nameTextBox = new TextBox
            {
                Location = new Point(12, 35),
                Width = 290
            };

            Label typeLabel = new()
            {
                Text = "Variable Type",
                AutoSize = true,
                Location = new Point(12, 70)
            };

            _typeComboBox = new ComboBox
            {
                Location = new Point(12, 90),
                Width = 170,
                DropDownStyle = ComboBoxStyle.DropDownList,
                DataSource = Enum.GetValues<VariableBlockType>()
            };

            Button okButton = new()
            {
                Text = "OK",
                DialogResult = DialogResult.None,
                Location = new Point(146, 115),
                Width = 75
            };
            okButton.Click += OkButton_Click;

            Button cancelButton = new()
            {
                Text = "Cancel",
                DialogResult = DialogResult.Cancel,
                Location = new Point(227, 115),
                Width = 75
            };

            Controls.Add(nameLabel);
            Controls.Add(_nameTextBox);
            Controls.Add(typeLabel);
            Controls.Add(_typeComboBox);
            Controls.Add(okButton);
            Controls.Add(cancelButton);

            AcceptButton = okButton;
            CancelButton = cancelButton;
        }

        private void OkButton_Click(object? sender, EventArgs e)
        {
            string variableName = _nameTextBox.Text.Trim();
            if (string.IsNullOrWhiteSpace(variableName))
            {
                MessageBox.Show(this, "Variable name is required.", "Validation", MessageBoxButtons.OK, MessageBoxIcon.Warning);
                return;
            }

            if (_typeComboBox.SelectedItem is not VariableBlockType selectedType)
            {
                MessageBox.Show(this, "Please select a variable type.", "Validation", MessageBoxButtons.OK, MessageBoxIcon.Warning);
                return;
            }

            Result = selectedType switch
            {
                VariableBlockType.String => VariableBlock.CreateString(variableName),
                VariableBlockType.Int => VariableBlock.CreateInt(variableName),
                VariableBlockType.Bool => VariableBlock.CreateBool(variableName),
                _ => null
            };

            if (Result is null)
            {
                MessageBox.Show(this, "Unable to create variable block.", "Validation", MessageBoxButtons.OK, MessageBoxIcon.Warning);
                return;
            }

            DialogResult = DialogResult.OK;
            Close();
        }
    }
}
