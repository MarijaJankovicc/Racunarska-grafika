// -----------------------------------------------------------------------
// <file>World.cs</file>
// <copyright>Grupa za Grafiku, Interakciju i Multimediju 2013.</copyright>
// <author>Srđan Mihić</author>
// <author>Aleksandar Josić</author>
// <summary>Klasa koja enkapsulira OpenGL programski kod.</summary>
// -----------------------------------------------------------------------
using System;
using Assimp;
using System.IO;
using System.Reflection;
using SharpGL.SceneGraph;
using SharpGL.SceneGraph.Primitives;
using SharpGL.SceneGraph.Quadrics;
using SharpGL.SceneGraph.Core;
using SharpGL;
using System.Windows.Threading;
using System.Windows.Media;
using System.Drawing;
using System.Drawing.Imaging;

namespace AssimpSample
{
    public enum Color
    {
        White,
        Red,
        Blue,
        Green,
        Yellow,
        Orange

    };


    /// <summary>
    ///  Klasa enkapsulira OpenGL kod i omogucava njegovo iscrtavanje i azuriranje.
    /// </summary>
    public class World : IDisposable
    {
        #region Atributi
        //visina stuba
        private float[] light0pos; //tackasni izvor bele boje
        private Color lightSourceColor = Color.White;

        /// <summary>
        ///	 Ugao rotacije Meseca
        /// </summary>
        private float m_moonRotation = 0.0f;

        /// <summary>
        ///	 Ugao rotacije Zemlje
        /// </summary>
        private float m_earthRotation = 0.0f;

        /// <summary>
        ///	 Scena koja se prikazuje.
        /// </summary>
        private AssimpScene m_scene;
        private AssimpScene m_scene2;

        /// <summary>
        ///	 Ugao rotacije sveta oko X ose.
        /// </summary>
        private float m_xRotation = 20.0f;

        /// <summary>
        ///	 Ugao rotacije sveta oko Y ose.
        /// </summary>
        private float m_yRotation = 0.0f;

        /// <summary>
        ///	 Udaljenost scene od kamere.
        /// </summary>
        private float m_sceneDistance = 2000.0f;
      

        /// <summary>
        ///	 Sirina OpenGL kontrole u pikselima.
        /// </summary>
        private int m_width;

        /// <summary>
        ///	 Visina OpenGL kontrole u pikselima.
        /// </summary>
        private int m_height;

        //SKALIRANJE BANDERE
        private double scaling = 10f;

        //ANIMACIJA
        private float speed = 250f;
        public float mX = 0f, mY = 0f, mZ = 0f, mR = 0f;
        private bool animation;
        private DispatcherTimer timer;

        //Skaliranje motora
        private float scaleMotor = 2f;

        // TEKSTURE
        private enum TextureObjects { Concrete = 0, Brick, Wood, Lamp, Moto, Semaphore};
        private readonly int m_textureCount = Enum.GetNames(typeof(TextureObjects)).Length;
        private uint[] m_textures = null;

        private string[] m_textureFiles = { "..//..//Textures//concrete.jpg", "..//..//Textures//brick.jfif", "..//..//Textures//tree.jpg", "..//..//Textures//lamp.jfif", "..//..//Textures//dark.jpg", "..//..//Textures//green.jpg" };



        #endregion Atributi

        #region Properties

        /// <summary>
        ///	 Scena koja se prikazuje.
        /// </summary>
        public AssimpScene Scene
        {
            get { return m_scene; }
            set { m_scene = value; }
        }
      public AssimpScene Scene2
        {
            get { return m_scene2; }
            set { m_scene2 = value; }
        }


        /// <summary>
        ///	 Ugao rotacije sveta oko X ose.
        /// </summary>
        public float RotationX
        {
            get { return m_xRotation; }
            set
            {
                m_xRotation = value;
                if (m_xRotation >= 90)
                {
                    m_xRotation = 90;
                }
                if (m_xRotation <= 1)
                {
                    m_xRotation = 1;
                }

            }
        }

        /// <summary>
        ///	 Ugao rotacije sveta oko Y ose.
        /// </summary>
        public float RotationY
        {
            get { return m_yRotation; }
            set { m_yRotation = value; }
        }

        /// <summary>
        ///	 Udaljenost scene od kamere.
        /// </summary>
        public float SceneDistance
        {
            get { return m_sceneDistance; }
            set { m_sceneDistance = value; }
        }

