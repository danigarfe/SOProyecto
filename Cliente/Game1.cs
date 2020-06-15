using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;
using Microsoft.Xna.Framework.Input;
using System;
using System.Collections.Generic;
using PongSO.Modelos;
using PongSO.Sprites;
using System.Net.Sockets;
using System.Threading;
using System.Text;
using System.Threading.Tasks;


/* El Juego ejecuta la función Update() y después la función Draw() a cada frame de la partida.
 * Los diferentes objetos del juego se han definido como clases.
 * Se ha utilizado la libreria OpenSource de la libreia XNA FrameWork.
*/
namespace PongSO
{
  public class Game1 : Game
  {        
        //VARIABLES GLOBALES
    GraphicsDeviceManager graphics;
    SpriteBatch spriteBatch;

    public static int AnchoPantalla;
    public static int AltoPantalla;
    public int ganador=100;
        int goles0;
        int goles1;
        int tiempo = 0;

    private Score puntuacionBola;
    private List<Sprite> sprites;
    private Texture2D fondo;
        private SpriteFont fuente;
        Bola bola;

        private Socket server;
        private string usuario;
        private int campo; //0 izquierda, 1 derecha
        Thread atender;

        bool MensajeServidorEmpezar = false;
        Palo Palo1;
        //Palo Palo2;
        //Palo Palo3;
        Palo Palo4;
        public Game1(Socket s, string usr, int camp) //CONSTRUCTOR DEL JUEGO
        {
          graphics = new GraphicsDeviceManager(this); 
          Content.RootDirectory = "Content";
                server = s;
                usuario = usr;
                campo = camp;
        }

    protected override void Initialize() //PRIMERA FUNCIÓN QUE SE EJECUTA
    {
      AnchoPantalla = graphics.PreferredBackBufferWidth;
      AltoPantalla = graphics.PreferredBackBufferHeight;
      ThreadStart ts = delegate { AtenderServidor(); }; //SE INICIA EL THREAD QUE ATIENDE AL SERVIDOR
      atender = new Thread(ts);
      atender.Start();
      this.IsMouseVisible = false; //ESTA SERÁ UNA DE LAS CONDICIONES PARA FINALIZAR LA PARTIDA
      base.Initialize();
    }

    protected override void LoadContent() //CARGAMOS EL CONTENIDO DEL JUEGO USANDO FUNCIONES DE XNA
    {
      spriteBatch = new SpriteBatch(GraphicsDevice);

      var TexturaPalo = Content.Load<Texture2D>("Palo");
      var TexturaBola = Content.Load<Texture2D>("Bola");
      //var TexturaPalo2 = Content.Load<Texture2D>("Palo2");

      fondo = Content.Load<Texture2D>("Fondo");

      puntuacionBola = new Score(Content.Load<SpriteFont>("Font"));
          fuente = Content.Load<SpriteFont>("Font");

          sprites = new List<Sprite>();
          Palo1 = new Palo(TexturaPalo, server, campo);
          Palo1.Posicion = new Vector2(20, (AltoPantalla / 2) - (TexturaPalo.Height / 2));
            if (campo == 0) //SI SOMOS EL JUGADOR 0 (IZQUIERDA)
            {
                Palo1.paloimportante = true; //PALOIMPORTANTE ES EL PALO QUE PODREMOS CONTROLAR
                Palo1.Input = new Input() //DEFINIMOS LOS CONTROLES
                {
                    Arriba = Keys.W,
                    Abajo = Keys.S,
                };
            }
            sprites.Add(Palo1); //LO AÑADIMOS A LA LISTA DE SPRITES DEL JUEGO
            /*
            Palo2 = new Palo(TexturaPalo2, server, campo);
            Palo2.palopequeño = true;
            Palo2.Posicion = new Vector2(200, (AltoPantalla / 2) - (TexturaPalo2.Height / 2));
            if (campo == 0)
            {
                Palo2.paloimportante = true;
                Palo2.Input = new Input()
                {
                    Arriba = Keys.E,
                    Abajo = Keys.D,
                };
            }
            sprites.Add(Palo2);
            
            Palo3 = new Palo(TexturaPalo2, server, campo);
            Palo3.palopequeño = true;
            Palo3.Posicion = new Vector2(AnchoPantalla - 200 - TexturaPalo2.Width, (AltoPantalla / 2) - (TexturaPalo.Height / 2));
            if (campo == 1)
            {
                Palo3.paloimportante = true;
                Palo3.Input = new Input()
                {
                    Arriba = Keys.I,
                    Abajo = Keys.K,
                };
            }
            sprites.Add(Palo3);
            */
            Palo4 = new Palo(TexturaPalo, server, campo); //CREAMOS EL PALO DE LA DERECHA
            Palo4.Posicion = new Vector2(AnchoPantalla - 20 - TexturaPalo.Width, (AltoPantalla / 2) - (TexturaPalo.Height / 2));
            if (campo == 1)
            {
                Palo4.paloimportante = true;
                Palo4.Input = new Input()
                {
                    Arriba = Keys.O,
                    Abajo = Keys.L,
                };
            }
            sprites.Add(Palo4);

            bola = new Bola(TexturaBola); //CREAMOS LA BOLA
            //FIJAMOS LA POSICIÓN CENTRAL
            bola.Posicion = new Vector2((AnchoPantalla / 2) - (TexturaBola.Width / 2) - 4, (AltoPantalla / 2) - (TexturaBola.Height / 2));
            bola.Score = puntuacionBola;
            sprites.Add(bola); //AÑADIMOS LA BOLA A LA LISTA DE SPRITES
        }


