#include <string.h>
#include <unistd.h>
#include <stdlib.h>
#include <sys/types.h>
#include <sys/socket.h>
#include <netinet/in.h>
#include <stdio.h>
#include <mysql/mysql.h>
#include <pthread.h>
#include <netdb.h>

#define PUERTO 50068

//ESTRUCTURAS UTILIZADAS EN EL SERVIDOR
typedef struct {
	char nombre [20];
	int socket;
} Conectado;
typedef struct {
	int sockets[2];
	int puntuacion[2];
	int preparado[2];
} Partida;
typedef struct {
	Partida lista[100];
	int num;
} ListaPartidas;
typedef struct {
	Conectado conectados [100];
	int num;
} ListaConectados;

typedef struct{
	int socket;
	ListaConectados miLista;
	MYSQL *conn;
} Argumento;

//VARIABLES GLOBALES
pthread_mutex_t mutex = PTHREAD_MUTEX_INITIALIZER;
ListaPartidas milistapartidas;
Argumento arg;

int PonSocket (ListaConectados *milista, int socket){ //INTRODUCE UN NUEVO SOCKET EN LA LISTA DE CONECTADOS
	if(milista->num<100){
		milista->conectados[milista->num].socket = socket;
		milista->num++;
		return 0;
	}
	else
	   return -1;
}
int EliminaSocket (ListaConectados *milista, int socket){ //ELIMINA UN SOCKET DE LA LISTA DE CONECTADOS
	int i=0;
	int encontrado = 0;
	while ((i<milista->num) && !encontrado)
	{
		if(milista->conectados[i].socket==socket)
			encontrado=1;
		if (!encontrado)
			i=i+1;
	}
	if(encontrado){
		int pos = i;
		for (int j=pos; j < milista->num-1; j++)
		{
			milista->conectados[j] = milista->conectados[j+1];
			//strcpy (lista->conectados[i].nombre, lista->conectados[i+1].nombre);
			//lista->conectados[i].socket =lista->conectados[i+1].socket;
		}
		milista->num--;
		return 0;
	}
	else
	   return -1;
}
	
	
void PonNombre (ListaConectados *milista, char nombre[30], int socket){ //INTRODUCE UN NOMBRE EN LA LISTA DE CONECTADOS
	int encontrado = 0;
	for(int i = 0; encontrado == 0 && i<100; i++){
		if(milista->conectados[i].socket == socket){
			encontrado = 1;
			strcpy(milista->conectados[i].nombre, nombre);
		}
	}
}

int DameSocket(ListaConectados *milista, char nombre[30]){ //DEVUELVE EL SOCKET DE UN USUARIO CONECTADO
	int encontrado = 0;
	int socket;
	for(int i =0; encontrado == 0 && i<100; i++){
		if(strcmp(milista->conectados[i].nombre, nombre) == 0){
			encontrado == 1;
			socket = milista->conectados[i].socket;
		}
	}
	return socket;
}

int EliminarPartida(int partida){ //ELIMINA UNA PARTIDA DE LA LISTA DE PARTIDAS
	for (int j=partida; j < milistapartidas.num; j++)
		{
			milistapartidas.lista[j] = milistapartidas.lista[j+1];
			//strcpy (lista->conectados[i].nombre, lista->conectados[i+1].nombre);
			//lista->conectados[i].socket =lista->conectados[i+1].socket;
		}
		milistapartidas.num--;
		return 0;
}

int DamePartida(int socket){ //DEVUELVE EL NUMERO DE LA PARTIDA QUE ESTÁ JUGANDO ESE SOCKET
	int encontrado = 0;
	int i;
	for(i=0; encontrado==0 && i<100; i++){
		if(milistapartidas.lista[i].sockets[0]==socket || milistapartidas.lista[i].sockets[1]==socket)
			encontrado=1;
	}
	return i-1;
}

