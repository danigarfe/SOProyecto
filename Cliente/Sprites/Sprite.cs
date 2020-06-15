using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using PongSO.Modelos;

//CLASE GENERAL UTILIZADA PARA TODOS LOS SPRITES

namespace PongSO.Sprites
{
  public class Sprite
  {
        //VARIABLES GLOBALES
    protected Texture2D textura;

    public Vector2 Posicion;
    public Vector2 Velocidad;
    public float ModVel; //Módulo de la velocidad 
    public Input Input;

    public Rectangle Rectangle //DEFINE LOS LÍMITES DEL SPRITE. SE USA PARA COLISIONES
    {
      get
      {
        return new Rectangle((int)Posicion.X, (int)Posicion.Y, textura.Width, textura.Height);
      }
    }

    public Sprite(Texture2D texture) //CONSTRUCTOR
        {
            textura = texture;
    }

    public virtual void Update(GameTime gameTime, List<Sprite> sprites)
    {

    }

    public virtual void Draw(SpriteBatch spriteBatch)
    {
      spriteBatch.Draw(textura, Posicion, Color.Green);
    }


        //DETECTA COLISIONES ENTRE SPRITES

    protected bool Toca_Izquierda(Sprite sprite)
    {
      return this.Rectangle.Right + this.Velocidad.X > sprite.Rectangle.Left &&
        this.Rectangle.Left < sprite.Rectangle.Left &&
        this.Rectangle.Bottom > sprite.Rectangle.Top &&
        this.Rectangle.Top < sprite.Rectangle.Bottom;
    }

    protected bool Toca_Derecha(Sprite sprite)
    {
      return this.Rectangle.Left + this.Velocidad.X < sprite.Rectangle.Right &&
        this.Rectangle.Right > sprite.Rectangle.Right &&
        this.Rectangle.Bottom > sprite.Rectangle.Top &&
        this.Rectangle.Top < sprite.Rectangle.Bottom;
    }

    protected bool Toca_Arriba(Sprite sprite)
    {
      return this.Rectangle.Bottom + this.Velocidad.Y > sprite.Rectangle.Top &&
        this.Rectangle.Top < sprite.Rectangle.Top &&
        this.Rectangle.Right > sprite.Rectangle.Left &&
        this.Rectangle.Left < sprite.Rectangle.Right;
    }

    protected bool Toca_Abajo(Sprite sprite)
    {
      return this.Rectangle.Top + this.Velocidad.Y < sprite.Rectangle.Bottom &&
        this.Rectangle.Bottom > sprite.Rectangle.Bottom &&
        this.Rectangle.Right > sprite.Rectangle.Left &&
        this.Rectangle.Left < sprite.Rectangle.Right;
    }
  }
}
