using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Data;
using System.Drawing;
using System.Linq;
using System.Text;
using System.Windows.Forms;

using System.Drawing.Drawing2D;

namespace ImageProcessing
{
    public partial class Form1 : Form
    {
        public Form1()
        {
            InitializeComponent();
        }

        private Bitmap? OriginalBm = null;
        private Bitmap? CurrentBm = null;

        private void Form1_Load(object sender, EventArgs e)
        {
            // Disable menu items because no image is loaded.
            SetMenusEditable(false);
        }

        // Enable or disable menu items that are
        // appropriate when an image is loaded.
        private void SetMenusEditable(bool enabled)
        {
            ToolStripMenuItem[] items =
            {
                mnuFileSaveAs,
                mnuFileReset,
                mnuGeometry,
                mnuPointOperations,
                mnuEnhancements,
                mnuFilters,
            };
            foreach (ToolStripMenuItem item in items)
                item.Enabled = enabled;
            resultPictureBox.Visible = enabled;
        }

        private void mnuFileOpen_Click(object sender, EventArgs e)
        {
            ofdFile.Title = "Open Image File";
            ofdFile.Multiselect = false;
            if (ofdFile.ShowDialog() == DialogResult.OK)
            {
                try
                {
                    Bitmap bm = LoadBitmapUnlocked(ofdFile.FileName);
                    OriginalBm = bm;
                    CurrentBm = (Bitmap)OriginalBm.Clone();
                    resultPictureBox.Image = CurrentBm;

                    // Enable menu items because an image is loaded.
                    SetMenusEditable(true);
                }
                catch (Exception ex)
                {
                    MessageBox.Show(string.Format(
                        "Error opening file {0}.\n{1}",
                        ofdFile.FileName, ex.Message));
                }
            }
        }

        private void mnuFileSaveAs_Click(object sender, EventArgs e)
        {
            sfdFile.Title = "Save As";
            if (sfdFile.ShowDialog() == DialogResult.OK)
            {
                try
                {
                    CurrentBm?.SaveImage(sfdFile.FileName);
                }
                catch (Exception ex)
                {
                    MessageBox.Show(string.Format(
                        "Error saving file {0}.\n{1}",
                        sfdFile.FileName, ex.Message));
                }
            }
        }

        // Restore the original unmodified image.
        private void mnuFileReset_Click(object sender, EventArgs e)
        {
            CurrentBm = OriginalBm?.Clone() as Bitmap;
            resultPictureBox.Image = CurrentBm;
        }

        // Make a montage of files.
        private void mnuFileMontage_Click(object sender, EventArgs e)
        {
            // Let the user select the files.
            ofdFile.Title = "Select Montage Files";
            ofdFile.Multiselect = true;
            if (ofdFile.ShowDialog() == DialogResult.OK)
            {
                OriginalBm = MakeMontage(ofdFile.FileNames, Color.Black);
                CurrentBm = (Bitmap)OriginalBm!.Clone();
                resultPictureBox.Image = CurrentBm;

                // Enable menu items because an image is loaded.
                SetMenusEditable(true);
            }
        }

        // Make a montage of files, four per row.
        private Bitmap? MakeMontage(string[] filenames, Color bgColor)
        {
            int maxHeight = 0;
            int maxWidth = 0;
            int imageCount = filenames.Length;
            Bitmap[] bitmaps = new Bitmap[imageCount];
            for (int i = 0; i < imageCount; i++)
            {
                bitmaps[i] = LoadBitmapUnlocked(filenames[i]);
                maxWidth = Math.Max(maxWidth, bitmaps[i].Width);
                maxHeight = Math.Max(maxHeight, bitmaps[i].Height);
            }

            int columnCount = Math.Min(4, imageCount);
            int width = maxWidth * columnCount;
            int rowCount = (int)Math.Ceiling((double)(imageCount / 4.0));
            int height = maxHeight * rowCount;
            Bitmap result = new(width, height);

            using (Graphics graphics = Graphics.FromImage(result))
            {
                graphics.Clear(bgColor);
                for (int j = 0; j < imageCount; j++)
                {
                    int row = j / columnCount;
                    int column = j % columnCount;
                    int x = column * maxWidth;
                    int y = row * maxHeight;
                    graphics.DrawImage(bitmaps[j], x, y);
                }
            }
            return result;
        }

