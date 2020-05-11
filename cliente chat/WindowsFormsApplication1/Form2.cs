using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Data;
using System.Drawing;
using System.Linq;
using System.Text;
using System.Windows.Forms;
using System.Net;
using System.Net.Sockets;

namespace WindowsFormsApplication1
{
    public partial class Form2 : Form
    {
        Socket server;
        string IPservidor = "147.83.117.22"; //poner aqui la IP del servidor
        int puertoservidor = 50066; //poner aqui el puerto del servidor
        public Form2()
        {
            InitializeComponent();
        }

        private void button2_Click(object sender, EventArgs e)
        {
            //Creamos un IPEndPoint con el ip del servidor y puerto del servidor 
            //al que deseamos conectarnos
            IPAddress direc = IPAddress.Parse(IPservidor);
            IPEndPoint ipep = new IPEndPoint(direc, puertoservidor);


            //Creamos el socket 
            server = new Socket(AddressFamily.InterNetwork, SocketType.Stream, ProtocolType.Tcp);
            try
            {
                server.Connect(ipep);//Intentamos conectar el socket
            }
            catch (SocketException ex)
            {
                //Si hay excepcion imprimimos error y salimos del programa con return 
                MessageBox.Show("No he podido conectar con el servidor");
                return;
            }
            if (Registrarse.Checked)
            {
                string mensaje = "1/" + nombre.Text + "/" + contraseña.Text;
                // Enviamos al servidor el nombre tecleado
                byte[] msg = System.Text.Encoding.ASCII.GetBytes(mensaje);
                server.Send(msg);

                //Recibimos la respuesta del servidor
                byte[] msg2 = new byte[80];
                server.Receive(msg2);
                mensaje = Encoding.ASCII.GetString(msg2).Split ('\0')[0];
                if (mensaje == "2/NOMBRE EN USO")
                    MessageBox.Show("El usuario " + nombre.Text + " ya existe.");
                else if (mensaje == "2/REGISTRADO OK")
                    MessageBox.Show("Registro realizado correctamente.");
                else if (mensaje == "2/ERROR")
                    MessageBox.Show("Error en el registro.");
            }
            else if (Inicia.Checked)
            {
                string mensaje = "2/" + nombre.Text + "/" + contraseña.Text;
                // Enviamos al servidor el nombre tecleado
                byte[] msg = System.Text.Encoding.ASCII.GetBytes(mensaje);
                server.Send(msg);

                //Recibimos la respuesta del servidor
                byte[] msg2 = new byte[80];
                server.Receive(msg2);

                byte[] msg3 = new byte[80];
                server.Receive(msg3);

                mensaje = Encoding.ASCII.GetString(msg3).Split('\0')[0];
                if (mensaje == "2/ENTRA")
                {
                    Form1 inicio = new Form1(nombre.Text, IPservidor, puertoservidor, server);
                    inicio.ShowDialog();
                    this.Visible = false;
                }
                else if (mensaje == "2/NO ENTRA")
                    MessageBox.Show("Usuario y/o contraseña incorrectos.");
            }
        }


    }
}
