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
using System.Text.RegularExpressions;

namespace PongSO
{
    public partial class Form1 : Form
    {
        //VARIABLES GLOBALES
        Thread atender;
        Socket server;
        string usuario;
        string IPservidor;
        int puertoservidor;
        bool nuevomensaje = false;
        int campo;

        //DELEGATE PARA CAMBIAR EL LABEL DE RESPUESTA
        delegate void DelegadoParaEscribir (string label, string texto, string[] lista);

        public void CambiaString(string label, string texto, string[] lista)
        {
            int blancos = 0;
            if (label == "resultado1")
            {
                resultado1.Text = texto; //LABEL DE LAS RESPUESTAS A LAS CONSULTAS A LA BBDD
            }
            if (label == "NConectados")
                NConectados.Text = texto; //LABEL DEL NÚMERO DE CONECTADOS

            if (label == "listBox1")
            {
                listBox1.Items.Clear();
                    for (int i = 0; i < lista.Length; i++) //SE USA PARA ARREGLAR UN POSIBLE ERROR DE LA LISTA DE CONECTADOS
                    {
                    if (lista[i] == "")
                    {
                        blancos++;
                    }
                    else
                    {
                        listBox1.Items.Add(lista[i]);
                    }
                    //AQUI LO ARREGLAMOS
                    NConectados.Text = Convert.ToString(Convert.ToInt32(NConectados.Text) - blancos);
                    }
            }
            if (label == "listBox2")
            {
                //CHAT
                string emisor = lista[0];
                string txt = lista[1];
                listBox2.Items.Add(emisor + ": " + txt);
                nuevomensaje = true;
                listBox1.SelectedItem = emisor;
            }
        }
        public Form1(string u, string ip, int puerto, Socket srv) //CONSTRUCTOR DE LA FORM1
        {
            InitializeComponent();
            usuario = u;
            IPservidor = ip;
            puertoservidor = puerto;
            server = srv;
        }


