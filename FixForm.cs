using System;
using System.Drawing;
using System.IO;
using System.Windows.Forms;
using System.Collections.Generic;
using System.Linq;
using PdfS = PdfSharp.Pdf;
using PdfSharp.Pdf.IO;
using PdfiumViewer;
using PdfSharp.Pdf.Content.Objects;

namespace FlipFix
{
    public partial class FixForm : Form
    {
        // Fields to manage PDF processing and UI state
        private string selectedFilePath = null; // Path of the selected PDF file
        private List<Image> pagePreviewImages = new List<Image>(); // Stores preview images of PDF pages
        private PdfiumViewer.PdfDocument pdfiumDoc = null; // PDF document for rendering previews
        private PdfS.PdfDocument pdfSharpDoc = null; // PDF document for manipulation (reordering/swapping)
        private PdfS.PdfDocument fixedDocument = null; // Modified PDF after processing
        private MemoryStream fixedDocumentStream = null; // In-memory stream for the modified PDF
        private HashSet<int> pagesWithoutBacks = new HashSet<int>(); // Pages without back pages in custom mode
        private bool customModeEnabled = false; // Flag for custom interleaving mode
        private bool swapPerformed = false; // Tracks if a page swap has occurred

        private List<MemoryStream> documentHistory = new List<MemoryStream>(); // Stores document states for undo/redo
        private int currentHistoryIndex = -1; // Current position in document history
        private const int MAX_HISTORY = 50; // Maximum number of history states to store
        private float currentZoomFactor = 1.0f; // Current zoom level for document preview (100% default)

        public FixForm()
        {
            InitializeComponent(); // Initialize UI components
            this.MinimumSize = new Size(972, 400);// Set minimum form size
            SetupMouseWheelZoom(); // Setup mouse wheel zooming
        }

        /// Handles zoom in button click
        private void BtnZoomIn_Click(object sender, EventArgs e)
        {
            // Increase zoom by 10% up to 200%
            if (currentZoomFactor < 2.0f)
            {
                currentZoomFactor += 0.1f;
                ApplyZoom();

            }
        }

        // Handles zoom out button click
        private void BtnZoomOut_Click(object sender, EventArgs e)
        {
            // Decrease zoom down to 10%
            if (currentZoomFactor > 0.1f) 
            {
                currentZoomFactor -= 0.1f;
                ApplyZoom();

            }
        }

        // Handles zoom track bar value change
        private void ZoomTrackBar_ValueChanged(object sender, EventArgs e)
        {
            // Convert trackbar value to zoom factor (0.5-2.0)
            ApplyZoom();
        }

        // Applies the current zoom factor to the document preview
        private void ApplyZoom()
        {
            try
            {
                // Clamp zoom factor between 10% and 200%
                if (currentZoomFactor < 0.1f) currentZoomFactor = 0.1f;  
                if (currentZoomFactor > 2.0f) currentZoomFactor = 2.0f;

                // Update zoom percentage label
                lblZoomPercentage.Text = $"{(int)(currentZoomFactor * 100)}%";

                // Resize all preview images while maintaining aspect ratio
                foreach (Control rowControl in previewTableLayout.Controls)
                {
                    if (rowControl is Panel pagePanel)
                    {
                        PictureBox pictureBox = null;
                        foreach (Control control in pagePanel.Controls)
                        {
                            if (control is PictureBox pb)
                            {
                                pictureBox = pb;
                                break;
                            }
                        }

                        if (pictureBox != null && pictureBox.Image != null)
                        {
                            // Store original aspect ratio of the image
                            float aspectRatio = (float)pictureBox.Image.Height / pictureBox.Image.Width;

                            int baseWidth = 900; // Base width at 100% zoom
                            int newWidth = (int)(baseWidth * currentZoomFactor);
                            int newHeight = (int)(newWidth * aspectRatio);

                            // Update PictureBox dimensions
                            pictureBox.Width = newWidth;
                            pictureBox.Height = newHeight;

                            // Update panel dimensions to fit PictureBox and labels
                            pagePanel.Width = newWidth;
                            pagePanel.Height = newHeight + 40;

                            // Reposition labels
                            foreach (Control control in pagePanel.Controls)
                            {
                                if (control is Label label)
                                {
                                    label.Width = newWidth;

                                    if (label.Text.StartsWith("Page "))
                                    {
                                        // Page number label
                                        label.Location = new Point(0, pagePanel.Height - 38);
                                    }
                                    else if (label.Text == "Reordered" || label.Text == "Modified")
                                    {
                                        // Status label
                                        label.Location = new Point(0, pagePanel.Height - 18);
                                    }
                                }
                            }
                        }
                    }
                }

                // Update layout
                previewTableLayout.PerformLayout();
                Application.DoEvents(); // Ensure UI responsiveness
            }
            catch (Exception ex)
            {
                MessageBox.Show($"Error during zoom operation: {ex.Message}", "Zoom Error", MessageBoxButtons.OK, MessageBoxIcon.Error);
            }
        }

        // Resets zoom to default (100%)
        private void ResetZoom()
        {
            currentZoomFactor = 1.0f;
            lblZoomPercentage.Text = "100%";
            ApplyZoom();
        }
        // Overrides key processing for keyboard shortcuts
        protected override bool ProcessCmdKey(ref Message msg, Keys keyData)
        {
            // Ctrl+Plus: Zoom in
                if (keyData == (Keys.Control | Keys.Oemplus) || keyData == (Keys.Control | Keys.Add)) 
            {
                BtnZoomIn_Click(this, EventArgs.Empty);
                return true;
            }

            // Ctrl+Minus: Zoom out
            if (keyData == (Keys.Control | Keys.OemMinus) || keyData == (Keys.Control | Keys.Subtract))
            {
                BtnZoomOut_Click(this, EventArgs.Empty);
                return true;
            }

            // Ctrl+0: Reset zoom
            if (keyData == (Keys.Control | Keys.D0) || keyData == (Keys.Control | Keys.NumPad0))
            {
                ResetZoom();
                ApplyZoom();
                return true;
            }

            // Ctrl+Z: Undo
            if (keyData == (Keys.Control | Keys.Z))
            {
                if (btnUndo.Enabled)
                {
                    BtnUndo_Click(this, EventArgs.Empty);
                }
                return true;
            }

            // Ctrl+Y: Redo
            if (keyData == (Keys.Control | Keys.Y))
            {
                if (btnRedo.Enabled)
                {
                    BtnRedo_Click(this, EventArgs.Empty);
                }
                return true;
            }

            return base.ProcessCmdKey(ref msg, keyData);
        }

