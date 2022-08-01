using System;
using System.IO;
using System.Reflection;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Data;
using System.Windows.Documents;
using System.Windows.Input;
using System.Windows.Media;
using System.Windows.Media.Imaging;
using System.Windows.Navigation;
using SharpGL.SceneGraph;
using SharpGL;
using Microsoft.Win32;
using System.Text.RegularExpressions;

namespace AssimpSample
{
    /// <summary>
    /// Interaction logic for MainWindow.xaml
    /// </summary>
    public partial class MainWindow : Window
    {
        #region Atributi

        /// <summary>
        ///	 Instanca OpenGL "sveta" - klase koja je zaduzena za iscrtavanje koriscenjem OpenGL-a.
        /// </summary>
        World m_world = null;

        #endregion Atributi

        #region Konstruktori

        public MainWindow()
        {
            // Inicijalizacija komponenti
            InitializeComponent();

            // Kreiranje OpenGL sveta
            try
            {
                m_world = new World(Path.Combine(Path.GetDirectoryName(Assembly.GetExecutingAssembly().Location), "3D Models\\Models-KT1"), "motor.3DS", "trafficlight.obj", (int)openGLControl.ActualWidth, (int)openGLControl.ActualHeight, openGLControl.OpenGL);
            }
            catch (Exception e)
            {
                MessageBox.Show("Neuspesno kreirana instanca OpenGL sveta. Poruka greške: " + e.Message, "Poruka", MessageBoxButton.OK);
                this.Close();
            }
        }

        #endregion Konstruktori

        /// <summary>
        /// Handles the OpenGLDraw event of the openGLControl1 control.
        /// </summary>
        /// <param name="sender">The source of the event.</param>
        /// <param name="args">The <see cref="SharpGL.SceneGraph.OpenGLEventArgs"/> instance containing the event data.</param>
        private void openGLControl_OpenGLDraw(object sender, OpenGLEventArgs args)
        {
            m_world.Draw(args.OpenGL);
        }

        /// <summary>
        /// Handles the OpenGLInitialized event of the openGLControl1 control.
        /// </summary>
        /// <param name="sender">The source of the event.</param>
        /// <param name="args">The <see cref="SharpGL.SceneGraph.OpenGLEventArgs"/> instance containing the event data.</param>
        private void openGLControl_OpenGLInitialized(object sender, OpenGLEventArgs args)
        {
            m_world.Initialize(args.OpenGL);
        }

        /// <summary>
        /// Handles the Resized event of the openGLControl1 control.
        /// </summary>
        /// <param name="sender">The source of the event.</param>
        /// <param name="args">The <see cref="SharpGL.SceneGraph.OpenGLEventArgs"/> instance containing the event data.</param>
        private void openGLControl_Resized(object sender, OpenGLEventArgs args)
        {
            m_world.Resize(args.OpenGL, (int)openGLControl.ActualWidth, (int)openGLControl.ActualHeight);
        }

        private void Window_KeyDown(object sender, KeyEventArgs e)
        {
            if (!m_world.Animation)
            {
                switch (e.Key)
                {
                    case Key.F4:
                        if (!m_world.Animation)
                            this.Close(); break;               // izlazi iz aplikacije
                    case Key.E:
                        if (!m_world.Animation)
                            m_world.RotationX -= 5.0f; break;   //oko horizontale
                    case Key.D:
                        if (!m_world.Animation)
                            m_world.RotationX += 5.0f; break;   //oko horizontale
                    case Key.S:
                        if (!m_world.Animation)
                            m_world.RotationY -= 5.0f; break;   //oko vertikale
                    case Key.F:
                        if (!m_world.Animation)
                            m_world.RotationY += 5.0f; break;   //oko vertikale
                    case Key.Add:
                        if (!m_world.Animation)
                            m_world.SceneDistance -= 700.0f; break;
                    case Key.Subtract:
                        if (!m_world.Animation)
                            m_world.SceneDistance += 700.0f; break;
                    case Key.V:
                        {
                            if (m_world.Animation == false)
                            {
                                m_world.Drive();
                            }
                            break;
                        }
                }
            }
            /*     case Key.F2:
                     OpenFileDialog opfModel = new OpenFileDialog();
                     bool result = (bool) opfModel.ShowDialog();
                     if (result)
                     {

                         try
                         {
                             World newWorld = new World(Directory.GetParent(opfModel.FileName).ToString(), Path.GetFileName(opfModel.FileName), Path.GetFileName(opfModel.FileName), (int)openGLControl.Width, (int)openGLControl.Height, openGLControl.OpenGL);
                             m_world.Dispose();
                             m_world = newWorld;
                             m_world.Initialize(openGLControl.OpenGL);
                         }
                         catch (Exception exp)
                         {
                             MessageBox.Show("Neuspesno kreirana instanca OpenGL sveta:\n" + exp.Message, "GRESKA", MessageBoxButton.OK );
                         }
                     }
                     break;
            */

        }
        private void lightSourceComboBox_SelectionChanged(object sender, SelectionChangedEventArgs e)
        {
            if (!m_world.Animation)
            {
                m_world.SelectedColor = (Color)lightSourceComboBox.SelectedItem;
            }
        }
        private void Window_Loaded_1(object sender, RoutedEventArgs e)
        {
            lightSourceComboBox.ItemsSource = Enum.GetValues(typeof(Color));
        }
        private void scalingTextBox_TextChanged(object sender, RoutedEventArgs e)
        {
            if (!m_world.Animation)
            {
                string s = Regex.Replace(((TextBox)sender).Text, @"[^\d.]", "");
                ((TextBox)sender).Text = s;
                double result;
                if (s.Equals(""))
                {
                    m_world.Scaling = 10;
                }
                else if (double.TryParse(s, out result))
                {
                    m_world.Scaling = result;
                }
            }
        }


        private void slider3_ValueChanged(object sender, RoutedPropertyChangedEventArgs<double> e)
        {
            if (m_world != null && !m_world.Animation)
                m_world.ScaleMotor = (float)e.NewValue;
        }

    }
}
