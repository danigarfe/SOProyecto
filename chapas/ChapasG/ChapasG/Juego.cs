using Chapas;
using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;
using Microsoft.Xna.Framework.Input;
using System;
using System.Net.Sockets;
using System.Text;

namespace MyGame
{
    public class Chapas : Game
    {
        GraphicsDeviceManager graphics;
        SpriteBatch spriteBatch;
        private Texture2D texturaflecha;
        private SpriteFont courier;
        private Vector2 distancia_mouse_ficha; //para agrandar la ficha conforme alejo el mouse
        float escalaflecha;
                                                                            // ----------------------------
                                                                            //USADOS PARA LA CINEMÁTICA DE LAS FICHAS, especificada en la clase ficha
        private Vector2[] velocidadinicial = new Vector2[4]; //define la velocidad inicial para cada ficha                                                
        private Vector2[] posicioninicial = new Vector2[4];//define la posicion inicial de cada ficha 
        public TimeSpan[] start = new TimeSpan[4];  //valores de start para cada ficha
                                                                           // -----------------------------
        private float angulo; //ángulo de lanzamiento respecto a la horizontal x+ 
        private bool botonpulsado = false; //detecta si el botón izquierdo del mouse está siendo pulsado
        private MouseState estadoanterior = Mouse.GetState();
        private ficha[] fichas = new ficha[4]; //el servidor me dice cuál es la mía, mirar abajo
        int mificha = 0; //mi número de ficha en el vector, me lo asigna el servidor
        private bool fichaseleccionada = false; //para que solo se pueda lanzar arrastrando desde la ficha
        private int turno = 0;//indica la ficha  a la que le toca tirar



        //PARA EL SERVIDOR
        private Socket server;
        private string IPservidor;
        private int puertoservidor;


        public Chapas(/*string ip, int puerto, Socket srv*/)
        {//constructor, de momento sin parámetros
            graphics = new GraphicsDeviceManager(this); //inicia los gráficos
            this.IsMouseVisible = true; //establece que el mouse sea visible. 
            /* Lo usaremos para implementar el servidor
            IPservidor = ip;
            puertoservidor = puerto;
            server = srv;
            */
        }

        protected override void Initialize()
        {
            //comandos de inicio, de momento ninguno

            base.Initialize();
        }

        protected override void LoadContent() //usado para cargar el contenido de nuestro juego
        {
            Content.RootDirectory = "Content"; //establece la dirección de los contenidos 
            spriteBatch = new SpriteBatch(GraphicsDevice); //permite dibujar texturas
            Texture2D texturaficha1 = Content.Load<Texture2D>("Personaje"); //cargamos la textura del personaje (ficha)
            texturaflecha = Content.Load<Texture2D>("flecha"); //igual, con la flecha
            courier = Content.Load<SpriteFont>("courier");
            fichas[0] = new ficha(new Vector2(50, 50), texturaficha1); //pone cada ficha en su posición
            fichas[1] = new ficha(new Vector2(50, 200), texturaficha1);
            fichas[2] = new ficha(new Vector2(300, 50), texturaficha1);
            fichas[3] = new ficha(new Vector2(300, 200), texturaficha1);
            /*
            string mensaje = "7/"; //código 7, petición de inicio de partida
            byte[] msg = System.Text.Encoding.ASCII.GetBytes(mensaje);
            server.Send(msg);
            Console.WriteLine("Esperando a más jugadores...");
            byte[] msg2 = new byte[80];
            server.Receive(msg2);
            mensaje = Encoding.ASCII.GetString(msg2).Split('\0')[0];
            Console.WriteLine("Mensaje recibido: {0}", mensaje);
            mificha = Convert.ToInt32(mensaje); //saco el valor de mificha que me pasa el servidor
            */
        }

        protected override void UnloadContent() //función contraria, no la usaremos de momento
        {
        }

        protected override void Update(GameTime gameTime) //se ejecuta constantemente, junto a Draw()
        {        
            for(int i = 0; i < 4; i++)
            {
                fichas[i].SiguientePosicion(start[i], velocidadinicial[i], posicioninicial[i]); //actualiza la posición de todas las fichas
            }
            CompruebaClick();
            for (int i = 0; i < 4; i++) //pone todos los valores a 0 excepto los de la ficha a la que le toca tirar
            {
                if (fichas[i].movimientoX == false && fichas[i].movimientoY == false)
                {
                    velocidadinicial[i].X = 0;
                    velocidadinicial[i].Y = 0;
                    posicioninicial[i] = fichas[i].posicion;
                }

            }
            if (fichas[mificha].turnoacabado == 2)         ///////////////////////////////////
            {                                              ///////////////////////////////////
                fichas[mificha].turnoacabado = 0;          ///////////////////////////////////
                if (mificha == 3)                          //A MODO DE PRUEBA, MIENTRAS CONFIGURAMOS EL SERVIDOR VOY ALTERNANDO ENTRE FICHAS
                    mificha = 0;                           ///////////////////////////////////
                else                                       ///////////////////////////////////
                    mificha++;                             ///////////////////////////////////
            }                                              ///////////////////////////////////
            base.Update(gameTime);
        }