        // Sets up mouse wheel zooming for the form and preview controls
        private void SetupMouseWheelZoom()
        {
            this.MouseWheel += Form_MouseWheel;// Attach mouse wheel event to form

            documentPreviewPanel.MouseWheel += Form_MouseWheel; // Attach to preview panel
            previewTableLayout.MouseWheel += Form_MouseWheel; // Attach to table layout
        }

        
    // Handles mouse wheel events for zooming
        private void Form_MouseWheel(object sender, MouseEventArgs e)
        {
            // Only zoom if Ctrl key is pressed
            if (ModifierKeys == Keys.Control)
            {
                // Zoom in or out based on wheel direction
                if (e.Delta > 0)
                {
                    // Wheel scrolled up - zoom in
                    if (currentZoomFactor < 2.0f)
                    {
                        currentZoomFactor += 0.1f;
                        ApplyZoom();
                    }
                }
                else
                {
                    // Wheel scrolled down - zoom out
                    if (currentZoomFactor > 0.1f)
                    {
                        currentZoomFactor -= 0.1f;
                        ApplyZoom();
                    }
                }
            }
        }

        // Records the current document state in history
        private void RecordDocumentState()
        {
            if (fixedDocumentStream == null) return;

            // Create a copy of the current document state
            MemoryStream stateCopy = new MemoryStream();
            fixedDocumentStream.Position = 0;
            fixedDocumentStream.CopyTo(stateCopy);
            fixedDocumentStream.Position = 0;

            // Clear future states if not at the end of history
            if (currentHistoryIndex < documentHistory.Count - 1)
            {
                // Remove all states after the current one
                for (int i = documentHistory.Count - 1; i > currentHistoryIndex; i--)
                {
                    documentHistory[i].Dispose();
                    documentHistory.RemoveAt(i);
                }
            }

            // Add the new state
            documentHistory.Add(stateCopy);
            currentHistoryIndex = documentHistory.Count - 1;

            // Update button states
            UpdateUndoRedoButtons();

            // Limit history size
            if (MAX_HISTORY > 0 && documentHistory.Count > MAX_HISTORY)
            {
                // Remove oldest state
                documentHistory[0].Dispose();
                documentHistory.RemoveAt(0);
                currentHistoryIndex--;
            }
        }
        // Handles undo button click
        private async void BtnUndo_Click(object sender, EventArgs e)
        {
            // Nothing to undo or at the first state
            if (currentHistoryIndex <= 0 || documentHistory.Count <= 1)
            {
                MessageBox.Show("Nothing to undo.", "Undo", MessageBoxButtons.OK, MessageBoxIcon.Information);
                return;
            }

            try
            {
                // Disable buttons during undo
                btnUndo.Enabled = false;
                btnRedo.Enabled = false;
                btnGenerate.Enabled = false;
                btnSwap.Enabled = false;
                btnDownload.Enabled = false;

                UpdateProgress(0, "Undoing last operation...");

                await Task.Run(() =>
                {
                    // Decrement history index to go back one state
                    currentHistoryIndex--;

                    // Replace current document with previous state
                    if (fixedDocumentStream != null)
                        fixedDocumentStream.Dispose();

                    // Create a copy of the previous state
                    fixedDocumentStream = new MemoryStream();
                    documentHistory[currentHistoryIndex].Position = 0;
                    documentHistory[currentHistoryIndex].CopyTo(fixedDocumentStream);
                    documentHistory[currentHistoryIndex].Position = 0;
                    fixedDocumentStream.Position = 0;
                });

                // Update preview
                await DisplayFixedDocumentPreviewAsync();

                UpdateProgress(100, "Undo successful.");
                HideProgressBarAfterDelay();

                // Update button states
                UpdateUndoRedoButtons();
                btnGenerate.Enabled = true;
                btnSwap.Enabled = true;
                btnDownload.Enabled = true;
            }
            catch (Exception ex)
            {
                progressBar.Visible = false;
                lblStatus.Text = "Error during undo operation.";
                MessageBox.Show($"Error during undo: {ex.Message}", "Undo Error", MessageBoxButtons.OK, MessageBoxIcon.Error);

                // Re-enable buttons
                UpdateUndoRedoButtons();
                btnGenerate.Enabled = true;
                btnSwap.Enabled = true;
                btnDownload.Enabled = true;
            }
        }

        // Handles redo button click
        private async void BtnRedo_Click(object sender, EventArgs e)
        {
            if (currentHistoryIndex >= documentHistory.Count - 1)
            {
                // Nothing to redo
                MessageBox.Show("Nothing to redo.", "Redo", MessageBoxButtons.OK, MessageBoxIcon.Information);
                return;
            }

            try
            {
                // Disable buttons during redo
                btnUndo.Enabled = false;
                btnRedo.Enabled = false;
                btnGenerate.Enabled = false;
                btnSwap.Enabled = false;
                btnDownload.Enabled = false;

                UpdateProgress(0, "Redoing last undone operation...");

                await Task.Run(() =>
                {
                    // Increment history index to go forward one state
                    currentHistoryIndex++;

                    // Replace current document with next state
                    if (fixedDocumentStream != null)
                        fixedDocumentStream.Dispose();

                    // Create a copy of the next state
                    fixedDocumentStream = new MemoryStream();
                    documentHistory[currentHistoryIndex].Position = 0;
                    documentHistory[currentHistoryIndex].CopyTo(fixedDocumentStream);
                    documentHistory[currentHistoryIndex].Position = 0;
                    fixedDocumentStream.Position = 0;
                });

                // Update preview
                await DisplayFixedDocumentPreviewAsync();

                UpdateProgress(100, "Redo successful.");
                HideProgressBarAfterDelay();

                // Update button states
                UpdateUndoRedoButtons();
                btnGenerate.Enabled = true;
                btnSwap.Enabled = true;
                btnDownload.Enabled = true;
            }
            catch (Exception ex)
            {
                progressBar.Visible = false;
                lblStatus.Text = "Error during redo operation.";
                MessageBox.Show($"Error during redo: {ex.Message}", "Redo Error", MessageBoxButtons.OK, MessageBoxIcon.Error);

                // Re-enable buttons
                UpdateUndoRedoButtons();
                btnGenerate.Enabled = true;
                btnSwap.Enabled = true;
                btnDownload.Enabled = true;
            }
        }