        private void button2_Click(object sender, EventArgs e) //BOTÓN ENVIAR CONSULTAS A LA BBDD
        {

            if (checkganador.Checked)
            {
                if (IDpartida.Text.Length != 0)
                {
                    bool simbolomalo = false; //ESTO SIRVE PARA EVITAR ERRORES EN LAS CONSULTAS DEBIDO A SIMBOLOS INCORRECTOS
                    for(int i=0;i<IDpartida.Text.Length && simbolomalo == false; i++)
                    {
                        if (!Regex.IsMatch(IDpartida.Text, @"^\d+$")) //MIRAMOS SI EL VALOR INTRODUCIDO ES NUMÉRICO
                            simbolomalo = true;
                    }
                    if (simbolomalo == false)
                    {
                        string mensaje = "4/" + IDpartida.Text;
                        // Enviamos al servidor el nombre tecleado
                        byte[] msg = System.Text.Encoding.ASCII.GetBytes(mensaje);
                        server.Send(msg);
                    }
                    else
                    {
                        MessageBox.Show("Valor no permitido.");
                    }
                }
                else
                {
                    MessageBox.Show("Introduce un valor de partida.");
                }
            }
            if (checkgoles.Checked || checkpartidas.Checked)
            {

                if (nombre.Text.Length > 0)
                {
                    bool simbolomalo = false;
                    for (int i = 0; i < nombre.Text.Length && simbolomalo == false; i++)
                    {
                        if (nombre.Text[i] == '/' || nombre.Text[i] == ' ') //ESTOS SIMBOLOS PUEDEN HACER QUE NO FUNCIONE BIEN EL CLIENTE
                            simbolomalo = true;
                    }
                    if (simbolomalo == false)
                    {
                        if (checkgoles.Checked)
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
                    else
                    {
                        MessageBox.Show("Símbolo no permitido.");
                    }
                }
                else
                {
                    MessageBox.Show("Introduzca un nombre.");
                }
            }
        }

        private void button1_Click(object sender, EventArgs e) //DESCONECTARSE
        {
            string ms = "0/" + usuario;
            byte[] msg = System.Text.Encoding.ASCII.GetBytes(ms);
            try
            {
                server.Send(msg);
                atender.Abort();
                this.Close();
                Application.Exit();
            }
            catch(SocketException)//SI HAY UN FALLO, NO HACEMOS NADA
            {
            }
        }

        private void AtenderServidor() //ATIENDE AL SERVIDOR
        {
            DelegadoParaEscribir delegado = new DelegadoParaEscribir(CambiaString);
            while (true)
            {
                try //LO PONEMOS POR SI HAY ERRORES EN LA CONEXIÓN
                {
                    string mensaje = null;
                    //recibimos mensaje del servidor
                    byte[] msg2 = new byte[80];
                    server.Receive(msg2);
                    string[] recibido = Encoding.ASCII.GetString(msg2).Split('/');
                    int codigo = Convert.ToInt32(recibido[0]);
                    if (recibido.Length > 1)
                    {
                        mensaje = recibido[1].Split('\0')[0];
                    }
                    switch (codigo)
                    {
                        case 3:  //número de partidas ganadas por un jugador
                            if (mensaje == "NO EXISTE")
                                resultado1.Invoke(delegado, new object[] { "resultado1", "El jugador introducido no existe.", null });
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
                            else if (mensaje == "(null)")
                                resultado1.Invoke(delegado, new object[] { "resultado1", "Número de goles marcados: 0", null });
                            else
                                resultado1.Invoke(delegado, new object[] { "resultado1", "Número de goles marcados: " + mensaje, null });
                            break;

                        case 6: //lista de conectados
                            mensaje = recibido[2].Split('\0')[0];
                            String[] lista = mensaje.Split(',');
                            NConectados.Invoke(delegado, new object[] { "NConectados", recibido[1], null });
                            listBox1.Invoke(delegado, new object[] { "listBox1", null, lista });
                            break;
                        case 7: //RECIBIMOS INVITACIÓN
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
                                //iniciar partida como invitado
                                //
                                campo = 0; //el invitado tiene el campo de la izquierda
                                Thread thread = new Thread(IniciarJuego);
                                thread.Start();
                            }
                            if (invi.resultadoinvitacion == 2) //no ha aceptado la invitacion
                            {
                                ms = "8/NO," + mensaje + "," + usuario;
                                byte[] msg = System.Text.Encoding.ASCII.GetBytes(ms);
                                server.Send(msg);
                            }
                            break;

                        case 8: //ENVIAMOS INVITACION
                            string[] trozos = mensaje.Split(',');
                            if (trozos[0] == "SI") //la otra persona ha aceptado la invitación
                            {
                                //
                                //iniciar partida
                                //
                                campo = 1; //el invitador tiene el campo de la derecha
                                Thread thread = new Thread(IniciarJuego);
                                thread.Start();
                            }

                            if (trozos[0] == "NO")//la otra persona no ha aceptado la invitación
                            {
                                MessageBox.Show(trozos[1] + " ha rechazado tu invitación.");
                            }
                            break;
                        case 9: //CHAT
                            string[] partes = mensaje.Split('$');
                            listBox2.Invoke(delegado, new object[] { "listBox2", null, partes });
                            break;

                        case 14: //ELIMINAMOS LA CUENTA ACTUAL
                            if (recibido[1].Split('\0')[0] == "OK")
                            {
                                MessageBox.Show("Tu cuenta se ha eliminado correctamente.");
                                ms = "0/" + usuario;
                                byte[] msg = System.Text.Encoding.ASCII.GetBytes(ms);
                                server.Send(msg);
                                Application.Exit();
                                atender.Abort();
                            }
                            else if (recibido[1] == "ERROR")
                            {
                                MessageBox.Show("Error al eliminar tu cuenta.");
                            }
                            break;
                    }
                }
                catch (FormatException)
                {

                }
                catch (SocketException)
                {
                    MessageBox.Show("Conexión interrumpida.");
                    atender.Abort();
                    this.Close();
                    Application.Exit();
                }
            }
        }

        private void Form1_Load(object sender, EventArgs e)
        {
            ThreadStart ts = delegate { AtenderServidor(); };
            atender = new Thread(ts);
            atender.Start();
            string ms = "6/";       //ACTUALIZAMOS LA LISTA DE CONECTADOS
            byte[] msg = System.Text.Encoding.ASCII.GetBytes(ms);
            server.Send(msg);
        }

