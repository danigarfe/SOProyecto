using System;
using System.Net;
using System.Net.Sockets;

namespace Chapas
{
#if WINDOWS || LINUX
    /// <summary>
    /// The main class.
    /// </summary>
    public static class Program
    {
        /// <summary>
        /// The main entry point for the application.
        /// </summary>
        [STAThread]

        static void Main()
        {
            /*
            Socket server;
            string IPservidor = "192.168.56.101"; //poner aqui la IP del servidor
            int puertoservidor = 9060; //poner aqui el puerto del servidor
            IPAddress direc = IPAddress.Parse(IPservidor);
            IPEndPoint ipep = new IPEndPoint(direc, puertoservidor);


            //Creamos el socket 
            server = new Socket(AddressFamily.InterNetwork, SocketType.Stream, ProtocolType.Tcp);
            try
            {
                server.Connect(ipep);//Intentamos conectar el socket
                Console.WriteLine("Conectado");

            }
            catch (SocketException ex)
            {
                //Si hay excepcion imprimimos error y salimos del programa con return 
                Console.WriteLine("No he podido conectar con el servidor");
                return;
            }
            */
            var game = new MyGame.Chapas(/*IPservidor, puertoservidor, server*/);
            game.Run();
        }
    }
#endif
}