        // Updates the enabled state of undo/redo buttons
        private void UpdateUndoRedoButtons()
        {
            if (InvokeRequired)
            {
                // Ensure UI thread execution
                BeginInvoke(new Action(UpdateUndoRedoButtons));
                return;
            }

            // Enable/disable undo button based on history
            btnUndo.Enabled = currentHistoryIndex > 0 && documentHistory.Count > 0;

            // Enable/disable redo button based on history
            btnRedo.Enabled = currentHistoryIndex < documentHistory.Count - 1;
        }
        // Updates the progress bar and status label with thread safety
        private void UpdateProgress(int value, string statusText)
        {
            if (InvokeRequired)
            {
                // Ensure UI update on the main thread
                BeginInvoke(new Action(() => UpdateProgress(value, statusText)));
                return;
            }

            progressBar.Value = value;// Set progress bar value
            lblStatus.Text = statusText;// Update status label
            progressBar.Visible = true;// Show progress bar
            Application.DoEvents();// Process UI events to keep it responsive
        }

        // Validates if the selection of pages without backs matches the page count parity
        private bool IsValidSelection(HashSet<int> selection)
        {
            int totalPageCount = pdfiumDoc.PageCount; // Total pages in the document
            int selectedCount = selection.Count; // Number of selected pages
            bool isOddTotalPages = totalPageCount % 2 != 0; // Check if total pages are odd
            bool isOddSelection = selectedCount % 2 != 0; // Check if selection is odd

            // Valid if odd total pages have odd selection, or even total pages have even selection
            return isOddTotalPages ? isOddSelection : !isOddSelection;
        }

        // Shows an error message for invalid page selections in custom mode
        private void ShowValidationError(int selectedCount)
        {
            int totalPageCount = pdfiumDoc.PageCount; // Total pages
            bool isOddTotalPages = totalPageCount % 2 != 0; // Parity check
            string message = $"Error: You have {totalPageCount} pages ({(isOddTotalPages ? "odd" : "even")} number). " +
                $"You need to select an {(isOddTotalPages ? "odd" : "even")} number of pages without backs. " +
                $"Currently you have {selectedCount} pages selected.";

            // Display error message to the user
            MessageBox.Show(message, "Interleaving Error", MessageBoxButtons.OK, MessageBoxIcon.Error);
        }

        // Handles click on the drop zone to open a file dialog for PDF selection
        private void PanelDropZone_Click(object sender, EventArgs e)
        {
            OpenFileDialog openFileDialog = new OpenFileDialog
            {
                Filter = "PDF files (*.pdf)|*.pdf",// Restrict to PDF files
                Title = "Select a PDF File"
            };

            if (openFileDialog.ShowDialog() == DialogResult.OK)
            {
                string filePath = openFileDialog.FileName; // Get selected file path
                ProcessSelectedFile(filePath); // Process the selected file
            }
        }

        // Processes a selected PDF file, loading it and generating previews
        private void ProcessSelectedFile(string filePath)
        {
            FileInfo fileInfo = new FileInfo(filePath); // Get file information
            string ext = fileInfo.Extension.ToLower(); // Get file extension

            lblFileName.Text = fileInfo.Name; // Display file name in UI

            if (ext == ".pdf") // Check if the file is a PDF
            {
                try
                {
                    UpdateProgress(0, "Loading PDF pages..."); // Show loading progress
                    btnGenerate.Enabled = false;// Disable buttons during loading
                    if (btnDownload != null)
                        btnDownload.Enabled = false;
                    if (btnSwap != null)
                        btnSwap.Enabled = false;

                    // Run PDF loading on a background thread
                    Task.Run(() =>
                    {
                        try
                        {
                            CleanupDocuments(); // Clean up any existing documents

                            // Load PDF documents for rendering and manipulation
                            var tempPdfiumDoc = PdfiumViewer.PdfDocument.Load(filePath);
                            var tempPdfSharpDoc = PdfReader.Open(filePath, PdfSharp.Pdf.IO.PdfDocumentOpenMode.Import);

                            // Update UI on the main thread
                            this.Invoke((MethodInvoker)delegate
                            {
                                pdfiumDoc = tempPdfiumDoc; // Assign loaded documents
                                pdfSharpDoc = tempPdfSharpDoc;

                                lblPageCount.Text = pdfiumDoc.PageCount.ToString();// Show page count
                                selectedFilePath = filePath; // Store file path

                                // Generate page previews asynchronously
                                GeneratePagePreviewsAsync();
                                btnClear.Enabled = true;// Enable clear button
                                btnGenerate.Enabled = true;// Enable generate button
                                if (btnDownload != null)
                                    btnDownload.Enabled = true; // Enable download button

                                if (btnSwap != null)
                                    btnSwap.Enabled = true; // Enable swap button

                                // Create initial document stream
                                CreateInitialDocumentStream();

                                UpdateProgress(100, "PDF loaded successfully.");// Update progress
                                HideProgressBarAfterDelay();// Hide progress bar after delay
                            });
                        }
                        catch (Exception ex)
                        {
                            // Handle errors on the main thread
                            this.Invoke((MethodInvoker)delegate
                            {
                                progressBar.Visible = false; // Hide progress bar
                                MessageBox.Show($"Error opening PDF file: {ex.Message}"); // Show error
                                lblStatus.Text = "Error loading PDF."; // Update status
                            });
                        }
                    });
                }
                catch (Exception ex)
                {
                    progressBar.Visible = false;// Hide progress bar on error
                    MessageBox.Show($"Error opening PDF file: {ex.Message}"); // Show error
                    lblStatus.Text = "Error loading PDF."; // Update status
                }
            }
            else
            {
                // Handle non-PDF files
                lblPageCount.Text = "N/A"; // Reset page count
                lblStatus.Text = "Only PDF files are supported."; // Show error message
                selectedFilePath = null; // Clear file path
                ClearPagePreviews(); // Clear previews
                progressBar.Visible = false; // Hide progress bar
                btnGenerate.Enabled = false; // Disable buttons
                if (btnDownload != null)
                    btnDownload.Enabled = false;
                if (btnSwap != null)
                    btnSwap.Enabled = false;
            }
        }

