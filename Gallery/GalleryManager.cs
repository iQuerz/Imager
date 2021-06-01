using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Drawing;
using System.IO;

namespace Gallery
{
    class GalleryManager
    {
        private List<Image> _pics;
        private DirectoryInfo _source;
        private Form1 _form;    //saving the form so I can use Form1.getForm();
        private string[] _types = { "jpg", "JPG", "jpeg", "JPEG", "png", "PNG", "bmp", "BMP", "gif", "GIF", "tiff", "TIFF" };

        public GalleryManager(Form1 form) //if gallery_data has no data about directory path, call this one
        {
            _form = form;
            _pics = new List<Image>();
        }
        public GalleryManager(Form1 form, string path) //
        {
            _form = form;
            _pics = new List<Image>();
            _source = new DirectoryInfo(path);
        }

        /// <summary>
        /// Add a new Image to the pictures list
        /// </summary>
        /// <param name="pic"></param>
        public void addPic(Image pic)
        {
            _pics.Add(NormalizeOrientation(pic));
        }

        /// <summary>
        /// Updates the Folder Source with new folder path.
        /// This method requires flush() and load() if you want to update the pictures.
        /// </summary>
        /// <param name="path"></param>
        public void updateSource(string path)
        {
            _source = new DirectoryInfo(path);
            BinaryWriter updater = new BinaryWriter(File.Open("source.dat", FileMode.Create));
            updater.Write(path);
            updater.Close();
        }

        /// <summary>
        /// Clears the pictures list.
        /// </summary>
        public void flush()
        {
            _pics.Clear();
        }

        /// <summary>
        /// Loads the pictures list from the Folder Source
        /// </summary>
        public void load()
        {
            if (_source == null) throw new Exception("No directory selected, go to \"File->Folder Source\" to choose a root directory.");
                searchDirs(_source);
        }

        /// <summary>
        /// Recursive search through all subdirectories of the parameter "dir".
        /// Loads the "_pics" list with all image files found inside.
        /// </summary>
        /// <param name="dir"></param>
        private void searchDirs(DirectoryInfo dir)
        {
            //going through all subdirs
            foreach (DirectoryInfo subdir in dir.GetDirectories())
            {
                searchDirs(subdir);
            }
            //checking if each file is an image, then adding it to _pics list
            foreach (FileInfo file in dir.GetFiles())
            {
                string ext = file.Extension;
                ext = ext.Remove(0, 1); //remove the dot from the start of the extension
                if (allowedType(ext))
                    _pics.Add(Image.FromFile(file.FullName));
            }
        }

        /// <summary>
        /// Returns Image from pictures list at position "index".
        /// Starting value = 0.
        /// </summary>
        /// <param name="index"></param>
        /// <returns></returns>
        public Image this[int index] { get => _pics[index]; }
        
        /// <summary>
        /// Returns the number of elements in the current pictures list.
        /// </summary>
        /// <returns></returns>
        public int Count() { return _pics.Count; }

        /// <summary>
        /// Checks the exif data tag for rotation and returns properly rotated image.
        /// </summary>
        /// <param name="image"></param>
        /// <returns></returns>
        private Image NormalizeOrientation(Image image) //rotate the image properly
        {
            if (Array.IndexOf(image.PropertyIdList, 0x112) > -1) //0x122 = tag ID for rotation
            {
                int orientation;
                orientation = image.GetPropertyItem(0x112).Value[0];
                if (orientation >= 1 && orientation <= 8)
                {
                    switch (orientation)
                    {
                        case 2:
                            image.RotateFlip(RotateFlipType.RotateNoneFlipX);
                            break;
                        case 3:
                            image.RotateFlip(RotateFlipType.Rotate180FlipNone);
                            break;
                        case 4:
                            image.RotateFlip(RotateFlipType.Rotate180FlipX);
                            break;
                        case 5:
                            image.RotateFlip(RotateFlipType.Rotate90FlipX);
                            break;
                        case 6:
                            image.RotateFlip(RotateFlipType.Rotate90FlipNone);
                            break;
                        case 7:
                            image.RotateFlip(RotateFlipType.Rotate270FlipX);
                            break;
                        case 8:
                            image.RotateFlip(RotateFlipType.Rotate270FlipNone);
                            break;
                    }
                    image.RemovePropertyItem(0x112);
                }
            }
            return image;
        }

        /// <summary>
        /// Returns true if string "type" matches one of the allowed image types.
        /// </summary>
        /// <param name="type"></param>
        /// <returns></returns>
        private bool allowedType(string type)
        {
            foreach(string check in _types)
                if (check.Equals(type)) return true;
            return false;
        }
    }
}
