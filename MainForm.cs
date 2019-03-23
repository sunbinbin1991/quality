// Simple Player sample application
// AForge.NET framework
// http://www.aforgenet.com/framework/
//
// Copyright © AForge.NET, 2006-2011
// contacts@aforgenet.com
//

using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Data;
using System.Drawing;
using System.Text;
using System.Windows.Forms;
using System.Diagnostics;
using System.Runtime.InteropServices;
using AForge.Video;
using AForge.Video.DirectShow;
using Uface;
using System.Drawing.Imaging;
using System.Drawing.Drawing2D;
using AForge.Imaging.Formats;
using System.IO;
namespace Player
{
    public partial class MainForm : Form
    {
        //init picture box setting
        private bool isSave = false;
        private bool isRecursive = false;
        private int clickCount;
        private string baseFolder;
        private int baseFolderLevel;
        UImage uimage_pic = new UImage();
        private JudgeResult jr = new JudgeResult();
        private Dictionary<string, long> fileList = new Dictionary<string, long> { };
        System.IO.StreamWriter file_sucess;
        System.IO.StreamWriter file_failed;
        //JudgeResult jr = new JudgeResult();

        private Stopwatch stopWatch = null;
        private UImage uimage_1 = new UImage();
        private QualityJudge judge = new QualityJudge(48);   
        private Bitmap tmpBitmap = null;
        


        // Class constructor
        public MainForm( )
        {
            InitializeComponent( );
            initGridView();
            //Control.CheckForIllegalCrossThreadCalls = false;
            pictureBox.Visible = false;
            videoSourcePlayer.Visible = true;
            dataGridView1.Visible = false;
            //checkBox1.Visible = false;
            //checkBox2.Visible = false;
            button1.Visible = false;
            button3.Visible = false;
        }

        private void MainForm_FormClosing( object sender, FormClosingEventArgs e )
        {
            CloseCurrentVideoSource( );
        }

        // "Exit" menu item clicked
        private void exitToolStripMenuItem_Click( object sender, EventArgs e )
        {
            this.Close( );
        }

        // Open local video capture device
        private void localVideoCaptureDeviceToolStripMenuItem_Click( object sender, EventArgs e )
        {
            VideoCaptureDeviceForm form = new VideoCaptureDeviceForm( );

            if ( form.ShowDialog( this ) == DialogResult.OK )
            {
                // create video source
                VideoCaptureDevice videoSource = form.VideoDevice;

                // open it
                OpenVideoSource( videoSource );
            }
        }

        // Open video file using DirectShow
        private void openVideofileusingDirectShowToolStripMenuItem_Click( object sender, EventArgs e )
        {
            if ( openFileDialog.ShowDialog( ) == DialogResult.OK )
            {
                pictureBox.Visible = false;
                videoSourcePlayer.Visible = true;
                dataGridView1.Visible = false;
                checkBox1.Visible = false;
                checkBox2.Visible = false;
                button1.Visible = true;
                button3.Visible = true;
                // create video source
                FileVideoSource fileSource = new FileVideoSource(openFileDialog.FileName);
                //FileVideoSource fileSource = new FileVideoSource("single.avi");
                // open it
                OpenVideoSource( fileSource );
            }
        }

        // Open JPEG URL
        private void openJPEGURLToolStripMenuItem_Click( object sender, EventArgs e )
        {
            URLForm form = new URLForm( );

            form.Description = "Enter URL of an updating JPEG from a web camera:";
            form.URLs = new string[]
				{
					"http://195.243.185.195/axis-cgi/jpg/image.cgi?camera=1",
				};

            if ( form.ShowDialog( this ) == DialogResult.OK )
            {
                // create video source
                JPEGStream jpegSource = new JPEGStream( form.URL );

                // open it
                OpenVideoSource( jpegSource );
            }
        }