        // Creates an initial in-memory stream from the original PDF
        private void CreateInitialDocumentStream()
        {
            if (pdfSharpDoc == null) return; // Exit if no document is loaded

            fixedDocument = new PdfS.PdfDocument(); // Create new document

            // Copy all pages from the original document
            for (int i = 0; i < pdfSharpDoc.PageCount; i++)
            {
                fixedDocument.AddPage(pdfSharpDoc.Pages[i]);
            }

            // Dispose of existing stream if it exists
            if (fixedDocumentStream != null)
                fixedDocumentStream.Dispose();

            fixedDocumentStream = new MemoryStream(); // Create new memory stream
            fixedDocument.Save(fixedDocumentStream); // Save document to stream
            fixedDocumentStream.Position = 0; // Reset stream position

            // Clear history and add initial state
            ClearDocumentHistory();
            RecordDocumentState();
        }

        // Clear document history when loading a new file
        private void ClearDocumentHistory()
        {
            // Dispose of all memory streams
            foreach (var stream in documentHistory)
            {
                stream.Dispose();
            }

            // Clear the list and reset index
            documentHistory.Clear();
            currentHistoryIndex = -1;

            // Update button states
            UpdateUndoRedoButtons();
        }

        // Handles drag-and-drop of files onto the drop zone
        private void Panel_DragDrop(object sender, DragEventArgs e)
        {
            string[] files = (string[])e.Data.GetData(DataFormats.FileDrop); // Get dropped files
            string filePath = files[0]; // Process the first file
            ProcessSelectedFile(filePath); // Process the file
        }

        // Sets the drag effect for drag-and-drop operations
        private void Panel_DragEnter(object sender, DragEventArgs e)
        {
            if (e.Data.GetDataPresent(DataFormats.FileDrop))
                e.Effect = DragDropEffects.Copy; // Allow file drop
        }

        // Hides the progress bar after a 1-second delay
        private void HideProgressBarAfterDelay()
        {
            System.Threading.Timer timer = null;
            timer = new System.Threading.Timer((state) =>
            {
                this.BeginInvoke(new Action(() => { progressBar.Visible = false; })); // Hide progress bar
                timer.Dispose(); // Dispose timer
            }, null, 1000, System.Threading.Timeout.Infinite);
        }