        private void mnuFileExit_Click(object sender, EventArgs e)
        {
            Close();
        }

        // Load a bitmap without locking it.
        private Bitmap LoadBitmapUnlocked(string file_name)
        {
            using (Bitmap bm = new(file_name))
            {
                return new Bitmap(bm);
            }
        }

        // Rotate the image.
        private void mnuGeometryRotate_Click(object sender, EventArgs e)
        {
            var angle = InputForm.GetFloat("Rotation Angle", "Angle", "0", float.NegativeInfinity, float.PositiveInfinity, "Invalid input");
            if (angle == float.NaN) return;

            CurrentBm = CurrentBm?.RotateAtCenter(angle, Color.Black, InterpolationMode.High);
            resultPictureBox.Image = CurrentBm;
        }

        // Scale the image uniformly.
        private void mnuGeometryScale_Click(object sender, EventArgs e)
        {
            var scale = InputForm.GetFloat("Scale Factor", "Scale", "1", 0.1f, 100.0f, "Invalid input");
            if (scale == float.NaN) return;

            CurrentBm = CurrentBm?.Scale(scale, InterpolationMode.High);
            resultPictureBox.Image = CurrentBm;
        }

        private void mnuGeometryStretch_Click(object sender, EventArgs e)
        {
            var rawScale = InputForm.GetString("Stretch Parameters", "Stretch x Stretch y (space as seperator)", "1 1");
            if (rawScale == null)
            {
                MessageBox.Show("No input detected");
                return;
            }

            rawScale = rawScale.Trim();
            var rawScaleArray = rawScale.Split(' ', StringSplitOptions.RemoveEmptyEntries);
            if (rawScaleArray.Length != 2)
            {
                MessageBox.Show("Malformed input. Use number <space> number");
                return;
            }

            float scaleX, scaleY;
            if (!float.TryParse(rawScaleArray[0], out scaleX))
            {
                MessageBox.Show($"X value not a valid float: {rawScaleArray[0]}");
            }

            if (!float.TryParse(rawScaleArray[1], out scaleY))
            {
                MessageBox.Show($"Y value not a valid float: {rawScaleArray[1]}");
            }

            CurrentBm = CurrentBm?.Scale(scaleX, scaleY, InterpolationMode.High);
            resultPictureBox.Image = CurrentBm;
        }

        private void mnuGeometryRotateFlip_Click(object sender, EventArgs e)
        {
            InputForm dialog = new();
            dialog.captionLabel.Text =
                "1) Flip Horizontal\n" +
                "2) Flip Vertical\n" +
                "3) Rotate 90\n" +
                "4) Rotate 180\n" +
                "5) Rotate 270\n";
            dialog.Text = "Rotate/Flip";
            dialog.valueTextBox.Text = "1";
            dialog.Size = new(300, 220);
            if (dialog.ShowDialog() == DialogResult.Cancel) return;
            int choice;
            if (!int.TryParse(dialog.valueTextBox.Text, out choice) || choice < 1 || choice > 5)
            {
                MessageBox.Show("Choice should be an integer between 1 and 5");
                return;
            }

            switch (choice)
            {
                case 1:
                    CurrentBm?.RotateFlip(RotateFlipType.RotateNoneFlipX);
                    break;
                case 2:
                    CurrentBm?.RotateFlip(RotateFlipType.RotateNoneFlipY);
                    break;
                case 3:
                    CurrentBm?.RotateFlip(RotateFlipType.Rotate90FlipNone);
                    break;
                case 4:
                    CurrentBm?.RotateFlip(RotateFlipType.Rotate180FlipNone);
                    break;
                case 5:
                    CurrentBm?.RotateFlip(RotateFlipType.Rotate270FlipNone);
                    break;
            }

            resultPictureBox.Image = CurrentBm;
        }

