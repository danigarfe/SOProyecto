using System;
using System.Collections.Generic;
using System.Linq;
using System.Net.Sockets;
using System.Text;
using System.Threading.Tasks;
using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;
using Microsoft.Xna.Framework.Input;

namespace PongSO.Sprites
{
  public class Palo : Sprite
  {
        //VARIABLES GLOBALES
        Socket server;
        int campo;
        public bool paloimportante=false;

    public Palo(Texture2D texture, Socket sock, int camp) //CONSTRUCTOR
      : base(texture)
    {
      ModVel = 5f; //FIJAMOS LA VELOCIDAD DEL PALO
      server = sock;
            campo = camp;
    }

    public override void Update(GameTime gameTime, List<Sprite> sprites) //UPDATE DEL PALO
    {
            if (paloimportante && Input!=null)
            {
                if (Keyboard.GetState().IsKeyDown(Input.Arriba))
                {
                    Velocidad.Y = -ModVel;
                    Posicion += Velocidad;
                    Posicion.Y = MathHelper.Clamp(Posicion.Y, 0, Game1.AltoPantalla - textura.Height);
                }

                else if (Keyboard.GetState().IsKeyDown(Input.Abajo))
                {
                    Velocidad.Y = ModVel;
                    Posicion += Velocidad;
                    Posicion.Y = MathHelper.Clamp(Posicion.Y, 0, Game1.AltoPantalla - textura.Height);
                }
                    string ms = "12/" + campo + "/" + Posicion.Y;
                    byte[] msg = System.Text.Encoding.ASCII.GetBytes(ms);
                try
                {
                    server.Send(msg); //MANDAMOS AL CONTRARIO NUESTRA POSICIÓN
                }
                catch (SocketException)
                {
                }
                    Velocidad = Vector2.Zero;
            }
    }
  }
}