        // Generates preview images for each PDF page asynchronously
        private async Task GeneratePagePreviewsAsync()
        {
            if (pdfiumDoc == null) return; // Exit if no document is loaded

            // Ensure UI thread execution
            if (InvokeRequired)
            {
                Invoke((MethodInvoker)delegate
                {
                    GeneratePagePreviewsAsync(); // Recurse on UI thread
                });
                return;
            }

            // Store current zoom 
            float savedZoomFactor = currentZoomFactor;

            ClearPagePreviews(); // Clear existing previews
            documentPreviewPanel.Visible = true; // Show preview panel
            int previewWidth = 900; // Width for preview images

            // Clear and configure the TableLayoutPanel
            previewTableLayout.Controls.Clear();
            previewTableLayout.RowStyles.Clear();
            previewTableLayout.ColumnStyles.Clear();

            previewTableLayout.ColumnCount = 1; // Single column layout
            previewTableLayout.ColumnStyles.Add(new ColumnStyle(SizeType.Percent, 100F));

            int rowCount = pdfiumDoc.PageCount; // One row per page
            previewTableLayout.RowCount = rowCount;

            for (int i = 0; i < rowCount; i++)
            {
                previewTableLayout.RowStyles.Add(new RowStyle(SizeType.AutoSize)); // Auto-size rows
            }

            // Configure layout anchoring and width
            previewTableLayout.Anchor = AnchorStyles.Top | AnchorStyles.Left | AnchorStyles.Right;
            previewTableLayout.Width = documentPreviewPanel.ClientSize.Width - 20;

            // Generate previews for each page
            for (int i = 0; i < pdfiumDoc.PageCount; i++)
            {
                int pageIndex = i; // Capture for lambda
                int progressValue = (int)((pageIndex + 1) * 100.0 / pdfiumDoc.PageCount);
                UpdateProgress(progressValue, $"Rendering page {pageIndex + 1} of {pdfiumDoc.PageCount}...");

                try
                {
                    // Render page image on a background thread
                    Image pageImage = await Task.Run(() =>
                    {
                        try
                        {
                            SizeF pageSize = pdfiumDoc.PageSizes[pageIndex]; // Get page size
                            double pageRatio = pageSize.Height / pageSize.Width;  // Calculate aspect ratio
                            int previewHeight = (int)(previewWidth * pageRatio); // Calculate height
                            if (previewHeight < 50) previewHeight = (int)(previewWidth * 1.414); // Minimum height

                            // Render page as an image
                            return pdfiumDoc.Render(pageIndex, previewWidth, previewHeight, 72, 72, false);
                        }
                        catch (Exception ex)
                        {
                            Console.WriteLine($"Error rendering page {pageIndex}: {ex.Message}");
                            return null;
                        }
                    });

                    if (pageImage == null) continue;  // Skip if rendering failed

                    pagePreviewImages.Add(pageImage);  // Store preview image

                    // Create UI elements for the page preview
                    Panel pagePanel = new Panel
                    {
                        Width = previewWidth,
                        Height = (int)(previewWidth * (pageImage.Height / (double)pageImage.Width)) + 25,
                        BorderStyle = BorderStyle.FixedSingle,
                        Margin = new Padding(10),
                        BackColor = Color.White,
                        Anchor = AnchorStyles.None
                    };

                    PictureBox pictureBox = new PictureBox
                    {
                        Image = pageImage,
                        SizeMode = PictureBoxSizeMode.StretchImage,
                        Width = previewWidth,
                        Height = pagePanel.Height - 25,
                        Location = new Point(0, 0),
                        Dock = DockStyle.Top
                    };

                    Label pageNumberLabel = new Label
                    {
                        Text = "Page " + (pageIndex + 1).ToString(),
                        TextAlign = ContentAlignment.MiddleCenter,
                        Width = previewWidth,
                        Height = 20,
                        Location = new Point(0, pagePanel.Height - 23),
                        Dock = DockStyle.Bottom
                    };
                    // Add controls to panel and panel to layout
                    pagePanel.Controls.Add(pictureBox);
                    pagePanel.Controls.Add(pageNumberLabel);
                    previewTableLayout.Controls.Add(pagePanel, 0, pageIndex);

                    // Update UI periodically to keep it responsive
                    if (pageIndex % 3 == 0 || pageIndex == pdfiumDoc.PageCount - 1)
                        Application.DoEvents();
                }
                catch (Exception ex)
                {
                    Console.WriteLine($"Error processing preview for page {pageIndex}: {ex.Message}");
                    continue;
                }
            }

            if (savedZoomFactor != 1.0f)
            {
                currentZoomFactor = savedZoomFactor;
                ApplyZoom();
            }

            previewTableLayout.AutoSize = true;
            previewTableLayout.AutoSize = true; // Adjust layout size
        }

      
        // Clears all page previews and disposes of images
        private void ClearPagePreviews()
        {
            foreach (Image img in pagePreviewImages)
            {
                img.Dispose(); // Dispose of each preview image
            }
            pagePreviewImages.Clear(); // Clear the list

            if (previewTableLayout != null)
            {
                previewTableLayout.Controls.Clear();// Clear layout controls
            }

            documentPreviewPanel.Visible = false;// Hide preview panel

            // Reset zoom to 100%
            //ResetZoom();
        }
        // Handles the "Generate" button click to reorder pages
        private async void BtnGenerate_Click(object sender, EventArgs e)
        {
            // If a swap was performed, use the current fixed document stream
            if (swapPerformed && fixedDocumentStream != null)
            {
                try
                {
                    // Create a temporary file from the current stream
                    string tempSwappedPath = Path.Combine(Path.GetTempPath(), "temp_swapped_" + Guid.NewGuid().ToString() + ".pdf");
                    using (FileStream fs = new FileStream(tempSwappedPath, FileMode.Create))
                    {
                        fixedDocumentStream.Position = 0;
                        fixedDocumentStream.CopyTo(fs);
                    }
                    fixedDocumentStream.Position = 0;

                    // Load the swapped document for further processing
                    pdfSharpDoc = PdfReader.Open(tempSwappedPath, PdfSharp.Pdf.IO.PdfDocumentOpenMode.Import);


                    // Clean up temporary file
                    try { File.Delete(tempSwappedPath); } catch { }
                }
                catch (Exception ex)
                {
                    MessageBox.Show("Error processing swapped document: " + ex.Message);
                    return;
                }
            }

            swapPerformed = false;  // Reset swap flag
            if (string.IsNullOrEmpty(selectedFilePath) || pdfSharpDoc == null)
            {
                MessageBox.Show("Please drag a valid PDF file first.");  // Prompt for PDF
                return;
            }
            // Validate custom mode selection
            if (customModeEnabled && !IsValidSelection(pagesWithoutBacks))
            {
                ShowValidationError(pagesWithoutBacks.Count); // Show validation error
                return; 
            }

            try
            {
                // Disable buttons during processing
                btnGenerate.Enabled = false;
                btnSwap.Enabled = false;
                btnDownload.Enabled = false;
                UpdateProgress(0, "Processing... Please wait."); // Show progress

                int pageCount = pdfSharpDoc.PageCount; // Get page count

                // Reorder pages on a background thread
                await Task.Run(() =>
                {
                    fixedDocument = new PdfS.PdfDocument(); // Create new document

                    if (customModeEnabled && pagesWithoutBacks.Count > 0)
                    {
                        // Custom interleaving logic
                        List<int> processOrder = new List<int>();
                        foreach (int pageIndex in pagesWithoutBacks.OrderBy(p => p))
                        {
                            processOrder.Add(pageIndex); // Add pages without backs
                        }

                        int half = (pageCount - pagesWithoutBacks.Count) / 2;  // Calculate front/back pages
                        List<int> frontPages = new List<int>();
                        List<int> backPages = new List<int>();

                        // Separate remaining pages into front and back
                        for (int i = 0; i < pageCount; i++)
                        {
                            if (!pagesWithoutBacks.Contains(i) && frontPages.Count < half)
                            {
                                frontPages.Add(i);
                            }
                            else if (!pagesWithoutBacks.Contains(i))
                            {
                                backPages.Add(i);
                            }
                        }
                        // Add front and back pages in interleaved order
                        for (int i = 0; i < frontPages.Count; i++)
                        {
                            processOrder.Add(frontPages[i]);
                            if (i < backPages.Count)
                            {
                                processOrder.Add(backPages[i]);
                            }
                        }
                        // Add pages to the new document
                        for (int i = 0; i < processOrder.Count; i++)
                        {
                            fixedDocument.AddPage(pdfSharpDoc.Pages[processOrder[i]]);

                            int progressValue = (int)((i + 1) * 100.0 / processOrder.Count);
                            UpdateProgress(progressValue, $"Processing page {i + 1} of {processOrder.Count}...");
                        }
                    }
                    else
                    {
                        // Standard interleaving for odd or even page counts
                        bool isOdd = pageCount % 2 != 0;// Check if page count is odd
                        int halfPoint = (int)Math.Ceiling(pageCount / 2.0);// Calculate midpoint

                        if (isOdd)
                        {
                            // Interleave for odd page counts
                            for (int i = 0; i < halfPoint - 1; i++)
                            {
                                // Add front page
                                fixedDocument.AddPage(pdfSharpDoc.Pages[i]);

                                // Add back page 
                                fixedDocument.AddPage(pdfSharpDoc.Pages[i + halfPoint]);

                                int progressValue = (int)((i * 2 + 2) * 100.0 / pageCount);
                                UpdateProgress(progressValue, $"Processing page pair {i + 1} of {halfPoint - 1}...");
                            }

                            // Add the middle page (no back)
                            fixedDocument.AddPage(pdfSharpDoc.Pages[halfPoint - 1]);
                            UpdateProgress(100, "Completed processing all pages");
                        }
                        else
                        {
                            // Standard interleaving for even page counts
                            for (int i = 0; i < halfPoint; i++)
                            {
                                fixedDocument.AddPage(pdfSharpDoc.Pages[i]);  // Add front page
                                fixedDocument.AddPage(pdfSharpDoc.Pages[i + halfPoint]); // Add back page

                                int progressValue = (int)((i + 1) * 100.0 / halfPoint);
                                UpdateProgress(progressValue, $"Processing page pair {i + 1} of {halfPoint}...");
                            }
                        }
                    }

                    // Save the reordered document to a memory stream
                    if (fixedDocumentStream != null)
                        if (fixedDocumentStream != null)
                        fixedDocumentStream.Dispose();

                    fixedDocumentStream = new MemoryStream();
                    fixedDocument.Save(fixedDocumentStream);
                    fixedDocumentStream.Position = 0; // Reset stream position
                });

                RecordDocumentState(); // Record the new state


                // Display the reordered document preview
                await DisplayFixedDocumentPreviewAsync();
                UpdateProgress(100, "Pages reordered successfully."); // Update progress
                HideProgressBarAfterDelay(); // Hide progress bar

                  // Re-enable buttons
                btnSwap.Enabled = true;
                btnDownload.Enabled = true;
                btnGenerate.Enabled = true;
            }
            catch (Exception ex)
            {
                // Handle errors during generation
                progressBar.Visible = false;
                lblStatus.Text = "Error during generation.";
                MessageBox.Show("Error: " + ex.Message);
                btnGenerate.Enabled = true;
                btnSwap.Enabled = true;
                btnDownload.Enabled = true;
            }
        }