        #region Cropping
        private Point StartPoint, EndPoint;

        // Let the user select an area and crop to that area.
        private void mnuGeometryCrop_Click(object sender, EventArgs e)
        {
            resultPictureBox.MouseDown += crop_MouseDown;
            resultPictureBox.Cursor = Cursors.Cross;
        }

        // Let the user select an area with a desired
        // aspect ratio and crop to that area.
        private void mnuGeometryCropToAspect_Click(object sender, EventArgs e)
        {

        }

        private void crop_MouseDown(object? sender, MouseEventArgs e)
        {
            resultPictureBox.MouseDown -= crop_MouseDown;
            resultPictureBox.MouseMove += crop_MouseMove;
            resultPictureBox.MouseUp += crop_MouseUp;
            resultPictureBox.Paint += resultPictureBox_Paint;
            StartPoint = new Point(e.X, e.Y);
            EndPoint = StartPoint;
        }

        private void resultPictureBox_Paint(object? sender, PaintEventArgs e)
        {
            e.Graphics.DrawDashedRectange(Color.Black, Color.White, 1, 3, StartPoint, EndPoint);
        }

        private void crop_MouseMove(object? sender, MouseEventArgs e)
        {
            EndPoint = new Point(e.X, e.Y);
            resultPictureBox.Refresh();
        }

        private void crop_MouseUp(object? sender, MouseEventArgs e)
        {
            resultPictureBox.Cursor = Cursors.Default;
            resultPictureBox.MouseMove -= crop_MouseMove;
            resultPictureBox.MouseUp -= crop_MouseUp;
            resultPictureBox.Paint -= resultPictureBox_Paint;
            CurrentBm = CurrentBm?.Crop(StartPoint.ToRectangle(EndPoint), InterpolationMode.High);
            resultPictureBox.Image = CurrentBm;
        }

        #endregion Cropping

        #region Point Processes

        // Set each color component to 255 - the original value.
        private void mnuPointInvert_Click(object sender, EventArgs e)
        {
            CurrentBm?.ApplyPointOp(
                (ref byte r, ref byte g, ref byte b, ref byte a) =>
                {
                    r = (byte)(255 - r);
                    g = (byte)(255 - g);
                    b = (byte)(255 - b);
                }
            );
            resultPictureBox.Refresh();
        }

        // Set color components less than a specified value to 0.
        private void mnuPointColorCutoff_Click(object sender, EventArgs e)
        {
            var cutOff = InputForm.GetInt("Cutoff Value", "Cutoff (0 - 255)", "0", 0, 255, "Value should be between 0 and 255");
            if (cutOff < 0 || cutOff > 255)
            {
                MessageBox.Show("Value should be between 0 and 255");
                return;
            }

            CurrentBm?.ApplyPointOp(
                (ref byte r, ref byte g, ref byte b, ref byte a) =>
                {
                    if (r < cutOff) r = 0;
                    if (g < cutOff) g = 0;
                    if (b < cutOff) b = 0;
                }
            );
            resultPictureBox.Refresh();
        }

        // Set each pixel's red color component to 0.
        private void mnuPointClearRed_Click(object sender, EventArgs e)
        {
            CurrentBm?.ApplyPointOp(
                (ref byte r, ref byte g, ref byte b, ref byte a) =>
                {
                    r = 0;
                }
            );
            resultPictureBox.Refresh();
        }

        // Set each pixel's green color component to 0.
        private void mnuPointClearGreen_Click(object sender, EventArgs e)
        {
            CurrentBm?.ApplyPointOp(
                (ref byte r, ref byte g, ref byte b, ref byte a) =>
                {
                    g = 0;
                }
            );
            resultPictureBox.Refresh();
        }


        // Set each pixel's blue color component to 0.
        private void mnuPointClearBlue_Click(object sender, EventArgs e)
        {
            CurrentBm?.ApplyPointOp(
                (ref byte r, ref byte g, ref byte b, ref byte a) =>
                {
                    b = 0;
                }
            );
            resultPictureBox.Refresh();
        }