        // Open MJPEG URL
        private void openMJPEGURLToolStripMenuItem_Click( object sender, EventArgs e )
        {
            URLForm form = new URLForm( );

            form.Description = "Enter URL of an MJPEG video stream:";
            form.URLs = new string[]
				{
					"http://195.243.185.195/axis-cgi/mjpg/video.cgi?camera=4",
					"http://195.243.185.195/axis-cgi/mjpg/video.cgi?camera=3",
				};

            if ( form.ShowDialog( this ) == DialogResult.OK )
            {
                // create video source
                MJPEGStream mjpegSource = new MJPEGStream( form.URL );

                // open it
                OpenVideoSource( mjpegSource );
            }
        }

        // Capture 1st display in the system
        private void capture1stDisplayToolStripMenuItem_Click( object sender, EventArgs e )
        {
            OpenVideoSource( new ScreenCaptureStream( Screen.AllScreens[0].Bounds, 100 ) );
        }

        // Open video source
        private void OpenVideoSource( IVideoSource source )
        {
            // set busy cursor
            this.Cursor = Cursors.WaitCursor;

            // stop current video source
            CloseCurrentVideoSource( );

            // start new video source
            videoSourcePlayer.VideoSource = source;
            videoSourcePlayer.Start( );

            // reset stop watch
            stopWatch = null;

            // start timer
            timer.Start( );

            this.Cursor = Cursors.Default;
        }

        // Close video source if it is running
        private void CloseCurrentVideoSource( )
        {
            if ( videoSourcePlayer.VideoSource != null )
            {
                videoSourcePlayer.SignalToStop( );
                
                // wait ~ 3 seconds
                for ( int i = 0; i < 30; i++ )
                {
                    if ( !videoSourcePlayer.IsRunning )
                        break;
                    System.Threading.Thread.Sleep( 100 );
                }

                if ( videoSourcePlayer.IsRunning )
                {
                    videoSourcePlayer.Stop( );
                }
                freeBuffer(uimage_1);
                videoSourcePlayer.VideoSource = null;
            }
        }

        // New frame received by the player
        delegate void setVisualbe();
        private void videoSourcePlayer_NewFrame( object sender, ref Bitmap image )
        {
            //pictureBox.Visible = false;
            //videoSourcePlayer.Visible = true;
            //dataGridView1.Visible = false;
            //checkBox1.Visible = false;
            //checkBox2.Visible = false;
            //button1.Visible = true;
 
            DateTime now = DateTime.Now;
            Graphics g = Graphics.FromImage( image );
            Bitmap tp = new Bitmap(image);
            tmpBitmap = tp;
            SolidBrush brush = new SolidBrush( Color.Red );
            SolidBrush circle = new SolidBrush(Color.AliceBlue);
            g.DrawString( now.ToString( ), this.Font, brush, new PointF( 5, 5 ) );
            //Pen greenPen = new Pen(Color.Green, 3);
            Pen greenPen = new Pen(Color.CornflowerBlue, 3);
            System.Drawing.Font drawFont = new System.Drawing.Font("Arial", 16);
            g.DrawString( "最佳注册框", drawFont, circle, new PointF(5*image.Width / 12, image.Height / 12) );
            Point point1 = new Point(image.Width/3, image.Height/4);
            Point point2 = new Point(2*image.Width / 3, image.Height / 4);
            Point point3 = new Point(2*image.Width / 3, 3 * image.Height / 4);
            Point point4 = new Point( image.Width / 3, 3 * image.Height / 4);
            Point[] curvePoints = { point1, point2, point3, point4 };
            float tension = 0.8F;
            FillMode aFillMode = FillMode.Alternate;
            g.DrawClosedCurve(greenPen, curvePoints, tension,aFillMode);
            brush.Dispose( );
            g.Dispose( );
        }
        // On timer event - gather statistics
        private void timer_Tick( object sender, EventArgs e )
        {
            IVideoSource videoSource = videoSourcePlayer.VideoSource;

            if ( videoSource != null )
            {
                // get number of frames since the last timer tick
                int framesReceived = videoSource.FramesReceived;

                if ( stopWatch == null )
                {
                    stopWatch = new Stopwatch( );
                    stopWatch.Start( );
                }
                else
                {
                    stopWatch.Stop( );
                    float fps = 1000.0f * framesReceived / stopWatch.ElapsedMilliseconds;
                    fpsLabel.Text = fps.ToString( "F2" ) + " fps";
                    stopWatch.Reset( );
                    stopWatch.Start( );
                }
            }
        }