void DameNombre(ListaConectados *milista, int socket, char nombre[30]){ //DEVUELVE EL NOMBRE DEL USUARIO QUE SE CORRESPONDE A ESE SOCKET
	int encontrado = 0;
	for(int i =0; encontrado == 0 && i<100; i++){
		if(milista->conectados[i].socket==socket){
			encontrado == 1;
			strcpy(nombre, milista->conectados[i].nombre);
		}
	}
}

int Eliminar (ListaConectados *lista, char nombre[20]){ //ELIMINA UN USUARIO DE LA LISTA DE CONECTADOS SEGÚN EL NOMBRE
	int i=0;
	int encontrado = 0;
	while ((i<lista->num) && !encontrado)
	{
		if (strcmp(lista->conectados[i].nombre, nombre) == 0)
			encontrado=1;
		if (!encontrado)
			i=i+1;
	}
	int pos = i;
	if (pos == -1)
		return -1;
	else {
		for (int j=pos; j < lista->num-1; j++)
		{
			lista->conectados[j] = lista->conectados[j+1];
			//strcpy (lista->conectados[i].nombre, lista->conectados[i+1].nombre);
			//lista->conectados[i].socket =lista->conectados[i+1].socket;
		}
		lista->num--;
		return 0;
	}
}  //Retorna 0 si elimina y -1 si ese usuario no estï¿½ en la lista
void DameConectados (ListaConectados *lista, char conectados [300]){
	// Pone en conectados los nombres de todos los conectados separados
	// por una coma. Primero pone el numero de conectados y lo separa
	// de los nombres con un /. Ejemplo:
	// "3/Juan,Maria,Pedro"
	char con[300];
	sprintf (con,"%d/", lista->num);
	int i;
	for (i=0; i< lista->num; i++){
		sprintf (conectados, "%s%s,", con, lista->conectados[i].nombre);
		strcpy(con, conectados);
	}
	conectados[strlen(conectados)-1]='\0';
}
void *AtenderCliente (){ //ATIENDE AL CLIENTE
	int sock_conn=arg.socket;
	int err;
	// Estructura especial para almacenar resultados de consultas
	MYSQL_RES *resultado;
	MYSQL_ROW row;

	//int socket_conn = * (int *) socket;

	char peticion[512];
	char respuesta[512];
	int ret;



	int i=1;
	int terminar=0;
	while(terminar==0)
	{
		//sock_conn es el socket que usaremos para este cliente
		// Ahora recibimos la peticiÃ³n
		ret=read(sock_conn,peticion, sizeof(peticion));
		printf("Recibido\n");

		// Tenemos que aÃ±adirle la marca de fin de string
		// para que no escriba lo que hay despues en el buffer
		peticion[ret]='\0';
		printf("Peticion: %s\n",peticion);

		char *p = strtok(peticion, "/");
		int codigo = atoi(p);
		printf("Codigo detectado: %d\n", codigo);
		char nombre[30];
		char contrasena[30];

		if (codigo == 0) //Desconectarse
		{
			terminar = 1;
			strcpy(respuesta, "DESCONECTA");
			printf("Conexiï¿½n terminada.\n");

			p = strtok(NULL,"/"); //Cogemos el nombre para eliminarlo de la lista de conectados
			strcpy(nombre,p);
			pthread_mutex_lock(&mutex);
			int eliminar = Eliminar(&arg.miLista, nombre);
			pthread_mutex_unlock(&mutex);
			if (eliminar == -1)
				printf("No estaba conectado\n");
			if (eliminar == 0)
				printf("Eliminado correctamente\n");

			char notificacion[300];
			char conectados[300];
			DameConectados(&arg.miLista,conectados);
			sprintf(notificacion, "6/%s", conectados);
			int j;
			//Envia notificaciÃ³n a toda la lista de Conectados
			for (j=0; j<arg.miLista.num;j++)
			{
				write(arg.miLista.conectados[j].socket,notificacion,strlen(notificacion));
				printf("Enviada notificaciÃ³n a %s\n", arg.miLista.conectados[j].nombre);
				printf("Mensaje notificacion: %s\n", notificacion);
			}
			write(sock_conn, respuesta, strlen(respuesta));
		}

		if (codigo==1) //Registrarse 	1/Nombre/Contraseña
		{
			int igual=0;
			p = strtok(NULL,"/");
			strcpy(nombre,p);
			char consulta [500];

			sprintf(consulta, "SELECT nombre FROM Jugador WHERE Jugador.nombre = '%s';", nombre); //CONSULTA MYSQL

			err=mysql_query(arg.conn, consulta);
			if (err != 0)
			{
				sprintf(respuesta, "1/ERROR");
			}
			else
			{
				resultado = mysql_store_result(arg.conn);
				row = mysql_fetch_row(resultado);

				if (row == NULL)
				{
					p = strtok(NULL, "/");
					strcpy(contrasena,p);


					err=mysql_query(arg.conn, "SELECT COUNT(*) FROM Jugador;");
					resultado = mysql_store_result(arg.conn);
					row = mysql_fetch_row(resultado);


					char consulta[500];

					sprintf(consulta, "INSERT INTO Jugador VALUES(%s,'%s','%s',0);", row[0], nombre, contrasena); 
					//METEMOS DATOS A LA BBDD
					err=mysql_query(arg.conn, consulta);


					sprintf(respuesta, "1/REGISTRADO OK");
				}

				else
				{
					sprintf(respuesta,"1/NOMBRE EN USO");
				}

			}
			write(sock_conn, respuesta, strlen(respuesta));
			terminar=1;
		}

		else if (codigo==2) //Loguearse		2/Nombre/Contraseï¿½a
		{
			p = strtok(NULL, "/");
			strcpy(nombre,p);

			p = strtok(NULL,"/");
			strcpy(contrasena,p);
			char consulta[500];

			sprintf(consulta, "SELECT * FROM Jugador WHERE Jugador.nombre = '%s' AND Jugador.contrasena = '%s';", nombre, contrasena);


			err=mysql_query(arg.conn, consulta);
			resultado = mysql_store_result(arg.conn);
			row = mysql_fetch_row(resultado);
			if(row == NULL)
			{
				sprintf(respuesta, "2/NO ENTRA");
				printf("Respuesta: 2/NO ENTRA");
				terminar=1;
			}
			else{
				char mensajeentra[30];
				sprintf(mensajeentra, "2/ENTRA");
				write(sock_conn,mensajeentra,strlen(mensajeentra));
				printf("%s conectado.\n", nombre);
				pthread_mutex_lock(&mutex);
				PonNombre(&arg.miLista, nombre, sock_conn);
				pthread_mutex_unlock(&mutex);
				char notificacion[300];
				char conectados[300];
				DameConectados(&arg.miLista,conectados);
				sprintf(notificacion, "6/%s", conectados);
				int j;
				//Envia notificación a toda la lista de Conectados
				for (j=0; j<arg.miLista.num;j++)
				{
					write(arg.miLista.conectados[j].socket,notificacion,strlen(notificacion));
					printf("Enviada notificaciÃ³n a %s\n", arg.miLista.conectados[j].nombre);
					printf("Mensaje notificacion: %s\n", notificacion);
				}
			}
			write(sock_conn, respuesta, strlen(respuesta));
		}

		else if (codigo==3)	//Numero de partidas ganadas por un jugador	3/Nombre
		{
			p=strtok(NULL,"/");
			strcpy(nombre,p);
			char consulta [500];

			strcpy(consulta,"SELECT COUNT(nombreganador) FROM Partida WHERE nombreganador = '");
			strcat(consulta, nombre);
			strcat(consulta,"';");
			// hacemos la consulta
			err=mysql_query(arg.conn, consulta);
			if (err!=0) {
				sprintf(respuesta,"3/ERROR");
			}
			else
			{
				resultado = mysql_store_result(arg.conn);
				row = mysql_fetch_row(resultado);
				if (row == NULL)
					sprintf(respuesta,"3/NO EXISTE");
				else
				{
					int result = atoi(row[0]);
					sprintf(respuesta,"3/%d",result);
				}
			}
			write(sock_conn, respuesta, strlen(respuesta));
		}

		else if (codigo==4)	//Nombre del ganador de una partida en concreto		4/idPartida
		{
			char idPartida [20];
			p=strtok(NULL,"/");
			strcpy(idPartida,p);
			char consulta [500];

			strcpy(consulta,"SELECT Jugador.nombre FROM Jugador, Partida WHERE Partida.id=");
			strcat(consulta, idPartida);
			strcat(consulta," AND Jugador.nombre=Partida.nombreganador;");
			err=mysql_query(arg.conn, consulta);
			if (err!=0) {
				sprintf(respuesta,"4/ERROR");
			}
			else
			{
				resultado = mysql_store_result(arg.conn);
				row = mysql_fetch_row(resultado);
				if (row == NULL)
					sprintf(respuesta,"4/NO EXISTE");
				else
					sprintf(respuesta,"4/%s",row[0]);
			}
			write(sock_conn, respuesta, strlen(respuesta));
		}

		else if (codigo==5)			//Numero de goles de un jugador		5/Nombre
		{
			int golesresultado;
			p=strtok(NULL,"/");
			strcpy(nombre,p);
			
			char consulta [500];
			
			sprintf(consulta, "SELECT nombre FROM Jugador WHERE Jugador.nombre = '%s';", nombre); //MIRAMOS SI EL JUGADOR EXISTE
			err=mysql_query(arg.conn, consulta);
			if (err != 0)
			{
				sprintf(respuesta, "5/ERROR");
			}
			else{
				resultado = mysql_store_result(arg.conn);
				row = mysql_fetch_row(resultado);
				if(row==NULL){
					sprintf(respuesta,"5/NO EXISTE");
				}
				else{       
						strcpy(consulta,"SELECT SUM(RelacionJP.goles) FROM Jugador, RelacionJP WHERE Jugador.nombre='");
						strcat(consulta, nombre);
						strcat(consulta,"' AND Jugador.id=RelacionJP.id_J;");
						
						err=mysql_query(arg.conn, consulta);
						
						if (err!=0)
						{
							sprintf(respuesta, "5/ERROR");
						}
						else
						{
							resultado = mysql_store_result(arg.conn);
							
							row = mysql_fetch_row(resultado);
							
							if (row == NULL)
								sprintf(respuesta,"5/NO EXISTE");
							else
							{
								// la columna 0 contiene una palabra que son los goles
								sprintf(respuesta,"5/%s", row[0]);
							}
						}
				}
			}
			write(sock_conn, respuesta, strlen(respuesta));
		}
		else if(codigo==6){ //NOTIFICACIÓN LISTA DE CONECTADOS 			6/
			char notificacion[300];
			char conectados[300];
			DameConectados(&arg.miLista,conectados); //NOS DEVUELVE UNA LISTA DE NOMBRES SEPARADOS POR UNA COMA PRECEDIDOS POR EL NÚMERO DE CONECTADOS
			sprintf(notificacion, "6/%s/", conectados);
			int j;
			//Envia notificación a toda la lista de Conectados
			for (j=0; j<arg.miLista.num;j++)
			{
				write(arg.miLista.conectados[j].socket,notificacion,strlen(notificacion));
				printf("Enviada notificaciï¿½n a %s\n", arg.miLista.conectados[j].nombre);
				printf("Mensaje notificacion: %s\n", notificacion);
			}
		}

		else if(codigo==7){ //ENVIA LA INVITACIÓN AL USUARIO INVITADO		7/invitador/invitado
			char invitador[30];
			char invitado[30];
			char mensaje[100];
			p = strtok(NULL, ",");
			strcpy(invitador, p);
			p = strtok(NULL, ",");
			strcpy(invitado, p);
			int socketinvitado = DameSocket(&arg.miLista, invitado);
			sprintf(mensaje, "7/%s", invitador);
			write(socketinvitado, mensaje, strlen(mensaje));
			printf("Mensaje enviado: %s\n", mensaje);
		}

		else if(codigo == 8){ //DEVUELVE LA RESPUESTA DEL INVITADO AL INVITADOR		8/resultado/invitador/invitado
			char resultado[30];
			char invitador[30];
			char invitado[30];
			char mensaje[100];

			p = strtok(NULL, ",");
			strcpy(resultado, p); //cojo la respuesta

			p = strtok(NULL, ",");
			strcpy(invitador, p); //cojo el nombre del invitador

			p = strtok(NULL, ",");
			strcpy(invitado, p); //cojo el nombre del invitado

			int socketinvitador = DameSocket(&arg.miLista, invitador);
			sprintf(mensaje, "8/%s,%s", resultado, invitado); //informa al invitador de que su partida ha sido aceptada
			write(socketinvitador, mensaje, strlen(mensaje));


			//AQUI EMPEZAR NUEVA PARTIDA
			Partida nuevapartida;
			nuevapartida.sockets[0] = DameSocket(&arg.miLista, invitado);
			nuevapartida.sockets[1] = DameSocket(&arg.miLista, invitador);
			nuevapartida.puntuacion[0]=0;
			nuevapartida.puntuacion[1]=0;

			pthread_mutex_lock(&mutex);
			milistapartidas.lista[milistapartidas.num]=nuevapartida; //aï¿½ade la partida a la lista de partidas
			milistapartidas.num++;
			pthread_mutex_unlock(&mutex);
			printf("Creada la partida %d\n", milistapartidas.num-1);
		}

		else if(codigo==9){ //CHAT 		9/DESTINATARIO/MENSAJE
			char destinatario[30];
			int socket_destinatario;
			char emisor[30];
			char texto[300];
			char res[400];
			p = strtok(NULL, "$");
			strcpy(destinatario, p);

			p = strtok(NULL, "$");
			strcpy(texto, p);

			DameNombre(&arg.miLista, sock_conn, emisor);	
			socket_destinatario = DameSocket(&arg.miLista, destinatario);
			sprintf(res, "9/%s$%s", emisor, texto);
			write(socket_destinatario, res, strlen(res));
		}
		
		else if(codigo==11){ //ESPERAMOS A QUE LOS DOS JUGADORES ESTÉN PREPARADOS	11/
			int partida = DamePartida(sock_conn);
			printf("Valor de la variable partida: %d\n", partida);
			if(milistapartidas.lista[partida].sockets[0]==sock_conn){
				milistapartidas.lista[partida].preparado[0]=1;
				printf("Jugador 0 de la partida %d preparado.\n", partida);
			}
			if(milistapartidas.lista[partida].sockets[1]==sock_conn){
				milistapartidas.lista[partida].preparado[1]=1;
				printf("Jugador 1 de la partida %d preparado.\n", partida);
			}

			if(milistapartidas.lista[partida].preparado[0]==1 && milistapartidas.lista[partida].preparado[1]==1){
				char notificacion[20];
				strcpy(notificacion, "11/");
				for (int j=0; j<2;j++)
				{
					write(milistapartidas.lista[partida].sockets[j],notificacion,strlen(notificacion));
					printf("NotificaciÃ³n de inicio de sesiÃ³n enviada al socket %d\n", milistapartidas.lista[partida].sockets[j]);
				}
			}
		}
		
		else if(codigo==12){ //POSICIÓN DE LAS BARRAS DURANTE LA PARTIDA	12/CAMPO/POSICIÓN (ÚNICAMENTE EN Y, PORQUE X NO CAMBIA)
			int campo;
			char respuesta12[30];
			char posicion[30];
			p=strtok(NULL,"/");
			campo=atoi(p);
			p=strtok(NULL,"/");
			strcpy(posicion, p);
			sprintf(respuesta12, "12/%d/%s", campo, posicion);
			int partida = DamePartida(sock_conn);
			if(campo==0){
				write(milistapartidas.lista[partida].sockets[1],respuesta12, strlen(respuesta12));
			}
			if(campo==1){
				write(milistapartidas.lista[partida].sockets[0],respuesta12, strlen(respuesta12));
			}
		}
		
		else if(codigo==13){  //TERMINA PARTIDA, METEMOS DATOS EN BBDD 	13/GOLES DEL CAMPO 0/GOLES DEL CAMPO 1
			int partida=DamePartida(sock_conn);
			char consulta[200];
			p = strtok(NULL, "/");
			int goles0=atoi(p);
			p = strtok(NULL, "/");
			int goles1=atoi(p);
			char nombre0[30];
			char nombre1[30];
			DameNombre(&arg.miLista, sock_conn, nombre0);
			DameNombre(&arg.miLista, milistapartidas.lista[partida].sockets[1], nombre1);
			err=mysql_query(arg.conn, "SELECT * FROM Partida;");
			resultado = mysql_store_result(arg.conn);
			row = mysql_fetch_row(resultado);
			int salir=0;
			int i;
			for(i=0;salir==0;i++){
				if(row == NULL)
					salir=1;
				row = mysql_fetch_row(resultado);
			}
			i--;
			if(goles0>goles1){
			sprintf(consulta, "INSERT INTO Partida VALUES (%d, '%s', '%s', %d, %d);", i, nombre0, nombre1, goles0, goles1);
			}
			else{
				sprintf(consulta, "INSERT INTO Partida VALUES (%d, '%s', '%s', %d, %d);", i, nombre1, nombre0, goles1, goles0);
			}
			err=mysql_query(arg.conn, consulta);
			if(err==0)
				printf("PARTIDA GUARDADA\n");
			else
				printf("ERROR GUARDANDO PARTIDA\n");
			//EMPEZAMOS POR EL JUGADOR 0
			sprintf(consulta, "SELECT id FROM Jugador WHERE Jugador.nombre = '%s';", nombre0);
			err=mysql_query(arg.conn, consulta);
			resultado = mysql_store_result(arg.conn);
			row = mysql_fetch_row(resultado);
			sprintf(consulta, "INSERT INTO RelacionJP VALUES (%s, %d, %d);", row[0], partida, goles0);
			err=mysql_query(arg.conn, consulta);
			if(err==0){
				printf("INTRODUCIDA RELACIONJP PARA EL JUGADOR 0.\n");
			}
			else{
				printf("ERROR AL INTRODUCIR RELACIONJP PARA EL JUGADOR 0.\n");
			}

			
			//AHORA EL JUGADOR 1
			sprintf(consulta, "SELECT id FROM Jugador WHERE Jugador.nombre = '%s';", nombre1);
			err=mysql_query(arg.conn, consulta);
			resultado = mysql_store_result(arg.conn);
			row = mysql_fetch_row(resultado);
			sprintf(consulta, "INSERT INTO RelacionJP VALUES (%s, %d, %d);", row[0], partida, goles1);
			err=mysql_query(arg.conn, consulta);
			if(err==0){
				printf("INTRODUCIDA RELACIONJP PARA EL JUGADOR 1.\n");
			}
			else{
				printf("ERROR AL INTRODUCIR RELACIONJP PARA EL JUGADOR 1.\n");
			}
			pthread_mutex_lock(&mutex);
			EliminarPartida(partida); //ELIMINAMOS PARTIDA ACTUAL DE LA LISTA DE PARTIDAS (ACTUALES)
			pthread_mutex_unlock(&mutex);
			}
		
		
		else if(codigo==14){ //BORRAR CUENTA	14/
			char nombre[30];
			char respuesta14[100];
			char consulta[200];
			DameNombre(&arg.miLista, sock_conn, nombre);
			sprintf(consulta, "DELETE FROM Jugador WHERE nombre='%s';", nombre);
			err=mysql_query(arg.conn, consulta);
			if(err==0){
				sprintf(respuesta14, "14/OK");
			}
			else{
				sprintf(respuesta14, "14/ERROR");
			}
			write(sock_conn,respuesta14, strlen(respuesta14));
			printf("Respuesta código 14: %s\n", respuesta14);
			terminar=1;
			pthread_mutex_lock(&mutex);
			err = Eliminar(&arg.miLista, nombre);
			pthread_mutex_unlock(&mutex);
		}
		// Se acabo el servicio para este cliente
	}
	close(sock_conn); //CERRAMOS CONEXIÓN
	EliminaSocket(&arg.miLista, sock_conn); //ELIMINAMOS ESE SOCKET
}