        // Displays previews of the reordered PDF pages
        private async Task DisplayFixedDocumentPreviewAsync()
        {
            if (InvokeRequired)
            {
                // Ensure UI thread execution
                await Task.Run(() => BeginInvoke(new Action(async () => await DisplayFixedDocumentPreviewAsync())));
                return;
            }

            // Store current zoom 
            float savedZoomFactor = currentZoomFactor;

            ClearPagePreviews(); // Clear existing previews
            if (fixedDocumentStream == null) return; // Exit if no document stream

            documentPreviewPanel.Visible = true; // Show preview panel
            int previewWidth = 900; // Width for preview images

            // Create a temporary file for preview
            string tempPdfPath = Path.Combine(Path.GetTempPath(), "temp_fixed_" + Guid.NewGuid().ToString() + ".pdf"); 
            using (FileStream fileStream = new FileStream(tempPdfPath, FileMode.Create))
            {
                fixedDocumentStream.Position = 0;
                fixedDocumentStream.CopyTo(fileStream); // Save stream to file
            }

            // Clear and configure the TableLayoutPanel
            previewTableLayout.Controls.Clear();
            previewTableLayout.RowStyles.Clear();
            previewTableLayout.ColumnStyles.Clear();

            using (PdfiumViewer.PdfDocument tempPdfiumDoc = PdfiumViewer.PdfDocument.Load(tempPdfPath))
            {
                previewTableLayout.ColumnCount = 1; // Single column layout
                previewTableLayout.ColumnStyles.Add(new ColumnStyle(SizeType.Percent, 100F));

                int rowCount = tempPdfiumDoc.PageCount;// One row per page
                previewTableLayout.RowCount = rowCount;

                for (int i = 0; i < rowCount; i++)
                {
                    previewTableLayout.RowStyles.Add(new RowStyle(SizeType.AutoSize)); // Auto-size rows
                }

                // Configure layout anchoring and width
                previewTableLayout.Anchor = AnchorStyles.Top | AnchorStyles.Left | AnchorStyles.Right;
                previewTableLayout.Width = documentPreviewPanel.ClientSize.Width - 20;

                // Generate previews for each page
                for (int i = 0; i < tempPdfiumDoc.PageCount; i++)
                {
                    int progressValue = (int)((i + 1) * 100.0 / tempPdfiumDoc.PageCount);
                    UpdateProgress(progressValue, $"Rendering fixed page {i + 1} of {tempPdfiumDoc.PageCount}...");

                    try
                    {
                        // Render page image on a background thread
                        Image pageImage = await Task.Run(() =>
                        {
                            SizeF pageSize = tempPdfiumDoc.PageSizes[i]; // Get page size
                            double pageRatio = pageSize.Height / pageSize.Width;  // Calculate aspect ratio
                            int previewHeight = (int)(previewWidth * pageRatio); // Calculate height
                            if (previewHeight < 50) previewHeight = (int)(previewWidth * 1.414);  // Minimum height

                            // Render page as an image
                            return tempPdfiumDoc.Render(i, previewWidth, previewHeight, 72, 72, false);
                        });

                        pagePreviewImages.Add(pageImage);  // Store preview image

                        // Create UI elements for the page preview
                        Panel pagePanel = new Panel
                        {
                            Width = previewWidth,
                            Height = (int)(previewWidth * (pageImage.Height / (double)pageImage.Width)) + 40,
                            BorderStyle = BorderStyle.FixedSingle,
                            Margin = new Padding(10),
                            BackColor = Color.FromArgb(240, 248, 255),
                            Anchor = AnchorStyles.None
                        };

                        PictureBox pictureBox = new PictureBox
                        {
                            Image = pageImage,
                            SizeMode = PictureBoxSizeMode.StretchImage,
                            Width = previewWidth,
                            Height = pagePanel.Height - 40,
                            Location = new Point(0, 0),
                            Dock = DockStyle.Top
                        };

                        Label pageNumberLabel = new Label
                        {
                            Text = "Page " + (i + 1).ToString(),
                            TextAlign = ContentAlignment.MiddleCenter,
                            Width = previewWidth,
                            Height = 20,
                            Location = new Point(0, pagePanel.Height - 38)
                        };

                        Label fixedLabel = new Label
                        {
                            Text = swapPerformed ? "Modified" : "Reordered",
                            ForeColor = Color.Green,
                            Font = new Font("Arial", 7, FontStyle.Italic),
                            TextAlign = ContentAlignment.MiddleCenter,
                            Width = previewWidth,
                            Height = 15,
                            Location = new Point(0, pagePanel.Height - 18)
                        };

                        // Add controls to panel and panel to layout
                        pagePanel.Controls.Add(pictureBox);
                        pagePanel.Controls.Add(pageNumberLabel);
                        pagePanel.Controls.Add(fixedLabel);
                        previewTableLayout.Controls.Add(pagePanel, 0, i);

                        // Update UI periodically
                        if (i % 3 == 0 || i == tempPdfiumDoc.PageCount - 1)
                            Application.DoEvents();
                    }
                    catch
                    {
                        continue; // Skip failed pages
                    }
                }
            }

            previewTableLayout.AutoSize = true; // Adjust layout size
            if (savedZoomFactor != 1.0f)
            {
                currentZoomFactor = savedZoomFactor;
                ApplyZoom();
            }

            // Clean up temporary file
            try
            {
                if (File.Exists(tempPdfPath))
                    File.Delete(tempPdfPath);
            }
            catch { }
        }