        private void button4_Click(object sender, EventArgs e) //BOTÓN DE INVITAR
        {
            if (listBox1.SelectedItem != null) //TIENE QUE HABER ALGUIEN SELECCIONADO
            {
                string invitado = listBox1.SelectedItem.ToString();
                if (invitado != usuario)        //SI NO TE HAS SELECCIONADO A TÍ MISMO
                {
                    string ms = "7/" + usuario + "," + invitado;
                    byte[] msg = System.Text.Encoding.ASCII.GetBytes(ms);
                    server.Send(msg);
                }
                else
                {
                    MessageBox.Show("No te puedes invitar a ti mismo.");
                }
            }
            else
            {
                MessageBox.Show("No hay usuario seleccionado.");
            }
        }



        public void IniciarJuego() //INICIA UNA PARTIDA
        {
            Game1 juego = new Game1(server, usuario, campo);
            juego.Run();
            juego.Exit();
        }

        private void button5_Click(object sender, EventArgs e) //BOTÓN DE ENVIAR DEL CHAT
        {
            if (listBox1.SelectedItem != null)//TIENE QUE HABER ALGUIEN SELECCIONADO PARA ENVIAR
            {
                string seleccionado = listBox1.SelectedItem.ToString();
                string texto = textBox1.Text;
                bool dolar = false;
                for (int i = 0; i < texto.Length; i++)
                {
                    if (texto[i] == '$' || texto[i]=='/')
                        dolar = true;
                }
                if (textBox1.TextLength > 0 && dolar == false)
                {
                    string ms = "9/" + seleccionado + "$" + textBox1.Text;
                    byte[] msg = System.Text.Encoding.ASCII.GetBytes(ms);
                    server.Send(msg);
                    listBox2.Items.Add(usuario + ": " + textBox1.Text);
                    textBox1.Text = "";
                }
                else if (textBox1.TextLength == 0)
                {
                    MessageBox.Show("Mensaje vacío.");
                }
                else if (dolar == true)
                {
                    MessageBox.Show("Mensaje erróneo.");
                }
            }
            else
            {
                MessageBox.Show("Selecciona a un usuario.");
            }
        }

        private void listBox1_SelectedIndexChanged(object sender, EventArgs e) //REINICIA EL CHAT CUANDO SELECIONAS A OTRO USUARIO
        {
            if (listBox1.SelectedItem != null)
            {
                if (nuevomensaje == false)
                    listBox2.Items.Clear();
                label5.Text = "Chat con " + listBox1.SelectedItem.ToString();
                nuevomensaje = false;
            }
        }

        private void textBox1_KeyDown(object sender, KeyEventArgs e) //PODEMOS PRESIONAR ENTER PARA ENVIAR MENSAJES EN EL CHAT
        {
            if (listBox1.SelectedItem != null && e.KeyCode == Keys.Enter) //MISMO PROCESO QUE ANTES
            {
                string seleccionado = listBox1.SelectedItem.ToString();
                string texto = textBox1.Text;
                bool dolar = false;
                for (int i = 0; i < texto.Length; i++)
                {
                    if (texto[i] == '$' || texto[i]=='/')
                        dolar = true;
                }
                if (textBox1.TextLength > 0 && dolar == false)
                {
                    string ms = "9/" + seleccionado + "$" + textBox1.Text;
                    byte[] msg = System.Text.Encoding.ASCII.GetBytes(ms);
                    server.Send(msg);
                    listBox2.Items.Add(usuario + ": " + textBox1.Text);
                    textBox1.Text = "";
                }
                else if (textBox1.TextLength == 0)
                {
                    MessageBox.Show("Mensaje vacío.");
                }
                else if (dolar == true)
                {
                    MessageBox.Show("Mensaje erróneo.");
                }
            }
            else if (listBox1.SelectedItem == null)
            {
                MessageBox.Show("Selecciona a un usuario.");
            }
        }

        private void button3_Click_1(object sender, EventArgs e) //BORRAR CUENTA
        {
            string ms = "14/";
            byte[] msg = System.Text.Encoding.ASCII.GetBytes(ms);
            server.Send(msg);
        }
    }
}