        private void button1_Click(object sender, EventArgs e)
        {
            DateTime now = DateTime.Now;
            IVideoSource videoSource = videoSourcePlayer.VideoSource;
            if (videoSource != null)
            {
                createBuffer(tmpBitmap, ref uimage_1);
                Bitmap saveImg = new Bitmap(tmpBitmap);
                Bitmap2UImage(saveImg, ref uimage_1);              
                int res = judge.isQualified(ref uimage_1);               
                Console.WriteLine(res);
                if (res == 0)
                {
                    ImageFormat format = ImageFormat.Bmp;
                    string savestr = now.ToString();
                    savestr = savestr.Replace(":", "_");
                    savestr = savestr.Replace(" ", "_");
                    savestr = savestr.Replace("/", "_");
                    Console.WriteLine(savestr);
                    if (!System.IO.Directory.Exists("image"))
                    {
                        System.IO.Directory.CreateDirectory("image");
                    };
                    saveImg.Save("image/" + savestr + ".bmp", format);
                    MessageBox.Show("恭喜你，采集成功了", "采集成功");
                }
                else if(res==-2){
                    MessageBox.Show("采集失败，未检测到人脸", "采集失败");
                }
                else if (res == -3)
                {
                    MessageBox.Show("采集失败，检测到多张人脸", "采集失败");
                }
                else if (res == -4)
                {
                    MessageBox.Show("采集失败，人脸过小", "采集失败");
                }
                else if (res == -5)
                {
                    MessageBox.Show("采集失败，人脸越界", "采集失败");
                }
                else if (res == -6)
                {
                    MessageBox.Show("采集失败，人脸角度不正常", "采集失败");
                }
                else if (res == -7)
                {
                    MessageBox.Show("采集失败，人脸模糊，或光照不均", "采集失败");
                }
                else {
                    MessageBox.Show("采集失败，请您重新采集", "采集失败");
                }                      
                Console.WriteLine(now.ToString());
                if (videoSourcePlayer.IsRunning)
                {
                    int  image = videoSource.FramesReceived;
                    Console.WriteLine(image);
                }
            }
        }
        static void Bitmap2UImage(Bitmap bitmap, ref UImage uimage)
        {
            for (int i = 0; i < bitmap.Height; i++)
                for (int j = 0; j < bitmap.Width; j++)
                {
                    Marshal.WriteByte(uimage.pixels, (i * bitmap.Width + j) * 3 + 0, bitmap.GetPixel(j, i).B);
                    Marshal.WriteByte(uimage.pixels, (i * bitmap.Width + j) * 3 + 1, bitmap.GetPixel(j, i).G);
                    Marshal.WriteByte(uimage.pixels, (i * bitmap.Width + j) * 3 + 2, bitmap.GetPixel(j, i).R);
                }
            //Console.WriteLine(bitmap.GetPixel(0, 0).B);
            //Console.WriteLine(bitmap.GetPixel(0, 0).G);
            //Console.WriteLine(bitmap.GetPixel(0, 0).R);
        }

        static void createBuffer(Bitmap bitmap, ref UImage uimage)
        {
            uimage.Width = bitmap.Width;
            uimage.Height = bitmap.Height;
            uimage.pixels = Marshal.AllocHGlobal(bitmap.Width * bitmap.Height * 3);
        }
        static void freeBuffer(UImage uimage)
        {
            Marshal.FreeHGlobal(uimage.pixels);            
        }