        /// <summary>
        ///	 Sirina OpenGL kontrole u pikselima.
        /// </summary>
        public int Width
        {
            get { return m_width; }
            set { m_width = value; }
        }


        /// <summary>
        ///	 Visina OpenGL kontrole u pikselima.
        /// </summary>
        public int Height
        {
            get { return m_height; }
            set { m_height = value; }
        }
        public double Scaling
        {
            get { return scaling; }
            set { scaling = value; }
        }

        public float ScaleMotor
        {
            get { return scaleMotor; }
            set { scaleMotor = value; }
        }
        public float Speed
        {
            get { return speed; }
            set { speed = value; }
        }
        public bool Animation
        {
            get { return animation; }
            set { animation = value; }
        }
        private DispatcherTimer Timer
        {
            get { return timer; }
            set { timer = value; }
        }
        public Color SelectedColor
        {
            get { return lightSourceColor; }
            set { lightSourceColor = value; }
        }


        #endregion Properties

        #region Konstruktori

        /// <summary>
        ///  Konstruktor klase World.
        /// </summary>
        public World(String scenePath, String sceneFileName, String sceneFileName2, int width, int height, OpenGL gl)
        {
            m_textures = new uint[m_textureCount];
            this.m_scene = new AssimpScene(scenePath, sceneFileName, gl);
            this.m_scene2 = new AssimpScene(scenePath, sceneFileName2, gl);
            this.m_width = width;
            this.m_height = height;
        }

        /// <summary>
        ///  Destruktor klase World.
        /// </summary>
        ~World()
        {
            this.Dispose(false);
        }

        #endregion Konstruktori

        #region Metode

        /// <summary>
        ///  Korisnicka inicijalizacija i podesavanje OpenGL parametara.
        /// </summary>
        public void Initialize(OpenGL gl)
        {
            // Model sencenja na flat (konstantno)
            gl.ShadeModel(OpenGL.GL_FLAT);
            gl.Enable(OpenGL.GL_DEPTH_TEST);
            gl.Enable(OpenGL.GL_CULL_FACE);

            gl.Enable(OpenGL.GL_COLOR_MATERIAL); // STAVKA 1
            gl.ColorMaterial(OpenGL.GL_FRONT, OpenGL.GL_AMBIENT_AND_DIFFUSE);

            //STAVKA 2
            SetLights(gl);
            gl.ClearColor(0.0f, 0.0f, 0.0f, 1.0f);

            //STAVKA 3
            LoadTextures(gl);

            m_scene.LoadScene();
            m_scene.Initialize();
            m_scene2.LoadScene();
            m_scene2.Initialize();
        }

