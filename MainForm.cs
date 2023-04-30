using Quantizer.Classes;
using Quantizer.Extensions;
using Quantizer.Interfaces;
using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Data;
using System.Drawing;
using System.Drawing.Imaging;
using System.Linq;
using System.Runtime.InteropServices;
using System.Windows.Forms;
using System.IO;

namespace Quantizer
{
    public partial class MainForm : Form
    {
        Bitmap result;
        public MainForm()
        {
            InitializeComponent();
        }

        private void MainForm_Load(object sender, EventArgs e)
        {
            panelRight.Width = panelMain.Width / 2;
        }

        private void buttonBrowse_Click(object sender, EventArgs e)
        {
            if (dialogOpenFile.ShowDialog() == DialogResult.OK)
            {
                var filename = dialogOpenFile.FileName;
                var source = new Bitmap(filename);

                SetSourceImage(filename);

                result = source.Quantize(int.Parse(colorCount.Text));
                result.Save("./temp");

                textBox1.Text = string.Format("Оригинальное изображение: {0} цветов ({1} Б)", source.GetColorCount(), new FileInfo(filename).Length);
                textBox2.Text = string.Format("Квантизированное изображение: {0} цветов ({1} Б)", result.GetColorCount(), new FileInfo("./temp").Length);

                SetTargetImage(result);

            }
        }

        private void SetSourceImage(string filename)
        {
            pictureSource.Image = Image.FromFile(filename);
        }

        private void SetTargetImage(Bitmap target)
        {
            pictureTarget.Image = target;
        }

        private void MainForm_Resize(object sender, EventArgs e)
        {
            panelRight.Width = panelMain.Width / 2;
        }

        private void buttonSave_Click(object sender, EventArgs e)
        {
            if(result == null)
            {
                MessageBox.Show("Сперва нужно загрузить изображение!");
                return;
            }

            SaveFileDialog saveFileDialog1 = new SaveFileDialog();
            saveFileDialog1.Filter = "PNG Image|*.png|JPeg Image|*.jpg";
            saveFileDialog1.Title = "Save an Image File";
            saveFileDialog1.ShowDialog();


            if (saveFileDialog1.FileName != "")
            {
                result.Save(saveFileDialog1.FileName);
            }
        }
    }
}
