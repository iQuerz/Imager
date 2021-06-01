using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Data;
using System.Drawing;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Forms;
using System.IO;

namespace Gallery
{
    public partial class Form1 : Form
    {
        private List<PictureBox> _pics;
        GalleryManager _manager;
        int border; // space between each picture
        public Form1()
        {
            InitializeComponent();
            _pics = new List<PictureBox>();
            border = 10;
            //if there's no root folder data, initialize the manager without it. otherwise initialize with the previously read path.
            BinaryReader rootCheck = new BinaryReader(File.Open("source.dat", FileMode.Open));
            string s = rootCheck.ReadString();
            rootCheck.Close();
            _manager = s != null ? new GalleryManager(this, s) : new GalleryManager(this);
            //load the pictures
            try { _manager.load(); }
            catch(Exception e) { MessageBox.Show(e.Message); }
        }

        private void Form1_Load(object sender, EventArgs e)
        {
            loadPictureBoxes();
            AutoScroll = false;
            HorizontalScroll.Visible = false;
            HorizontalScroll.Enabled = false;
            HorizontalScroll.Maximum = 0;
            AutoScroll = true;
        }

        private void FolderSourceClick(object sender, EventArgs e)
        {
            if (browser.ShowDialog() == DialogResult.OK)
            {
                _manager.updateSource(browser.SelectedPath);
                _manager.flush();
                _manager.load();
            }
            deletePictureBoxes();
            loadPictureBoxes();
        }

        private void RefreshClick(object sender, EventArgs e)
        {
            _manager.flush();
            _manager.load();
            deletePictureBoxes();
            loadPictureBoxes();
        }

        /// <summary>
        /// Makes new pictureboxes and loads them with pictures. Use this after updating the folder source.
        /// </summary>
        private void loadPictureBoxes()
        {
            int len = _manager.Count();
            int x = border;
            int y = border;
            for (int i = 0; i < len; i++)
            {
                Image img = _manager[i];
                int height = System.Windows.Forms.Screen.PrimaryScreen.Bounds.Height / 5; //smallest picture displayed should be quarter of the screen's height
                if (this.ClientRectangle.Height / 4 > height) height = this.ClientRectangle.Height / 4;
                double width = ((double)img.Width / img.Height) * height; //height = const, width varies from image proportions
                if(x + width> ClientRectangle.Width) //determines when will the picture be positioned in the next row
                {
                    x = border; //next row means x=0+border
                    y += height + border; 
                }
                PictureBox picture = new PictureBox
                {
                    Location = new Point(x, y),
                    Name = "picture" + i,
                    Size = new Size((int)width, height),
                    BackgroundImage = img,
                    BackgroundImageLayout = ImageLayout.Zoom,
                };
                x += (int)width + border;//next picture should be border(10) pixels away
                _pics.Add(picture);
                this.Controls.Add(picture);
            }
        }

        /// <summary>
        /// Removes all the picture boxes.
        /// </summary>
        private void deletePictureBoxes()
        {
            for (int i = _pics.Count - 1; i >= 0; i--)
            {
                _pics[i].Dispose();
                this.Controls.Remove(_pics[i]);
                _pics.RemoveAt(i);
            }
        }

        public Form1 getForm()
        {
            return this;
        }

        //re-position and re-size pictures on window size change
        private void Form1_ResizeEnd(object sender, EventArgs e)
        {
            AutoScrollPosition = new Point(0, 0);
            int x = border;
            int y = border;
            foreach (PictureBox pic in _pics)
            {
                Image img = pic.BackgroundImage;
                int height = System.Windows.Forms.Screen.PrimaryScreen.Bounds.Height / 5; //smallest picture displayed should be quarter of the screen's height
                if (this.ClientRectangle.Height / 4 > height)
                    height = this.ClientRectangle.Height / 4;
                double width = ((double)img.Width / img.Height) * height; //height = const, width varies from image proportions
                if (x + width> ClientRectangle.Width) //determines when will the picture be positioned in the next row
                {
                    x = border;
                    y += height + border;
                }
                pic.Size = new Size((int)width, height);
                pic.Location = new Point(x, y);
                x += (int)width + border;
            }
        }
    }
}