        // Handles the "Download" button click to save the modified PDF
        private async void BtnDownload_Click(object sender, EventArgs e)
        {
            if (fixedDocumentStream == null && pdfSharpDoc != null)
            {
                // Create a default stream if none exists
                CreateInitialDocumentStream();
            }

            if (fixedDocumentStream == null)
            {
                MessageBox.Show("No document available to download. Please load a PDF file first."); // Prompt for PDF
                return;
            }

            using (SaveFileDialog sfd = new SaveFileDialog())
            {
                sfd.Filter = "PDF files (*.pdf)|*.pdf"; // Restrict to PDF files
                sfd.Title = "Save PDF"; // Dialog title

                string prefix = swapPerformed ? "Modified_" : "Fixed_"; // File name prefix
                sfd.FileName = prefix + Path.GetFileName(selectedFilePath);  // Suggested file name

                if (sfd.ShowDialog() == DialogResult.OK)
                {
                    try
                    {
                        btnDownload.Enabled = false;  // Disable button during saving
                        UpdateProgress(0, "Saving file...");  // Show progress

                      // Save file on a background thread
                        await Task.Run(() =>
                        {
                            using (FileStream fileStream = new FileStream(sfd.FileName, FileMode.Create))
                            {
                                fixedDocumentStream.Position = 0;
                                fixedDocumentStream.CopyTo(fileStream);  // Save stream to file
                            }
                        });

                        UpdateProgress(100, "File saved: " + sfd.FileName);  // Update progress
                        HideProgressBarAfterDelay();  // Hide progress bar
                        MessageBox.Show("PDF saved successfully.");  // Show success message
                        btnDownload.Enabled = true;  // Re-enable button
                    }
                    catch (Exception ex)
                    {
                        // Handle errors during saving
                        progressBar.Visible = false;
                        lblStatus.Text = "Error saving file.";
                        MessageBox.Show($"Error saving file: {ex.Message}");
                        btnDownload.Enabled = true;
                    }
                }
                else
                {
                    btnDownload.Enabled = true;  // Re-enable button if dialog is canceled
                }
            }
        }

        // Cleans up document resources and resets state
        private void CleanupDocuments()
        {
            if (pdfiumDoc != null)
            {
                pdfiumDoc.Dispose();  // Dispose of Pdfium document
                pdfiumDoc = null;
            }

            if (fixedDocumentStream != null)
            {
                fixedDocumentStream.Dispose();  // Dispose of memory stream
                fixedDocumentStream = null;
            }
            swapPerformed = false;  // Reset swap flag

            // Disable buttons requiring a document
            if (btnSwap != null)
                btnSwap.Enabled = false;

            if (btnDownload != null)
                btnDownload.Enabled = false;

            pdfSharpDoc = null; // Clear PdfSharp document
            fixedDocument = null; // Clear fixed document
            pagesWithoutBacks.Clear(); // Clear custom mode pages
            customModeEnabled = false; // Disable custom mode
        }

