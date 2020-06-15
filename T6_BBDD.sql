DROP DATABASE IF EXISTS T6_BBDD;
CREATE DATABASE T6_BBDD;

USE T6_BBDD;

CREATE TABLE Jugador (
	id INT PRIMARY KEY,
	nombre VARCHAR (30),
	contrasena VARCHAR (20),
	puntuacion INT
)ENGINE=InnoDB;

CREATE TABLE Partida(
	id INT PRIMARY KEY,
	nombreganador VARCHAR (30),
	nombreperdedor VARCHAR (30),
	golesganador INT,
	golesperdedor INT
)ENGINE=InnoDB;

CREATE TABLE RelacionJP(
	id_J INT,
	id_P INT,
	goles INT,
	FOREIGN KEY (id_J) REFERENCES Jugador(id),
	FOREIGN KEY (id_P) REFERENCES Partida(id)	
)ENGINE=InnoDB;

INSERT INTO Jugador VALUES (0, 'Juan', 'HOLA', 30);
INSERT INTO Jugador VALUES (1, 'Maria', 'CASA', 35);
INSERT INTO Partida VALUES (0, 'Maria', 'Juan', 5, 2);
INSERT INTO Jugador VALUES (2, 'Pedro', 'ABC', 21);
INSERT INTO Partida VALUES (1, 'Pedro', 'Juan', 5, 1);
INSERT INTO Partida VALUES (2, 'Maria', 'Pedro', 5, 4);
INSERT INTO RelacionJP VALUES (0, 0, 5);
INSERT INTO RelacionJP VALUES (1, 0, 3);
INSERT INTO RelacionJP VALUES (0, 1, 4);
INSERT INTO RelacionJP VALUES (1, 1, 5);
INSERT INTO RelacionJP VALUES (0, 2, 2);
INSERT INTO RelacionJP VALUES (2, 2, 5);