        protected override void Draw(GameTime gameTime) //usado para dibujar en pantalla. Se ejecuta constantemente.
        {
            spriteBatch.Begin(); //comienza a dibujar
            GraphicsDevice.Clear(Color.CornflowerBlue); //limpia los gráficos anteriores
            //DIBUJO DE LAS FICHAS
            fichas[0].Draw(spriteBatch);
            fichas[1].Draw(spriteBatch);
            fichas[2].Draw(spriteBatch);
            fichas[3].Draw(spriteBatch);

            //CÁLCULOS Y DIBUJO DE LA FLECHA
            MouseState nuevoestado = Mouse.GetState(); //volvemos a coger el estado del mouse
            distancia_mouse_ficha = Vector2.Subtract(new Vector2(nuevoestado.X, nuevoestado.Y), fichas[mificha].posicion);
            escalaflecha = distancia_mouse_ficha.Length() / 100;
            if (escalaflecha > 3)
                escalaflecha = 3;
            if (botonpulsado == true && escalaflecha > 0.5 && fichaseleccionada == true && fichas[mificha].movimientoX == false && fichas[mificha].movimientoY == false)
            //si el botón está pulsado, la escala es mayor que 0.5 y la ficha está parada, se dibuja la flecha con escala el módulo del vector ficha-mouse y en la posición de la ficha
                spriteBatch.Draw(texturaflecha, fichas[mificha].posicion, null, Color.Black, Convert.ToSingle(Math.Atan2(nuevoestado.Y - fichas[mificha].posicion.Y, nuevoestado.X - fichas[mificha].posicion.X)), new Vector2(2, 3), escalaflecha, SpriteEffects.None, 0f); //dibuja la flecha



            //DIBUJO DE TEXTO
            string texto = "Velocidad actual X:" + Convert.ToString(fichas[mificha].velocidadactual.X) + "Velocidad actual Y:" + Convert.ToString(fichas[mificha].velocidadactual.Y);
            spriteBatch.DrawString(courier, texto, new Vector2(0, 0), Color.White);


            spriteBatch.End(); //termina de dibujar
            base.Draw(gameTime);
        }


        //FUNCIONES UTILIZADAS:
        private void CompruebaClick() //Gestiona todo lo relacionado con los clicks del ratón
        {
            MouseState nuevoestado = Mouse.GetState();
            if (nuevoestado.LeftButton == ButtonState.Pressed && estadoanterior.LeftButton == ButtonState.Released && fichas[mificha].movimientoX == false && fichas[mificha].movimientoY == false) //detecta cuándo hemos pulsado el botón, y no estaba pulsado ni en movimiento
            {
                botonpulsado = true; 
                if (escalaflecha < 0.5)
                    fichaseleccionada = true; //botón pulsado dentro de la ficha, se interpreta como ficha seleccionada
            }

            if (nuevoestado.LeftButton == ButtonState.Released && estadoanterior.LeftButton == ButtonState.Pressed && fichaseleccionada == true && fichas[mificha].movimientoX == false && fichas[mificha].movimientoY == false) //si hemos soltado el botón estando pulsado, y además la ficha estaba seleccionada y parada (queremos lanzar)...
            {
                velocidadinicial[mificha].X = (nuevoestado.X - fichas[mificha].posicion.X)*3; //coge la componente X del vector mouse-ficha multiplicada por 3, como velocidad inicial X
                velocidadinicial[mificha].Y = (nuevoestado.Y - fichas[mificha].posicion.Y)*3; //coge la componente Y del vector mouse-ficha multiplicada por 3, como velocidad inicial Y
                angulo = Convert.ToSingle(Math.Atan2(velocidadinicial[mificha].Y, velocidadinicial[mificha].X)); //calcula el ángulo de dicho vector
                fichas[mificha].movimientoX = true; //establece el inicio de movimiento en ambos ejes
                fichas[mificha].movimientoY = true;
                posicioninicial[mificha] = fichas[mificha].posicion; //guardamos la posición inicial en ambos ejes, para cinemática
                start[mificha] = new TimeSpan(DateTime.Now.Ticks); //registramos el instante en el que se inicia el movimiento, para poder sacar t(variabletiempo) para cinemática
                botonpulsado = false; //consideramos el botón del mouse como no pulsado, para que desaparezca la flecha
                fichaseleccionada = false;
            }
            estadoanterior = nuevoestado; //el estado del mouse pasa a ser el estado anterior
        }
    }
}