        // Handles the "Swap" button click to swap two pages
        private async void BtnSwap_Click(object sender, EventArgs e)
        {
            if (pdfSharpDoc == null)
            {
                MessageBox.Show("Please load a PDF file first.");  // Prompt for PDF
                return;
            }

            // Create a default stream if none exists
            if (fixedDocumentStream == null)
            {
                CreateInitialDocumentStream();
            }

            // Get page count from the current document stream
            int pageCount = 0;
            using (MemoryStream ms = new MemoryStream())
            {
                fixedDocumentStream.Position = 0;
                fixedDocumentStream.CopyTo(ms);
                ms.Position = 0;

                using (var tempPdf = PdfiumViewer.PdfDocument.Load(ms))
                {
                    pageCount = tempPdf.PageCount;  // Get page count
                }
            }
            fixedDocumentStream.Position = 0;

            // Check if document has only one page - throw error
            if (pageCount <= 1)
            {
                MessageBox.Show("Error: Document contains only one page. Page swapping requires at least two pages.",
                                "Cannot Swap Pages",
                                MessageBoxButtons.OK,
                                MessageBoxIcon.Error);
                return;
            }

            // Create a dialog for selecting pages to swap
            using (Form swapDialog = new Form())
            {
                swapDialog.Text = "Swap Pages";  // Dialog title
                swapDialog.Size = new Size(400, 250); // Dialog size
                swapDialog.FormBorderStyle = FormBorderStyle.FixedDialog;  // Fixed border
                swapDialog.StartPosition = FormStartPosition.CenterParent;  // Center on parent
                swapDialog.MaximizeBox = false; // Disable maximize
                swapDialog.MinimizeBox = false; // Disable minimize

                // UI elements for page selection with adjusted font size
                Label lblPage1 = new Label
                {
                    Text = "First Page:",
                    Location = new Point(20, 20),
                    Size = new Size(100, 25),
                    Font = new Font(FontFamily.GenericSansSerif, 12, FontStyle.Regular)
                };

                NumericUpDown numPage1 = new NumericUpDown
                {
                    Location = new Point(150, 18),
                    Size = new Size(100, 30),
                    Minimum = 1,
                    Maximum = pageCount,
                    Value = 1,
                    Font = new Font(FontFamily.GenericSansSerif, 12, FontStyle.Regular)
                };

                Label lblPage2 = new Label
                {
                    Text = "Second Page:",
                    Location = new Point(20, 60),
                    Size = new Size(125, 25),
                    Font = new Font(FontFamily.GenericSansSerif, 12, FontStyle.Regular)
                };

                NumericUpDown numPage2 = new NumericUpDown
                {
                    Location = new Point(150, 58),
                    Size = new Size(100, 30),
                    Minimum = 1,
                    Maximum = pageCount,
                    Value = Math.Min(2, pageCount),
                    Font = new Font(FontFamily.GenericSansSerif, 12, FontStyle.Regular)
                };

                // Removed the extra label

                Button btnSwapExecute = new Button
                {
                    Text = "Swap",
                    DialogResult = DialogResult.OK,
                    Location = new Point(130, 110),
                    Size = new Size(120, 40),
                    Font = new Font(FontFamily.GenericSansSerif, 12, FontStyle.Bold)
                };

                // No validation events needed - we'll check only when button is clicked

                // Add controls to dialog
                swapDialog.Controls.Add(lblPage1);
                swapDialog.Controls.Add(numPage1);
                swapDialog.Controls.Add(lblPage2);
                swapDialog.Controls.Add(numPage2);
                // Removed the lblPageCount control
                swapDialog.Controls.Add(btnSwapExecute);

                // No validation events needed - we'll check only when button is clicked

                // Show the dialog and process if OK was clicked
                if (swapDialog.ShowDialog() == DialogResult.OK)
                {
                    int pageIndex1 = (int)numPage1.Value - 1; // Convert to 0-based index
                    int pageIndex2 = (int)numPage2.Value - 1; // Convert to 0-based index

                    // Final validation before proceeding
                    if (pageIndex1 >= pageCount || pageIndex2 >= pageCount)
                    {
                        MessageBox.Show($"Error: Cannot swap pages. Page number exceeds document total ({pageCount} pages).",
                                        "Invalid Page Number",
                                        MessageBoxButtons.OK,
                                        MessageBoxIcon.Error);
                        return;
                    }

                    if (pageIndex1 == pageIndex2)
                    {
                        MessageBox.Show("Cannot swap a page with itself.",
                                        "Invalid Selection",
                                        MessageBoxButtons.OK,
                                        MessageBoxIcon.Warning);
                        return;
                    }

                    try
                    {
                        // Disable buttons during swapping
                        btnSwap.Enabled = false;
                        btnDownload.Enabled = false;
                        btnGenerate.Enabled = false;
                        UpdateProgress(0, "Swapping pages..."); // Show progress

                        // Perform page swap on a background thread
                        await Task.Run(() =>
                        {
                            fixedDocumentStream.Position = 0;  // Reset stream position

                            // Create a temporary file for swapping
                            string tempFilePath = Path.Combine(Path.GetTempPath(), "temp_swap_" + Guid.NewGuid().ToString() + ".pdf");
                            using (FileStream fs = new FileStream(tempFilePath, FileMode.Create))
                            {
                                fixedDocumentStream.CopyTo(fs);
                            }

                            // Load the temporary file for modification
                            PdfS.PdfDocument sourceDoc = PdfReader.Open(tempFilePath, PdfSharp.Pdf.IO.PdfDocumentOpenMode.Import);

                            PdfS.PdfDocument swappedDoc = new PdfS.PdfDocument(); // Create new document

                            // Copy pages with the swap applied
                            for (int i = 0; i < sourceDoc.PageCount; i++)
                            {
                                int sourceIndex = i;

                                if (i == pageIndex1)
                                    sourceIndex = pageIndex2;  // Swap first page
                                else if (i == pageIndex2)
                                    sourceIndex = pageIndex1;  // Swap second page

                                swappedDoc.AddPage(sourceDoc.Pages[sourceIndex]); // Add page

                                int progressValue = (int)((i + 1) * 100.0 / sourceDoc.PageCount);
                                UpdateProgress(progressValue, $"Processing page {i + 1} of {sourceDoc.PageCount}...");
                            }

                            // Save the swapped document to a new stream
                            MemoryStream newDocStream = new MemoryStream();
                            swappedDoc.Save(newDocStream);

                            // Replace the old stream
                            var oldStream = fixedDocumentStream;
                            fixedDocumentStream = newDocStream;
                            oldStream?.Dispose();

                            // Clean up temporary file
                            try
                            {
                                File.Delete(tempFilePath);
                            }
                            catch { }
                        });

                        RecordDocumentState(); // Record the new state
                        swapPerformed = true; // Mark swap as performed
                        await DisplayFixedDocumentPreviewAsync();  // Display updated preview
                        UpdateProgress(100, $"Pages {pageIndex1 + 1} and {pageIndex2 + 1} swapped successfully.");  // Update progress
                        HideProgressBarAfterDelay(); // Hide progress bar

                        // Re-enable buttons
                        btnSwap.Enabled = true;
                        btnDownload.Enabled = true;
                        btnGenerate.Enabled = true;
                    }
                    catch (Exception ex)
                    {
                        // Handle errors during swapping
                        progressBar.Visible = false;
                        lblStatus.Text = "Error swapping pages.";
                        MessageBox.Show($"Error: {ex.Message}", "Page Swap Error", MessageBoxButtons.OK, MessageBoxIcon.Error);
                        btnSwap.Enabled = true;
                        btnDownload.Enabled = true;
                        btnGenerate.Enabled = true;
                    }
                }
            }
        }


        // Handles the "Clear" button click to reset the application
        private void BtnClear_Click(object sender, EventArgs e)
        {
            // Call the comprehensive clear method
            ClearAll();
        }

        // Resets the application state, clearing all resources and UI
        private void ClearAll()
        {
            try
            {
                //ClearDocumentHistory(); // Clear document history
                // Clean up documents and resources
                //CleanupDocuments();

                ResetZoom(); // Reset zoom to default

                // Clear UI elements
                ClearPagePreviews();  // Clear page previews
                lblFileName.Text = "No file selected"; // Reset file name
                lblPageCount.Text = "0"; // Reset page count
                lblStatus.Text = "Ready"; // Reset status

                selectedFilePath = null; // Clear file path
                swapPerformed = false; // Reset swap flag
                customModeEnabled = false; // Disable custom mode
                pagesWithoutBacks.Clear(); // Clear custom mode pages
                progressBar.Visible = false; // Hide progress bar

                // Disable buttons requiring a document
                btnGenerate.Enabled = false;
                btnDownload.Enabled = false;
                btnSwap.Enabled = false;
                //btnClear.Enabled = false;

                // Show the drop zone and hide previews
                panelDropZone.Visible = true;
                documentPreviewPanel.Visible = true;

                // Force garbage collection to free memory
                GC.Collect();

                // Show confirmation message
                MessageBox.Show("Application has been reset successfully.", "Clear Complete", MessageBoxButtons.OK, MessageBoxIcon.Information);
            }
            catch (Exception ex)
            {


                MessageBox.Show($"Error while clearing: {ex.Message}", "Clear Error", MessageBoxButtons.OK, MessageBoxIcon.Error);
            }
        }
    }
}


    