    protected override void UnloadContent() //EN ESTE CASO NO LO UTILIZAMOS
    {
            this.Exit();
    }

    protected override void Update(GameTime gameTime) //SE ENCARGA DE LA LÓGICA DEL JUEGO
    {
            //HA RECIBIDO UN MENSAJE DEL SERVIDOR CONFORME PUEDE EMPEZAR LA PARTIDA (Y TODAVIA NO HA TERMINADO)
            if (MensajeServidorEmpezar == true && this.IsMouseVisible==false)
            {
                foreach (var sprite in sprites) //EJECUTA LA FUNCIÓN UPDATE DE CADA UNO DE LOS OBJETOS DE LA LISTA DE SPRITES
                {
                    sprite.Update(gameTime, sprites);
                }
                if (bola.Score.Score2 >= 5 || bola.Score.Score1 >= 5) //GANA EL QUE LLEGA A 5 GOLES
                {
                    Palo1.paloimportante = false;
                    Palo4.paloimportante = false;
                    tiempo++;
                    if (tiempo >= 20)
                    {
                        bola.Jugando = false;
                        goles0 = bola.Score.Score1;
                        goles1 = bola.Score.Score2;
                        if (campo == 0)
                        {
                            string ms = "13/" + goles0 + "/" + goles1;
                            byte[] msg = System.Text.Encoding.ASCII.GetBytes(ms);
                            //EL QUE TIENE EL CAMPO 0 ES EL QUE ENVIA LA INFORMACIÓN FINAL DE LA PARTIDA AL SERVIDOR
                            server.Send(msg);
                        }
                        atender.Abort(); //cerramos el thread
                                         //ANTES DE CERRAR EL JUEGO
                        if (bola.Score.Score1 == 5)
                        {
                            ganador = 0;
                        }
                        else if (bola.Score.Score2 == 5)
                        {
                            ganador = 1;
                        }
                        this.IsMouseVisible = true;
                    }
                }
            }
            else if(MensajeServidorEmpezar == false && this.IsMouseVisible==false)
            {
                string ms = "11/"; //ENVIA AL SERVIDOR UN MENSAJE CONFORME YA ESTÁ PREPARADO
                byte[] msg = System.Text.Encoding.ASCII.GetBytes(ms);
                server.Send(msg);
            }
      base.Update(gameTime);
    }

    protected override void Draw(GameTime gameTime) //TAMBIEN SE EJECUTA A CADA FRAME Y SE UTILIZA PARA DIBUJAR EN PANTALLA
    {
      GraphicsDevice.Clear(Color.CornflowerBlue);

      spriteBatch.Begin(); //EL OBJETO SPRITEBATCH ES EL QUE NOS PERMITE DIBUJAR TEXTURAS Y TEXTO
            if (MensajeServidorEmpezar && this.IsMouseVisible==false)
            {
                spriteBatch.Draw(fondo, new Vector2(0, 0), Color.White);

                foreach (var sprite in sprites)
                    sprite.Draw(spriteBatch);

                puntuacionBola.Draw(spriteBatch);
            }
            if(MensajeServidorEmpezar==false)
            {
                spriteBatch.DrawString(fuente, "Esperando al rival...", new Vector2(0,0), Color.White);
            }
            if (this.IsMouseVisible == true) //PANTALLA FINAL
            {
                if (ganador == campo)
                {
                    spriteBatch.DrawString(fuente, "HAS GANADO!", new Vector2(0, 0), Color.White);
                }
                else
                {
                    spriteBatch.DrawString(fuente, "HAS PERDIDO :(", new Vector2(0, 0), Color.White);
                }
            }
      spriteBatch.End();

      base.Draw(gameTime);
    }





        private void AtenderServidor() //ATENDEMOS AL SERVIDOR
        {
            while (true)
            {
                try
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
                        case 11: //MENSAJE PARA INICIAR PARTIDA
                            bola.Jugando = true;
                            MensajeServidorEmpezar = true;
                            break;
                        case 12: //CAMBIA LA POSICIÓN DEL CONTRARIO
                            int posicion = Convert.ToInt32(recibido[2]);
                            if (campo == 0)
                            {
                                Palo4.Posicion.Y = posicion;
                            }
                            else if (campo == 1)
                            {
                                Palo1.Posicion.Y = posicion;
                            }
                            break;
                    }
                }
                catch(FormatException)
                {

                }
                catch (SocketException)
                {
                }
            }
        }
    }
}