        /// <summary>
        ///  Iscrtavanje OpenGL kontrole.
        /// </summary>
        public void Draw(OpenGL gl)
        {
            float[] lightColorComponent = { 1.0f, 1.0f, 1.0f, 1.0f }; //white default
            switch (SelectedColor)
            {
                case Color.Yellow:
                    lightColorComponent = new float[] { 1.0f, 1.0f, 0.4f, 1.0f };
                    break;
                case Color.Red:
                    lightColorComponent = new float[] { 1.0f, 0.0f, 0.0f, 1.0f };
                    break;
                case Color.Green:
                    lightColorComponent = new float[] { 0.0f, 1.0f, 0.0f, 1.0f };
                    break;
                case Color.Blue:
                    lightColorComponent = new float[] { 0.0f, 0.0f, 1.0f, 1.0f };
                    break;
                case Color.White:
                    lightColorComponent = new float[] { 1.0f, 1.0f, 1.0f, 1.0f };
                    break;
                case Color.Orange:
                    lightColorComponent = new float[] { 1.0f, 0.65f, 0.0f, 1.0f };
                    break;
            }

            gl.Light(OpenGL.GL_LIGHT0, OpenGL.GL_AMBIENT, lightColorComponent);
            gl.Light(OpenGL.GL_LIGHT0, OpenGL.GL_DIFFUSE, lightColorComponent);

            gl.Clear(OpenGL.GL_COLOR_BUFFER_BIT | OpenGL.GL_DEPTH_BUFFER_BIT);

            gl.MatrixMode(OpenGL.GL_PROJECTION);
            gl.LoadIdentity();
            gl.Perspective(60f, (double)m_width / m_height, 0.5f, 20000f);

            gl.MatrixMode(OpenGL.GL_MODELVIEW);
            gl.FrontFace(OpenGL.GL_CCW);

            //STAVKA 2
            light0pos = new float[] { 150.0f, 300.0f, 0.0f, 1.0f };
            gl.Light(OpenGL.GL_LIGHT0, OpenGL.GL_POSITION, light0pos); // Neosetljiv na transformacije

            gl.PushMatrix();

            gl.LookAt(0, 800, m_sceneDistance,      //STAVKA 6
                     0, 600, 0,
                     0, 1, 0);

            gl.Translate(0.0f, -0.0f, -m_sceneDistance);
            gl.Rotate(m_xRotation, 1.0f, 0.0f, 0.0f);
            gl.Rotate(m_yRotation, 0.0f, 1.0f, 0.0f);

            #region Motor
            //MOTOR
            gl.PushMatrix();
            gl.Translate(mX, mY, mZ);
            gl.Rotate(0, mR, 0);
            gl.Scale(scaleMotor, scaleMotor, scaleMotor);
            gl.BindTexture(OpenGL.GL_TEXTURE_2D, m_textures[(int)TextureObjects.Moto]);
            gl.Enable(OpenGL.GL_TEXTURE_GEN_S);
            gl.Enable(OpenGL.GL_TEXTURE_GEN_T);
            gl.TexEnv(OpenGL.GL_TEXTURE_ENV, OpenGL.GL_TEXTURE_ENV_MODE, OpenGL.GL_MODULATE);
            m_scene.Draw();
            gl.Disable(OpenGL.GL_TEXTURE_GEN_S);
            gl.Disable(OpenGL.GL_TEXTURE_GEN_T);
            gl.PopMatrix();
            #endregion
        
            #region Semafor
            //SEMAFOR
            gl.PushMatrix();
            gl.Translate(-800f, -0.5f, -550f);
            gl.Scale(50, 70, 50);
            gl.Color(0f, 1f, 0f, 1.0f);
            gl.BindTexture(OpenGL.GL_TEXTURE_2D, m_textures[(int)TextureObjects.Semaphore]);
            gl.Enable(OpenGL.GL_TEXTURE_GEN_S);
            gl.Enable(OpenGL.GL_TEXTURE_GEN_T);
            gl.TexEnv(OpenGL.GL_TEXTURE_ENV, OpenGL.GL_TEXTURE_ENV_MODE, OpenGL.GL_MODULATE);
            m_scene2.Draw();
            gl.Disable(OpenGL.GL_TEXTURE_GEN_S);
            gl.Disable(OpenGL.GL_TEXTURE_GEN_T);
            gl.PopMatrix();
            #endregion

            #region Podloga
            //PODLOGA
              gl.PushMatrix();
              gl.BindTexture(OpenGL.GL_TEXTURE_2D, m_textures[(int)TextureObjects.Concrete]);
              gl.MatrixMode(OpenGL.GL_TEXTURE);            
              gl.Scale(1,1,1);
              gl.PushMatrix();

              gl.MatrixMode(OpenGL.GL_MODELVIEW);
              gl.Translate(0f, 1f, 0f);
              gl.Scale(70f, 1.0f, 110f);
              gl.Begin(OpenGL.GL_QUADS);
                  gl.Normal(0f, 1f, 0f);
                  gl.TexCoord(1.0f, 0.0f);
                  gl.Vertex(30, 0, 20);
                  gl.TexCoord(1.0f, 1.0f);
                  gl.Vertex(30, 0, -40);
                  gl.TexCoord(0.0f, 1.0f);
                  gl.Vertex(-30, 0, -40);
                  gl.TexCoord(0.0f, 0.0f);
                  gl.Vertex(-30f, 0, 20);
              gl.End();
              gl.PopMatrix();
              gl.MatrixMode(OpenGL.GL_TEXTURE);
              gl.PopMatrix();
              gl.MatrixMode(OpenGL.GL_MODELVIEW);
             #endregion

            #region Zid levi
            //LEVI ZID
            /*
            Cube wall = new Cube();
            gl.PushMatrix();
            gl.Translate(-1600f, 0f, -770f);
            gl.Color(1f, 0f, 0f);
            gl.Scale(280, 750f, 2000);
            gl.Translate(0f, 1f, 0f);
            gl.BindTexture(OpenGL.GL_TEXTURE_2D, m_textures[(int)TextureObjects.Brick]);
            wall.Render(gl, RenderMode.Render);
            gl.PopMatrix();          
            */

            // gl.MatrixMode(OpenGL.GL_TEXTURE);
            // gl.LoadIdentity();
            // gl.Scale(1f, 1f, 1f);  

            gl.MatrixMode(OpenGL.GL_MODELVIEW);         
            Cube wall = new Cube();
            gl.PushMatrix();
            gl.Translate(-1600f, 0f, -770f);
            //gl.Color(1f, 0f, 0f);
            gl.Scale(280, 750f, 2000);
            gl.Translate(0f, 1f, 0f);
            gl.BindTexture(OpenGL.GL_TEXTURE_2D, m_textures[(int)TextureObjects.Brick]);
            wall.Render(gl, RenderMode.Render);
            gl.PopMatrix();
            #endregion

            #region Zid desni
            //DESNI ZID
            gl.PushMatrix();
            gl.Translate(1600f, 0f, -770f);
            gl.Scale(280, 750f, 2000);
            gl.Translate(0f, 1f, 0f);
            gl.BindTexture(OpenGL.GL_TEXTURE_2D, m_textures[(int)TextureObjects.Brick]);
            wall.Render(gl, RenderMode.Render);
            gl.PopMatrix();
            #endregion

            #region Bandera drska-desna
            gl.PushMatrix();
            gl.Scale(10f, Scaling, 10f);
            gl.Translate(80.0f, 1f, -200.0f);
            gl.Rotate(-90, 1.0f, 0.0f, 0.0f);
            gl.Color(0.8f, 0.3f, 0f);
            Cylinder cil1 = new Cylinder();
            cil1.BaseRadius = 1.5;
            cil1.TopRadius = 1.5;
            cil1.Height = 80;
            cil1.QuadricDrawStyle = DrawStyle.Fill;
            cil1.CreateInContext(gl);
            gl.BindTexture(OpenGL.GL_TEXTURE_2D, m_textures[(int)TextureObjects.Wood]);
            cil1.TextureCoords = true;
            cil1.Render(gl, SharpGL.SceneGraph.Core.RenderMode.Render);
            gl.PopMatrix();
            #endregion

            #region Bandera drska-leva
            //BANDERA2
            gl.PushMatrix();
            gl.Scale(10f, 10f, 10f);
            gl.Translate(-80.0f, 1f, -200.0f);
            gl.Rotate(-90, 1.0f, 0.0f, 0.0f);
            gl.Color(0.8f, 0.3f, 0f);
            Cylinder cil2 = new Cylinder();
            cil2.TopRadius = 1.5;
            cil2.Height = 80;
            cil2.QuadricDrawStyle = DrawStyle.Fill;
            cil2.BaseRadius = 1.5;
            cil2.CreateInContext(gl);
            gl.BindTexture(OpenGL.GL_TEXTURE_2D, m_textures[(int)TextureObjects.Wood]);
            cil2.TextureCoords = true;
            cil2.Render(gl, SharpGL.SceneGraph.Core.RenderMode.Render);
            gl.PopMatrix();
            #endregion

            #region Bandera svetiljka-desna
            //SVETLO1
            gl.PushMatrix();
            gl.Color(1, 1f, 0f);
            gl.Translate(800.0f, Scaling * 0.1 * 780f, -1950.0f);
            gl.Scale(50f, 50f, 50f);
            Cube c1 = new Cube();
            gl.BindTexture(OpenGL.GL_TEXTURE_2D, m_textures[(int)TextureObjects.Lamp]);
            c1.Render(gl, RenderMode.Render);
            gl.PopMatrix();
            #endregion

            #region Bandera svetiljka-leva
            //SVETLO2
            gl.PushMatrix();
            gl.Color(1, 1f, 0f);
            gl.Translate(-800f, 780f, -1950.0f);
            gl.Scale(50f, 50f, 50f);
            Cube c2 = new Cube();
            gl.BindTexture(OpenGL.GL_TEXTURE_2D, m_textures[(int)TextureObjects.Lamp]);
            c2.Render(gl, RenderMode.Render);
            gl.PopMatrix();
            #endregion

            #region Text
            gl.PushMatrix();
                gl.MatrixMode(OpenGL.GL_PROJECTION);
                gl.LoadIdentity();
                gl.Ortho2D(-20, 5, -4, 8);

                gl.MatrixMode(OpenGL.GL_MODELVIEW);
                gl.LoadIdentity();
                gl.Color(0f, 0f, 1f);
                gl.Scale(0.5f, 0.5f, 0.5f);
               
                    gl.PushMatrix();
                    gl.Translate(-2.5f, -3f, 0f);
                    gl.DrawText3D("Verdana", 10f, 1f, 1f, "Predmet: Racunarska grafika");
                    gl.PopMatrix();

                    gl.PushMatrix();
                    gl.Translate(-2.5f, -3.1f, 0f);
                    gl.DrawText3D("Verdana", 10f, 1f, 0.1f, "_______________________");
                    gl.PopMatrix();


                    gl.PushMatrix();
                    gl.Translate(-2.5f, -4f, 0f);
                    gl.DrawText3D("Verdana", 10f, 1f, 0.1f, "Sk.god: 2021/22.");
                    gl.PopMatrix();

                    gl.PushMatrix();
                    gl.Translate(-2.5f, -4.1f, 0f);
                    gl.DrawText3D("Verdana", 10f, 1f, 0.1f, "_______________________");
                    gl.PopMatrix();


                    gl.PushMatrix();
                    gl.Translate(-2.5f, -5f, 0f);
                    gl.DrawText3D("Verdana", 10f, 1f, 0.1f, "Ime: Marija");
                    gl.PopMatrix();

                    gl.PushMatrix();
                    gl.Translate(-2.5f, -5.1f, 0f);
                    gl.DrawText3D("Verdana", 10f, 1f, 0.1f, "_______________________");
                    gl.PopMatrix();

                    gl.PushMatrix();
                    gl.Translate(-2.5f, -6f, 0f);
                    gl.DrawText3D("Verdana", 10f, 1f, 0.1f, "Prezime: Jankovic");
                    gl.PopMatrix();

                    gl.PushMatrix();
                    gl.Translate(-2.5f, -6.1f, 0f);
                    gl.DrawText3D("Verdana", 10f, 1f, 0.1f, "_______________________");
                    gl.PopMatrix();

                    gl.PushMatrix();
                    gl.Translate(-2.5f, -7f, 0f);
                    gl.DrawText3D("Verdana", 10f, 1f, 0.1f, "Sifra zad: 11.2");
                    gl.PopMatrix();

                    gl.PushMatrix();
                    gl.Translate(-2.5f, -7.1f, 0f);
                    gl.DrawText3D("Verdana", 10f, 1f, 0.1f, "_______________________");
                    gl.PopMatrix();
         
            gl.PopMatrix();
            #endregion

            gl.PopMatrix();
            // Oznaci kraj iscrtavanja
            gl.Flush();
        }