int main(int argc, char *argv[]){

	int err;
	// Estructura especial para almacenar resultados de consultas
	arg.miLista.num=0;

	milistapartidas.num=0;

	//Creamos una conexion al servidor MYSQL
	arg.conn = mysql_init(NULL);
	if (arg.conn==NULL) {
		printf("Error al crear la conexion: %u %s\n",
			   mysql_errno(arg.conn), mysql_error(arg.conn));
		exit(1);
	}
	//inicializar la conexion
	arg.conn = mysql_real_connect(arg.conn, "shiva2.upc.es", "root", "mysql", "T6_BBDD",0, NULL, 0);
	if (arg.conn==NULL) {
		printf("Error al inicializar la conexion: %u %s\n",
			   mysql_errno(arg.conn), mysql_error(arg.conn));
		exit(1);
	}

	//---------------------------------------------------------------------------------

	int sock_conn, sock_listen;
	struct sockaddr_in serv_adr;


	// INICIALITZACIONS
	// Obrim el socket
	if ((sock_listen=socket(AF_INET,SOCK_STREAM,0))< 0)
	{
		printf("Error creant socket");
	}
	// Fem el bind al port


	memset(&serv_adr, 0, sizeof(serv_adr));//inicialitza a zero serv_addr
	serv_adr.sin_family = AF_INET;

	// asocia el socket a cualquiera de las IP de la m?quina.
	//htonl formatea el numero que recibe al formato necesario
	serv_adr.sin_addr.s_addr = htonl(INADDR_ANY);
	// establecemos el puerto de escucha




	int puerto = PUERTO;
	serv_adr.sin_port = htons(puerto); //AQUï¿½ EL PUERTO










	if (bind(sock_listen, (struct sockaddr *) &serv_adr, sizeof(serv_adr)) < 0)
		printf("Error en el bind\n");

	if (listen(sock_listen, 3) < 0)
		printf("Error en el Listen\n");
	pthread_t thread;
	int errorlista;
	// Bucle para atender a 5 clientes
	for (;;){
		printf ("Escuchando\n");
		sock_conn = accept(sock_listen, NULL, NULL);
		pthread_mutex_lock(&mutex);
		arg.socket=sock_conn;
		errorlista = PonSocket(&arg.miLista, arg.socket);
		pthread_mutex_unlock(&mutex);
		if(errorlista == 0){
			printf("He recibido conexion\n");
			printf("Socket: %d\n", arg.socket);
			//sock_conn es el socket que usaremos para este cliente

			// Crear thead y decirle lo que tiene que hacer

			pthread_create (&thread, NULL, AtenderCliente, NULL);
		}
		else if(errorlista == -1){
			printf("Lista de conectados llena.");
		}
	}

}
