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
using System.Threading;

namespace WindowsFormsApplication1
{
    public partial class Form1 : Form
    {
        Thread atender;
        Socket server;
        string usuario;
        string IPservidor;
        int puertoservidor;
        bool nuevomensaje = false;

        delegate void DelegadoParaEscribir (string label, string texto, string[] lista);

        public void CambiaString(string label, string texto, string[] lista)
        {
            if (label == "resultado1")
            {
                resultado1.Text = texto;
            }
            if (label == "NConectados")
                NConectados.Text = texto;

            if (label == "listBox1")
            {
                listBox1.Items.Clear();
                for (int i = 0; i < lista.Length; i++)
                {
                    listBox1.Items.Add(lista[i]);
                }
            }
            if (label == "listBox2")
            {
                string emisor = lista[0];
                string txt = lista[1];
                listBox2.Items.Add(emisor + ": " + txt);
                nuevomensaje = true;
                listBox1.SelectedItem = emisor;
            }
        }
        public Form1(string u, string ip, int puerto, Socket srv)
        {
            InitializeComponent();
            usuario = u;
            IPservidor = ip;
            puertoservidor = puerto;
            server = srv;
            //CheckForIllegalCrossThreadCalls = false;
        }


        private void button2_Click(object sender, EventArgs e)
        {
            if (checkganador.Checked)
            {
                string mensaje = "4/" + IDpartida.Text;
                // Enviamos al servidor el nombre tecleado
                byte[] msg = System.Text.Encoding.ASCII.GetBytes(mensaje);
                server.Send(msg);
            }
            else if (checkgoles.Checked)
            {
                string mensaje = "5/" + nombre.Text;
                // Enviamos al servidor el nombre tecleado
                byte[] msg = System.Text.Encoding.ASCII.GetBytes(mensaje);
                server.Send(msg);

            }
            else if (checkpartidas.Checked)
            {
                string mensaje = "3/" + nombre.Text;
                // Enviamos al servidor el nombre tecleado
                byte[] msg = System.Text.Encoding.ASCII.GetBytes(mensaje);
                server.Send(msg);
            }   
        }

        private void button1_Click(object sender, EventArgs e)
        {
            string ms = "0/" + usuario;
            byte[] msg = System.Text.Encoding.ASCII.GetBytes(ms);
            server.Send(msg);
            atender.Abort();
            this.Close();
        }