        private void SetLights(OpenGL gl)
        {
            light0pos = new float[] { 150.0f, 300.0f,0, 1.0f, 1f };
            float[] light0ambient = new float[] { 1.0f, 1.0f, 1.0f, 1.0f }; // belo svetlo 
            float[] light0diffuse = new float[] { 1.0f, 1.0f, 1.0f, 1.0f };

            gl.Light(OpenGL.GL_LIGHT0, OpenGL.GL_POSITION, light0pos);
            gl.Light(OpenGL.GL_LIGHT0, OpenGL.GL_AMBIENT, light0ambient);
            gl.Light(OpenGL.GL_LIGHT0, OpenGL.GL_DIFFUSE, light0diffuse);
            gl.Light(OpenGL.GL_LIGHT0, OpenGL.GL_SPOT_CUTOFF, 180.0f);

          

            float[] light1direction = new float[] {0f, 0f, 1f};
            float[] light1pos = new float[] { -800f, 780f, -1950.0f, 1f };
            float[] light1ambient = new float[] { 1.0f, 1.0f, 0.4f, 1.0f };
            float[] light1diffuse = new float[] { 1.0f, 1.0f, 0.4f, 1.0f };

            gl.Light(OpenGL.GL_LIGHT1, OpenGL.GL_POSITION, light1pos);
            gl.Light(OpenGL.GL_LIGHT1, OpenGL.GL_AMBIENT, light1ambient);
            gl.Light(OpenGL.GL_LIGHT1, OpenGL.GL_DIFFUSE, light1diffuse);
            gl.Light(OpenGL.GL_LIGHT1, OpenGL.GL_SPOT_CUTOFF, 40.0f);
            gl.Light(OpenGL.GL_LIGHT1, OpenGL.GL_SPOT_DIRECTION, light1direction);
  
            gl.Enable(OpenGL.GL_LIGHTING); 
            gl.Enable(OpenGL.GL_LIGHT0);
            gl.Enable(OpenGL.GL_LIGHT1);

            gl.Enable(OpenGL.GL_NORMALIZE); 

        }