        // Average each pixel's color component.
        private void mnuPointAverage_Click(object sender, EventArgs e)
        {
            CurrentBm?.ApplyPointOp(
                (ref byte r, ref byte g, ref byte b, ref byte a) =>
                {
                    var avg = (byte)((r + g + b) / 3);
                    r = avg;
                    g = avg;
                    b = avg;
                }
            );
            resultPictureBox.Refresh();
        }

        // Convert each pixel to grayscale.
        private void mnuPointGrayscale_Click(object sender, EventArgs e)
        {
            CurrentBm?.ApplyPointOp(
            (ref byte r, ref byte g, ref byte b, ref byte a) =>
                {
                    var avg = (byte)(r * 0.3 + g * 0.5 + b * 0.2);
                    r = avg;
                    g = avg;
                    b = avg;
                }
            );
            resultPictureBox.Refresh();
        }

        // Convert each pixel to sepia tone.
        private void mnuPointSepiaTone_Click(object sender, EventArgs e)
        {
            CurrentBm?.ApplyPointOp(
                (ref byte r, ref byte g, ref byte b, ref byte a) =>
                    {
                        float newR = r * 0.393f + g * 0.769f + b * 0.189f;
                        float newG = r * 0.349f + g * 0.686f + b * 0.168f;
                        float newB = r * 0.272f + g * 0.534f + b * 0.131f;
                        r = (byte)(newR > 255 ? 255 : newR);
                        g = (byte)(newG > 255 ? 255 : newG);
                        b = (byte)(newB > 255 ? 255 : newB);
                    }
                );
            resultPictureBox.Refresh();
        }

        // Apply a color tone to the image.
        private void mnuPointColorTone_Click(object sender, EventArgs e)
        {
            if (cdColorTone.ShowDialog() == DialogResult.OK)
            {
                float newR = cdColorTone.Color.R;
                float newG = cdColorTone.Color.G;
                float newB = cdColorTone.Color.B;
                CurrentBm?.ApplyPointOp(
                    (ref byte r, ref byte g, ref byte b, ref byte a) =>
                    {
                        float brightness = (r + g + b) / (3f * 255f);
                        r = (byte)(brightness * newR);
                        g = (byte)(brightness * newG);
                        b = (byte)(brightness * newB);
                    }
                );
                resultPictureBox.Refresh();
            }
        }

        // Set non-maximal color components to 0.
        private void mnuPointSaturate_Click(object sender, EventArgs e)
        {
            CurrentBm?.ApplyPointOp(
                (ref byte r, ref byte g, ref byte b, ref byte a) =>
                {
                    byte max = Math.Max(Math.Max(r, g), b);
                    if (r != max) r = 0;
                    if (g != max) g = 0;
                    if (b != max) b = 0;
                }
            );
            resultPictureBox.Refresh();
        }

        #endregion Point Processes

        #region Enhancements

        private void mnuEnhancementsColor_Click(object sender, EventArgs e)
        {

        }

        // Use histogram stretching to modify contrast.
        private void mnuEnhancementsContrast_Click(object sender, EventArgs e)
        {

        }

        private void mnuEnhancementsBrightness_Click(object sender, EventArgs e)
        {

        }

        #endregion Enhancements

        #region Filters

        private void mnuFiltersBoxBlur_Click(object sender, EventArgs e)
        {

        }

        private void mnuFiltersUnsharpMask_Click(object sender, EventArgs e)
        {

        }

        private void mnuFiltersRankFilter_Click(object sender, EventArgs e)
        {

        }

        private void mnuFiltersMedianFilter_Click(object sender, EventArgs e)
        {

        }

        private void mnuFiltersMinFilter_Click(object sender, EventArgs e)
        {

        }

        private void mnuFiltersMaxFilter_Click(object sender, EventArgs e)
        {

        }

        // Display a dialog where the user can select
        // and modify a default kernel.
        // If the user clicks OK, apply the kernel.
        private void mnuFiltersCustomKernel_Click(object sender, EventArgs e)
        {

        }

        #endregion Filters

    }
}
