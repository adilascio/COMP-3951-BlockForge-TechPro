namespace COMP_3951_BlockForge_TechPro
{
    public partial class Form1 : Form
    {
        /// <summary>
        /// Defines the width of one workspace grid cell in pixels.
        /// </summary>
        private const int StandardBlockWidth = 140;
        private const int StandardBlockHeight = 45;
        private const int ConnectorVisualHeight = 12;
        private const int ConnectorVerticalGap = 15;
        private const int GridCellWidth = StandardBlockWidth;

        /// <summary>
        /// Defines the height of one workspace grid cell in pixels.
        /// </summary>
        private const int GridCellHeight = StandardBlockHeight + ConnectorVisualHeight + ConnectorVerticalGap;
        private const int TopPanelMinSize = 80;
        private const int BottomPanelMinSize = 120;
        private const int LeftPanelMinSize = 120;
        private const int RightPanelMinSize = 240;
        private const int DeleteZoneSize = 28;
        private const int DeleteZoneMargin = 8;

        /// <summary>
        /// Calculates snapped block positions for the workspace grid.
        /// </summary>
        private readonly GridSnapService _gridSnapService;

        /// <summary>
        /// Stores the backing <see cref="CodeBlock"/> for each workspace panel.
        /// </summary>
        private readonly Dictionary<Panel, CodeBlock> _workspaceBlocks = new();
        private readonly Dictionary<Panel, BlockConnectorControl> _connectorControlsByChild = new();

        /// <summary>
        /// Stores the console messages shown in the workspace console window.
        /// </summary>
        private readonly WorkspaceConsole _workspaceConsole = new();

        /// <summary>
        /// Displays console messages in the form UI.
        /// </summary>
        private ListBox? _consoleListBox;
        private readonly BlockConnectorService _blockConnectorService;
        private readonly ProjectFileManager _projectFileManager;
        private readonly WorkspaceSaveNotifier _workspaceSaveNotifier;
        private FlowLayoutPanel? _blockBinRow;
        private ToolStrip? _variableToolStrip;
        private SplitContainer? _horizontalSplit;
        private SplitContainer? _topVerticalSplit;
        private SplitContainer? _bottomVerticalSplit;
        private PictureBox? _deleteDropZone;
        private bool _syncingVerticalSplit;

        public Form1()
        {
            InitializeComponent();
            SetupResizableLayout();
            _gridSnapService = new GridSnapService(GridCellWidth, GridCellHeight);
            _blockConnectorService = new BlockConnectorService(GridCellWidth, GridCellHeight);
            _projectFileManager = new ProjectFileManager(new PayloadTransformer(5));
            _workspaceSaveNotifier = new WorkspaceSaveNotifier(_workspaceConsole);
            SetupDragDrop();
            SetupDeleteDropZone();
            SetupConsoleWindow();
            SetupVariableToolbar();
            CreateBlockTemplates();
        }

        /// <summary>
        /// Gets the current workspace client size so snap calculations always use the visible workspace bounds.
        /// </summary>
        private Size WorkspaceBounds => groupBoxWorkSpace.ClientSize;

        private void SetupResizableLayout()
        {
            _horizontalSplit = new SplitContainer
            {
                Dock = DockStyle.None,
                Orientation = Orientation.Horizontal,
                SplitterWidth = 6,
                Anchor = AnchorStyles.Top | AnchorStyles.Bottom | AnchorStyles.Left | AnchorStyles.Right
            };

            _topVerticalSplit = new SplitContainer
            {
                Dock = DockStyle.Fill,
                Orientation = Orientation.Vertical,
                SplitterWidth = 6
            };

            _bottomVerticalSplit = new SplitContainer
            {
                Dock = DockStyle.Fill,
                Orientation = Orientation.Vertical,
                SplitterWidth = 6
            };

            _topVerticalSplit.SplitterMoved += TopVerticalSplit_SplitterMoved;
            _bottomVerticalSplit.SplitterMoved += BottomVerticalSplit_SplitterMoved;

            groupBox1.Dock = DockStyle.Fill;
            groupBoxWorkSpace.Dock = DockStyle.Fill;
            groupBox2.Dock = DockStyle.Fill;
            groupBoxBlockBin.Dock = DockStyle.Fill;

            _topVerticalSplit.Panel1.Controls.Add(groupBox1);
            _topVerticalSplit.Panel2.Controls.Add(groupBoxWorkSpace);
            _bottomVerticalSplit.Panel1.Controls.Add(groupBox2);
            _bottomVerticalSplit.Panel2.Controls.Add(groupBoxBlockBin);

            _horizontalSplit.Panel1.Controls.Add(_topVerticalSplit);
            _horizontalSplit.Panel2.Controls.Add(_bottomVerticalSplit);

            Controls.Add(_horizontalSplit);
            LayoutSplitContainerBelowMenu();
            _horizontalSplit.BringToFront();
            menuStrip1.BringToFront();
        }

        private void LayoutSplitContainerBelowMenu()
        {
            if (_horizontalSplit == null)
            {
                return;
            }

            int top = menuStrip1.Bottom;
            _horizontalSplit.Location = new Point(0, top);
            _horizontalSplit.Size = new Size(ClientSize.Width, Math.Max(1, ClientSize.Height - top));
        }

        private void ApplyInitialSplitLayout()
        {
            if (_horizontalSplit != null)
            {
                ApplySplitLayout(_horizontalSplit, TopPanelMinSize, BottomPanelMinSize, (int)(_horizontalSplit.ClientSize.Height * 0.68));
            }

            if (_topVerticalSplit != null)
            {
                ApplySplitLayout(_topVerticalSplit, LeftPanelMinSize, RightPanelMinSize, 180);
            }

            if (_bottomVerticalSplit != null)
            {
                ApplySplitLayout(_bottomVerticalSplit, LeftPanelMinSize, RightPanelMinSize, 180);
            }
        }

        private static void ApplySplitLayout(SplitContainer splitContainer, int panel1Min, int panel2Min, int desiredDistance)
        {
            int span = splitContainer.Orientation == Orientation.Vertical
                ? splitContainer.Width
                : splitContainer.Height;

            int available = Math.Max(0, span);
            int safePanel1Min = Math.Clamp(panel1Min, 0, available);
            int safePanel2Min = Math.Clamp(panel2Min, 0, Math.Max(0, available - safePanel1Min));

            splitContainer.Panel1MinSize = 0;
            splitContainer.Panel2MinSize = 0;

            int minDistance = safePanel1Min;
            int maxDistance = available - safePanel2Min;
            if (maxDistance < minDistance)
            {
                return;
            }

            splitContainer.Panel1MinSize = safePanel1Min;
            splitContainer.Panel2MinSize = safePanel2Min;
            TrySetSplitterDistance(splitContainer, desiredDistance);
        }

        private void TopVerticalSplit_SplitterMoved(object? sender, SplitterEventArgs e)
        {
            SyncVerticalSplit(_topVerticalSplit, _bottomVerticalSplit);
        }

        private void BottomVerticalSplit_SplitterMoved(object? sender, SplitterEventArgs e)
        {
            SyncVerticalSplit(_bottomVerticalSplit, _topVerticalSplit);
        }

        private void SyncVerticalSplit(SplitContainer? source, SplitContainer? target)
        {
            if (_syncingVerticalSplit || source == null || target == null)
            {
                return;
            }

            _syncingVerticalSplit = true;
            try
            {
                TrySetSplitterDistance(target, source.SplitterDistance);
            }
            finally
            {
                _syncingVerticalSplit = false;
            }
        }

        private static void TrySetSplitterDistance(SplitContainer splitContainer, int desiredDistance)
        {
            int span = splitContainer.Orientation == Orientation.Vertical
                ? splitContainer.Width
                : splitContainer.Height;

            int minDistance = splitContainer.Panel1MinSize;
            int maxDistance = span - splitContainer.Panel2MinSize;
            if (maxDistance < minDistance)
            {
                return;
            }

            int clamped = Math.Clamp(desiredDistance, minDistance, maxDistance);
            try
            {
                splitContainer.SplitterDistance = clamped;
            }
            catch (InvalidOperationException)
            {
                int fallback = Math.Clamp(clamped - 1, minDistance, maxDistance);
                splitContainer.SplitterDistance = fallback;
            }
        }

        // --- Drag/Drop wiring for the workspace ---
        private void SetupDragDrop()
        {
            // Allow the workspace groupbox to accept drops
            groupBoxWorkSpace.AllowDrop = true;
            groupBoxWorkSpace.DragEnter += WorkSpace_DragEnter;
            groupBoxWorkSpace.DragDrop += WorkSpace_DragDrop;
        }

        private void SetupDeleteDropZone()
        {
            _deleteDropZone = new PictureBox
            {
                Size = new Size(DeleteZoneSize, DeleteZoneSize),
                Anchor = AnchorStyles.Bottom | AnchorStyles.Right,
                SizeMode = PictureBoxSizeMode.StretchImage,
                BackColor = Color.Transparent,
                Image = CreateDeleteZoneImage()
            };
            _deleteDropZone.Region = CreateDeleteZoneRegion();

            PositionDeleteDropZone();
            groupBoxWorkSpace.Controls.Add(_deleteDropZone);
            _deleteDropZone.BringToFront();
            groupBoxWorkSpace.Resize += (_, _) => PositionDeleteDropZone();
        }

        private void PositionDeleteDropZone()
        {
            if (_deleteDropZone == null)
            {
                return;
            }

            _deleteDropZone.Location = new Point(
                Math.Max(DeleteZoneMargin, groupBoxWorkSpace.ClientSize.Width - _deleteDropZone.Width - DeleteZoneMargin),
                Math.Max(DeleteZoneMargin, groupBoxWorkSpace.ClientSize.Height - _deleteDropZone.Height - DeleteZoneMargin));
        }

        private static Bitmap CreateDeleteZoneImage()
        {
            Bitmap bitmap = new(DeleteZoneSize, DeleteZoneSize);
            using Graphics graphics = Graphics.FromImage(bitmap);
            graphics.SmoothingMode = System.Drawing.Drawing2D.SmoothingMode.AntiAlias;
            graphics.Clear(Color.Transparent);

            using Pen iconPen = new(Color.Black, 2.3f);
            iconPen.StartCap = System.Drawing.Drawing2D.LineCap.Round;
            iconPen.EndCap = System.Drawing.Drawing2D.LineCap.Round;

            int cx = DeleteZoneSize / 2;
            graphics.DrawLine(iconPen, cx - 6, 9, cx + 6, 9);
            graphics.DrawRectangle(iconPen, cx - 5, 10, 10, 12);
            graphics.DrawLine(iconPen, cx - 2, 13, cx - 2, 19);
            graphics.DrawLine(iconPen, cx, 13, cx, 19);
            graphics.DrawLine(iconPen, cx + 2, 13, cx + 2, 19);
            return bitmap;
        }

        private static Region CreateDeleteZoneRegion()
        {
            System.Drawing.Drawing2D.GraphicsPath path = new();
            int cx = DeleteZoneSize / 2;

            path.AddRectangle(new Rectangle(cx - 7, 8, 14, 3));
            path.AddRectangle(new Rectangle(cx - 6, 10, 12, 14));
            path.AddRectangle(new Rectangle(cx - 3, 13, 2, 8));
            path.AddRectangle(new Rectangle(cx - 1, 13, 2, 8));
            path.AddRectangle(new Rectangle(cx + 2, 13, 2, 8));

            return new Region(path);
        }

        /// <summary>
        /// Creates the console window UI and binds it to the in-memory workspace console.
        /// </summary>
        private void SetupConsoleWindow()
        {
            groupBox3.Visible = false;

            _consoleListBox = new ListBox
            {
                Dock = DockStyle.Fill,
                BorderStyle = BorderStyle.None,
                BackColor = Color.WhiteSmoke,
                Font = new Font("Consolas", 9F, FontStyle.Regular),
                HorizontalScrollbar = true
            };

            groupBox2.Controls.Add(_consoleListBox);
            _consoleListBox.BringToFront();

            RefreshConsoleDisplay();
        }

        /// <summary>
        /// Refreshes the visible console list so it matches the stored console messages.
        /// </summary>
        private void RefreshConsoleDisplay()
        {
            if (_consoleListBox == null)
            {
                return;
            }

            _consoleListBox.Items.Clear();

            foreach (ConsoleMessage message in _workspaceConsole.Messages)
            {
                _consoleListBox.Items.Add($"[{message.Severity}] {message.Text}");
            }
        }

        /// <summary>
        /// Appends a message to the workspace console and refreshes the visible console window.
        /// </summary>
        /// <param name="severity">The severity level of the message.</param>
        /// <param name="text">The text displayed in the console window.</param>
        private void AppendConsoleMessage(ConsoleMessageSeverity severity, string text)
        {
            _workspaceConsole.Append(new ConsoleMessage(severity, text));
            RefreshConsoleDisplay();
        }

        // Testing Templates
        private void CreateBlockTemplates()
        {
            // A container that sits at the TOP of BlockBin and auto-lays out blocks left-to-right
            var topRow = new FlowLayoutPanel
            {
                Dock = DockStyle.Top,
                AutoSize = true,
                WrapContents = false,
                FlowDirection = FlowDirection.LeftToRight,
                Padding = new Padding(10, 10, 10, 10), // space from the groupbox border/title
                BackColor = Color.Transparent
            };

            //templates
            var block1 = MakeTemplateBlock("If", Color.Aqua);
            var block2 = MakeTemplateBlock("While", Color.PeachPuff);
            var block3 = MakeTemplateBlock("Run", Color.Green);
            var block4 = MakeTemplateBlock("Print", Color.Khaki);
            var block5 = MakeTemplateBlock("==", Color.Plum);
            var block6 = MakeTemplateBlock("=", Color.MistyRose);
            var block7 = MakeTemplateBlock("Operator", Color.LightSteelBlue, new OperatorBlock());

            // Size 
            block1.Size = block2.Size = block3.Size = block4.Size = block5.Size = block6.Size = block7.Size = new Size(StandardBlockWidth, StandardBlockHeight);

            // Small gap between blocks (FlowLayoutPanel uses each control's Margin)
            block1.Margin = new Padding(0, 0, 8, 0);
            block2.Margin = new Padding(0, 0, 8, 0);
            block3.Margin = new Padding(0, 0, 8, 0);
            block4.Margin = new Padding(0, 0, 8, 0);
            block5.Margin = new Padding(0, 0, 8, 0);
            block6.Margin = new Padding(0, 0, 8, 0);
            block7.Margin = new Padding(0, 0, 8, 0);

            // Add to the row
            topRow.Controls.Add(block1);
            topRow.Controls.Add(block2);
            topRow.Controls.Add(block3);
            topRow.Controls.Add(block4);
            topRow.Controls.Add(block5);
            topRow.Controls.Add(block6);
            topRow.Controls.Add(block7);

            // Add the row to BlockBin
            groupBoxBlockBin.Controls.Add(topRow);

            // Optional: ensure it stays above anything else added later
            topRow.BringToFront();
            _blockBinRow = topRow;
        }

        // Creates a template block panel with MouseDown -> DoDragDrop
        private Panel MakeTemplateBlock(string text, Color color, object? tagValue = null)
        {
            var p = new Panel
            {
                Size = new Size(StandardBlockWidth, StandardBlockHeight),
                BackColor = color,
                BorderStyle = BorderStyle.FixedSingle,
                Cursor = Cursors.Hand,
                Tag = tagValue ?? text
            };

            var lbl = new Label
            {
                Text = text,
                Dock = DockStyle.Fill,
                TextAlign = ContentAlignment.MiddleCenter,
                Font = new Font("Segoe UI", 10, FontStyle.Bold)
            };

            p.Controls.Add(lbl);

            // clicking the label should also drag
            p.MouseDown += TemplateBlock_MouseDown;
            lbl.MouseDown += (s, e) => TemplateBlock_MouseDown(p, e);
            p.Click += Block_Click;
            lbl.Click += (s, e) => Block_Click(p, e);

            return p;
        }

        // When you click a template in BlockBin, start a drag with COPY effect
        private void TemplateBlock_MouseDown(object sender, MouseEventArgs e)
        {
            if (e.Button != MouseButtons.Left) return;

            var template = sender as Panel;
            if (template == null) return;

            // Pass the template panel as the drag data
            template.DoDragDrop(template, DragDropEffects.Copy);
        }

        // Workspace DragEnter 
        private void WorkSpace_DragEnter(object sender, DragEventArgs e)
        {
            // We only accept Panel data
            if (e.Data != null && e.Data.GetDataPresent(typeof(Panel)))
                e.Effect = DragDropEffects.Copy;
            else
                e.Effect = DragDropEffects.None;
        }

        // Workspace DragDrop -> clone template into workspace 
        private void WorkSpace_DragDrop(object sender, DragEventArgs e)
        {
            if (e.Data == null) return;

            var template = e.Data.GetData(typeof(Panel)) as Panel;
            if (template == null) return;

            // Clone a new block based on the template
            var newBlock = CloneAsWorkspaceBlock(template);

            // Convert screen mouse coords to workspace client coords
            Point dropPoint = groupBoxWorkSpace.PointToClient(new Point(e.X, e.Y));

            // groupBox has a border/title; keep it inside a bit
            dropPoint.Offset(-newBlock.Width / 2, -newBlock.Height / 2);

            SnappedPlacement snappedPlacement = _gridSnapService.Snap(dropPoint, newBlock.Size, WorkspaceBounds);

            if (IsGridCellOccupied(snappedPlacement.GridPosition))
            {
                AppendConsoleMessage(ConsoleMessageSeverity.Warning, "That grid cell is already occupied.");
                return;
            }

            newBlock.Location = snappedPlacement.Location;
            groupBoxWorkSpace.Controls.Add(newBlock);
            newBlock.BringToFront();

            RegisterWorkspaceBlock(newBlock, snappedPlacement);
            TryAttachBlockToConnector(newBlock);
            TryDeleteBlockOnDrop(newBlock);
            _deleteDropZone?.BringToFront();
        }

        // Clone template into a draggable workspace block 
        private Panel CloneAsWorkspaceBlock(Panel template)
        {
            // Pull text/type from Tag, and color from BackColor
            object tagValue = template.Tag ?? "Block";
            string text = GetBlockDisplayText(tagValue);
            Color color = template.BackColor;
            object workspaceTag = tagValue switch
            {
                VariableBlock variableBlock => CloneVariableBlock(variableBlock),
                OperatorBlock operatorBlock => CloneOperatorBlock(operatorBlock),
                _ => text
            };

            var p = new Panel
            {
                Size = template.Size,
                BackColor = color,
                BorderStyle = BorderStyle.FixedSingle,
                Cursor = Cursors.SizeAll,
                Tag = workspaceTag
            };

            // Make it draggable INSIDE the workspace
            p.MouseDown += WorkspaceBlock_MouseDown;
            p.MouseMove += WorkspaceBlock_MouseMove;
            p.MouseUp += WorkspaceBlock_MouseUp;
            p.Click += Block_Click;
            BuildWorkspaceBlockContent(p, workspaceTag, text);

            return p;
        }

        // Drag blocks around inside WorkSpace 
        private bool _dragging = false;
        private Point _dragOffset; // mouse offset within the panel being dragged
        private Panel? _activeBlock;
        private Color _activeBlockOriginalColor;

        private void WorkspaceBlock_MouseDown(object sender, MouseEventArgs e)
        {
            if (e.Button != MouseButtons.Left) return;

            _activeBlock = sender as Panel;
            if (_activeBlock == null) return;

            _activeBlockOriginalColor = _activeBlock.BackColor;
            _activeBlock.BackColor = GetDragVisualColor(_activeBlockOriginalColor);
            _dragging = true;
            _dragOffset = e.Location; // where in the panel the mouse grabbed it
            _activeBlock.BringToFront();
            _deleteDropZone?.BringToFront();
        }

        private void WorkspaceBlock_MouseMove(object sender, MouseEventArgs e)
        {
            if (!_dragging || _activeBlock == null) return;

            // Mouse position relative to workspace
            Point mouseInWorkspace = groupBoxWorkSpace.PointToClient(MousePosition);

            // New top-left = mouse minus grab offset
            Point newLoc = new Point(mouseInWorkspace.X - _dragOffset.X,
                                     mouseInWorkspace.Y - _dragOffset.Y);

            newLoc = ClampToBounds(newLoc, _activeBlock.Size, groupBoxWorkSpace.ClientSize);
            MovePanelChainDuringDrag(_activeBlock, newLoc);
            _deleteDropZone?.BringToFront();
        }

        private void WorkspaceBlock_MouseUp(object sender, MouseEventArgs e)
        {
            if (_activeBlock != null)
            {
                if (TryDeleteBlockOnDrop(_activeBlock))
                {
                    _dragging = false;
                    _activeBlock = null;
                    return;
                }

                SnappedPlacement snappedPlacement = _gridSnapService.Snap(_activeBlock.Location, _activeBlock.Size, WorkspaceBounds);

                if (IsGridCellOccupied(snappedPlacement.GridPosition, _activeBlock))
                {
                    AppendConsoleMessage(ConsoleMessageSeverity.Warning, "That grid cell is already occupied.");
                    RestoreStoredBlockPosition(_activeBlock);
                }
                else
                {
                    _activeBlock.Location = snappedPlacement.Location;
                    UpdateStoredBlockPosition(_activeBlock, snappedPlacement);
                    AlignConnectedChildren(_activeBlock);
                    TryAttachBlockToConnector(_activeBlock);
                }

                _activeBlock.BackColor = _activeBlockOriginalColor;
            }

            _dragging = false;
            _activeBlock = null;
        }

        private static Color GetDragVisualColor(Color baseColor)
        {
            int r = (baseColor.R + 255) / 2;
            int g = (baseColor.G + 255) / 2;
            int b = (baseColor.B + 255) / 2;
            return Color.FromArgb(r, g, b);
        }

        /// <summary>
        /// Creates and stores the backing <see cref="CodeBlock"/> for a newly dropped workspace block.
        /// </summary>
        /// <param name="blockPanel">The workspace panel representing the block.</param>
        /// <param name="snappedPlacement">The snapped placement assigned to the block.</param>
        private void RegisterWorkspaceBlock(Panel blockPanel, SnappedPlacement snappedPlacement)
        {
            (CodeBlockType blockType, string blockName, VariableBlockType? variableType) = ResolveBlockMetadata(blockPanel.Tag);
            string uid = $"{blockName}-{Guid.NewGuid():N}";
            var codeBlock = new CodeBlock(
                snappedPlacement.Location.X,
                snappedPlacement.Location.Y,
                uid,
                snappedPlacement.GridPosition.Column,
                snappedPlacement.GridPosition.Row,
                blockType,
                blockName,
                variableType);
            ApplyBlockValues(codeBlock, blockPanel.Tag);

            _workspaceBlocks[blockPanel] = codeBlock;
            AppendConsoleMessage(ConsoleMessageSeverity.Message, $"{blockName} block created.");
        }

        private bool TryDeleteBlockOnDrop(Panel blockPanel)
        {
            if (_deleteDropZone == null || !blockPanel.Bounds.IntersectsWith(_deleteDropZone.Bounds))
            {
                return false;
            }

            string blockName = GetBlockDisplayText(blockPanel.Tag);
            RemoveConnectorVisualForChild(blockPanel);

            if (_workspaceBlocks.TryGetValue(blockPanel, out CodeBlock? codeBlock))
            {
                DisconnectBlockFromConnector(codeBlock);
            }

            _workspaceBlocks.Remove(blockPanel);
            groupBoxWorkSpace.Controls.Remove(blockPanel);
            blockPanel.Dispose();
            AppendConsoleMessage(ConsoleMessageSeverity.Message, $"Deleted block: {blockName}");
            _deleteDropZone.BringToFront();
            return true;
        }

        private void SetupVariableToolbar()
        {
            _variableToolStrip = new ToolStrip
            {
                GripStyle = ToolStripGripStyle.Hidden,
                Dock = DockStyle.Top
            };

            var addVariableButton = new ToolStripButton("Add Variable");
            addVariableButton.Click += AddVariableButton_Click;
            _variableToolStrip.Items.Add(addVariableButton);

            groupBoxBlockBin.Controls.Add(_variableToolStrip);
            _variableToolStrip.BringToFront();
        }

        private void AddVariableButton_Click(object? sender, EventArgs e)
        {
            using var dialog = new VariableDefinitionDialog();
            if (dialog.ShowDialog(this) != DialogResult.OK || dialog.Result is null)
            {
                return;
            }

            AddVariableTemplate(dialog.Result);
            AppendConsoleMessage(ConsoleMessageSeverity.Message, $"Added variable block: {dialog.Result.VariableName} ({dialog.Result.VariableType})");
        }

        private void AddVariableTemplate(VariableBlock variable)
        {
            if (_blockBinRow == null)
            {
                return;
            }

            Color variableColor = variable.VariableType switch
            {
                VariableBlockType.String => Color.LightSkyBlue,
                VariableBlockType.Int => Color.LightGoldenrodYellow,
                VariableBlockType.Bool => Color.LightCoral,
                _ => Color.LightGray
            };

            var variableTemplate = MakeTemplateBlock(GetBlockDisplayText(variable), variableColor, variable);
            variableTemplate.Size = new Size(StandardBlockWidth, StandardBlockHeight);
            variableTemplate.Margin = new Padding(0, 0, 8, 0);
            _blockBinRow.Controls.Add(variableTemplate);
        }

        private static VariableBlock CloneVariableBlock(VariableBlock variableBlock)
        {
            return variableBlock.VariableType switch
            {
                VariableBlockType.String => VariableBlock.CreateString(variableBlock.VariableName, variableBlock.StringValue ?? string.Empty),
                VariableBlockType.Int => VariableBlock.CreateInt(variableBlock.VariableName, variableBlock.IntValue ?? 0),
                VariableBlockType.Bool => VariableBlock.CreateBool(variableBlock.VariableName, variableBlock.BoolValue ?? false),
                _ => throw new InvalidOperationException("Unsupported variable type.")
            };
        }

        private static OperatorBlock CloneOperatorBlock(OperatorBlock operatorBlock)
        {
            return new OperatorBlock(operatorBlock.SelectedOperator);
        }

        private void BuildWorkspaceBlockContent(Panel blockPanel, object workspaceTag, string text)
        {
            if (workspaceTag is OperatorBlock operatorBlock)
            {
                ComboBox operatorComboBox = new()
                {
                    Dock = DockStyle.Fill,
                    DropDownStyle = ComboBoxStyle.DropDownList,
                    Font = new Font("Segoe UI", 10, FontStyle.Bold),
                    FlatStyle = FlatStyle.Flat
                };

                operatorComboBox.Items.AddRange(OperatorBlock.SupportedOperators);
                operatorComboBox.SelectedItem = operatorBlock.SelectedOperator;
                if (operatorComboBox.SelectedIndex < 0)
                {
                    operatorComboBox.SelectedIndex = 0;
                    operatorBlock.SelectedOperator = operatorComboBox.SelectedItem?.ToString() ?? "+";
                }

                operatorComboBox.SelectedIndexChanged += (_, _) =>
                {
                    operatorBlock.SelectedOperator = operatorComboBox.SelectedItem?.ToString() ?? "+";
                    if (_workspaceBlocks.TryGetValue(blockPanel, out CodeBlock? codeBlock))
                    {
                        codeBlock.UpdateVariableValues(stringValue: operatorBlock.SelectedOperator);
                    }
                };

                blockPanel.Controls.Add(operatorComboBox);
                return;
            }

            Label label = new()
            {
                Text = text,
                Dock = DockStyle.Fill,
                TextAlign = ContentAlignment.MiddleCenter,
                Font = new Font("Segoe UI", 10, FontStyle.Bold)
            };

            blockPanel.Controls.Add(label);
            label.MouseDown += (s, e) => WorkspaceBlock_MouseDown(blockPanel, e);
            label.MouseMove += (s, e) => WorkspaceBlock_MouseMove(blockPanel, e);
            label.MouseUp += (s, e) => WorkspaceBlock_MouseUp(blockPanel, e);
            label.Click += (s, e) => Block_Click(blockPanel, e);
        }

        private static string GetBlockDisplayText(object? tagValue)
        {
            if (tagValue is VariableBlock variableBlock)
            {
                return $"{variableBlock.VariableName} : {variableBlock.VariableType}";
            }

            if (tagValue is OperatorBlock)
            {
                return "Operator";
            }

            return tagValue?.ToString() ?? "Block";
        }

        private static Color GetBlockColor(CodeBlockType blockType, VariableBlockType? variableType = null)
        {
            if (blockType == CodeBlockType.Variable)
            {
                return variableType switch
                {
                    VariableBlockType.String => Color.LightSkyBlue,
                    VariableBlockType.Int => Color.LightGoldenrodYellow,
                    VariableBlockType.Bool => Color.LightCoral,
                    _ => Color.LightGray
                };
            }

            return blockType switch
            {
                CodeBlockType.If => Color.Aqua,
                CodeBlockType.While => Color.PeachPuff,
                CodeBlockType.Run => Color.Green,
                CodeBlockType.Print => Color.Khaki,
                CodeBlockType.Assignment => Color.MistyRose,
                CodeBlockType.Operator => Color.LightSteelBlue,
                CodeBlockType.Equals => Color.Plum,
                _ => Color.LightGray
            };
        }

        private static object CreateWorkspaceTagFromCodeBlock(CodeBlock codeBlock)
        {
            if (codeBlock.BlockType != CodeBlockType.Variable || !codeBlock.VariableType.HasValue)
            {
                return codeBlock.BlockType == CodeBlockType.Operator
                    ? new OperatorBlock(codeBlock.StringValue ?? "+")
                    : codeBlock.BlockName ?? codeBlock.BlockType.ToString();
            }

            return codeBlock.VariableType.Value switch
            {
                VariableBlockType.String => VariableBlock.CreateString(codeBlock.BlockName ?? "variable", codeBlock.StringValue ?? string.Empty),
                VariableBlockType.Int => VariableBlock.CreateInt(codeBlock.BlockName ?? "variable", codeBlock.IntValue ?? 0),
                VariableBlockType.Bool => VariableBlock.CreateBool(codeBlock.BlockName ?? "variable", codeBlock.BoolValue ?? false),
                _ => codeBlock.BlockName ?? "variable"
            };
        }

        private static (CodeBlockType BlockType, string BlockName, VariableBlockType? VariableType) ResolveBlockMetadata(object? tagValue)
        {
            if (tagValue is VariableBlock variableBlock)
            {
                return (CodeBlockType.Variable, variableBlock.VariableName, variableBlock.VariableType);
            }

            string blockName = tagValue?.ToString() ?? "Block";
            if (tagValue is OperatorBlock)
            {
                return (CodeBlockType.Operator, "Operator", null);
            }

            CodeBlockType blockType = blockName switch
            {
                "If" => CodeBlockType.If,
                "While" => CodeBlockType.While,
                "Run" => CodeBlockType.Run,
                "Print" => CodeBlockType.Print,
                "=" => CodeBlockType.Assignment,
                "==" => CodeBlockType.Equals,
                _ => CodeBlockType.Unknown
            };

            return (blockType, blockName, null);
        }

        private static void ApplyBlockValues(CodeBlock codeBlock, object? tagValue)
        {
            if (tagValue is VariableBlock variableBlock)
            {
                codeBlock.UpdateVariableValues(variableBlock.StringValue, variableBlock.IntValue, variableBlock.BoolValue);
                return;
            }

            if (tagValue is OperatorBlock operatorBlock)
            {
                codeBlock.UpdateVariableValues(stringValue: operatorBlock.SelectedOperator);
                return;
            }

            codeBlock.UpdateVariableValues();
        }

        private void Block_Click(object? sender, EventArgs e)
        {
            if (!toggleBlockTypeLoggingToolStripMenuItem.Checked)
            {
                return;
            }

            if (sender is not Panel blockPanel)
            {
                return;
            }

            (CodeBlockType blockType, string blockName, VariableBlockType? variableType) = ResolveBlockMetadata(blockPanel.Tag);
            string typeText = variableType.HasValue
                ? $"{blockType} ({variableType.Value})"
                : blockType.ToString();

            AppendConsoleMessage(ConsoleMessageSeverity.Message, $"Clicked block '{blockName}' type: {typeText}");
        }

        /// <summary>
        /// Updates the stored <see cref="CodeBlock"/> position after a valid block move.
        /// </summary>
        /// <param name="blockPanel">The workspace panel representing the block.</param>
        /// <param name="snappedPlacement">The snapped placement assigned to the block.</param>
        private void UpdateStoredBlockPosition(Panel blockPanel, SnappedPlacement snappedPlacement)
        {
            if (!_workspaceBlocks.TryGetValue(blockPanel, out CodeBlock? codeBlock))
            {
                return;
            }

            codeBlock.UpdatePosition(snappedPlacement.Location.X, snappedPlacement.Location.Y);
            codeBlock.UpdateGridPosition(snappedPlacement.GridPosition.Column, snappedPlacement.GridPosition.Row);
            RepositionConnectorForChild(blockPanel);
        }

        /// <summary>
        /// Restores a workspace block to its last stored valid position.
        /// </summary>
        /// <param name="blockPanel">The workspace panel to restore.</param>
        private void RestoreStoredBlockPosition(Panel blockPanel)
        {
            if (!_workspaceBlocks.TryGetValue(blockPanel, out CodeBlock? codeBlock))
            {
                return;
            }

            blockPanel.Location = new Point((int)codeBlock.PosX, (int)codeBlock.PosY);
            AlignConnectedChildren(blockPanel);
            RepositionConnectorForChild(blockPanel);
        }

        /// <summary>
        /// Determines whether a grid cell is already occupied by another workspace block.
        /// </summary>
        /// <param name="gridPosition">The grid position to check.</param>
        /// <param name="movingBlock">The block currently being moved, if any.</param>
        /// <returns><see langword="true"/> if the grid cell is occupied; otherwise, <see langword="false"/>.</returns>
        private bool IsGridCellOccupied(GridPosition gridPosition, Panel? movingBlock = null)
        {
            foreach (KeyValuePair<Panel, CodeBlock> workspaceBlock in _workspaceBlocks)
            {
                if (workspaceBlock.Key == movingBlock)
                {
                    continue;
                }

                if (workspaceBlock.Value.GridColumn == gridPosition.Column &&
                    workspaceBlock.Value.GridRow == gridPosition.Row)
                {
                    return true;
                }
            }

            return false;
        }

        private void MovePanelChainDuringDrag(Panel rootPanel, Point newRootLocation)
        {
            Point delta = new(newRootLocation.X - rootPanel.Location.X, newRootLocation.Y - rootPanel.Location.Y);
            rootPanel.Location = newRootLocation;
            MoveChildPanelsByDelta(rootPanel, delta);
        }

        private void MoveChildPanelsByDelta(Panel parentPanel, Point delta)
        {
            Panel? childPanel = GetChildPanel(parentPanel);
            if (childPanel == null)
            {
                return;
            }

            childPanel.Location = new Point(childPanel.Location.X + delta.X, childPanel.Location.Y + delta.Y);
            RepositionConnectorForChild(childPanel);
            MoveChildPanelsByDelta(childPanel, delta);
        }

        private Panel? GetChildPanel(Panel parentPanel)
        {
            if (!_workspaceBlocks.TryGetValue(parentPanel, out CodeBlock? parentBlock) ||
                string.IsNullOrWhiteSpace(parentBlock.ChildBlockUid))
            {
                return null;
            }

            return GetPanelByUid(parentBlock.ChildBlockUid);
        }

        private Panel? GetParentPanel(Panel childPanel)
        {
            if (!_workspaceBlocks.TryGetValue(childPanel, out CodeBlock? childBlock) ||
                string.IsNullOrWhiteSpace(childBlock.ParentBlockUid))
            {
                return null;
            }

            return GetPanelByUid(childBlock.ParentBlockUid);
        }

        private Panel? GetPanelByUid(string uid)
        {
            foreach (KeyValuePair<Panel, CodeBlock> workspaceBlock in _workspaceBlocks)
            {
                if (workspaceBlock.Value.Uid == uid)
                {
                    return workspaceBlock.Key;
                }
            }

            return null;
        }

        private Panel? FindAttachableParentPanel(Panel childPanel)
        {
            if (!_workspaceBlocks.TryGetValue(childPanel, out CodeBlock? childBlock))
            {
                return null;
            }

            foreach (KeyValuePair<Panel, CodeBlock> workspaceBlock in _workspaceBlocks)
            {
                if (workspaceBlock.Key == childPanel)
                {
                    continue;
                }

                CodeBlock parentBlock = workspaceBlock.Value;
                bool isDirectlyBelowParent =
                    parentBlock.GridColumn == childBlock.GridColumn &&
                    parentBlock.GridRow + 1 == childBlock.GridRow;

                if (isDirectlyBelowParent && _blockConnectorService.CanConnect(parentBlock, childBlock))
                {
                    return workspaceBlock.Key;
                }
            }

            return null;
        }

        private void TryAttachBlockToConnector(Panel childPanel)
        {
            if (!_workspaceBlocks.TryGetValue(childPanel, out CodeBlock? childBlock))
            {
                return;
            }

            Panel? existingParentPanel = GetParentPanel(childPanel);
            Panel? attachableParentPanel = FindAttachableParentPanel(childPanel);

            if (existingParentPanel != null && existingParentPanel != attachableParentPanel)
            {
                DisconnectBlockFromConnector(childBlock);
            }

            if (attachableParentPanel == null)
            {
                RemoveConnectorVisualForChild(childPanel);
                return;
            }

            if (!_workspaceBlocks.TryGetValue(attachableParentPanel, out CodeBlock? parentBlock))
            {
                return;
            }

            if (childBlock.ParentBlockUid != parentBlock.Uid)
            {
                _blockConnectorService.Connect(parentBlock, childBlock);
                childPanel.Location = new Point((int)childBlock.PosX, (int)childBlock.PosY);
                AppendConsoleMessage(ConsoleMessageSeverity.Message, $"Connected {childBlock.BlockName ?? childBlock.Uid} to {parentBlock.BlockName ?? parentBlock.Uid}.");
            }

            EnsureConnectorVisual(attachableParentPanel, childPanel);
            RepositionConnectorForChild(childPanel);
            AlignConnectedChildren(childPanel);
        }

        private void DisconnectBlockFromConnector(CodeBlock block)
        {
            Dictionary<string, CodeBlock> blocksByUid = _workspaceBlocks.Values.ToDictionary(workspaceBlock => workspaceBlock.Uid, workspaceBlock => workspaceBlock);
            _blockConnectorService.Disconnect(block, blocksByUid);

            Panel? childPanel = GetPanelByUid(block.Uid);
            if (childPanel != null)
            {
                RemoveConnectorVisualForChild(childPanel);
            }
        }

        private void EnsureConnectorVisual(Panel parentPanel, Panel childPanel)
        {
            if (_connectorControlsByChild.ContainsKey(childPanel))
            {
                return;
            }

            BlockConnectorControl connector = new()
            {
                Tag = childPanel
            };

            _connectorControlsByChild[childPanel] = connector;
            groupBoxWorkSpace.Controls.Add(connector);
            connector.BringToFront();
            childPanel.BringToFront();
            _deleteDropZone?.BringToFront();
        }

        private void RemoveConnectorVisualForChild(Panel childPanel)
        {
            if (!_connectorControlsByChild.TryGetValue(childPanel, out BlockConnectorControl? connector))
            {
                return;
            }

            _connectorControlsByChild.Remove(childPanel);
            groupBoxWorkSpace.Controls.Remove(connector);
            connector.Dispose();
        }

        private void RepositionConnectorForChild(Panel childPanel)
        {
            if (!_connectorControlsByChild.TryGetValue(childPanel, out BlockConnectorControl? connector))
            {
                return;
            }

            Panel? parentPanel = GetParentPanel(childPanel);
            if (parentPanel == null)
            {
                return;
            }

            int connectorX = parentPanel.Left + (parentPanel.Width - connector.Width) / 2;
            int connectorY = parentPanel.Bottom + (childPanel.Top - parentPanel.Bottom - connector.Height) / 2;

            connector.Location = new Point(connectorX, connectorY);
            connector.BringToFront();
            childPanel.BringToFront();
            _deleteDropZone?.BringToFront();
        }

        private void AlignConnectedChildren(Panel parentPanel)
        {
            Panel? childPanel = GetChildPanel(parentPanel);
            if (childPanel == null ||
                !_workspaceBlocks.TryGetValue(parentPanel, out CodeBlock? parentBlock) ||
                !_workspaceBlocks.TryGetValue(childPanel, out CodeBlock? childBlock))
            {
                return;
            }

            Dictionary<string, CodeBlock> blocksByUid = _workspaceBlocks.Values.ToDictionary(workspaceBlock => workspaceBlock.Uid, workspaceBlock => workspaceBlock);
            _blockConnectorService.MoveChain(parentBlock, blocksByUid, parentBlock.GridColumn, parentBlock.GridRow);

            CodeBlock currentBlock = parentBlock;
            Panel? currentPanel = parentPanel;
            while (currentPanel != null && currentBlock != null)
            {
                currentPanel.Location = new Point((int)currentBlock.PosX, (int)currentBlock.PosY);
                RepositionConnectorForChild(currentPanel);

                currentPanel = GetChildPanel(currentPanel);
                currentBlock = currentPanel != null && _workspaceBlocks.TryGetValue(currentPanel, out CodeBlock? nextBlock)
                    ? nextBlock
                    : null;
            }
        }

        // keep blocks inside workspace bounds
        private Point ClampToBounds(Point loc, Size blockSize, Size containerSize)
        {
            int x = loc.X;
            int y = loc.Y;

            if (x < 0) x = 0;
            if (y < 0) y = 0;

            int maxX = containerSize.Width - blockSize.Width;
            int maxY = containerSize.Height - blockSize.Height;

            if (x > maxX) x = maxX;
            if (y > maxY) y = maxY;

            return new Point(x, y);
        }
        private void menuStrip1_ItemClicked(object sender, ToolStripItemClickedEventArgs e)
        {

        }

        private void Form1_Load(object sender, EventArgs e)
        {
            LayoutSplitContainerBelowMenu();
            ApplyInitialSplitLayout();
            SizeChanged += Form1_SizeChanged;
            AppendConsoleMessage(ConsoleMessageSeverity.Message, "Console ready.");
        }

        private void Form1_SizeChanged(object? sender, EventArgs e)
        {
            LayoutSplitContainerBelowMenu();
            EnsureSplitBounds();
        }

        private void EnsureSplitBounds()
        {
            if (_horizontalSplit != null)
            {
                ApplySplitLayout(_horizontalSplit, TopPanelMinSize, BottomPanelMinSize, _horizontalSplit.SplitterDistance);
            }

            if (_topVerticalSplit != null)
            {
                ApplySplitLayout(_topVerticalSplit, LeftPanelMinSize, RightPanelMinSize, _topVerticalSplit.SplitterDistance);
            }

            if (_bottomVerticalSplit != null)
            {
                ApplySplitLayout(_bottomVerticalSplit, LeftPanelMinSize, RightPanelMinSize, _bottomVerticalSplit.SplitterDistance);
            }
        }

        private void flowLayoutPanel1_Paint(object sender, PaintEventArgs e)
        {

        }

        private void groupBox1_Enter(object sender, EventArgs e)
        {

        }

        private void groupBox5_Enter(object sender, EventArgs e)
        {

        }

        private Project CreateWorkspaceProjectSnapshot(string projectName)
        {
            List<CodeBlock> blocks = new();

            foreach (KeyValuePair<Panel, CodeBlock> workspaceBlock in _workspaceBlocks)
            {
                CodeBlock source = workspaceBlock.Value;
                object? tagValue = workspaceBlock.Key.Tag;

                var snapshot = new CodeBlock(
                    source.PosX,
                    source.PosY,
                    source.Uid,
                    source.GridColumn,
                    source.GridRow,
                    source.BlockType,
                    source.BlockName,
                    source.VariableType,
                    source.StringValue,
                    source.IntValue,
                    source.BoolValue,
                    source.ParentBlockUid,
                    source.ChildBlockUid);

                ApplyBlockValues(snapshot, tagValue);
                blocks.Add(snapshot);
            }

            return new Project(projectName, blocks);
        }

        private void ReportWorkspaceSaveSuccess()
        {
            _workspaceSaveNotifier.ReportSaveSuccess();
            RefreshConsoleDisplay();
        }

        private void SaveWorkspaceLayout()
        {
            using SaveFileDialog saveDialog = new()
            {
                Filter = "BlockForge Project (*.bfg)|*.bfg|All files (*.*)|*.*",
                DefaultExt = "bfg",
                AddExtension = true,
                OverwritePrompt = true,
                FileName = "workspace.bfg"
            };

            if (saveDialog.ShowDialog(this) != DialogResult.OK)
            {
                return;
            }

            string projectName = Path.GetFileNameWithoutExtension(saveDialog.FileName);
            Project project = CreateWorkspaceProjectSnapshot(projectName);

            try
            {
                _projectFileManager.SaveFile(project, saveDialog.FileName);
                ReportWorkspaceSaveSuccess();
            }
            catch (Exception ex)
            {
                AppendConsoleMessage(ConsoleMessageSeverity.Warning, $"Save failed: {ex.Message}");
            }
        }

        private void ClearWorkspaceBlocks()
        {
            foreach (BlockConnectorControl connector in _connectorControlsByChild.Values)
            {
                groupBoxWorkSpace.Controls.Remove(connector);
                connector.Dispose();
            }

            _connectorControlsByChild.Clear();
            List<Panel> workspacePanels = new(_workspaceBlocks.Keys);

            foreach (Panel panel in workspacePanels)
            {
                groupBoxWorkSpace.Controls.Remove(panel);
                panel.Dispose();
            }

            _workspaceBlocks.Clear();
            _deleteDropZone?.BringToFront();
        }

        private Panel CreateImportedWorkspaceBlock(CodeBlock codeBlock)
        {
            object workspaceTag = CreateWorkspaceTagFromCodeBlock(codeBlock);
            string text = GetBlockDisplayText(workspaceTag);
            Color color = GetBlockColor(codeBlock.BlockType, codeBlock.VariableType);

            var panel = new Panel
            {
                Size = new Size(StandardBlockWidth, StandardBlockHeight),
                BackColor = color,
                BorderStyle = BorderStyle.FixedSingle,
                Cursor = Cursors.SizeAll,
                Tag = workspaceTag,
                Location = new Point((int)codeBlock.PosX, (int)codeBlock.PosY)
            };
            panel.MouseDown += WorkspaceBlock_MouseDown;
            panel.MouseMove += WorkspaceBlock_MouseMove;
            panel.MouseUp += WorkspaceBlock_MouseUp;
            panel.Click += Block_Click;
            BuildWorkspaceBlockContent(panel, workspaceTag, text);

            return panel;
        }

        private void LoadWorkspaceLayout()
        {
            using OpenFileDialog openDialog = new()
            {
                Filter = "BlockForge Project (*.bfg)|*.bfg|All files (*.*)|*.*",
                DefaultExt = "bfg",
                CheckFileExists = true,
                Multiselect = false
            };

            if (openDialog.ShowDialog(this) != DialogResult.OK)
            {
                return;
            }

            try
            {
                Project project = _projectFileManager.LoadFile(openDialog.FileName);

                ClearWorkspaceBlocks();

                foreach (CodeBlock codeBlock in project.CodeBlocks)
                {
                    Panel panel = CreateImportedWorkspaceBlock(codeBlock);
                    groupBoxWorkSpace.Controls.Add(panel);
                    panel.BringToFront();

                    var storedBlock = new CodeBlock(
                        codeBlock.PosX,
                        codeBlock.PosY,
                        codeBlock.Uid,
                        codeBlock.GridColumn,
                        codeBlock.GridRow,
                        codeBlock.BlockType,
                        codeBlock.BlockName,
                        codeBlock.VariableType,
                        codeBlock.StringValue,
                        codeBlock.IntValue,
                        codeBlock.BoolValue,
                        codeBlock.ParentBlockUid,
                        codeBlock.ChildBlockUid);

                    _workspaceBlocks[panel] = storedBlock;
                }

                foreach (Panel panel in _workspaceBlocks.Keys)
                {
                    if (GetParentPanel(panel) != null)
                    {
                        EnsureConnectorVisual(GetParentPanel(panel)!, panel);
                        RepositionConnectorForChild(panel);
                    }
                }

                _deleteDropZone?.BringToFront();
                AppendConsoleMessage(ConsoleMessageSeverity.Message, $"Loaded workspace: {project.ProjectName}");
            }
            catch (Exception ex)
            {
                AppendConsoleMessage(ConsoleMessageSeverity.Warning, $"Load failed: {ex.Message}");
            }
        }

        private void saveToolStripMenuItem_Click(object sender, EventArgs e)
        {
            SaveWorkspaceLayout();
        }

        private void openToolStripMenuItem_Click(object sender, EventArgs e)
        {
            LoadWorkspaceLayout();
        }

        private void generateDummyFile(Object sender, EventArgs e)
        {
            PayloadTransformer transformer = new PayloadTransformer(5);
            ProjectFileManager filemanager = new ProjectFileManager(transformer);

            List<CodeBlock> blocks = new List<CodeBlock>();
            blocks.Add(new CodeBlock(150, 300, "UID-1"));
            blocks.Add(new CodeBlock(300, 600, "UID-2"));
            blocks.Add(new CodeBlock(450, 900, "UID-3"));

            Project project = new Project("dummy_file", blocks);
            string filepath = project.ProjectName + ".bfg";

            filemanager.SaveFile(project);
        }

        private void loadDummyFile(Object sender, EventArgs e)
        {
            PayloadTransformer transformer = new PayloadTransformer(5);
            ProjectFileManager filemanager = new ProjectFileManager(transformer);

            string filepath = "dummy_file.bfg";
            Project? loaded = null;

            try
            {
                loaded = filemanager.LoadFile(filepath);
            }
            catch (Exception ex)
            {
                System.Diagnostics.Debug.WriteLine("Error occurred: " + ex.Message);
            }
            finally
            {
                if (loaded != null)
                {
                    System.Diagnostics.Debug.WriteLine("Loaded Project: " + loaded.ProjectName + " BFG Version: " + loaded.Version);
                    if (loaded.CodeBlocks.Count > 0)
                    {
                        foreach (CodeBlock block in loaded.CodeBlocks)
                        {
                            System.Diagnostics.Debug.WriteLine($"{block.Uid}: {block.PosX}, {block.PosY}");
                        }
                    }
                }
            }
        }
    }
}