        private void LoadTextures(OpenGL gl)
        {
            gl.Enable(OpenGL.GL_TEXTURE_2D);
            gl.GenTextures(m_textureCount, m_textures);
            for (int i = 0; i < m_textureCount; ++i)
            {

                gl.BindTexture(OpenGL.GL_TEXTURE_2D, m_textures[i]);
                Bitmap image = new Bitmap(m_textureFiles[i]);
                image.RotateFlip(RotateFlipType.RotateNoneFlipY);
                Rectangle rect = new Rectangle(0, 0, image.Width, image.Height);
                BitmapData imageData = image.LockBits(rect, ImageLockMode.ReadOnly,
                                                      System.Drawing.Imaging.PixelFormat.Format32bppArgb);

                gl.Build2DMipmaps(OpenGL.GL_TEXTURE_2D, (int)OpenGL.GL_RGBA8, image.Width, image.Height, OpenGL.GL_BGRA, OpenGL.GL_UNSIGNED_BYTE, imageData.Scan0);

                //Stavka 3
                gl.TexParameter(OpenGL.GL_TEXTURE_2D, OpenGL.GL_TEXTURE_MIN_FILTER, OpenGL.GL_NEAREST);
                gl.TexParameter(OpenGL.GL_TEXTURE_2D, OpenGL.GL_TEXTURE_MAG_FILTER, OpenGL.GL_NEAREST);
                gl.TexParameter(OpenGL.GL_TEXTURE_2D, OpenGL.GL_TEXTURE_WRAP_S, OpenGL.GL_REPEAT); 
                gl.TexParameter(OpenGL.GL_TEXTURE_2D, OpenGL.GL_TEXTURE_WRAP_T, OpenGL.GL_REPEAT);  
                gl.TexEnv(OpenGL.GL_TEXTURE_ENV, OpenGL.GL_TEXTURE_ENV_MODE, OpenGL.GL_ADD); // Stapanja teksture sa materijalom

                image.UnlockBits(imageData);
                image.Dispose();
            }

        }