        private void pictureBox_Click(object sender, EventArgs e)
        {

        }
        private int judgeImage(ref Bitmap bitmap)
        {
            DateTime now = DateTime.Now;           
            createBuffer(bitmap, ref uimage_pic);
            Bitmap2UImage(bitmap, ref uimage_pic);
            UImage tempUimage = new UImage();
            tempUimage = uimage_pic;
            int res = judge.isQualified(ref tempUimage);           
            return res;
            
        }
        private void 打开图片ToolStripMenuItem_Click(object sender, EventArgs e)
        {
            CloseCurrentVideoSource();
            videoSourcePlayer.Visible = false;
            button1.Visible = false;
            button3.Visible = false;
            if (openFileDialog.ShowDialog() == DialogResult.OK)
            {
                pictureBox.Visible = true;             
                dataGridView1.Visible = true;
                checkBox1.Visible = true;
                checkBox2.Visible = true;              
                try
                {
                    ImageInfo imageInfo = null;
                    Bitmap bit = ImageDecoder.DecodeFromFile(openFileDialog.FileName, out imageInfo);
                    this.pictureBox.Image = bit;
                    Graphics g = Graphics.FromImage(bit);                           
                    Bitmap tmpBitmap = new Bitmap(Image.FromFile(openFileDialog.FileName));
                    int judgeflag = judgeImage(ref tmpBitmap);                   
                    jr = getJudgeResult(tmpBitmap);
                    Pen greenPen = new Pen(Color.CornflowerBlue, 3);
                    Rectangle rect = new Rectangle(jr.xmin, jr.ymin, jr.xmax- jr.xmin, jr.ymax -jr.ymin);
                    g.DrawRectangle(greenPen, rect);
                    for (int i = 0; i < jr.numpts; i++) {
                        g.FillEllipse(Brushes.Blue, jr.landmark[2 * i], jr.landmark[2 * i + 1], 5, 5);
                    }                 
                    showJudgeResult(ref jr);
                    showJudgeMessageBox(judgeflag);
                }
                catch (NotSupportedException ex)
                {
                    MessageBox.Show("Image format is not supported: " + ex.Message, "Error",
                        MessageBoxButtons.OK, MessageBoxIcon.Error);
                }
                catch (ArgumentException ex)
                {
                    MessageBox.Show("Invalid image: " + ex.Message, "Error",
                        MessageBoxButtons.OK, MessageBoxIcon.Error);
                }
                catch
                {
                    MessageBox.Show("Failed loading the image", "Error",
                        MessageBoxButtons.OK, MessageBoxIcon.Error);
                }
            }
        }
        private void checkBox1_CheckedChanged(object sender, EventArgs e)
        {
            if (checkBox1.Checked == true)
            {
                isSave = true;
            }
            else
            {
                isSave = false;
            }
        }
        private void movefile(string pathfile, int flag)
        {
            string[] spiltString = pathfile.Split('\\');
            int filelevel = pathfile.Split('\\').Length;
            string imgName = spiltString[filelevel - 1];
            string targetPath = "./image/";
            if (flag == 0)
            {
                targetPath += "成功/";
            }
            else
            {
                targetPath += "失败/";
            }
            if (isRecursive)
            {
                for (int j = baseFolderLevel; j < filelevel - 1; j++)
                {
                    targetPath += spiltString[j];
                    targetPath += "/";
                }
            }
            if (!System.IO.Directory.Exists(targetPath))
            {
                System.IO.Directory.CreateDirectory(targetPath);
            }
            System.IO.File.Copy(pathfile, targetPath + imgName, true);
        }
        public static Dictionary<string, long> GetFile(string path, Dictionary<string, long> FileList)
        {
            DirectoryInfo dir = new DirectoryInfo(path);
            FileInfo[] fil = dir.GetFiles();
            DirectoryInfo[] dii = dir.GetDirectories();
            foreach (FileInfo f in fil)
            {
                //int size = Convert.ToInt32(f.Length);
                long size = f.Length;
                String file = f.FullName;
                if (file.EndsWith(".png") || file.EndsWith(".JPG") || file.EndsWith(".jpg") || file.EndsWith(".jpeg") || file.EndsWith(".JPEG") || file.EndsWith(".bmp") || file.EndsWith(".gif"))
                {
                    FileList.Add(f.FullName, size);//添加文件路径到列表中
                }
            }
            //获取子文件夹内的文件列表，递归遍历            
            foreach (DirectoryInfo d in dii)
            {
                GetFile(d.FullName, FileList);
            }
            return FileList;
        }
        private void 打开图片文件夹ToolStripMenuItem_Click(object sender, EventArgs e)
        {
            CloseCurrentVideoSource();
            FolderBrowserDialog dlg = new FolderBrowserDialog();
            if (dlg.ShowDialog() == DialogResult.OK)
            {
                baseFolder = dlg.SelectedPath.ToString();
                file_sucess = new System.IO.StreamWriter(baseFolder + "./result_sucess.txt");
                file_failed = new System.IO.StreamWriter(baseFolder + "./result_failed.txt");
                //file = File.AppendText(baseFolder + "./result.txt");
                baseFolderLevel = baseFolder.Split('\\').Length;
                MessageBox.Show(dlg.SelectedPath.ToString());
                Console.WriteLine("checkbox issave " + isSave);
                Console.WriteLine("checkbox isRecursive " + isRecursive);
                GetFile(dlg.SelectedPath.ToString(), fileList);
                if (fileList.Count <= 0)
                {
                    MessageBox.Show("目录异常，该目录下并没有图片", "目录异常");
                    return;
                }
                ImageInfo imageInfo = null;
                List<string> test = new List<string>(fileList.Keys);
                pictureBox.Image = ImageDecoder.DecodeFromFile(test[0], out imageInfo);
                processForm progress = new processForm();
            
                for (int i = 0; i < test.Count; i++)
                {
                    progress.Show();
                    Console.WriteLine("test.Count=" + i);                 
                    //Image img = Image.FromFile(test[i]);
                    //Image bmp = new Bitmap(img.Width, img.Height, System.Drawing.Imaging.PixelFormat.Format32bppArgb);                  
                    Bitmap tmpBitmap = new Bitmap(Image.FromFile(test[i]));
                    int judgeflag = judgeImage(ref tmpBitmap);
                    if (isSave || isRecursive)
                    {
                        movefile(test[i], judgeflag);
                    }
                    string result = flag2String(judgeflag);
                    if (judgeflag == 0)
                    {
                        file_sucess.WriteLine(test[i] + "\t" + result);
                    }
                    else {
                        file_failed.WriteLine(test[i] + "\t" + result);
                    }               

                    int num = test.Count;
                    int processPercent = progress.Addprogess(num);
                    progress.Text = "任务当前进度 "+Convert.ToString(processPercent) +"%";
                }
                progress.Close();
                file_sucess.Close();
                file_failed.Close();
                fileList.Clear();
                freeBuffer(uimage_pic);
            }
        }
        private JudgeResult getJudgeResult(Bitmap bitmap)
        {
            JudgeResult res = new JudgeResult();
            DateTime now = DateTime.Now;
            UImage uimage_1 = new UImage();
            createBuffer(bitmap, ref uimage_1);
            Bitmap2UImage(bitmap, ref uimage_1);
            res = judge.getQualified(uimage_1);
            freeBuffer(uimage_1);
            return res;
        }
        private void showJudgeResult(ref JudgeResult jr)
        {
            this.dataGridView1.Rows[0].Cells[1].Value = jr.xmax - jr.xmin;
            this.dataGridView1.Rows[1].Cells[1].Value = jr.pitch;
            this.dataGridView1.Rows[2].Cells[1].Value = jr.yaw;
            this.dataGridView1.Rows[3].Cells[1].Value = jr.roll;
            this.dataGridView1.Rows[4].Cells[1].Value = jr.light;
            this.dataGridView1.Rows[5].Cells[1].Value = jr.blur;
            this.dataGridView1.Rows[6].Cells[1].Value = jr.yinyang;
        }
        private void initGridView()
        {
            int index_facesize = this.dataGridView1.Rows.Add();
            int index_pitch = this.dataGridView1.Rows.Add();
            int index_yaw = this.dataGridView1.Rows.Add();
            int index_roll = this.dataGridView1.Rows.Add();
            int index_light = this.dataGridView1.Rows.Add();
            int index_blur = this.dataGridView1.Rows.Add();
            int index_yinyang = this.dataGridView1.Rows.Add();
            this.dataGridView1.Rows[index_facesize].Cells[0].Value = "人脸大小";
            this.dataGridView1.Rows[index_pitch].Cells[0].Value = "Pitch";
            this.dataGridView1.Rows[index_yaw].Cells[0].Value = "Yaw";
            this.dataGridView1.Rows[index_roll].Cells[0].Value = "Roll";
            this.dataGridView1.Rows[index_light].Cells[0].Value = "光照";
            this.dataGridView1.Rows[index_blur].Cells[0].Value = "模糊";
            this.dataGridView1.Rows[index_yinyang].Cells[0].Value = "阴阳脸";
            //DataGridViewRow row = new DataGridViewRow();
            //DataGridViewTextBoxCell textboxcell = new DataGridViewTextBoxCell();
            //DataGridViewTextBoxCell textboxcellscore = new DataGridViewTextBoxCell();
            //DataGridViewTextBoxCell textboxcellscore2 = new DataGridViewTextBoxCell();
            //textboxcell.Value = "pitch";
            //textboxcellscore.Value = "score";
            //row.Cells.Add(textboxcell);
            //row.Cells.Add(textboxcellscore);
            ////row.Cells.Add(textboxcell);
            ////DataGridViewComboBoxCell comboxcell = new DataGridViewComboBoxCell();
            ////row.Cells.Add(comboxcell);
            //dataGridView1.Rows.Add(row);
        }
        private string flag2String(int res)
        {
            string result = null;
            if (res == 0) {
                result = "";
            }
            else if (res == -2)
            {
                result = "未检测到人脸";
            }
            else if (res == -3)
            {
                result = "检测到多张人脸";             
            }
            else if (res == -4)
            {
                result = "人脸过小";            
            }
            else if (res == -5)
            {
                result = "人脸越界";               
            }
            else if (res == -6)
            {
                result = "人脸角度不正常";             
            }
            else if (res == -7)
            {
                result = "人脸模糊，或光照不均";             
            }
            else
            {
                result = "采集失败,其他原因";               
            }
            return result;
        }
        private void button3_Click(object sender, EventArgs e)
        {
            CloseCurrentVideoSource();
        }

        private void showJudgeMessageBox(int res) {
            if (res == 0)
            {              
                MessageBox.Show("恭喜你，这幅图注册成功了", "采集成功");
            }
            else if (res == -2)
            {
                MessageBox.Show("注册失败，未检测到人脸", "注册失败");
            }
            else if (res == -3)
            {
                MessageBox.Show("注册失败，检测到多张人脸", "注册失败");
            }
            else if (res == -4)
            {
                MessageBox.Show("注册失败，人脸过小", "注册失败");
            }
            else if (res == -5)
            {
                MessageBox.Show("注册失败，人脸越界", "注册失败");
            }
            else if (res == -6)
            {
                MessageBox.Show("注册失败，人脸角度不正常", "注册失败");
            }
            else if (res == -7)
            {
                MessageBox.Show("注册失败，人脸模糊，或光照不均", "注册失败");
            }
            else
            {
                MessageBox.Show("注册失败，请您重新采集", "注册失败");
            }
        }

        private void checkBox2_CheckedChanged_1(object sender, EventArgs e)
        {
            if (checkBox2.Checked == true)
            {
                isRecursive = true;
            }
            else
            {
                isRecursive = false;
            }
        }
    }
}