        private void AtenderServidor()
        {
            DelegadoParaEscribir delegado = new DelegadoParaEscribir(CambiaString);
            while (true)
            {
                //recibimos mensaje del servidor
                byte[] msg2 = new byte[80];
                server.Receive(msg2);
                string[] recibido = Encoding.ASCII.GetString(msg2).Split('/');
                int codigo = Convert.ToInt32(recibido[0]);
                string mensaje = recibido[1].Split('\0')[0];
                switch (codigo)
                {
                    case 3:  //número de partidas ganadas por un jugador
                        if (mensaje == "NO EXISTE")
                            resultado1.Invoke(delegado, new object[] {"resultado1", "El jugador introducido no existe.", null});
                        else if (mensaje == "ERROR")
                            resultado1.Invoke(delegado, new object[] { "resultado1", "No se ha podido realizar la búsqueda en la base de datos.", null });
                        else
                            resultado1.Invoke(delegado, new object[] { "resultado1", "Número de partidas ganadas: " + mensaje, null });
                        break;
                    case 4: //nombre del ganador de una partida concreta
                        if (mensaje == "NO EXISTE")
                            resultado1.Invoke(delegado, new object[] { "resultado1", "La partida introducida no existe.", null });
                        else if (mensaje == "ERROR")
                            resultado1.Invoke(delegado, new object[] { "resultado1", "No se ha podido realizar la búsqueda en la base de datos.", null });
                        else
                        {
                            //MessageBox.Show("Nombre del ganador: " + mensaje);
                            resultado1.Invoke(delegado, new object[] { "resultado1", "Nombre del ganador: " + mensaje, null });
                        }
                        break;
                    case 5: //numero de goles de un jugador en concreto
                        if (mensaje == "NO EXISTE")
                            resultado1.Invoke(delegado, new object[] { "resultado1", "El jugador introducido no existe.", null });
                        else if (mensaje == "ERROR")
                            resultado1.Invoke(delegado, new object[] { "resultado1", "No se ha podido realizar la búsqueda en la base de datos.", null });
                        else
                            resultado1.Invoke(delegado, new object[] { "resultado1", "Número de goles marcados: " + mensaje, null });
                        break;

                    case 6: //lista de conectados
                        mensaje = recibido[2].Split('\0')[0];
                        String[] lista = mensaje.Split(',');
                        NConectados.Invoke(delegado, new object[] { "NConectados", recibido[1], null });
                        listBox1.Invoke(delegado, new object[] { "listBox1", null, lista });
                        break;
                    case 7:
                        Form3 invi = new Form3(mensaje);
                        invi.ShowDialog();
                        while (invi.resultadoinvitacion == 0)
                        {
                        }
                        invi.Close();
                        string ms;
                        if (invi.resultadoinvitacion == 1) //ha aceptado la invitacion
                        {
                            ms = "8/SI," + mensaje + "," + usuario;
                            byte[] msg = System.Text.Encoding.ASCII.GetBytes(ms);
                            server.Send(msg);
                            //
                            //iniciar partida
                            //
                        }
                        if (invi.resultadoinvitacion == 2) //no ha aceptado la invitacion
                        {
                            ms = "8/NO," + mensaje + "," + usuario;
                            byte[] msg = System.Text.Encoding.ASCII.GetBytes(ms);
                            server.Send(msg);
                        }
                        break;

                    case 8:
                        string[] trozos = mensaje.Split(',');
                        if (trozos[0] == "SI") //la otra persona ha aceptado la invitación
                        {
                            MessageBox.Show(trozos[1] + " ha aceptado tu invitación. Prepárate para jugar.");
                            //
                            //iniciar partida
                            //
                        }

                        if (trozos[0] == "NO")//la otra persona no ha aceptado la invitación
                        {
                            MessageBox.Show(trozos[1] + " ha rechazado tu invitación.");
                        }
                        break;
                    case 9:
                        string[] partes = mensaje.Split('$');
                        listBox2.Invoke(delegado, new object[] { "listBox2", null, partes });
                        break;
                }
            }
        }

        private void Form1_Load(object sender, EventArgs e)
        {
            ThreadStart ts = delegate { AtenderServidor(); };
            atender = new Thread(ts);
            atender.Start();
            string ms = "6/";
            byte[] msg = System.Text.Encoding.ASCII.GetBytes(ms);
            server.Send(msg);
        }

        private void button4_Click(object sender, EventArgs e)
        {
            string invitado = listBox1.SelectedItem.ToString();
            string ms = "7/" + usuario + "," + invitado;
            byte[] msg = System.Text.Encoding.ASCII.GetBytes(ms);
            server.Send(msg);
        }

        private void button3_Click(object sender, EventArgs e)
        {

        }

        private void button5_Click(object sender, EventArgs e)
        {
            string seleccionado = listBox1.SelectedItem.ToString();
            string ms = "9/" + seleccionado + "$" + textBox1.Text;
            byte[] msg = System.Text.Encoding.ASCII.GetBytes(ms);
            server.Send(msg);
            listBox2.Items.Add(usuario+": "+textBox1.Text);
            textBox1.Text = "";
        }

        private void listBox1_SelectedIndexChanged(object sender, EventArgs e)
        {
            if(nuevomensaje == false)
                listBox2.Items.Clear();
            label5.Text = "Chat con " + listBox1.SelectedItem.ToString();
            nuevomensaje = false;
        }

        private void textBox1_KeyDown(object sender, KeyEventArgs e)
        {
            if (e.KeyCode == Keys.Enter)
            {
                string seleccionado = listBox1.SelectedItem.ToString();
                string ms = "9/" + seleccionado + "$" + textBox1.Text;
                byte[] msg = System.Text.Encoding.ASCII.GetBytes(ms);
                server.Send(msg);
                listBox2.Items.Add(usuario + ": " + textBox1.Text);
                textBox1.Text = "";
            }
        }

    }
}
