using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;
using Microsoft.Xna.Framework.Input;

namespace PongSO.Sprites
{
  public class Bola : Sprite
  {
    private float temporizador = 0f; // Contador para incrementar la velocidad de la bola
    private Vector2? PosicionInicial = null;
    private float? VelocidadInicial;
    public bool Jugando;

    public Score Score;
    public int PeriodoIncremento = 5; // Cada cuántos segundos se incrementa la velocidad de la bola

    public Bola(Texture2D texture) //CONSTRUCTOR DE LA BOLA
      : base(texture)
    {
      ModVel = 1.5f; //FIJAMOS VELOCIDAD INICIAL DE LA BOLA
    }

    public override void Update(GameTime gameTime, List<Sprite> sprites)
    {
      if (PosicionInicial == null)
      {
                PosicionInicial = Posicion;
                VelocidadInicial = ModVel;

        Restart(); //REINICIAMOS EL ESCENARIO
      }

      if (!Jugando)
        return;

            temporizador += (float)gameTime.ElapsedGameTime.TotalSeconds;

      if (temporizador > PeriodoIncremento)
      {
        ModVel++; //aumentamos el módulo de la velocidad si el contador llega al periodo 
                temporizador = 0;
      }

      foreach (var sprite in sprites)
      {
        if (sprite == this)
          continue;

        //CHOQUE DE LA BOLA CON LOS PALOS
        if (this.Velocidad.X > 0 && this.Toca_Izquierda(sprite))
          this.Velocidad.X = -this.Velocidad.X;
        if (this.Velocidad.X < 0 && this.Toca_Derecha(sprite))
          this.Velocidad.X = -this.Velocidad.X;
        if (this.Velocidad.Y > 0 && this.Toca_Arriba(sprite))
          this.Velocidad.Y = -this.Velocidad.Y;
        if (this.Velocidad.Y < 0 && this.Toca_Abajo(sprite))
          this.Velocidad.Y = -this.Velocidad.Y;
      }

      if (Posicion.Y <= 0 || Posicion.Y + textura.Height >= Game1.AltoPantalla)//CHOQUE DE LA BOLA CON LOS LÍMITES SUPERIORES DEL CAMPO
        Velocidad.Y = -Velocidad.Y;

      if (Posicion.X <= 0) //CHOQUE CON EL LÍMITE IZQUIERDO. DETECTA EL GOL
      {
        Score.Score2++;
        Restart();
      }

      if (Posicion.X + textura.Width >= Game1.AnchoPantalla) //CHOQUE CON EL LÍMITE DERECHO. DETECTA EL GOL
            {
        Score.Score1++;
        Restart();
      }

            Posicion += Velocidad * ModVel;
    }

    public void Restart() //REINICIA EL ESCENARIO
    {
            Velocidad = new Vector2(1, -1); //LA BOLA SIEMPRE INICIARÁ EN ESA DIRECCIÓN
            Posicion = (Vector2)PosicionInicial;
            ModVel = (float)VelocidadInicial;
            temporizador = 0;
            Jugando = true;
    }
  }
}
