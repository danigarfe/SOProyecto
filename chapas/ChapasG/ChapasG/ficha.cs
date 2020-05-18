using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Chapas
{
    class ficha
    {
        public ficha(Vector2 pos, Texture2D tex) //constructor
        {
            posicion = pos;
            textura = tex;
        }
        private Texture2D textura; //variables de la clase ficha
        public Vector2 posicion;
        public Vector2 velocidadactual;
        public bool movimientoX=false;
        public bool movimientoY=false;
        public int turnoacabado = 0; //a 1 significa que ha acabado el movimiento en un eje, a 2 que ha acabado en los dos y por tanto el turno

        public void SiguientePosicion(TimeSpan start, Vector2 velocidadinicial, Vector2 posicioninicial) //calcula la siguiente posición de la ficha por cinemática
        {
            if (movimientoX == true || movimientoY == true) //si detecta movimiento en algún eje
            {
                if (velocidadinicial.Length() < 150)
                {
                    movimientoX = false;
                    movimientoY = false;
                    turnoacabado = 0;
                }
                TimeSpan stop = new TimeSpan(DateTime.Now.Ticks); //mira el tiempo actual
                float variabletiempo = Convert.ToSingle(stop.Subtract(start).TotalMilliseconds) / 1000; //resta el inicial para ver cuánto tiempo ha pasado desde el inicio del movimiento
                if (movimientoX == true) //si hay movimiento en el eje x...
                {
                    if (velocidadinicial.X > 0) //si el movimiento es hacia la derecha
                    {
                        posicion.X = posicioninicial.X + velocidadinicial.X * variabletiempo - 50 * (variabletiempo * variabletiempo) / 2; //establece la siguiente posición, por cinemática
                        velocidadactual.X = velocidadinicial.X - 50 * variabletiempo; //disminuye la velocidad, en valor absoluto
                    }
                    else //si es hacia la izquierda...
                    {
                        posicion.X = posicioninicial.X + velocidadinicial.X * variabletiempo + 50 * (variabletiempo * variabletiempo) / 2; //ídem
                        velocidadactual.X = velocidadinicial.X + 50 * variabletiempo; //ídem
                    }
                    if (velocidadactual.X > -0.5 && velocidadactual.X < 0.5) //si la velocidad en X es cercana a 0...
                    {
                        velocidadactual.X = 0; //fijamos la velocidad a 0
                        movimientoX = false; //paramos el movimiento
                        if (turnoacabado == 0)
                            turnoacabado = 1;
                        else if (turnoacabado == 1)
                            turnoacabado = 2;
                    }
                }
                if (movimientoY == true) //si detecta movimiento en el eje Y...
                {
                    if (velocidadinicial.Y > 0) //si el movimiento es hacia abajo (los ejes van al revés)
                    {
                        posicion.Y = posicioninicial.Y + velocidadinicial.Y * variabletiempo - 50 * (variabletiempo * variabletiempo) / 2; //cinemática en Y
                        velocidadactual.Y = velocidadinicial.Y - 50 * variabletiempo;
                    }
                    else //si es hacia arriba...
                    {
                        posicion.Y = posicioninicial.Y + velocidadinicial.Y * variabletiempo + 50 * (variabletiempo * variabletiempo) / 2; //cinemática en Y
                        velocidadactual.Y = velocidadinicial.Y + 50 * variabletiempo;
                    }

                    if (velocidadactual.Y > -0.5 && velocidadactual.Y < 0.5) //lo mismo que antes, para el movimiento si es muy bajo (para que no se vuelva negativo)
                    {
                        velocidadactual.Y = 0;
                        movimientoY = false;
                        if (turnoacabado == 0)
                            turnoacabado = 1;
                        else if (turnoacabado == 1)
                            turnoacabado = 2;
                    }
                }
            }
        } //devuelve la siguiente posición si hay movimiento y se le da posición, velocidad e instante iniciales
        public void Update()
        {

        }
        public void Draw(SpriteBatch spriteBatch) //dibuja la ficha en su posición, escala y con el origen en el centro
        {
            spriteBatch.Draw(textura, posicion, null, Color.White, 0f, new Vector2(110, 100), 0.3f, SpriteEffects.None, 0f);
        }
    }
}