        public void Drive()
        {
            mX = 0f;
            mY = 0f;
            mZ = 0f;
            animation = true;
            timer = new DispatcherTimer();
            timer.Interval = TimeSpan.FromMilliseconds(25);
            timer.Tick += new EventHandler(Move);
            timer.Start();

        }


        public void Move(object sender, EventArgs e)
        {
            if (mZ > -3300)
            {
                mZ = mZ - Speed;
            }
            else if (mR < 90)
            {
                mR = mR + 10;
            }
            else if (mX > -2500)
            {
                mX = mX - Speed;
            }
            else
            {

                mX = 0f;
                mZ = 0f;
                mY = 0f;
                mR = 0f;

                animation = false;
                Timer.Stop();

            }
        }


        /// <summary>
        /// Podesava viewport i projekciju za OpenGL kontrolu.
        /// </summary>
        public void Resize(OpenGL gl, int width, int height)
        {
            m_width = width;
            m_height = height;

            gl.Viewport(0, 0, m_width, m_height);                     
            gl.MatrixMode(OpenGL.GL_PROJECTION);      // selektuj Projection Matrix
            gl.LoadIdentity();
            gl.Perspective(60f, (double)width / height, 0.5f, 20000f);
            gl.MatrixMode(OpenGL.GL_MODELVIEW);
            gl.LoadIdentity();                // resetuj ModelView Matrix
        }

        /// <summary>
        ///  Implementacija IDisposable interfejsa.
        /// </summary>
        protected virtual void Dispose(bool disposing)
        {
            if (disposing)
            {
                m_scene.Dispose();
            }
        }

        #endregion Metode

        #region IDisposable metode

        /// <summary>
        ///  Dispose metoda.
        /// </summary>
        public void Dispose()
        {
            this.Dispose(true);
            GC.SuppressFinalize(this);
        }

        #endregion IDisposable metode
    }
}
